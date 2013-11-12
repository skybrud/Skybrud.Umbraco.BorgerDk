using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;
using Skybrud.BorgerDk;
using umbraco.interfaces;
using System.Linq;

namespace Skybrud.Umbraco.BorgerDk.DataTypes {

    [ValidationProperty("IsValid")]
    public class BorgerDkDataEditor : Control, IDataEditor {

        public int MunicipalityId { get; private set; }
        public int ReloadInterval { get; private set; }

        private readonly IData _data;
        protected readonly BorgerDkDataType DataType;
        private TextBox _txtUrl;
        private HtmlInputHidden _hiddenSelectedFields;

        public BorgerDkDataEditor(IData data, BorgerDkDataType dataType, string municipalityId) {
            _data = data;
            DataType = dataType;
            MunicipalityId = Int32.Parse(municipalityId);
        }

        public virtual bool TreatAsRichTextEditor {
            get { return false; }
        }

        public bool ShowLabel {
            get { return true; }
        }

        public int DataPropertyId {
            get { return ((BorgerDkData) _data).PropertyId; }
        }

        public Control Editor { get { return this; } }

        public string IsValid {
            get  { return String.Empty; }
        }

        public void Save() {

            // Strip the query string
            _txtUrl.Text = _txtUrl.Text.Split('?').First();

            string[] selectedTypes = _hiddenSelectedFields.Value.Split(new [] {","}, StringSplitOptions.RemoveEmptyEntries);
            
            if (BorgerDkService.IsValidUrl(_txtUrl.Text)) {
                try {
                    BorgerDkArticle article = BorgerDkHelper.GetArticle(_txtUrl.Text, MunicipalityId, true);
                    _data.Value = BorgerDkHelper.ToXElement(article, selectedTypes, MunicipalityId, ReloadInterval).ToString(SaveOptions.DisableFormatting);
                    return;
                } catch (Exception ex) {
                    // TODO: Can we stop the saving process at this point?
                }
            }

            _data.Value = new XElement("article", new XElement("url", _txtUrl.Text)).ToString(SaveOptions.DisableFormatting);
        
        }

        private string Pre(string html) {
            return "<pre style=\"margin: 5px 0; border: 1px solid #EFEFEF; background: #EEEEEE; padding: 3px;\">" + html.Trim() + "</pre>";
        }

        private HtmlGenericControl BuildSubTitle(string text, int marginTop = 10, int marginBottom = 10) {
            HtmlGenericControl title = new HtmlGenericControl("div") { InnerHtml = text };
            title.Style.Add("font-weight", "bold");
            title.Style.Add("margin-top", marginTop + "px");
            title.Style.Add("margin-bottom", marginBottom + "px");
            return title;
        }

        private HtmlGenericControl GetSpacer(int marginTop = 10, int marginBottom = 10) {
            HtmlGenericControl spacer = new HtmlGenericControl("div");
            spacer.Style.Add("margin-top", marginTop + "px");
            spacer.Style.Add("margin-bottom", marginBottom + "px");
            spacer.Style.Add("height", "1px");
            spacer.Style.Add("background", "#d9d7d7");
            return spacer;
        }

