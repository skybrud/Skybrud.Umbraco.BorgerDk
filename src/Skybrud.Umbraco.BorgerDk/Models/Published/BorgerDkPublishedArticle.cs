using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Extensions;
using Skybrud.Integrations.BorgerDk;
using Skybrud.Integrations.BorgerDk.Elements;
using System.Collections.Generic;
using System.Linq;

namespace Skybrud.Umbraco.BorgerDk.Models.Published {

    public class BorgerDkPublishedArticle {

        #region Properties

        [JsonProperty("id")]
        public int Id { get; }

        [JsonProperty("url")]
        public string Url { get; }

        [JsonProperty("title")]
        public string Title { get; }

        [JsonProperty("header")]
        public string Header { get; }

        [JsonProperty("byline")]
        public string ByLine { get; }

        [JsonIgnore]
        public string[] Selection { get; }

        [JsonIgnore]
        public BorgerDkArticle Article { get; }

        [JsonProperty("elements")]
        public BorgerDkPublishedElement[] Elements { get; }

        #endregion

        public BorgerDkPublishedArticle(JObject obj, BorgerDkArticle article) {

            Id = obj.GetInt32("id");
            Url = obj.GetString("url");
            Title = obj.GetString("title");
            Header = obj.GetString("header");
            ByLine = obj.GetString("byline");
            Selection = obj.GetStringArray("selection");

            Article = article;

            if (article == null) {
                return;
            }

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

                    if (micros.Any()) {
                        elements.Add(new BorgerDkPublishedBlockElement(micros));
                    }
                }

            }

            Elements = elements.ToArray();

        }

    }
}