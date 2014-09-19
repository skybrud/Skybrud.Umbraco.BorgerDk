using System.Linq;
using System.Xml.Linq;
using Umbraco.Core;

namespace Skybrud.Umbraco.BorgerDk.Model {

    public class BorgerDkMicroArticlesBlock : IBorgerDkBlock {

        public string Type { get; private set; }
        public BorgerDkMicroArticle[] MicroArticles { get; private set; }

        public static BorgerDkMicroArticlesBlock Parse(XElement xBlock) {

            return new BorgerDkMicroArticlesBlock {
                Type = xBlock.Name.LocalName,
                MicroArticles = (
                    from xMicroArticle in xBlock.Elements("microArticle")
                    select BorgerDkMicroArticle.Parse(xMicroArticle)
                ).WhereNotNull().ToArray()
            };

        }

    }

}