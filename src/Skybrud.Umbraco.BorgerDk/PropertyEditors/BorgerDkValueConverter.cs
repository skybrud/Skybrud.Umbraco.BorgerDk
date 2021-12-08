using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json;
using Skybrud.Essentials.Json.Extensions;
using Skybrud.Integrations.BorgerDk;
using Skybrud.Umbraco.BorgerDk.Models.Published;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Skybrud.Umbraco.BorgerDk.PropertyEditors
{

    public class BorgerDkValueConverter : PropertyValueConverterBase {
        private readonly IScopeProvider _scopeProvider;
        private readonly ILogger<BorgerDkService> _logger;
        private readonly IIOHelper _iOHelper;

        public BorgerDkValueConverter(IScopeProvider scopeProvider, ILogger<BorgerDkService> logger, IIOHelper iOHelper)
        {
            _scopeProvider = scopeProvider;
            _logger = logger;
            _iOHelper = iOHelper;
        }

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

                BorgerDkArticle article = new BorgerDkService(_scopeProvider, _logger, _iOHelper).GetArticleById(domain, municipality, articleId);

                return new BorgerDkPublishedArticle(json, article);

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
            return typeof(BorgerDkPublishedArticle);
        }

    }

}