using Newtonsoft.Json.Linq;

namespace Skybrud.Umbraco.BorgerDk.Model.Json {
    
    public class BorgerDkJsonElement {

        #region Properties

        public string Type { get; private set; }

        #endregion

        #region Constructors

        protected BorgerDkJsonElement(string type) {
            Type = type;
        }

        #endregion

        #region Static methods

        public static BorgerDkJsonElement Parse(JObject obj) {
            switch (obj.GetString("type")) {
                case "kernetekst":
                    return BorgerDkJsonBlockElement.Parse(obj);
                default:
                    return BorgerDkJsonTextElement.Parse(obj);
            }
        }

        #endregion

    }

}