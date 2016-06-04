var gulp = require("gulp");
var sass = require("gulp-sass");
var concat = require("gulp-concat");
var cssnano = require("gulp-cssnano");
var rename = require("gulp-rename");
var minifyJs = require("gulp-uglify");
var rimraf = require("rimraf");
var prefixer = require("gulp-autoprefixer");


var paths = {
    js: ["wwwroot/js/featherlight.js", "wwwroot/js/featherlight.gallery.js", "wwwroot/js/app/**/*.js"]
};


gulp.task("js", function () {
    return gulp.src(paths.js, { base: "." })
        .pipe(concat("site.min.js"))
        .pipe(minifyJs())
        .pipe(rename("wwwroot/js/site.min.js"))
        .pipe(gulp.dest("."));
});


gulp.task("css", function () {
    return gulp.src("wwwroot/css/site.scss", { base: "." })
        .pipe(sass().on("error", sass.logError))
        .pipe(concat("site.min.css"))
        .pipe(prefixer({ browsers: ["last 3 versions"] }))
        .pipe(cssnano())
        .pipe(rename("wwwroot/css/site.min.css"))
        .pipe(gulp.dest("."));
});


gulp.task("watch", function () {
    gulp.watch("wwwroot/css/*.scss", ["css"]);
    gulp.watch(paths.js, ["js"]);
});

gulp.task("default", ["js", "css", "watch"]);
