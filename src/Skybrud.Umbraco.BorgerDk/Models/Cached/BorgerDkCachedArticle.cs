using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Xml.Linq;
using System.Xml.XPath;
using Skybrud.BorgerDk;
using Skybrud.Umbraco.BorgerDk.Extensions.XElement;

namespace Skybrud.Umbraco.BorgerDk.Models.Cached {

    /// <summary>
    /// Class representing a cached Borger.dk article.
    /// </summary>
    public class BorgerDkCachedArticle {

        #region Properties

        /// <summary>
        /// Gets whether the article exists on disk.
        /// </summary>
        public bool Exists { get; private set; }

        /// <summary>
        /// Gets the instance of <see cref="XElement"/> the micro article was parsed from.
        /// </summary>
        public XElement XElement { get; private set; }

        /// <summary>
        /// Gets the ID of the article.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the URL of the article.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Gets the domain of the article.
        /// </summary>
        public string Domain { get; private set; }

        /// <summary>
        /// Gets the municipality of the article.
        /// </summary>
        public BorgerDkMunicipality Municipality { get; private set; }

        /// <summary>
        /// Gets the timestamp for when the article was first published.
        /// </summary>
        public DateTime Published { get; private set; }

        /// <summary>
        /// Gets the timestamp for when the article was last updated.
        /// </summary>
        public DateTime LastUpdated { get; private set; }

        /// <summary>
        /// Gets the title of the article.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the header of the article.
        /// </summary>
        public string Header { get; private set; }

        /// <summary>
        /// Gets an array of all elements of the article.
        /// </summary>
        public BorgerDkCachedElement[] Elements { get; private set; }

        /// <summary>
        /// Gets a reference to the <em>kernetekst</em> element of the article.
        /// </summary>
        public BorgerDkCachedKernetekstElement Kernetekst { get; private set; }

        /// <summary>
        /// Gets an array of all text elements (blocks) of the article.
        /// </summary>
        public BorgerDkCachedTextElement[] Blocks { get; private set; }

        /// <summary>
        /// Gets an array of all micro articles of the article.
        /// </summary>
        public BorgerDkCachedMicroArticle[] MicroArticles { get; private set; }

        #endregion

        private BorgerDkCachedArticle(bool exists, XElement xml) {
        
            Exists = exists;
            XElement = xml;
            if (xml == null) return;

            Id = xml.GetElementValue<int>("id");
            Url = xml.GetElementValue("url");
            Domain = xml.GetElementValue("domain");
            Municipality = xml.GetElementValue("municipality", BorgerDkMunicipality.GetFromCode);
            Published = xml.GetElementValue("publishingdate", DateTime.Parse);
            LastUpdated = xml.GetElementValue("lastupdated", DateTime.Parse);
            Title = xml.GetElementValue("title");
            Header = xml.GetElementValue("header");

            List<BorgerDkCachedElement> elements = new List<BorgerDkCachedElement>();
            List<BorgerDkCachedTextElement> blocks = new List<BorgerDkCachedTextElement>();
            foreach (XElement element in xml.XPathSelectElements("/xml/*")) {
                if (element.Name.LocalName == "kernetekst") {
                    elements.Add(BorgerDkCachedKernetekstElement.Parse(element));
                } else {
                    BorgerDkCachedTextElement text = BorgerDkCachedTextElement.Parse(element);
                    elements.Add(text);
                    blocks.Add(text);
                }
            }
            Elements = elements.ToArray();

            Kernetekst = elements.OfType<BorgerDkCachedKernetekstElement>().FirstOrDefault();

            Blocks = blocks.ToArray();

            MicroArticles = (Kernetekst == null ? new BorgerDkCachedMicroArticle[0] : Kernetekst.MicroArticles);

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