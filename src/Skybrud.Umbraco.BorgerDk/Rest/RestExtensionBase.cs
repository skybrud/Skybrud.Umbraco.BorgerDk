using System;
using System.Collections;
using System.Configuration;
using System.Net;
using System.Web;
using System.Web.Caching;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Services;
using umbraco.BusinessLogic;

namespace Skybrud.Umbraco.BorgerDk.Rest {

    public abstract class RestExtensionBase {

        public static bool HasValidLogin {
            get {
                if (User.GetCurrent() != null) return true;
                string skybrudUniqueKey = ConfigurationManager.AppSettings["skybrudUniqueKey"];
                return String.IsNullOrWhiteSpace(skybrudUniqueKey) || Request.QueryString["uniqueKey"] == skybrudUniqueKey;
            }
        }

        public static HttpContext Context {
            get { return HttpContext.Current; }
        }

        public static HttpRequest Request {
            get { return HttpContext.Current.Request; }
        }

        public static HttpResponse Response {
            get { return HttpContext.Current.Response; }
        }

        public static HttpServerUtility Server {
            get { return HttpContext.Current.Server; }
        }

        public static HttpSessionState Session {
            get { return HttpContext.Current.Session; }
        }

        public static Cache Cache {
            get { return HttpContext.Current.Cache; }
        }

        public static IDictionary Items {
            get { return HttpContext.Current.Items; }
        }

        public static ServiceContext Services {
            get { return ApplicationContext.Current.Services; }
        }

        public static IContentService ContentService {
            get { return ApplicationContext.Current.Services.ContentService; }
        }

        public static IContentTypeService ContentTypeService {
            get { return ApplicationContext.Current.Services.ContentTypeService; }
        }

        public static IDataTypeService DataTypeService {
            get { return ApplicationContext.Current.Services.DataTypeService; }
        }

        public static IFileService FileService {
            get { return ApplicationContext.Current.Services.FileService; }
        }

        public static ILocalizationService LocalizationService {
            get { return ApplicationContext.Current.Services.LocalizationService; }
        }

        public static IMediaService MediaService {
            get { return ApplicationContext.Current.Services.MediaService; }
        }

        public static void WriteXmlResponse(XDocument document) {
            Response.Clear();
            Response.ContentType = "text/xml";
            Response.Write(document);
            Response.End();
        }

        public static void WriteXmlResponse(XElement element) {
            Response.Clear();
            Response.ContentType = "text/xml";
            Response.Write(element);
            Response.End();
        }

        public static void WriteJsonResponse(object obj) {
            Response.Clear();
            Response.ContentType = "application/json";
            Response.Write(new JavaScriptSerializer().Serialize(obj));
            Response.End();
        }

        public static void WriteJsonSuccess(object data, HttpStatusCode statusCode) {
            WriteJsonSuccess(data, (int) statusCode);
        }

        public static void WriteJsonSuccess(object data, int code = 200) {
            Response.Clear();
            Response.ContentType = "application/json";
            Response.Write(new JavaScriptSerializer().Serialize(new {
                meta = new { code },
                data
            }));
            Response.End();
        }

        public static void WriteJsonError(HttpStatusCode statusCode, string error, object data = null) {
            WriteJsonError((int) statusCode, error, data);
        }

        public static void WriteJsonError(int code, string error, object data = null) {
            Response.Clear();
            Response.ContentType = "application/json";
            Response.Write(new JavaScriptSerializer().Serialize(new {
                meta = new { code, error },
                data
            }));
            Response.End();
        }

    }

}