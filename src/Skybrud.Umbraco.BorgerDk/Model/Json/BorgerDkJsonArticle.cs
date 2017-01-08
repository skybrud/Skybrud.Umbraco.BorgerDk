using System;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using Skybrud.BorgerDk;
using Skybrud.Umbraco.BorgerDk.Interfaces;

namespace Skybrud.Umbraco.BorgerDk.Model.Json {

    public class BorgerDkJsonArticle : IBorgerDkArticle {

        #region Properties

        public int Id { get; private set; }

        public string Domain { get; private set; }

        public string Url { get; private set; }

        public BorgerDkMunicipality Municipality { get; private set; }

        public DateTime Published { get; private set; }

        public DateTime Updated { get; private set; }

        public string Title { get; private set; }

        public string Header { get; private set; }

        public string Html { get; private set; }

        public BorgerDkJsonElement[] Elements { get; private set; }

        public BorgerDkJsonBlockElement Kernetekst {
            get { return Elements.OfType<BorgerDkJsonBlockElement>().FirstOrDefault(); }
        }

        public BorgerDkJsonMicroArticle[] MicroArticles {
            get {
                BorgerDkJsonBlockElement block = Kernetekst;
                return Kernetekst == null ? new BorgerDkJsonMicroArticle[0] : block.MicroArticles;
            }
        }

        #endregion

        #region Constructors

        private BorgerDkJsonArticle() { }

        #endregion

        #region Static methods

        public static BorgerDkJsonArticle GetFromCache(string domain, int articleId, BorgerDkMunicipality municipality) {

            // Declare the path to the storage directory
            string storagePath = HttpContext.Current.Server.MapPath("~/App_Data/Skybrud.BorgerDk/");

            // Declare the path to the cache file
            string filePath = String.Format("{0}{1}__{2}__{3}.json", storagePath, domain.Replace(".", "_"), articleId, (municipality ?? BorgerDkMunicipality.NoMunicipality).Code);

            // Read the contents of the file (throws an exception if not found)
            string contents = File.ReadAllText(filePath);

            // Parse the JSON
            return GetFromJson(contents);

        }

        public static BorgerDkJsonArticle GetFromJson(string json) {

            JObject obj = JObject.Parse(json);
            
            return new BorgerDkJsonArticle {
                Id = obj.GetInt32("id"),
                Domain = obj.GetString("domain"),
                Url = obj.GetString("url"),
                Municipality = obj.GetInt32("municipality", BorgerDkMunicipality.GetFromCode),
                Published = obj.GetString("published", DateTime.Parse),
                Updated = obj.GetString("updated", DateTime.Parse),
                Title = obj.GetString("title"),
                Header = obj.GetString("header"),
                Html = obj.GetString("html"),
                Elements = obj.GetArray("elements", BorgerDkJsonElement.Parse)
            };

        }

        #endregion

    }

}