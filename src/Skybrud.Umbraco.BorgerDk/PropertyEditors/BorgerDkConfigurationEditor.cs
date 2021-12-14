using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;

namespace Skybrud.Umbraco.BorgerDk.PropertyEditors {
    
    public class BorgerDkConfigurationEditor : ConfigurationEditor<BorgerDkConfiguration> {

        public BorgerDkConfigurationEditor(IIOHelper iOHelper) : base(iOHelper) { }

    }

}