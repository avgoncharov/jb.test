(function (angular) {
	"use strict";

	angular.module("tstfeed").factory("packageService", ["$resource", "$http", packageServiceFactory]);

	function packageServiceFactory($resource, $http) {
		return new PackageService($resource, $http);
	}

	function PackageService($resource, $http) {
		const srvc = this;

		srvc.getAll = function(successHandler, errorHandler) {
			$resource("api/packages").query(successHandler, errorHandler);
		};

		srvc.findById = function(id, successHandler, errorHandler) {
			$resource("api/packages/:id/AllVersion")
				.query({ id: id }, successHandler, errorHandler);
		};

		//srvc.findById = function(id, successHandler, errorHandler) {
		//	$resource("api/packages/:id/LatestVersion")
		//		.get({ id: id }, successHandler, errorHandler);
		//};

		srvc.getRawMetadata = function(id, version, successHandler, errorHandler) {
			$resource("api/packages/:id/:version/raw-metadata")
				.get({ id: id, version: version }, successHandler, errorHandler);
		};

		srvc.findByFilter = function (
			idPattern,
			titlePatter,
			versionPattern,
			descriptionPattern,
			successHandler,
			errorHandler) {
			$http.post('api/packages/find-by-filter', {
				IdPattern: idPattern,
				TitlePattern: titlePatter,
				VersionPattern: versionPattern,
				DescriptionPattern: descriptionPattern
			},
				{}).then(successHandler, errorHandler);
		};

		return srvc;
	}

})(window.angular);