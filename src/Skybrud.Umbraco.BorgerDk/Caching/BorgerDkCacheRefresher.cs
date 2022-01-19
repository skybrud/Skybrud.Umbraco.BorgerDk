﻿using Skybrud.Umbraco.BorgerDk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Sync;

namespace Skybrud.Umbraco.BorgerDk.Caching {

    public class BorgerDkCacheRefresher : CacheRefresherBase<BorgerDkCacheRefresherNotification> {

        public BorgerDkCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory, BorgerDkCache borgerDkCachingService) : base(appCaches, eventAggregator, factory) {
            _borgerDkCachingService = borgerDkCachingService;
        }

        public static readonly Guid _uniqueId = Guid.Parse("1F28970B-746B-451A-9B57-7A8AD394F1C8");
        private readonly BorgerDkCache _borgerDkCachingService;

        public override Guid RefresherUniqueId => _uniqueId;

        public override string Name => "BorgerDkCacheRefresher";

        public override void RefreshAll() {
            base.RefreshAll();
        }

        public void Remove(string id) {
            _borgerDkCachingService.ClearArticle(id);
            OnCacheUpdated(NotificationFactory.Create<BorgerDkCacheRefresherNotification>(id, MessageType.RemoveById));
        }

        public void Refresh(string id) {
            _borgerDkCachingService.CacheArticle(id);
            OnCacheUpdated(NotificationFactory.Create<BorgerDkCacheRefresherNotification>(id, MessageType.RefreshById));
        }

        public void Refresh(BorgerDkArticleDto dto) {
            OnCacheUpdated(NotificationFactory.Create<BorgerDkCacheRefresherNotification>(dto, MessageType.RefreshByInstance));
        }

        public void RefreshAll(List<BorgerDkArticleDto> dtos) {
            OnCacheUpdated(NotificationFactory.Create<BorgerDkCacheRefresherNotification>(dtos.Select(x => x.Id), MessageType.RefreshAll));
        }
    }

}