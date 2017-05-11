using Newtonsoft.Json.Linq;
using Skybrud.Umbraco.BorgerDk.Grid.Values;
using Skybrud.Umbraco.GridData;
using Skybrud.Umbraco.GridData.Interfaces;
using Skybrud.Umbraco.GridData.Rendering;

namespace Skybrud.Umbraco.BorgerDk.Grid.Converters {
    
    public class BorgerDkGridConverter : IGridConverter {

        /// <summary>
        /// Converts the specified <code>token</code> into an instance of <see cref="IGridControlValue"/>.
        /// </summary>
        /// <param name="control">The parent control.</param>
        /// <param name="token">The instance of <see cref="JToken"/> representing the control value.</param>
        /// <param name="value">The converted value.</param>
        public bool ConvertControlValue(GridControl control, JToken token, out IGridControlValue value) {
            value = null;
            if (control.Editor.Alias.EndsWith(".borgerdk") || control.Editor.Alias.Contains(".borgerdk.")) {
                value = BorgerDkGridControlValue.Parse(control, token as JObject);
            }
            return value != null;
        }

        /// <summary>
        /// Converts the specified <code>token</code> into an instance of <see cref="IGridEditorConfig"/>.
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="token">The instance of <see cref="JToken"/> representing the editor config.</param>
        /// <param name="config">The converted config.</param>
        public bool ConvertEditorConfig(GridEditor editor, JToken token, out IGridEditorConfig config) {
            config = null;
            return false;
        }

        /// <summary>
        /// Gets an instance <see cref="GridControlWrapper"/> for the specified <code>control</code>.
        /// </summary>
        /// <param name="control">The control to be wrapped.</param>
        /// <param name="wrapper">The wrapper.</param>
        public bool GetControlWrapper(GridControl control, out GridControlWrapper wrapper) {
            wrapper = null;
            if (control.Editor.Alias.EndsWith(".borgerdk") || control.Editor.Alias.Contains(".borgerdk.")) {
                wrapper = control.GetControlWrapper<BorgerDkGridControlValue>();
            }
            return wrapper != null;
        }
    
    }

}