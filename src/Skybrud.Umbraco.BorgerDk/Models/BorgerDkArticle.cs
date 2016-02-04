using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.Dynamics;

namespace Skybrud.Umbraco.BorgerDk.Models {

    public static class SlagelseBorgerDkHelpers  {

        public static string GetElementValue(XElement xElement, string name) {
            XElement xChild = xElement == null ? null : xElement.Element(name);
            return xChild == null ? null : xChild.Value;
        }

        public static T GetElementValue<T>(XElement xElement, string name) {
            XElement xChild = xElement == null ? null : xElement.Element(name);
            return xChild == null ? default(T) : (T) Convert.ChangeType(xChild.Value, typeof(T));
        }
    
    }
    
    public class SlagelseBorgerDkElement {

        public XElement XElement { get; protected set; }

        public string Alias { get; protected set; }
        
    }

    public class SlagelseBorgerDkMicroArticle {

        public XElement XElement { get; protected set; }

        public string Title { get; protected set; }

        public string Html { get; protected set; }

        public string Content { get; protected set; }

        public static SlagelseBorgerDkMicroArticle Parse(XElement element) {

            string title = SlagelseBorgerDkHelpers.GetElementValue(element, "title");

            string html = (SlagelseBorgerDkHelpers.GetElementValue(element, "html") ?? "").Trim();

            string content = Regex.Replace(html, "^<h2>(.+?)</h2>", "").Trim();
            
            return new SlagelseBorgerDkMicroArticle {
                XElement = element,
                Title = title,
                Html = html,
                Content = content
            };
        }

    }

    public class SlagelseBorgerDkKernetekstElement : SlagelseBorgerDkElement {

        public SlagelseBorgerDkMicroArticle[] MicroArticles { get; private set; }

        public static SlagelseBorgerDkKernetekstElement Parse(XElement element) {
            return new SlagelseBorgerDkKernetekstElement {
                Alias = element.Name.LocalName,
                XElement = element,
                MicroArticles = (
                    from xMicroArticle in element.Elements("microArticle")
                    select SlagelseBorgerDkMicroArticle.Parse(xMicroArticle)
                ).ToArray()
            };

        }

    }

    public class SlagelseBorgerDkTextElement : SlagelseBorgerDkElement {

        public string Title { get; protected set; }

        public string Html { get; protected set; }

        public string Content { get; protected set; }
        
        public static SlagelseBorgerDkTextElement Parse(XElement element) {

            string title = SlagelseBorgerDkHelpers.GetElementValue(element, "title");

            string html = (SlagelseBorgerDkHelpers.GetElementValue(element, "html") ?? "").Trim();

            string content = Regex.Replace(html, "^<h3>(.+?)</h3>", "").Trim();
            
            return new SlagelseBorgerDkTextElement {
                Alias = element.Name.LocalName,
                Title = title,
                XElement = element,
                Html = html,
                Content = content
            };
            
        }
        
    }

    public class SlagelseBorgerDkArticle {
        
        public int Id { get; private set; }
        public string Domain { get; private set; }
        public string Url { get; private set; }
        public int MunicipalityId { get; private set; }
        public int ReloadInterval { get; private set; }
        public string Title { get; private set; }
        public string Header { get; private set; }
        public string[] Selected { get; private set; }
        
        public SlagelseBorgerDkElement[] Elements { get; set; }

        public static SlagelseBorgerDkArticle Parse(DynamicXml xml) {
            return Parse(xml.BaseElement);
        }

        public static SlagelseBorgerDkArticle Parse(XElement xElement) {

            List<SlagelseBorgerDkElement> elements = new List<SlagelseBorgerDkElement>();

            foreach (XElement xml in xElement.XPathSelectElements("/xml/*")) {
                
                if (xml.Name.LocalName == "kernetekst") {
                    elements.Add(SlagelseBorgerDkKernetekstElement.Parse(xml));
                } else {
                    elements.Add(SlagelseBorgerDkTextElement.Parse(xml));
                }
                
            }

            return new SlagelseBorgerDkArticle {
                Id = GetElementValue<int>(xElement, "id"),
                Domain = GetElementValue(xElement, "domain"),
                Url = GetElementValue(xElement, "url"),
                MunicipalityId = GetElementValue<int>(xElement, "municipalityid"),
                ReloadInterval = GetElementValue<int>(xElement, "reloadinterval"),
                Title = GetElementValue(xElement, "title"),
                Header = GetElementValue(xElement, "header"),
                Selected = (GetElementValue(xElement, "selected") ?? "").Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries),
                Elements = elements.ToArray()
            };

        }
        
        private static string GetElementValue(XElement xElement, string name) {
            XElement xChild = xElement == null ? null : xElement.Element(name);
            return xChild == null ? null : xChild.Value;
        }
        
        private static T GetElementValue<T>(XElement xElement, string name) {
            XElement xChild = xElement == null ? null : xElement.Element(name);
            return xChild == null ? default(T) : (T) Convert.ChangeType(xChild.Value, typeof(T));
        }
        
    }

}