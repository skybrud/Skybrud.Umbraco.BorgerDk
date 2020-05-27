using Skybrud.Umbraco.BorgerDk.Caching;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;

namespace Skybrud.Umbraco.BorgerDk.Composers {

    public class BorgerDkComposer : IUserComposer {

        public void Compose(Composition composition) {

            composition.Register<BorgerDkService>();

            composition.RegisterUnique(CreateSearchIndexCacheRefresher);
        }

        private static BorgerDkCacheRefresher CreateSearchIndexCacheRefresher(IFactory factory) {
            var appCaches = factory.GetInstance<AppCaches>();
            return new BorgerDkCacheRefresher(appCaches);
        }

    }

}