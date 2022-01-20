using Skybrud.Integrations.BorgerDk;
using Umbraco.Cms.Core.Notifications;

namespace Skybrud.Umbraco.BorgerDk.Notifications {

    /// <summary>
    /// Class representing a notification about an updated article.
    /// </summary>
    public class BorgerDkArticleUpdatedNotification : INotification {

        /// <summary>
        /// Gets a reference to the updated article.
        /// </summary>
        public BorgerDkArticle Article { get; }

        /// <summary>
        /// Initializes a new notification based on the specified <paramref name="article"/>.
        /// </summary>
        /// <param name="article"></param>
        public BorgerDkArticleUpdatedNotification(BorgerDkArticle article) {
            Article = article;
        }

    }

}