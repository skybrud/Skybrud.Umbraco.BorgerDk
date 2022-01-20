using Newtonsoft.Json;
using Skybrud.Essentials.Json.Converters.Enums;

#pragma warning disable 1591

namespace Skybrud.Umbraco.BorgerDk.Models.Import {

    [JsonConverter(typeof(EnumStringConverter))]
    public enum ImportAction {
        None,
        NotModified,
        Added,
        Updated,
        Deleted,
        Rejected
    }

}