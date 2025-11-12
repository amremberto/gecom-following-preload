const { series, src, dest, parallel, watch } = require("gulp");

const autoprefixer = require("gulp-autoprefixer");
const concat = require("gulp-concat");
const CleanCSS = require("gulp-clean-css");
const rename = require("gulp-rename");
const rtlcss = require("gulp-rtlcss");
const sourcemaps = require("gulp-sourcemaps");
const sass = require("gulp-dart-sass");
const terser = require("gulp-terser");

const paths = {
  // Source paths (fuera de wwwroot - no se publican)
  srcScripts: "Scripts/", // Archivos JS fuente
  srcStyles: "Styles/scss/", // Archivos SCSS fuente
  
  // Distribution paths (dentro de wwwroot - se publican)
  distAssets: "wwwroot/", // Directorio base de distribución
  distVendor: "wwwroot/vendor/", // Vendors copiados desde node_modules
  distJS: "wwwroot/js/", // JS compilado
  distCSS: "wwwroot/css/", // CSS compilado
  
  // Node modules
  nodeModules: "node_modules/", // node_modules directory
};

// ============================================
// VENDOR FILES - Copiar archivos necesarios desde node_modules
// ============================================

// Copia archivos individuales de vendors que necesitan estar disponibles
// (por ejemplo, imágenes, fuentes, o archivos que no se pueden concatenar)
const copyVendorAssets = function () {
  // Ejemplo: copiar fuentes de FontAwesome si es necesario
  // return src([
  //   paths.nodeModules + "@fortawesome/fontawesome-free/webfonts/**",
  // ]).pipe(dest(paths.distVendor + "fonts/"));
  
  // Por ahora retornamos vacío, pero puedes agregar archivos específicos aquí
  return Promise.resolve();
};

// ============================================
// VENDOR BUNDLES - Modular por funcionalidad
// ============================================

// Core: Librerías base usadas en todas las páginas
const vendorCore = function () {
  const outCSS = paths.distCSS;
  const outJS = paths.distJS;

  // vendor-core.css
  src([
    paths.nodeModules + "bootstrap/dist/css/bootstrap.min.css",
  ])
    .pipe(concat("vendor-core.css"))
    .pipe(CleanCSS())
    .pipe(rename({ suffix: ".min" }))
    .pipe(dest(outCSS));

  // vendor-core.js
  return src([
    paths.nodeModules + "jquery/dist/jquery.min.js",
    paths.nodeModules + "bootstrap/dist/js/bootstrap.bundle.min.js",
    paths.nodeModules + "iconify-icon/dist/iconify-icon.min.js",
  ])
    .pipe(concat("vendor-core.js"))
    .pipe(terser())
    .pipe(rename({ suffix: ".min" }))
    .pipe(dest(outJS));
};

// Forms: Plugins de formularios avanzados
const vendorForms = function () {
  const outCSS = paths.distCSS;
  const outJS = paths.distJS;

  // vendor-forms.css
  src([
    paths.nodeModules + "flatpickr/dist/flatpickr.min.css",
    paths.nodeModules + "select2/dist/css/select2.min.css",
    paths.nodeModules + "choices.js/public/assets/styles/choices.min.css",
    paths.nodeModules + "quill/dist/quill.core.css",
    paths.nodeModules + "quill/dist/quill.snow.css",
    paths.nodeModules + "quill/dist/quill.bubble.css",
    paths.nodeModules + "dropzone/dist/dropzone.css",
  ])
    .pipe(concat("vendor-forms.css"))
    .pipe(CleanCSS())
    .pipe(rename({ suffix: ".min" }))
    .pipe(dest(outCSS));

  // vendor-forms.js
  return src([
    paths.nodeModules + "flatpickr/dist/flatpickr.min.js",
    paths.nodeModules + "select2/dist/js/select2.min.js",
    paths.nodeModules + "inputmask/dist/inputmask.min.js",
    paths.nodeModules + "choices.js/public/assets/scripts/choices.min.js",
    paths.nodeModules + "quill/dist/quill.js",
    paths.nodeModules + "dropzone/dist/dropzone-min.js",
  ])
    .pipe(concat("vendor-forms.js"))
    .pipe(terser())
    .pipe(rename({ suffix: ".min" }))
    .pipe(dest(outJS));
};

