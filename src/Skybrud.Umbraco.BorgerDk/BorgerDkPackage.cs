using System;
using Umbraco.Cms.Core.Semver;

namespace Skybrud.Umbraco.BorgerDk {

    /// <summary>
    /// Static class with various information and constants about the package.
    /// </summary>
    public static class BorgerDkPackage {

        /// <summary>
        /// Gets the alias of the package.
        /// </summary>
        public const string Alias = "Skybrud.Umbraco.BorgerDk";

        /// <summary>
        /// Gets the friendly name of the package.
        /// </summary>
        public const string Name = "Skybrud Borger.dk";

        /// <summary>
        /// Gets the version of the package.
        /// </summary>
        public static readonly Version Version = typeof(BorgerDkPackage).Assembly.GetName().Version;

        /// <summary>
        /// Gets the semantic version of the package.
        /// </summary>
        public static readonly SemVersion SemVersion = new SemVersion(Version.Major, Version.Minor, Version.Build);

    }

}