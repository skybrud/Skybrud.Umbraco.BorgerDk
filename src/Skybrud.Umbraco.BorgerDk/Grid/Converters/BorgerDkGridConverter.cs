using Newtonsoft.Json.Linq;
using Skybrud.Umbraco.BorgerDk.Grid.Values;
using Skybrud.Umbraco.GridData;
using Skybrud.Umbraco.GridData.Converters;
using Skybrud.Umbraco.GridData.Interfaces;
using Skybrud.Umbraco.GridData.Rendering;

namespace Skybrud.Umbraco.BorgerDk.Grid.Converters {
    
    public class BorgerDkGridConverter : GridConverterBase {

        /// <summary>
        /// Converts the specified <paramref name="token"/> into an instance of <see cref="IGridControlValue"/>.
        /// </summary>
        /// <param name="control">The parent control.</param>
        /// <param name="token">The instance of <see cref="JToken"/> representing the control value.</param>
        /// <param name="value">The converted value.</param>
        public override bool ConvertControlValue(GridControl control, JToken token, out IGridControlValue value) {
            value = null;
            if (IsBorgerDkEditor(control)) {
                value = BorgerDkGridControlValue.Parse(control, token as JObject);
            }
            return value != null;
        }

        /// <summary>
        /// Gets an instance <see cref="GridControlWrapper"/> for the specified <paramref name="control"/>.
        /// </summary>
        /// <param name="control">The control to be wrapped.</param>
        /// <param name="wrapper">The wrapper.</param>
        public override bool GetControlWrapper(GridControl control, out GridControlWrapper wrapper) {
            wrapper = null;
            if (IsBorgerDkEditor(control)) {
                wrapper = control.GetControlWrapper<BorgerDkGridControlValue>();
            }
            return wrapper != null;
        }

        private bool IsBorgerDkEditor(GridControl control) {

            // The editor may be NULL if it no longer exists in a package.manifest file
            if (control.Editor == null) return false;

            var editor = control.Editor;

            const string alias = "Skybrud.BorgerDk";
            const string view = "/App_Plugins/Skybrud.BorgerDk/Views/BorgerDkGridEditor.html";

            return ContainsIgnoreCase(editor.View.Split('?')[0], view) || EqualsIgnoreCase(editor.Alias, alias) || ContainsIgnoreCase(editor.Alias, alias + ".");

        }

    }

}