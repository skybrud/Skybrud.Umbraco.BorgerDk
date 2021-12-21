using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skybrud.Umbraco.BorgerDk.Options {
    public class BorgerDkOptions {
        public const string Position = "Skybrud:Umbraco:BorgerDK";

        public bool UseCaching { get; set; } = false;

        public string CachingConnectionstring { get; set; } = string.Empty;
    }
}
