angular.module('umbraco').directive('skybrudBorgerDk', ['$http', 'notificationsService', 'dialogService', function ($http, notificationsService, dialogService) {
    return {
        scope: {
            value: '=',
            config: '='
        },
        transclude: true,
        restrict: 'E',
        replace: true,
        templateUrl: '/App_Plugins/Skybrud.BorgerDk/Views/BorgerDkDirective.html',
        link: function (scope) {

            scope.microArticles = 0;
            scope.blocks = 0;

            // Generate a unique ID for the control
            scope.id = ('borgerdk' + Math.random()).replace('.', '');

            // Check whether we have a valid configuration
            scope.isConfigured = scope.config && scope.config.municipality;
            if (!scope.isConfigured) {
                notificationsService.error('Borger.dk', 'Ingen kommune angivet for Borger.dk-element.');
                return;
            }

            if (!scope.value) scope.value = { id: 0, url: '', selected: [] };
            if (!scope.value.selected) scope.value.selected = [];

            function updateCounts() {

                scope.microArticles = 0;
                scope.blocks = 0;

                angular.forEach(scope.value.selected, function (e) {
                    if (e.indexOf('-') >= 0) {
                        scope.microArticles++;
                    } else {
                        scope.blocks++;
                    }
                });

            }

            scope.edit = function () {


                var d = dialogService.open({
                    modalClass: 'borgerdk-dialog',
                    template: '/App_Plugins/Skybrud.BorgerDk/Views/BorgerDkDialog.html',
                    show: true,
                    animation: false,
                    value: scope.value,
                    config: scope.config,
                    callback: function (value) {
                        scope.value = value;
                        updateCounts();
                    }
                });

                d.element[0].style.width = '1000px';
                d.element[0].style.marginLeft = '-500px';


            };

            updateCounts();

            scope.updateCounts = updateCounts;

        }
    };
}]);