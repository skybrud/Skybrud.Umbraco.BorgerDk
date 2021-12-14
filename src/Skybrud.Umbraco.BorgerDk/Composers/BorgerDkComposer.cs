using Microsoft.Extensions.DependencyInjection;
using Skybrud.Umbraco.BorgerDk.Caching;
using Skybrud.Umbraco.BorgerDk.Scheduling;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Skybrud.Umbraco.BorgerDk.Composers
{

    public class BorgerDkComposer : IComposer {

        //public void Compose(Composition composition) {

        //    composition.RegisterUnique(CreateSearchIndexCacheRefresher);


        //}

        //private static BorgerDkCacheRefresher CreateSearchIndexCacheRefresher(IFactory factory) {
        //    var appCaches = factory.GetInstance<AppCaches>();
        //    return new BorgerDkCacheRefresher(appCaches);
        //}

        public void Compose(IUmbracoBuilder builder) {
            builder.Services.AddTransient<BorgerDkService>();
            builder.Services.AddSingleton<BorgerDkImportTaskSettings>();
            builder.Services.AddHostedService<BorgerDkImportTask>();
        }
    }

}