using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Http;
using Skybrud.BorgerDk;
using Skybrud.Umbraco.BorgerDk.Exceptions;
using Skybrud.WebApi.Json;
using Skybrud.WebApi.Json.Meta;
using Umbraco.Core.Logging;
using Umbraco.Web.WebApi;
using www.borger.dk._2009.WSArticleExport.v1.types;

namespace Skybrud.Umbraco.BorgerDk.Controllers.Api {

    [JsonOnlyConfiguration]
    public class BorgerDkController : UmbracoAuthorizedApiController {

        protected NameValueCollection QueryString {
            get { return HttpContext.Current.Request.QueryString; }
        }

        [HttpGet]
        public object GetMunicipalities() {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            return (
                from municipality in BorgerDkMunicipality.Values
                where municipality.Name != null
                orderby municipality.Name
                select new {
                    code = municipality.Code,
                    name = municipality.NameLong
                }
            );
        }

        [HttpGet]
        public object GetEndpoints() {
            return (
                from endpoint in BorgerDkEndpoint.Values
                select new {
                    id = endpoint.Domain.Replace(".", ""),
                    domain = endpoint.Domain,
                    name = endpoint.Name
                }
            );
        }

        [HttpGet]
        public object GetArticleList(string query = null, string sort = "title", string order = "asc") {

            // Set the culture to allow sorting based on Danish conventions
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");

            // Dtermine the sort order
            BorgerDkSortOrder sortOrder = (order == "desc" ? BorgerDkSortOrder.Descending : BorgerDkSortOrder.Ascending);

            // Determine the sort field
            BorgerDkArticleSortField sortField;
            switch (sort) {
                case "published":
                    sortField = BorgerDkArticleSortField.Published;
                    break;
                case "updated":
                    sortField = BorgerDkArticleSortField.Updated;
                    break;
                default:
                    sortField = BorgerDkArticleSortField.Title;
                    break;
            }
            
            List<object> temp = new List<object>();

            // Iterate through each endpoint. Since there are currently only two known endpoint, we
            // just fetch articles for both of them at the same time.
            foreach (BorgerDkEndpoint endpoint in BorgerDkEndpoint.Values) {

                DateTime start = DateTime.UtcNow;

                // Initialize the options
                BorgerDkGetArticlesOptions options = new BorgerDkGetArticlesOptions {
                    Endpoint = endpoint,
                    Query = (query ?? "").ToLower().Trim(),
                    SortField = sortField,
                    SortOrder = sortOrder
                };

                // Get the articles for the endpoint (calls the Borger.dk webservice)
                BorgerDkArticlesResult result = BorgerDkHelper.GetArticles(options);

                TimeSpan ts = DateTime.UtcNow.Subtract(start);

                temp.Add(new {
                    id = endpoint.Domain.Replace(".", ""),
                    domain = endpoint.Domain,
                    name = endpoint.Name,
                    time = ts.TotalSeconds,
                    articles = result.Articles,
                    count = result.Articles.Length
                });

            }

            return new {
                sorting = new {
                    field = sortField.ToString().ToLower(),
                    order = sortOrder == BorgerDkSortOrder.Ascending ? "asc" : "desc"
                },
                data = temp
            };

        }

        [HttpGet]
        public object GetArticleFromUrl() {

            string url = (QueryString["url"] ?? "").Split('?')[0];
            int municipalityId;

            bool useCache = !(QueryString["cache"] == "0" || QueryString["cache"] == "false");

            #region Validation

            if (String.IsNullOrEmpty(url)) {
                return Request.CreateResponse(JsonMetaResponse.GetError(HttpStatusCode.BadRequest, "Ingen adresse til borger.dk angivet"));
            }

            if (!BorgerDkService.IsValidUrl(url)) {
                return Request.CreateResponse(JsonMetaResponse.GetError(HttpStatusCode.BadRequest, "Ugyldig adresse til borger.dk angivet"));
            }

            if (QueryString["municipalityId"] == null) {
                return Request.CreateResponse(JsonMetaResponse.GetError(HttpStatusCode.BadRequest, "Intet kommune ID angivet"));
            }

            if (!Int32.TryParse(QueryString["municipalityId"], out municipalityId)) {
                return Request.CreateResponse(JsonMetaResponse.GetError(HttpStatusCode.BadRequest, "Ugyldigt kommune ID angivet"));
            }

            #endregion

            BorgerDkArticle article;

            var data = new {
                url,
                municipalityId,
                cache = useCache
            };

            try {
            
                article = BorgerDkHelper.GetArticle(url, municipalityId, useCache);
            
            } catch (System.ServiceModel.FaultException ex) {

                // Certain articles are protected from export
                if (ex.Message.EndsWith("has been marked as not exportable.")) {
                    return Request.CreateResponse(JsonMetaResponse.GetError(HttpStatusCode.InternalServerError, "Artiklen med den angivne adresse kan ikke eksporteres fra Borger.dk", data));
                }

                // Handle the article wasn't found
                if (ex.Message.StartsWith("No article found with url")) {
                    return Request.CreateResponse(JsonMetaResponse.GetError(HttpStatusCode.InternalServerError, "Den angivne artikel blev ikke fundet på Borger.dk", data));
                }
                
                // Handle remaining exceptions thrown by the web service
                LogHelper.Error<BorgerDkController>("Unable to import Borger.dk aticle with URL " + url + ": " + ex.Message, ex);
                return Request.CreateResponse(JsonMetaResponse.GetError(HttpStatusCode.InternalServerError, "Der skete en fejl i forbindelse med Borger.dk", data));
            
            } catch (BorgerDkException ex) {
                LogHelper.Error<BorgerDkController>("Unable to import Borger.dk aticle with URL " + url + ": " + ex.Message, ex);
                return Request.CreateResponse(JsonMetaResponse.GetError(HttpStatusCode.InternalServerError, "Der skete en fejl i forbindelse med Borger.dk: " + ex.Message, data));
            } catch (Exception ex) {
                LogHelper.Error<BorgerDkController>("Unable to import Borger.dk aticle with URL " + url + ": " + ex.Message, ex);
                return Request.CreateResponse(JsonMetaResponse.GetError(HttpStatusCode.InternalServerError, "Der skete en fejl i forbindelse med Borger.dk", data));
            }

            try {
                File.WriteAllText(BorgerDkHelper.GetCachePath(article), article.ToXElement() + "");
            } catch (Exception ex) {
                LogHelper.Error<BorgerDkController>("Unable to save Borger.dk article to disk: " + article.Url + " (" + article.Id + ")", ex);
            }

            return BorgerDkHelper.ToJsonObject(article);

        }

    }

}