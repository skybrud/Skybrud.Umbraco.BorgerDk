using Skybrud.BorgerDk;
using System;

namespace Skybrud.Umbraco.BorgerDk.Interfaces {
    
    public interface IBorgerDkArticle {
        
        int Id { get; }

        string Domain { get; }

        string Url { get; }

        BorgerDkMunicipality Municipality { get; }

        DateTime Published { get; }

        DateTime Updated { get; }

        string Title { get; }

        string Header { get; }
    
    }

}