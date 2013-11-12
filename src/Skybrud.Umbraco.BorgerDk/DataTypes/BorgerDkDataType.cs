using System;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;
using System.Linq;

namespace Skybrud.Umbraco.BorgerDk.DataTypes {
    
    public class BorgerDkDataType : BaseDataType, IDataType {
        
        private IDataEditor _dataEditor;
        private IData _baseData;
        private BorgerDkPrevalueEditor _prevalueEditor;

        public override IDataEditor DataEditor {
            get {
                var pre = (BorgerDkPrevalueEditor) PrevalueEditor;
                return _dataEditor ?? (_dataEditor = new BorgerDkDataEditor(Data, this, pre.ConfigurationMunicipalityId));
            }
        }

        public override IDataPrevalue PrevalueEditor {
            get { return _prevalueEditor ?? (_prevalueEditor = new BorgerDkPrevalueEditor(this)); }
        }

        public override IData Data {
            get { return _baseData ?? (_baseData = new BorgerDkData(this)); }
        }

        public override Guid Id {
            get { return new Guid("6b85170d-c095-42e1-a722-e047f0f4180c"); }
        }

        public override string DataTypeName {
            get { return "Borger.dk"; }
        }

    }

}
