using Newtonsoft.Json;

namespace Skybrud.Umbraco.BorgerDk.Models {

    public class BorgerDkMunicipality {

        #region Properties

        [JsonProperty("code")]
        public int Code { get; }

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("fullName")]
        public string FullName { get; }

        #endregion

        #region Constructors

        public BorgerDkMunicipality(Skybrud.BorgerDk.BorgerDkMunicipality municipality) {
            Code = municipality.Code;
            Name = municipality.Name;
            FullName = municipality.NameLong;
        }

        #endregion

        #region Static methods

        public static BorgerDkMunicipality GetFromCode(int code) {
            Skybrud.BorgerDk.BorgerDkMunicipality municipality = Skybrud.BorgerDk.BorgerDkMunicipality.GetFromCode(code);
            return municipality == null ? null : new BorgerDkMunicipality(municipality);
        }

        public static BorgerDkMunicipality GetFromCode(string code) {
            Skybrud.BorgerDk.BorgerDkMunicipality municipality = Skybrud.BorgerDk.BorgerDkMunicipality.GetFromCode(code);
            return municipality == null ? null : new BorgerDkMunicipality(municipality);
        }

        #endregion

    }

}