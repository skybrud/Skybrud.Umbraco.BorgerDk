using Newtonsoft.Json;

namespace Skybrud.Umbraco.BorgerDk.Models.Import {
    
    public class ImportJob : ImportTask {

        [JsonProperty("type", Order = -999)]
        public string Type => "Job";

    }

}