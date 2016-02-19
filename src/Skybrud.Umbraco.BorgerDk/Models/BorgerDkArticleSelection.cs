using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.BorgerDk;
using Skybrud.Umbraco.BorgerDk.Extensions.JObject;
using Skybrud.Umbraco.BorgerDk.Models.Cached;

namespace Skybrud.Umbraco.BorgerDk.Models {

    /// <summary>
    /// Class representing a selection of a Borger.dk article as saved from either an Umbraco property editor or grid editor.
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

        /// <summary>
        /// Gets an array of all selected text blocks.
        /// </summary>
        public BorgerDkCachedTextElement[] Blocks { get; private set; }

        /// <summary>
        /// Gets whether the selection has any text elements.
        /// </summary>
        public bool HasBlocks {
            get { return Blocks.Length > 0; }
        }

        /// <summary>
        /// Gets an array of all selected micro articles.
        /// </summary>
        public BorgerDkCachedMicroArticle[] MicroArticles { get; private set; }

        /// <summary>
        /// Gets whether the selection has any micro articles.
        /// </summary>
        public bool HasMicroArticles {
            get { return MicroArticles.Length > 0; }
        }

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
            Blocks = (Article == null ? new BorgerDkCachedTextElement[0] : Article.Blocks.Where(IsSelected).ToArray());
            MicroArticles = (Article == null ? new BorgerDkCachedMicroArticle[0] : Article.MicroArticles.Where(IsSelected).ToArray());
        }

        #endregion

        #region Member methods

        public bool IsSelected(BorgerDkCachedTextElement text) {
            return text != null && Selected.Contains(text.Alias);
        }

        public bool IsSelected(BorgerDkCachedMicroArticle microArticle) {
            return microArticle != null && Selected.Contains(microArticle.Id);
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