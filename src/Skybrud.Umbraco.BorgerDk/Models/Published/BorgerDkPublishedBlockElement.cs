using Newtonsoft.Json;
using System.Collections.Generic;

namespace Skybrud.Umbraco.BorgerDk.Models.Published {

    public class BorgerDkPublishedBlockElement : BorgerDkPublishedElement {

        [JsonProperty("microArticles")]
        public BorgerDkPublishedMicroArticle[] MicroArticles { get; }

        public BorgerDkPublishedBlockElement(List<BorgerDkPublishedMicroArticle> microArticles) {
            Id = "kernetekst";
            Title = "Kernetekst";
            MicroArticles = microArticles.ToArray();
        }

    }

}