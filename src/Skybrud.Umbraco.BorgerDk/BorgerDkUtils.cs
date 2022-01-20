using Skybrud.Integrations.BorgerDk;

namespace Skybrud.Umbraco.BorgerDk {

    /// <summary>
    /// Static class with various Borger.dk related utility methods.
    /// </summary>
    public static class BorgerDkUtils {

        /// <summary>
        /// Returns the unique ID of the specified <paramref name="article"/>.
        /// </summary>
        /// <param name="article">The article.</param>
        /// <returns>The unique ID of the article.</returns>
        public static string GetUniqueId(BorgerDkArticle article) {
            return GetUniqueId(article.Domain, article.Municipality.Code, article.Id);
        }
        
        /// <summary>
        /// Returns the unique ID of the article with the specified <paramref name="domain"/>, <paramref name="municipality"/> and <paramref name="articleId"/>.
        /// </summary>
        /// <param name="domain">The domain of the article.</param>
        /// <param name="municipality">The municipality of the article.</param>
        /// <param name="articleId">The ID of the article.</param>
        /// <returns>The unique ID of the article.</returns>
        public static string GetUniqueId(string domain, int municipality, int articleId) {
            return $"{domain}_{municipality}_{articleId}";
        }

    }

}