using Skybrud.Integrations.BorgerDk;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Skybrud.Umbraco.BorgerDk.PropertyEditors {

    [DataEditor("Skybrud.BorgerDk", EditorType.PropertyValue, "Skybrud Borger.dk", "/App_Plugins/Skybrud.BorgerDk/Views/Editor.html", ValueType = ValueTypes.Json, Group = "Skybrud.dk", Icon = "icon-school")]
    public class BorgerDkPropertyEditor : DataEditor {

        public BorgerDkPropertyEditor(ILogger logger) : base(logger) { }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new BorgerDkConfigurationEditor();

    }

    public class BorgerDkConfigurationEditor : ConfigurationEditor<BorgerDkConfiguration> {

        public BorgerDkConfigurationEditor() { }

    }

    public class BorgerDkConfiguration {

        [ConfigurationField("municipality", "Municipality", "/App_Plugins/Skybrud.BorgerDk/Views/Municipality.html", Description = "Select the municipality to be used.")]
        public BorgerDkMunicipality Municipality { get; set; }

        [ConfigurationField("allowedTypes", "Allowed types", "/App_Plugins/Skybrud.BorgerDk/Views/AllowedTypes.html", Description = "Select the element types that should be allowed.")]
        public string[] AllowedTypes { get; set; }

    }

}