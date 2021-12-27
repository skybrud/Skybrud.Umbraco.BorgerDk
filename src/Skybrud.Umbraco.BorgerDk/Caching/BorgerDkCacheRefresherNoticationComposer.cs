using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Skybrud.Umbraco.BorgerDk.Caching {
    public class BorgerDkCacheRefresherNoticationComposer : IComposer {
        public void Compose(IUmbracoBuilder builder) {
            builder.AddNotificationHandler<BorgerDkCacheRefresherNotification, BorgerDkCacheRefresherNotificationHandler>();
        }
    }
}
