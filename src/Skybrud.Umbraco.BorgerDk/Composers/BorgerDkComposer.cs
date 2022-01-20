using Microsoft.Extensions.DependencyInjection;
using Skybrud.Umbraco.BorgerDk.Caching;
using Skybrud.Umbraco.BorgerDk.NotificationHandlers;
using Skybrud.Umbraco.BorgerDk.Notifications;
using Skybrud.Umbraco.BorgerDk.Scheduling;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Skybrud.Umbraco.BorgerDk.Composers {

    public class BorgerDkComposer : IComposer {

        public void Compose(IUmbracoBuilder builder) {

            // Register services
            builder.Services
                .AddTransient<BorgerDkService>()
                .AddSingleton<BorgerDkCache>()
                .AddSingleton<BorgerDkImportTaskSettings>()
                .AddHostedService<BorgerDkImportTask>();

            // Register cache refresher
            builder.CacheRefreshers()
                .Add<BorgerDkCacheRefresher>();

            // Register notifications
            builder
                .AddNotificationHandler<BorgerDkArticleUpdatedNotification, BorgerDkArticleUpdatedHandler>();

        }

    }

}