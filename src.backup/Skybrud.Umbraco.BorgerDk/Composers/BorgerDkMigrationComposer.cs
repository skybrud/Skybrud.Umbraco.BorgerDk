using Skybrud.Umbraco.BorgerDk.Components;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Skybrud.Umbraco.BorgerDk.Composers {

    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class BorgerDkMigrationComposer : IUserComposer {

        public void Compose(Composition composition) {
            composition.Components().Append<BorgerDkMigrationComponent>();
        }

    }

}