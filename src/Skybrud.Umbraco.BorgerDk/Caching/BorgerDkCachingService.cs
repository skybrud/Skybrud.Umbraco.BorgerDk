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
    public class BorgerDkCachingService {
        private readonly IMemoryCache cache;
        private readonly IScopeProvider scopeProvider;
        private readonly ILogger<BorgerDkCachingService> logger;
        private readonly MemoryCacheEntryOptions cacheEntryOptions;
        public List<string> CurrentlyCachedIds { get; set; } = new List<string>();

        public BorgerDkCachingService(IMemoryCache cache, IScopeProvider scopeProvider, ILogger<BorgerDkCachingService> logger) {
            this.cache = cache;
            this.scopeProvider = scopeProvider;
            this.logger = logger;
            cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(1));

            RefreshCache();
        }

        public void CacheArticle(BorgerDkArticleDto article) {
            cache.Set(article.Id, article, cacheEntryOptions);
            CurrentlyCachedIds.Add(article.Id);
        }

        public void CacheArticle(string id) {

            using (IScope scope = scopeProvider.CreateScope(autoComplete: true)) {
                try {
                    Sql<ISqlContext> sql = scope.SqlContext.Sql()
                        .Select<BorgerDkArticleDto>()
                        .From<BorgerDkArticleDto>()
                        .Where<BorgerDkArticleDto>(x => x.Id == id);

                    var article = scope.Database.FirstOrDefault<BorgerDkArticleDto>(sql);

                    cache.Set(article.Id, article, cacheEntryOptions);
                    CurrentlyCachedIds.Add(article.Id);

                } catch (Exception ex) {
                    logger.LogError(ex, "Failed to load article with id: " + id);
                }
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
            using (IScope scope = scopeProvider.CreateScope(autoComplete: true)) {

                try {

                    // Generate the SQL for the query
                    Sql<ISqlContext> sql = scope.SqlContext.Sql()
                        .Select<BorgerDkArticleDto>()
                        .From<BorgerDkArticleDto>();

                    var dtos = scope.Database.Fetch<BorgerDkArticleDto>(sql);

                    CacheArticles(dtos);

                } catch (Exception ex) {
                    logger.LogError(ex, "Unable to fetch all articles from the database.");
                    throw new Exception("Unable to fetch all articles from the database.", ex);
                }

            }
        }

        public void ClearArticle(string articleId) {
            cache.Remove(articleId);
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
            return cache.Get<BorgerDkArticleDto>(articleId);
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
