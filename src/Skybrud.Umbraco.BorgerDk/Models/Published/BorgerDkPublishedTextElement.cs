using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Skybrud.Integrations.BorgerDk;
using Skybrud.Integrations.BorgerDk.Elements;

namespace Skybrud.Umbraco.BorgerDk.Models.Published {

    /// <summary>
    /// Class representing a text based element of a <see cref="BorgerDkPublishedArticle"/>.
    /// </summary>
    public class BorgerDkPublishedTextElement : BorgerDkPublishedElement {
        
        /// <summary>
        /// Gets the HTML content of the micro article.
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; }
        
        /// <summary>
        /// Initializes a new published text element based on the specified <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The element as received from an instance of <see cref="BorgerDkArticle"/>.</param>
        public BorgerDkPublishedTextElement(BorgerDkTextElement element) {
            Id = element.Id;
            Title = element.Title;
            Content = Regex.Replace(element.Content, "^<h3>(.+?)</h3>", string.Empty);
        }

    }

}