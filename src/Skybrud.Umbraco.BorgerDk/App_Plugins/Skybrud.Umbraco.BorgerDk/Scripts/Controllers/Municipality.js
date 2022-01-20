angular.module("umbraco").controller("Skybrud.BorgerDk.Municipality.Controller", function ($scope, borgerDkService) {

    $scope.municipalities = borgerDkService.getMunicipalities();

    $scope.municipality = $scope.municipalities[0];

    angular.forEach($scope.municipalities, function(m) {
        if (m.id == $scope.model.value) {
            $scope.municipality = m;
        }
    });

    $scope.valueChanged = function() {
        $scope.model.value = $scope.municipality.id;
    };

});