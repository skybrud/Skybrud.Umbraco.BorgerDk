using Skybrud.Integrations.BorgerDk;
using Umbraco.Cms.Core.PropertyEditors;

namespace Skybrud.Umbraco.BorgerDk.PropertyEditors {

    /// <summary>
    /// Class representing the configuration of a <see cref="BorgerDkPropertyEditor"/>.
    /// </summary>
    public class BorgerDkConfiguration {

        /// <summary>
        /// Gets or sets the municipality.
        /// </summary>
        [ConfigurationField("municipality", "Municipality", "/App_Plugins/Skybrud.BorgerDk/Views/Municipality.html", Description = "Select the municipality to be used.")]
        public BorgerDkMunicipality Municipality { get; set; }

        /// <summary>
        /// Gets or sets an array with the allowed types. If <c>null</c> or empty, all types are allowed.
        /// </summary>
        [ConfigurationField("allowedTypes", "Allowed types", "/App_Plugins/Skybrud.BorgerDk/Views/AllowedTypes.html", Description = "Select the element types that should be allowed.")]
        public string[] AllowedTypes { get; set; }

    }

}