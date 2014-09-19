using System;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Umbraco.Core;

namespace Skybrud.Umbraco.BorgerDk.Model {

    public class BorgerDkArticle {

        public int Id { get; private set; }
        public string Domain { get; private set; }
        public string Url { get; private set; }
        public int MunicipalityId { get; private set; }
        public DateTime LastReloaded { get; private set; }
        public DateTime PublishingDate { get; private set; }
        public DateTime LastUpdated { get; private set; }
        public string Title { get; private set; }
        public string Header { get; private set; }
        public string[] Selected { get; private set; }
        public IBorgerDkBlock[] Blocks { get; private set; }
        public XElement Xml { get; private set; }

        public bool HasSelected {
            get { return Selected.Length > 0; }
        }

        public HtmlString TitleAsHtml {
            get { return new HtmlString(Title); }
        }

        public HtmlString HeaderAsHtml {
            get { return new HtmlString(Header); }
        }

        public BorgerDkMicroArticle[] MicroArticles {
            get {
                var kernetekst = Blocks.OfType<BorgerDkMicroArticlesBlock>().FirstOrDefault();
                return kernetekst == null ? new BorgerDkMicroArticle[0] : kernetekst.MicroArticles;
            }
        }

        public static BorgerDkArticle ParseXml(string xml) {
            if (xml == null) return null;
            return xml.StartsWith("<article>") && xml.EndsWith("</article>") ? Parse(XElement.Parse(xml)) : null;
        }

        public static BorgerDkArticle Parse(XElement xml) {

            XElement xId = xml.Element("id");
            XElement xDomain = xml.Element("domain");
            XElement xUrl = xml.Element("url");
            XElement xMunicipalityId = xml.Element("municipalityid");
            XElement xLastReloaded = xml.Element("lastreloaded");
            XElement xPublishingDate = xml.Element("publishingdate");
            XElement xLastUpdated = xml.Element("lastupdated");
            XElement xTitle = xml.Element("title");
            XElement xHeader = xml.Element("header");
            XElement xSelected = xml.Element("selected");
            XElement xXml = xml.Element("xml");

            if (xId == null) return null;
            if (xDomain == null) return null;
            if (xUrl == null) return null;
            if (xMunicipalityId == null) return null;
            if (xLastReloaded == null) return null;
            if (xPublishingDate == null) return null;
            if (xLastUpdated == null) return null;
            if (xTitle == null) return null;
            if (xHeader == null) return null;
            if (xSelected == null) return null;
            if (xXml == null) return null;

            return new BorgerDkArticle {
                Id = Int32.Parse(xId.Value),
                Domain = xDomain.Value,
                MunicipalityId = Int32.Parse(xMunicipalityId.Value),
                LastReloaded = DateTime.Parse(xLastReloaded.Value),
                PublishingDate = DateTime.Parse(xPublishingDate.Value),
                LastUpdated = DateTime.Parse(xLastUpdated.Value),
                Title = xTitle.Value,
                Header = xHeader.Value,
                Selected = xSelected.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
                Xml = xml,
                Blocks = (
                    from xBlock in xXml.Elements()
                    select xBlock.Name.LocalName == "kernetekst" ? (IBorgerDkBlock) BorgerDkMicroArticlesBlock.Parse(xBlock) : (IBorgerDkBlock) BorgerDkTextBlock.Parse(xBlock)
                    ).WhereNotNull().ToArray()
            };

        }

    }

}
