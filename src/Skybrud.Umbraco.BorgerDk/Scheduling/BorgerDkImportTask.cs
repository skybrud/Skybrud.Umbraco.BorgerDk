using System;
using System.Text;
using System.Threading.Tasks;
using Skybrud.Essentials.Time;
using Skybrud.Essentials.Umbraco.Scheduling;
using Skybrud.Umbraco.BorgerDk.Models.Import;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HostedServices;

#pragma warning disable 1591

namespace Skybrud.Umbraco.BorgerDk.Scheduling {

    public class BorgerDkImportTask : RecurringHostedServiceBase {

        private readonly BorgerDkService _borgerDkService;
        private readonly BorgerDkImportTaskSettings _importSettings;
        private readonly TaskHelper _taskHelper;

        private static TimeSpan HowOftenWeRepeat => TimeSpan.FromMinutes(5);

        private static TimeSpan DelayBeforeWeStart => TimeSpan.FromMinutes(5);

        public BorgerDkImportTask(BorgerDkService borgerDkService, BorgerDkImportTaskSettings importSettings, TaskHelper taskHelper) : base(HowOftenWeRepeat, DelayBeforeWeStart) {
            _borgerDkService = borgerDkService;
            _importSettings = importSettings;
            _taskHelper = taskHelper;
        }

        public override Task PerformExecuteAsync(object state) {

            // Don't do anything if the site is not running.
            if (_taskHelper.RuntimeLevel != RuntimeLevel.Run) return Task.CompletedTask;

            switch (_importSettings.State) {

                // If the job is disabled, we return right away
                case BorgerDkImportTaskState.Disabled:
                    return Task.CompletedTask;

                // If the state is set to "Auto", we check the current role of the server
                case BorgerDkImportTaskState.Auto: {
                        ServerRole role = _taskHelper.ServerRole;
                        if (role is ServerRole.Subscriber or ServerRole.Unknown) return Task.CompletedTask;
                        break;
                    }

            }

            StringBuilder sb = new();
            sb.AppendLine(EssentialsTime.Now.Iso8601);

            if (!_taskHelper.ShouldRun(this, _importSettings.ImportInterval)) {
                sb.AppendLine("> Exiting as not supposed to run yet.");
                _taskHelper.AppendToLog(this, sb);
                return Task.CompletedTask;
            }

            // Run a new import
            ImportJob result = _borgerDkService.Import();

            // Save the result to the disk
            if (_importSettings.LogResults) _borgerDkService.WriteToLog(result);

            // Make sure we save that the job has run
            _taskHelper.SetLastRunTime(this);

            // Write a bit to the log
            sb.AppendLine($"> Import finished with status {result.Status}.");
            _taskHelper.AppendToLog(this, sb);

            return Task.CompletedTask;

        }

    }

}