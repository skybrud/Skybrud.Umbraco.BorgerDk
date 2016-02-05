angular.module("umbraco").controller("Skybrud.BorgerDkArticlesDialog.Controller", function ($scope, $http, $timeout, $routeParams, dialogService, notificationsService) {

    $scope.loading = true;
    $scope.active = 'wwwborgerdk';
    $scope.query = '';
    $scope.endpoints = [
        {
            id: 'wwwborgerdk',
            name: 'Borger.dk',
            articles: null,
            count: -1
        },
        {
            id: 'lifeindenmarkborgerdk',
            name: 'Life in Denmark',
            articles: null,
            count: -1
        }
    ];

    $scope.sorting = {
        field: 'title',
        order: 'asc'
    };

    $scope.selectEndpoint = function(endpoint) {
        $scope.active = endpoint.id;
    };

    // Uodate the sorting to match "field" and "order"
    $scope.sort = function (field, order) {

        // Update the sorting options
        if (order != 'desc') order = 'asc';
        if (field == $scope.sorting.field) {
            $scope.sorting.order = ($scope.sorting.order == 'asc' ? 'desc' : 'asc');
        } else {
            $scope.sorting.field = field;
            $scope.sorting.order = order;
        }

        // Update the list with the new sorting options
        $scope.updateList();

    };

    // Check the sorting direction (used in the view)
    $scope.isSortDirection = function (field, order) {
        return field == $scope.sorting.field && order == $scope.sorting.order;
    };

    $scope.updateList = function () {
        $scope.loading = true;
        $http({
            url: '/umbraco/backoffice/api/BorgerDk/GetArticleList',
            method: 'GET',
            params: {
                sort: $scope.sorting.field,
                order: $scope.sorting.order,
                query: $scope.query
            }
        }).success(function (res) {
            $scope.loading = false;
            $scope.endpoints = res.data;
            $scope.sorting = res.sorting;
        });
    };

    // Listen for changes of the input field of the search text
    var wait = null;
    $timeout(function() {
        $scope.$watch('query', function () {
            if (wait) $timeout.cancel(wait);
            wait = $timeout(function () {
                $scope.updateList();
            }, 300);
        }, true);
    }, 100);

    $scope.updateList();

});

