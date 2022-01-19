using Skybrud.Umbraco.BorgerDk.Models;
using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Sync;

namespace Skybrud.Umbraco.BorgerDk.Caching {

    public class BorgerDkCacheRefresherNotificationHandler : INotificationHandler<BorgerDkCacheRefresherNotification> {
        private readonly BorgerDkCache _borgerDkCachingService;

        public BorgerDkCacheRefresherNotificationHandler(BorgerDkCache borgerDkCachingService) {
            _borgerDkCachingService = borgerDkCachingService;
        }

        public void Handle(BorgerDkCacheRefresherNotification notification) {
            if (notification.MessageType == MessageType.RefreshAll) {
                if (notification.MessageObject.ToString() == string.Empty) {
                    _borgerDkCachingService.RefreshCache();
                } else {
                    _borgerDkCachingService.CacheArticles(notification.MessageObject as IEnumerable<string>);
                }
            } else if (notification.MessageType == MessageType.RefreshById) {
                _borgerDkCachingService.CacheArticle(notification.MessageObject as string);
            } else if (notification.MessageType == MessageType.RemoveById) {
                _borgerDkCachingService.ClearArticle(notification.MessageObject as string);
            } else if (notification.MessageType == MessageType.RefreshByInstance) {
                _borgerDkCachingService.CacheArticle(notification.MessageObject as BorgerDkArticleDto);
            }
        }
    }
}
