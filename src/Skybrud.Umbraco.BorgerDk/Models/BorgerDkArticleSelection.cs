using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Extensions;
using Skybrud.Umbraco.BorgerDk.Models.Cached;
using Umbraco.Core.Logging;

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
        [JsonProperty("id")]
        public int Id { get; private set; }

        /// <summary>
        /// Gets the URL of the selected Borger.dk article.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; private set; }

        /// <summary>
        /// Gets the URL of the selected Borger.dk article.
        /// </summary>
        [JsonProperty("domain")]
        public string Domain { get; private set; }

        /// <summary>
        /// Gets the municipality ID of the selected Borger.dk article.
        /// </summary>
        [JsonProperty("municipality")]
        public BorgerDkMunicipality Municipality { get; private set; }

        /// <summary>
        /// Gets the editorial title of the section.
        /// </summary>
        [JsonProperty("title")]
        public string Title {
            get {
                switch (CustomTitleType) {
                    case BorgerDkArticleTitleType.None: return "";
                    case BorgerDkArticleTitleType.Custom: return CustomTitleValue;
                    default: return Article.Title;
                }
            }
        }

        /// <summary>
        /// Gets whether the selection has an editorial title.
        /// </summary>
        [JsonIgnore]
        public bool HasTitle {
            get { return !String.IsNullOrWhiteSpace(Title); }
        }

        /// <summary>
        /// Gets the header of the selected Borger.dk article.
        /// </summary>
        [JsonProperty("header")]
        public string Header { get; private set; }

        /// <summary>
        /// Gets an array of the IDs of the selected micro articles and blocks.
        /// </summary>
        [JsonIgnore]
        public string[] Selected { get; private set; }

        /// <summary>
        /// Gets whether an article has been selected.
        /// </summary>
        [JsonIgnore]
        public bool HasSelection {
            get { return Id > 0 && !String.IsNullOrWhiteSpace(Url); }
        }

        /// <summary>
        /// Gets a reference to the selected article.
        /// </summary>
        [JsonIgnore]
        public BorgerDkCachedArticle Article { get; private set; }

        /// <summary>
        /// Gets an array of all selected text blocks.
        /// </summary>
        [JsonProperty("blocks")]
        public BorgerDkCachedTextElement[] Blocks { get; private set; }

        /// <summary>
        /// Gets whether the selection has any text elements.
        /// </summary>
        [JsonIgnore]
        public bool HasBlocks {
            get { return Blocks.Length > 0; }
        }

        /// <summary>
        /// Gets an array of all selected micro articles.
        /// </summary>
        [JsonProperty("microArticles")]
        public BorgerDkCachedMicroArticle[] MicroArticles { get; private set; }

        /// <summary>
        /// Gets whether the selection has any micro articles.
        /// </summary>
        [JsonIgnore]
        public bool HasMicroArticles {
            get { return MicroArticles.Length > 0; }
        }

        /// <summary>
        /// Gets a reference to the <code>byline</code> element, or <code>null</code> if the element hasn't been
        /// selected.
        /// </summary>
        [JsonProperty("byline")]
        public BorgerDkCachedTextElement ByLine { get; }

        /// <summary>
        /// Gets whether the <c>byline</c> element has been selected.
        /// </summary>
        [JsonIgnore]
        public bool HasByLine => ByLine != null;

        /// <summary>
        /// Gets whether the selection is valid. A selection is considered valid if either <see cref="HasBlocks"/> or
        /// <see cref="HasMicroArticles"/> returns <code>true</code>.
        /// </summary>
        [JsonIgnore]
        public bool IsValid {
            get { return HasSelection && (HasBlocks || HasMicroArticles); }
        }

        [JsonIgnore]
        public BorgerDkArticleTitleType CustomTitleType { get; private set; }

        [JsonIgnore]
        public string CustomTitleValue { get; private set; }

        #endregion

        #region Constructors

        private BorgerDkArticleSelection() {
            Selected = new string[0];
            Blocks = new BorgerDkCachedTextElement[0];
            MicroArticles = new BorgerDkCachedMicroArticle[0];
        }

        protected BorgerDkArticleSelection(JObject obj) {

            var municipality = obj.GetInt32("municipality", Skybrud.BorgerDk.BorgerDkMunicipality.GetFromCode);

            JObject = obj;
            Id = obj.GetInt32("id");
            Url = obj.GetString("url");
            Domain = obj.GetString("domain");
            Municipality = municipality == null ? null : new BorgerDkMunicipality(municipality);
            Header = obj.GetString("header");
            Selected = obj.GetStringArray("selected");
            Article = BorgerDkCachedArticle.Load(municipality, Domain, Id);
            Blocks = (Article.Exists ? Article.Blocks.Where(x => IsSelected(x) && x.Alias != "byline").ToArray() : new BorgerDkCachedTextElement[0]);
            MicroArticles = Article.Exists ? Article.MicroArticles.Where(IsSelected).ToArray() : new BorgerDkCachedMicroArticle[0];
            CustomTitleType = obj.GetEnum("customTitle.type", BorgerDkArticleTitleType.Article);
            CustomTitleValue = (obj.GetString("customTitle.value") ?? "").Trim();
            ByLine = Article.Exists ? Article.Blocks.FirstOrDefault(x => IsSelected(x) && x.Alias == "byline") : null;
        }

        #endregion

        #region Member methods

        /// <summary>
        /// Gets whether an element with the specified <code>alias</code> has been selected.
        /// </summary>
        /// <param name="alias">The alias (or ID) of the element.</param>
        /// <returns>Returns <code>true</code> if a matching element has been selected, otherwise <code>false</code>.</returns>
        public bool IsSelected(string alias) {
            return !String.IsNullOrWhiteSpace(alias) && Selected.Contains(alias);
        }

        /// <summary>
        /// Gets whether the specified <code>text</code> element has been selected.
        /// </summary>
        /// <param name="text">An instance of <see cref="BorgerDkCachedTextElement"/> representing the text element.</param>
        /// <returns>Returns <code>true</code> if the element has been selected, otherwise <code>false</code>.</returns>
        public bool IsSelected(BorgerDkCachedTextElement text) {
            return text != null && Selected.Contains(text.Alias);
        }

        /// <summary>
        /// Gets whether the specified <code>microArticle</code> element has been selected.
        /// </summary>
        /// <param name="microArticle">An instance of <see cref="BorgerDkCachedMicroArticle"/> representing the micro article.</param>
        /// <returns>Returns <code>true</code> if the micro article has been selected, otherwise <code>false</code>.</returns>
        public bool IsSelected(BorgerDkCachedMicroArticle microArticle) {
            return microArticle != null && Selected.Contains(microArticle.Id);
        }

        /// <summary>
        /// Gets the selection as a searchable text - eg. for use in Examine.
        /// </summary>
        /// <returns>Returns an instance of <see cref="System.String"/> with the selection as a searchable text.</returns>
        public string GetSearchableText() {

            StringBuilder sb = new StringBuilder();

            // Append the "title" and "header"
            sb.AppendLine(Title);
            sb.AppendLine(Header);
            sb.AppendLine();

            // Append the selected block elements
            foreach (BorgerDkCachedTextElement block in Blocks) {
                sb.AppendLine(block.Title);
                sb.AppendLine(Regex.Replace(block.Content, "<.*?>", ""));
                sb.AppendLine();
            }

            // Append the selected micro articles
            foreach (BorgerDkCachedMicroArticle micro in MicroArticles) {
                sb.AppendLine(micro.Title);
                sb.AppendLine(Regex.Replace(micro.Content, "<.*?>", ""));
                sb.AppendLine();
            }

            return sb.ToString();

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
            try {
            return Parse(JsonConvert.DeserializeObject<JObject>(str));
            } catch (Exception ex) {
                LogHelper.Error<BorgerDkArticleSelection>("Unable to parse Borger.dk article JSON", ex);
                return new BorgerDkArticleSelection();
            }
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