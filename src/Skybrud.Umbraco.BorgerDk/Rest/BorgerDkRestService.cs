using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using Skybrud.BorgerDk;
using System.Linq;
using Skybrud.BorgerDk.Elements;
using Skybrud.Umbraco.BorgerDk.Extensions;
using Skybrud.Umbraco.BorgerDk.Rest.Jobs;
using Umbraco.Web.BaseRest;
using umbraco.NodeFactory;
using www.borger.dk._2009.WSArticleExport.v1.types;
using Property = umbraco.NodeFactory.Property;

namespace Skybrud.Umbraco.BorgerDk.Rest {

    [RestExtension("BorgerDk")]
    public class BorgerDkRestService : RestExtensionBase {

        [RestExtensionMethod(AllowAll = true)]
        public static void Test() {

            File.AppendAllText(Server.MapPath("~/App_Data/BorgerDkTest.txt"), ServerVariables.RemoteAddress + " " + HasValidLogin + "\r\n");

            HttpContext.Current.Response.ContentType = "application/json";
            HttpContext.Current.Response.Write(new JavaScriptSerializer().Serialize(new {
                bacon = true
            }));
            HttpContext.Current.Response.End();

        }

        [RestExtensionMethod(AllowAll = true)]
        public static void UpdateAllArticles() {

            // Check for permissions
            if (HasValidLogin) {
                WriteJsonError(HttpStatusCode.Forbidden, "Access denied");
                return;
            }

            // Read from input
            bool forceUpdate = Request.QueryString["forceUpdate"] == "1";

            // Run the update job
            var result = new BorgerDkUpdateArticlesTask(forceUpdate).Run();

            HttpContext.Current.Response.ContentType = "application/json";
            HttpContext.Current.Response.Write(new JavaScriptSerializer().Serialize(result));
            HttpContext.Current.Response.End();

        }

        [RestExtensionMethod(AllowAll = true)]
        public static void UpdateArticlesOnPage(int pageId) {

            // Check for permissions
            if (HasValidLogin) {
                WriteJsonError(HttpStatusCode.Forbidden, "Access denied");
                return;
            }

            // Validate the page ID
            if (pageId <= 0) {
                WriteJsonError(HttpStatusCode.BadRequest, "Invalid page ID specified.");
                return;
            }

            // Get the node
            Node node = new Node(pageId);

            // Check whether the node was found
            if (node.Id != pageId) {
                WriteJsonError(HttpStatusCode.NotFound, "Page not found.");
                return;
            }

            // Declare the log
            List<object> log = new List<object>();

            foreach (Property property in node.Properties) {

                if (!property.Value.StartsWith("<article><id>")) continue;

                try {

                    XElement xArticle = XElement.Parse(property.Value);

                    int articleId = xArticle.GetValue<int>("id");
                    string domain = xArticle.GetValue("domain") ?? "www.borger.dk";
                    string url = xArticle.GetValue("url");
                    int municipalityId = xArticle.GetValue<int>("municipalityid");
                    int reloadInterval = xArticle.GetValue<int>("reloadinterval");
                    string[] selected = (xArticle.GetValue("selected") ?? "").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    BorgerDkEndpoint endpoint = BorgerDkEndpoint.GetFromDomain(domain);

                    DateTime start = DateTime.UtcNow;

                    try {

                        BorgerDkService service = new BorgerDkService(endpoint, BorgerDkMunicipality.FirstOrDefault(x => x.Code == municipalityId));

                        BorgerDkArticle article = service.GetArticleFromId(articleId);

                        // Generate the new XML value
                        string value = BorgerDkHelper.ToXElement(article, selected, municipalityId, reloadInterval).ToString(SaveOptions.DisableFormatting);

                        string compareStringOld = Regex.Replace(property.Value, "<lastreloaded>(.+?)<\\/lastreloaded>", "").Replace("\r", "");
                        string compareStringNew = Regex.Replace(value, "<lastreloaded>(.+?)<\\/lastreloaded>", "").Replace("\r", "");

                        //throw new Exception("--" + compareStringOld.Split('\n').Length + "--" + compareStringNew.Split('\n').Length + "--");
                        
                        if (compareStringOld == compareStringNew) {
                            
                            // The XML was not modified
                            log.Add(new {
                                node = node.Id,
                                property = property.Alias,
                                article = new {
                                    id = articleId,
                                    url = url,
                                    code = (int) HttpStatusCode.NotModified,
                                    duration = DateTime.UtcNow.Subtract(start).TotalMilliseconds
                                }
                            });

                        } else {
                            
                            // Get the content node via the content service
                            var content = ContentService.GetById(node.Id);

                            // Set and save the value
                            content.SetValue(property.Alias, value);
                            ContentService.SaveAndPublish(content);

                            log.Add(new {
                                node = node.Id,
                                property = property.Alias,
                                article = new {
                                    id = articleId,
                                    blah = content.Id,
                                    url = url,
                                    code = (int) HttpStatusCode.OK,
                                    duration = DateTime.UtcNow.Subtract(start).TotalMilliseconds
                                }
                            });
                        
                        }

                    } catch (Exception ex) {

                        log.Add(new {
                            node = node.Id,
                            property = property.Alias,
                            article = new {
                                id = articleId,
                                url = url,
                                code = 500,
                                error = ex.Message
                            }
                        });

                    }

                } catch (Exception ex) {
                
                    // Unable to parse the XML
                
                }

            }

            WriteJsonSuccess(log);

        }


