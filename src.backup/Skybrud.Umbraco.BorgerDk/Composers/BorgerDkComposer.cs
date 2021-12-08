using Skybrud.Umbraco.BorgerDk.Caching;
using Skybrud.Umbraco.BorgerDk.Components;
using Skybrud.Umbraco.BorgerDk.Scheduling;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;

namespace Skybrud.Umbraco.BorgerDk.Composers {

    public class BorgerDkComposer : IUserComposer {

        public void Compose(Composition composition) {
            
            composition.Register<BorgerDkService>();
            composition.Register<BorgerDkImportTaskSettings>(Lifetime.Singleton);
            composition.RegisterUnique(CreateSearchIndexCacheRefresher);

            composition.Register<BorgerDkTaskRunner>(Lifetime.Singleton);
            composition.Components().Append<BorgerDkTaskRunnerComponent>();

        }

        private static BorgerDkCacheRefresher CreateSearchIndexCacheRefresher(IFactory factory) {
            var appCaches = factory.GetInstance<AppCaches>();
            return new BorgerDkCacheRefresher(appCaches);
        }

    }

}