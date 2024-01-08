using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using Skybrud.Essentials.Strings;
using Skybrud.Integrations.BorgerDk;
using Skybrud.Integrations.BorgerDk.Elements;
using Skybrud.Integrations.BorgerDk.Exceptions;
using Skybrud.Umbraco.BorgerDk.Models.Import;
using Skybrud.Umbraco.BorgerDk.Scheduling;
using Skybrud.WebApi.Json;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

// ReSharper disable AssignNullToNotNullAttribute

namespace Skybrud.Umbraco.BorgerDk.Controllers {

    [JsonOnlyConfiguration]
    [PluginController("Skybrud")]
    public class BorgerDkController : UmbracoAuthorizedApiController {

        private readonly IServerRegistrar _serverRegistrar;
        private readonly BorgerDkService _borgerdk;
        private readonly BorgerDkImportTaskSettings _importSettings;

        public BorgerDkController(IServerRegistrar serverRegistrar, BorgerDkService borgerdk, BorgerDkImportTaskSettings importSettings) {
            _serverRegistrar = serverRegistrar;
            _borgerdk = borgerdk;
            _importSettings = importSettings;
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
                role = _serverRegistrar.GetCurrentServerRole().ToString(),
                import = _importSettings
            };
        }

        public object GetEndpoints() {

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

            BorgerDkHttpService service = new BorgerDkHttpService(endpoint);

            IEnumerable<BorgerDkArticleDescription> articles = (IEnumerable<BorgerDkArticleDescription>) Current.AppCaches.RuntimeCache.Get("BorgerDkArticleList:" + endpoint.Domain, () => service.GetArticleList(), TimeSpan.FromMinutes(10));

            if (string.IsNullOrWhiteSpace(text) == false) {
                articles = articles.Where(x => x.Title.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0);
            }

            return articles.Select(x => new {
                id = x.Id,
                url = x.Url,
                title = HttpUtility.HtmlDecode(x.Title),
                publishDate = x.PublishDate.UnixTimeSeconds,
                updateDate = x.UpdateDate.UnixTimeSeconds
            });

        }

        public object GetArticleByUrl() {

            string url = HttpContext.Current.Request.QueryString["url"];
            int municipalityCode = StringUtils.ParseInt32(HttpContext.Current.Request.QueryString["municipality"]);

            if (string.IsNullOrWhiteSpace(url)) {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Ingen URL angivet.");
            }

            // Get the endpoint from the domain/URL
            BorgerDkEndpoint endpoint = BorgerDkEndpoint.GetFromUrl(url);
            if (endpoint == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Den angivne URL er ikke gyldig.");

            // Parse the municipality code
            if (BorgerDkMunicipality.TryGetFromCode(municipalityCode, out BorgerDkMunicipality municipality) == false) {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Den angivne kommune er ikke gyldig.");
            }

            // Initialize a new service instance
            BorgerDkHttpService http = new BorgerDkHttpService(endpoint);

            // Look up the ID of the article with the specified URL
            BorgerDkArticleShortDescription item;
            try {
                item = http.GetArticleIdFromUrl(url);
            } catch (BorgerDkNotFoundException) {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Artiklen med den angivne URL blev ikke fundet.");
            } catch (BorgerDkNotExportableException) {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Artiklen med den angivne URL er låst for eksport fra Borger.dk.");
            } catch (BorgerDkException ex) {
                Logger.Error<BorgerDkController>(ex.Message, ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            } catch (Exception ex) {
                Logger.Error<BorgerDkController>("Der skete en fejl i kaldet til Borger.dk.", ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Der skete en fejl i kaldet til Borger.dk.");
            }

            // Get the article via the web service
            BorgerDkArticle article;
            try {
                article = http.GetArticleFromId(item.Id, municipality);
            } catch (Exception ex) {
                Logger.Error<BorgerDkController>("Der skete en fejl i kaldet til Borger.dk.", ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Der skete en fejl i kaldet til Borger.dk.");
            }

            // Make sure to import/update the article
            _borgerdk.Import(article);



            List<object> elements = new List<object>();

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
                publishDate = article.PublishDate.UnixTimeSeconds,
                updateDate = article.UpdateDate.UnixTimeSeconds,
                elements
            };

        }

    }

}