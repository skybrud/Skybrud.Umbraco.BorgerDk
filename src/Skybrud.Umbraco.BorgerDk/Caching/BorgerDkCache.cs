using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NPoco;
using Skybrud.Umbraco.BorgerDk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Skybrud.Umbraco.BorgerDk.Caching {
    public class BorgerDkCache {
        private readonly IMemoryCache _cache;
        private readonly IScopeProvider _scopeProvider;
        private readonly ILogger<BorgerDkCache> _logger;
        private readonly MemoryCacheEntryOptions _cacheEntryOptions;
        public List<string> CurrentlyCachedIds { get; set; } = new List<string>();

        public BorgerDkCache(IMemoryCache cache, IScopeProvider scopeProvider, ILogger<BorgerDkCache> logger) {
            _cache = cache;
            _scopeProvider = scopeProvider;
            _logger = logger;
            _cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(1));

            RefreshCache();
        }

        public void CacheArticle(BorgerDkArticleDto article) {
            _cache.Set(article.Id, article, _cacheEntryOptions);
            CurrentlyCachedIds.Add(article.Id);
        }

        public void CacheArticle(string id) {

            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
            try {
                Sql<ISqlContext> sql = scope.SqlContext.Sql()
                    .Select<BorgerDkArticleDto>()
                    .From<BorgerDkArticleDto>()
                    .Where<BorgerDkArticleDto>(x => x.Id == id);

                var article = scope.Database.FirstOrDefault<BorgerDkArticleDto>(sql);

                _cache.Set(article.Id, article, _cacheEntryOptions);
                CurrentlyCachedIds.Add(article.Id);

            } catch (Exception ex) {
                _logger.LogError(ex, "Failed to load article with id: " + id);
            }
        }

        public void CacheArticles(IEnumerable<BorgerDkArticleDto> article) {
            foreach (var item in article) {
                CacheArticle(item);
            }
        }

        public void CacheArticles(IEnumerable<string> article) {
            foreach (var item in article) {
                CacheArticle(item);
            }
        }

        public void RefreshCache() {
            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
            try {

                // Generate the SQL for the query
                Sql<ISqlContext> sql = scope.SqlContext.Sql()
                    .Select<BorgerDkArticleDto>()
                    .From<BorgerDkArticleDto>();

                var dtos = scope.Database.Fetch<BorgerDkArticleDto>(sql);

                CacheArticles(dtos);

            } catch (Exception ex) {
                _logger.LogError(ex, "Unable to fetch all articles from the database.");
                throw new Exception("Unable to fetch all articles from the database.", ex);
            }
        }

        public void ClearArticle(string articleId) {
            _cache.Remove(articleId);
            CurrentlyCachedIds.Remove(articleId);
        }

        public void ClearArticles(IEnumerable<string> articleIds) {
            foreach (var item in articleIds) {
                ClearArticle(item);
            }
        }

        public void ClearAllArticles() {
            ClearArticles(CurrentlyCachedIds);
        }

        public BorgerDkArticleDto GetArticle(string articleId) {
            return _cache.Get<BorgerDkArticleDto>(articleId);
        }

        public IEnumerable<BorgerDkArticleDto> GetArticles(IEnumerable<string> articleId) {
            var list = new List<BorgerDkArticleDto>();
            foreach (var item in articleId) {
                list.Add(GetArticle(item));
            }
            return list;
        }

        public IEnumerable<BorgerDkArticleDto> GetAllArticles() {
            return GetArticles(CurrentlyCachedIds);
        }
    }
}
