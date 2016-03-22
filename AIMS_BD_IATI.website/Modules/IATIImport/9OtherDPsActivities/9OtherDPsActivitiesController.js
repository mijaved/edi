﻿angular.module('iatiDataImporter').controller("9OtherDPsActivitiesController", function ($rootScope, $timeout, $scope, $http, $uibModal, $filter) {
    $rootScope.IsImportFromOtherDP = true;

    $http({
        method: 'GET',
        url: apiprefix + '/api/IATIImport/GetAssignedActivities',
        params: { dp: $rootScope.selectedFundSource.ID }
    }).success(function (result) {
        $scope.AssignedActivities = result.AssignedActivities;
        $scope.Projects = result.Projects;
        $scope.TrustFunds = result.TrustFunds;

        console.log(result);
    });


    $scope.OpenTFnCF = function (activity) {
        var modalInstance = $uibModal.open({
            animation: true,
            backdrop: false,
            templateUrl: '9TFnCF/9TFnCFView.html',
            controller: '9TFnCFController',
            size: 'lg',
            windowClass: 'full-modal-window',
            resolve: {
                Activity: function () {

                    return activity;
                },
                Project: function () {
                    var project = $filter('filter')($scope.Projects, { ProjectId: activity.MappedProjectId })[0];
                    return project;
                }
            }
        });

        //modalInstance.result.then(function (selectedItem) {
        //    $scope.selected = selectedItem;
        //}, function () {
        //    //$log.info('Modal dismissed at: ' + new Date());
        //});
    };
});