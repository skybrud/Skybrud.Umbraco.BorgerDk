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

        /// <summary>
        /// Gets whether the instance is of type <code>T</code>.
        /// </summary>
        /// <typeparam name="T">The intended type.</typeparam>
        public bool IsOfType<T>() where T : BorgerDkCachedElement {
            T casted = this as T;
            return casted != null;
        }

        /// <summary>
        /// Gets whether the instance is of type <code>T</code>.
        /// </summary>
        /// <typeparam name="T">The intended type.</typeparam>
        /// <param name="value">A reference to the casted value.</param>
        public bool IsOfType<T>(out T value) where T : BorgerDkCachedElement {
            value = this as T;
            return value != null;
        }

    }

}