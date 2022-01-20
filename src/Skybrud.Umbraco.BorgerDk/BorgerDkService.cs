using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NPoco;
using Skybrud.Integrations.BorgerDk;
using Skybrud.Umbraco.BorgerDk.Models;
using Skybrud.Umbraco.BorgerDk.Notifications;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Skybrud.Umbraco.BorgerDk {

    public partial class BorgerDkService {

        private readonly IScopeProvider _scopeProvider;
        private readonly ILogger<BorgerDkService> _logger;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IEventAggregator _eventAggregator;

        #region Constructors

        public BorgerDkService(IScopeProvider scopeProvider, ILogger<BorgerDkService> logger, IHostingEnvironment hostingEnvironment, IEventAggregator eventAggregator) {
            _scopeProvider = scopeProvider;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _eventAggregator = eventAggregator;
        }

        #endregion

        private BorgerDkArticleDto GetArticleDtoById(string domain, int municipality, int articleId) {

            string id = BorgerDkUtils.GetUniqueId(domain, municipality, articleId);

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
            return GetArticleDtoById(domain, municipality, articleId)?.Meta;
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

            }

            // Broadcast that the article has been updated
            _eventAggregator.Publish(new BorgerDkArticleUpdatedNotification(article));

        }

        public BorgerDkArticle[] GetAllArticles() {
            return GetAllArticlesDtos().Select(x => x.Meta).ToArray();
        }

        private List<BorgerDkArticleDto> GetAllArticlesDtos() {

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