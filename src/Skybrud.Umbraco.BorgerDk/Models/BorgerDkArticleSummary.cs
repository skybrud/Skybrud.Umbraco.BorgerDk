using System;
using System.Web;
using Newtonsoft.Json;
using Skybrud.Umbraco.BorgerDk.Json.Converters;
using www.borger.dk._2009.WSArticleExport.v1.types;

namespace Skybrud.Umbraco.BorgerDk.Models {

    /// <summary>
    /// Class representing a summary about a Borger.dk article.
    /// </summary>
    public class BorgerDkArticleSummary {

        #region Properties

        /// <summary>
        /// Gets the ID of the article.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; private set; }

        /// <summary>
        /// Gets the URL of the article.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; private set; }

        /// <summary>
        /// Gets the title of the article.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; private set; }

        /// <summary>
        /// Gets the timestamp for when the article was first published.
        /// </summary>
        [JsonProperty("published")]
        [JsonConverter(typeof(UnixDateTimeJsonConverter))]
        public DateTime Published { get; private set; }

        /// <summary>
        /// Gets the timestamp for when the article was last updated.
        /// </summary>
        [JsonProperty("updated")]
        [JsonConverter(typeof(UnixDateTimeJsonConverter))]
        public DateTime Updated { get; private set; }

        #endregion

        #region Constructors

        internal BorgerDkArticleSummary(ArticleDescription article) {
            Id = article.ArticleID;
            Url = article.ArticleUrl;
            Title = HttpUtility.HtmlDecode(article.ArticleTitle);
            Published = article.PublishingDate;
            Updated = article.LastUpdated;
        }

        #endregion

    }

}