using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.BorgerDk;
using Skybrud.Umbraco.BorgerDk.Extensions.JObject;
using Skybrud.Umbraco.BorgerDk.Models.Cached;

namespace Skybrud.Umbraco.BorgerDk.Models {

    /// <summary>
    /// Class representing a selection of a Borger.dk article.
    /// </summary>
    public class BorgerDkArticleSelection {

        #region Properties

        /// <summary>
        /// Gets a reference to the underlying instance of <see cref="JObject"/>.
        /// </summary>
        [JsonIgnore]
        public JObject JObject { get; private set; }

        /// <summary>
        /// Gets the ID of the selected Borger.dk article, or <code>0</code> if no article has been selected.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the URL of the selected Borger.dk article.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Gets the URL of the selected Borger.dk article.
        /// </summary>
        public string Domain { get; private set; }

        /// <summary>
        /// Gets the municipality ID of the selected Borger.dk article.
        /// </summary>
        public BorgerDkMunicipality Municipality { get; private set; }

        /// <summary>
        /// Gets the title of the selected Borger.dk article.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the header of the selected Borger.dk article.
        /// </summary>
        public string Header { get; private set; }

        /// <summary>
        /// Gets an array of the IDs of the selected micro articles and blocks.
        /// </summary>
        public string[] Selected { get; private set; }

        /// <summary>
        /// Gets whether an article has been selected.
        /// </summary>
        public bool HasSelection {
            get { return Id > 0 && !String.IsNullOrWhiteSpace(Url); }
        }

        /// <summary>
        /// Gets a reference to the selected article.
        /// </summary>
        public BorgerDkCachedArticle Article { get; private set; }

        #endregion

        #region Constructors

        private BorgerDkArticleSelection() {
            Selected = new string[0];
        }

        protected BorgerDkArticleSelection(JObject obj) {
            JObject = obj;
            Id = obj.GetInt32("id");
            Url = obj.GetString("url");
            Domain = obj.GetString("domain");
            Municipality = obj.GetInt32("municipality", BorgerDkMunicipality.GetFromCode);
            Title = obj.GetString("title");
            Header = obj.GetString("header");
            Selected = obj.GetStringArray("selected");
            Article = BorgerDkCachedArticle.Load(Municipality, Domain, Id);
        }

        #endregion

        #region Static methods

        /// <summary>
        /// Parses the specified string into an instance of <see cref="BorgerDkArticleSelection"/>.
        /// </summary>
        /// <param name="str">The JSON string to be parsed.</param>
        /// <returns>Returns an instance of <see cref="BorgerDkArticleSelection"/>.</returns>
        public static BorgerDkArticleSelection Deserialize(string str) {
            if (str == null || !str.StartsWith("{") || !str.EndsWith("}")) return new BorgerDkArticleSelection();
            return Parse(JsonConvert.DeserializeObject<JObject>(str));
        }
        
        /// <summary>
        /// Parses the specified <code>obj</code> into an instance of <see cref="BorgerDkArticleSelection"/>, or <code>null</code> if <code>obj</code> is <code>null</code>.
        /// </summary>
        /// <param name="obj">The instance of <code>JObject</code> to be parsed.</param>
        /// <returns>Returns an instance of <see cref="BorgerDkArticleSelection"/>.</returns>
        public static BorgerDkArticleSelection Parse(JObject obj) {
            return obj == null ? null : new BorgerDkArticleSelection(obj);
        }

        #endregion

    }

}