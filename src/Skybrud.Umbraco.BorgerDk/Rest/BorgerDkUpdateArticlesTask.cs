using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Skybrud.BorgerDk;
using Skybrud.Umbraco.BorgerDk.Extensions;
using Umbraco.Core;
using Umbraco.Core.Services;
using umbraco.NodeFactory;
using Property = umbraco.NodeFactory.Property;

namespace Skybrud.Umbraco.BorgerDk.Rest {

    public class BorgerDkUpdateArticlesTask {

        public static IContentService ContentService {
            get { return ApplicationContext.Current.Services.ContentService; }
        }

        public static DateTime BusyTimestamp;

        public bool ForceUpdate { get; private set; }

        protected Dictionary<string, DateTime> Lookup = new Dictionary<string, DateTime>();

        private List<object> _log = new List<object>();


        public BorgerDkUpdateArticlesTask() {
            ForceUpdate = false;
        }
        
        public BorgerDkUpdateArticlesTask(bool forceUpdate) {
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
                    using (var service = endpoint.GetClient()) {
                        foreach (var article in service.GetAllArticles()) {
                            Lookup.Add(endpoint.Domain + "_" + article.ArticleID, article.LastUpdated);
                        }
                    }
                }

                #endregion

                #region Loop through all documents

                UpdateChildren(new Node(-1));

                #endregion

                double duration = DateTime.UtcNow.Subtract(start).TotalMilliseconds;

                // Reset the timestamp
                BusyTimestamp = default(DateTime);

                return new {
                    meta = new { code = 200, duration },
                    data = _log
                };

            } catch (Exception ex) {

                // Reset the timestamp
                BusyTimestamp = default(DateTime);

                double duration = DateTime.UtcNow.Subtract(start).TotalMilliseconds;

                return new {
                    meta = new { code = 500, error = ex.Message, duration },
                    data = (object) null
                };

            }

        }

        private void UpdateChildren(Node node) {
            foreach (Node child in node.Children) {
                UpdateNode(child);
            }
        }

        private void UpdateNode(Node node) {

            foreach (Property property in node.Properties) {

                if (property.Value.StartsWith("<article><id>")) {

                    try {
                    
                        XElement xArticle = XElement.Parse(property.Value);

                        int articleId = xArticle.GetValue<int>("id");
                        string domain = xArticle.GetValue("domain") ?? "www.borger.dk";
                        string url = xArticle.GetValue("url");
                        DateTime lastReloaded = xArticle.GetValue<DateTime>("lastreloaded");
                        int municipalityId = xArticle.GetValue<int>("municipalityid");
                        int reloadInterval = xArticle.GetValue<int>("reloadinterval");
                        string[] selected = (xArticle.GetValue("selected") ?? "").Split(new [] {","}, StringSplitOptions.RemoveEmptyEntries);

                        BorgerDkEndpoint endpoint = BorgerDkEndpoint.GetFromDomain(domain);

                        DateTime webserviceDate;

                        if (Lookup.TryGetValue(endpoint.Domain + "_" + articleId, out webserviceDate)) {

                            if (lastReloaded < webserviceDate || ForceUpdate) {

                                DateTime start = DateTime.UtcNow;

                                try {

                                    BorgerDkService service = new BorgerDkService(endpoint, BorgerDkMunicipality.FirstOrDefault(x => x.Code == municipalityId));

                                    BorgerDkArticle article = service.GetArticleFromId(articleId);

                                    // Get the content node via the content service
                                    var content = ContentService.GetById(node.Id);

                                    // If the node for some obscure reason doesn't exist, we just continue
                                    if (content == null) continue;

                                    // Set and save the value
                                    content.SetValue(property.Alias, BorgerDkHelper.ToXElement(article, selected, municipalityId, reloadInterval).ToString(SaveOptions.DisableFormatting));
                                    ContentService.SaveAndPublish(content);

                                    TimeSpan duration = DateTime.UtcNow.Subtract(start);

                                    _log.Add(new {
                                        node = node.Id,
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
                                        node = node.Id,
                                        property = property.Alias,
                                        article = new {
                                            id = articleId,
                                            url = url,
                                            lastReloaded = lastReloaded.ToString("yyyy-MM-dd HH:mm:ss"),
                                            webServiceDate = (object)null,
                                            code = 500,
                                            error = ex.Message
                                        }
                                    });

                                }

                            } else {

                                _log.Add(new {
                                    node = node.Id,
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
                                node = node.Id,
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
                        HandleException(node, property, ex);
                    }

                }

            }

            UpdateChildren(node);

        }

        private void HandleException(Node node, Property property, Exception ex) {
            // TODO: Add some error handling or logging
        }

    }

}
