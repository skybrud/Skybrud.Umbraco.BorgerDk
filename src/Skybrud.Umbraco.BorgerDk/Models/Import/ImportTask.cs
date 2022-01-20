using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Skybrud.Essentials.Json.Converters.Time;

namespace Skybrud.Umbraco.BorgerDk.Models.Import {

    public class ImportTask {

        [JsonIgnore]
        public ImportTask Parent { get; protected set; }

        [JsonIgnore]
        internal Stopwatch Stopwatch;

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("duration")]
        [JsonConverter(typeof(TimeSpanSecondsConverter))]
        public TimeSpan? Duration { get; set; }

        [JsonProperty("exception")]
        public Exception Exception { get; set; }

        [JsonProperty("status")]
        public ImportStatus Status { get; set; }

        [JsonProperty("action")]
        public ImportAction Action { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("items", Order = 998)]
        public List<ImportTask> Items = new();

        public ImportTask AddTask(string name) {
            var item = new ImportTask { Name = name, Parent = this };
            Items.Add(item);
            return item;
        }

        public bool ShouldSerializeAction() {
            return Action != ImportAction.None;
        }

        public bool ShouldSerializeMessage() {
            return !string.IsNullOrWhiteSpace(Message);
        }

    }

}