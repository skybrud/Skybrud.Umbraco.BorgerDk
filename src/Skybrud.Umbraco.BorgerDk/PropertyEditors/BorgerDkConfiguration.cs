using Skybrud.Integrations.BorgerDk;
using Umbraco.Cms.Core.PropertyEditors;

namespace Skybrud.Umbraco.BorgerDk.PropertyEditors {
    
    public class BorgerDkConfiguration {

        [ConfigurationField("municipality", "Municipality", "/App_Plugins/Skybrud.BorgerDk/Views/Municipality.html", Description = "Select the municipality to be used.")]
        public BorgerDkMunicipality Municipality { get; set; }

        [ConfigurationField("allowedTypes", "Allowed types", "/App_Plugins/Skybrud.BorgerDk/Views/AllowedTypes.html", Description = "Select the element types that should be allowed.")]
        public string[] AllowedTypes { get; set; }

    }

}