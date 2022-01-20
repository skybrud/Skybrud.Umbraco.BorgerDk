using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Extensions;
using Skybrud.Integrations.BorgerDk;
using Skybrud.Integrations.BorgerDk.Elements;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Skybrud.Umbraco.BorgerDk.Models.Published {

    /// <summary>
    /// Class representing a published Borger.dk - aka an article selected on an <see cref="IPublishedElement"/>.
    /// </summary>
    public class BorgerDkPublishedArticle {

        #region Properties

        /// <summary>
        /// Gets the numeric ID of the article.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; }

        /// <summary>
        /// Gets the URL of the article.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; }

        /// <summary>
        /// Gets the title of the article.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; }

        /// <summary>
        /// Gets the header of the article.
        /// </summary>
        [JsonProperty("header")]
        public string Header { get; }

        /// <summary>
        /// Gets the by line of the article.
        /// </summary>
        [JsonProperty("byline")]
        public string ByLine { get; }

        /// <summary>
        /// Gets an array with the IDs for the selected article lements.
        /// </summary>
        [JsonIgnore]
        public string[] Selection { get; }

        /// <summary>
        /// Gets a reference to the article as received from the Borger.dk web service.
        /// </summary>
        [JsonIgnore]
        public BorgerDkArticle Article { get; }

        /// <summary>
        /// Gets a reference to the selected article elements.
        /// </summary>
        [JsonProperty("elements")]
        public BorgerDkPublishedElement[] Elements { get; }

        #endregion

        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="json"/> object and <paramref name="article"/>.
        /// </summary>
        /// <param name="json">The JSON object as saved on the <see cref="IPublishedElement"/>.</param>
        /// <param name="article">The article as received from the Borger.dk web service.</param>
        public BorgerDkPublishedArticle(JObject json, BorgerDkArticle article) {

            Id = json.GetInt32("id");
            Url = json.GetString("url");
            Title = json.GetString("title");
            Header = json.GetString("header");
            ByLine = json.GetString("byline");
            Selection = json.GetStringArray("selection");

            Article = article;

            if (article == null) return;

            Title = article.Title;
            Header = article.Header;

            List<BorgerDkPublishedElement> elements = new();

            foreach (BorgerDkElement element in article.Elements) {

                if (element is BorgerDkTextElement text && Selection.Contains(element.Id)) {

                    elements.Add(new BorgerDkPublishedTextElement(text));

                } else if (element is BorgerDkBlockElement block) {

                    List<BorgerDkPublishedMicroArticle> micros = new();

                    foreach (var micro in block.MicroArticles) {

                        if (Selection.Contains("kernetekst") || Selection.Contains(micro.Id)) {
                            micros.Add(new BorgerDkPublishedMicroArticle(micro));
                        }

                    }

                    if (micros.Any()) elements.Add(new BorgerDkPublishedBlockElement(micros));

                }

            }

            Elements = elements.ToArray();

        }

    }
}