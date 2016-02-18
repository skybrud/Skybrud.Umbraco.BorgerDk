using System;
using System.IO;
using System.Web.Hosting;
using System.Xml.Linq;
using Skybrud.BorgerDk;

namespace Skybrud.Umbraco.BorgerDk.Models.Cached {

    public class BorgerDkCachedArticle {

        #region Properties

        public bool Exists { get; private set; }

        public XElement XElement { get; private set; }

        #endregion

        private BorgerDkCachedArticle(bool exists, XElement xml) {
            Exists = exists;
            XElement = xml;
        }

        public static string GetSavePath(BorgerDkMunicipality municipality, string domain, int articleId) {
            return HostingEnvironment.MapPath("~/App_Data/BorgerDk/" + municipality.Code + "_" + domain.Replace(".", "") + "_" + articleId + ".xml");
        }

        public static BorgerDkCachedArticle Load(BorgerDkMunicipality municipality, string domain, int articleId) {
            if (municipality != null && !String.IsNullOrWhiteSpace(domain) && articleId > 0) {
                string path = GetSavePath(municipality, domain, articleId);
                if (File.Exists(path)) {
                    return new BorgerDkCachedArticle(true, XElement.Load(path));
                }
            }
            return new BorgerDkCachedArticle(false, null);
        }

    }

}