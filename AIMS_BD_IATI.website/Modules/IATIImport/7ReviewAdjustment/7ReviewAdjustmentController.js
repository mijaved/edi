﻿angular.module('iatiDataImporter').controller("7ReviewAdjustmentController", function ($rootScope, $scope, $http, $uibModal) {
    $http({
        method: 'POST',
        url: apiprefix + '/api/IATIImport/GetProjectsToMap',
        data: JSON.stringify($rootScope.GeneralPreference)
    }).success(function (result) {
        $scope.models = result;
    });

    $scope.disbursmentDiff = function (m) {
        var numerator = m.iatiActivity.TotalDisbursment >= m.aimsProject.TotalDisbursment ? m.iatiActivity.TotalDisbursment : m.aimsProject.TotalDisbursment;

        var denominator = m.iatiActivity.TotalDisbursment < m.aimsProject.TotalDisbursment ? m.iatiActivity.TotalDisbursment : m.aimsProject.TotalDisbursment;

        return (numerator / denominator) * 100;
    }
    $scope.commitmentDiff = function (m) {
        var numerator = m.iatiActivity.TotalCommitment >= m.aimsProject.TotalCommitment ? m.iatiActivity.TotalCommitment : m.aimsProject.TotalCommitment;

        var denominator = m.iatiActivity.TotalCommitment < m.aimsProject.TotalCommitment ? m.iatiActivity.TotalCommitment : m.aimsProject.TotalCommitment;

        return (numerator / denominator) * 100;

    }
    $scope.isDiffGT5 = function (mkl) {

        return Math.abs(($scope.disbursmentDiff(mkl) + $scope.disbursmentDiff(mkl)) / 2) > 5;
    }

    $scope.OpenProjectSpecificAdjustment = function (MatchedProject) {
        var modalInstance = $uibModal.open({
            animation: true,
            backdrop: false,
            templateUrl: '8ProjectSpecificAdjustment/8ProjectSpecificAdjustmentView.html',
            controller: '8ProjectSpecificAdjustmentController',
            size: 'lg',
            resolve: {
                MatchedProject: function () {

                    return MatchedProject;
                }
            }
        });

        //modalInstance.result.then(function (selectedItem) {
        //    $scope.selected = selectedItem;
        //}, function () {
        //    //$log.info('Modal dismissed at: ' + new Date());
        //});
    };

    $scope.ImportProjects = function () {
        $http({
            method: 'POST',
            url: apiprefix + '/api/IATIImport/ImportProjects',
            data: JSON.stringify($scope.models)
        }).success(function (result) {
            alert("Projects are imported.");
        });

    }

});


iatiDataImporterApp.directive('commitmentvsdisbursmentchart', function () {
    return {
        restrict: 'EC',
        template: '<div></div>',
        replace: true,
        link: function (scope, elem, attrs) {
            var renderChart = function () {
                elem.html('');
                var series = scope.$eval(attrs.series);
                //if (angular.isUndefined(attrs.series)) {
                //    return;
                //}
                var aimsCommitment = scope.$eval(attrs.aimsCommitment);
                var aimsDisbursment = scope.$eval(attrs.aimsDisbursment);
                var iatiCommitment = scope.$eval(attrs.iatiCommitment);
                var iatiDisbursment = scope.$eval(attrs.iatiDisbursment);

                var config = {
                    chart: {
                        type: 'bar',
                        height: 100,
                        //spacing: [0, 0, 0, 0],
                        margin: [0, 110, 0, 100],
                    },
                    title: {
                        text: 'Comparison for Financial Data',
                        style: { display: 'none' },
                        visible: false
                    },
                    //subtitle: {
                    //    text: 'between AIMS and IATI'
                    //},
                    xAxis: {
                        categories: ['Commitment', 'Disbursement'],
                        title: {
                            text: null
                        }
                    },
                    yAxis: {
                        gridLineWidth: 0,
                        "startOnTick": true,
                        title: null,
                        labels: {
                            enabled: false
                        },
                        visible: false

                    },

                    tooltip: {
                        valueSuffix: ' USD'
                    },
                    plotOptions: {
                        //bar: {
                        //    dataLabels: {
                        //        enabled: true
                        //    }
                        //}
                    },
                    legend: {
                        layout: 'vertical',
                        align: 'right',
                        verticalAlign: 'top',
                        x: 0,
                        y: 0,
                        floating: true,
                        borderWidth: 0,
                        backgroundColor: ((Highcharts.theme && Highcharts.theme.legendBackgroundColor) || '#FFFFFF'),
                        shadow: true
                    },
                    credits: {
                        enabled: false
                    }
                };

                //config.series = series;
                config.series = [{
                    name: 'AIMS',
                    data: [aimsCommitment, aimsDisbursment],
                    color: '#a94442'
                }, {
                    name: 'IATI',
                    data: [iatiCommitment, iatiDisbursment],
                    color: '#3071a9'
                }];

                config.chart.renderTo = elem[0];

                var chart = new Highcharts.Chart(config);

                chart.redraw();
            };

            renderChart();
            //scope.$watch(attrs.uiChart, function () {
            //    renderChart();
            //}, true);

            //scope.$watch(attrs.chartOptions, function () {
            //    renderChart();
            //});
        }
    };
});

