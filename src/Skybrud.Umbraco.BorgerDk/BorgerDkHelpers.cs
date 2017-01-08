using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.BorgerDk;
using Skybrud.BorgerDk.Elements;
using Skybrud.Umbraco.BorgerDk.Interfaces;
using Umbraco.Core.Logging;

namespace Skybrud.Umbraco.BorgerDk {

    public static class BorgerDkHelpers {

        public static Dictionary<string, string> GetContentTypes() {
            /*
                TODO: This method should propably be automized somehow,
                      so we don't have to update the DLL if Borger.dk
                      removes or adds any content types.
            */
            return new Dictionary<string, string> {
                {"title", "Titel"},
                {"header", "Manchet"},
                {"kernetekst", "Kernetekst"},
                {"byline", "Skrevet af"},
                {"selvbetjeningslinks", "Selvbetjeningslinks"},
                {"anbefaler", "Anbefaler"},
                {"huskeliste", "Huskeliste"},
                {"lovgivning", "Lovgivning"},
                {"faktaboks", "Faktaboks"},
                {"regler", "Regler"}
            };
        }

        private static string[] TrimSelectedTypes(string[] selectedTypes) {
            return selectedTypes.Contains("kernetekst") ? selectedTypes.Where(x => !x.StartsWith("microArticle")).ToArray() : selectedTypes;
        }

