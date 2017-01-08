using System;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.BorgerDk;
using Skybrud.Umbraco.BorgerDk.Interfaces;
using Skybrud.Umbraco.BorgerDk.Model.Json;

namespace Skybrud.Umbraco.BorgerDk.DataTypes.MicroArticles {
    
    public class BorgerDkMicroArticlesModel : IBorgerDkArticle {

        #region Private fields

        private BorgerDkJsonArticle _article;

        private BorgerDkJsonMicroArticle[] _microArticles;
        private BorgerDkJsonTextElement[] _blocks;

        private string[] _selected = new string[0];

        #endregion

        #region Properties

        public int Id { get; set; }

        public string Domain { get; set; }

        public string Url { get; set; }

        public BorgerDkMunicipality Municipality { get; set; }

        public DateTime LastReloaded { get; set; }

        public DateTime Published { get; set; }

        public DateTime Updated { get; set; }

        public string Title { get; set; }

        public string Header { get; set; }

        public string[] Selected {
            get { return _selected; }
            set { _selected = value ?? new string[0]; }
        }

        /// <summary>
        /// Gets the referenced article from the cache on the disk. If the article hasn't been saved to the disk, an
        /// exception of the type <code>FileNotFoundException</code> will be thrown.
        /// </summary>
        public BorgerDkJsonArticle Article {
            get { return _article ?? InitArticle(); }
        }

        public BorgerDkJsonMicroArticle[] MicroArticles {
            get { if (_microArticles == null) { InitArticle(); } return _microArticles; }
        }

        public BorgerDkJsonTextElement[] Blocks {
            get { if (_blocks == null) { InitArticle(); } return _blocks; }
        }

        /// <summary>
        /// Gets whether the model is refering an article (meaning that the <code>Id</code> property is above <code>0</code>).
        /// </summary>
        public bool HasArticle {
            get { return Id > 0; }
        }

        public bool HasCacheFile {
            get { return BorgerDkHelpers.HasCacheFile(this); }
        }

        public bool HasUrl {
            get { return !String.IsNullOrWhiteSpace(Url); }
        }

        #endregion

        private BorgerDkJsonArticle InitArticle() {
            _article = BorgerDkJsonArticle.GetFromCache(Domain, Id, Municipality);
            _microArticles = _article.MicroArticles.Where(x => Selected.Contains(x.Id)).ToArray();
            _blocks = _article.Elements.OfType<BorgerDkJsonTextElement>().Where(x => Selected.Contains(x.Type)).ToArray();
            return _article;
        }

        public void UpdateFromArticle(BorgerDkArticle article, BorgerDkMunicipality municipality) {
            UpdateFromArticle(article, municipality, new string[0]);
        }

        public void UpdateFromArticle(BorgerDkArticle article, BorgerDkMunicipality municipality, string[] selected) {
            if (article == null) {
                Id = 0;
                Domain = null;
                Municipality = municipality;
                Published = default(DateTime);
                Updated = default(DateTime);
                Title = null;
                Header = null;
                Selected = new string[0];
                LastReloaded = default(DateTime);
            } else {
                Id = article.Id;
                Domain = article.Domain;
                Municipality = municipality;
                Published = article.Published;
                Updated = article.Modified;
                Title = HttpUtility.HtmlDecode(article.Title.Trim());
                Header = HttpUtility.HtmlDecode(article.Header.Trim());
                Selected = selected ?? new string[0];
                LastReloaded = DateTime.Now;
            }
        }

        public string ToJson() {
            return ToJson(Formatting.None);
        }

        public string ToJson(Formatting formatting) {
            return new JObject {
                {"id", Id},
                {"domain", Domain},
                {"url", Url},
                {"municipality", (Municipality ?? BorgerDkMunicipality.NoMunicipality).Code},
                {"lastreloaded", LastReloaded.ToString("yyyy-MM-dd HH:mm:ss")},
                {"published", Published.ToString("yyyy-MM-dd HH:mm:ss")},
                {"modified", Updated.ToString("yyyy-MM-dd HH:mm:ss")},
                {"title", Title},
                {"header", Header},
                {"selected", String.Join(",", Selected)}
            }.ToString(formatting);
        }

        public static BorgerDkMicroArticlesModel GetFromJson(string json) {

            if (String.IsNullOrWhiteSpace(json) || !json.StartsWith("{") || !json.EndsWith("}")) {
                return new BorgerDkMicroArticlesModel();
            }

            JObject obj = JObject.Parse(json);

            string sLastReloaded = obj.GetString("lastreloaded");
            string sPublished = obj.GetString("published");
            string sModified = obj.GetString("modified");

            DateTime dtLastReloaded;
            DateTime dtPublished;
            DateTime dtModified;

            return new BorgerDkMicroArticlesModel {
                Id = obj.GetInt32("id"),
                Domain = obj.GetString("domain"),
                Url = obj.GetString("url"),
                Municipality = obj.GetInt32("municipality", BorgerDkMunicipality.GetFromCode),
                LastReloaded = DateTime.TryParse(sLastReloaded, out dtLastReloaded) ? dtLastReloaded : default(DateTime),
                Published = DateTime.TryParse(sPublished, out dtPublished) ? dtPublished : default(DateTime),
                Updated = DateTime.TryParse(sModified, out dtModified) ? dtModified : default(DateTime),
                Title = obj.GetString("title"),
                Header = obj.GetString("header"),
                Selected = (obj.GetString("selected") ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            };

        }
    
    }

}