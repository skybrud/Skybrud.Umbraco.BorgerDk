angular.module("umbraco").controller("Skybrud.BorgerDkAllowedTypesPreValueEditor.Controller", function ($scope, borgerDkService) {

    $scope.all = true;

    $scope.types = borgerDkService.getTypes();

    if ($scope.model.value) {
        var selected = ($scope.model.value + '').split(',');
        angular.forEach($scope.types, function (type) {
            type.selected = selected.indexOf(type.alias) >= 0;
            if (type.selected) $scope.all = false;
        });
    } else {
        $scope.model.value = 'all';
    }

    $scope.update = function() {
        
        if ($scope.all) {
            $scope.model.value = 'all';
            return;
        }

        var temp = [];
        angular.forEach($scope.types, function (type) {
            if (type.selected) temp.push(type.alias);
        });
        $scope.model.value = temp.length == 0 ? 'none' : temp.join(',');

    };

});