using System;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using Skybrud.BorgerDk;
using Skybrud.BorgerDk.Elements;
using Skybrud.Umbraco.BorgerDk.Exceptions;
using Skybrud.Umbraco.BorgerDk.Models;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.IO;
using www.borger.dk._2009.WSArticleExport.v1.types;
using BorgerDkMunicipality = Skybrud.Umbraco.BorgerDk.Models.BorgerDkMunicipality;

namespace Skybrud.Umbraco.BorgerDk {

    public static class BorgerDkHelpers {

        /// <summary>
        /// Gets the virtual path to the Borger.dk temp directory.
        /// </summary>
        const string VirtualPath = "~/App_Data/TEMP/BorgerDk/";

        private static IRuntimeCacheProvider RuntimeCache => ApplicationContext.Current.ApplicationCache.RuntimeCache;

        public static FileInfo[] GetCachedFiles() {

            // Get a reference to the temp directory
            DirectoryInfo directory = new DirectoryInfo(IOHelper.MapPath(VirtualPath));

            // Return an array of all .xml files
            return directory.GetFiles("*.xml");

        }

        public static string GetCachePath(BorgerDkArticle article) {
            return GetCachePath(article.Municipality, article.Domain, article.Id);
        }

        public static string GetCachePath(BorgerDkMunicipality municipality, string domain, int articleId) {
            return GetCachePath(municipality.Code, domain, articleId);
        }

        public static string GetCachePath(Skybrud.BorgerDk.BorgerDkMunicipality municipality, string domain, int articleId) {
            return GetCachePath(municipality.Code, domain, articleId);
        }

        public static string GetCachePath(int municipalityCode, string domain, int articleId) {
            return IOHelper.MapPath(String.Format(
                "{0}{1}_{2}_{3}.{4}",
                VirtualPath,
                municipalityCode,
                domain.Replace(".", ""),
                articleId,
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
                {"municipality", article.Municipality.Code},
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
            Skybrud.BorgerDk.BorgerDkMunicipality municipality = Skybrud.BorgerDk.BorgerDkMunicipality.FirstOrDefault(x => x.Code == municipalityId);

            // Some input validation
            if (endpoint == null) throw new BorgerDkException("Den angivne URL er på et ukendt domæne.");
            if (municipality == null) throw new BorgerDkException("En kommune med det angivne ID blev ikke fundet.");
            if (!endpoint.IsValidUrl(url)) throw new BorgerDkException("Den angivne URL er ikke gyldig.");

            // Return the article directly as caching is disabled
            if (!useCache) return GetArticleCallback(endpoint, municipality, url);

            // Declare a name for the article in the cache
            string cacheName = "BorgerDk_Url:" + url + "_Kommune:" + municipalityId;

            // Attempt to get the article from the cache (or add it if not present)
            return (BorgerDkArticle) RuntimeCache.GetCacheItem(cacheName, () => GetArticleCallback(endpoint, municipality, url), TimeSpan.FromHours(6));


        }

        private static BorgerDkArticle GetArticleCallback(BorgerDkEndpoint endpoint, Skybrud.BorgerDk.BorgerDkMunicipality municipality, string url) {

            // Initialize the service
            BorgerDkService service = new BorgerDkService(endpoint, municipality);

            // TODO: Add some caching here?
            int articleId = service.GetArticleIdFromUrl(url);

            // Get the article from the ID
            BorgerDkArticle article = service.GetArticleFromId(articleId);

            // We also save the article to the disk so we can use it in the frontend
            SaveToDisk(article);

            return article;

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
        /// Saves the specified <code>article</code> to the <code>~/App_Data/TEMP/BorgerDk/</code> directory. If the
        /// directory does not already exist, it will be created.
        /// </summary>
        /// <param name="article">The instance of <see cref="Skybrud.BorgerDk.BorgerDkArticle"/> representing the
        /// article.</param>
        public static void SaveToDisk(BorgerDkArticle article) {

            // Obviously we can't save the article if NULL
            if (article == null) throw new ArgumentNullException("article");

            // Make sure we have a directory
            EnsureTempDirectory();

            // Construct the cache path
            string path = GetCachePath(article);
    
            // And finally the article (XML) to the disk
            article.ToXElement().Save(path);

        }

        /// <summary>
        /// Ensures that the Borger.dk temp directory (<code>~/App_Data/TEMP/BorgerDk/</code>) exists on disk.
        /// </summary>
        /// <returns>Returns an instance of <see cref="DirectoryInfo"/> representing the temp directory.</returns>
        public static DirectoryInfo EnsureTempDirectory() {
            
            // Get a reference to the Borger.dk directory
            DirectoryInfo directory = new DirectoryInfo(IOHelper.MapPath(VirtualPath));
            
            // Create the directory if it doesn't already exist
            if (!directory.Exists) directory.Create();

            return directory;

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