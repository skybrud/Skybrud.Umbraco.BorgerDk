using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;

namespace Skybrud.Umbraco.BorgerDk.PropertyEditors {

    [DataEditor("Skybrud.BorgerDk", EditorType.PropertyValue, "Skybrud Borger.dk", "/App_Plugins/Skybrud.BorgerDk/Views/Editor.html", ValueType = ValueTypes.Json, Group = "Skybrud.dk", Icon = "icon-school")]
    public class BorgerDkPropertyEditor : DataEditor {
        
        private readonly IIOHelper _iOHelper;

        public BorgerDkPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper iOHelper) : base(dataValueEditorFactory) {
            _iOHelper = iOHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new BorgerDkConfigurationEditor(_iOHelper);

    }

}