        protected override void OnInit(EventArgs e) {

            base.OnInit(e);
            
            string mandatoryTypes = DataTypeHelpers.GetPreValue(DataType, "mandatorycontenttypes");
            string allowedTyps = DataTypeHelpers.GetPreValue(DataType, "allowedcontenttypes");
            string municipalityId = DataTypeHelpers.GetPreValue(DataType, "municipalityid");

            string[] selected = new string[0];

            if (_data.Value.ToString().Contains("<article>")) {
                XElement xValue = XElement.Parse(_data.Value + "");
                XElement xSelected = xValue.Element("selected");
                selected = xSelected == null ? new string[0] : xSelected.Value.Split(',');
                //foreach (XElement xElement in xValue.Elements()) {
                //    if (xElement.Name == "url") continue;
                //    if (xElement.Name == "id") continue;
                //    if (xElement.Name == "municipalityid") continue;
                //    if (xElement.Name == "reloadinterval") continue;
                //    if (xElement.Name == "lastreloaded") continue;
                //    if (xElement.Name == "publishingdate") continue;
                //    if (xElement.Name == "lastupdated") continue;
                //    selected.Add(xElement.Name.ToString());
                //}
            }


            HtmlGenericControl outer = new HtmlGenericControl("div");
            outer.Attributes.Add("class", "borgerDkPanel");
            outer.Attributes.Add("data-propertyid", DataPropertyId + "");
            outer.Attributes.Add("data-mandatory", mandatoryTypes);
            outer.Attributes.Add("data-types", allowedTyps);
            outer.Attributes.Add("data-municipalityid", municipalityId);
            outer.Style.Add("padding-right", "5px");
            Controls.Add(outer);

            _hiddenSelectedFields = new HtmlInputHidden();
            _hiddenSelectedFields.Attributes.Add("class", "borgerDkSelected");
            _hiddenSelectedFields.Value = String.Join(",", selected);
            outer.Controls.Add(_hiddenSelectedFields);


            HtmlGenericControl loader = new HtmlGenericControl("div");
            loader.Attributes.Add("class", "borgerDkLoader");
            loader.Style.Add("display", "none");

            HtmlGenericControl inner = new HtmlGenericControl("div");
            inner.Attributes.Add("class", "borgerDkSummary");
            inner.Style.Add("display", "none");





            outer.Controls.Add(BuildSubTitle("Sidens adresse "));

            _txtUrl = new TextBox {Text = "https://"};
            if (_data.Value.ToString().Length > 0) {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(_data.Value.ToString());
                _txtUrl.Text = xmlDocument.SelectSingleNode("article/url").InnerText;
            }
            _txtUrl.TextMode = TextBoxMode.SingleLine;
            _txtUrl.Width = 600;
            _txtUrl.CssClass = "umbEditorTextField borgerDkUrl";
            outer.Controls.Add(_txtUrl);
            
            
            
            HtmlGenericControl errors = new HtmlGenericControl("div");
            errors.Attributes.Add("class", "borgerDkErrors");



            Literal example = new Literal();
            example.Text = "<div class=\"borgerDkExample\">Fx https://www.borger.dk/Sider/Drikkevand-og-vandforbrug.aspx</div>";

            outer.Controls.Add(example);
            outer.Controls.Add(loader);
            outer.Controls.Add(errors);
            outer.Controls.Add(inner);

            Literal lit = new Literal();
            lit.Text = "<ul class=\"borderDkDetails\">";
            lit.Text += "<li><label>ID</label><span class=\"borgerDkArticleId\"></span></li>";
            lit.Text += "<li><label>Publiceret</label><span class=\"borgerDkArticlePublished\"></span></li>";
            lit.Text += "<li><label>Ændret</label><span class=\"borgerDkArticleUpdated\"></span></li>";
            lit.Text += "</ul>";
            inner.Controls.Add(lit);


            inner.Controls.Add(GetSpacer());






            inner.Controls.Add(BuildSubTitle("Indholdstyper"));

            HtmlGenericControl content = new HtmlGenericControl("div");
            content.Attributes.Add("class", "borgerDkContent");
            inner.Controls.Add(content);

            AddJavaScript(GetCachableUrl("/umbraco/skybrud/borgerdk/script.min.js"));
            AddStyleSheet(GetCachableUrl("/umbraco/skybrud/borgerdk/style.min.css"));




            /*if (_txtUrl.Text != "https://" && _txtUrl.Text != "")
            {
                try
                {
                    /*_hiddenFields = new HiddenField();
                    _hiddenFields.ID = "fields";
                    _hiddenFields.Value = fields;
                    ContentTemplateContainer.Controls.Add(_hiddenFields);

                    _hiddenSelectedFields = new HiddenField();
                    _hiddenSelectedFields.ID = "selectedfields";
                    _hiddenSelectedFields.Value = selectedFields;
                    ContentTemplateContainer.Controls.Add(_hiddenSelectedFields);* /

                    AddJavaScript("/umbraco/plugins/SkybrudDk/BorgerDk/script.js");
                    AddStyleSheet("/umbraco/plugins/SkybrudDk/BorgerDk/style.css");

                }
                catch (Exception)
                {
                }
            }*/
        }

        private string GetCachableUrl(string url) {
            if (url.StartsWith("/")) {
                string path = Page.Server.MapPath(url);
                return File.Exists(path) ? url + "?" + File.GetLastWriteTime(path).Ticks : url;
            }
            return url;
        }

        private void AddJavaScript(string url) {

            if (Page.Header.Controls.OfType<HtmlControl>().Any(control => control.TagName == "script" && control.Attributes["src"] == url)) {
                return;
            }

            HtmlGenericControl script = new HtmlGenericControl("script");
            script.Attributes["type"] = "text/javascript";
            script.Attributes["src"] = url;
            Page.Header.Controls.Add(script);

        }

        private void AddStyleSheet(string url) {

            if (Page.Header.Controls.OfType<HtmlControl>().Any(control => control.TagName == "link" && control.Attributes["type"] == "text/css" && control.Attributes["href"] == url)) {
                return;
            }

            HtmlLink link = new HtmlLink { Href = url };
            link.Attributes.Add("rel", "stylesheet");
            link.Attributes.Add("type", "text/css");
            Page.Header.Controls.Add(link);

        }

    }

}