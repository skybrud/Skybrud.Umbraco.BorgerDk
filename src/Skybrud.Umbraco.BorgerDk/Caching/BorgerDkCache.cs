using System;
using System.Collections.Generic;
using System.Linq;
using Skybrud.Integrations.BorgerDk;

namespace Skybrud.Umbraco.BorgerDk.Caching {

    /// <summary>
    /// Class representing the cache layer on top of <see cref="BorgerDkService"/>.
    /// </summary>
    public class BorgerDkCache {

        private readonly BorgerDkService _borgerDkService;

        private Dictionary<string, BorgerDkArticle> _articles;

        #region Constructors

        public BorgerDkCache(BorgerDkService borgerDkService) {
            _borgerDkService = borgerDkService;
            RefreshAll();
        }

        #endregion

        #region Member methods

        public void RefreshAll() {
            _articles = _borgerDkService
                .GetAllArticles()
                .ToDictionary(BorgerDkUtils.GetUniqueId);
        }

        public BorgerDkArticle GetArticleById(string domain, int municipality, int articleId) {
            return TryGetArticle(domain, municipality, articleId, out BorgerDkArticle article) ? article : null;
        }

        public bool TryGetArticle(string domain, int municipality, int articleId, out BorgerDkArticle article) {
            string uniqueId = BorgerDkUtils.GetUniqueId(domain, municipality, articleId);
            return _articles.TryGetValue(uniqueId, out article);
        }

        public void AddOrUpdate(BorgerDkArticle article) {
            if (article == null) throw new ArgumentNullException(nameof(article));
            string uniqueId = BorgerDkUtils.GetUniqueId(article);
            _articles[uniqueId] = article;
            Console.WriteLine(" - Successfully updated article " + article.Id + " in the cache.");
        }

        #endregion

    }

}