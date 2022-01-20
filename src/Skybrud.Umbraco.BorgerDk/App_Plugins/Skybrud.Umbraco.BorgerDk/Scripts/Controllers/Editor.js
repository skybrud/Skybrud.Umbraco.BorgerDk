angular.module("umbraco").controller("Skybrud.BorgerDk.Controller", function ($scope, borgerDkService, editorService) {

    $scope.edit = function () {
        borgerDkService.editArticle($scope.model.value, $scope.model.config, function (model) {
            $scope.model.value = model.value;
            update();
            editorService.close();
        });
    };

    $scope.reset = function() {
        $scope.model.value = null;
    };

    function update() {

        var kernetekst = false;
        var microArticles = 0;
        var blocks = 0;

        if (!$scope.model.value) return;
        if (!$scope.model.value.selection) return;

        $scope.model.value.selection.forEach(function(id) {
            if (id === "kernetekst") {
                kernetekst = true;
            } else if (id.length === 36) {
                microArticles++;
            } else {
                blocks++;
            }
        });


        var temp = [];

        if (kernetekst) {
            temp.push("alle mikroartikler");
        } else if (microArticles === 1) {
            temp.push("èn mikroartikel");
        } else if (microArticles > 0) {
            temp.push(microArticles + " mikroartikler");
        }

        if (blocks === 1) {
            temp.push("én boks");
        } else if (blocks > 1) {
            temp.push(blocks + " bokse");
        }

        temp = temp.join(" og ");

        $scope.summary = temp[0].toUpperCase() + temp.slice(1);

    }

    update();

});