        public static XElement ToXElement(BorgerDkArticle article, string[] selectedTypes, int municipalityId, int reloadInterval) {

            // Clean up the selected types a bit
            selectedTypes = TrimSelectedTypes(selectedTypes);

            // Initialize the "xml" element
            XElement xElements = new XElement("xml");

            // Add the "title" if selected
            if (selectedTypes.Contains("title")) {
                xElements.Add(new XElement(
                    "title",
                    new XElement("title", "Overskrift"),
                    new XElement("html", new XCData(article.Title.Trim())),
                    new XElement("xml", new XCData(article.Title.Trim()))
                ));
            }

            // Add the "header" if selected
            if (selectedTypes.Contains("header")) {
                xElements.Add(new XElement(
                    "header",
                    new XElement("title", "Manchet"),
                    new XElement("html", new XCData(article.Header.Trim())),
                    new XElement("xml", new XCData(article.Header.Trim()))
                ));
            }

            // Loop through the elements (content types)
            foreach (BorgerDkElement element in article.Elements) {

                if (element is BorgerDkTextElement) {

                    BorgerDkTextElement text = (BorgerDkTextElement)element;

                    if (selectedTypes.Contains(element.Type)) {
                        xElements.Add(text.ToXElement());
                    }

                } else if (element is BorgerDkBlockElement) {

                    // Cast to the block element class to expose it's properties
                    BorgerDkBlockElement block = (BorgerDkBlockElement)element;

                    // Get micro articles based on the selected types
                    var microArticles = selectedTypes.Contains("kernetekst") ? block.MicroArticles : block.MicroArticles.Where(x => selectedTypes.Contains("microArticle-" + x.Id));

                    // Initialize the "kernetekst" element
                    XElement xKernetekst = new XElement("kernetekst");

                    // Add the "kernetekst" element if necesary (as well as any micro articles)
                    if (microArticles.Any()) {
                        foreach (var micro in microArticles) {
                            xKernetekst.Add(micro.ToXElement());
                        }
                        xElements.Add(xKernetekst);
                    } else if (selectedTypes.Contains("kernetekst")) {
                        xElements.Add(xKernetekst);
                    }

                }

            }

            // Generate and return the XML representation of the article
            return new XElement(
                "article",
                new XElement("id", article.Id),
                new XElement("domain", article.Domain),
                new XElement("url", article.Url),
                new XElement("municipalityid", municipalityId),
                new XElement("reloadinterval", reloadInterval),
                new XElement("lastreloaded", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                new XElement("publishingdate", article.Published.ToString("yyyy-MM-dd HH:mm:ss")),
                new XElement("lastupdated", article.Modified.ToString("yyyy-MM-dd HH:mm:ss")),
                new XElement("title", article.Title),
                new XElement("header", article.Header),
                new XElement("selected", String.Join(",", selectedTypes)),
                new XElement("html", new XCData(article.Content)),
                xElements
            );

        }

        public static XElement ToXElement(BorgerDkArticle article, int municipalityId) {
            // Initialize the "xml" element
            XElement xElements = new XElement("xml");

            // Add the "title"
            xElements.Add(new XElement(
                "title",
                new XElement("title", "Overskrift"),
                new XElement("html", new XCData(article.Title.Trim())),
                new XElement("xml", new XCData(article.Title.Trim()))
            ));

            // Add the "header" if selected
            xElements.Add(new XElement(
                "header",
                new XElement("title", "Manchet"),
                new XElement("html", new XCData(article.Header.Trim())),
                new XElement("xml", new XCData(article.Header.Trim()))
            ));

            // Loop through the elements (content types)
            foreach (BorgerDkElement element in article.Elements) {

                if (element is BorgerDkTextElement) {

                    BorgerDkTextElement text = (BorgerDkTextElement)element;

                    xElements.Add(text.ToXElement());

                } else if (element is BorgerDkBlockElement) {

                    // Cast to the block element class to expose it's properties
                    BorgerDkBlockElement block = (BorgerDkBlockElement)element;

                    // Get micro articles based on the selected types
                    var microArticles = block.MicroArticles;

                    // Initialize the "kernetekst" element
                    XElement xKernetekst = new XElement("kernetekst");

                    // Add the "kernetekst" element if necesary (as well as any micro articles)
                    if (microArticles.Any()) {
                        foreach (var micro in microArticles) {
                            xKernetekst.Add(micro.ToXElement());
                        }
                        xElements.Add(xKernetekst);
                    } else {
                        xElements.Add(xKernetekst);
                    }

                }

            }

            // Generate and return the XML representation of the article
            return new XElement(
                "article",
                new XElement("id", article.Id),
                new XElement("domain", article.Domain),
                new XElement("url", article.Url),
                new XElement("municipalityid", municipalityId),
                new XElement("reloadinterval", 0),
                new XElement("lastreloaded", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                new XElement("publishingdate", article.Published.ToString("yyyy-MM-dd HH:mm:ss")),
                new XElement("lastupdated", article.Modified.ToString("yyyy-MM-dd HH:mm:ss")),
                new XElement("title", article.Title),
                new XElement("header", article.Header),
                new XElement("html", new XCData(article.Content)),
                xElements
            );

        }

        private static JObject ToJsonObject(BorgerDkTextElement element) {
            return new JObject {
                {"type", element.Type},
                {"title", element.Title},
                {"html", element.Content}
            };
        }

        private static JObject ToJsonObject(BorgerDkBlockElement element) {

            JArray children = new JArray();
            foreach (BorgerDkMicroArticle child in element.MicroArticles) {
                children.Add(ToJsonObject(child));
            }

            return new JObject {
                {"type", element.Type},
                {"microArticles", children},
            };

        }

        private static JObject ToJsonObject(BorgerDkMicroArticle microArticle) {
            return new JObject {
                {"id", microArticle.Id},
                {"title", microArticle.Title},
                {"html", microArticle.Content}
            };
        }

        public static JObject ToJsonObject(BorgerDkArticle article) {

            JArray elements = new JArray();

            foreach (BorgerDkElement element in article.Elements) {
                if (element is BorgerDkTextElement) {
                    elements.Add(ToJsonObject((BorgerDkTextElement)element));
                } else if (element is BorgerDkBlockElement) {
                    elements.Add(ToJsonObject((BorgerDkBlockElement)element));
                }
            }

            return new JObject {
                {"id", article.Id},
                {"domain", article.Domain},
                {"url", article.Url},
                {"municipality", article.Municipality.Code},
                {"published", article.Published.ToString("yyyy-MM-dd HH:mm:ss")},
                {"updated", article.Modified.ToString("yyyy-MM-dd HH:mm:ss")},
                {"title", HttpUtility.HtmlDecode(article.Title ?? "").Trim()},
                {"header", HttpUtility.HtmlDecode(article.Header ?? "").Trim()},
                {"html", article.Content},
                {"elements", elements}
            };

        }

        public static string ToJson(BorgerDkArticle article) {
            return ToJsonObject(article).ToString();
        }

        public static string ToJson(BorgerDkArticle article, Formatting formatting) {
            return ToJsonObject(article).ToString(formatting);
        }

        public static BorgerDkArticle GetArticle(string url, int municipalityId, bool useCache) {

            // Get the endpoint
            BorgerDkEndpoint endpoint = BorgerDkEndpoint.GetFromUrl(url);

            // Get the municipality
            BorgerDkMunicipality municipality = BorgerDkMunicipality.GetFromCode(municipalityId);

            // Some input validation
            if (endpoint == null) throw new Exception("Den angivne URL er på et ukendt domæne.");
            if (municipality == null) throw new Exception("En kommune med det angivne ID blev ikke fundet.");
            if (!endpoint.IsValidUrl(url)) throw new Exception("Den angivne URL er ikke gyldig.");

            // Declare a name for the article in the cache
            string cacheName = "BorgerDk_Url:" + url + "_Kommune:" + municipalityId;

            if (!useCache || HttpContext.Current.Cache[cacheName] == null) {

                // Initialize the service
                BorgerDkService service = new BorgerDkService(endpoint, municipality);

                // TODO: Add some caching here?
                int articleId = service.GetArticleIdFromUrl(url);

                // Get the article from the ID
                BorgerDkArticle article = service.GetArticleFromId(articleId);

                HttpContext.Current.Cache.Add(
                    cacheName, article, null, DateTime.Now.AddHours(6),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.High, null
                );

                return article;

            }

            return HttpContext.Current.Cache[cacheName] as BorgerDkArticle;

        }

        public static bool HasCacheFile(IBorgerDkArticle article) {
            return File.Exists(GetCacheFilePath(article));
        }

        public static void SaveToCacheFile(BorgerDkArticle article) {

            try {

                // Declare the path to the storage directory
                string storagePath = HttpContext.Current.Server.MapPath("~/App_Data/Skybrud.BorgerDk/");

                // Create the directory if it doesn't already exist
                if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);

                // Declare the path to the cache file
                string filePath = String.Format("{0}{1}__{2}__{3}.json", storagePath, article.Domain.Replace(".", "_"), article.Id, article.Municipality.Code);

                // Save the article to the disk
                File.WriteAllText(filePath, ToJson(article, Formatting.Indented));

            } catch (Exception ex) {

                LogHelper.Error(typeof(BorgerDkHelpers), "Unable to save cache file for article **" + article.Title + "** (" + article.Id + ")", ex);

            }

        }

        public static string GetCacheFilePath(IBorgerDkArticle article) {

            // Make sure we have a storage directory
            string storagePath = HttpContext.Current.Server.MapPath("~/App_Data/Skybrud.BorgerDk/");

            // Declare the path to the cache file
            return String.Format("{0}{1}__{2}__{3}.json", storagePath, article.Domain.Replace(".", "_"), article.Id, article.Municipality.Code);

        }

    }

}