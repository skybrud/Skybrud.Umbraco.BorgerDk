using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;

namespace Skybrud.Umbraco.BorgerDk.PropertyEditors {

    [DataEditor(_editorAlias, EditorType.PropertyValue, "Skybrud Borger.dk", _editorView, ValueType = ValueTypes.Json, Group = "Skybrud.dk", Icon = _editorIcon)]
    public class BorgerDkPropertyEditor : DataEditor {

        internal const string _editorAlias = "Skybrud.BorgerDk";

        internal const string _editorIcon = "icon-school color-skybrud";

        internal const string _editorView = "/App_Plugins/Skybrud.BorgerDk/Views/Editor.html";

        private readonly IIOHelper _iOHelper;

        public BorgerDkPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper iOHelper) : base(dataValueEditorFactory) {
            _iOHelper = iOHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() {
            return new BorgerDkConfigurationEditor(_iOHelper);
        }
    }

}