using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Newtonsoft.Json.Linq;
using Skybrud.BorgerDk;
using Skybrud.BorgerDk.Elements;
using Skybrud.Umbraco.BorgerDk.Exceptions;
using Skybrud.Umbraco.BorgerDk.Models;
using www.borger.dk._2009.WSArticleExport.v1.types;

namespace Skybrud.Umbraco.BorgerDk {

    public static class BorgerDkHelpers {

        public static string GetCachePath(BorgerDkArticle article) {

            // Generate the absolute path to the file
            return HttpContext.Current.Server.MapPath(String.Format(
                "~/App_Data/BorgerDk/{0}_{1}_{2}.{3}",
                article.Municipality.Code,
                article.Domain.Replace(".", ""),
                article.Id,
                "xml"
            ));

        }

        /// <summary>
        /// Converts the specified <code>article</code> into an instance of <code>JObject</code>.
        /// </summary>
        /// <param name="article">The article to be converted.</param>
        /// <returns>Returns an instance of <code>JObject</code> representing the article.</returns>
        public static JObject ToJsonObject(BorgerDkArticle article) {
            
            JArray elements = new JArray();
    
            JObject obj = new JObject {
                {"id", article.Id},
                {"domain", article.Domain},
                {"url", article.Url},
                {"municipality", BorgerDkMunicipality.AssensKommune.Code},
                {"title", HttpUtility.HtmlDecode(article.Title).Trim()},
                {"header", HttpUtility.HtmlDecode(article.Header).Trim()},
                {"published", GetUnixTimeFromDateTime(article.Published)},
                {"modified", GetUnixTimeFromDateTime(article.Modified)},
                {"updated", GetUnixTimeFromDateTime(article.Modified)},
                {"elements", elements}
            };
    
            foreach (var element in article.Elements) {

                BorgerDkTextElement text = element as BorgerDkTextElement;
                BorgerDkBlockElement block = element as BorgerDkBlockElement;

                if (text != null) {
                    elements.Add(new JObject {
                        {"type", text.Type},
                        {"title", text.Title},
                        {"content", text.Content}
                    });
                } else if (block != null) {
                    elements.Add(new JObject {
                        {"type", block.Type},
                        {"microArticles", new JArray(
                            from micro in block.MicroArticles
                            select new JObject {
                                {"id", micro.Id},
                                {"title", micro.Title},
                                {"content", micro.Content}
                            }
                        )}
                    });
                }
        
            }

            return obj;

        }

        /// <summary>
        /// Attempts to load the article with the specified <code>url</code> for the specified <code>municipality</code>.
        /// </summary>
        /// <param name="url">The URL of the article.</param>
        /// <param name="municipalityId">The ID of the municipality.</param>
        /// <param name="useCache">Whether the method should look for the article in the local runtime cachhe, or make a new request to the Borger.dk web service.</param>
        /// <returns>Returns an instance of <code>BorgerDkArticle</code>.</returns>
        public static BorgerDkArticle GetArticle(string url, int municipalityId, bool useCache) {

            // Get the endpoint
            BorgerDkEndpoint endpoint = BorgerDkEndpoint.GetFromUrl(url);

            // Get the municipality
            BorgerDkMunicipality municipality = BorgerDkMunicipality.FirstOrDefault(x => x.Code == municipalityId);

            // Some input validation
            if (endpoint == null) throw new BorgerDkException("Den angivne URL er på et ukendt domæne.");
            if (municipality == null) throw new BorgerDkException("En kommune med det angivne ID blev ikke fundet.");
            if (!endpoint.IsValidUrl(url)) throw new BorgerDkException("Den angivne URL er ikke gyldig.");

            // Declare a name for the article in the cache
            string cacheName = "BorgerDk_Url:" + url + "_Kommune:" + municipalityId;

            if (!useCache || HttpContext.Current.Cache[cacheName] == null) {

                // Initialize the service
                BorgerDkService service = new BorgerDkService(endpoint, municipality);

                // TODO: Add some caching here?
                int articleId = service.GetArticleIdFromUrl(url);

                // Get the article from the ID
                BorgerDkArticle article = service.GetArticleFromId(articleId);

                // Add the article to the runtime cache (for six hours)
                HttpContext.Current.Cache.Add(
                    cacheName, article, null, DateTime.Now.AddHours(6),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.High, null
                );

                // We also save the article to the disk so we can use it in the frontend
                SaveToAppData(article);

                return article;

            }

            return HttpContext.Current.Cache[cacheName] as BorgerDkArticle;

        }

