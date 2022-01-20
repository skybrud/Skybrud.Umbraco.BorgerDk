using System;
using Newtonsoft.Json.Linq;
using Skybrud.Integrations.BorgerDk;
using Skybrud.Umbraco.BorgerDk.Notifications;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;

#pragma warning disable 1591

namespace Skybrud.Umbraco.BorgerDk.Caching {

    /// <summary>
    /// Cache refresher for <see cref="BorgerDkCache"/>.
    /// </summary>
    public class BorgerDkCacheRefresher : PayloadCacheRefresherBase<BorgerDkCacheRefresherNotification, BorgerDkCacheRefresher.JsonPayload> {

        private readonly BorgerDkService _borgerDkService;
        private readonly BorgerDkCache _borgerDkCache;

        #region Properties

        /// <summary>
        /// Gets the unique ID associated with this cache refresher.
        /// </summary>
        public static readonly Guid UniqueId = Guid.Parse("1F28970B-746B-451A-9B57-7A8AD394F1C8");

        /// <summary>
        /// Gets the unique ID associated with this cache refresher.
        /// </summary>
        public override Guid RefresherUniqueId => UniqueId;

        /// <summary>
        /// gets the name of this cache refresher.
        /// </summary>
        public override string Name => "BorgerDkCacheRefresher";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance based on the specified DI dependencies.
        /// </summary>
        public BorgerDkCacheRefresher(AppCaches appCaches,
            IJsonSerializer jsonSerializer,
            IEventAggregator eventAggregator,
            ICacheRefresherNotificationFactory factory,
            BorgerDkService borgerDkService,
            BorgerDkCache borgerDkCache) : base(appCaches, jsonSerializer, eventAggregator, factory) {
            _borgerDkService = borgerDkService;
            _borgerDkCache = borgerDkCache;
        }

        #endregion

        #region Member methods

        /// <inheritdoc/>
        public override void RefreshAll() {
            Console.WriteLine("RefreshAll()");
            _borgerDkCache.RefreshAll();
        }
        
        /// <inheritdoc/>
        public override void Refresh(string json) {

            Console.WriteLine("Refresh(string)");

            var payloads = Deserialize(json);

            foreach (JsonPayload payload in payloads) {

                string uniqueId = BorgerDkUtils.GetUniqueId(payload.Domain, payload.Municipality, payload.ArticleId);

                Console.WriteLine($"BorgerDkCacheRefresher received payload that {uniqueId} was updated.");

                BorgerDkArticle article = _borgerDkService.GetArticleById(payload.Domain, payload.Municipality, payload.ArticleId);

                if (article == null) {
                    Console.WriteLine(" - Article not found in database");
                } else {
                    Console.WriteLine(" - Article found in the database.");
                    _borgerDkCache.AddOrUpdate(article);
                }

            }

        }
        
        /// <inheritdoc/>
        public override void Refresh(JsonPayload[] payloads) {

            Console.WriteLine("Refresh(payloads)");

            foreach (JsonPayload payload in payloads) {

                Console.WriteLine(payload.GetType());
                Console.WriteLine(JObject.FromObject(payload));

                string uniqueId = BorgerDkUtils.GetUniqueId(payload.Domain, payload.Municipality, payload.ArticleId);

                Console.WriteLine($"BorgerDkCacheRefresher received payload that {uniqueId} was updated.");

                BorgerDkArticle article = _borgerDkService.GetArticleById(payload.Domain, payload.Municipality, payload.ArticleId);

                if (article == null) {
                    Console.WriteLine(" - Article not found in database");
                } else {
                    Console.WriteLine(" - Article found in the database.");
                    _borgerDkCache.AddOrUpdate(article);
                }

            }

        }

        #endregion

        public class JsonPayload {

            public string Domain { get; }

            public int Municipality { get; }

            public int ArticleId { get; }

            public JsonPayload(string domain, int municipality, int articleId) {
                Domain = domain;
                Municipality = municipality;
                ArticleId = articleId;
            }

            public JsonPayload(BorgerDkArticle article) {
                Domain = article.Domain;
                Municipality = article.Municipality.Code;
                ArticleId = article.Id;
            }

        }

    }

}