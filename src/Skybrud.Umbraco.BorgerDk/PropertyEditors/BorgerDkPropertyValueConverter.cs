using Skybrud.Umbraco.BorgerDk.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Skybrud.Umbraco.BorgerDk.PropertyEditors {

    public class BorgerDkPropertyValueConverter : IPropertyValueConverter {

        public bool IsConverter(PublishedPropertyType propertyType) {
            return propertyType.PropertyEditorAlias == "Skybrud.BorgerDk";
        }

        public object ConvertDataToSource(PublishedPropertyType propertyType, object data, bool preview) {
            return BorgerDkArticleSelection.Deserialize(data as string);
        }

        public object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview) {
            return source;
        }

        public object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview) {
            return null;
        }

    }

}