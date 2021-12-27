using Skybrud.Umbraco.BorgerDk.Models;
using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Sync;

namespace Skybrud.Umbraco.BorgerDk.Caching {

    public class BorgerDkCacheRefresherNotificationHandler : INotificationHandler<BorgerDkCacheRefresherNotification> {
        private readonly BorgerDkCachingService borgerDkCachingService;

        public BorgerDkCacheRefresherNotificationHandler(BorgerDkCachingService borgerDkCachingService) {
            this.borgerDkCachingService = borgerDkCachingService;
        }

        public void Handle(BorgerDkCacheRefresherNotification notification) {
            if (notification.MessageType == MessageType.RefreshAll) {
                if (notification.MessageObject.ToString() == string.Empty) {
                    borgerDkCachingService.RefreshCache();
                } else {
                    borgerDkCachingService.CacheArticles(notification.MessageObject as IEnumerable<string>);
                }
            } else if (notification.MessageType == MessageType.RefreshById) {
                borgerDkCachingService.CacheArticle(notification.MessageObject as string);
            } else if (notification.MessageType == MessageType.RemoveById) {
                borgerDkCachingService.ClearArticle(notification.MessageObject as string);
            } else if (notification.MessageType == MessageType.RefreshByInstance) {
                borgerDkCachingService.CacheArticle(notification.MessageObject as BorgerDkArticleDto);
            }
        }
    }
}
