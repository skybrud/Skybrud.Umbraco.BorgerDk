using System;
using Skybrud.Essentials.Time;
using Skybrud.Integrations.BorgerDk;
using Umbraco.Core;

namespace Skybrud.Umbraco.BorgerDk.Models {

    public class BorgerDkArticleModel {

        internal BorgerDkArticleDto Dto { get; }

        public int Id => Dto.ArticleId;

        public string Domain => Dto.Domain;

        public BorgerDkMunicipality Municipality { get; }

        public string Title => Dto.Meta.Title;

        public string Header => Dto.Meta.Header;

        public EssentialsTime PublishDate { get; set; }

        public EssentialsTime UpdateDate { get; set; }
        
        public BorgerDkArticleModel(BorgerDkArticleDto dto) {
            Dto = dto;
            Municipality = BorgerDkMunicipality.GetFromCode(dto.Municipality);
            PublishDate = dto.Meta.PublishDate;
            UpdateDate = dto.Meta.UpdateDate;
        }

    }

}