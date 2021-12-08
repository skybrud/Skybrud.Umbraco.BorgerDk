using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Skybrud.Integrations.BorgerDk.Elements;

namespace Skybrud.Umbraco.BorgerDk.Models.Published {

    public class BorgerDkPublishedTextElement : BorgerDkPublishedElement {

        [JsonProperty("content")]
        public string Content { get; }

        public BorgerDkPublishedTextElement(BorgerDkTextElement element) {
            Id = element.Id;
            Title = element.Title;
            Content = Regex.Replace(element.Content, "^<h3>(.+?)</h3>", string.Empty);
        }

    }

}