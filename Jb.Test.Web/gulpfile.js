let gulp = require('gulp');

gulp.task('copy_js', function () {
	return gulp.src([
		'node_modules/angular/angular.min.js',
		'node_modules/angular-resource/angular-resource.min.js',
		'node_modules/angular-sanitize/angular-sanitize.min.js',
		'node_modules/lodash/lodash.min.js',
		'node_modules/angular-ui-bootstrap/dist/ui-bootstrap-tpls.js'])
		.pipe(gulp.dest(['Scripts']));
});

gulp.task('copy_bootstrap_js', function () {
	return gulp.src([
			'node_modules/bootstrap/dist/js/*'])
		.pipe(gulp.dest(['Scripts/bootstrap']));
});

gulp.task('bootstrap', function () {
	return gulp.src(['node_modules/bootstrap/dist/css/*', 'node_modules/bootstrap/dist/fonts/*'])
		.pipe(gulp.dest(['Content/bootstrap']));
});

gulp.task('default', gulp.parallel(['copy_js', 'copy_bootstrap_js', 'bootstrap'],function (done) {done();}));