        /// <summary>
        /// Gets a list of articles matching the specified <code>options</code>.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static BorgerDkArticlesResult GetArticles(BorgerDkGetArticlesOptions options) {

            // Make sure the query is lower case
            string query = (options.Query ?? "").ToLower().Trim();
            
            // Get all articles for the endpoint
            ArticleDescription[] all = options.Endpoint.GetClient().GetAllArticles();

            int total = all.Length;

            // Convert the articles to our own model
            var articles = all.Select(x => new BorgerDkArticleSummary(x));

            // Filter the articles by a search query?
            if (!String.IsNullOrWhiteSpace(query)) {
                articles = articles.Where(x => x.Title.ToLower().Contains(query));
            }

            switch (options.SortField) {
                case BorgerDkArticleSortField.Published:
                    articles = options.SortOrder == BorgerDkSortOrder.Ascending ? articles.OrderBy(x => x.Published) : articles.OrderByDescending(x => x.Published);
                    break;
                case BorgerDkArticleSortField.Updated:
                    articles = options.SortOrder == BorgerDkSortOrder.Ascending ? articles.OrderBy(x => x.Updated) : articles.OrderByDescending(x => x.Updated);
                    break;
                case BorgerDkArticleSortField.Title:
                    articles = options.SortOrder == BorgerDkSortOrder.Ascending ? articles.OrderBy(x => x.Title) : articles.OrderByDescending(x => x.Title);
                    break;
            }

            return new BorgerDkArticlesResult(options.Endpoint, total, articles.ToArray(), options.SortField, options.SortOrder);

        }

        /// <summary>
        /// Gets an instance of <see cref="System.DateTime"/> from the specified <code>timestamp</code>.
        /// </summary>
        /// <param name="timestamp">The Unix timestamp.</param>
        /// <returns>Returns an instance of <see cref="System.DateTime"/> from the specified <code>timestamp</code>.</returns>
        public static DateTime GetDateTimeFromUnixTime(long timestamp) {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
        }

        /// <summary>
        /// Gets a Unix timestamp from the specified <code>dateTime</code>.
        /// </summary>
        /// <param name="dateTime">The instance of <see cref="System.DateTime"/> to be converted.</param>
        /// <returns></returns>
        public static long GetUnixTimeFromDateTime(DateTime dateTime) {
            return (long) (dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// Saves the specified <code>article</code> to the <code>~/App_Data/BorgerDk/</code> directory. If the
        /// directory does not already exist, it will be created.
        /// </summary>
        /// <param name="article">The instance of <see cref="Skybrud.BorgerDk.BorgerDkArticle"/> representing the
        /// article.</param>
        public static void SaveToAppData(BorgerDkArticle article) {

            // Obviously we can't save the article if NULL
            if (article == null) throw new ArgumentNullException("article");

            // Get the municipality ID
            int municipalityId = article.Municipality.Code;

            // Get a reference to the Borger.dk directory (and create it if not found)
            DirectoryInfo directory = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/App_Data/BorgerDk/"));
            if (!directory.Exists) directory.Create();

            // Construct the cache path
            string path = directory.FullName + municipalityId + "_" + article.Domain.Replace(".", "") + "_" + article.Id + ".xml";
    
            // And finally the article (XML) to the disk
            article.ToXElement().Save(path);

        }

    }

    public enum BorgerDkArticleSortField {
        Title,
        Published,
        Updated
    }

    public enum BorgerDkSortOrder {
        Ascending,
        Descending
    }


    public class BorgerDkGetArticlesOptions {

        public BorgerDkEndpoint Endpoint { get; set; }

        public string Query { get; set; }

        public BorgerDkArticleSortField SortField { get; set; }

        public BorgerDkSortOrder SortOrder { get; set; }

        public bool UseCache { get; set; }

        public BorgerDkGetArticlesOptions() {
            Endpoint = BorgerDkEndpoint.Default;
            UseCache = true;
        }

    }

    public class BorgerDkArticlesResult {

        public BorgerDkEndpoint Endpoint { get; private set; }

        public int Total { get; private set; }

        public BorgerDkArticleSummary[] Articles { get; private set; }

        public BorgerDkArticlesResult(BorgerDkEndpoint endpoint, int total, BorgerDkArticleSummary[] articles, BorgerDkArticleSortField sortField, BorgerDkSortOrder sortOrder) {
            Endpoint = endpoint;
            Total = total;
            Articles = articles;
        }

    }

}