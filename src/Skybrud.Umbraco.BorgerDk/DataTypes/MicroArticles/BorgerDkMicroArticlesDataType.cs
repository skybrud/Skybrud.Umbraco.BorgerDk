using System;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;

namespace Skybrud.Umbraco.BorgerDk.DataTypes.MicroArticles {

    public class BorgerDkMicroArticlesDataType : BaseDataType, IDataType {
        
        private IDataEditor _dataEditor;
        private BorgerDkMicroArticlesData _baseData;
        private BorgerDkMicroArticlesPrevalueEditor _prevalueEditor;

        public override IDataEditor DataEditor {
            get {
                var pre = (BorgerDkMicroArticlesPrevalueEditor) PrevalueEditor;
                return _dataEditor ?? (_dataEditor = new BorgerDkMicroArticlesDataEditor(Data as BorgerDkMicroArticlesData, this, pre.ConfigurationMunicipalityId));
            }
        }

        public override IDataPrevalue PrevalueEditor {
            get { return _prevalueEditor ?? (_prevalueEditor = new BorgerDkMicroArticlesPrevalueEditor(this)); }
        }

        public override IData Data {
            get { return _baseData ?? (_baseData = new BorgerDkMicroArticlesData(this)); }
        }

        public override Guid Id {
            get { return new Guid("88B29A40-3518-44F7-B90F-EE69E31A7787"); }
        }

        public override string DataTypeName {
            get { return "Borger.dk - Mikroartikel"; }
        }

    }

}
