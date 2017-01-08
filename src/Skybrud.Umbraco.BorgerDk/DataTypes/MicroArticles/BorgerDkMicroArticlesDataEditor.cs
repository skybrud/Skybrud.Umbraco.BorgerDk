using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Skybrud.BorgerDk;
using Umbraco.Core.Logging;
using umbraco.interfaces;
using System.Linq;

namespace Skybrud.Umbraco.BorgerDk.DataTypes.MicroArticles {

    public class BorgerDkMicroArticlesDataEditor : Control, IDataEditor {

        public int MunicipalityId { get; private set; }

        protected readonly BorgerDkMicroArticlesData Data;
        protected readonly BorgerDkMicroArticlesDataType DataType;
        private TextBox _txtUrl;
        private HtmlInputHidden _hiddenSelectedFields;

        public BorgerDkMicroArticlesDataEditor(BorgerDkMicroArticlesData data, BorgerDkMicroArticlesDataType dataType, string municipalityId) {
            Data = data;
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
            get { return 1234; }
        }

        public Control Editor { get { return this; } }

        public void Save() {

            // Strip the query string
            _txtUrl.Text = _txtUrl.Text.Split('?').First();

            string[] selectedTypes = _hiddenSelectedFields.Value.Split(new [] {","}, StringSplitOptions.RemoveEmptyEntries);

            BorgerDkMicroArticlesModel model = new BorgerDkMicroArticlesModel {
                Url = _txtUrl.Text
            };
            
            if (BorgerDkService.IsValidUrl(_txtUrl.Text)) {
                
                try {

                    // Get a reference to the municipality
                    BorgerDkMunicipality municipality = BorgerDkMunicipality.GetFromCode(MunicipalityId);
                    
                    // Fetch the article from the Borger.dk web service
                    BorgerDkArticle article = BorgerDkHelpers.GetArticle(_txtUrl.Text, MunicipalityId, true);
                    
                    // Save the article to the disk
                    BorgerDkHelpers.SaveToCacheFile(article);

                    // Update the model
                    model.UpdateFromArticle(article, municipality, selectedTypes);
                
                } catch (Exception ex) {

                    LogHelper.Error<BorgerDkMicroArticlesDataEditor>("Unable to save data type", ex);
                
                    // TODO: Can we stop the saving process at this point?

                    // Prevent the data from being saved
                    return;

                }
            
            }

            Data.Model = model;

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

            // HACKS!!!
            Data.Value = Data.Value;

            string municipalityId = DataTypeHelpers.GetPreValue(DataType, "municipalityid");
            int limit = Math.Max(1, DataTypeHelpers.GetPreValueInt32(DataType, "limit", 1));
            bool allowBlocks = DataTypeHelpers.GetPreValue(DataType, "allowBlocks") == "true";

            HtmlGenericControl outer = new HtmlGenericControl("div");
            outer.Attributes.Add("class", "borgerDkMicroArticles");
            outer.Attributes.Add("data-propertyid", DataPropertyId + "");
            outer.Attributes.Add("data-municipalityid", municipalityId);
            outer.Attributes.Add("data-limit", limit + "");
            outer.Attributes.Add("data-allow-blocks", allowBlocks ? "true" : "false");
            outer.Style.Add("padding-right", "5px");
            Controls.Add(outer);

            _hiddenSelectedFields = new HtmlInputHidden();
            _hiddenSelectedFields.Attributes.Add("class", "borgerDkSelected");
            _hiddenSelectedFields.Value = String.Join(",", Data.Model.Selected);
            outer.Controls.Add(_hiddenSelectedFields);


            HtmlGenericControl loader = new HtmlGenericControl("div");
            loader.Attributes.Add("class", "borgerDkLoader");
            loader.Style.Add("display", "none");

            HtmlGenericControl inner = new HtmlGenericControl("div");
            inner.Attributes.Add("class", "borgerDkSummary");
            inner.Style.Add("display", "none");





            outer.Controls.Add(BuildSubTitle("Sidens adresse "));

            _txtUrl = new TextBox {Text = "https://"};
            if (Data.Model.HasUrl) _txtUrl.Text = Data.Model.Url;
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






            inner.Controls.Add(BuildSubTitle("Mikroatikler"));

            HtmlGenericControl content = new HtmlGenericControl("div");
            content.Attributes.Add("class", "borgerDkContent borgerDkMicroArticles");
            inner.Controls.Add(content);

            HtmlGenericControl blocksContainer = new HtmlGenericControl("div");

            blocksContainer.Controls.Add(BuildSubTitle("Bokse"));

            HtmlGenericControl contentBlocks = new HtmlGenericControl("div");
            contentBlocks.Attributes.Add("class", "borgerDkContent borgerDkBlocks");
            blocksContainer.Controls.Add(contentBlocks);

            inner.Controls.Add(blocksContainer);

            AddJavaScript(GetCachableUrl("/umbraco/skybrud/borgerdk/script.min.js"));
            AddStyleSheet(GetCachableUrl("/umbraco/skybrud/borgerdk/style.min.css"));

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