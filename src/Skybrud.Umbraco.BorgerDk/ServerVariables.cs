using System;
using System.Web;

namespace Skybrud.Umbraco.BorgerDk {
    
    public static class ServerVariables {

        public static HttpContext Context {
            get { return HttpContext.Current; }
        }

        public static string HttpHost {
            get { return Context.Request.ServerVariables["HTTP_HOST"]; }
        }

        public static string HttpReferer {
            get { return Context.Request.ServerVariables["HTTP_REFERER"]; }
        }

        public static string UserAgent {
            get { return Context.Request.ServerVariables["HTTP_USER_AGENT"]; }
        }

        public static bool Https {
            get { return Context.Request.ServerVariables["HTTPS"] == "ON"; }
        }

        public static string QueryString {
            get { return Context.Request.ServerVariables["QUERY_STRING"]; }
        }

        public static string RequestMethod {
            get { return Context.Request.ServerVariables["REQUEST_METHOD"]; }
        }

        public static int ServerPort {
            get { return Int32.Parse(Context.Request.ServerVariables["SERVER_PORT"]); }
        }

        public static string RemoteAddress {
            get { return Context.Request.ServerVariables["REMOTE_ADDR"]; }
        }

    }

}
