﻿/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
/// <reference path="../../Modules/authentication/definitions.d.ts" />
/// <reference path="../IatiImportApp.ts" />


angular.module('iatiDataImporter').controller("9OtherDPsActivitiesController", function ($rootScope: RootScopeModel, $timeout, $scope, $http, $uibModal, $filter) {
    $rootScope.IsImportFromOtherDP = true;

    $http({
        method: 'GET',
        url: apiprefix + '/api/CFnTF/GetAssignedActivities',
        params: { dp: $rootScope.getCookie('selectedFundSource').ID }
    }).success(function (result) {
       $rootScope.AssignedActivities = $scope.AssignedActivities = result.AssignedActivities;
       $rootScope.AimsProjects = $scope.AimsProjects = result.AimsProjects;
       $rootScope.TrustFunds = $scope.TrustFunds = result.TrustFunds;

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
