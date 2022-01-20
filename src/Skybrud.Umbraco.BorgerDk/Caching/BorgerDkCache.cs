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

        /// <summary>
        /// Initializes a new instanced based on the specified DI depenencies.
        /// </summary>
        public BorgerDkCache(BorgerDkService borgerDkService) {
            _borgerDkService = borgerDkService;
            RefreshAll();
        }

        #endregion

        #region Member methods

        /// <summary>
        /// Rebuilds the internal dictionary of articles.
        /// </summary>
        public void RefreshAll() {
            _articles = _borgerDkService
                .GetAllArticles()
                .ToDictionary(BorgerDkUtils.GetUniqueId);
        }

        /// <summary>
        /// Returns the article matching the specified <paramref name="domain"/>, <paramref name="municipality"/> and
        /// <paramref name="articleId"/>, or <c>null</c> if not found.
        /// </summary>
        /// <param name="domain">The domain of the article.</param>
        /// <param name="municipality">The municipality of the article.</param>
        /// <param name="articleId">The ID of the article.</param>
        /// <returns>An instance of <see cref="BorgerDkArticle"/>, or <c>null</c> if not found.</returns>
        public BorgerDkArticle GetArticleById(string domain, int municipality, int articleId) {
            return TryGetArticle(domain, municipality, articleId, out BorgerDkArticle article) ? article : null;
        }

        /// <summary>
        /// Gets the article associated with the specified <paramref name="domain"/>, <paramref name="municipality"/> and
        /// <paramref name="articleId"/>.
        /// </summary>
        /// <param name="domain">The domain of the article.</param>
        /// <param name="municipality">The municipality of the article.</param>
        /// <param name="articleId">The ID of the article.</param>
        /// <param name="article">When this method returns, contains the article associated with the specified parameters, if the key is found; otherwise <c>null</c>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the cache contains an article matching the specified parameters; otherwise, <c>false</c>.</returns>
        public bool TryGetArticle(string domain, int municipality, int articleId, out BorgerDkArticle article) {
            string uniqueId = BorgerDkUtils.GetUniqueId(domain, municipality, articleId);
            return _articles.TryGetValue(uniqueId, out article);
        }

        /// <summary>
        /// Adds or updates the specified <paramref name="article"/> in the cache.
        /// </summary>
        /// <param name="article">The article.</param>
        public void AddOrUpdate(BorgerDkArticle article) {
            if (article == null) throw new ArgumentNullException(nameof(article));
            string uniqueId = BorgerDkUtils.GetUniqueId(article);
            _articles[uniqueId] = article;
            Console.WriteLine(" - Successfully updated article " + article.Id + " in the cache.");
        }

        #endregion

    }

}