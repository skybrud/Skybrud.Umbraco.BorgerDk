using Microsoft.Extensions.Logging;
using Skybrud.Essentials.Time;
using Skybrud.Umbraco.BorgerDk.Models.Import;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace Skybrud.Umbraco.BorgerDk.Scheduling {

    public class BorgerDkRecurringHostedService : RecurringHostedServiceBase {
        
        private readonly BorgerDkService _borgerDkService;
        private readonly BorgerDkImportTaskSettings _importSettings;

        private readonly IRuntimeState _runtimeState;
        private readonly IServerRoleAccessor _serverRoleAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;

        private static TimeSpan HowOftenWeRepeat => TimeSpan.FromMinutes(5);
        
        private static TimeSpan DelayBeforeWeStart => TimeSpan.FromMinutes(5);
        
        private const string TaskName = "BorgerDkRecurringHostedService";

        public BorgerDkRecurringHostedService(
            BorgerDkService borgerDkService,
            BorgerDkImportTaskSettings importSettings,
            IRuntimeState runtimeState,
            IServerRoleAccessor serverRoleAccessor,
            IHostingEnvironment hostingEnvironment)
            : base(HowOftenWeRepeat, DelayBeforeWeStart) {
            _borgerDkService = borgerDkService;
            _importSettings = importSettings;
            _runtimeState = runtimeState;
            _serverRoleAccessor = serverRoleAccessor;
            _hostingEnvironment = hostingEnvironment;
        }

        public override Task PerformExecuteAsync(object state) {
            
            // Don't do anything if the site is not running.
            if (_runtimeState.Level != RuntimeLevel.Run) return Task.CompletedTask;

            switch (_importSettings.State) {

                // If the job is disabled, we return right away
                case BorgerDkImportTaskState.Disabled:
                    return Task.CompletedTask;

                // If the state is set to "Auto", we check the current role of the server
                case BorgerDkImportTaskState.Auto: {
                    ServerRole role = _serverRoleAccessor.CurrentServerRole;
                    if (role is ServerRole.Subscriber or ServerRole.Unknown) return Task.CompletedTask;
                    break;
                }

            }

            StringBuilder sb = new();
            sb.AppendLine(EssentialsTime.Now.Iso8601);

            if (!ShouldRun(TaskName, _importSettings.ImportInterval)) {
                sb.AppendLine("> Exiting as not supposed to run yet.");
                AppendToLog(TaskName, sb);
                return Task.CompletedTask;
            }

            // Run a new import
            ImportJob result = _borgerDkService.Import();

            // Save the result to the disk
            if (_importSettings.LogResults) _borgerDkService.WriteToLog(result);

            // Make sure we save that the job has run
            SetLastRunTime(TaskName);

            // Write a bit to the log
            sb.AppendLine($"> Import finished with status {result.Status}.");
            AppendToLog(TaskName, sb);

            return Task.CompletedTask;

        }

        public DateTime GetLastRunTime(string taskName) {

            string[] pathParts = new string[] { Constants.SystemDirectories.Umbraco, "borgerdk", "tasks", $"{taskName}", "lastruntime.text" };

            string path = Path.Combine(pathParts);

            string fullPath = _hostingEnvironment.MapPathContentRoot(path);

            return File.Exists(fullPath) ? File.GetLastWriteTime(fullPath) : DateTime.MinValue;

        }
        
        public DateTime GetLastRunTimeUtc(string taskName) {

            string[] pathParts = new string[] { Constants.SystemDirectories.Umbraco, "borgerdk", "tasks", $"{taskName}", "lastruntime.text" };

            string path = Path.Combine(pathParts);

            string fullPath = _hostingEnvironment.MapPathContentRoot(path); 

            return File.Exists(fullPath) ? File.GetLastWriteTimeUtc(fullPath) : DateTime.MinValue;
        }

        public bool ShouldRun(string taskName, DateTime now, int hour, int minute, DayOfWeek[] weekdays) {

            // Determine when the task is supposed to run on the current day
            DateTime scheduled = new(now.Year, now.Month, now.Day, hour, minute, 0);

            // Return "false" if we haven't reached the scheduled time yet
            if (now < scheduled) return false;

            // Return "false" if the task is not supposed to run the current day
            if (weekdays != null && weekdays.Length > 0 && !weekdays.Contains(now.DayOfWeek)) return false;

            // Get the last run time of the task
            DateTime lastRunTime = GetLastRunTime(taskName);

            // Return "true" if the last run time is before the schduled time
            return lastRunTime < scheduled;

        }

        public bool ShouldRun(string taskName, int hour) {
            return ShouldRun(taskName, DateTime.Now, hour, 0, null);
        }

        public bool ShouldRun(string taskName, int hour, int minute) {
            return ShouldRun(taskName, DateTime.Now, hour, minute, null);
        }

        public bool ShouldRun(string taskName, int hour, int minute, params DayOfWeek[] weekdays) {
            return ShouldRun(taskName, DateTime.Now, hour, minute, weekdays);
        }

        public void SetLastRunTime(string taskName) {

            string[] pathParts = new string[] { Constants.SystemDirectories.Umbraco, "borgerdk", "tasks", $"{taskName}", "lastruntime.text" };

            string path = Path.Combine(pathParts);

            string fullPath = _hostingEnvironment.MapPathContentRoot(path);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllText(fullPath, "Hello there!", Encoding.UTF8);

        }

        public void AppendToLog(string taskName, StringBuilder stringBuilder) {

            string[] pathParts = new string[] { Constants.SystemDirectories.Umbraco, "borgerdk", "tasks", $"{taskName}", "log.text" };

            string path = Path.Combine(pathParts);

            string fullPath = _hostingEnvironment.MapPathContentRoot(path);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            File.AppendAllText(fullPath, stringBuilder.ToString(), Encoding.UTF8);

        }

        public bool ShouldRun(string taskName, TimeSpan interval) {
            return GetLastRunTimeUtc(taskName) < DateTime.UtcNow.Subtract(interval);
        }

    }

}