using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Skybrud.BorgerDk;
using umbraco.interfaces;
using System.Linq;

namespace Skybrud.Umbraco.BorgerDk.DataTypes.MicroArticles {

    public class BorgerDkMicroArticlesPrevalueEditor : PlaceHolder, IDataPrevalue {
        
        protected readonly umbraco.cms.businesslogic.datatype.BaseDataType DataType;

        private DropDownList _dropDownMunicipalityId;

        private TextBox _limitTextBox;

        private CheckBox _allowBlocksCheckBox;

        public BorgerDkMunicipality Municipality { get; private set; }

        public int Limit { get; private set; }

        public bool AllowBlocks { get; private set; }

        public string Dafuq;

        public BorgerDkMicroArticlesPrevalueEditor(umbraco.cms.businesslogic.datatype.BaseDataType dataType) {

            DataType = dataType;

            Limit = ConfigurationLimit;

            AllowBlocks = ConfigurationAllowBlocks;

            SetupChildControls();

            // Get the selected municipality id
            Municipality = BorgerDkMunicipality.FirstOrDefault(x => x.Code.ToString(CultureInfo.InvariantCulture) == ConfigurationMunicipalityId);

        }

        private void SetupChildControls() {

            _dropDownMunicipalityId = new DropDownList { ID = "dropDownMunicipalityId" };
            _dropDownMunicipalityId.Style.Add("height", "24px");
            Controls.Add(_dropDownMunicipalityId);

            _limitTextBox = new TextBox();
            _limitTextBox.Attributes.Add("type", "number");
            _limitTextBox.Attributes.Add("min", "1");
            _limitTextBox.Attributes.Add("max", "99");
            _limitTextBox.Text = Limit + "";
            Controls.Add(_limitTextBox);

            _allowBlocksCheckBox = new CheckBox { Checked = AllowBlocks };
            Controls.Add(_allowBlocksCheckBox);

        }

        public Control Editor {
            get { return this; }
        }

        private void InitDropDown() {
            foreach (var municipality in BorgerDkMunicipality.Values.OrderBy(x => x.Name)) {
                string text = municipality.NameLong + " (" + municipality.Code + ")";
                if (municipality.Code == 0) text = "Ikke angivet";
                ListItem item = new ListItem(text, municipality.Code.ToString(CultureInfo.InvariantCulture));
                if (Municipality.Code == municipality.Code) item.Selected = true;
                _dropDownMunicipalityId.Items.Add(item);
            }
        }

        protected override void OnLoad(EventArgs e) {
            
            base.OnLoad(e);

            if (Page.IsPostBack) {

                Municipality = BorgerDkMunicipality.GetFromCode(_dropDownMunicipalityId.SelectedValue);

                int limit;
                Limit = Int32.TryParse(_limitTextBox.Text, out limit) ? Math.Max(1, limit) : 1;

            } else {

                Limit = 1;
                _limitTextBox.Text = "1";

                InitDropDown();

            }
        
        }

        public void Save() {

            // The the selected municipality
            Municipality = BorgerDkMunicipality.GetFromCode(_dropDownMunicipalityId.SelectedValue);

            DataType.DBType = umbraco.cms.businesslogic.datatype.DBTypes.Ntext;
            DataTypeHelpers.DeletePreValues(DataType);
            ConfigurationMunicipalityId = Municipality.Code + "";

            int limit;
            ConfigurationLimit = Limit = Int32.TryParse(_limitTextBox.Text, out limit) ? Math.Max(1, limit) : 1;

            ConfigurationAllowBlocks = AllowBlocks = _allowBlocksCheckBox.Checked;

        }

        protected override void Render(HtmlTextWriter writer) {

            writer.WriteLine("<div class=\"propertyItem\">");
            writer.WriteLine("<div class=\"propertyItemheader\">Kommune</div>");
            writer.WriteLine("<div class=\"propertyItemContent\">");
            _dropDownMunicipalityId.RenderControl(writer);
            writer.WriteLine("</div>");
            writer.WriteLine("</div>");

            writer.WriteLine("<div class=\"propertyItem\">");
            writer.WriteLine("<div class=\"propertyItemheader\">Antal mikroartikler</div>");
            writer.WriteLine("<div class=\"propertyItemContent\">");
            _limitTextBox.RenderControl(writer);
            writer.WriteLine("</div>");
            writer.WriteLine("</div>");
            writer.WriteLine("</div>");

            writer.WriteLine("<div class=\"propertyItem\">");
            writer.WriteLine("<div class=\"propertyItemheader\">Tillad andre bokse?</div>");
            writer.WriteLine("<div class=\"propertyItemContent\">");
            _allowBlocksCheckBox.RenderControl(writer);
            writer.WriteLine("</div>");
            writer.WriteLine("</div>");
        
        }

        public string ConfigurationMunicipalityId {
            get { return DataTypeHelpers.GetPreValue(DataType, "municipalityid"); }
            set { DataTypeHelpers.SavePreValue(DataType, "municipalityid", value); }
        }

        public int ConfigurationLimit {
            get {
                string value = DataTypeHelpers.GetPreValue(DataType, "limit");
                int limit;
                return Int32.TryParse(value, out limit) ? Math.Max(1, limit) : 1;
            }
            set { DataTypeHelpers.SavePreValue(DataType, "limit", value + ""); }
        }

        public bool ConfigurationAllowBlocks {
            get { return DataTypeHelpers.GetPreValue(DataType, "allowBlocks") == "true"; }
            set { DataTypeHelpers.SavePreValue(DataType, "allowBlocks", value ? "true" : "false"); }
        }

    }

}