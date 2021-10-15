using Skybrud.Umbraco.BorgerDk.Scheduling;
using Umbraco.Core.Composing;
using Umbraco.Core.Sync;

namespace Skybrud.Umbraco.BorgerDk.Components {
    
    public class BorgerDkTaskRunnerComponent : IComponent {
        
        private readonly IServerRegistrar _serverRegistrar;
        private readonly BorgerDkTaskRunner _taskRunner;
        private readonly BorgerDkService _borgerDkService;
        private readonly BorgerDkImportTaskSettings _importSettings;

        public BorgerDkTaskRunnerComponent(IServerRegistrar serverRegistrar, BorgerDkTaskRunner taskRunner, BorgerDkService borgerDkService, BorgerDkImportTaskSettings importSettings) {
            _serverRegistrar = serverRegistrar;
            _taskRunner = taskRunner;
            _borgerDkService = borgerDkService;
            _importSettings = importSettings;
        }
        
        public void Initialize() {

            // Convert to milliseconds
            int delayMilliseconds = (int) _importSettings.TaskDelay.TotalMilliseconds;
            int periodMilliseconds = (int) _importSettings.TaskInterval.TotalMilliseconds;

            // Append the task to the task runner
            _taskRunner.Add(new BorgerDkImportTask(_serverRegistrar, _borgerDkService, _importSettings, _taskRunner, delayMilliseconds, periodMilliseconds));

        }

        public void Terminate() { }

    }

}