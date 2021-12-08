using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NPoco;
using Skybrud.Integrations.BorgerDk;
using Skybrud.Umbraco.BorgerDk.Models;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Skybrud.Umbraco.BorgerDk {

    public partial class BorgerDkService {

        private readonly IScopeProvider _scopeProvider;

        private readonly ILogger<BorgerDkService> _logger;
        private readonly IIOHelper _iOHelper;

        #region Constructors

        public BorgerDkService(IScopeProvider scopeProvider, ILogger<BorgerDkService> logger, IIOHelper iOHelper) {
            _scopeProvider = scopeProvider;
            _logger = logger;
            _iOHelper = iOHelper;
        }

        #endregion

        private BorgerDkArticleDto GetArticleDtoById(string domain, int municipality, int articleId) {

            string id = domain + "_" + municipality + "_" + articleId;

            using (IScope scope = _scopeProvider.CreateScope(autoComplete: true)) {

                try {

                    // Generate the SQL for the query
                    Sql<ISqlContext> sql = scope.SqlContext.Sql()
                        .Select<BorgerDkArticleDto>()
                        .From<BorgerDkArticleDto>()
                        .Where<BorgerDkArticleDto>(x => x.Id == id);

                    // Make the call to the database
                    return scope.Database.FirstOrDefault<BorgerDkArticleDto>(sql);

                } catch (Exception ex) {
                    _logger.LogError(ex, "Unable to insert redirect into the database");
                    throw new Exception("Unable to insert redirect into the database", ex);
                }

            }

        }

        public BorgerDkArticle GetArticleById(string domain, int municipality, int articleId) {

            string id = domain + "_" + municipality + "_" + articleId;

            using (IScope scope = _scopeProvider.CreateScope(autoComplete: true)) {

                try {

                    // Generate the SQL for the query
                    Sql<ISqlContext> sql = scope.SqlContext.Sql()
                        .Select<BorgerDkArticleDto>()
                        .From<BorgerDkArticleDto>()
                        .Where<BorgerDkArticleDto>(x => x.Id == id);

                    // Make the call to the database
                    var dto = scope.Database.FirstOrDefault<BorgerDkArticleDto>(sql);

                    // Return the article
                    return dto?.Meta;

                } catch (Exception ex) {
                    _logger.LogError(ex, "Unable to insert redirect into the database");
                    throw new Exception("Unable to insert redirect into the database", ex);
                }

            }

        }

        public void Import(BorgerDkArticle article) {

            // Get the article DTO (if it already exists in the db)
            BorgerDkArticleDto dto = GetArticleDtoById(article.Domain, article.Municipality.Code, article.Id);

            using (IScope scope = _scopeProvider.CreateScope(autoComplete: true)) {

                if (dto == null) {
                    dto = new BorgerDkArticleDto(article);
                    scope.Database.Insert(dto);
                } else {
                    dto.Meta = article;
                    dto.UpdateDate = DateTime.UtcNow;
                    scope.Database.Update(dto);
                }
                
                // TODO: notify other servers about the change

            }

        }

        public BorgerDkArticleModel[] GetAllArticles() {

            using (IScope scope = _scopeProvider.CreateScope(autoComplete: true)) {

                try {

                    // Generate the SQL for the query
                    Sql<ISqlContext> sql = scope.SqlContext.Sql()
                        .Select<BorgerDkArticleDto>()
                        .From<BorgerDkArticleDto>();

                    // Make the call to the database
                    return scope.Database.Fetch<BorgerDkArticleDto>(sql).Select(x => new BorgerDkArticleModel(x)).ToArray();

                } catch (Exception ex) {
                    _logger.LogError(ex, "Unable to fetch all articles from the database.");
                    throw new Exception("Unable to fetch all articles from the database.", ex);
                }

            }

        }

        public List<BorgerDkArticleDto> GetAllArticlesDtos() {

            using (IScope scope = _scopeProvider.CreateScope(autoComplete: true)) {

                try {

                    // Generate the SQL for the query
                    Sql<ISqlContext> sql = scope.SqlContext.Sql()
                        .Select<BorgerDkArticleDto>()
                        .From<BorgerDkArticleDto>();

                    // Make the call to the database
                    return scope.Database.Fetch<BorgerDkArticleDto>(sql);

                } catch (Exception ex) {
                    _logger.LogError(ex, "Unable to fetch all articles from the database.");
                    throw new Exception("Unable to fetch all articles from the database.", ex);
                }

            }

        }

    }

}