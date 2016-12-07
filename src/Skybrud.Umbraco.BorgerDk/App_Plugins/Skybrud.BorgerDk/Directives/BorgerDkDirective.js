angular.module('umbraco').directive('skybrudBorgerDk', ['$http', 'notificationsService', 'borgerDkService', 'dialogService', function ($http, notificationsService, borgerDkService, dialogService) {
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

            // Initialies the configuration
            function initConfig() {
                
                // Make sure boolean values are always boolean
                scope.config.mergeTypes = scope.config.mergeTypes === true || scope.config.mergeTypes === '1';
                scope.config.allowCustomTitle = scope.config.allowCustomTitle === true || scope.config.allowCustomTitle === '1';

                // Make sure we always have an array of the allowed types
                if (!scope.config.allowedTypes || scope.config.allowedTypes == 'all') {
                    var temp = [];
                    angular.forEach(borgerDkService.getTypes(), function(type) {
                        temp.push(type.alias);
                    });
                    scope.config.allowedTypes = temp;
                } else if (typeof (scope.config.allowedTypes) == 'string') {
                    scope.config.allowedTypes = scope.config.allowedTypes.split(',');
                }

            }

            // Initializes the model/value
            function initValue() {
                
                if (!scope.value) scope.value = { id: 0, url: '', selected: [] };
                if (!scope.value.selected) scope.value.selected = [];

                if (!scope.value.customTitle) {
                    scope.value.customTitle = {
                        type: 'article',
                        value: ''
                    };
                }

            }

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

            initConfig();
            initValue();

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

                        // Since the custom title would otherwise be overwritten, we need to back it up...
                        var customTitle = scope.value.customTitle;

                        // ... then update the model
                        scope.value = value;

                        // ... and then restore the custom title
                        scope.value.customTitle = customTitle;

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