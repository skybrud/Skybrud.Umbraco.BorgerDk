angular.module("umbraco").directive("skybrudBorgerdkItem", function () {

    return {
        scope: {
            item: "="
        },
        transclude: true,
        restrict: "E",
        replace: true,
        templateUrl: "/App_Plugins/Skybrud.BorgerDk/Views/Directives/Item.html",
        link: function (scope) {

            scope.item.expanded = scope.item.type === "Job" || scope.item.type === "Group" || scope.item.status !== "Completed";

            switch (scope.item.status) {

                case "Completed":

                    switch (scope.item.action) {

                        case "NotModified":
                            scope.item.icon = "icon-check color-grey";
                            break;

                        case "Rejected":
                            scope.item.icon = "icon-stop-hand color-orange";
                            break;

                        default:
                            scope.item.icon = "icon-check color-green";
                            break;

                    }

                    break;

                case "Pending":
                    scope.item.icon = "icon-pause color-grey";
                    break;

                case "Failed":
                    scope.item.icon = "icon-delete color-red";
                    break;

            }

            scope.debug = function () {
                console.log(scope.item);
            };

        }
    };

});