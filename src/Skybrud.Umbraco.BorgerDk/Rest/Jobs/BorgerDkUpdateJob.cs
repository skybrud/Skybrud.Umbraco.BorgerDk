using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml.Linq;
using Skybrud.BorgerDk;
using Skybrud.Umbraco.BorgerDk.Extensions;
using umbraco.NodeFactory;
using www.borger.dk._2009.WSArticleExport.v1.types;

namespace Skybrud.Umbraco.BorgerDk.Rest.Jobs {
    
    internal class BorgerDkUpdateJob : RestExtensionBase {

        #region Properties

        public Dictionary<string, ArticleDescription> Lookup { get; private set; }

        #endregion

        /// <summary>
        /// Gets a list of all articles from Borger.dk and adds their summaries to a dictionary for O(1) lookups
        /// </summary>
        private void GenerateArticleLookup() {

            // Initialize the dictionary
            Lookup = new Dictionary<string, ArticleDescription>();

            // Loop through the articles for each endpoint
            foreach (BorgerDkEndpoint endpoint in BorgerDkEndpoint.Values) {
                using (var service = endpoint.GetClient()) {
                    foreach (var article in service.GetAllArticles()) {
                        Lookup.Add(endpoint.Domain + "_" + article.ArticleID, article);
                    }
                }
            }

        }

        /// <summary>
        /// Recursively add children to the list if they contain at least one valid Borger.dk article
        /// </summary>
        private static void AddChildrenToList(Node node, List<Node> list) {
            foreach (Node child in node.Children) {
                foreach (Property property in child.Properties) {
                    if (!property.Value.StartsWith("<article><id>")) continue;
                    list.Add(child);
                    break;
                }
                AddChildrenToList(child, list);
            }
        }

        /// <summary>
        /// Recursively add children to the list
        /// </summary>
        private static List<Node> GenerateNodeList() {

            // Get the "Content" node
            Node root = new Node(-1);
           
            // Initialize the temporary list
            List<Node> list = new List<Node>();

            // Add the children
            AddChildrenToList(root, list);

            // Return the ordered list
            return list.OrderBy(x => x.Id).ToList();

        }

        /// <summary>
        /// Runs a cycle of the Borger.dk update job. No more than the specified <var>pagesPerCycle</var> amount of
        /// pages are updated for each cycle. A page may have multiple articles.
        /// </summary>
        /// <param name="pagesPerCycle">The maximum amount of pages to update for each cycle.</param>
        public static void RunCycle(int pagesPerCycle) {

            // Check whether the job is currently running
            if (BorgerDkUpdateJobState.IsBusy) {
                WriteJsonError(500, "The job is currently running.");
                return;
            }

            // Check whether the job has already been completed at the present day
            if (BorgerDkUpdateJobState.LastNodeId == 0 && BorgerDkUpdateJobState.LastDay == DateTime.UtcNow.ToString("yyyyMMdd")) {
                WriteJsonError(500, "The job has already finished once today.");
                return;
            }

            // Update the entry for when the job was executed last
            BorgerDkUpdateJobState.LastDay = DateTime.UtcNow.ToString("yyyyMMdd");

            // Total start
            DateTime totalStart = DateTime.UtcNow;

            // Initialize the class
            BorgerDkUpdateJob instance = new BorgerDkUpdateJob();

            // Get list of articles from Borger.dk
            instance.GenerateArticleLookup();

            // Generate the node list
            var list = GenerateNodeList();

            // Keep track of what's going on
            List<object> log = new List<object>();

            // Counter for the amount of processed pages for the current cycle
            int processed = 0;

            // Counter for the amount of processed pages for all cycles
            int processedTotal = 0;

            foreach (Node node in list) {

                // Increment the counter for total processed pages
                processedTotal++;

                // Skip if we already processed the node
                if (node.Id <= BorgerDkUpdateJobState.LastNodeId) {
                    continue;
                }

                // Break if maximum reached
                if (processed++ >= pagesPerCycle) break;

                // Temporary list for each proparty that has a Borger.dk article
                List<object> properties = new List<object>();

                // Loop through each property
                foreach (Property property in node.Properties) {

                    // Skip the property if the value is not a valid Borger.dk article
                    if (!property.Value.StartsWith("<article><id>")) continue;

                    DateTime start = DateTime.UtcNow;
                    
                    try {

                        XElement xArticle = XElement.Parse(property.Value);

                        int articleId = xArticle.GetValue<int>("id");
                        string domain = xArticle.GetValue("domain") ?? "www.borger.dk";
                        string url = xArticle.GetValue("url");
                        int municipalityId = xArticle.GetValue<int>("municipalityid");
                        int reloadInterval = xArticle.GetValue<int>("reloadinterval");
                        string[] selected = (xArticle.GetValue("selected") ?? "").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                        BorgerDkEndpoint endpoint = BorgerDkEndpoint.GetFromDomain(domain);

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
                                properties.Add(new {
                                    property = property.Alias,
                                    code = (int) HttpStatusCode.NotModified,
                                    duration = DateTime.UtcNow.Subtract(start).TotalMilliseconds,
                                    article = new {
                                        id = articleId,
                                        url = url
                                    }
                                });

                            } else {

                                // Get the content node via the content service
                                var content = ContentService.GetById(node.Id);

                                // Set and save the value
                                content.SetValue(property.Alias, value);
                                ContentService.SaveAndPublish(content);
                                
                                // Add relevant information to the log
                                properties.Add(new {
                                    alias = property.Alias,
                                    code = HttpStatusCode.OK,
                                    duration = DateTime.UtcNow.Subtract(start).TotalMilliseconds,
                                    article = new {
                                        id = articleId,
                                        url = url
                                    }
                                });

                            }

                        } catch (Exception ex) {

                            // This point will be reached if downloading the article from Borger.dk fails or saving the
                            // value to Umbraco fails.
                            properties.Add(new {
                                alias = property.Alias,
                                code = HttpStatusCode.InternalServerError,
                                error = ex.Message,
                                duration = DateTime.UtcNow.Subtract(start).TotalMilliseconds,
                                article = new {
                                    id = articleId,
                                    url = url
                                }
                            });

                        }

                    } catch (Exception ex) {

                        // If this point is reached, the article could not be parsed. Will most likely not happen.
                        properties.Add(new {
                            alias = property.Alias,
                            code = HttpStatusCode.InternalServerError,
                            error = ex.Message,
                            duration = DateTime.UtcNow.Subtract(start).TotalMilliseconds,
                            article = (object) null
                        });

                    }

                }

                // Generate the log entry
                log.Add(new {
                    node = node.Id,
                    properties
                });

                // Set the last processed node
                BorgerDkUpdateJobState.LastNodeId = node.Id;

            }

            // Should we reset?
            if (list.Count > 0 && BorgerDkUpdateJobState.LastNodeId == list.Last().Id) {
                BorgerDkUpdateJobState.LastNodeId = 0;
            }

            // The job is done for now
            BorgerDkUpdateJobState.IsBusy = false;

            // Write the JSON response
            WriteJsonSuccess(new {
                lastNode = BorgerDkUpdateJobState.LastNodeId,
                processedPages = processedTotal,
                totalPages = list.Count,
                duration = DateTime.UtcNow.Subtract(totalStart).TotalMilliseconds,
                log
            });

        }

    }

}
