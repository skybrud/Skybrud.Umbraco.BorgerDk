angular.module("umbraco").controller("Skybrud.BorgerDk.SearchOverlay.Controller", function ($scope, $element, $http, $timeout, borgerDkService, editorService, notificationsService) {

    // Borger.dk doesn't appear to have made any changes to the amount of
    // endpoints or their domains in years, so for now they are hardcoded here
    $scope.endpoints = [
        {
            domain: "www.borger.dk",
            name: "Borger.dk"
        },
        {
            domain: "lifeindenmark.borger.dk",
            name: "Life in Denmark"
        }
    ];

    $scope.articles = [];

    $scope.loading = false;

    $scope.params = {
        text: ""
    };

    function search(text) {

        var params = {};

        if (text) params.text = text;

        console.log(text, params);

        $scope.loading = true;

        $http.get("/umbraco/backoffice/Skybrud/BorgerDk/GetArticles", {params: params}).then(function (r) {
            $scope.articles = r.data;
            $scope.loading = false;
        });

    }


    search("");

    var wait = null;

    $scope.textChanged = function () {

        if (wait) $timeout.cancel(wait);

        // Add a small delay so we dont call the API on each keystroke
        wait = $timeout(function () {
            search($scope.params.text);
        }, 300);

    };

    $timeout(function() {
        var input = $element[0].querySelector("input.text");
        if (input) {
            input.focus();
            console.log("Successfully set focus on input");
            console.log(input);
        } else {
            console.log("Input not found.");
        }
    }, 100);

});