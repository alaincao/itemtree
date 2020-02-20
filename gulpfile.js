
var fs = require('fs');
var gulp = require('gulp');
var concat = require('gulp-concat');
var source = require('vinyl-source-stream');
var buffer = require('vinyl-buffer');
var del = require('del');
var browserify = require('browserify');
var tsify = require('tsify');
var uglify = require('gulp-uglify');
var sourcemaps = require('gulp-sourcemaps');
var sass = require('gulp-sass');

gulp.task( 'site.js', ()=>buildJs('site.js', false) )
gulp.task( 'site.min.js', ()=>buildJs('site.min.js', true) )  // <== NB: Not used but contains additional compilation checks => Leaving it ...
gulp.task( 'style.css', ()=>buildCSS('style.css') );
gulp.task( 'default', gulp.parallel('site.js', 'site.min.js', 'style.css') );

gulp.task( 'clean', function()
{
	return del( [	'./app/bin',
					'./app/obj',
					'./app/wwwroot/js/site*.js',
					'./app/wwwroot/js/site*.map',
					'./app/wwwroot/css/style.css',
					'./app/wwwroot/css/style.css.map',
				] );
} );

function buildJs(fileName, releaseMode)
{
	// cf. https://www.typescriptlang.org/docs/handbook/compiler-options.html
	var tsifyParms = {};
	if( releaseMode )
	{
		tsifyParms.noImplicitAny = true;
		tsifyParms.noUnusedLocals = true;
	}
	var b = browserify({ debug:true })
 				.add( './app/global.d.ts' )
				.add( './app/global.ts' )  // The TypeScript entry point
				.plugin( tsify, tsifyParms );

	var stream = b.bundle()  // Execute Browserify
			.pipe( source(fileName) )  // Destination filename
			.pipe( buffer() )
			.pipe( sourcemaps.init({ loadMaps: true }) );
	if( releaseMode )
		stream = stream
			.pipe( uglify() );
	stream = stream
			.on( 'error', function(error){ console.error(error.toString()); } )
			.pipe( sourcemaps.write('./') )
			.pipe( gulp.dest('./app/wwwroot/js/') );  // Destination directory
	return stream;
};

function buildCSS( fileName )
{
	return gulp.src( './app/src/Views/**/*.scss' )
		.pipe( sourcemaps.init({ loadMaps: true }) )
		.pipe( sass().on('error', sass.logError) )
		.pipe( concat(fileName) )
		.pipe( sourcemaps.write('./') )
		.pipe( gulp.dest('./app/wwwroot/css') );
};
