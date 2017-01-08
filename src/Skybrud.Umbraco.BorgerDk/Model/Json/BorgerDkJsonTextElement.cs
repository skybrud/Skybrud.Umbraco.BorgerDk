using Newtonsoft.Json.Linq;

namespace Skybrud.Umbraco.BorgerDk.Model.Json {
    
    public class BorgerDkJsonTextElement : BorgerDkJsonElement {

        #region Properties

        public string Title { get; private set; }

        public string Html { get; private set; }

        #endregion

        #region Constructors

        protected BorgerDkJsonTextElement(string type) : base(type) { }

        #endregion

        #region Static methods

        public new static BorgerDkJsonTextElement Parse(JObject obj) {
            string type = obj.GetString("type");
            return new BorgerDkJsonTextElement(type) {
                Title = obj.GetString("title"),
                Html = obj.GetString("html")
            };

        }

        #endregion

    }

}