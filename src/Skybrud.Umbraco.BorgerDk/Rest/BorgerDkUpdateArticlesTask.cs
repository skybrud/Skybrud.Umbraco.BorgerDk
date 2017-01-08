using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Skybrud.BorgerDk;
using Skybrud.Umbraco.BorgerDk.DataTypes.MicroArticles;
using Skybrud.Umbraco.BorgerDk.Extensions;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using www.borger.dk._2009.WSArticleExport.v1.types;

namespace Skybrud.Umbraco.BorgerDk.Rest {

    internal class UpdateQueueItem {

        public IPublishedContent Content { get; set; }

        public string PropertyAlias { get; set; }

        public BorgerDkMicroArticlesModel Model { get; set; }

    }

    public class BorgerDkUpdateArticlesTask {

        public string Id { get; private set; }

        public static IContentService ContentService {
            get { return ApplicationContext.Current.Services.ContentService; }
        }

        public static DateTime BusyTimestamp;

        public bool ForceUpdate { get; private set; }

        protected Dictionary<string, DateTime> Lookup = new Dictionary<string, DateTime>();

        private List<object> _log = new List<object>();

        private Dictionary<string, List<UpdateQueueItem>> _updateQueue = new Dictionary<string, List<UpdateQueueItem>>(); 

        public BorgerDkUpdateArticlesTask() : this(false) { }
        
        public BorgerDkUpdateArticlesTask(bool forceUpdate) {
            Id = Guid.NewGuid().ToString();
            ForceUpdate = forceUpdate;
        }

        public object Run() {

            if (BusyTimestamp.AddHours(1) > DateTime.UtcNow) {
                return new {
                    meta = new { code = 503, error = "The job appears to be running at the moment." },
                    data = (object) null
                };
            }

            DateTime start = DateTime.UtcNow;

            // Update the timestamp so the job is considered running
            BusyTimestamp = DateTime.UtcNow;

            try {

                #region Get list of articles from Borger.dk

                foreach (BorgerDkEndpoint endpoint in BorgerDkEndpoint.Values) {
                    using (ArticleExportClient service = endpoint.GetClient()) {
                        foreach (ArticleDescription article in service.GetAllArticles()) {
                            Lookup.Add(endpoint.Domain + "_" + article.ArticleID, article.LastUpdated);
                        }
                    }
                }

                #endregion

                #region Loop through all documents

                foreach (IPublishedContent content in UmbracoContext.Current.ContentCache.GetAtRoot()) {
                    UpdateContent(content);
                }

                #endregion

                #region Update the articles in the update queue

                foreach (List<UpdateQueueItem> list in _updateQueue.Values) {

                    // The list really shouldn't be empty, but just to be sure
                    if (list.Count == 0) continue;

                    BorgerDkArticle article;

                    try {
                        
                        // Get a reference to the endpoint for the domain
                        BorgerDkEndpoint endpoint = BorgerDkEndpoint.GetFromDomain(list[0].Model.Domain);

                        // Initialize a new service from the endpoint and municipality
                        BorgerDkService service = new BorgerDkService(endpoint, list[0].Model.Municipality);

                        // Get the article from the Borger.dk web service
                        article = service.GetArticleFromId(list[0].Model.Id);

                        AppendToLog("Fetched article **" + article.Title + "** (" + article.Id + ") for **" + article.Municipality.NameLong + "** (" + article.Municipality.Code + ")");

                        // Save the article to the disk
                        BorgerDkHelpers.SaveToCacheFile(article);

                    } catch (Exception ex) {

                        AppendToLog("Unabtle to fetch article with ID " + list[0].Model.Id + " for **" + list[0].Model.Municipality.NameLong + "** (" + list[0].Model.Municipality.Code + "): " + ex.Message);

                        continue;

                    }

                    // Update each property referencing the article
                    foreach (UpdateQueueItem item in list) {
                            
                        try {

                            // Update the model
                            item.Model.UpdateFromArticle(article, item.Model.Municipality, item.Model.Selected);

                            // Get the content node via the content service
                            IContent content = ContentService.GetById(item.Content.Id);

                            // If the content node for some obscure reason doesn't exist, we just continue the loop
                            if (content == null) continue;

                            // Set and save the value
                            content.SetValue(item.PropertyAlias, item.Model.ToJson());
                            ContentService.SaveAndPublish(content);

                            AppendToLog("Updated property **{0}** for the page **{1}** ({2})", item.PropertyAlias, item.Content.Name, item.Content.Id);

                        } catch (Exception ex) {
                            


                        }

                    }

                }

                #endregion

                double duration = DateTime.UtcNow.Subtract(start).TotalMilliseconds;

                // Reset the timestamp
                BusyTimestamp = default(DateTime);

                return new {
                    meta = new { code = 200, duration },
                    data = new {
                        id = Id,
                        log = _log
                    }
                };

            } catch (Exception ex) {

                // Reset the timestamp
                BusyTimestamp = default(DateTime);

                double duration = DateTime.UtcNow.Subtract(start).TotalMilliseconds;

                return new {
                    meta = new { code = 500, error = ex.Message, duration },
                    data = new {
                        id = Id,
                        log = _log
                    }
                };

            }

        }

