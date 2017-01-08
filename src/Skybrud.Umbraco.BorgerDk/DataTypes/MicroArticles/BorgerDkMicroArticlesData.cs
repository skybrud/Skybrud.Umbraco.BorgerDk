using umbraco.cms.businesslogic.datatype;

namespace Skybrud.Umbraco.BorgerDk.DataTypes.MicroArticles {

    public class BorgerDkMicroArticlesData : DefaultData {

        private BorgerDkMicroArticlesModel _model = new BorgerDkMicroArticlesModel();

        public BorgerDkMicroArticlesModel Model {
            get {
                return _model;
            }
            set {
                _model = value;
                UpdateValue();
            }
        }

        public override object Value {
            get {
                return base.Value;
            }
            set {
                base.Value = value;
                UpdateModel();
            }
        }

        public BorgerDkMicroArticlesData(BaseDataType dataType) : base(dataType) { }

        private void UpdateModel() {
            _model = BorgerDkMicroArticlesModel.GetFromJson(Value + "");
        }

        private void UpdateValue() {
            base.Value = _model.ToJson();
        }
    
    }

}