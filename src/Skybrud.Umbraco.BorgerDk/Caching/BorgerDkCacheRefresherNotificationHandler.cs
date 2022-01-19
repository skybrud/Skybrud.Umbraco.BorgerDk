using Skybrud.Umbraco.BorgerDk.Models;
using System.Collections.Generic;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Sync;

namespace Skybrud.Umbraco.BorgerDk.Caching {

    public class BorgerDkCacheRefresherNotificationHandler : INotificationHandler<BorgerDkCacheRefresherNotification> {
        private readonly BorgerDkCache _borgerDkCache;

        public BorgerDkCacheRefresherNotificationHandler(BorgerDkCache borgerDkCache) {
            _borgerDkCache = borgerDkCache;
        }

        public void Handle(BorgerDkCacheRefresherNotification notification) {
            if (notification.MessageType == MessageType.RefreshAll) {
                if (notification.MessageObject.ToString() == string.Empty) {
                    _borgerDkCache.RefreshCache();
                } else {
                    _borgerDkCache.CacheArticles(notification.MessageObject as IEnumerable<string>);
                }
            } else if (notification.MessageType == MessageType.RefreshById) {
                _borgerDkCache.CacheArticle(notification.MessageObject as string);
            } else if (notification.MessageType == MessageType.RemoveById) {
                _borgerDkCache.ClearArticle(notification.MessageObject as string);
            } else if (notification.MessageType == MessageType.RefreshByInstance) {
                _borgerDkCache.CacheArticle(notification.MessageObject as BorgerDkArticleDto);
            }
        }
    }
}
