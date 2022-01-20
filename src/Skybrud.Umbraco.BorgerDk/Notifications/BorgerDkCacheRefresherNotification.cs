using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace Skybrud.Umbraco.BorgerDk.Notifications {

    public class BorgerDkCacheRefresherNotification : CacheRefresherNotification {

        public BorgerDkCacheRefresherNotification(object messageObject, MessageType messageType) : base(messageObject, messageType) { }

    }

}