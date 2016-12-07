angular.module('umbraco').controller('Skybrud.BorgerDkGridEditor.Controller', function ($scope) {

    // Prevent the editor configuration from being stripped when Umbraco is saving the page
    $scope.borgerDkConfig = $scope.control.editor.config;

});