using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Skybrud.Essentials.Strings.Extensions;
using Skybrud.Integrations.BorgerDk;
using Skybrud.Umbraco.BorgerDk.Models;
using Skybrud.Umbraco.BorgerDk.Models.Import;
using Umbraco.Cms.Core;

namespace Skybrud.Umbraco.BorgerDk {

    public partial class BorgerDkService {

        public ImportJob Import() {

            ImportJob job = new() { Name = "Importing articles from Borger.dk web service" };

            job.Start();

            if (!FetchArticleList(job, out Dictionary<string, BorgerDkArticleDescription> fromApi)) return job;
            if (!FetchArticlesFromDatabase(job, out List<BorgerDkArticleDto> fromDb)) return job;

            SynchronizeArticles(job, fromApi, fromDb);

            if (job.Status == ImportStatus.Pending) {
                job.Completed();
            }

            return job;

        }

        public void WriteToLog(ImportJob job) {

            string path = Path.Combine(Constants.SystemDirectories.LogFiles, "borgerdk", $"{DateTime.UtcNow:yyyyMMddHHmmss}.txt");

            string fullPath = _hostingEnvironment.MapPathContentRoot(path);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.AppendAllText(fullPath, JsonConvert.SerializeObject(job), Encoding.UTF8);

        }

        private bool FetchArticleList(ImportJob job, out Dictionary<string, BorgerDkArticleDescription> articles) {

            ImportTask task = job.AddTask("Fetching article list from Borger.dk").Start();

            articles = new Dictionary<string, BorgerDkArticleDescription>();

            foreach (BorgerDkEndpoint endpoint in BorgerDkEndpoint.Values) {

                ImportTask endpointTask = task.AddTask($"Fetching articles from {endpoint.Domain}").Start();

                try {

                    // Initialize a new service instance for the endpoint
                    BorgerDkHttpService borgerdk = new(endpoint);

                    // Fetch the article list
                    BorgerDkArticleDescription[] list = borgerdk.GetArticleList();
                
                    // Add the articles to the dictionary
                    foreach (BorgerDkArticleDescription row in list) {
                        articles.Add(endpoint.Domain + "_" + row.Id, row);
                    }

                    endpointTask
                        .AppendToMessage($"Found {list.Length} articles")
                        .Completed();

                } catch (Exception ex) {

                    _logger.LogError(ex, "Failed fetching articles for endpoint {Endpoint}.", endpoint.Domain);

                    endpointTask.Failed(ex);

                    break;

                }

            }

            if (task.Status != ImportStatus.Failed) task.Completed();

            return task.Status != ImportStatus.Failed;

        }

        private bool FetchArticlesFromDatabase(ImportJob job, out List<BorgerDkArticleDto> result) {

            ImportTask task = job.AddTask("Fetching existing articles from the database").Start();

            result = null;

            try {

                result = GetAllArticlesDtos();

                task.AppendToMessage($"Found {result.Count} articles").Completed();

                return true;

            } catch (Exception ex) {

                _logger.LogError(ex, "Failed fetching existing articles for the database.");

                task.Failed(ex);

                return false;

            }

        }

        private void SynchronizeArticles(ImportJob job, Dictionary<string, BorgerDkArticleDescription> fromApi, List<BorgerDkArticleDto> fromDb) {

            ImportTask task = job.AddTask("Synchronizing articles").Start();

            int updated = 0;
            int skipped = 0;

            foreach (BorgerDkArticleDto dto in fromDb) {

                ImportTask articleTask = task.AddTask($"Synchronizing article with unique ID {dto.Id}.").Start();

                try {

                    if (fromApi.TryGetValue(dto.Domain + "_" + dto.ArticleId, out var value)) {

                        if (value.UpdateDate > dto.UpdateDate) {

                            ImportTask fetchTask = articleTask.AddTask("Fetching article content from web service").Start();

                            BorgerDkArticle article;

                            try {
                                
                                // Get the endpoint from the domain
                                BorgerDkEndpoint endpoint = BorgerDkEndpoint.GetFromDomain(dto.Domain);

                                // Initialize a new service instance from the endpoint
                                BorgerDkHttpService service = new(endpoint);

                                // Get the municipality from the code
                                BorgerDkMunicipality municipality = BorgerDkMunicipality.GetFromCode(dto.Municipality);

                                // Fetch the article from the web service
                                article = service.GetArticleFromId(dto.ArticleId, municipality);

                                // Update the task status
                                fetchTask.Completed();

                            } catch (Exception ex) {

                                fetchTask.Failed(ex);

                                articleTask.Stop();

                                continue;

                            }

                            try {

                                Import(article);

                                articleTask.Completed(ImportAction.Updated);

                                updated++;

                            } catch (Exception ex) {

                                articleTask.Failed(ex);

                            }
                            
                        } else {

                            articleTask
                                .AppendToMessage("No changes found ... skipping article.")
                                .Completed(ImportAction.NotModified);

                            skipped++;

                        }

                    } else {

                        articleTask
                            .AppendToMessage("Article no longer exists in web service ... ignoring for now.")
                            .Completed(ImportAction.Deleted);

                        skipped++;

                    }

                } catch (Exception ex) {

                    //_logger.Error<BorgerDkService>(ex, "Failed fetching articles for endpoint {Endpoint}.", endpoint.Domain);

                    articleTask.Failed(ex);

                    break;

                }

            }
            
            // Update the task
            if (task.Status == ImportStatus.Failed) {
                task.Stop();
                return;
            }

            List<string> message = new() {
                $"updated {updated} {(updated == 1 ? "article" : "articles")}",
                $"skipped {skipped} {(skipped == 1 ? "article" : "articles")}"
            };


            task
                .AppendToMessage(string.Join(" and ", message).FirstCharToUpper())
                .Completed();

        }

    }

}