using Microsoft.Extensions.Logging;
using NPoco;
using Skybrud.Integrations.BorgerDk;
using Skybrud.Umbraco.BorgerDk.Caching;
using Skybrud.Umbraco.BorgerDk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Skybrud.Umbraco.BorgerDk {

    public partial class BorgerDkService {

        private readonly IScopeProvider _scopeProvider;

        private readonly ILogger<BorgerDkService> _logger;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly BorgerDkCachingService borgerDkCachingService;
        private readonly BorgerDkCacheRefresher borgerDkCacheRefresher;

        #region Constructors

        public BorgerDkService(IScopeProvider scopeProvider, ILogger<BorgerDkService> logger, IHostingEnvironment hostingEnvironment, BorgerDkCachingService borgerDkCachingService, BorgerDkCacheRefresher borgerDkCacheRefresher) {
            _scopeProvider = scopeProvider;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            this.borgerDkCachingService = borgerDkCachingService;
            this.borgerDkCacheRefresher = borgerDkCacheRefresher;
        }

        #endregion

        private BorgerDkArticleDto GetArticleDtoById(string domain, int municipality, int articleId) {

            string id = domain + "_" + municipality + "_" + articleId;

            var cachedResult = borgerDkCachingService.GetArticle(id);
            if (cachedResult != null) return cachedResult;

            using (IScope scope = _scopeProvider.CreateScope(autoComplete: true)) {

                try {

                    // Generate the SQL for the query
                    Sql<ISqlContext> sql = scope.SqlContext.Sql()
                        .Select<BorgerDkArticleDto>()
                        .From<BorgerDkArticleDto>()
                        .Where<BorgerDkArticleDto>(x => x.Id == id);

                    var dto = scope.Database.FirstOrDefault<BorgerDkArticleDto>(sql);

                    borgerDkCacheRefresher.Refresh(dto);

                    return dto;

                } catch (Exception ex) {
                    _logger.LogError(ex, "Unable to insert redirect into the database");
                    throw new Exception("Unable to insert redirect into the database", ex);
                }

            }

        }

        public BorgerDkArticle GetArticleById(string domain, int municipality, int articleId) {

            string id = domain + "_" + municipality + "_" + articleId;

            var cachedResult = borgerDkCachingService.GetArticle(id);
            if (cachedResult != null) return cachedResult?.Meta;

            using (IScope scope = _scopeProvider.CreateScope(autoComplete: true)) {

                try {

                    // Generate the SQL for the query
                    Sql<ISqlContext> sql = scope.SqlContext.Sql()
                        .Select<BorgerDkArticleDto>()
                        .From<BorgerDkArticleDto>()
                        .Where<BorgerDkArticleDto>(x => x.Id == id);

                    // Make the call to the database
                    var dto = scope.Database.FirstOrDefault<BorgerDkArticleDto>(sql);

                    borgerDkCacheRefresher.Refresh(dto);

                    // Return the article
                    return dto?.Meta;

                } catch (Exception ex) {
                    _logger.LogError(ex, "Unable to get article from the database");
                    throw new Exception("Unable to get article from the database", ex);
                }
            }
        }

        public void Import(BorgerDkArticle article, bool useCache = false) {

            // Get the article DTO (if it already exists in the db)
            BorgerDkArticleDto dto = GetArticleDtoById(article.Domain, article.Municipality.Code, article.Id);

            if (useCache && dto != null) {
                return;
            }

            using (IScope scope = _scopeProvider.CreateScope(autoComplete: true)) {

                if (dto == null) {
                    dto = new BorgerDkArticleDto(article);
                    scope.Database.Insert(dto);
                } else {
                    dto.Meta = article;
                    dto.UpdateDate = DateTime.UtcNow;
                    scope.Database.Update(dto);
                }

                borgerDkCacheRefresher.Refresh(dto);
            }

        }

        public BorgerDkArticleModel[] GetAllArticles() {

            using (IScope scope = _scopeProvider.CreateScope(autoComplete: true)) {

                try {

                    // Generate the SQL for the query
                    Sql<ISqlContext> sql = scope.SqlContext.Sql()
                        .Select<BorgerDkArticleDto>()
                        .From<BorgerDkArticleDto>();

                    var dtos = scope.Database.Fetch<BorgerDkArticleDto>(sql);

                    borgerDkCacheRefresher.RefreshAll(dtos);

                    // Make the call to the database
                    return dtos.Select(x => new BorgerDkArticleModel(x)).ToArray();

                } catch (Exception ex) {
                    _logger.LogError(ex, "Unable to fetch all articles from the database.");
                    throw new Exception("Unable to fetch all articles from the database.", ex);
                }

            }

        }

        public BorgerDkArticleModel[] GetAllArticles(bool usecache) {

            if (usecache) {
                var cachedResult = borgerDkCachingService.GetAllArticles();
                if (cachedResult != null) return cachedResult.Select(x => new BorgerDkArticleModel(x)).ToArray();
            }

            return GetAllArticles();
        }

        public List<BorgerDkArticleDto> GetAllArticlesDtos() {

            using (IScope scope = _scopeProvider.CreateScope(autoComplete: true)) {

                try {

                    // Generate the SQL for the query
                    Sql<ISqlContext> sql = scope.SqlContext.Sql()
                        .Select<BorgerDkArticleDto>()
                        .From<BorgerDkArticleDto>();

                    var dtos = scope.Database.Fetch<BorgerDkArticleDto>(sql);

                    borgerDkCacheRefresher.RefreshAll(dtos);

                    // Make the call to the database
                    return dtos;

                } catch (Exception ex) {
                    _logger.LogError(ex, "Unable to fetch all articles from the database.");
                    throw new Exception("Unable to fetch all articles from the database.", ex);
                }

            }

        }

        public List<BorgerDkArticleDto> GetAllArticlesDtos(bool usecache) {

            if (usecache) {
                var cachedResult = borgerDkCachingService.GetAllArticles();
                if (cachedResult != null) return cachedResult.ToList();
            }

            return GetAllArticlesDtos();
        }

    }

}