namespace Skybrud.Umbraco.BorgerDk.Models {
    
    public enum BorgerDkArticleTitleType {

        /// <summary>
        /// Indicates that the actual title of the article should be used as the editorial title.
        /// </summary>
        Article,

        /// <summary>
        /// Indicates that the selection should not have an editorial title.
        /// </summary>
        None,

        /// <summary>
        /// Indicates that a custom title entered by the editor should be used as the editorial title.
        /// </summary>
        Custom

    }

}