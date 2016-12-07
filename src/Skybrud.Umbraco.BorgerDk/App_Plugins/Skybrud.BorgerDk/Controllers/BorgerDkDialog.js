angular.module("umbraco").controller("Skybrud.BorgerDkDialog.Controller", function ($scope, $http, $timeout, $routeParams, dialogService, notificationsService) {

    $scope.value = angular.copy($scope.dialogOptions.value);
    $scope.config = $scope.dialogOptions.config;

    // Set initial values
    $scope.mode = 'insert';
    $scope.article = null;
    $scope.loading = false;
    $scope.microArticles = 0;
    $scope.blocks = 0;
    $scope.all = 0;

    if (!$scope.value) $scope.value = { id: 0, url: '', selected: [] };
    if (!$scope.value.selected) $scope.value.selected = [];

    // Add the specified ID to the selection
    function addSelected(id) {
        if ($scope.value.selected) {
            var index = $scope.value.selected.indexOf(id);
            if (index == -1) $scope.value.selected.push(id);
        } else {
            $scope.value.selected = [id];
        }
    }

    // Removes the specified ID to the selection
    function removeSelected(id) {
        if ($scope.value.selected) {
            var index = $scope.value.selected.indexOf(id);
            if (index >= 0) $scope.value.selected.splice(index, 1);
        } else {
            $scope.value.selected = [];
        }
    }

    // Updates the UI and counters based on whats selected in "$scope.article.micro" and "$scope.article.other"
    function updateSelection() {
        $scope.value.selected = [];
        if (!$scope.article) {
            updateCounts();
            return;
        }
        angular.forEach($scope.article.micro, function (e) {
            if (e.selected) $scope.value.selected.push(e.id);
        });
        angular.forEach($scope.article.other, function (e) {
            if (e.selected) $scope.value.selected.push(e.type);
        });
        updateCounts();
    }

    // Selects all micro articles
    $scope.selectMicroArticles = function () {
        if (!$scope.article) return;
        angular.forEach($scope.article.micro, function (e) {
            e.selected = true;
        });
        updateSelection();
    };

    // Clears / unselects all micro articles
    $scope.clearMicroArticles = function () {
        if (!$scope.article) return;
        angular.forEach($scope.article.micro, function (e) {
            e.selected = false;
        });
        updateSelection();
    };

    // Toggles all micro articles (if all are selected, they will be cleared - otherwise all will be selected)
    $scope.toggleMicroArticles = function () {
        $scope.microArticles < $scope.article.micro.length ? $scope.selectMicroArticles() : $scope.clearMicroArticles();
    };

    // Selects all blocks (text elements)
    $scope.selectBlocks = function () {
        if (!$scope.article) return;
        angular.forEach($scope.article.other, function (e) {
            e.selected = true;
        });
        updateSelection();
    };

    // Clears / unselects all blocks (text elements)
    $scope.clearBlocks = function () {
        if (!$scope.article) return;
        angular.forEach($scope.article.other, function (e) {
            e.selected = false;
        });
        updateSelection();
    };

    // Toggles all blocks (text elements) (if all are selected, they will be cleared - otherwise all will be selected)
    $scope.toggleBlocks = function () {
        $scope.blocks < $scope.article.other.length ? $scope.selectBlocks() : $scope.clearBlocks();
    };

    // Toggles all micro articles and blocks (if all are selected, they will be cleared - otherwise all will be selected)
    $scope.toggleAll = function () {
        if ($scope.all < $scope.article.all.length) {
            $scope.selectMicroArticles();
            $scope.selectBlocks();
        } else {
            $scope.clearMicroArticles();
            $scope.clearBlocks();
        }
    };

    // Gets whether the element with the specified ID is selected
    function isSelected(id) {
        return $scope.value.selected.indexOf(id) >= 0;
    }

    // Resets the value of the property/control
    $scope.reset = function () {
        $scope.article = null;
        $scope.value = {
            id: 0,
            url: '',
            selected: []
        };
        updateCounts();
    };

    // Update the counters for how many micro articles and blocks that have been selected
    function updateCounts(a) {
        $scope.microArticles = 0;
        $scope.blocks = 0;
        $scope.all = 0;
        if (!a) { a = $scope.article; }
        if (!a) return;
        angular.forEach(a.micro, function (e) {
            if (e.selected) {
                $scope.microArticles++;
                $scope.all++;
            }
        });
        angular.forEach(a.other, function (e) {
            if (e.selected) {
                $scope.blocks++;
                $scope.all++;
            }
        });
    }

    // Updates the value of the property/control
    $scope.update = function (a) {
        $scope.microArticles = 0;
        $scope.blocks = 0;
        if (typeof (a) == 'string') {
            $scope.value = a;
        } else if (a) {
            $scope.value = {
                id: a.id,
                url: a.url,
                domain: a.domain,
                municipality: a.municipality,
                title: a.title,
                header: a.header,
                selected: $scope.value.selected || []
            };
            updateCounts(a);
        }
    };

    // Triggered when the value of a checkbox is changed
    $scope.updateSelected = function (element) {
        var id = element.id || element.type;
        if (element.selected) {
            addSelected(id);
        } else {
            removeSelected(id);
        }
        updateCounts();
    };

    $scope.reload = function () {
        if ($scope.value.url) {
            $scope.loadArticle($scope.value.url, true, true);
        } else {
            $scope.reset();
        }
    };

    $scope.loadArticle = function (articleUrl, keepSelected, hardRefresh) {

        $scope.insertError = null;
        $scope.loading = true;

        if (!keepSelected) {
            $scope.value.selected = [];
        }

        $http({
            url: '/umbraco/backoffice/api/BorgerDk/GetArticleFromUrl',
            method: 'GET',
            params: {
                municipalityId: $scope.config.municipality,
                url: articleUrl,
                cache: hardRefresh ? 0 : 1
            }
        }).success(function (res) {

            $scope.insertError = null;
            $scope.loading = false;
            $scope.article = res;
            $scope.article.micro = [];
            $scope.article.other = [];
            $scope.article.all = [];
            angular.forEach($scope.article.elements, function (element) {

                // Skip the element if not allowed
                if ($scope.config.allowedTypes.indexOf(element.type) == -1) return;

                if (element.type == 'kernetekst') {
                    $scope.article.micro = element.microArticles;
                    angular.forEach($scope.article.micro, function (micro) {
                        micro.type = 'microArticle';
                        micro.selected = isSelected(micro.id);
                        $scope.article.all.push(micro);
                    });
                } else {
                    element.id = element.type;
                    element.selected = isSelected(element.type);
                    $scope.article.other.push(element);
                    $scope.article.all.push(element);
                }

            });

            $scope.update($scope.article);

        }).error(function (res) {
            $scope.loading = false;
            $scope.insertError = res.meta ? res.meta : { error: 'Det skete en fejl i kaldet til serveren.' };
        });

    };

    // Change to search mode (select articles from a list)
    $scope.search = function () {
        $scope.query = '';
        $scope.mode = 'search';
        $scope.updateList();
    };

    // Cancel the current mode. If in "insert" mode, we close the dialog. Otherwise we just jump back to the "insert" mode.
    $scope.cancel = function () {
        if ($scope.mode == 'insert') {
            $scope.close();
        } else {
            $scope.mode = 'insert';
        }
    };

    $scope.confirm = function () {
        $scope.submit($scope.value);
    };

    $scope.url = $scope.value.url || ''; 

    $scope.urlChanged = function () {
        var url = $.trim($scope.value.url.split('?')[0]);
        if ($scope.url == url) return;
        $scope.url = url;
        if (url) {
            $scope.loadArticle(url, false);
        } else {
            $scope.reset();
        }
    };

    if ($scope.value.url) {
        $scope.loadArticle($scope.value.url, true);
    }


    $scope.listError = null;
    $scope.selectedEndpoint = 'wwwborgerdk';
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

    $scope.selectEndpoint = function (endpoint) {
        $scope.selectedEndpoint = endpoint.id;
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
        $scope.listError = null;
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
            $scope.listError = null;
            $scope.loading = false;
            $scope.endpoints = res.data;
            $scope.sorting = res.sorting;
        }).error(function (res) {
            $scope.loading = false;
            $scope.listError = res.meta ? res.meta : { error: 'Det skete en fejl i kaldet til serveren.' };
        });
    };

    $scope.selectArticle = function(article) {
        $scope.loadArticle(article.url, false, false);
        $scope.mode = 'insert';
    };

    // Listen for changes of the input field of the search text
    var wait = null;
    $scope.$watch('query', function () {
        if ($scope.mode != 'search') return;
        if (wait) $timeout.cancel(wait);
        wait = $timeout(function () {
            $scope.updateList();
        }, 300);
    }, true);












});