        private void AppendToLog(string format, params object[] args) {

            string str = String.Format(format, args);

            LogHelper.Info<BorgerDkUpdateArticlesTask>("[" + Id + "] " + str);

            _log.Add(str);
        
        }

        private void UpdateContent(IPublishedContent content) {
            
            foreach (IPublishedContentProperty property in content.Properties) {

                object value = content.GetPropertyValue(property.Alias);
                
                BorgerDkMicroArticlesModel model = value as BorgerDkMicroArticlesModel;
                string str = value == null ? null : value.ToString();

                if (model != null) {
                    UpdateArticleFromModel(content, property, (BorgerDkMicroArticlesModel) value);
                } else if (str != null) {
                    if (str.StartsWith("<article><id>")) {
                        UpdateArticleFromXml(content, property, value + "");
                    }
                }

            }

            foreach (IPublishedContent child in content.Children) {
                UpdateContent(child);
            }

        }

        private void UpdateArticleFromModel(IPublishedContent publishedContent, IPublishedContentProperty property, BorgerDkMicroArticlesModel value) {

            // Make sure that the model/article is valid
            if (value.Id == 0 || !BorgerDkService.IsValidUrl(value.Url)) return;

            // Declare a key that is unique for the article (including domain and municipality)
            string key = value.Domain + "_" + value.Id + "_" + value.Municipality.Code;

            try {
                
                // Get the endpoint for the domain of the article
                BorgerDkEndpoint endpoint = BorgerDkEndpoint.GetFromDomain(value.Domain);

                DateTime webserviceDate;
                if (Lookup.TryGetValue(endpoint.Domain + "_" + value.Id, out webserviceDate)) {

                    if (value.LastReloaded < webserviceDate || ForceUpdate) {

                        List<UpdateQueueItem> items;

                        if (!_updateQueue.TryGetValue(key, out items)) {
                            items = new List<UpdateQueueItem>();
                            _updateQueue.Add(key, items);
                        }

                        _log.Add("Added property **" + property.Alias + "** for page **" + publishedContent.Name + "** (" + publishedContent.Id + ") to the update queue with article **" + value.Title + "** (" + value.Id + ")");

                        items.Add(new UpdateQueueItem {
                            Content = publishedContent,
                            PropertyAlias = property.Alias,
                            Model = value
                        });

                    }

                } else {

                    _log.Add(new {
                        node = publishedContent.Id,
                        property = property.Alias,
                        article = new {
                            id = value.Id,
                            url = value.Url,
                            lastReloaded = value.LastReloaded.ToString("yyyy-MM-dd HH:mm:ss"),
                            webServiceDate = (object) null,
                            code = 404,
                            error = "Artiklen er ikke længere tilgængelig gennem den benyttede web service."
                        }
                    });

                }

            } catch (Exception ex) {
                HandleException(publishedContent, property, ex);
            }

        }

