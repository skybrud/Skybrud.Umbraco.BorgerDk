using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Skybrud.Integrations.BorgerDk.Elements;

namespace Skybrud.Umbraco.BorgerDk.Models.Published {

    public class BorgerDkPublishedMicroArticle {

        [JsonProperty("id")]
        public string Id { get; }

        [JsonProperty("title")]
        public string Title { get; }

        [JsonProperty("content")]
        public string Content { get; }

        public BorgerDkPublishedMicroArticle(BorgerDkMicroArticle micro) {
            Id = micro.Id;
            Title = micro.Title;
            Content = Regex.Replace(micro.Content, "^<h2>(.+?)</h2>", string.Empty);
        }

    }

}