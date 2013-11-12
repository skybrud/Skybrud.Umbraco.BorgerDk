using System;
using System.IO;
using System.Web;

namespace Skybrud.Umbraco.BorgerDk.Rest.Jobs {
    
    /// <summary>
    /// Class keeping track of the job state. The job state will be saved to <var>~/App_Data/BorgerDkState.txt</var>
    /// so it will remain even the same even if the server is restarted.
    /// 
    /// Since there might be several hundred articles to update, the articles are updated in cycles. The
    /// <var>LastDay</var> and <var>LastNodeId</var> properties are used to keep track of the progress.
    /// <var>LastDay</var> is specified in the format yyyyMMdd, and if set to the present day together with
    /// <var>LastNodeId</var> being zero, it means that the job has already been completed at the present day.
    /// </summary>
    internal static class BorgerDkUpdateJobState {

        private static bool _busy;
        private static string _lastDay = "";
        private static int _lastNodeId;

        /// <summary>
        /// If the job is currently running, this will be <var>TRUE</var>.
        /// </summary>
        public static bool IsBusy {
            get { return _busy; }
            set { _busy = value; Save(); }
        }

        /// <summary>
        /// The last day the job has completed. This value may initially be an empty string. 
        /// </summary>
        public static string LastDay {
            get { return _lastDay; }
            set { _lastDay = value ?? ""; Save(); }
        }

        /// <summary>
        /// The job will go through relevant nodes one by one from a list of nodes sorted by their IDs. This value will
        /// keep track of the progess. If the value is zero, the job has completed.
        /// </summary>
        public static int LastNodeId {
            get { return _lastNodeId; }
            set { _lastNodeId = value; Save(); }
        }

        public static string SavePath {
            get { return HttpContext.Current.Server.MapPath("~/App_Data/BorgerDkState.txt"); }
        }

        #region Constructors

        static BorgerDkUpdateJobState() {
            Load();
        }

        #endregion

        /// <summary>
        /// Resets the state.
        /// </summary>
        public static void Reset() {
            _busy = false;
            _lastDay = "";
            _lastNodeId = 0;
            Save();
        }

        /// <summary>
        /// Save the current job state. There should really be no reason to call this manually since it's used
        /// internally when a property is changed.
        /// </summary>
        public static void Save() {
            File.WriteAllText(SavePath, _busy + "\r\n" + _lastDay + "\r\n" + _lastNodeId);
        }

        /// <summary>
        /// Loads the job state.
        /// </summary>
        public static void Load() {

            // Check whether the file exists
            if (!File.Exists(SavePath)) {
                Reset();
                return;
            }

            // Read the lines from the file
            string[] lines = File.ReadAllLines(SavePath);

            // Validate the file
            if (lines.Length < 3) {
                Reset();
                return;
            }

            // Parse the file
            _busy = lines[0].ToLower() == "true";
            _lastDay = lines[1];
            _lastNodeId = Int32.Parse(lines[2]);

        }

    }

}