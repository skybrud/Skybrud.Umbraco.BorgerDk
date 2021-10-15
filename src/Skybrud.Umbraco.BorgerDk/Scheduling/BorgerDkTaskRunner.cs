using System;
using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Scheduling;

namespace Skybrud.Umbraco.BorgerDk.Scheduling {
    
    public class BorgerDkTaskRunner : BackgroundTaskRunner<IBackgroundTask> {

        #region Constructors

        public BorgerDkTaskRunner(ILogger logger) : base(nameof(BorgerDkTaskRunner), logger) { }

        #endregion

        #region Member methods

        /// <summary>
        /// Returns the last run time of the specified <paramref name="task"/>, or <see cref="DateTime.MinValue"/> if the task has not yet been run.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>An instance of <see cref="DateTime"/> representing the last run time.</returns>
        public DateTime GetLastRunTime(RecurringTaskBase task) {
            string name = task.GetType().ToString();
            string path = IOHelper.MapPath($"/App_Data/Skybrud/BorgerDk/Tasks/{name}/LastRunTime.txt");
            return File.Exists(path) ? File.GetLastWriteTime(path) : DateTime.MinValue;
        }

        /// <summary>
        /// Returns the last run time of the specified <paramref name="task"/>, or <see cref="DateTime.MinValue"/> if the task has not yet been run.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>An instance of <see cref="DateTime"/> representing the last run time.</returns>
        public DateTime GetLastRunTimeUtc(RecurringTaskBase task) {
            string name = task.GetType().ToString();
            string path = IOHelper.MapPath($"/App_Data/Skybrud/BorgerDk/Tasks/{name}/LastRunTime.txt");
            return File.Exists(path) ? File.GetLastWriteTimeUtc(path) : DateTime.MinValue;
        }
        
        public bool ShouldRun(RecurringTaskBase task, DateTime now, int hour, int minute, DayOfWeek[] weekdays) {

            // Determine when the task is supposed to run on the current day
            DateTime scheduled = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);

            // Return "false" if we haven't reached the scheduled time yet
            if (now < scheduled) return false;

            // Return "false" if the task is not supposed to run the current day
            if (weekdays != null && weekdays.Length > 0 && !weekdays.Contains(now.DayOfWeek)) return false;
            
            // Get the last run time of the task
            DateTime lastRunTime = GetLastRunTime(task);

            // Return "true" if the last run time is before the schduled time
            return lastRunTime < scheduled;

        }

        public bool ShouldRun(RecurringTaskBase task, int hour) {
            return ShouldRun(task, DateTime.Now, hour, 0, null);
        }

        public bool ShouldRun(RecurringTaskBase task, int hour, int minute) {
            return ShouldRun(task, DateTime.Now, hour, minute, null);
        }
        
        public bool ShouldRun(RecurringTaskBase task, int hour, int minute, params DayOfWeek[] weekdays) {
            return ShouldRun(task, DateTime.Now, hour, minute, weekdays);
        }

        public void SetLastRunTime(RecurringTaskBase task) {

            string name = task.GetType().ToString();

            string path = IOHelper.MapPath($"/App_Data/Skybrud/BorgerDk/Tasks/{name}/LastRunTime.txt");

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            File.WriteAllText(path, "Hello there!", Encoding.UTF8);

        }

        public void AppendToLog(RecurringTaskBase task, StringBuilder stringBuilder) {

            string name = task.GetType().ToString();

            string path = IOHelper.MapPath($"/App_Data/Skybrud/BorgerDk/Tasks/{name}/Log.txt");

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            File.AppendAllText(path, stringBuilder.ToString(), Encoding.UTF8);

        }

        public bool ShouldRun(RecurringTaskBase task, TimeSpan interval) {
            return GetLastRunTimeUtc(task) < DateTime.UtcNow.Subtract(interval);
        }

        #endregion

    }

}