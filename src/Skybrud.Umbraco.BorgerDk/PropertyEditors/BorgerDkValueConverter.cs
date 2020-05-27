using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json;
using Skybrud.Essentials.Json.Extensions;
using Skybrud.Integrations.BorgerDk;
using Skybrud.Integrations.BorgerDk.Elements;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Skybrud.Umbraco.BorgerDk.PropertyEditors {

    public class BorgerDkValueConverter : PropertyValueConverterBase {

        /// <summary>
        /// Gets a value indicating whether the converter supports a property type.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <returns>A value indicating whether the converter supports a property type.</returns>
        public override bool IsConverter(IPublishedPropertyType propertyType) {
            return propertyType.EditorAlias == "Skybrud.BorgerDk";
        }
        
        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview) {

            if (source is string str && str.DetectIsJson()) {

                JObject json = JsonUtils.ParseJsonObject(str);

                string domain = json.GetString("domain");

                int municipality = json.GetInt32("municipality");

                int articleId = json.GetInt32("id");

                BorgerDkArticle article = new BorgerDkService().GetArticleById(domain, municipality, articleId);

                return new BorgerDkModel(json, article);

            }

            return null;

        }
        
        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview) {
            return inter;
        }

        public override object ConvertIntermediateToXPath(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview) {
            return null;
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType) {
            return PropertyCacheLevel.Snapshot;
        }
        
        /// <summary>
        /// Gets the type of values returned by the converter.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <returns>The CLR type of values returned by the converter.</returns>
        public override Type GetPropertyValueType(IPublishedPropertyType propertyType) {
            return typeof(BorgerDkModel);
        }

    }

    public class BorgerDkModel {

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
        public Element[] Elements { get; }

        #endregion

        public BorgerDkModel(JObject obj, BorgerDkArticle article) {

            Id = obj.GetInt32("id");
            Url = obj.GetString("url");
            Title = obj.GetString("title");
            Header = obj.GetString("header");
            ByLine = obj.GetString("byline");
            Selection = obj.GetStringArray("selection");

            Article = article;

            if (article == null) return;

            Title = article.Title;
            Header = article.Header;

            List<Element> elements = new List<Element>();

            foreach (BorgerDkElement element in article.Elements) {

                if (element is BorgerDkTextElement text && Selection.Contains(element.Id)) {

                    elements.Add(new TextElement(text));

                } else if (element is BorgerDkBlockElement block) {

                    List<MicroArticle> micros = new List<MicroArticle>();

                    foreach (var micro in block.MicroArticles) {

                        if (Selection.Contains("kernetekst") || Selection.Contains(micro.Id)) {
                            micros.Add(new MicroArticle(micro));
                        }

                    }

                    if (micros.Any()) elements.Add(new BlockElement(micros));

                }

            }

            Elements = elements.ToArray();

        }

    }

    public class Element {

        [JsonProperty("id", Order = -99)]
        public string Id { get; protected set; }

        [JsonProperty("title", Order = -98)]
        public string Title { get; protected set; }

    }

    public class TextElement : Element {

        [JsonProperty("content")]
        public string Content { get; }

        public TextElement(BorgerDkTextElement element) {
            Id = element.Id;
            Title = element.Title;
            Content = Regex.Replace(element.Content, "^<h3>(.+?)</h3>", string.Empty);
        }

    }

    public class BlockElement : Element {

        [JsonProperty("microArticles")]
        public MicroArticle[] MicroArticles { get; }

        public BlockElement(List<MicroArticle> microArticles) {
            Id = "kernetekst";
            Title = "Kernetekst";
            MicroArticles = microArticles.ToArray();
        }

    }

    public class MicroArticle {

        [JsonProperty("id")]
        public string Id { get; }

        [JsonProperty("title")]
        public string Title { get; }

        [JsonProperty("content")]
        public string Content { get; }

        public MicroArticle(BorgerDkMicroArticle micro) {
            Id = micro.Id;
            Title = micro.Title;
            Content = Regex.Replace(micro.Content, "^<h2>(.+?)</h2>", string.Empty);
        }

    }

}