        private void UpdateArticleFromXml(IPublishedContent publishedContent, IPublishedContentProperty property, string xml) {

            try {

                XElement xArticle = XElement.Parse(xml);

                int articleId = xArticle.GetValue<int>("id");
                string domain = xArticle.GetValue("domain") ?? "www.borger.dk";
                string url = xArticle.GetValue("url");
                DateTime lastReloaded = xArticle.GetValue<DateTime>("lastreloaded");
                int municipalityId = xArticle.GetValue<int>("municipalityid");
                int reloadInterval = xArticle.GetValue<int>("reloadinterval");
                string[] selected = (xArticle.GetValue("selected") ?? "").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                BorgerDkEndpoint endpoint = BorgerDkEndpoint.GetFromDomain(domain);

                DateTime webserviceDate;

                if (Lookup.TryGetValue(endpoint.Domain + "_" + articleId, out webserviceDate)) {

                    if (lastReloaded < webserviceDate || ForceUpdate) {

                        DateTime start = DateTime.UtcNow;

                        try {

                            BorgerDkService service = new BorgerDkService(endpoint, BorgerDkMunicipality.GetFromCode(municipalityId));

                            BorgerDkArticle article = service.GetArticleFromId(articleId);

                            // Save the article to the disk
                            BorgerDkHelpers.SaveToCacheFile(article);

                            // Get the content node via the content service
                            IContent content = ContentService.GetById(publishedContent.Id);

                            // If the node for some obscure reason doesn't exist, we just return
                            if (content == null) return;

                            // Set and save the value
                            content.SetValue(property.Alias, BorgerDkHelpers.ToXElement(article, selected, municipalityId, reloadInterval).ToString(SaveOptions.DisableFormatting));
                            ContentService.SaveAndPublish(content);

                            TimeSpan duration = DateTime.UtcNow.Subtract(start);

                            _log.Add(new {
                                node = publishedContent.Id,
                                property = property.Alias,
                                article = new {
                                    id = articleId,
                                    url = url,
                                    lastReloaded = lastReloaded.ToString("yyyy-MM-dd HH:mm:ss"),
                                    webServiceDate = webserviceDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                    code = 200,
                                    duration = duration.TotalMilliseconds
                                }
                            });

                        } catch (Exception ex) {

                            _log.Add(new {
                                node = publishedContent.Id,
                                property = property.Alias,
                                article = new {
                                    id = articleId,
                                    url = url,
                                    lastReloaded = lastReloaded.ToString("yyyy-MM-dd HH:mm:ss"),
                                    webServiceDate = (object) null,
                                    code = 500,
                                    error = ex.Message
                                }
                            });

                        }

                    } else {

                        _log.Add(new {
                            node = publishedContent.Id,
                            property = property.Alias,
                            article = new {
                                id = articleId,
                                url = url,
                                lastReloaded = lastReloaded.ToString("yyyy-MM-dd HH:mm:ss"),
                                webServiceDate = webserviceDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                code = 304
                            }
                        });

                    }

                } else {

                    _log.Add(new {
                        node = publishedContent.Id,
                        property = property.Alias,
                        article = new {
                            id = articleId,
                            url = url,
                            lastReloaded = lastReloaded.ToString("yyyy-MM-dd HH:mm:ss"),
                            webServiceDate = (object) null,
                            code = 404,
                            error = "Artiklen er ikke længere tilgængelig gennem den benyttede web service."
                        }
                    });

                }

            } catch (Exception ex) {
                HandleException(publishedContent, property, ex);
            }

        }

        private void HandleException(IPublishedContent publishedContent, IPublishedContentProperty property, Exception ex) {

            _log.Add(new {
                message = "Exception: " + ex.Message,
                node = publishedContent.Id,
                property = property.Alias
            });

        }

    }

}
