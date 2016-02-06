using System;
using System.Web;
using Newtonsoft.Json;
using www.borger.dk._2009.WSArticleExport.v1.types;

namespace Skybrud.Umbraco.BorgerDk.Models {

    public class BorgerDkArticleSummary {

        #region Properties

        [JsonProperty("id")]
        public int Id { get; private set; }

        [JsonProperty("url")]
        public string Url { get; private set; }
        
        [JsonProperty("title")]
        public string Title { get; private set; }
        
        [JsonProperty("published")]
        public DateTime Published { get; private set; }

        [JsonProperty("updated")]
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