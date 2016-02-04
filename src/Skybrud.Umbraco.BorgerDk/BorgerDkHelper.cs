using System;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Newtonsoft.Json.Linq;
using Skybrud.BorgerDk;
using Skybrud.BorgerDk.Elements;
using Skybrud.Umbraco.BorgerDk.Exceptions;

namespace Skybrud.Umbraco.BorgerDk {
    
    public static class BorgerDkHelper {

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

                return article;

            }

            return HttpContext.Current.Cache[cacheName] as BorgerDkArticle;

        }

        public static long GetUnixTimeFromDateTime(DateTime dateTime) {
            return (long) (dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

    }

}