using Newtonsoft.Json;

namespace Skybrud.Umbraco.BorgerDk.Models.Published {

    public class BorgerDkPublishedElement {

        [JsonProperty("id", Order = -99)]
        public string Id { get; protected set; }

        [JsonProperty("title", Order = -98)]
        public string Title { get; protected set; }

    }

}