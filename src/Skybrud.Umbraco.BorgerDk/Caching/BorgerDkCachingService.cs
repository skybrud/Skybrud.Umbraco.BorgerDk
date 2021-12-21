using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Skybrud.Umbraco.BorgerDk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skybrud.Umbraco.BorgerDk.Caching {
    public class BorgerDkCachingService {
        private readonly IMemoryCache cache;
        private readonly MemoryCacheEntryOptions cacheEntryOptions;
        public List<string> CurrentlyCachedIds { get; set; }

        public BorgerDkCachingService(IMemoryCache cache) {
            this.cache = cache;
            cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(1));
        }

        public void CacheArticle(BorgerDkArticleDto article) {
            cache.Set(article.Id, article, cacheEntryOptions);
            CurrentlyCachedIds.Add(article.Id);
        }

        public void CacheArticles(IEnumerable<BorgerDkArticleDto> article) {
            foreach(var item in article) {
                CacheArticle(item);
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
