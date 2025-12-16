/**
 * Theme: Adminto - Responsive Bootstrap 5 Admin Dashboard
 * Author: Coderthemes
 * Module/App: App and Layout Js
 */

class App {
  // Bootstrap Components
  initComponents() {
    // loader - Preloader
    $(window).on("load", function () {
      $("#status").fadeOut();
      $("#preloader").delay(350).fadeOut("slow");
    });

    // Popovers
    const popoverTriggerList = document.querySelectorAll(
      '[data-bs-toggle="popover"]'
    );
    const popoverList = [...popoverTriggerList].map(
      (popoverTriggerEl) => new bootstrap.Popover(popoverTriggerEl)
    );

    // Tooltips
    const tooltipTriggerList = document.querySelectorAll(
      '[data-bs-toggle="tooltip"]'
    );
    const tooltipList = [...tooltipTriggerList].map(
      (tooltipTriggerEl) => new bootstrap.Tooltip(tooltipTriggerEl)
    );

    // offcanvas
    const offcanvasElementList = document.querySelectorAll(".offcanvas");
    const offcanvasList = [...offcanvasElementList].map(
      (offcanvasEl) => new bootstrap.Offcanvas(offcanvasEl)
    );

    //Toasts
    var toastPlacement = document.getElementById("toastPlacement");
    if (toastPlacement) {
      document
        .getElementById("selectToastPlacement")
        .addEventListener("change", function () {
          if (!toastPlacement.dataset.originalClass) {
            toastPlacement.dataset.originalClass = toastPlacement.className;
          }
          toastPlacement.className =
            toastPlacement.dataset.originalClass + " " + this.value;
        });
    }

    var toastElList = [].slice.call(document.querySelectorAll(".toast"));
    var toastList = toastElList.map(function (toastEl) {
      return new bootstrap.Toast(toastEl);
    });

    // Bootstrap Alert Live Example
    const alertPlaceholder = document.getElementById("liveAlertPlaceholder");
    const alert = (message, type) => {
      const wrapper = document.createElement("div");
      wrapper.innerHTML = [
        `<div class="alert alert-${type} alert-dismissible" role="alert">`,
        `   <div>${message}</div>`,
        '   <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>',
        "</div>",
      ].join("");

      alertPlaceholder.append(wrapper);
    };

    const alertTrigger = document.getElementById("liveAlertBtn");
    if (alertTrigger) {
      alertTrigger.addEventListener("click", () => {
        alert("Nice, you triggered this alert message!", "success");
      });
    }

    // RTL Layout
    // if (document.getElementById("app-style").href.includes("rtl.min.css")) {
    //   document.getElementsByTagName("html")[0].dir = "rtl";
    // }
  }

  // Portlet Widget (Card Reload, Collapse, and Delete)
  initPortletCard() {
    var portletIdentifier = ".card";
    var portletCloser = '.card a[data-bs-toggle="remove"]';
    var portletRefresher = '.card a[data-bs-toggle="reload"]';
    let self = this;

    // Panel closest
    $(document).on("click", portletCloser, function (ev) {
      ev.preventDefault();
      var $portlet = $(this).closest(portletIdentifier);
      var $portlet_parent = $portlet.parent();
      $portlet.remove();
      if ($portlet_parent.children().length == 0) {
        $portlet_parent.remove();
      }
    });

    // Panel Reload
    $(document).on("click", portletRefresher, function (ev) {
      ev.preventDefault();
      var $portlet = $(this).closest(portletIdentifier);
      // This is just a simulation, nothing is going to be reloaded
      $portlet.append(
        '<div class="card-disabled"><div class="card-portlets-loader"></div></div>'
      );
      var $pd = $portlet.find(".card-disabled");
      setTimeout(function () {
        $pd.fadeOut("fast", function () {
          $pd.remove();
        });
      }, 500 + 300 * (Math.random() * 5));
    });
  }

  //  Multi Dropdown
  initMultiDropdown() {
    $(".dropdown-menu a.dropdown-toggle").on("click", function () {
      var dropdown = $(this).next(".dropdown-menu");
      var otherDropdown = $(this)
        .parent()
        .parent()
        .find(".dropdown-menu")
        .not(dropdown);
      otherDropdown.removeClass("show");
      otherDropdown.parent().find(".dropdown-toggle").removeClass("show");
      return false;
    });
  }

  // Counterup
  initCounterUp() {
    var delay = $(this).attr("data-delay") ? $(this).attr("data-delay") : 100; //default is 100
    var time = $(this).attr("data-time") ? $(this).attr("data-time") : 1200; //default is 1200
    $('[data-plugin="counterup"]').each(function (idx, obj) {
      $(this).counterUp({
        delay: delay,
        time: time,
      });
    });
  }

