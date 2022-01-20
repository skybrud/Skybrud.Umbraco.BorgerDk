using Skybrud.Integrations.BorgerDk;

namespace Skybrud.Umbraco.BorgerDk {

    public class BorgerDkUtils {

        public static string GetUniqueId(BorgerDkArticle article) {
            return GetUniqueId(article.Domain, article.Municipality.Code, article.Id);
        }

        public static string GetUniqueId(string domain, int municipality, int articleId) {
            return $"{domain}_{municipality}_{articleId}";
        }

    }

}