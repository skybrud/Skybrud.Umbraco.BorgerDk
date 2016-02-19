using System;

namespace Skybrud.Umbraco.BorgerDk.Extensions.XElement {

    public static class XElementExtensions {

        public static string GetElementValue(this System.Xml.Linq.XElement xElement, string name) {
            System.Xml.Linq.XElement xChild = xElement == null ? null : xElement.Element(name);
            return xChild == null ? null : xChild.Value;
        }

        public static T GetElementValue<T>(this System.Xml.Linq.XElement xElement, string name) {
            System.Xml.Linq.XElement xChild = xElement == null ? null : xElement.Element(name);
            return xChild == null ? default(T) : (T) Convert.ChangeType(xChild.Value, typeof(T));
        }

        public static T GetElementValue<T>(this System.Xml.Linq.XElement xElement, string name, Func<string, T> func) {
            System.Xml.Linq.XElement xChild = xElement == null ? null : xElement.Element(name);
            return func(xChild == null ? null : xChild.Value);
        }

        public static string GetAttributeValue(this System.Xml.Linq.XElement xElement, string name) {
            System.Xml.Linq.XAttribute xAttr = xElement == null ? null : xElement.Attribute(name);
            return xAttr == null ? null : xAttr.Value;
        }

    }

}