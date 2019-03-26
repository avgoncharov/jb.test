(
	function (angular) {
		"use strict";
		angular.module("tstfeed").component(
			"packageInfo",
			{
				templateUrl: "app/components/package-info/package-info.template.html?v=" + angular.module('tstfeed').info().version,
				controller: ["packageService", packageInfoController],
				bindings: {
					modalInstance: "<",
					resolve: "<"
				}
			});

		function packageInfoController(packageService) {
			const ctrl = this;
			ctrl.selected = "";
			ctrl.versions = [{ Id: 1 , Version: "1111"}];
		
			ctrl.$onInit = function () {
				ctrl.Id = ctrl.resolve.Id;
				packageService.findById(ctrl.resolve.Id,
					ctrl.successVersionsHandler,
					ctrl.errorHandler);
			};

			ctrl.successVersionsHandler = function(result) {
				ctrl.versions = result;
				var selected = _.find(ctrl.versions, function (itr) { return itr.IsLatestVersion; });
				ctrl.selected = selected.Version;

				ctrl.versionChanged();
			};

			ctrl.errorHandler = function (err) {
				console.log(err);
			};

			ctrl.versionChanged = function () {
				packageService.getRawMetadata(ctrl.resolve.Id,
					ctrl.selected,
					function(result) {
						 ctrl.RawData = result.Data;
					},
					ctrl.errorHandler);
			};

			ctrl.close = function () {
				ctrl.modalInstance.close({ dialogResult: "cancel" });
			};

		}
	}
)(window.angular);