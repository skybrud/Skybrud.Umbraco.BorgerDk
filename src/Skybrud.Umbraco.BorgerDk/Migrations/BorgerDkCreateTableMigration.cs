using Skybrud.Umbraco.BorgerDk.Models;
using Umbraco.Cms.Infrastructure.Migrations;

#pragma warning disable 1591

namespace Skybrud.Umbraco.BorgerDk.Migrations {

    public class BorgerDkCreateTableMigration : MigrationBase {

        public BorgerDkCreateTableMigration(IMigrationContext context) : base(context) { }

        protected override void Migrate() {
            if (TableExists(BorgerDkArticleSchema.TableName)) return;
            Create.Table<BorgerDkArticleSchema>().Do();
        }

    }

}