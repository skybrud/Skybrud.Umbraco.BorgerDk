using Newtonsoft.Json;
using Skybrud.Essentials.Json.Newtonsoft.Converters.Enums;

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