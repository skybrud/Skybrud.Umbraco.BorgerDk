using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;

#pragma warning disable 1591

namespace Skybrud.Umbraco.BorgerDk.PropertyEditors {

    public class BorgerDkConfigurationEditor : ConfigurationEditor<BorgerDkConfiguration> {

        public BorgerDkConfigurationEditor(IIOHelper iOHelper) : base(iOHelper) { }

    }

}