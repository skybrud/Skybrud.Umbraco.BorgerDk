using Newtonsoft.Json.Linq;

namespace Skybrud.Umbraco.BorgerDk.Model.Json {
    
    public class BorgerDkJsonMicroArticle {

        #region Properties

        public string Id { get; private set; }

        public string Title { get; private set; }

        public string Html { get; private set; }

        #endregion

        #region Constructors

        protected BorgerDkJsonMicroArticle() { }

        #endregion

        #region Static methods

        public static BorgerDkJsonMicroArticle Parse(JObject obj) {
            return new BorgerDkJsonMicroArticle {
                Id = obj.GetString("id"),
                Title = obj.GetString("title"),
                Html = obj.GetString("html")
            };
        }

        #endregion

    }

}