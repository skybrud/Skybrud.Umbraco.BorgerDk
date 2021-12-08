using System;

namespace Skybrud.Umbraco.BorgerDk.Scheduling {

    /// <summary>
    /// Class representing the settings for the Borger.dk import task. The class is registered with the DI container as
    /// a singleton, and the settings can thereby be modified by a component - ideally running before
    /// <see cref="BorgerDkTaskRunnerComponent"/>.
    /// </summary>
    public class BorgerDkImportTaskSettings {

        #region Properties

        /// <summary>
        /// Gets or sets whether the state for whether the automatic import should run or not. If set to
        /// <see cref="BorgerDkImportTaskState.Enabled"/> the task is enabled, and if set to
        /// <see cref="BorgerDkImportTaskState.Disabled"/> the task is disabled.
        ///
        /// The default value is <see cref="BorgerDkImportTaskState.Auto"/> where the task will check the role of the
        /// server to find out whether the task should run or not. If the server role is either
        /// <see cref="ServerRole.Replica"/> or <see cref="ServerRole.Unknown"/>, the task will not run.
        /// </summary>
        public BorgerDkImportTaskState State { get; set; }

        /// <summary>
        /// Gets or sets the delay before the import task should run the first time after an Umbraco reboot. Default is five minutes.
        /// </summary>
        public TimeSpan TaskDelay { get; set; }

        /// <summary>
        /// Gets or sets the interval between each time the task should run. As the task internally has some logic to determine
        /// whether it should start an import, the task should ideally run very often. Default is five minutes.
        /// </summary>
        public TimeSpan TaskInterval { get; set; }

        /// <summary>
        /// Gets or sets the interval between each time the import should run. Default is 12 hours.
        /// </summary>
        public TimeSpan ImportInterval { get; set; }

        /// <summary>
        /// Gets or sets whether the result of automatic imports should be logged to the disk. This will help debugging
        /// if anything goes wrong, but also take a bit of disk space over time. Default is <c>true</c>.
        /// </summary>
        public bool LogResults { get; set; }

        public string Hej { get; }

        #endregion

        #region Constructors

        public BorgerDkImportTaskSettings() {

            Hej = State.ToString();

            TaskDelay = TimeSpan.FromMinutes(5);
            TaskInterval = TimeSpan.FromMinutes(5);
            ImportInterval = TimeSpan.FromHours(12);
            LogResults = true;

        }

        #endregion

    }

}