        [RestExtensionMethod(AllowAll = true)]
        public static void UpdateAllArticlesInSteps(int pagesPerCycles) {

            // Check for permissions
            if (HasValidLogin) {
                WriteJsonError(HttpStatusCode.Forbidden, "Access denied");
                return;
            }

            // Run the job
            BorgerDkUpdateJob.RunCycle(pagesPerCycles);
        
        }


        [RestExtensionMethod(AllowAll = true)]
        public static void GetArticleList() {

            // Check for permissions
            if (HasValidLogin) {
                WriteJsonError(HttpStatusCode.Forbidden, "Access denied");
                return;
            }

            Dictionary<string, ArticleDescription> lookup = new Dictionary<string, ArticleDescription>();

            #region Get list of articles from Borger.dk

            foreach (BorgerDkEndpoint endpoint in BorgerDkEndpoint.Values) {
                using (ArticleExportClient service = endpoint.GetClient()) {
                    foreach (var article in service.GetAllArticles()) {
                        lookup.Add(endpoint.Domain + "_" + article.ArticleID, article);
                    }
                }
            }

            #endregion

            List<object> pages = new List<object>();

            Node root = new Node(-1);

            foreach (Node node in root.Children) {
                GetArticleListWorker(node, lookup, pages);
            }

            HttpContext.Current.Response.ContentType = "application/json";
            HttpContext.Current.Response.Write(new JavaScriptSerializer().Serialize(new {
                meta = new {
                    code = 200
                },
                data = pages
            }));
            
            HttpContext.Current.Response.End();

        }

        private static void GetArticleListWorker(Node node, Dictionary<string, ArticleDescription> lookup, List<object> pages) {

            List<object> properties = new List<object>();

            foreach (Property property in node.Properties) {

                if (!property.Value.StartsWith("<article><id>")) continue;

                try {
                    
                    XElement xArticle = XElement.Parse(property.Value);

                    int articleId = xArticle.GetValue<int>("id");
                    string domain = xArticle.GetValue("domain") ?? "www.borger.dk";
                    string url = xArticle.GetValue("url");
                    DateTime lastReloaded = xArticle.GetValue<DateTime>("lastreloaded");
                    int municipalityId = xArticle.GetValue<int>("municipalityid");
                    int reloadInterval = xArticle.GetValue<int>("reloadinterval");
                    string[] selected = (xArticle.GetValue("selected") ?? "").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    if (lookup.ContainsKey(domain + "_" + articleId)) {

                        ArticleDescription article = lookup[domain + "_" + articleId];

                        properties.Add(new {
                            Name = property.Alias,
                            Info = new {
                                Id = articleId,
                                Domain = domain,
                                Url = url,
                                LastReloaded = xArticle.GetValue("lastreloaded"),
                                MunicipalityId = municipalityId,
                                ReloadInterval = reloadInterval,
                                Selected = selected,
                                IsUpdated = article.LastUpdated < lastReloaded,
                                Article = new {
                                    Id = article.ArticleID,
                                    Url = article.ArticleUrl,
                                    Title = article.ArticleTitle,
                                    PublishingDate = article.PublishingDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                    LastUpdated = article.LastUpdated.ToString("yyyy-MM-dd HH:mm:ss")
                                }
                            }
                        });

                    } else {
                        
                        properties.Add(new {
                            Name = property.Alias,
                            Error = "Article with ID <strong>" + articleId + "</strong> for domain <strong>" + domain + "</strong> not found."
                        });

                    }

                } catch (Exception ex) {

                    properties.Add(new {
                        Name = property.Alias,
                        Error = "Unable to parse article: " + ex.Message
                    });

                }

            }

            if (properties.Count > 0) {
                pages.Add(new {
                    node.Id,
                    node.Name,
                    Properties = properties
                });
            }

            foreach (Node child in node.Children) {
                GetArticleListWorker(child, lookup, pages);
            }
            
        }

