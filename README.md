Skybrud.Umbraco.BorgerDk
========================

This repository covers the integration between our [**Skybrud.Integrations.BorgerDk**](https://github.com/skybrud/Skybrud.Integrations.BorgerDk) package and Umbraco. This gives editors the option to insert Borger.dk articles as part of their content in their own pages in Umbraco.

The `master` branch (the one you're viewing now) contains the Umbraco 7 implementation, while an older implementation for Umbraco 6 can be found in the <code>[v1.x](https://github.com/skybrud/Skybrud.Umbraco.BorgerDk/tree/v1.x)</code> branch.

## List of contents

* [Installation](#installation)
* [Setup](#setup)
    * [In the grid](#in-the-grid)
    * [As a property](#as-a-property)
* [Municipalities](#municipalities)

## Installation

1. [**NuGet Package**][NuGetPackageUrl]  
Install this NuGet package in your Visual Studio project. Makes updating easy.

2. [**Umbraco package**][UmbracoPackageUrl]  
<s>Install this Umbraco package via the developer section in Umbraco.</s>

3. [**ZIP file**][GitHubReleaseUrl]  
Manually unzip and move files to the root directory of your website.

## Setup

### In the grid

This package comes with the option to insert an article as a control in the Umbraco Grid. The control however needs a bit of configuration, so you have to add the editor manually.

The editor requires you to specify the `municipality` to be used, since each munipality has the option for adding specific content to a given article. If you don't know the municipality ID, you can find it [in this list](#municipalities). `630` as shown in the examples below is *Vejle Kommune*.

If you wish to add the editor in a custom `package.manifest` file, it could look like this:

```JSON
{
    "gridEditors": [
        {
            "name": "Borger.dk",
            "alias": "skybrud.borgerdk",
            "view": "/App_Plugins/Skybrud.BorgerDk/Views/BorgerDkGridEditor.html",
            "icon": "icon-school",
            "config": { 
                "municipality": 630
            }
        }
    ]
}
```

You can also just add the editor to the global configuration file located at `~/config/grid.editors.config.js`. In that case you could just append the JSON for the editor to the array of editors:

```JSON
{
    "name": "Borger.dk",
    "alias": "skybrud.borgerdk",
    "view": "/App_Plugins/Skybrud.BorgerDk/Views/BorgerDkGridEditor.html",
    "icon": "icon-school",
    "config": { 
        "municipality": 630
    }
}
```

#### Usage

If using our [Skybrud.Umbraco.GridData-package](https://github.com/skybrud/Skybrud.Umbraco.GridData) for showing the grid on your website, you can use the `BorgerDkGridControlValue` class (which inherits from `BorgerDkArticleSelection` used in the property editor) for the value of the Borger.dk grid control.

In order for the grid package to return a strongly typed model, you should use the default `skybrud.borgerdk` editor alias as shown in the JSON examples above, or specify a custom alias either ending with `.borgerdk` or containing `.borgerdk.` - eg. `mysite.borgerdk` or `mysite.borgerdk.main`. If you use another editor alias, you should add your own grid converter to handle the model.

If you're using our grid package, and already have a reference to the control holding the Borger.dk article, you can access the value shown in the Razor partial view example below (where `Model` is the control):

```C#
@using Skybrud.Umbraco.BorgerDk.Grid.Values
@using Skybrud.Umbraco.BorgerDk.Models.Cached
@inherits UmbracoViewPage<Skybrud.Umbraco.GridData.GridControl>
              
@{

    BorgerDkGridControlValue value = Model.GetValue<BorgerDkGridControlValue>();

    if (!value.HasSelection || !value.Article.Exists) { return; }

    foreach (BorgerDkCachedMicroArticle microArticle in value.MicroArticles) {
        <div class="BorgerDkMicroArticle">
            <h2>@microArticle.Title</h2>
            @Html.Raw(microArticle.Content)
        </div>
    }

    foreach (BorgerDkCachedTextElement block in value.Blocks) {
        <div class="BorgerDkText">
            <h3>@block.Title</h3>
            @Html.Raw(block.Content)
        </div>
    }

}
```

The `value.HasSelection` property indicates whether the value has an article selection. It is necessary to check this since an editor might insert the Borger.dk grid control on a page, and then save the page without actually selecting an article, leaving the selection empty.

The `value.Article` property is a reference to the cached article. When fetching an article from the Borger.dk webservice through Umbraco, it will automatically be saved/cached on the disk for later use. The `value.Article.Exists` then indicates whether the article is actually saved on the disk. If not, the example just returns since we don't have any content to show.

Also in the example, the properties `value.MicroArticles` and `value.Blocks` will only contain the micro articles and text elements that the editor has selected in Umbraco, while all micro articles and text elements of the article can be accessed through `value.Article.MicroArticles` and `value.Article.Blocks`.

### As a property

This will be supported in the future ;)

## Municipalities

| ID | Name |
|----|------|
|580|Aabenraa Kommune|
|851|Aalborg Kommune|
|751|Aarhus Kommune|
|492|Ærø Kommune|
|165|Albertslund Kommune|
|201|Allerød Kommune|
|420|Assens Kommune|
|151|Ballerup Kommune|
|530|Billund Kommune|
|400|Bornholms Regionskommune|
|153|Brøndby Kommune|
|810|Brønderslev Kommune|
|155|Dragør Kommune|
|240|Egedal Kommune|
|561|Esbjerg Kommune|
|430|Faaborg-Midtfyn Kommune|
|563|Fanø Kommune|
|710|Favrskov Kommune|
|320|Faxe Kommune|
|210|Fredensborg Kommune|
|607|Fredericia Kommune|
|147|Frederiksberg Kommune|
|813|Frederikshavn Kommune|
|250|Frederikssund Kommune|
|190|Furesø Kommune|
|157|Gentofte Kommune|
|159|Gladsaxe Kommune|
|161|Glostrup Kommune|
|253|Greve Kommune|
|270|Gribskov Kommune|
|376|Guldborgsund Kommune|
|510|Haderslev Kommune|
|260|Halsnæs Kommune|
|766|Hedensted Kommune|
|217|Helsingør Kommune|
|163|Herlev Kommune|
|657|Herning Kommune|
|219|Hillerød Kommune|
|860|Hjørring Kommune|
|169|Høje-Taastrup Kommune|
|316|Holbæk Kommune|
|661|Holstebro Kommune|
|615|Horsens Kommune|
|223|Hørsholm Kommune|
|167|Hvidovre Kommune|
|756|Ikast-Brande Kommune|
|183|Ishøj Kommune|
|849|Jammerbugt Kommune|
|326|Kalundborg Kommune|
|440|Kerteminde Kommune|
|101|Københavns Kommune|
|259|Køge Kommune|
|621|Kolding Kommune|
|825|Læsø Kommune|
|482|Langeland Kommune|
|350|Lejre Kommune|
|665|Lemvig Kommune|
|360|Lolland Kommune|
|173|Lyngby-Taarbæk Kommune|
|846|Mariagerfjord Kommune|
|410|Middelfart Kommune|
|773|Morsø Kommune|
|370|Næstved Kommune|
|707|Norddjurs Kommune|
|480|Nordfyns Kommune|
|450|Nyborg Kommune|
|727|Odder Kommune|
|461|Odense Kommune|
|306|Odsherred Kommune|
|730|Randers Kommune|
|840|Rebild Kommune|
|760|Ringkøbing-Skjern Kommune|
|329|Ringsted Kommune|
|175|Rødovre Kommune|
|265|Roskilde Kommune|
|230|Rudersdal Kommune|
|741|Samsø Kommune|
|740|Silkeborg Kommune|
|746|Skanderborg Kommune|
|779|Skive Kommune|
|330|Slagelse Kommune|
|269|Solrød Kommune|
|540|Sønderborg Kommune|
|340|Sorø Kommune|
|336|Stevns Kommune|
|671|Struer Kommune|
|479|Svendborg Kommune|
|706|Syddjurs Kommune|
|185|Tårnby Kommune|
|787|Thisted Kommune|
|550|Tønder Kommune|
|187|Vallensbæk Kommune|
|573|Varde Kommune|
|575|Vejen Kommune|
|630|Vejle Kommune|
|820|Vesthimmerlands Kommune|
|791|Viborg Kommune|
|390|Vordingborg Kommune|




[NuGetPackageUrl]: https://www.nuget.org/packages/Skybrud.Umbraco.BorgerDk
[UmbracoPackageUrl]: http://our.umbraco.org/projects/website-utilities/skybrud.umbraco.borgerdk
[GitHubReleaseUrl]: https://github.com/skybrud/Skybrud.Umbraco.BorgerDk/releases/latest
