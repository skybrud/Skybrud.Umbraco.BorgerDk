using Newtonsoft.Json;

namespace Skybrud.Umbraco.BorgerDk.Models.Published {

    /// <summary>
    /// Base class representing an element of a <see cref="BorgerDkPublishedArticle"/>. 
    /// </summary>
    public class BorgerDkPublishedElement {

        /// <summary>
        /// Gets the ID of the element.
        /// </summary>
        [JsonProperty("id", Order = -99)]
        public string Id { get; protected set; }

        /// <summary>
        /// Gets the title of the element.
        /// </summary>
        [JsonProperty("title", Order = -98)]
        public string Title { get; protected set; }

    }

}