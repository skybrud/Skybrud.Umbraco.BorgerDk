using System;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Skybrud.Umbraco.BorgerDk.DataTypes.MicroArticles {

    public class BorgerDkMicroArticlesPropertyEditorValueConverter : IPropertyEditorValueConverter {
        
        public bool IsConverterFor(Guid propertyEditorId, string docTypeAlias, string propertyTypeAlias) {
            return propertyEditorId.ToString().ToUpper() == "88B29A40-3518-44F7-B90F-EE69E31A7787";
        }

        public Attempt<object> ConvertPropertyValue(object value) {
            return new Attempt<object>(true, BorgerDkMicroArticlesModel.GetFromJson(value + ""));
        }
    
    }

}