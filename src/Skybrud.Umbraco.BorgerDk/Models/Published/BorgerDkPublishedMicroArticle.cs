using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Skybrud.Integrations.BorgerDk;
using Skybrud.Integrations.BorgerDk.Elements;

namespace Skybrud.Umbraco.BorgerDk.Models.Published {

    /// <summary>
    /// Class representing a micro article of a <see cref="BorgerDkPublishedArticle"/>.
    /// </summary>
    public class BorgerDkPublishedMicroArticle {

        /// <summary>
        /// Gets the ID of the micro article.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; }

        /// <summary>
        /// Gets the title of the micro article.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; }

        /// <summary>
        /// Gets the HTML content of the micro article.
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; }

        /// <summary>
        /// Initializes a new published micro article based on the specified <paramref name="micro"/> article.
        /// </summary>
        /// <param name="micro">The micro article as received from an instance of <see cref="BorgerDkArticle"/>.</param>
        public BorgerDkPublishedMicroArticle(BorgerDkMicroArticle micro) {
            Id = micro.Id;
            Title = micro.Title;
            Content = Regex.Replace(micro.Content, "^<h2>(.+?)</h2>", string.Empty);
        }

    }

}