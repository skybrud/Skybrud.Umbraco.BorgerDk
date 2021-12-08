using Skybrud.Integrations.BorgerDk;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;

namespace Skybrud.Umbraco.BorgerDk.PropertyEditors
{

    [DataEditor("Skybrud.BorgerDk", EditorType.PropertyValue, "Skybrud Borger.dk", "/App_Plugins/Skybrud.BorgerDk/Views/Editor.html", ValueType = ValueTypes.Json, Group = "Skybrud.dk", Icon = "icon-school")]
    public class BorgerDkPropertyEditor : DataEditor {
        private readonly IOHelper _iOHelper;

        public BorgerDkPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IOHelper iOHelper) : base(dataValueEditorFactory)
        {
            _iOHelper = iOHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new BorgerDkConfigurationEditor(_iOHelper);

    }

    public class BorgerDkConfigurationEditor : ConfigurationEditor<BorgerDkConfiguration> {

        public BorgerDkConfigurationEditor(IOHelper iOHelper) : base(iOHelper) { }

    }

    public class BorgerDkConfiguration {

        [ConfigurationField("municipality", "Municipality", "/App_Plugins/Skybrud.BorgerDk/Views/Municipality.html", Description = "Select the municipality to be used.")]
        public BorgerDkMunicipality Municipality { get; set; }

        [ConfigurationField("allowedTypes", "Allowed types", "/App_Plugins/Skybrud.BorgerDk/Views/AllowedTypes.html", Description = "Select the element types that should be allowed.")]
        public string[] AllowedTypes { get; set; }

    }

}