using System;
using Skybrud.Umbraco.BorgerDk.Models;
using Umbraco.Core.Cache;

namespace Skybrud.Umbraco.BorgerDk.Caching {

    public class BorgerDkCacheRefresher : PayloadCacheRefresherBase<BorgerDkCacheRefresher, BorgerDkArticleReference> {

        public static readonly Guid UniqueId = Guid.Parse("3c51ff10-45e1-47fa-8d99-f783c0f2b753");

        private readonly IAppPolicyCache _cache;

        public BorgerDkCacheRefresher(AppCaches appCaches) : base(appCaches) {
            _cache = appCaches.RuntimeCache;
        }

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "Borger.dk cache refresher";

        protected override BorgerDkCacheRefresher This => this;

        public override void Refresh(BorgerDkArticleReference[] payloads) {
            foreach (BorgerDkArticleReference payload in payloads) {
                _cache.InsertCacheItem(payload.Id, () => payload);
            }
        }

    }

}