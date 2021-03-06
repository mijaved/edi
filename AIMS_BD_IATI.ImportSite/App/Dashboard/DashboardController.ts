﻿/// <reference path="../../scripts/typings/angularjs/angular.d.ts" />
/// <reference path="../iatiimportapp.ts" />

angular.module('iatiDataImporter').controller("DashboardController", function ($rootScope, $scope, $http, $uibModal, $timeout) {

    $http({
        method: 'POST',
        url: apiprefix + '/api/Dashboard/GetDashboardData',

        data: JSON.stringify($rootScope.getCookie('selectedFundSource'))

    }).success(function (result) {
        $scope.model = result;
    });


    $scope.OpenAdjustImportPreferences = function () {
        var modalInstance = $uibModal.open({
            animation: true,
            backdrop: false,
            templateUrl: 'Dashboard/ImportPreferenceModal.html',
            controller: '6GeneralPreferencesController',
            size: 'lg',
        });

    };

    $scope.OpenTrustFund = function (trustfunddetail) {
        var modalInstance = $uibModal.open({
            animation: true,
            backdrop: false,
            templateUrl: 'Dashboard/TrustFundModal.html',
            controller: '9CofinancingController',
            size: 'lg',
            resolve: {
                model: function () {

                    return { TrustFundDetails: [trustfunddetail] };
                }
            }
        });

    };
    $scope.OpenCofinancing = function (aimsproject) {
        var modalInstance = $uibModal.open({
            animation: true,
            backdrop: false,
            templateUrl: 'Dashboard/CofinancingModal.html',
            controller: '9CofinancingController',
            size: 'lg',
            resolve: {
                model: function () {

                    return {
                        AimsProjects: [aimsproject],
                    };
                }
            }
        });

    };


    $scope.OpenMergeConflictAlert = function (log) {
        var modalInstance = $uibModal.open({
            animation: true,
            backdrop: false,
            templateUrl: 'MergeConflictAlert/MergeConflictAlertView.html',
            controller: 'MergeConflictAlertController',
            size: 'lg',
            resolve: {
                log: function () {

                    return log;
                }
            }
        });

        modalInstance.result.then(function (selectedItem) {
            var index = $scope.model.FinancialDataMismatchedLogs.indexOf(selectedItem, 0);
            if (index > -1) {
                $scope.model.FinancialDataMismatchedLogs.splice(index, 1);
            }
        }, function () {
            //$log.info('Modal dismissed at: ' + new Date());
        });
    };

    $scope.RecallDelegatedActivity = function (da) {

        if (da.IsProccessed == true) {
            document.getElementById('btnRecallExplanation').click(); //Show explanation
        }
        else {
            $http({
                method: 'POST',
                url: apiprefix + '/api/Dashboard/RecallDelegatedActivity',

                data: JSON.stringify(da)

            }).success(function (result) {
                $scope.model.DelegatedActivities = result;
            });
        }
    };

    $scope.timeSince = function (date) {
        if (date == null || date == undefined) return '';

        if (typeof date !== 'object') {
            date = new Date(date);
        }
        var di: any = new Date();
        var seconds = Math.floor(Math.abs(di - date) / 1000);
        var intervalType;

        var interval = Math.floor(seconds / 31536000);
        if (interval >= 1) {
            intervalType = 'year';
        } else {
            interval = Math.floor(seconds / 2592000);
            if (interval >= 1) {
                intervalType = 'month';
            } else {
                interval = Math.floor(seconds / 86400);
                if (interval >= 1) {
                    intervalType = 'day';
                } else {
                    interval = Math.floor(seconds / 3600);
                    if (interval >= 1) {
                        intervalType = "hour";
                    } else {
                        interval = Math.floor(seconds / 60);
                        if (interval >= 1) {
                            intervalType = "minute";
                        } else {
                            interval = seconds;
                            intervalType = "second";
                        }
                    }
                }
            }
        }

        if (interval > 1 || interval === 0) {
            intervalType += 's';
        }

        return interval + ' ' + intervalType + ' ago';
    };

});

angular.module('iatiDataImporter').filter('sumByKey', function () {
    return function (data, key) {
        if (typeof (data) === 'undefined' || typeof (key) === 'undefined') {
            return 0;
        }

        var sum = 0;
        for (var i = data.length - 1; i >= 0; i--) {
            sum += parseFloat(data[i][key]);
        }

        return sum;
    };
});