using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Umbraco.BorgerDk.Models;
using Skybrud.Umbraco.GridData;
using Skybrud.Umbraco.GridData.Interfaces;

namespace Skybrud.Umbraco.BorgerDk.Grid.Values {

    public class BorgerDkGridControlValue : BorgerDkArticleSelection, IGridControlValue {

        #region Properties

        [JsonIgnore]
        public GridControl Control { get; private set; }

        #endregion

        #region Constructors

        protected BorgerDkGridControlValue(GridControl control, JObject obj) : base(obj) {
            Control = control;
        }

        #endregion

        #region Static methods

        /// <summary>
        /// Gets a media value from the specified <see cref="JObject"/>.
        /// </summary>
        /// <param name="control">The parent control.</param>
        /// <param name="obj">The instance of <see cref="JObject"/> to be parsed.</param>
        public static BorgerDkGridControlValue Parse(GridControl control, JObject obj) {
            return obj == null ? null : new BorgerDkGridControlValue(control, obj);
        }

        #endregion

    }

}