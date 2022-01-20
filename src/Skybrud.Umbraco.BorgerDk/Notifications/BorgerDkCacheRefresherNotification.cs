using Skybrud.Umbraco.BorgerDk.Caching;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

#pragma warning disable 1591

namespace Skybrud.Umbraco.BorgerDk.Notifications {

    /// <summary>
    /// Class representing a notification used by the <see cref="BorgerDkCacheRefresher"/> class.
    /// </summary>
    public class BorgerDkCacheRefresherNotification : CacheRefresherNotification {

        public BorgerDkCacheRefresherNotification(object messageObject, MessageType messageType) : base(messageObject, messageType) { }

    }

}