// Tables: DataTables y plugins relacionados
const vendorTables = function () {
  const outCSS = paths.distCSS;
  const outJS = paths.distJS;

  // vendor-tables.css
  src([
    paths.nodeModules +
      "datatables.net-bs5/css/dataTables.bootstrap5.min.css",
    paths.nodeModules +
      "datatables.net-responsive-bs5/css/responsive.bootstrap5.min.css",
    paths.nodeModules +
      "datatables.net-fixedcolumns-bs5/css/fixedColumns.bootstrap5.min.css",
    paths.nodeModules +
      "datatables.net-fixedheader-bs5/css/fixedHeader.bootstrap5.min.css",
    paths.nodeModules +
      "datatables.net-buttons-bs5/css/buttons.bootstrap5.min.css",
    paths.nodeModules +
      "datatables.net-select-bs5/css/select.bootstrap5.min.css",
  ])
    .pipe(concat("vendor-tables.css"))
    .pipe(CleanCSS())
    .pipe(rename({ suffix: ".min" }))
    .pipe(dest(outCSS));

  // vendor-tables.js
  return src([
    paths.nodeModules + "datatables.net/js/dataTables.min.js",
    paths.nodeModules + "datatables.net-bs5/js/dataTables.bootstrap5.min.js",
    paths.nodeModules +
      "datatables.net-responsive/js/dataTables.responsive.min.js",
    paths.nodeModules +
      "datatables.net-responsive-bs5/js/responsive.bootstrap5.min.js",
    paths.nodeModules +
      "datatables.net-fixedcolumns-bs5/js/fixedColumns.bootstrap5.min.js",
    paths.nodeModules +
      "datatables.net-fixedheader/js/dataTables.fixedHeader.min.js",
    paths.nodeModules + "datatables.net-buttons/js/dataTables.buttons.min.js",
    paths.nodeModules +
      "datatables.net-buttons-bs5/js/buttons.bootstrap5.min.js",
    paths.nodeModules + "datatables.net-buttons/js/buttons.print.min.js",
    paths.nodeModules + "datatables.net-keytable/js/dataTables.keyTable.min.js",
    paths.nodeModules + "datatables.net-select/js/dataTables.select.min.js",
  ])
    .pipe(concat("vendor-tables.js"))
    .pipe(terser())
    .pipe(rename({ suffix: ".min" }))
    .pipe(dest(outJS));
};

// Charts: ApexCharts
const vendorCharts = function () {
  const outCSS = paths.distCSS;
  const outJS = paths.distJS;

  // vendor-charts.css
  src([paths.nodeModules + "apexcharts/dist/apexcharts.css"])
    .pipe(concat("vendor-charts.css"))
    .pipe(CleanCSS())
    .pipe(rename({ suffix: ".min" }))
    .pipe(dest(outCSS));

  // vendor-charts.js
  return src([paths.nodeModules + "apexcharts/dist/apexcharts.min.js"])
    .pipe(concat("vendor-charts.js"))
    .pipe(terser())
    .pipe(rename({ suffix: ".min" }))
    .pipe(dest(outJS));
};

// Maps: Leaflet, jsvectormap, gmaps
const vendorMaps = function () {
  const outCSS = paths.distCSS;
  const outJS = paths.distJS;

  // vendor-maps.css
  src([
    paths.nodeModules + "leaflet/dist/leaflet.css",
    paths.nodeModules + "jsvectormap/dist/jsvectormap.min.css",
  ])
    .pipe(concat("vendor-maps.css"))
    .pipe(CleanCSS())
    .pipe(rename({ suffix: ".min" }))
    .pipe(dest(outCSS));

  // vendor-maps.js
  return src([
    paths.nodeModules + "leaflet/dist/leaflet.js",
    paths.nodeModules + "jsvectormap/dist/jsvectormap.min.js",
    paths.nodeModules + "jsvectormap/dist/maps/world-merc.js",
    paths.nodeModules + "jsvectormap/dist/maps/world.js",
    paths.nodeModules + "gmaps/gmaps.min.js",
  ])
    .pipe(concat("vendor-maps.js"))
    .pipe(terser())
    .pipe(rename({ suffix: ".min" }))
    .pipe(dest(outJS));
};

