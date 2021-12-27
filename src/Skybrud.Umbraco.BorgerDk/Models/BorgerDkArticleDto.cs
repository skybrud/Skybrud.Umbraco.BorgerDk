using Newtonsoft.Json;
using NPoco;
using Skybrud.Essentials.Json;
using Skybrud.Integrations.BorgerDk;
using System;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Skybrud.Umbraco.BorgerDk.Models {

    [ExplicitColumns]
    [TableName(BorgerDkArticleSchema.TableName)]
    [PrimaryKey("Id", AutoIncrement = false)]
    public class BorgerDkArticleDto {

        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        public string Id { get; set; }

        [Column("ArticleId")]
        public int ArticleId { get; set; }

        [Column("Domain")]
        public string Domain { get; set; }

        [Column("Municipality")]
        public int Municipality { get; set; }

        [Ignore]
        public BorgerDkArticle Meta { get; set; }

        [Column("Meta")]
        public string MetaJson {
            get => JsonConvert.SerializeObject(Meta);
            set => Meta = string.IsNullOrWhiteSpace(value) ? null : JsonUtils.ParseJsonObject(value, x => new BorgerDkArticle(x));
        }

        [Column("CreateDate")]
        public DateTime CreateDate { get; set; }

        [Column("UpdateDate")]
        public DateTime UpdateDate { get; set; }

        public BorgerDkArticleDto() { }

        public BorgerDkArticleDto(BorgerDkArticle article) {
            Id = article.Domain + "_" + article.Municipality.Code + "_" + article.Id;
            ArticleId = article.Id;
            Domain = article.Domain;
            Meta = article;
            Municipality = article.Municipality.Code;
            CreateDate = DateTime.UtcNow;
            UpdateDate = DateTime.UtcNow;
        }

    }

}