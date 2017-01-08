using Newtonsoft.Json.Linq;

namespace Skybrud.Umbraco.BorgerDk.Model.Json {
    
    public class BorgerDkJsonBlockElement : BorgerDkJsonElement {

        #region Properties

        public BorgerDkJsonMicroArticle[] MicroArticles { get; private set; }

        #endregion

        #region Constructors

        protected BorgerDkJsonBlockElement(string type) : base(type) { }

        #endregion

        #region Static methods

        public new static BorgerDkJsonBlockElement Parse(JObject obj) {
            string type = obj.GetString("type");
            return new BorgerDkJsonBlockElement(type) {
                MicroArticles = obj.GetArray("microArticles", BorgerDkJsonMicroArticle.Parse)
            };

        }

        #endregion

    }

}