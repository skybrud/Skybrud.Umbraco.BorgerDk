using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using System.Xml.Linq;
using Skybrud.BorgerDk;
using Skybrud.Umbraco.BorgerDk.Extensions.XElement;
using Skybrud.WebApi.Json;
using Umbraco.Core.Logging;
using Umbraco.Web.WebApi;

namespace Skybrud.Umbraco.BorgerDk.Controllers.Api {

    [JsonOnlyConfiguration]
    public class BorgerDkMaintenanceController : UmbracoApiController {
        
        [HttpGet]
        public object UpdateArticles(int limit = 30) {

            // Make sure we have a directory
            BorgerDkHelpers.EnsureTempDirectory();

            List<object> results = new List<object>();

            // Determine the files/articles to be updated
            var files = (
                from file in BorgerDkHelpers.GetCachedFiles()
                let errorPath = file.FullName + ".errors"
                where file.LastWriteTimeUtc.AddHours(24) <= DateTime.UtcNow && File.GetLastWriteTimeUtc(errorPath).AddHours(24) <= DateTime.UtcNow
                select new {
                    ErrorPath = errorPath,
                    file.Name,
                    file.FullName,
                    file.LastWriteTimeUtc
                }
            ).ToArray();

            int total = files.Length;

            foreach (var file in files.Take(limit)) {

                DateTime dt = DateTime.UtcNow;
        
                try {

                    // Load the XML for the article
                    XElement xArticle = XElement.Load(file.FullName);

                    // Get basic information about the article
                    int articleId = xArticle.GetElementValue<int>("id");
                    string articleDomain = xArticle.GetElementValue("domain");
                    BorgerDkMunicipality municipality = xArticle.GetElementValue("municipalityid", BorgerDkMunicipality.GetFromCode);

                    BorgerDkEndpoint endpoint = BorgerDkEndpoint.GetFromDomain(articleDomain);
                    if (endpoint == null) {
                        throw new Exception("Domain \"" + articleDomain + "\" doesn't match a known Borger.dk endpoint.");
                    }

                    // Initialize a new service instance from the endpoint and municipality
                    BorgerDkService service = new BorgerDkService(endpoint, municipality);

                    // Fetch the article from the Borger.dk web service
                    BorgerDkArticle article = service.GetArticleFromId(articleId);

                    // Save the article to disk
                    BorgerDkHelpers.SaveToDisk(article);

                    results.Add(new {
                        id = article.Id,
                        url = article.Url,
                        title = article.Title,
                        duration = DateTime.UtcNow.Subtract(dt).TotalMilliseconds
                    });

                } catch (Exception ex) {
            
                    LogHelper.Error<BorgerDkMaintenanceController>("Unable to update Borger.dk article (" + file.Name + "):" + ex.Message, ex);

                    File.AppendAllText(file.FullName + ".errors", "Unable to update Borger.dk article (" + file.Name + "):" + ex.Message + "\r\n" + ex.StackTrace + "\r\n");

                    results.Add(new {
                        error = "Unable to update Borger.dk article (" + file.Name + ").",
                        duration = DateTime.UtcNow.Subtract(dt).TotalMilliseconds
                    });
            
                }

            }

            return new {
                total,
                limit,
                results
            };

        }

    }

}