        [RestExtensionMethod(AllowAll = true)]
        public static void GetArticle() {
            GetUrlResponse();
        }

        [RestExtensionMethod(AllowAll = true)]
        public static void GetUrlResponse() {

            string url = (Request.QueryString["url"] ?? "").Split('?')[0];
            int municipalityId;

            bool useCache = !(Request.QueryString["cache"] == "0" || Request.QueryString["cache"] == "false");

            #region Validation

            if (String.IsNullOrEmpty(url)) {
                WriteJsonError(400, "Ingen adresse til borger.dk angivet");
                return;
            }

            if (!BorgerDkService.IsValidUrl(url)) {
                WriteJsonError(400, "Ugyldig adresse til borger.dk angivet");
                return;
            }

            if (Request.QueryString["municipalityId"] == null) {
                WriteJsonError(400, "Intet kommune ID angivet");
            }

            if (!Int32.TryParse(Request.QueryString["municipalityId"], out municipalityId)) {
                WriteJsonError(400, "Ugyldigt kommune ID angivet");
            }

            #endregion

            BorgerDkArticle article;
            
            try {
                article = BorgerDkHelper.GetArticle(url, municipalityId, useCache);
            } catch (System.ServiceModel.FaultException ex) {
                if (ex.Message.StartsWith("No article found with url")) {
                    WriteJsonError(404, "Den angivne artikel blev ikke fundet på Borger.dk");
                    return;
                }
                WriteJsonError(500, "Der skete en fejl i forbindelse med Borger.dk");
                return;
            } catch (Exception) {
                WriteJsonError(500, "Der skete en fejl i forbindelse med Borger.dk");
                return;
            }

            if (HttpContext.Current.Request.QueryString["showXml"] == "true") {
                HttpContext.Current.Response.ContentType = "text/xml";
                HttpContext.Current.Response.Write(article.ToXElement(0,0));
                HttpContext.Current.Response.End();
            }

            List<object> elements = new List<object>();

            elements.Add(new { type = "title", id = "title", text = "Overskrift", content = article.Title });
            elements.Add(new { type = "header", id = "header", text = "Manchet", content = article.Header });

            foreach (BorgerDkElement element in article.Elements) {
                if (element is BorgerDkTextElement) {
                    BorgerDkTextElement text = (BorgerDkTextElement)element;
                    elements.Add(new { type = text.Type, id = text.Type, text = text.Title, content = text.Content.Trim() });
                } else if (element is BorgerDkBlockElement) {
                    BorgerDkBlockElement block = (BorgerDkBlockElement)element;
                    elements.Add(new {
                        type = block.Type,
                        id = block.Type,
                        text = "Hovedindhold",
                        content = block.MicroArticles.Select(x => new {
                            type = "microArticle",
                            id = x.Id,
                            text = x.Title,
                            content = x.Content.Trim()
                        })
                    });
                }
            }

            WriteJsonSuccess(new {
                id = article.Id,
                domain = article.Domain,
                url = article.Url,
                published = article.Published.ToString("yyyy-MM-dd HH:mm:ss"),
                modified = article.Modified.ToString("yyyy-MM-dd HH:mm:ss"),
                title = article.Title,
                header = article.Header,
                elements = elements
            });

        }

    }

}
