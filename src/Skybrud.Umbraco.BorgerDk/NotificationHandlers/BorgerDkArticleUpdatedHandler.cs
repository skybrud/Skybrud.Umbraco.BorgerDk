using Skybrud.Umbraco.BorgerDk.Caching;
using System;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;

namespace Skybrud.Umbraco.BorgerDk.Notifications.Handlers {
    
    /// <summary>
    /// When <see cref="BorgerDkService"/> updates an article, it will broadcast this via a
    /// <see cref="BorgerDkArticleUpdatedNotification"/>.
    ///
    /// The purpose of the <see cref="BorgerDkArticleUpdatedHandler"/> class is then to receive the notification, and
    /// propagate this information to the distributed cache. When doing this, Umbraco will automatically call the
    /// <see cref="BorgerDkCacheRefresher"/> for each environment.
    /// </summary>
    public class BorgerDkArticleUpdatedHandler : INotificationHandler<BorgerDkArticleUpdatedNotification> {

        private readonly DistributedCache _distributedCache;

        public BorgerDkArticleUpdatedHandler(DistributedCache distributedCache) {
            _distributedCache = distributedCache;
        } 
        
        public void Handle(BorgerDkArticleUpdatedNotification notification) {

            Console.WriteLine("BorgerDkArticleUpdatedHandler received word that article " + BorgerDkUtils.GetUniqueId(notification.Article) + " has been updated.");

            var payloads = new[] { new BorgerDkCacheRefresher.JsonPayload(notification.Article) };
            _distributedCache.RefreshByPayload(BorgerDkCacheRefresher.UniqueId, payloads);

        }

    }

}