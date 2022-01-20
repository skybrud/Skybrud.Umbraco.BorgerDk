using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skybrud.Essentials.Strings;
using Skybrud.Integrations.BorgerDk;
using Skybrud.Integrations.BorgerDk.Elements;
using Skybrud.Integrations.BorgerDk.Exceptions;
using Skybrud.Umbraco.BorgerDk.Models.Import;
using Skybrud.Umbraco.BorgerDk.Scheduling;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

#pragma warning disable 1591

// ReSharper disable AssignNullToNotNullAttribute

namespace Skybrud.Umbraco.BorgerDk.Controllers {

    [PluginController("Skybrud")]
    public class BorgerDkController : UmbracoAuthorizedApiController {

        private readonly IServerRoleAccessor _serverRoleAccessor;
        private readonly BorgerDkService _borgerdk;
        private readonly BorgerDkImportTaskSettings _importSettings;
        private readonly ILogger<BorgerDkController> _logger;
        private readonly IAppPolicyCache _runtimeCache;

        public BorgerDkController(IServerRoleAccessor serverRoleAccessor, BorgerDkService borgerdk, BorgerDkImportTaskSettings importSettings, ILogger<BorgerDkController> logger, AppCaches appCaches) {
            _serverRoleAccessor = serverRoleAccessor;
            _borgerdk = borgerdk;
            _importSettings = importSettings;
            _logger = logger;
            _runtimeCache = appCaches.RuntimeCache;
        }

        [HttpGet]
        [AllowAnonymous]
        public object Import() {

            // Run a new import
            ImportJob result = _borgerdk.Import();

            // Save the result to the disk
            _borgerdk.WriteToLog(result);

            // Return the result for the API
            return result;

        }

        public object GetSettings() {
            return new {
                role = _serverRoleAccessor.CurrentServerRole.ToString(),
                import = _importSettings
            };
        }

        public static object GetEndpoints() {

            return BorgerDkEndpoint.Values.Select(x => new {
                domain = x.Domain,
                name = x.Name
            });

        }

        public object GetArticles(string text = null, string domain = null) {

            BorgerDkEndpoint endpoint = BorgerDkEndpoint.Default;

            if (string.IsNullOrWhiteSpace(domain) == false) {
                endpoint = BorgerDkEndpoint.GetFromDomain(domain);
            }

            BorgerDkHttpService service = new(endpoint);

            IEnumerable<BorgerDkArticleDescription> articles = (IEnumerable<BorgerDkArticleDescription>) _runtimeCache.Get("BorgerDkArticleList:" + endpoint.Domain, () => service.GetArticleList(), TimeSpan.FromMinutes(10));

            if (string.IsNullOrWhiteSpace(text) == false) {
                articles = articles.Where(x => x.Title.Contains(text, StringComparison.CurrentCultureIgnoreCase));
            }

            return articles.Select(x => new {
                id = x.Id,
                url = x.Url,
                title = WebUtility.HtmlDecode(x.Title),
                publishDate = x.PublishDate.UnixTimestamp,
                updateDate = x.UpdateDate.UnixTimestamp
            });

        }

        public object GetArticleByUrl() {

            string url = HttpContext.Request.Query["url"];
            int municipalityCode = StringUtils.ParseInt32(HttpContext.Request.Query["municipality"]);

            if (string.IsNullOrWhiteSpace(url)) {
                return BadRequest("Ingen URL angivet.");
            }

            // Get the endpoint from the domain/URL
            BorgerDkEndpoint endpoint = BorgerDkEndpoint.GetFromUrl(url);
            if (endpoint == null) return BadRequest("Den angivne URL er ikke gyldig.");

            // Parse the municipality code
            if (BorgerDkMunicipality.TryGetFromCode(municipalityCode, out BorgerDkMunicipality municipality) == false) {
                return BadRequest("Den angivne kommune er ikke gyldig.");
            }

            // Initialize a new service instance
            BorgerDkHttpService http = new(endpoint);

            // Look up the ID of the article with the specified URL
            BorgerDkArticleShortDescription item;
            try {
                item = http.GetArticleIdFromUrl(url);
            } catch (BorgerDkNotFoundException) {
                return StatusCode((int) HttpStatusCode.InternalServerError, "Artiklen med den angivne URL blev ikke fundet.");
            } catch (BorgerDkNotExportableException) {
                return StatusCode((int) HttpStatusCode.InternalServerError, "Artiklen med den angivne URL er låst for eksport fra Borger.dk.");
            } catch (BorgerDkException ex) {
                _logger.LogError(ex, ex.Message);
                return StatusCode((int) HttpStatusCode.InternalServerError, ex.Message);
            } catch (Exception ex) {
                _logger.LogError(ex, "Der skete en fejl i kaldet til Borger.dk.");
                return StatusCode((int) HttpStatusCode.InternalServerError, "Der skete en fejl i kaldet til Borger.dk.");
            }

            // Get the article via the web service
            BorgerDkArticle article;
            try {
                article = http.GetArticleFromId(item.Id, municipality);
            } catch (Exception ex) {
                _logger.LogError(ex, "Der skete en fejl i kaldet til Borger.dk.");
                return StatusCode((int) HttpStatusCode.InternalServerError, "Der skete en fejl i kaldet til Borger.dk.");
            }

            // Make sure to import/update the article
            _borgerdk.Import(article);



            List<object> elements = new();

            foreach (BorgerDkElement element in article.Elements) {

                if (element is BorgerDkTextElement text) {
                    elements.Add(new {
                        id = element.Id,
                        title = text.Title,
                        content = Regex.Replace(text.Content, "^<h3>(.+?)</h3>", string.Empty).Trim().Replace("<a ", "<a prevent-default")
                    });
                } else if (element is BorgerDkBlockElement block) {
                    elements.Add(new {
                        id = element.Id,
                        title = "Mikroartikler",
                        microArticles = block.MicroArticles.Select(x => new {
                            id = x.Id,
                            title = x.Title,
                            content = Regex.Replace(x.Content, "^<h2>(.+?)</h2>", string.Empty).Trim().Replace("<a ", "<a prevent-default")
                        })
                    });
                }

            }

            return new {
                id = article.Id,
                url = article.Url,
                domain = article.Domain,
                municipality = article.Municipality,
                title = article.Title,
                header = article.Header,
                byline = StringUtils.StripHtml(article.Elements.OfType<BorgerDkTextElement>().FirstOrDefault(x => x.Id == "byline")?.Content),
                publishDate = article.PublishDate.UnixTimestamp,
                updateDate = article.UpdateDate.UnixTimestamp,
                elements
            };

        }

    }

}