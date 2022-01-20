angular.module("umbraco").controller("Skybrud.BorgerDk.AllowedTypes.Controller", function ($scope, borgerDkService) {

    $scope.types = borgerDkService.getTypes();

    if (!Array.isArray($scope.model.value)) {
        $scope.model.value = [];
    }

    $scope.all = $scope.model.value.length === 0;

    if ($scope.model.value) {
        $scope.types.forEach(function (type) {
            type.selected = $scope.model.value.indexOf(type.alias) >= 0;
            if (type.selected) $scope.all = false;
        });
    }

    $scope.update = function () {

        if ($scope.all) {
            $scope.model.value = [];
            return;
        }

        var temp = [];
        $scope.types.forEach(function (type) {
            if (type.selected) temp.push(type.alias);
        });
        $scope.model.value = temp;

    };

});