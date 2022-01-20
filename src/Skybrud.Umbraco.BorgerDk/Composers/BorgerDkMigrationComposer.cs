using Skybrud.Umbraco.BorgerDk.NotificationHandlers;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

#pragma warning disable 1591

namespace Skybrud.Umbraco.BorgerDk.Composers {

    public class BorgerDkMigrationComposer : IComposer {
        public void Compose(IUmbracoBuilder builder) {
            builder.AddNotificationHandler<UmbracoApplicationStartingNotification, BorgerDkMigrationHandler>();
        }
    }

}