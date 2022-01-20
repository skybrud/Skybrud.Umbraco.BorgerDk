using System.Collections.Generic;
using Newtonsoft.Json;

namespace Skybrud.Umbraco.BorgerDk.Models.Published {

    /// <summary>
    /// Class representing a block element - also referred to as <c>kernetekst</c>.
    /// </summary>
    public class BorgerDkPublishedBlockElement : BorgerDkPublishedElement {

        /// <summary>
        /// Gets an array with the selected micro articles.
        /// </summary>
        [JsonProperty("microArticles")]
        public BorgerDkPublishedMicroArticle[] MicroArticles { get; }

        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="microArticles"/>.
        /// </summary>
        /// <param name="microArticles">The micro articles that should make up the element.</param>
        public BorgerDkPublishedBlockElement(List<BorgerDkPublishedMicroArticle> microArticles) {
            Id = "kernetekst";
            Title = "Kernetekst";
            MicroArticles = microArticles.ToArray();
        }

    }

}