using System.Xml.Linq;

namespace Skybrud.Umbraco.BorgerDk.Models.Cached {

    /// <summary>
    /// Class representing a generic element of a Borger.dk article.
    /// </summary>
    public class BorgerDkCachedElement {

        /// <summary>
        /// Gets the instance of <see cref="XElement"/> the micro article was parsed from.
        /// </summary>
        public XElement XElement { get; protected set; }

        /// <summary>
        /// Gets the alias (or ID if you will) of the element.
        /// </summary>
        public string Alias { get; protected set; }

    }

}