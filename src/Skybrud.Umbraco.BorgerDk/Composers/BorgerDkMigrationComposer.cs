using Skybrud.Umbraco.BorgerDk.Notifications.Handlers;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Skybrud.Umbraco.BorgerDk.Composers {

    public class BorgerDkMigrationComposer : IComposer {
        public void Compose(IUmbracoBuilder builder) {
            builder.AddNotificationHandler<UmbracoApplicationStartingNotification, BorgerDkMigrationHandler>();
        }
    }

}