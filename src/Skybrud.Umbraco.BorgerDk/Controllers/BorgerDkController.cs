using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using Skybrud.Essentials.Strings;
using Skybrud.Integrations.BorgerDk;
using Skybrud.Integrations.BorgerDk.Elements;
using Skybrud.Integrations.BorgerDk.Exceptions;
using Skybrud.Umbraco.BorgerDk.Models;
using Skybrud.WebApi.Json;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

// ReSharper disable AssignNullToNotNullAttribute

namespace Skybrud.Umbraco.BorgerDk.Controllers {

    [JsonOnlyConfiguration]
    [PluginController("Skybrud")]
    public class BorgerDkController : UmbracoAuthorizedApiController {

        private readonly BorgerDkService _borgerdk;

        public BorgerDkController(BorgerDkService borgerdk) {
            _borgerdk = borgerdk;
        }

        [HttpGet]
        [AllowAnonymous]
        public object Import() {

            StringBuilder sb = new StringBuilder();

            AppendLine(sb, "Starting new import from Borger.dk");

            Dictionary<string, BorgerDkArticleDescription> fromApi = new Dictionary<string, BorgerDkArticleDescription>();

            foreach (BorgerDkEndpoint endpoint in BorgerDkEndpoint.Values) {

                Stopwatch sw1 = Stopwatch.StartNew();

                AppendLine(sb, " Fetching articles from " + endpoint.Domain);

                BorgerDkHttpService borgerdk = new BorgerDkHttpService(endpoint);

                BorgerDkArticleDescription[] list = borgerdk.GetArticleList();

                sw1.Stop();

                AppendLine(sb, "  Found " + list.Length + " articles in " + sw1.ElapsedMilliseconds + " ms");
        
                foreach (BorgerDkArticleDescription row in list) {
                    fromApi.Add(endpoint.Domain + "_" + row.Id, row);
                }

            }

            sb.AppendLine();




            Stopwatch sw2 = Stopwatch.StartNew();

            AppendLine(sb, "Fetching existing articles from the database.");

            BorgerDkService service = new BorgerDkService();

            var all = service.GetAllArticlesDtos();

            sw2.Stop();

            AppendLine(sb, " Found " + all.Count + " articles in " + sw2.ElapsedMilliseconds + " ms");

            sb.AppendLine();



            Stopwatch sw3 = Stopwatch.StartNew();

            AppendLine(sb, "Synchronizing articles.");

            foreach (BorgerDkArticleDto dto in all) {

                string left = "  The article with unique ID \"" + dto.Id + "\"";

                if (fromApi.TryGetValue(dto.Domain + "_" + dto.ArticleId, out var value)) {

                    if (value.UpdateDate > dto.UpdateDate) {

                        AppendLine(sb, left + " was updated since the last import.");

                        service.Import(dto.Meta);

                        AppendLine(sb, left + " was updated in the database.");

                    } else {

                        AppendLine(sb, left + " is up-to-date.");

                    }

                } else {

                    AppendLine(sb, left + " was deleted in the web service.");

                }

            }

            sw3.Stop();

            AppendLine(sb, " Completed in " + sw3.ElapsedMilliseconds + " ms");

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();





            string path = IOHelper.MapPath("~/App_Data/LOGS/BorgerDk/" + DateTime.UtcNow.ToString("yyyyMMdd") + ".txt");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.AppendAllText(path, sb.ToString(), Encoding.UTF8);




            return new { log = sb.ToString().Split('\n')};


        }

        private void AppendLine(StringBuilder builder, string line) {

            builder.AppendLine(DateTime.UtcNow.ToString("[HH:mm:ss] ") + line);

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
                publishDate = x.PublishDate.UnixTimestamp,
                updateDate = x.UpdateDate.UnixTimestamp
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
                publishDate = article.PublishDate.UnixTimestamp,
                updateDate = article.UpdateDate.UnixTimestamp,
                elements
            };

        }

    }

}