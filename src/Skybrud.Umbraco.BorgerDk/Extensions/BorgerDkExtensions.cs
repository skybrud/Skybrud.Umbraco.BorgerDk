using System;
using System.Xml.Linq;

namespace Skybrud.Umbraco.BorgerDk.Extensions {

    internal static class BorgerDkExtensions {

        public static string GetAttribute(this XElement e, string name) {
            XAttribute x = e == null ? null : e.Attribute(name);
            return x == null ? null : x.Value;
        }

        public static T GetAttribute<T>(this XElement e, string name) {
            XAttribute x = e == null ? null : e.Attribute(name);
            return x == null ? default(T) : (T) Convert.ChangeType(x.Value, typeof(T));
        }

        public static string GetValue(this XElement e, string name) {
            XElement x = e == null ? null : e.Element(name);
            return x == null ? null : x.Value;
        }

        public static T GetValue<T>(this XElement e, string name) {
            XElement x = e == null ? null : e.Element(name);
            return x == null ? default(T) : (T) Convert.ChangeType(x.Value, typeof(T));
        }

    }

}
