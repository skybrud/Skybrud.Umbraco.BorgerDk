using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using Skybrud.Essentials.Xml.Extensions;
using Umbraco.Core;

namespace Skybrud.Umbraco.BorgerDk.Models.Cached {

    /// <summary>
    /// Class representing a text element (block) of a Borger.dk article.
    /// </summary>
    public class BorgerDkCachedTextElement : BorgerDkCachedElement {

        #region Properties

        /// <summary>
        /// Gets the title of the text element (block).
        /// </summary>
        public string Title { get; protected set; }

        /// <summary>
        /// Gets the raw HTML of the block including the title.
        /// </summary>
        public string Html { get; protected set; }

        /// <summary>
        /// Gets the content making up the content of the block.
        /// </summary>
        public string Content { get; protected set; }

        /// <summary>
        /// Gets the HTML string making up the content of the block.
        /// </summary>
        public HtmlString ContentHtml {
            get { return new HtmlString(Content); }
        }

        /// <summary>
        /// Gets the inner text of the block.
        /// </summary>
        public string InnerText {
            get { return Content.StripHtml(); }
        }

        #endregion

        #region Constructors

        protected BorgerDkCachedTextElement(XElement xml) {

            string title = xml.GetElementValue("title");
            string html = (xml.GetElementValue("html") ?? "").Trim();
            string content = Regex.Replace(html, "^<h3>(.+?)</h3>", "").Trim();

            Alias = xml.Name.LocalName;
            Title = title;
            XElement = xml;
            Html = html;
            Content = content;

        }

        #endregion

        #region Static methods

        public static BorgerDkCachedTextElement Parse(XElement xml) {
            return xml == null ? null : new BorgerDkCachedTextElement(xml);
        }

        #endregion

    }

}