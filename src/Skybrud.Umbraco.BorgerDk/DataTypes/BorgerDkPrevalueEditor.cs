using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Skybrud.BorgerDk;
using umbraco.interfaces;
using System.Linq;

namespace Skybrud.Umbraco.BorgerDk.DataTypes {


    public class BorgerDkPrevalueEditor : PlaceHolder, IDataPrevalue {

        private class Bacon {
            
            public RadioButton NotAllowed;
            public RadioButton Allowed;
            public RadioButton Mandatory;
            public string Name;
            public string Text;

            public Bacon(string name, string text, int status = 0) {
                Name = name;
                Text = text;
                Mandatory = new RadioButton { ID = "radioTypeMandatory_" + name, Checked = status == 2 };
                Allowed = new RadioButton { ID = "radioTypeAllowed_" + name, Checked = status == 1 };
                NotAllowed = new RadioButton { ID = "radioTypeNotAllowed_" + name, Checked = status != 2 && status != 1 };
                Mandatory.GroupName = "radioType_" + Name;
                Allowed.GroupName = "radioType_" + Name;
                NotAllowed.GroupName = "radioType_" + Name;
            }

            public void SetMandatory() {
                SetStatus(2);
            }

            public void SetAllowed() {
                SetStatus(1);
            }

            public void SetNotAllowed() {
                SetStatus(0);
            }

            private void SetStatus(int status) {
                Mandatory.Checked = status == 2;
                Allowed.Checked = status == 1;
                NotAllowed.Checked = status != 2 && status != 1;
            }

        }

        // referenced datatype
        protected readonly umbraco.cms.businesslogic.datatype.BaseDataType DataType;

        private DropDownList _dropDownMunicipalityId;

        private List<Bacon> _chkTypes = new List<Bacon>();

        public BorgerDkMunicipality Municipality { get; private set; }
        public string[] MandatoryTypes { get; private set; }
        public string[] AllowedTypes { get; private set; }

        public string Dafuq;

        public BorgerDkPrevalueEditor(umbraco.cms.businesslogic.datatype.BaseDataType dataType) {

            DataType = dataType;

            SetupChildControls();

            // Get the selected municipality id
            Municipality = BorgerDkMunicipality.FirstOrDefault(x => x.Code.ToString() == ConfigurationMunicipalityId);
            MandatoryTypes = ConfigurationMandatoryContentTypes;
            AllowedTypes = ConfigurationAllowedContentTypes;
        
        }

        private void SetupChildControls() {

            _dropDownMunicipalityId = new DropDownList { ID = "dropDownMunicipalityId" };

            _dropDownMunicipalityId.Style.Add("height", "24px");

            Controls.Add(_dropDownMunicipalityId);

            foreach (var pair in BorgerDkHelper.GetContentTypes()) {
                Bacon bacon = new Bacon(pair.Key, pair.Value);
                _chkTypes.Add(bacon);
                Controls.Add(bacon.Mandatory);
                Controls.Add(bacon.Allowed);
                Controls.Add(bacon.NotAllowed);
            }

        }

        public Control Editor {
            get { return this; }
        }

        private void InitDropDown() {
            foreach (var municipality in BorgerDkMunicipality.Values.OrderBy(x => x.Name)) {
                string text = municipality.NameLong + " (" + municipality.Code + ")";
                if (municipality.Code == 0) text = "Ikke angivet";
                ListItem item = new ListItem(text, municipality.Code.ToString());
                if (Municipality.Code == municipality.Code) item.Selected = true;
                _dropDownMunicipalityId.Items.Add(item);
            }
        }

        protected override void OnLoad(EventArgs e) {
            
            base.OnLoad(e);

            if (Page.IsPostBack) {

                Municipality = BorgerDkMunicipality.GetFromCode(_dropDownMunicipalityId.SelectedValue);

            } else {

                foreach (var chk in _chkTypes) {
                    if (MandatoryTypes.Contains(chk.Name)) chk.SetMandatory();
                    else if (AllowedTypes.Contains(chk.Name)) chk.SetAllowed();
                    else chk.SetNotAllowed();
                }

                InitDropDown();

            }
        
        }

