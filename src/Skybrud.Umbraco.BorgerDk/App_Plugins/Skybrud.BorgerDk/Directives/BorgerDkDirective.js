angular.module('umbraco').directive('skybrudBorgerDk', ['$http', 'notificationsService', 'dialogService', function ($http, notificationsService, dialogService) {
    return {
        scope: {
            value: '=',
            config: '='
        },
        transclude: true,
        restrict: 'E',
        replace: true,
        templateUrl: '/App_Plugins/Skybrud.BorgerDk/Views/Directive.html',
        link: function (scope) {

            // Generate a unique ID for the control
            scope.id = ('borgerdk' + Math.random()).replace('.', '');

            // Check whether we have a valid configuration
            scope.isConfigured = scope.config && scope.config.municipality;
            if (!scope.isConfigured) {
                notificationsService.error('Borger.dk', 'Ingen kommune angivet for Borger.dk-element.');
                return;
            }

            // Set initial values
            var url = '';
            scope.url = '';
            scope.article = null;
            scope.loading = false;

            // Add the specified ID to the selection
            function addSelected(id) {
                if (scope.value.selected) {
                    var index = scope.value.selected.indexOf(id);
                    if (index == -1) scope.value.selected.push(id);
                } else {
                    scope.value.selected = [id];
                }
            }

            // Removes the specified ID to the selection
            function removeSelected(id) {
                if (scope.value.selected) {
                    var index = scope.value.selected.indexOf(id);
                    if (index >= 0) scope.value.selected.splice(index, 1);
                } else {
                    scope.value.selected = [];
                }
            }

            // Gets whether the element with the specified ID is selected
            function isSelected(id) {
                return scope.value.selected.indexOf(id) >= 0;
            }

            // Resets the value of the property/control
            scope.reset = function () {
                scope.article = null;
                scope.value = {
                    id: 0,
                    url: '',
                    selected: []
                };
            };

            // Updates the value of the property/control
            scope.update = function (a) {
                if (typeof (a) == 'string') {
                    url = scope.url = a;
                } else if (a) {
                    url = scope.url = a.url;
                    scope.value = {
                        id: a.id,
                        url: a.url,
                        domain: a.domain,
                        municipality: a.municipality,
                        title: a.title,
                        header: a.header,
                        selected: scope.value.selected || []
                    };
                }
            };

            // Triggered when the value of a checkbox is changed
            scope.updateSelected = function(element) {
                var id = element.id || element.type;
                if (element.selected) {
                    addSelected(id);
                } else {
                    removeSelected(id);
                }
            };

            scope.reload = function () {
                console.log('reload article: ' + scope.url);
                if (scope.url) {
                    scope.loadArticle(scope.url, true, true);
                } else {
                    scope.reset();
                }
            };

            scope.search = function() {
                var d = dialogService.open({
                    modalClass: 'BorgerDkDialog',
                    template: '/App_Plugins/Skybrud.BorgerDk/Views/ArticlesDialog.html',
                    show: true,
                    callback: function(article) {
                        scope.loadArticle(article.url, false);
                    }
                });

                d.element[0].style.width = '1000px';
                d.element[0].style.marginLeft = '-500px';

            };

            if (!scope.value) {
                scope.reset();
            } else {
                scope.update(scope.value.url);
            }

            scope.loadArticle = function (articleUrl, keepSelected, hardRefresh) {

                scope.loading = true;

                if (!keepSelected) {
                    scope.value.selected = [];
                }

                $http({
                    url: '/umbraco/backoffice/api/BorgerDk/GetArticleFromUrl',
                    method: 'GET',
                    params: {
                        municipalityId: scope.config.municipality,
                        url: articleUrl,
                        cache: hardRefresh ? 0 : 1
                    }
                }).success(function (res) {

                    scope.loading = false;
                    scope.article = res;
                    scope.article.micro = [];
                    scope.article.other = [];
                    angular.forEach(scope.article.elements, function(element) {
                        if (element.type == 'kernetekst') {
                            scope.article.micro = element.microArticles;
                            angular.forEach(scope.article.micro, function (micro) {
                                micro.selected = isSelected(micro.id);
                            });
                        } else {
                            element.selected = isSelected(element.type);
                            scope.article.other.push(element);
                        }
                    });

                    scope.update(scope.article);

                }).error(function (res) {
                    scope.loading = false;
                    console.log(res);
                });
            };

            scope.urlChanged = function () {

                scope.url = $.trim(scope.value.url.split('?')[0]);

                if (scope.url == url) return;

                url = scope.url;

                if (url) {
                    scope.loadArticle(url, false);
                } else {
                    scope.reset();
                }

            };

            if (url) {
                scope.loadArticle(url, true);
            }

        }
    };
}]);