// UI: Componentes de UI (sweetalert2, nouislider, simplebar, dragula, etc.)
const vendorUI = function () {
  const outCSS = paths.distCSS;
  const outJS = paths.distJS;

  // vendor-ui.css
  src([
    paths.nodeModules + "sweetalert2/dist/sweetalert2.min.css",
    paths.nodeModules + "nouislider/dist/nouislider.min.css",
    paths.nodeModules + "simplebar/dist/simplebar.min.css",
  ])
    .pipe(concat("vendor-ui.css"))
    .pipe(CleanCSS())
    .pipe(rename({ suffix: ".min" }))
    .pipe(dest(outCSS));

  // vendor-ui.js
  return src([
    paths.nodeModules + "sweetalert2/dist/sweetalert2.min.js",
    paths.nodeModules + "wnumb/wNumb.min.js",
    paths.nodeModules + "nouislider/dist/nouislider.min.js",
    paths.nodeModules + "simplebar/dist/simplebar.min.js",
    paths.nodeModules + "dragula/dist/dragula.min.js",
    paths.nodeModules + "vanilla-wizard/dist/js/wizard.min.js",
  ])
    .pipe(concat("vendor-ui.js"))
    .pipe(terser())
    .pipe(rename({ suffix: ".min" }))
    .pipe(dest(outJS));
};

// Calendar: FullCalendar
const vendorCalendar = function () {
  const outJS = paths.distJS;

  // vendor-calendar.js
  return src([paths.nodeModules + "fullcalendar/index.global.min.js"])
    .pipe(concat("vendor-calendar.js"))
    .pipe(terser())
    .pipe(rename({ suffix: ".min" }))
    .pipe(dest(outJS));
};

// Grid: GridJS
const vendorGrid = function () {
  const outCSS = paths.distCSS;
  const outJS = paths.distJS;

  // vendor-grid.css
  src([paths.nodeModules + "gridjs/dist/theme/mermaid.min.css"])
    .pipe(concat("vendor-grid.css"))
    .pipe(CleanCSS())
    .pipe(rename({ suffix: ".min" }))
    .pipe(dest(outCSS));

  // vendor-grid.js
  return src([paths.nodeModules + "gridjs/dist/gridjs.umd.js"])
    .pipe(concat("vendor-grid.js"))
    .pipe(terser())
    .pipe(rename({ suffix: ".min" }))
    .pipe(dest(outJS));
};

// Utils: Utilidades varias (waypoints, counterup, etc.)
const vendorUtils = function () {
  const outJS = paths.distJS;

  // vendor-utils.js
  return src([
    paths.nodeModules + "waypoints/lib/jquery.waypoints.min.js",
    paths.nodeModules + "jquery.counterup/jquery.counterup.min.js",
  ])
    .pipe(concat("vendor-utils.js"))
    .pipe(terser())
    .pipe(rename({ suffix: ".min" }))
    .pipe(dest(outJS));
};

// Función que genera todos los bundles de vendors
const vendors = parallel(
  copyVendorAssets,
  vendorCore,
  vendorForms,
  vendorTables,
  vendorCharts,
  vendorMaps,
  vendorUI,
  vendorCalendar,
  vendorGrid,
  vendorUtils
);

// ============================================
// SCRIPTS - Compilar JS desde Scripts/ a wwwroot/js/
// Manteniendo la estructura de carpetas
// ============================================

