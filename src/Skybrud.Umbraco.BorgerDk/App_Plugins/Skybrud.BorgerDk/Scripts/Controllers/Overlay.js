angular.module("umbraco").controller("Skybrud.BorgerDk.Overlay.Controller", function ($scope, $element, $http, $timeout, borgerDkService, editorService, notificationsService) {

    // Append the "BorgerDkOverlay" class to the editor so we can target it via CSS
    var editor = $element.closest(".umb-editor");
    if (editor) editor[0].classList.add("BorgerDkOverlay");

    if (!$scope.model.value || typeof $scope.model.value !== "object") {
        $scope.model.value = { url: "" };
    }

    var wait = null;

    $scope.validUrl = false;
    
    $scope.changed = function() {

        if (wait) $timeout.cancel(wait);

        if (!$scope.model.value || !$scope.model.value.url) {
            $scope.reset();
            return;
        }

        if ($scope.model.value.url.indexOf("borger.dk") === -1) {
            $scope.reset();
            return;
        }

        $scope.validUrl = true;

        // Add a small delay so we dont call the API on each keystroke
        wait = $timeout(function () {
            update();
        }, 300);

    };

    function update() {

        // Clear the article if the user entered a URL from another article
        if ($scope.article && $scope.article.url && $scope.article.url !== $scope.model.value.url) {
            $scope.article = null;
        }

        $scope.loading = true;

        var params = {
            url: $scope.model.value.url,
            municipality: $scope.model.config.municipality
        };

        $http.get("/umbraco/backoffice/Skybrud/BorgerDk/GetArticleByUrl", { params: params }).then(function (r) {

            setArticle(r.data);

            $scope.loading = false;

        }, function (r) {

            notificationsService.error(r.data.Message || "Nope");

            $scope.loading = false;

        });

    }

    function setArticle(article) {

        if (!article) {

            $scope.article = null;
            $scope.elements = [];

            delete $scope.model.value.id;
            delete $scope.model.value.domain;
            delete $scope.model.value.municipality;
            delete $scope.model.value.title;
            delete $scope.model.value.header;
            delete $scope.model.value.byline;
            delete $scope.model.value.selection;

            return;

        }

        var selection = $scope.article && $scope.article.id === article.id ? $scope.model.value.selection : [];
 
        $scope.article = article;

        $scope.model.value = {
            id: article.id,
            url: $scope.model.value.url,
            domain: article.domain,
            municipality: article.municipality,
            title: article.title,
            header: article.header,
            byline: article.byline,
            selection: selection
        };

        $scope.elements = [];

        article.elements.forEach(function (el) {

            if (!$scope.model.config.allowedTypes || $scope.model.config.allowedTypes && ($scope.model.config.allowedTypes.length === 0 || $scope.model.config.allowedTypes.indexOf(el.id) >= 0)) {

                $scope.elements.push(el);

                if (selection.indexOf(el.id) >= 0) el.selected = true;

                if (el.id === "kernetekst") {
                    el.microArticles.forEach(function (m) {
                        if (selection.indexOf(m.id) >= 0) m.selected = true;
                    });
                }

            }

        });

    }

    $scope.reset = function () {
        $scope.validUrl = false;
        setArticle(null);
    };

    $scope.reload = function() {
        if ($scope.model.value && $scope.model.value.url) update();
    };

    $scope.toggle = function(element) {

        // Toggle the selection
        element.selected = !element.selected;

        // Update the "selection" array in the value
        var temp = [];
        $scope.elements.forEach(function (el) {
            if (el.selected) temp.push(el.id);
            if (el.id === "kernetekst") {
                el.microArticles.forEach(function (micro) {
                    if (micro.selected) temp.push(micro.id);
                });
            }
        });
        $scope.model.value.selection = temp;

    };

    $scope.search = function() {
        editorService.open({
            title: "Indsæt artikel",
            //size: "medium",
            view: "/App_Plugins/Skybrud.Borgerdk/Views/SearchOverlay.html",
            submit: function (article) {
                
                $scope.model.value = {
                    url: article.url
                };

                update();

                editorService.close();

            },
            close: function () {
                editorService.close();
            }
        });
    };

    if ($scope.model.value && $scope.model.value.url) {

        // As we haven't pulled information about the article from the API yet,
        // we must initialize a dummy article so the selection isn't overwritten
        $scope.article = { id: $scope.model.value.id };

        $scope.changed();

    }

});