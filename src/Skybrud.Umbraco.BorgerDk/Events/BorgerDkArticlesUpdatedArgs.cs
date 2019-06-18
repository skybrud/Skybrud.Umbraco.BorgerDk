using Skybrud.BorgerDk;

namespace Skybrud.Umbraco.BorgerDk.Events {

    public class BorgerDkArticlesUpdatedArgs {

        public BorgerDkArticle[] Articles { get; }

        public BorgerDkArticlesUpdatedArgs(BorgerDkArticle[] articles) {
            Articles = articles;
        }

    }

}