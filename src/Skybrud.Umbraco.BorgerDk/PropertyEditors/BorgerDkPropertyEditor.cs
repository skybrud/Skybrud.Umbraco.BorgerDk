using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;

namespace Skybrud.Umbraco.BorgerDk.PropertyEditors {

    [DataEditor(EditorAlias, EditorType.PropertyValue, "Skybrud Borger.dk", EditorView, ValueType = ValueTypes.Json, Group = "Skybrud.dk", Icon = EditorIcon)]
    public class BorgerDkPropertyEditor : DataEditor {

        internal const string EditorAlias = "Skybrud.BorgerDk";

        internal const string EditorIcon = "icon-school color-skybrud";

        internal const string EditorView = "/App_Plugins/Skybrud.BorgerDk/Views/Editor.html";
        
        private readonly IIOHelper _iOHelper;

        public BorgerDkPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper iOHelper) : base(dataValueEditorFactory) {
            _iOHelper = iOHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new BorgerDkConfigurationEditor(_iOHelper);

    }

}