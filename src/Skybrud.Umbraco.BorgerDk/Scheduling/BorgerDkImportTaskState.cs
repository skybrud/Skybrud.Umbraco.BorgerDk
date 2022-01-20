using Umbraco.Cms.Core.Sync;

namespace Skybrud.Umbraco.BorgerDk.Scheduling {

    /// <summary>
    /// Enum class indicating the state of the import task.
    /// </summary>
    public enum BorgerDkImportTaskState {

        /// <summary>
        /// Indiciates that the state is automatically resolved based on the current server role. If the server role is
        /// either <see cref="ServerRole.Subscriber"/> or <see cref="ServerRole.Unknown"/>, the task will not run.
        /// </summary>
        Auto,

        /// <summary>
        /// Indicates that the task is enabled.
        /// </summary>
        Enabled,
        
        /// <summary>
        /// Indicates that the task is disabled.
        /// </summary>
        Disabled

    }

}