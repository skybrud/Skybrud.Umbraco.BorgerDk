using System.Text;
using Skybrud.Essentials.Time;
using Skybrud.Umbraco.BorgerDk.Models.Import;
using Umbraco.Core.Sync;
using Umbraco.Web.Scheduling;

namespace Skybrud.Umbraco.BorgerDk.Scheduling {
    
    public class BorgerDkImportTask : RecurringTaskBase {
        
        private readonly IServerRegistrar _serverRegistrar;
        private readonly BorgerDkService _borgerDkService;
        private readonly BorgerDkImportTaskSettings _importSettings;
        private readonly BorgerDkTaskRunner _runner;

        #region Properties

        public override bool IsAsync => false;

        #endregion

        #region Constructors

        public BorgerDkImportTask(IServerRegistrar serverRegistrar, BorgerDkService borgerDkService, BorgerDkImportTaskSettings importSettings, BorgerDkTaskRunner runner, int delayMilliseconds, int periodMilliseconds) : base(runner, delayMilliseconds, periodMilliseconds) {
            _serverRegistrar = serverRegistrar;
            _importSettings = importSettings;
            _borgerDkService = borgerDkService;
            _runner = runner;
        }

        #endregion

        #region Member methods

        public override bool PerformRun() {

            switch (_importSettings.State) {
                
                // If the job is disabled, we return right away
                case BorgerDkImportTaskState.Disabled:
                    return true;
                
                // If the state is set to "Auto", we check the current role of the server
                case BorgerDkImportTaskState.Auto: {
                    ServerRole role = _serverRegistrar.GetCurrentServerRole();
                    if (role == ServerRole.Replica || role == ServerRole.Unknown) return true;
                    break;
                }

            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(EssentialsTime.Now.Iso8601);

            if (!_runner.ShouldRun(this, _importSettings.ImportInterval)) {
                sb.AppendLine("> Exiting as now supposed to run yet.");
                _runner.AppendToLog(this, sb);
                return true;
            }
            
            // Run a new import
            ImportJob result = _borgerDkService.Import();
            
            // Save the result to the disk
            if (_importSettings.LogResults) _borgerDkService.WriteToLog(result);

            // Make sure we save that the job has run
            _runner.SetLastRunTime(this);

            // Write a bit to the log
            sb.AppendLine($"> Import finished with status {result.Status}.");
            _runner.AppendToLog(this, sb);

            return true;

        }

        #endregion

    }

}