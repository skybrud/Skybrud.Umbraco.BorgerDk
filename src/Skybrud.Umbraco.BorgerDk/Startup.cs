using Skybrud.Umbraco.BorgerDk.Grid.Converters;
using Skybrud.Umbraco.GridData;
using Umbraco.Core;

namespace Skybrud.Umbraco.BorgerDk {

    public class Startup : ApplicationEventHandler {
    
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext) {
            GridContext.Current.Converters.Add(new BorgerDkGridConverter());
        }
    
    }
}