using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Skybrud.Integrations.BorgerDk;
using Skybrud.Umbraco.BorgerDk.Models;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;

namespace Skybrud.Umbraco.BorgerDk {

    public class BorgerDkService {

        private readonly IScopeProvider _scopeProvider;

        private readonly ILogger _logger;

        #region Constructors

        public BorgerDkService() {
            _scopeProvider = Current.ScopeProvider;
            _logger = Current.Logger;
        }

        public BorgerDkService(IScopeProvider scopeProvider, ILogger logger) {
            _scopeProvider = scopeProvider;
            _logger = logger;
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
                    _logger.Error<BorgerDkService>("Unable to insert redirect into the database", ex);
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
                    _logger.Error<BorgerDkService>("Unable to insert redirect into the database", ex);
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
                    _logger.Error<BorgerDkService>("Unable to fetch all articles from the database.", ex);
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
                    _logger.Error<BorgerDkService>("Unable to fetch all articles from the database.", ex);
                    throw new Exception("Unable to fetch all articles from the database.", ex);
                }

            }

        }

    }

}