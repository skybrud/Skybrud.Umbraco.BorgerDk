using System.Linq;
using System.Xml.Linq;

namespace Skybrud.Umbraco.BorgerDk.Models.Cached {

    /// <summary>
    /// Class representing the <em>kernetekst</em> element of a Borger.dk article.
    /// </summary>
    public class BorgerDkCachedKernetekstElement : BorgerDkCachedElement {

        #region Properties
        
        /// <summary>
        /// Gets an array of all micro articles within the <em>kernetekst</em> element.
        /// </summary>
        public BorgerDkCachedMicroArticle[] MicroArticles { get; private set; }

        #endregion

        #region Constructors

        protected BorgerDkCachedKernetekstElement(XElement xml) {
            Alias = xml.Name.LocalName;
            XElement = xml;
            MicroArticles = (
                from xMicroArticle in xml.Elements("microArticle")
                select BorgerDkCachedMicroArticle.Parse(xMicroArticle)
            ).ToArray();
        }

        #endregion

        #region Static methods

        public static BorgerDkCachedKernetekstElement Parse(XElement xml) {
            return xml == null ? null : new BorgerDkCachedKernetekstElement(xml);
        }

        #endregion

    }

}