        public void Save() {

            // The the selected municipality
            Municipality = BorgerDkMunicipality.GetFromCode(_dropDownMunicipalityId.SelectedValue);
            MandatoryTypes = (from chk in _chkTypes where chk.Mandatory.Checked select chk.Name).ToArray();
            AllowedTypes = (from chk in _chkTypes where chk.Allowed.Checked select chk.Name).ToArray();

            DataType.DBType = umbraco.cms.businesslogic.datatype.DBTypes.Ntext;
            //DataType.DBType = (umbraco.cms.businesslogic.datatype.DBTypes)Enum.Parse(typeof(umbraco.cms.businesslogic.datatype.DBTypes), DBTypes.Ntext.ToString(), true);
            DataTypeHelpers.DeletePreValues(DataType);
            ConfigurationMunicipalityId = Municipality.Code.ToString();
            ConfigurationMandatoryContentTypes = MandatoryTypes;
            ConfigurationAllowedContentTypes = AllowedTypes;

        }

        protected override void Render(HtmlTextWriter writer) {

            writer.WriteLine("<div class=\"propertyItem\">");
            writer.WriteLine("<div class=\"propertyItemheader\">Kommune</div>");
            writer.WriteLine("<div class=\"propertyItemContent\">");
            _dropDownMunicipalityId.RenderControl(writer);
            writer.WriteLine("</div>");
            writer.WriteLine("</div>");

            writer.WriteLine("<div class=\"propertyItem\">");
            writer.WriteLine("<div class=\"propertyItemheader\">Indholdstyper</div>");
            writer.WriteLine("<div class=\"propertyItemContent\">");
            writer.WriteLine("<table>");

            foreach (var bacon in _chkTypes) {

                writer.WriteLine("<tr>");

                writer.WriteLine("<td style=\"font-weight: bold; font-size: 10px; padding-right: 15px;\">" + bacon.Text + "</td>");

                writer.WriteLine("<td>");
                bacon.Mandatory.RenderControl(writer);
                writer.WriteLine("</td>");
                writer.WriteLine("<td style=\"vertical-align: middle; padding-right: 10px;\"><label for=\"" + bacon.Mandatory.ClientID + "\">Krævet</label></td>");

                writer.WriteLine("<td>");
                bacon.Allowed.RenderControl(writer);
                writer.WriteLine("</td>");
                writer.WriteLine("<td style=\"vertical-align: middle; padding-right: 10px;\"><label for=\"" + bacon.Allowed.ClientID + "\">Tilladt</label></td>");

                writer.WriteLine("<td>");
                bacon.NotAllowed.RenderControl(writer);
                writer.WriteLine("</td>");
                writer.WriteLine("<td style=\"vertical-align: middle;\"><label for=\"" + bacon.NotAllowed.ClientID + "\">Ikke tilladt</label></td>");

                writer.WriteLine("</tr>");

            }

            writer.Write("</table>");
            writer.WriteLine("</div>");
            writer.WriteLine("</div>");


            writer.WriteLine("<br /><br /><br />");
        
        }

        public string ConfigurationMunicipalityId {
            get { return DataTypeHelpers.GetPreValue(DataType, "municipalityid"); }
            set { DataTypeHelpers.SavePreValue(DataType, "municipalityid", value); }
        }

        public string[] ConfigurationMandatoryContentTypes {
            get {
                string value = DataTypeHelpers.GetPreValue(DataType, "mandatorycontenttypes");
                return (value ?? "").Split(new [] {","}, StringSplitOptions.RemoveEmptyEntries);
            }
            set {
                DataTypeHelpers.SavePreValue(DataType, "mandatorycontenttypes", String.Join(",", value));
            }
        }

        public string[] ConfigurationAllowedContentTypes {
            get {
                string value = DataTypeHelpers.GetPreValue(DataType, "allowedcontenttypes");
                return (value ?? "").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            }
            set {
                DataTypeHelpers.SavePreValue(DataType, "allowedcontenttypes", String.Join(",", value));
            }
        }

    }

}