const scripts = function () {
  const outJS = paths.distJS;

  // Compilar app.js y config.js en la raíz
  src([paths.srcScripts + "app.js", paths.srcScripts + "config.js"])
    .pipe(sourcemaps.init())
    .pipe(terser())
    .pipe(rename({ suffix: ".min" }))
    .pipe(sourcemaps.write("."))
    .pipe(dest(outJS));

  // Compilar archivos de components/ manteniendo la estructura
  src([
    paths.srcScripts + "components/**/*.js",
    "!" + paths.srcScripts + "components/**/*.min.js",
  ])
    .pipe(sourcemaps.init())
    .pipe(terser())
    .pipe(rename({ suffix: ".min" }))
    .pipe(sourcemaps.write("."))
    .pipe(dest(outJS + "components/"));

  // Compilar archivos de maps/ manteniendo la estructura
  src([
    paths.srcScripts + "maps/**/*.js",
    "!" + paths.srcScripts + "maps/**/*.min.js",
  ])
    .pipe(sourcemaps.init())
    .pipe(terser())
    .pipe(rename({ suffix: ".min" }))
    .pipe(sourcemaps.write("."))
    .pipe(dest(outJS + "maps/"));

  // Compilar archivos de pages/ manteniendo la estructura
  return src([
    paths.srcScripts + "pages/**/*.js",
    "!" + paths.srcScripts + "pages/**/*.min.js",
  ])
    .pipe(sourcemaps.init())
    .pipe(terser())
    .pipe(rename({ suffix: ".min" }))
    .pipe(sourcemaps.write("."))
    .pipe(dest(outJS + "pages/"));
};

// ============================================
// SCSS COMPILATION - Compilar desde Styles/scss/ a wwwroot/css/
// ============================================

const scss = function () {
  const out = paths.distCSS;

  // Compilar app.scss
  src(paths.srcStyles + "app.scss")
    .pipe(sourcemaps.init())
    .pipe(sass.sync().on("error", sass.logError)) // scss to css
    .pipe(
      autoprefixer({
        overrideBrowserslist: ["last 2 versions"],
      })
    )
    .pipe(dest(out))
    .pipe(CleanCSS())
    .pipe(rename({ suffix: ".min" }))
    .pipe(sourcemaps.write(".")) // source maps
    .pipe(dest(out));

  // generate rtl
  return src(paths.srcStyles + "app.scss")
    .pipe(sourcemaps.init())
    .pipe(sass.sync().on("error", sass.logError)) // scss to css
    .pipe(
      autoprefixer({
        overrideBrowserslist: ["last 2 versions"],
      })
    )
    .pipe(rtlcss())
    .pipe(rename({ suffix: "-rtl" }))
    .pipe(dest(out))
    .pipe(CleanCSS())
    .pipe(rename({ suffix: ".min" }))
    .pipe(sourcemaps.write(".")) // source maps
    .pipe(dest(out));
};

const icons = function () {
  const out = paths.distCSS;
  return src([
    paths.srcStyles + "icons.scss",
    paths.srcStyles + "icons/*.scss",
  ])
    .pipe(sourcemaps.init())
    .pipe(sass.sync().on("error", sass.logError)) // scss to css
    .pipe(
      autoprefixer({
        overrideBrowserslist: ["last 2 versions"],
      })
    )
    .pipe(dest(out))
    .pipe(CleanCSS())
    .pipe(rename({ suffix: ".min" }))
    .pipe(sourcemaps.write(".")) // source maps
    .pipe(dest(out));
};

// ============================================
// WATCH TASKS
// ============================================

function watchFiles() {
  // Watch SCSS
  watch(paths.srcStyles + "icons.scss", series(icons));
  watch(
    [
      paths.srcStyles + "**/*.scss",
      "!" + paths.srcStyles + "icons.scss",
      "!" + paths.srcStyles + "icons/*.scss",
    ],
    series(scss)
  );

  // Watch JS - Observar todos los archivos JS en Scripts/
  watch(
    [
      paths.srcScripts + "**/*.js",
      "!" + paths.srcScripts + "**/*.min.js", // Excluir archivos ya minificados
    ],
    series(scripts)
  );
}

// ============================================
// EXPORTS
// ============================================

// Production Tasks (con watch)
exports.default = series(
  parallel(vendors, scripts, scss, icons),
  parallel(watchFiles)
);

// Build Tasks (sin watch)
exports.build = series(parallel(vendors, scripts, scss, icons));

// Individual vendor bundles (por si necesitas generar solo uno)
exports.vendorCore = vendorCore;
exports.vendorForms = vendorForms;
exports.vendorTables = vendorTables;
exports.vendorCharts = vendorCharts;
exports.vendorMaps = vendorMaps;
exports.vendorUI = vendorUI;
exports.vendorCalendar = vendorCalendar;
exports.vendorGrid = vendorGrid;
exports.vendorUtils = vendorUtils;

// Individual tasks
exports.scripts = scripts;
exports.scss = scss;
exports.icons = icons;
