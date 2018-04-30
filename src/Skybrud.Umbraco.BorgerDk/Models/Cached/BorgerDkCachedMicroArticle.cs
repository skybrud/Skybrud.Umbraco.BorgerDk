using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using Newtonsoft.Json;
using Skybrud.Essentials.Xml.Extensions;

namespace Skybrud.Umbraco.BorgerDk.Models.Cached {

    /// <summary>
    /// Class representing a micro article of a Borger.dk article.
    /// </summary>
    public class BorgerDkCachedMicroArticle {

        #region Properties

        /// <summary>
        /// Gets the instance of <see cref="XElement"/> the micro article was parsed from.
        /// </summary>
        [JsonIgnore]
        public XElement XElement { get; protected set; }

        /// <summary>
        /// Gets the unique ID of the micro article.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; protected set; }

        /// <summary>
        /// Gets the title of the micro article.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; protected set; }

        /// <summary>
        /// Gets the raw HTML of the micro article including the title.
        /// </summary>
        [JsonIgnore]
        public string Html { get; protected set; }

        /// <summary>
        /// Gets the content making up the content of the micro article.
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; protected set; }

        /// <summary>
        /// Gets the HTML string making up the content of the micro article.
        /// </summary>
        [JsonIgnore]
        public HtmlString ContentHtml {
            get { return new HtmlString(Content); }
        }

        #endregion

        #region Constructors

        protected BorgerDkCachedMicroArticle(XElement xml) {
            
            string id  = xml.GetAttributeValue("id");
            string title = xml.GetElementValue("title");
            string html = (xml.GetElementValue("html") ?? "").Trim();
            string content = Regex.Replace(html, "^<h2>(.+?)</h2>", "").Trim();

            XElement = xml;
            Id = id;
            Title = title;
            Html = html;
            Content = content;

        }

        #endregion

        #region Static methods

        public static BorgerDkCachedMicroArticle Parse(XElement xml) {
            return xml == null ? null : new BorgerDkCachedMicroArticle(xml);
        }

        #endregion

    }

}