  // Left Sidebar Menu (Vertical Menu)
  initLeftSidebar() {
    var self = this;

    if ($(".side-nav").length) {
      var navCollapse = $(".side-nav li .collapse");
      var navToggle = $(".side-nav li [data-bs-toggle='collapse']");
      navToggle.on("click", function (e) {
        return false;
      });

      // open one menu at a time only
      navCollapse.on({
        "show.bs.collapse": function (event) {
          var parent = $(event.target).parents(".collapse.show");
          $(".side-nav .collapse.show")
            .not(event.target)
            .not(parent)
            .collapse("hide");
        },
      });

      // activate the menu in left side bar (Vertical Menu) based on url

      $(".side-nav li").each(function () {
        $(this).removeClass("active");
      });

      $(".side-nav a").each(function () {
        $(this).removeClass("active");
      });

      $(".side-nav a").each(function () {
        var pageUrl = window.location.href.split(/[?#]/)[0];
        if (this.href == pageUrl) {
          $(this).addClass("active");
          $(this).parent().addClass("active");
          $(this).parent().parent().parent().addClass("show");
          $(this).parent().parent().parent().parent().addClass("active"); // add active to li of the current link

          var firstLevelParent = $(this)
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent();
          if (firstLevelParent.attr("id") !== "sidebar-menu")
            firstLevelParent.addClass("show");

          $(this)
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .addClass("active");

          var secondLevelParent = $(this)
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent();
          if (secondLevelParent.attr("id") !== "wrapper")
            secondLevelParent.addClass("show");

          var upperLevelParent = $(this)
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent();
          if (!upperLevelParent.is("body")) upperLevelParent.addClass("active");
        }
      });

      setTimeout(function () {
        var activatedItem = document.querySelector("li.active .active");
        if (activatedItem != null) {
          var simplebarContent = document.querySelector(
            ".sidenav-menu .simplebar-content-wrapper"
          );
          var offset = activatedItem.offsetTop - 300;
          if (simplebarContent && offset > 100) {
            scrollTo(simplebarContent, offset, 600);
          }
        }
      }, 200);

      // scrollTo (Left Side Bar Active Menu)
      function easeInOutQuad(t, b, c, d) {
        t /= d / 2;
        if (t < 1) return (c / 2) * t * t + b;
        t--;
        return (-c / 2) * (t * (t - 2) - 1) + b;
      }
      function scrollTo(element, to, duration) {
        var start = element.scrollTop,
          change = to - start,
          currentTime = 0,
          increment = 20;
        var animateScroll = function () {
          currentTime += increment;
          var val = easeInOutQuad(currentTime, start, change, duration);
          element.scrollTop = val;
          if (currentTime < duration) {
            setTimeout(animateScroll, increment);
          }
        };
        animateScroll();
      }
    }
  }

  // Topbar Menu (Horizontal Menu)
  initTopbarMenu() {
    if ($(".navbar-nav").length) {
      $(".navbar-nav li a").each(function () {
        var pageUrl = window.location.href.split(/[?#]/)[0];
        if (this.href == pageUrl) {
          $(this).addClass("active");
          $(this).parent().parent().addClass("active"); // add active to li of the current link
          $(this).parent().parent().parent().parent().addClass("active");
          $(this)
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .addClass("active");
        }
      });

      // Topbar - main menu
      $(".navbar-toggle").on("click", function () {
        $(this).toggleClass("open");
        $("#navigation").slideToggle(400);
      });
    }
  }

  // Topbar Fullscreen Button
  initfullScreenListener() {
    var self = this;
    var fullScreenBtn = document.querySelector('[data-toggle="fullscreen"]');

    if (fullScreenBtn) {
      fullScreenBtn.addEventListener("click", function (e) {
        e.preventDefault();
        document.body.classList.toggle("fullscreen-enable");
        if (
          !document.fullscreenElement &&
          /* alternative standard method */ !document.mozFullScreenElement &&
          !document.webkitFullscreenElement
        ) {
          // current working methods
          if (document.documentElement.requestFullscreen) {
            document.documentElement.requestFullscreen();
          } else if (document.documentElement.mozRequestFullScreen) {
            document.documentElement.mozRequestFullScreen();
          } else if (document.documentElement.webkitRequestFullscreen) {
            document.documentElement.webkitRequestFullscreen(
              Element.ALLOW_KEYBOARD_INPUT
            );
          }
        } else {
          if (document.cancelFullScreen) {
            document.cancelFullScreen();
          } else if (document.mozCancelFullScreen) {
            document.mozCancelFullScreen();
          } else if (document.webkitCancelFullScreen) {
            document.webkitCancelFullScreen();
          }
        }
      });
    }
  }

  // Form Validation
  initFormValidation() {
    // Example starter JavaScript for disabling form submissions if there are invalid fields
    // Fetch all the forms we want to apply custom Bootstrap validation styles to
    // Loop over them and prevent submission
    document.querySelectorAll(".needs-validation").forEach((form) => {
      form.addEventListener(
        "submit",
        (event) => {
          if (!form.checkValidity()) {
            event.preventDefault();
            event.stopPropagation();
          }

          form.classList.add("was-validated");
        },
        false
      );
    });
  }

  // Form Advance
  initFormAdvance() {
    document.querySelectorAll('[data-toggle="input-mask"]').forEach((e) => {
      const maskFormat = e
        .getAttribute("data-mask-format")
        .toString()
        .replaceAll("0", "9");
      e.setAttribute("data-mask-format", maskFormat);
      const im = new Inputmask(maskFormat);
      im.mask(e);
    });

    // Choices Select plugin
    var choicesExamples = document.querySelectorAll("[data-choices]");
    choicesExamples.forEach(function (item) {
      var choiceData = {};
      var isChoicesVal = item.attributes;
      if (isChoicesVal["data-choices-groups"]) {
        choiceData.placeholderValue = "This is a placeholder set in the config";
      }
      if (isChoicesVal["data-choices-search-false"]) {
        choiceData.searchEnabled = false;
      }
      if (isChoicesVal["data-choices-search-true"]) {
        choiceData.searchEnabled = true;
      }
      if (isChoicesVal["data-choices-removeItem"]) {
        choiceData.removeItemButton = true;
      }
      if (isChoicesVal["data-choices-sorting-false"]) {
        choiceData.shouldSort = false;
      }
      if (isChoicesVal["data-choices-sorting-true"]) {
        choiceData.shouldSort = true;
      }
      if (isChoicesVal["data-choices-multiple-remove"]) {
        choiceData.removeItemButton = true;
      }
      if (isChoicesVal["data-choices-limit"]) {
        choiceData.maxItemCount =
          isChoicesVal["data-choices-limit"].value.toString();
      }
      if (isChoicesVal["data-choices-limit"]) {
        choiceData.maxItemCount =
          isChoicesVal["data-choices-limit"].value.toString();
      }
      if (isChoicesVal["data-choices-editItem-true"]) {
        choiceData.maxItemCount = true;
      }
      if (isChoicesVal["data-choices-editItem-false"]) {
        choiceData.maxItemCount = false;
      }
      if (isChoicesVal["data-choices-text-unique-true"]) {
        choiceData.duplicateItemsAllowed = false;
      }
      if (isChoicesVal["data-choices-text-disabled-true"]) {
        choiceData.addItems = false;
      }
      isChoicesVal["data-choices-text-disabled-true"]
        ? new Choices(item, choiceData).disable()
        : new Choices(item, choiceData);
    });

    // Select2
    if (jQuery().select2) {
      $('[data-toggle="select2"]').select2();
    }

    // Input Mask
    if (jQuery().mask) {
      $('[data-toggle="input-mask"]').each(function (idx, obj) {
        var maskFormat = $(obj).data("maskFormat");
        var reverse = $(obj).data("reverse");
        if (reverse != null) $(obj).mask(maskFormat, { reverse: reverse });
        else $(obj).mask(maskFormat);
      });
    }

    //  Flatpickr
    var flatpickrExamples = document.querySelectorAll("[data-provider]");
    Array.from(flatpickrExamples).forEach(function (item) {
      if (item.getAttribute("data-provider") == "flatpickr") {
        var dateData = {};
        var isFlatpickerVal = item.attributes;
        dateData.disableMobile = "true";
        if (isFlatpickerVal["data-date-format"])
          dateData.dateFormat =
            isFlatpickerVal["data-date-format"].value.toString();
        if (isFlatpickerVal["data-enable-time"]) {
          (dateData.enableTime = true),
            (dateData.dateFormat =
              isFlatpickerVal["data-date-format"].value.toString() + " H:i");
        }
        if (isFlatpickerVal["data-altFormat"]) {
          (dateData.altInput = true),
            (dateData.altFormat =
              isFlatpickerVal["data-altFormat"].value.toString());
        }
        if (isFlatpickerVal["data-minDate"]) {
          dateData.minDate = isFlatpickerVal["data-minDate"].value.toString();
          dateData.dateFormat =
            isFlatpickerVal["data-date-format"].value.toString();
        }
        if (isFlatpickerVal["data-maxDate"]) {
          dateData.maxDate = isFlatpickerVal["data-maxDate"].value.toString();
          dateData.dateFormat =
            isFlatpickerVal["data-date-format"].value.toString();
        }
        if (isFlatpickerVal["data-deafult-date"]) {
          dateData.defaultDate =
            isFlatpickerVal["data-deafult-date"].value.toString();
          dateData.dateFormat =
            isFlatpickerVal["data-date-format"].value.toString();
        }
        if (isFlatpickerVal["data-multiple-date"]) {
          dateData.mode = "multiple";
          dateData.dateFormat =
            isFlatpickerVal["data-date-format"].value.toString();
        }
        if (isFlatpickerVal["data-range-date"]) {
          dateData.mode = "range";
          dateData.dateFormat =
            isFlatpickerVal["data-date-format"].value.toString();
        }
        if (isFlatpickerVal["data-inline-date"]) {
          (dateData.inline = true),
            (dateData.defaultDate =
              isFlatpickerVal["data-deafult-date"].value.toString());
          dateData.dateFormat =
            isFlatpickerVal["data-date-format"].value.toString();
        }
        if (isFlatpickerVal["data-disable-date"]) {
          var dates = [];
          dates.push(isFlatpickerVal["data-disable-date"].value);
          dateData.disable = dates.toString().split(",");
        }
        if (isFlatpickerVal["data-week-number"]) {
          var dates = [];
          dates.push(isFlatpickerVal["data-week-number"].value);
          dateData.weekNumbers = true;
        }
        flatpickr(item, dateData);
      } else if (item.getAttribute("data-provider") == "timepickr") {
        var timeData = {};
        var isTimepickerVal = item.attributes;
        if (isTimepickerVal["data-time-basic"]) {
          (timeData.enableTime = true),
            (timeData.noCalendar = true),
            (timeData.dateFormat = "H:i");
        }
        if (isTimepickerVal["data-time-hrs"]) {
          (timeData.enableTime = true),
            (timeData.noCalendar = true),
            (timeData.dateFormat = "H:i"),
            (timeData.time_24hr = true);
        }
        if (isTimepickerVal["data-min-time"]) {
          (timeData.enableTime = true),
            (timeData.noCalendar = true),
            (timeData.dateFormat = "H:i"),
            (timeData.minTime =
              isTimepickerVal["data-min-time"].value.toString());
        }
        if (isTimepickerVal["data-max-time"]) {
          (timeData.enableTime = true),
            (timeData.noCalendar = true),
            (timeData.dateFormat = "H:i"),
            (timeData.minTime =
              isTimepickerVal["data-max-time"].value.toString());
        }
        if (isTimepickerVal["data-default-time"]) {
          (timeData.enableTime = true),
            (timeData.noCalendar = true),
            (timeData.dateFormat = "H:i"),
            (timeData.defaultDate =
              isTimepickerVal["data-default-time"].value.toString());
        }
        if (isTimepickerVal["data-time-inline"]) {
          (timeData.enableTime = true),
            (timeData.noCalendar = true),
            (timeData.defaultDate =
              isTimepickerVal["data-time-inline"].value.toString());
          timeData.inline = true;
        }
        flatpickr(item, timeData);
      }
    });
  }

  // Topbar Scroll
  initTopbarScroll() {
    var scrollPosition = window.scrollY;
    var topbar = document.getElementById("header");

    if (topbar) {
      window.addEventListener("scroll", function () {
        scrollPosition = window.scrollY;
        if (scrollPosition >= 25) {
          topbar.classList.add("topbar-active");
        } else {
          topbar.classList.remove("topbar-active");
        }
      });
    }
  }

  init() {
    this.initComponents();
    this.initPortletCard();
    this.initMultiDropdown();
    this.initCounterUp();
    this.initLeftSidebar();
    this.initTopbarMenu();
    this.initfullScreenListener();
    this.initFormValidation();
    this.initFormAdvance();
    this.initTopbarScroll();
  }
}

class ThemeCustomizer {
  constructor() {
    this.html = document.getElementsByTagName("html")[0];
    this.config = window.config;
    this.defaultConfig = window.config;
  }

  initConfig() {
    this.defaultConfig = JSON.parse(JSON.stringify(window.defaultConfig));
    this.config = JSON.parse(JSON.stringify(window.config));
    this.setSwitchFromConfig();
  }

  initTwoColumn() {
    // handling two columns menu if present
    var twoColSideNav = $("#two-col-sidenav-main");
    if (twoColSideNav.length) {
      var twoColSideNavItems = $("#two-col-sidenav-main .side-nav-link");
      var sideSubMenus = $(".sidenav-menu-item");

      var nav = $(".sidenav-menu-item .sub-menu");
      var navCollapse = $("#two-col-menu menu-item .collapse");

      // open one menu at a time only
      navCollapse.on({
        "show.bs.collapse": function () {
          var nearestNav = $(this).closest(nav).closest(nav).find(navCollapse);
          if (nearestNav.length) nearestNav.not($(this)).collapse("hide");
          else navCollapse.not($(this)).collapse("hide");
        },
      });

      twoColSideNavItems.on("click", function (e) {
        var target = $($(this).attr("href"));

        if (target.length) {
          e.preventDefault();
          twoColSideNavItems.removeClass("active");
          $(this).addClass("active");
          sideSubMenus.removeClass("d-block");
          target.addClass("d-block");
          if (window.innerWidth >= 1040) {
            self.changeLeftbarSize("default");
          }
        }
        return true;
      });

      // activate menu with no child
      var pageUrl = window.location.href; //.split(/[?#]/)[0];
      twoColSideNavItems.each(function () {
        if (this.href === pageUrl) {
          $(this).addClass("active");
        }
      });

      // activate the menu in left side bar (Two column) based on url
      $("#two-col-menu a").each(function () {
        if (this.href == pageUrl) {
          $(this).addClass("active");
          $(this).parent().addClass("active");
          $(this).parent().parent().parent().addClass("show");
          $(this).parent().parent().parent().parent().addClass("active"); // add active to li of the current link

          var firstLevelParent = $(this)
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent();
          if (firstLevelParent.attr("id") !== "sidebar-menu")
            firstLevelParent.addClass("show");

          $(this)
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .addClass("active");

          var secondLevelParent = $(this)
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent();
          if (secondLevelParent.attr("id") !== "wrapper")
            secondLevelParent.addClass("show");

          var upperLevelParent = $(this)
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent()
            .parent();
          if (!upperLevelParent.is("body")) upperLevelParent.addClass("active");

          // opening menu
          var matchingItem = null;
          var targetEl = "#" + $(this).parents(".sidenav-menu-item").attr("id");
          $("#two-col-sidenav-main .side-nav-link").each(function () {
            if ($(this).attr("href") === targetEl) {
              matchingItem = $(this);
            }
          });
          if (matchingItem) matchingItem.trigger("click");
        }
      });
    }
  }

  changeMenuColor(color) {
    this.config.menu.color = color;
    this.html.setAttribute("data-menu-color", color);
    this.setSwitchFromConfig();
  }

  changeLeftbarSize(size, save = true) {
    this.html.setAttribute("data-sidenav-size", size);
    if (save) {
      this.config.sidenav.size = size;
      this.setSwitchFromConfig();
    }
  }

  changeLayoutMode(mode, save = true) {
    this.html.setAttribute("data-layout-mode", mode);
    if (save) {
      this.config.layout.mode = mode;
      this.setSwitchFromConfig();
    }
  }

  changeLayoutColor(color) {
    this.config.theme = color;
    this.html.setAttribute("data-bs-theme", color);
    this.setSwitchFromConfig();
  }

  changeTopbarColor(color) {
    this.config.topbar.color = color;
    this.html.setAttribute("data-topbar-color", color);
    this.setSwitchFromConfig();
  }

  resetTheme() {
    this.config = JSON.parse(JSON.stringify(window.defaultConfig));
    this.changeMenuColor(this.config.menu.color);
    this.changeLeftbarSize(this.config.sidenav.size);
    this.changeLayoutColor(this.config.theme);
    this.changeLayoutMode(this.config.layout.mode);
    this.changeTopbarColor(this.config.topbar.color);
    this._adjustLayout();
  }

  initSwitchListener() {
    var self = this;
    document
      .querySelectorAll("input[name=data-menu-color]")
      .forEach(function (element) {
        element.addEventListener("change", function (e) {
          self.changeMenuColor(element.value);
        });
      });

    document
      .querySelectorAll("input[name=data-sidenav-size]")
      .forEach(function (element) {
        element.addEventListener("change", function (e) {
          self.changeLeftbarSize(element.value);
        });
      });

    document
      .querySelectorAll("input[name=data-bs-theme]")
      .forEach(function (element) {
        element.addEventListener("change", function (e) {
          self.changeLayoutColor(element.value);
        });
      });
    document
      .querySelectorAll("input[name=data-layout-mode]")
      .forEach(function (element) {
        element.addEventListener("change", function (e) {
          self.changeLayoutMode(element.value);
        });
      });
    document
      .querySelectorAll("input[name=data-layout]")
      .forEach(function (element) {
        element.addEventListener("change", function (e) {
          window.location =
            element.value === "horizontal"
              ? "layouts-horizontal.html"
              : "index.html";
        });
      });
    document
      .querySelectorAll("input[name=data-topbar-color]")
      .forEach(function (element) {
        element.addEventListener("change", function (e) {
          self.changeTopbarColor(element.value);
        });
      });

    // TopBar Light Dark
    var themeColorToggle = document.getElementById("light-dark-mode");
    if (themeColorToggle) {
      themeColorToggle.addEventListener("click", function (e) {
        if (self.config.theme === "light") {
          self.changeLayoutColor("dark");
          self.changeMenuColor("dark");
        } else {
          self.changeLayoutColor("light");
          self.changeMenuColor("light");
        }
      });
    }

    var resetBtn = document.querySelector("#reset-layout");
    if (resetBtn) {
      resetBtn.addEventListener("click", function (e) {
        self.resetTheme();
      });
    }

    var menuCloseBtn = document.querySelector(".button-close-fullsidebar");
    if (menuCloseBtn) {
      menuCloseBtn.addEventListener("click", function () {
        self.html.classList.remove("sidebar-enable");
        self.hideBackdrop();
      });
    }

    var hoverBtn = document.querySelectorAll(".button-sm-hover");
    hoverBtn.forEach(function (element) {
      element.addEventListener("click", function () {
        var configSize = self.config.sidenav.size;
        var size = self.html.getAttribute("data-sidenav-size", configSize);

        if (size === "sm-hover-active") {
          self.changeLeftbarSize("sm-hover", false);
        } else {
          self.changeLeftbarSize("sm-hover-active", false);
        }
      });
    });
  }

  toggleSidenav() {
    var configSize = this.config.sidenav.size;
    var size = this.html.getAttribute("data-sidenav-size", configSize);

    if (size === "full") {
      this.showBackdrop();
    } else {
      if (configSize == "fullscreen") {
        if (size === "fullscreen") {
          this.changeLeftbarSize(
            configSize == "fullscreen" ? "default" : configSize,
            false
          );
        } else {
          this.changeLeftbarSize("fullscreen", false);
        }
      } else {
        if (size === "condensed") {
          this.changeLeftbarSize(
            configSize == "condensed" ? "default" : configSize,
            false
          );
        } else {
          this.changeLeftbarSize("condensed", false);
        }
      }
    }

    this.html.classList.toggle("sidebar-enable");
  }

  showBackdrop() {
    const backdrop = document.createElement("div");
    backdrop.id = "custom-backdrop";
    backdrop.classList = "offcanvas-backdrop fade show";
    document.body.appendChild(backdrop);
    document.body.style.overflow = "hidden";
    if (window.innerWidth > 767) {
      document.body.style.paddingRight = "15px";
    }
    const self = this;
    backdrop.addEventListener("click", function (e) {
      self.html.classList.remove("sidebar-enable");
      self.hideBackdrop();
    });
  }

  hideBackdrop() {
    var backdrop = document.getElementById("custom-backdrop");
    if (backdrop) {
      document.body.removeChild(backdrop);
      document.body.style.overflow = null;
      document.body.style.paddingRight = null;
    }
  }

  initWindowSize() {
    var self = this;
    window.addEventListener("resize", function (e) {
      self._adjustLayout();
    });
  }

  _adjustLayout() {
    var self = this;

    if (window.innerWidth <= 1140) {
      self.changeLeftbarSize("full", false);
      self.changeLayoutMode("default", false);
    } else {
      self.changeLeftbarSize(self.config.sidenav.size);
      self.changeLayoutMode(self.config.layout.mode);
    }
  }

  setSwitchFromConfig() {
    sessionStorage.setItem("__ADMINTO_CONFIG__", JSON.stringify(this.config));

    document
      .querySelectorAll("#theme-settings-offcanvas input[type=checkbox]")
      .forEach(function (checkbox) {
        checkbox.checked = false;
      });

    var config = this.config;
    if (config) {
      var layoutNavSwitch = document.querySelector(
        "input[type=radio][name=data-layout][value=" + config.nav + "]"
      );
      var layoutColorSwitch = document.querySelector(
        "input[type=radio][name=data-bs-theme][value=" + config.theme + "]"
      );
      var layoutModeSwitch = document.querySelector(
        "input[type=radio][name=data-layout-mode][value=" +
          config.layout.mode +
          "]"
      );
      var topbarColorSwitch = document.querySelector(
        "input[type=radio][name=data-topbar-color][value=" +
          config.topbar.color +
          "]"
      );
      var menuColorSwitch = document.querySelector(
        "input[type=radio][name=data-menu-color][value=" +
          config.menu.color +
          "]"
      );
      var leftbarSizeSwitch = document.querySelector(
        "input[type=radio][name=data-sidenav-size][value=" +
          config.sidenav.size +
          "]"
      );

      if (layoutNavSwitch) layoutNavSwitch.checked = true;
      if (layoutColorSwitch) layoutColorSwitch.checked = true;
      if (layoutModeSwitch) layoutModeSwitch.checked = true;
      if (topbarColorSwitch) topbarColorSwitch.checked = true;
      if (menuColorSwitch) menuColorSwitch.checked = true;
      if (leftbarSizeSwitch) leftbarSizeSwitch.checked = true;
    }
  }

  init() {
    this.initConfig();
    this.initTwoColumn();
    this.initSwitchListener();
    this.initWindowSize();
    this._adjustLayout();
    this.setSwitchFromConfig();
  }
}

const customJS = () => {
  const mouseStopEvent = () => {
    var timeout;
    document.addEventListener("mousemove", function (e) {
      clearTimeout(timeout);
      timeout = setTimeout(function () {
        var event = new CustomEvent("mousestop", {
          detail: {
            clientX: e.clientX,
            clientY: e.clientY,
          },
          bubbles: true,
          cancelable: true,
        });
        e.target.dispatchEvent(event);
      }, 100);
    });
  };

  const initDropdownHover = () => {
    const dropdowns = document.querySelectorAll(
      "[data-bs-toggle=dropdown][data-bs-trigger=hover]"
    );
    console.info(dropdowns);
    dropdowns.forEach((dropdown) => {
      dropdown.addEventListener("mouseenter", (e) => {
        const dropdownContent = dropdown.nextElementSibling;
        if (!dropdown.classList.contains("show")) dropdown.click();
        const fn = (e) => {
          if (
            !(dropdown.contains(e.target) || dropdownContent.contains(e.target))
          ) {
            if (dropdown.classList.contains("show")) dropdown.click();
            window.removeEventListener("mousestop", fn);
          }
        };
        window.addEventListener("mousestop", fn);
      });
    });
  };

  const initToggle = () => {
    const toggleWrappers = document.querySelectorAll("[data-toggler]");
    const showToggler = (element) => {
      element.classList.remove("d-none");
      // element.classList.add('show');
    };
    const hideToggler = (element) => {
      element.classList.add("d-none");
      // element.classList.remove('show');
    };

    const toggleToggler = (toggleOn, toggleOff, toggleStatus) => {
      console.info(toggleOn, toggleOff, toggleStatus);
      if (toggleOn && toggleOff) {
        if (toggleStatus) {
          showToggler(toggleOn);
          hideToggler(toggleOff);
        } else {
          showToggler(toggleOff);
          hideToggler(toggleOn);
        }
      }
    };

    toggleWrappers.forEach((toggle) => {
      const toggleOn = toggle.querySelector("[data-toggler-on]");
      const toggleOff = toggle.querySelector("[data-toggler-off]");
      let toggleStatus = toggle.getAttribute("data-toggler") === "on";
      if (toggleOn) {
        toggleOn.addEventListener("click", () => {
          toggleStatus = false;
          toggleToggler(toggleOn, toggleOff, toggleStatus);
        });
      }
      if (toggleOff) {
        toggleOff.addEventListener("click", () => {
          toggleStatus = true;
          toggleToggler(toggleOn, toggleOff, toggleStatus);
        });
      }
      toggleToggler(toggleOn, toggleOff, toggleStatus);
    });
  };

  const initDismissible = () => {
    const dismissibleTriggers = document.querySelectorAll("[data-dismissible]");
    dismissibleTriggers.forEach((trigger) => {
      trigger.addEventListener("click", (e) => {
        const dataDismissible = trigger.getAttribute("data-dismissible");
        const dismissElement = document.querySelector(dataDismissible);
        if (dismissElement) {
          dismissElement.remove();
        }
      });
    });
  };

  const initTouchspin = () => {
    const touchspins = document.querySelectorAll("[data-touchspin]");
    touchspins.forEach((touchspin) => {
      const minusBtn = touchspin.querySelector(".minus");
      const plusBtn = touchspin.querySelector(".plus");
      const input = touchspin.querySelector("input");
      if (input) {
        const min = input.min.length !== 0 ? Number(input.min) : null;
        const max = input.max.length !== 0 ? Number(input.max) : null;
        if (minusBtn) {
          minusBtn.addEventListener("click", (e) => {
            const num = Number.parseInt(input.value) - 1;
            if (min === null) {
              input.value = num.toString();
            }
            if (min != null && num > min - 1) {
              input.value = num.toString();
            }
          });
        }
        if (plusBtn) {
          plusBtn.addEventListener("click", (e) => {
            const num = Number.parseInt(input.value) + 1;
            if (max === null) {
              input.value = num.toString();
            }
            if (max != null && num < max + 1) {
              input.value = num.toString();
            }
          });
        }
      }
    });
  };

  const init = () => {
    mouseStopEvent();
    initDismissible();
    initToggle();
    // initDropdownHover();
    initTouchspin();
  };

  init();
};

window.loadApps = function () {
console.log('here')
  new App().init();
  new ThemeCustomizer().init();
  customJS();
};

window.toggleSidenav = function () {
  new ThemeCustomizer().toggleSidenav();
};

window.updateHtmlAttributes = function (attr, value) {
  document.documentElement.setAttribute(attr, value);
};

window.updateBodyAttributes = function (attr, value) {
  document.body.setAttribute(attr, value);
};

window.showBlazorToast = function (toastId) {
    var toastEl = document.getElementById(toastId);
    if (toastEl) {
        var toast = bootstrap.Toast.getOrCreateInstance(toastEl);
        toast.show();
    }
};

window.blurElementById = function (id) {
    var el = document.getElementById(id);
    if (el) {
        el.blur();
        // Forzar evento change para Blazor
        var event = new Event('change', { bubbles: true });
        el.dispatchEvent(event);
    }
};

/**
 * Validates a date field against maxDate and minDate constraints
 * @param {Object} instance - Flatpickr instance
 * @param {string} fieldId - Field identifier
 * @param {string} maxDate - Maximum date allowed (YYYY-MM-DD format)
 * @param {string} minDate - Minimum date allowed (YYYY-MM-DD format)
 * @param {string} dependsOnFieldId - Field ID this field depends on
 * @returns {boolean} True if valid, false otherwise
 */
function validateDateField(instance, fieldId, maxDate, minDate, dependsOnFieldId) {
    if (!instance || !instance.altInput) {
        return true;
    }

    var val = instance.altInput.value;
    if (!val || val.length !== 10) {
        return false;
    }

    var parts = val.split("/");
    if (parts.length !== 3) {
        return false;
    }

    var day = parseInt(parts[0], 10);
    var month = parseInt(parts[1], 10);
    var year = parseInt(parts[2], 10);
    var date = new Date(year, month - 1, day);

    // Validate date is valid
    if (date.getFullYear() !== year || (date.getMonth() + 1) !== month || date.getDate() !== day) {
        return false;
    }

    // Validate maxDate (for fecha factura - cannot be greater than today)
    if (fieldId === "fecha-factura-edit" && maxDate) {
        var maxDateObj = new Date(maxDate + "T00:00:00");
        maxDateObj.setHours(0, 0, 0, 0);
        date.setHours(0, 0, 0, 0);
        if (date > maxDateObj) {
            instance.altInput.setCustomValidity("La fecha de factura no puede ser mayor al día actual.");
            return false;
        }
    }

    // Validate minDate (for vencimiento CAE/CAI - cannot be before fecha factura)
    if (fieldId === "venc-caecai-edit") {
        // Get fecha factura value if depends on it
        if (dependsOnFieldId === "fecha-factura-edit") {
            var fechaFacturaInput = document.querySelector("#fecha-factura-edit");
            if (fechaFacturaInput && fechaFacturaInput._flatpickr && fechaFacturaInput._flatpickr.selectedDates.length > 0) {
                var fechaFacturaDate = fechaFacturaInput._flatpickr.selectedDates[0];
                fechaFacturaDate.setHours(0, 0, 0, 0);
                date.setHours(0, 0, 0, 0);
                if (date < fechaFacturaDate) {
                    instance.altInput.setCustomValidity("La fecha Venc. CAE/CAI no puede ser anterior a la fecha de factura.");
                    return false;
                }
            } else if (minDate) {
                // Fallback to minDate if fecha factura is not set
                var minDateObj = new Date(minDate + "T00:00:00");
                minDateObj.setHours(0, 0, 0, 0);
                date.setHours(0, 0, 0, 0);
                if (date < minDateObj) {
                    instance.altInput.setCustomValidity("La fecha Venc. CAE/CAI no puede ser anterior a la fecha de factura.");
                    return false;
                }
            }
        } else if (minDate) {
            var minDateObj = new Date(minDate + "T00:00:00");
            minDateObj.setHours(0, 0, 0, 0);
            date.setHours(0, 0, 0, 0);
            if (date < minDateObj) {
                instance.altInput.setCustomValidity("La fecha Venc. CAE/CAI no puede ser anterior a la fecha de factura.");
                return false;
            }
        }
    }

    instance.altInput.setCustomValidity("");
    return true;
}

window.initFlatpickrWithStrictValidation = function (selector, options) {
    if (typeof flatpickr === 'undefined') {
        console.error('Flatpickr is not loaded.');
        return;
    }

    try {
        var input = document.querySelector(selector);
        if (!input) {
            console.warn('Input element not found:', selector);
            return;
        }

        // Destroy existing Flatpickr instance if it exists
        if (input._flatpickr) {
            input._flatpickr.destroy();
        }

        // Ensure options is an object (handle null/undefined)
        if (!options || typeof options !== 'object' || options === null) {
            options = {};
        }

        // Preserve values from C# anonymous object
        var defaultDate = options.defaultDate;
        var maxDate = options.maxDate;
        var minDate = options.minDate;
        var fieldId = options.fieldId;
        var dependsOnFieldId = options.dependsOnFieldId;

        // Set options to match Processed.razor implementation
        options.allowInput = true;
        options.altInput = true;
        options.altFormat = "d/m/Y";
        options.dateFormat = "Y-m-d";
        options.locale = "es";
        
        // Restore defaultDate if it was provided
        if (defaultDate) {
            options.defaultDate = defaultDate;
        }

        // Set maxDate if provided (for fecha factura - cannot be greater than today)
        if (maxDate) {
            options.maxDate = maxDate;
        }

        // Set minDate if provided (for vencimiento CAE/CAI - cannot be before fecha factura)
        if (minDate) {
            options.minDate = minDate;
        }
        
        options.onChange = [
            function (selectedDates, dateStr, instance) {
                if (instance.altInput) {
                    // Si el campo queda vacío, marcar como inválido
                    if (!instance.altInput.value || instance.altInput.value.trim() === "") {
                        instance.altInput.classList.add("is-invalid");
                    } else {
                        instance.altInput.classList.remove("is-invalid");
                    }

                    // If this is fecha-factura-edit and venc-caecai-edit depends on it, update minDate
                    if (fieldId === "fecha-factura-edit" && dependsOnFieldId === "venc-caecai-edit") {
                        var vencInput = document.querySelector("#venc-caecai-edit");
                        if (vencInput && vencInput._flatpickr) {
                            var fechaFacturaDate = selectedDates.length > 0 ? selectedDates[0] : null;
                            if (fechaFacturaDate) {
                                vencInput._flatpickr.set('minDate', fechaFacturaDate);
                            }
                        }
                    }

                    // Validate date constraints
                    validateDateField(instance, fieldId, maxDate, minDate, dependsOnFieldId);
                }
            }
        ];
        options.onClose = [
            function (selectedDates, dateStr, instance) {
                var input = instance.altInput;
                var val = input ? input.value : "";
                var isValid = false;
                if (val.length === 10) {
                    var parts = val.split("/");
                    if (parts.length === 3) {
                        var day = parseInt(parts[0], 10);
                        var month = parseInt(parts[1], 10);
                        var year = parseInt(parts[2], 10);
                        var date = new Date(year, month - 1, day);
                        isValid =
                            date.getFullYear() === year &&
                            (date.getMonth() + 1) === month &&
                            date.getDate() === day;
                    }
                }
                // Marcar como inválido si está vacío o es inválido
                if (!val || !isValid) {
                    input.classList.add("is-invalid");
                    instance.input.value = '';
                    input.value = '';
                    instance.clear();
                } else {
                    // Validate date constraints
                    var dateValid = validateDateField(instance, fieldId, maxDate, minDate, dependsOnFieldId);
                    if (!dateValid) {
                        input.classList.add("is-invalid");
                    } else {
                        input.classList.remove("is-invalid");
                    }
                }
            }
        ];

        var fp = flatpickr(selector, options);

        setTimeout(function () {
            if (fp && fp.altInput && window.Inputmask) {
                fp.altInput.removeAttribute('readonly');
                Inputmask('99/99/9999').mask(fp.altInput);

                // Validación en input: marca como inválido si está vacío o es inválido
                fp.altInput.addEventListener('input', function () {
                    var val = fp.altInput.value;
                    var isValid = false;
                    if (val.length === 10) {
                        var match = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(val);
                        if (match) {
                            var day = parseInt(match[1], 10);
                            var month = parseInt(match[2], 10);
                            var year = parseInt(match[3], 10);
                            var date = new Date(year, month - 1, day);
                            isValid =
                                date.getFullYear() === year &&
                                (date.getMonth() + 1) === month &&
                                date.getDate() === day;
                        }
                    }
                    if (!val || val.length < 10 || !isValid) {
                        fp.altInput.classList.add('is-invalid');
                    } else {
                        // Validate date constraints
                        var dateValid = validateDateField(fp, fieldId, maxDate, minDate, dependsOnFieldId);
                        if (!dateValid) {
                            fp.altInput.classList.add('is-invalid');
                        } else {
                            fp.altInput.classList.remove('is-invalid');
                        }
                    }
                });

                // Intercepta Enter y Tab para evitar que Flatpickr ajuste la fecha inválida o vacía
                fp.altInput.addEventListener('keydown', function (e) {
                    var val = fp.altInput.value;
                    var isValid = false;
                    if (val.length === 10) {
                        var match = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(val);
                        if (match) {
                            var day = parseInt(match[1], 10);
                            var month = parseInt(match[2], 10);
                            var year = parseInt(match[3], 10);
                            var date = new Date(year, month - 1, day);
                            isValid =
                                date.getFullYear() === year &&
                                (date.getMonth() + 1) === month &&
                                date.getDate() === day;
                        }
                    }
                    if ((e.key === 'Enter' || e.key === 'Tab') && (!val || val.length < 10 || !isValid)) {
                        e.preventDefault();
                        e.stopImmediatePropagation();
                        fp.altInput.classList.add('is-invalid');
                        fp.input.value = '';
                        fp.altInput.value = '';
                        if (fp) fp.clear();
                        return false;
                    }
                }, true);

                // Validación final al perder el foco
                fp.altInput.addEventListener('blur', function () {
                    var val = fp.altInput.value;
                    var isValid = false;
                    if (val.length === 10) {
                        var match = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(val);
                        if (match) {
                            var day = parseInt(match[1], 10);
                            var month = parseInt(match[2], 10);
                            var year = parseInt(match[3], 10);
                            var date = new Date(year, month - 1, day);
                            isValid =
                                date.getFullYear() === year &&
                                (date.getMonth() + 1) === month &&
                                date.getDate() === day;
                        }
                    }
                    if (!val || val.length < 10 || !isValid) {
                        fp.altInput.classList.add('is-invalid');
                        fp.input.value = '';
                        fp.altInput.value = '';
                        if (fp) fp.clear();
                    } else {
                        // Validate date constraints
                        var dateValid = validateDateField(fp, fieldId, maxDate, minDate, dependsOnFieldId);
                        if (!dateValid) {
                            fp.altInput.classList.add('is-invalid');
                        } else {
                            fp.altInput.classList.remove('is-invalid');
                        }
                    }
                });
            }
        }, 100);
    } catch (error) {
        console.error('Error al inicializar Flatpickr:', error);
    }
};
