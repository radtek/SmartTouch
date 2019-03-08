var setTimeOutTimer = 500;

function pageLoader() {
    $('body').append('<div class="pageloader-mask"><span class="pageloader-text">Loading...</span><div class="pageloader-image"></div><div class="pageloader-color"></div></div>');
}
function removepageloader() {
    $('.pageloader-mask').remove('');
}
function innerLoader(appendID) {
    $('#' + appendID).append('<div class="loader-overlay"><figure><img src="../../img/loader.gif"  alt="loading" /></figure></div>');
}
function removeinnerLoader(id) {
    $('#' + id).find('.loader-overlay').remove();
}
//convert given utc time to user timezone
Date.prototype.toUtzDate = function () {
    var now_utc = new Date(this.getUTCFullYear(), this.getUTCMonth(), this.getUTCDate(), this.getUTCHours(), this.getUTCMinutes(), this.getUTCSeconds());
    return offsetFix(now_utc);
}
//convert given time to utc then to user timezone
Date.prototype.ToUtcUtzDate = function () {
    return offsetFix(this);
}
//convert user time to utc time + locale offset
Date.prototype.toUtzUtcDate = function () {
    var offset = moment.tz(this, localStorage.getItem('utz')).utcOffset();
    var utc = new Date(this.getTime() + offset * 60 * 1000);
    var l_offset = new Date().getTimezoneOffset();
    return new Date(utc.getTime() - l_offset * 60 * 1000);
}
//convert localtime to Utc
Date.prototype.toLtUtcDate = function () {
    //this.setTime(this.getMinutes()*60*1000 + this.getTimezoneOffset()*60*1000);

    return new Date(this.getUTCFullYear(), this.getUTCMonth(), this.getUTCDate(), this.getUTCHours(), this.getUTCMinutes(), this.getUTCSeconds());
}

function ToPageDropdown() {
    var arlene1 = [
        { text: 10, value: 10 },
        { text: 25, value: 25 },
        { text: 50, value: 50 },
         { text: 100, value: 100 },
           { text: 250, value: 250 }//,
            //{ text: 500, value: 500 }
    ]

    return arlene1;
}
function ToPageSizeArray() {
    var arlene1 = [
       10, 25, 50, 100, 250//, 500
    ]

    return arlene1;
}
function offsetFix(date) {
    var offset = new moment.tz(date, localStorage.getItem('utz')).utcOffset();
    return new Date(date.getTime() + offset * 60 * 1000)
}
pageLoader();
removepageloader();

if (ko != null) {
    ko.bindingHandlers.stopBinding = {
        init: function () {
            return { controlsDescendantBindings: true };
        }
    };

    ko.virtualElements.allowedBindings.stopBinding = true;
}

// Some general UI pack related JS
// Extend JS String with repeat method
String.prototype.repeat = function (num) {
    return new Array(num + 1).join(this);
};

// Top Nav Dropdown
$(function () {

    // Fix input element click problem
    $('.topnav-inner').click(function (e) {
        e.stopPropagation();
    });
});

(function ($) {

    // Add segments to a slider
    $.fn.addSliderSegments = function (amount) {
        return this.each(function () {
            var segmentGap = 100 / (amount - 1) + "%"
              , segment = "<div class='ui-slider-segment' style='margin-left: " + segmentGap + ";'></div>";
            $(this).prepend(segment.repeat(amount - 2));
        });
    };

    $(function () {

        // Custom Selects
        //$("select[name='huge']").selectpicker({ style: 'btn-hg', menuStyle: 'dropdown-inverse' });

        $("select[name='huge']").selectpicker({ style: 'btn-hg' });

        $("select[name='large']").selectpicker({ style: 'btn-lg' });


        $("select[name='xsmall']").selectpicker({ style: 'btn-xs' });
        $("select[name='xsmallnm']").selectpicker({ style: 'btn-xs btn-normal' });

        $("select[name='medium']").selectpicker();
        $("select[name='mediumnm']").selectpicker({ style: 'btn-normal' });
        $("select[name='mediumdf']").selectpicker({ style: 'btn-default' });
        $("select[name='mediumin']").selectpicker({ style: 'btn-inverse' });

        $("select[name='small']").selectpicker({ style: 'btn-sm' });
        $("select[name='smallnm']").selectpicker({ style: 'btn-sm btn-normal' });
        $("select[name='smalldf']").selectpicker({ style: 'btn-sm btn-default' });
        $("select[name='smallp']").selectpicker({ style: 'btn-sm btn-primary' });
        $("select[name='smalliv']").selectpicker({ style: 'btn-sm btn-inverse' });
        $("select[name='smallif']").selectpicker({ style: 'btn-sm btn-info' });
        $("select[name='smalld']").selectpicker({ style: 'btn-sm btn-danger' });
        $("select[name='smallw']").selectpicker({ style: 'btn-sm btn-warning' });

        $("select[name='info']").selectpicker({ style: 'btn-info' });

        //$("select").selectpicker({ style: 'btn-normal' });


        // Tabs
        $(".nav-tabs a").on('click', function (e) {
            e.preventDefault();
            $(this).tab("show");
        })

        // Tooltips
        $("[data-toggle=tooltip]").tooltip("show");
        $('.tooltiptoggle').tooltip('hide');

        // Tags Input
        //$(".tagsinput").tagsInput();

        // jQuery UI Sliders
        var $slider = $("#slider");
        if ($slider.length > 0) {
            $slider.slider({
                min: 1,
                max: 5,
                value: 3,
                orientation: "horizontal",
                range: "min"
            }).addSliderSegments($slider.slider("option").max);
        }

        var $slider2 = $("#slider2");
        if ($slider2.length > 0) {
            $slider2.slider({
                min: 1,
                max: 5,
                values: [3, 4],
                orientation: "horizontal",
                range: true
            }).addSliderSegments($slider2.slider("option").max);
        }

        var $slider3 = $("#slider3")
          , slider3ValueMultiplier = 100
          , slider3Options;

        if ($slider3.length > 0) {
            $slider3.slider({
                min: 1,
                max: 5,
                values: [3, 4],
                orientation: "horizontal",
                range: true,
                slide: function (event, ui) {
                    $slider3.find(".ui-slider-value:first")
                      .text("$" + ui.values[0] * slider3ValueMultiplier)
                      .end()
                      .find(".ui-slider-value:last")
                      .text("$" + ui.values[1] * slider3ValueMultiplier);
                }
            });

            slider3Options = $slider3.slider("option");
            $slider3.addSliderSegments(slider3Options.max)
              .find(".ui-slider-value:first")
              .text("$" + slider3Options.values[0] * slider3ValueMultiplier)
              .end()
              .find(".ui-slider-value:last")
              .text("$" + slider3Options.values[1] * slider3ValueMultiplier);
        }

        // Add style class name to a tooltips
        $(".tooltip").addClass(function () {
            if ($(this).prev().attr("data-tooltip-style")) {
                return "tooltip-" + $(this).prev().attr("data-tooltip-style");
            }
        });

        // Placeholders for input/textarea
        $("input, textarea").placeholder();

        // Make pagination demo work
        $(".pagination a").on('click', function () {
            $(this).parent().siblings("li").removeClass("active").end().addClass("active");
        });

        $(".btn-group a").on('click', function () {
            $(this).siblings().removeClass("active").end().addClass("active");
        });

        //Icon Bar
        $(".iconbar ul li a").on('click', function () {
            $(".iconbar ul li a.active").removeClass("active");
            $(this).addClass("active");
        });

        // Disable link clicks to prevent page scrolling
        $('a[href="#fakelink"]').on('click', function (e) {
            e.preventDefault();
        });

        // jQuery UI Spinner
        $.widget("ui.customspinner", $.ui.spinner, {
            widgetEventPrefix: $.ui.spinner.prototype.widgetEventPrefix,
            _buttonHtml: function () { // Remove arrows on the buttons
                return "" +
                "<a class='ui-spinner-button ui-spinner-up ui-corner-tr'>" +
                  "<span class='ui-icon " + this.options.icons.up + "'></span>" +
                "</a>" +
                "<a class='ui-spinner-button ui-spinner-down ui-corner-br'>" +
                  "<span class='ui-icon " + this.options.icons.down + "'></span>" +
                "</a>";
            }
        });

        $('#spinner-01').customspinner({
            min: -99,
            max: 99
        }).on('focus', function () {
            $(this).closest('.ui-spinner').addClass('focus');
        }).on('blur', function () {
            $(this).closest('.ui-spinner').removeClass('focus');
        });


        // Focus state for append/prepend inputs
        $('.input-group').on('focus', '.form-control', function () {
            $(this).closest('.form-group, .navbar-search').addClass('focus');
        }).on('blur', '.form-control', function () {
            $(this).closest('.form-group, .navbar-search').removeClass('focus');
        });


        // Table: Toggle all checkboxes
        $('.table .toggle-all').on('click', function () {
            var ch = $(this).find(':checkbox').prop('checked');
            $(this).closest('.table').find('tbody :checkbox').checkbox(!ch ? 'check' : 'uncheck');
        });

        // Table: Add class row selected
        $('.table tbody :checkbox').on('check uncheck toggle', function (e) {
            var $this = $(this)
              , check = $this.prop('checked')
              , toggle = e.type == 'toggle'
              , checkboxes = $('.table tbody :checkbox')
              , checkAll = checkboxes.length == checkboxes.filter(':checked').length

            $this.closest('tr')[check ? 'addClass' : 'removeClass']('selected-row');
            if (toggle) $this.closest('.table').find('.toggle-all :checkbox').checkbox(checkAll ? 'check' : 'uncheck');
        });

        // jQuery UI Datepicker
        var datepickerSelector = '#datepicker-01, #datepicker-02, #datepicker-03, #datepicker-04, #datepicker-05, #datepicker-06 ,#datepicker-07,#datepicker-08,#datepicker-09,#datepicker-10';
        $(datepickerSelector).datepicker({
            showOtherMonths: true,
            selectOtherMonths: true,
            dateFormat: "d MM, yy",
            yearRange: '-1:+1'
        }).prev('.btn').on('click', function (e) {
            e && e.preventDefault();
            $(datepickerSelector).focus();
        });
        $.extend($.datepicker, { _checkOffset: function (inst, offset, isFixed) { return offset } });

        // Now let's align datepicker with the prepend button
        $(datepickerSelector).datepicker('widget').css({ 'margin-left': -$(datepickerSelector).prev('.input-group-btn').find('.btn').outerWidth() });

        // Switch
        $("[data-toggle='switch']").wrap('<div class="switch" />').parent().bootstrapSwitch();

        // make code pretty
        window.prettyPrint && prettyPrint();

        // Filters

        $('ul.label-fillters li a').click(function () {
            $('ul.label-fillters li.active').removeClass('active');
            $(this).parent('li').addClass('active');
        });

        // Custom ST Table: Toggle all checkboxes

        $('.st-tabel .toggle-all').on('click', function () {
            var ch = $(this).find(':checkbox').prop('checked');
            $(this).closest('.st-tabel').find('.grid-row :checkbox').checkbox(!ch ? 'check' : 'uncheck');

            if ($(this).hasClass('checked')) {
                $(this).closest('.st-tabel').find('.grid-row').each(function () {
                    $(this).removeClass('selected');
                });
            }
            else {

                $(this).closest('.st-tabel').find('.grid-row').each(function () {
                    $(this).addClass('selected');
                });

            }

        });

        $('.st-tabel .grid-row li').find('label.checkbox').on('click', function () {
            var ch = $(this).find(':checkbox').prop('checked');
            if ($(this).hasClass('checked')) {
                $(this).parents('ul.grid-row').removeClass('selected');
            }
            else {
                $(this).parents('ul.grid-row').addClass('selected');
            }
        });


        $('.st-tabel .grid-body ul:even').css('background-color', 'rgba(249, 249, 249, 0.9)');
        $('.st-tabel .grid-body ul:first').css('background-color', '#f6f8fa');

        $('.st-lg a').click(function () {
            $('.st-lg a.active').removeClass('active');
            $(this).addClass('active');
        });


        $(document).on('hidden.bs.modal', function (e) {
            $(e.target).removeData('bs.modal');
        });
    });


})(jQuery);


// Filter block

$('.filter-btn').click(function () {
    //$(this).toggleClass("active", "in-active");
    $(this).parents().find('.filter-block').slideToggle();
});
//$('.filter-block label.filter').click(function () {
//    $(this).toggleClass('active', '');
//    alert(1);
//});


/* 
Activities */

//$(".activities").click(function (event) {
//    event.stopPropagation();
//});

$('.activities').on('click', function (e) {
    if (e.target !== this) {
        if ($(e.target).hasClass('notifiy-view')) {
        }
        else {
            e.stopPropagation();
        }
    }
    else {
        e.stopPropagation();
    }
});

function ConvertToDate(date) {
    var milli = date.replace(/\/Date\((-?\d+)\)\//, '$1');
    var d = new Date(parseInt(milli));
    return d;
}

function GetMilliSeconds(date) {
    var milliSeconds = Date.parse(date);
    return milliSeconds;
}

function ConvertToCurrectDate(date) {
    var timeZoneDifference = date.getTimezoneOffset() * 60 * 1000; // 5 minutes in milliseconds
    date.setTime(date.getTime() - timeZoneDifference);
    return date;
}

function GetDifferenceOfTwoDatesInDays(dateOne, dateTwo) {
    //var diffDays = parseFloat(((dateOne - dateTwo)) / (1000 * 60 * 60 * 24)).toFixed(2);
    var diffDays = Math.round(parseFloat(((dateOne - dateTwo)) / (1000 * 60 * 60 * 24)));
    return diffDays;
}


function searchOpen() {
    $('.advanced-search-inner').toggleClass('advanced-search-inner-visible', '');
    setTimeout(function () {
        $("#quickSearchInput").focus();
    }, 500);
}


$(document).on('propertychange keyup input paste', 'input.advancedsearchinput', function () {
    $(this).next('.clear-text').css('opacity', '1');
    $(this).next('.clear-text').css('visibility', 'visible');
    $('.search-unified').addClass('quick-search-inner-visible');
}).on('click', '.clear-text', function () {
    setTimeout(function () {
        $("#quickSearchInput").focus();
    }, 500);
    $(this).delay(300).fadeTo(300, 0).prev('input').val('');
    $(this).next('.clear-text').css('opacity', '0');
    $(this).next('.clear-text').css('visibility', 'hidden');
    $('.search-unified').removeClass('quick-search-inner-visible');
});


// Add Action

$('#selectactiontype').change(function () {
    $('#actiontype1, #actiontype2, #actiontype3, #actiontype4, #actiontype5').addClass('hide');
    $('#actiontype' + $(this).find('option:selected').val()).removeClass('hide');
});


//$("body").click( function (e) {

//      var el = e.target;
//      var elmp = $('.moreinner.show').attr('id');
//      var elmpt = $('.dp-topnav-inner.show').attr('id');


//      if ($(el).parents('div.moreinner').length == 0 && $(el).parents('div.top-nav').length == 0) {
//            $('li.btn-group.showmore').removeClass('showmore');
//            $('#' + elmp).removeClass('show').addClass('hide');
//      }

//      //alert($(el).parent());
//      if ($(el).parents('div.dp-topnav-inner.show').length === 0 && $(el).parents('li.btn-group.showtopinner').length === 0) {
//          $('li.btn-group.showtopinner').removeClass('showtopinner');
//          $('#' + elmpt).removeClass('show').addClass('hide');
//      }
//  }
//);


function OpenMoreId(obj) {
    $(".dp-topnav-inner").removeClass('show').addClass('hide');
    $(obj).parents('.top-nav').find('li.btn-group.showtopinner').removeClass('showtopinner');
    $(obj).parents('li.btn-group').addClass('showmore');
    $('#' + $(obj).attr('data-openid')).removeClass('hide').addClass('show');
    $('#' + $(obj).attr('data-openid')).find('a.active').removeClass('active');
    //fnShowMore();
}

function OpenTopInner(menuItemId, menuItemContentId) {
    $(".dp-topnav-inner, .moreinner").removeClass('show').addClass('hide');
    $("#" + menuItemId).parents('.top-nav').find('li.btn-group.showtopinner, li.btn-group.showmore').removeClass('showtopinner showmore');
    $("#" + menuItemId).closest('li.btn-group').addClass('showtopinner');
    $('#' + menuItemContentId).removeClass('hide').addClass('show');
    $('#' + menuItemContentId).find('a.active').removeClass('active');
    setTimeout(function () {
        $('#' + menuItemContentId).removeClass('loader');
    }, 500);
}

function CloseTopInner(obj) {
    $(".dp-topnav-inner, .moreinner").removeClass('show').addClass('hide loader');
    $(obj).parents('.top-nav').find('li.btngroup.showtopinner, li.btn-group.showmore').removeClass('showtopinner showmore');
    $(obj).closest('li.btn-group').removeClass('showtopinner');
    $(obj).removeClass('active');
}


// Alertify

//function alertifyReset(okText,cancelText) {
function alertifyReset() {
    $("#toggleCSS").attr("href", "../themes/alertify.default.css");
    alertify.set({
        labels: {
            //ok: okText,
            //cancel: cancelText
            ok: "OK",
            cancel: "Cancel"
        },
        delay: 5000,
        buttonReverse: true,
        buttonFocus: "ok"
    });
}

function notifySuccess(message) {
    alertifyReset();
    alertify.success(message)
    return false;
}
function notifyError(message) {
    if (message.hasOwnProperty('statusText')) {
        if (message.statusText == 'abort')
            return;
    }
    alertifyReset();
    if (message == "[object Object]") {
        message = "An error occured, please contact administrator"
    }
    alertify.error(message);
}
function notifyInfo(message) {
    alertifyReset();
    alertify.log(message);
}

function notifyConfirm(message, okFn, delrecore, canFn, cancelMsg) {
    //function notifyConfirm(message) {
    alertifyReset();
    alertify.confirm(message, function (e) {
        if (e) {
            okFn(delrecore);
            return "ok";
        } else {
            canFn(cancelMsg);
            return "cancel";
        }
    });
}


function appendRadio() {
    $('[data-toggle="radio"]').each(function () {
        var $radio = $(this);
        $radio.radio();
    });
}

function appendCheckbox() {
    $('[data-toggle="checkbox"]').each(function () {
        var $checkbox = $(this);
        $checkbox.checkbox();
    });
}


function radiobtnActive(optionid) {
    $(optionid).closest('.btn-group').find('label.active').removeClass('active');
    $(optionid).parent('label.btn').addClass('active');
}

//$(".confirm").on('click', function () {
//    alertifyReset();
//    alertify.confirm("This is a confirm dialog", function (e) {
//        if (e) {
//            alertify.success("You've clicked OK");
//        } else {
//            alertify.error("You've clicked Cancel");
//        }
//    });
//    return false;
//});


// Table Toggle

function tableMasterCheckBox(gridclass) {
    var masterID = $('#masterCheckBox,#masterCheckBox_all');
    var classSelecter = gridclass + " table";
    masterID.change(function () {
        if ($(this).is(':checked')) {
            console.log("in checked 1");
            console.log($(this).context.id);

            $('.' + classSelecter).find('tbody :checkbox').prop('checked', true);
            //$('.' + classSelecter).find('tbody :checkbox').parents('tr').addClass('k-state-selected');
            $('.' + classSelecter).find('tbody :checkbox').parent('label.checkbox').addClass('checked');

            $("#selectcheckbox").addClass("checked");

            if ($("#selectcheckbox_text").text() == "All Pages") {

                selectallsearchstring = (localStorage.getItem("searchtext") == null ? "" : localStorage.getItem("searchtext")) + "$" +
                             (localStorage.getItem("searchcontent") == null ? "0" : localStorage.getItem("searchcontent")) + "$" + (localStorage.getItem("sortcontent") == null ? "0" : localStorage.getItem("sortcontent"));

                createCookie("selectallsearchstring", selectallsearchstring, 1);
            }
        }
        else {
            $('.' + classSelecter).find('tbody :checkbox').prop('checked', false);
            $('.' + classSelecter).find('tbody :checkbox').parents('tr').removeClass('k-state-selected');
            $('.' + classSelecter).find('tbody :checkbox').parent('label.checkbox').removeClass('checked');

            createCookie("selectallsearchstring", "", 1);
        }
    });
}

//tableMasterCheckBox('contacts-grid');

function bindCheckboxchnage(checkboxClass) {

    var gridCheckBox = $('.' + checkboxClass);
    var masterID = $('#masterCheckBox,#masterCheckBox_all');
    gridCheckBox.change(function () {
        if (!$(this).is(':checked')) {
            console.log("in checked 2");
            console.log($(this));
            console.log($(this).id);
            masterID.attr('checked', false);
            masterID.parent('label.checkbox').removeClass('checked');
            $(this).parents('tr').removeClass('k-state-selected');
            $('#maincheckbox').addClass("checked");

            if ($("#selectcheckbox_text").text() == "All Pages") {
                gridCheckBox.attr('checked', false);
                gridCheckBox.parent('label.checkbox').removeClass('checked');
                gridCheckBox.parents('tr').removeClass('k-state-selected');
                createCookie("selectallsearchstring", "", 1);
            }

        }
        else {
            if (gridCheckBox.length == $('.' + checkboxClass + ':checked').length) {
                masterID.attr('checked', 'checked');
                masterID.parent('label.checkbox').addClass('checked');
            }
            $(this).parents('tr').addClass('k-state-selected');
            $('#maincheckbox').removeClass("checked");
        }
    });
}



// Data Adding 

$('#addmeetingemail').on("click", function () {
    $('#meetingemail').append('<div class="form-group input-group mbs"><input type="email" class="form-control cu-large" value="" placeholder="xyz@smarttouchinteractive.com" /><span class="input-group-btn"><button type="button" class="btn btn-default email-trash"><span class="fui-cross"></span></button></span></div>');
});

$('#addphonetype').on("click", function () {
    $('#phonetype').append('<div class="form-group input-group cu-input-group mbs"><input type="text" placeholder="99009988890" class="form-control cu-small"><select name="mediumdf"  class="select-block"><option value="0">Work</option><option value="1">Home</option><option value="2">Private</option><option value="3">Mobile</option></select><span class="input-group-btn"><button type="button" class="btn btn-default email-trash"><span class="fui-cross"></span></button></span></div>');
    $("select[name='mediumdf']").selectpicker({ style: 'btn-default' });
});


$('#addrelation').on("click", function () {
    $('#relationtype').append('<div class="row"><div class="col-md-6 pln"><div class="form-group "><label class="control-label">Reminder method</label><select name="mediumnm" class="select-block"><option>Pop-up</option><option>Email</option><option>Pop-up & Email</option></select></div></div><div class="col-md-6 prn"><div class="form-group "><label class="control-label">Reminder Timeframe</label><select name="mediumnm" class="select-block"><option>At time of event</option><option selected>5 minutes before</option><option>10 minutes before</option><option>15 minutes before</option><option>30 minutes before</option><option>1 hour before</option><option>2 hour before</option><option>1 day before</option><option>2 day before</option><option>1 week before</option></select></div></div></div>');
    $(this).hide();
    $("select[name='mediumnm']").selectpicker({ style: 'btn-normal' });
});
$('#addactionreminder').on("click", function () {
    $('#actionsreminder').removeClass('hide');
    $(this).addClass('hide');
});

function actionreminderAdd(obj) {
    $(obj).parents('.reminder-block').find('.actionsreminder-data').toggleClass('hide', '');
    $(obj).find('i').toggleClass('st-icon-add st-icon-remove');
}


$('#addsocialtype').on("click", function () {
    $('#socialtype').append('<div class="form-group input-group cu-input-group mbs"><input type="text" placeholder="http://207.200.34.228/smartcrm/addcontacts.html" class="form-control cu-small"><select name="mediumdf" class="select-block"> <option value="0">Blog</option><option value="1">Facebook</option><option value="2">Twitter</option><option value="3">Linkedin</option></select><span class="input-group-btn"><button type="button" class="btn btn-default email-trash"><span class="fui-cross"></span></button></span></div>');
    $("select[name='mediumdf']").selectpicker({ style: 'btn-default' });
});


$('.layout a').click(function () {
    $('.campaigns-layouts li').find('div.layout.selected').removeClass('selected');
    $(this).parent('div.layout').addClass('selected');
});

$('#addfields').on("click", function () {
    $('#fields').append('<div class="row"><div class="form-group col-md-6 pln"><label>Contact Field</label><select name="mediumnm" class="select-block"><option value="0">Full Name</option><option value="1">Company</option><option value="3">Phone & Type</option><option value="2">Lifecycle</option></select></div><div class="form-group col-md-6 prn"><label>Value</label><input type="text" class="form-control" placeholder="Grant"></div></div>');
    $("select[name='mediumnm']").selectpicker({ style: 'btn-normal' });
});

//Work Flow Trash
$('.workflow-header-controls .st-trash-black').on("click", function () {
    $(this).parents('div.workflow-panel-box').remove();
});

$('.add-newaction, .workflow-header-controls .st-copy-black').on("click", function () {
    $(this).parents('div.workflow-panel-box').clone().insertAfter($(this).parents('div.workflow-panel-box')).find('div.select.select-block').remove();
    $("select[name='mediumnm']").selectpicker({ style: 'btn-normal' });
});


$('.email-trash').click(function () {
    $(this).parents('div.input-group').remove();
});

$('#sidebar-collapse').click(function () {
    $('.left-bar, .main-container').toggleClass('sidebar-collapsed');
    $('#sidebar-collapse').find('i').toggleClass('fui-arrow-right fui-arrow-left');
    $('#leftsearch').addClass('sidebarsearch');
    Scroll('campaignsdesignarea-body');
});
$('#leftsearch').click(function () {
    $('.left-bar.sidebar-collapsed, .main-container.sidebar-collapsed').removeClass('sidebar-collapsed');
    $('#sidebar-collapse').find('i').toggleClass('fui-arrow-right fui-arrow-left');
    Scroll('campaignsdesignarea-body');
});

$('#ccbccbtn').click(function () {
    $('#ccbcc').toggleClass('hide');
    $('#ccbccbtn').find('span').toggleClass('st-right-arrow st-down-arrow');
});


$('.closemore').click(function () {
    $('li.btn-group.showmore').removeClass('showmore');
    $(this).parents('.moreinner.show').removeClass('show').addClass('hide');
});

function Settings(slidClass) {
    $("." + slidClass).click(function () {
        $(".drguploderbtn").toggleClass('open', 'close')
        $('#drguploder, .setting-st').fadeToggle('');
    })
}

Settings('drguploderbtn');
Settings('closesettings');


//  Different device views
function campaignView(obj) {
    $(obj).parent('.campaign-viewbtn').find('a.active').removeClass('active');
    $(obj).addClass('active');
    $('#reviewcampaignacc .campaigns-body').removeClass('tablet mobile desktop');
    $('#reviewcampaignacc .campaigns-body').addClass($(obj).attr('data-class'));
}




//  Drag-control Tabing

function dragControlViews(obj) {
    selfDesigner.currentControlBox($(obj).attr("id"));
    if ($(obj).attr("data-name") == "Image") { GetImagessearch(); }
    $('.drag-control a.active').removeClass('active');
    $(obj).addClass('active');
    // $('.drag-controls-content .content-block.active').removeClass('active').fadeOut('');
    $('#' + $(obj).attr('data-target')).addClass('active').fadeIn('');
}


//  Control Tabing

function ControlViews(obj) {
    $('.ControlViews a.active').removeClass('active');
    $(obj).addClass('active');
    $('.drag-controls .ControlViews-block.active').removeClass('active').fadeOut('');
    $('#' + $(obj).attr('data-target')).addClass('active').fadeIn('');
}

//  Merge Control Tabing

function mergeControlViews(obj) {
    $('.merge-control-views a.active').removeClass('active');
    $(obj).addClass('active');
    $('.merge-controls-content .merge-content-block.active').removeClass('active').fadeOut('');
    $('#' + $(obj).attr('data-target')).addClass('active').fadeIn('');
}



// Inline Edit 

var Previousvalue = "";

$('#EditCampaignname').click(function () {
    var namv = $('#pagename').text();
    $('#pagename, #EditCampaignname').hide();
    Previousvalue = namv;
    $('#pagenameinput').show().find('input').val(namv).focus();

});
$('#pagenameinputval').keyup(function (e) {
    var varvalue = "";
    if (e.keyCode == 13) {
        varvalue = $('#pagenameinput').find('input').val();
    }
    //else if (e.keyCode == 27) {
    //    alert(varvalue);
    //    varvalue = Previousvalue;
    //}

    if (varvalue.trim() != "") {
        $('#pagename, #EditCampaignname').show();
        $('#pagenameinput').hide();
        $('#pagename').text(varvalue);
    }
});

function fnSaveorCancel(obj) {
    var varvalue = "";
    if ($(obj).attr('data-opt') == "save") {
        varvalue = $('#pagenameinput').find('input').val();
    }
    else {
        varvalue = Previousvalue;
    }

    if (varvalue.trim() != "") {
        $('#pagename, #EditCampaignname').show();
        $('#pagenameinput').hide()
        $('#pagename').text(varvalue);
    }
}

function editEntityName() {
    $('.control-label').addClass('hide');

}

$('#checkduplicates').on('click', function () {
    $('#checkduplicatesprogress').removeClass('hide').find('.progress-bar-success').animate({
        width: "40%"
    }, 500);
    $(this).text('Show Duplicates');
    setTimeout(function () {
        $('#checkduplicates').attr('href', 'contactduplicate.html');
    }, 500);
});

$('#actionremindertimeframe').change(function () {
    $('#actionremindertimer').addClass('hide');
    if ($(this).find('option:selected').val() == 5) {
        $('#actionremindertimer').removeClass('hide');
    }
});


$('.savevalue').on("click", function () {
    $(this).closest('.custom-fields').find('.addcustomfield').removeClass('hide');
    $(this).closest('.new-custom-field').remove();
});

$('.adddropdownvalue').on("click", function () {
    var adddropdownvalue = '<div class="form-group"><div class="input-group"><input type="text" class="form-control" placeholder="Opction Value" /><span class="input-group-btn"><button class="btn btn-default close-optionvalue"><span class="fui-cross"></span></button></span></div></div>';
    $(adddropdownvalue).insertBefore(this);
});


$('.addsection').on("click", function () {
    var addsection = '<div class="panel panel-default "><div class="form-horizontal-large custom-fields panel-body"><div class="legend">Section</div><a href="javascript:void(0)" class="addcustomfield"><span class="st-rounded-addicon"></span> Add Custom Field</a></div></div>';
    $(addsection).insertBefore(this);
});

$('.close-optionvalue').on("click", function () {
    $(this).closest('div.form-group').remove();
});

$('#leadcondition').change(function () {
    $('#leadcondition3, #leadcondition4, #leadcondition5, #leadcondition7, #leadcondition8').addClass('hide');
    $('#leadcondition' + $(this).find('option:selected').val()).removeClass('hide');
});

$('#daterangecustom').change(function () {
    $('#daterange5').addClass('hide');
    $('#daterange' + $(this).find('option:selected').val()).removeClass('hide');
});



$('#accountlogin').on("click", function () {
    if ($('#accountname').val() == 'admin') {
        $(this).attr('href', 'accountlist.html');
    } else {
        $(this).attr('href', '../../Views/Contacts/contacts.html');
    }
});

$('input:radio[name="TimeSensitive"]').change(function () {
    if ($('#TimeSensitiveYes').is(':checked')) {
        $("#deactivateworkflowdate").removeClass('hide');
    }
    else if ($('#TimeSensitiveNo').is(':checked')) {
        $("#deactivateworkflowdate").addClass('hide');
    }
});
$('input:radio[name="enrolledworkflow"]').change(function () {
    if ($('#enrolledworkflowRemove').is(':checked')) {
        $("#enrolledworkflowOpt").removeClass('hide');
    }
    else if ($('#enrolledworkflowYes').is(':checked')) {
        $("#enrolledworkflowOpt").addClass('hide');
    }
    else if ($('#enrolledworkflowNo').is(':checked')) {
        $("#enrolledworkflowOpt").addClass('hide');
    }
});
$('input:checkbox').change(function () {
    if ($('#notifymechecked').is(':checked')) {
        $("#notifyme").removeClass('hide');
    } else {
        $("#notifyme").addClass('hide');
    }
});

$('#mergebtn').on('click', function () {
    $('#mergeconfirmationdata').removeClass('hide');
    $(this).text('Continue Merging');

});

// View html and design
function OpenCPview(obj) {
    $('#campaign-tabs a.active').removeClass('active');
    $(obj).addClass('active');
    $(obj).parents('.campaigns-droparea').find('.campaigns-template.show').removeClass('show').addClass('hide');
    $('#' + $(obj).attr('data-CPview')).removeClass('hide').addClass('show');
}

function OpenTCview(obj) {
    $('#templatechange a.active').removeClass('active');
    $(obj).addClass('active');
}

function viewCamHtml(abj) {
    $('#campaign-tabs a.active').removeClass('active');
    $(abj).addClass('active');
    $('.campaignsdesignarea-body').find('textarea').val($('#campaignsdesignarea ul.campaigns-list').html());
    $('#canvas').removeClass('show').addClass('hide');
    $('#fhtml').removeClass('hide').addClass('show');
}
function viewCamDesign(abj) {
    $('#campaign-tabs a.active').removeClass('active');
    $(abj).addClass('active');
    $('#campaignsdesignarea').find('ul.campaigns-list').html($('.campaignsdesignarea-body').find('textarea').val());
    $('#canvas').removeClass('hide').addClass('show');
    $('#fhtml').removeClass('show').addClass('hide');
}
// End

$('.tag-edit').on("click", function () {
    var tagname = $(this).parents('ul.grid-row').find('li.tag-name').text();
    $(this).parents('ul.grid-row').find('li.tag-name').html('').append('<input type="text" value="" class="form-control display-inline tag-input" />');
    $(this).parents('ul.grid-row').find('input.tag-input').val(tagname);
    $(this).find('i').removeClass('st-grid-edit').addClass('st-tick-green');
    $(this).removeClass('tag-edit').addClass('tag-save');
});
$('.tag-save').on("click", function () {
    var tagvalue = $(this).parents('ul.grid-row').find('input.tag-input').val();
    $(this).parents('ul.grid-row').find('li.tag-name').html('').append(tagvalue);
    $(this).find('i').removeClass('st-tick-green').addClass('st-grid-edit');
    $(this).removeClass('tag-save').addClass('tag-edit');
    $('body').append('');
});



function createCookie(name, value, days) {
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        var expires = "; expires=" + date.toGMTString();
    } else var expires = "";
    document.cookie = escape(name) + "=" + escape(value) + expires + "; path=/";
}

function readCookie(name) {
    var nameEQ = escape(name) + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return unescape(c.substring(nameEQ.length, c.length));
    }
    return null;
}

function eraseCookie(name) {
    createCookie(name, "", -1);
}
function AssignURl() {

}

function actionCompleted(actioncomclass) {
    $('.' + actioncomclass).on('change', function () {
        $(this).closest('.action').find('.st-arrowleft').toggleClass('hide show');
    });
}
actionCompleted('setcompleted');


function selectedContacts(Id, checkedContacts, contacts) {
    arrayContacts = [];
    if (Id == 0) {
        if (checkedContacts.length != 0) {
            $.each(checkedContacts, function (index, value) {
                var contactDetails = value.split('|');
                var fullname = "";
                if (contactDetails[3] != null && contactDetails[3] != 'undefined' && contactDetails[3] != "null" && contactDetails[1] != " " && contactDetails[3] != 'Email Not Available')
                    fullname = contactDetails[1] + " ( " + contactDetails[3] + " ) ";
                else if (contactDetails[2] != null && contactDetails[2] != "" && contactDetails[2] != 'null' && contactDetails[4] != 2 && contactDetails[1] != " ")
                    fullname = contactDetails[1] + ":" + " " + contactDetails[2];
                else if (contactDetails[4] == 2)
                    fullname = contactDetails[2];
                else if (contactDetails[3] != null && contactDetails[3] != 'undefined' && contactDetails[3] != "null" && contactDetails[1] == " ")
                    fullname = contactDetails[3];
                else
                    fullname = contactDetails[1];
                arrayContacts.push({ Address: null, CompanyName: contactDetails[2], ContactType: contactDetails[4], FullName: fullname, Id: contactDetails[0] });
            });
            return arrayContacts;
        }
        else {
            if (localStorage.getItem("contactdetails") != null && typeof selfDetails != "undefined") {

                localStorage.setItem("contactdetails", selfDetails.contactdetails());

                var contactdtls = localStorage.getItem("contactdetails");
                var contactDetails = contactdtls.split('|');
                var fullname = "";

                if (contactDetails[1] != " " && contactDetails[4] != null && contactDetails[4] != 'undefined' && contactDetails[4] != "null" && contactDetails[4] != 'Email Not Available')
                    fullname = contactDetails[1] + " ( " + contactDetails[4] + " ) ";

                else if (contactDetails[2] != null && contactDetails[1] != " " && contactDetails[2] != "null")
                    fullname = contactDetails[1] + ":" + " " + contactDetails[2];
                else if (contactDetails[2] == contactDetails[1])
                    fullname = contactDetails[2]
                else if (contactDetails[4] != null && contactDetails[4] != 'undefined' && contactDetails[4] != "null" && contactDetails[1] == " ")
                    fullname = contactDetails[4];
                else
                    fullname = contactDetails[1];
                arrayContacts.push({ Address: null, CompanyName: contactDetails[2], ContactType: 0, FullName: fullname, Id: contactDetails[0] });
                return arrayContacts;
            }
        }
    }
    else
        return contacts;
}
function selectedContactEmails(checkedContactEmailValues, contacts, serviceProvider) {
    arrayContacts = [];
    if (checkedContactEmailValues.length != 0) {

        $.each(checkedContactEmailValues, function (index, value) {
            var contactDetails = value.split('|');
            var fullname = "";
            if (contactDetails[3] != null && contactDetails[3] != "" && contactDetails[3] != 'null' && contactDetails[3] != 'undefined'
                && contactDetails[3] != 'Email Not Available' && contactDetails[5] == 'false' &&
                contactDetails[1] != null && contactDetails[1] != 'null' && contactDetails[1] != "" && contactDetails[1] != 'undefined' && contactDetails[6] != 53 && contactDetails[6] != 54 && contactDetails[6] != 57) {
                if ((checkedContactEmailValues.length >= 1 && serviceProvider == 4) || (checkedContactEmailValues.length >= 1 && serviceProvider == 2)) {
                    fullname = contactDetails[1] + " " + "<" + contactDetails[3] + ">" + " " + "*";
                    arrayContacts.push({ DocumentId: contactDetails[0], Text: fullname, FullName: fullname, DocumentOwnedBy: 0, Type: "To" });
                }
                else if (checkedContactEmailValues.length > 1 && serviceProvider !== 4) {
                    fullname = contactDetails[1] + " " + "<" + contactDetails[3] + ">" + " " + "*";
                    arrayContacts.push({ DocumentId: contactDetails[0], Text: fullname, FullName: fullname, DocumentOwnedBy: 0, Type: "BCC" });
                }
            }
        });
    }

    else {
        if (localStorage.getItem("contactdetails") != null) {
            var contactdtls = localStorage.getItem("contactdetails");
            var contactDetails = contactdtls.split('|');
            var fullname = "";
            if (contactDetails[5] == 'false') {
                if (contactDetails[1] != null && contactDetails[1] != "null" && contactDetails[4] != 'Email Not Available' && contactDetails[4] != null && contactDetails[4] != 'null' && contactDetails[6] != '53' && contactDetails[6] != '54'
                    && contactDetails[6] != '57') {
                    fullname = contactDetails[1] + " " + "<" + contactDetails[4] + ">" + "*";
                    arrayContacts.push({ DocumentId: contactDetails[3], Text: fullname, FullName: fullname, DocumentOwnedBy: 0, Type: "To" });
                }
                return arrayContacts;
            }
        }
    }

    return arrayContacts;
}
function selectedContactPhoneNumbers(checkedContactPhoneNumbers, contacts) {
    arrayContacts = [];
    if (checkedContactPhoneNumbers.length > 0) {
        $.each(checkedContactPhoneNumbers, function (index, value) {
            var contactDetails = value.split('|');
            var fullname = "";
            if (contactDetails[3] != null && contactDetails[3] != "" && contactDetails[3] != 'null'
                && contactDetails[3] != 'undefined' && contactDetails[3] != '(xxx) xxx - xxxx' && contactDetails[1] != null
                && contactDetails[1] != 'null' && contactDetails[1] != "" && contactDetails[1] != 'undefined') {

                // splitting the phonetype and number . first part is phone number and second part is phone type
                var PhoneString = contactDetails[3].split(",");

                fullname = contactDetails[1] + " " + "(" + PhoneString[1] + ": " + formatPhone(PhoneString[0]) + ") *";
                //push to arry
                arrayContacts.push({ DocumentId: contactDetails[0], Text: fullname.toString(), FullName: fullname, DocumentOwnedBy: 0, Type: "To", ContactID: contactDetails[5] });
            }
        });
    }
    else {
        if (localStorage.getItem("contactdetails") != null) {
            var contactdtls = localStorage.getItem("contactdetails");
            var contactDetails = contactdtls.split('|');
            var fullname = "";
            if (contactDetails.length >= 10) {

                // splitting the phonetype and number . first part is phone number and second part is phone type
                var PhoneString = contactDetails[8].split(",");
                contactDetails[8] = formatPhone(PhoneString[0]);

                //push to arry
                if (contactDetails[1] != null && contactDetails[1] != "null" && contactDetails[8] != '' && contactDetails[8] != null) {
                    fullname = contactDetails[1] + " " + "(" + contactDetails[9] + ": " + contactDetails[8] + ") *";
                    arrayContacts.push({ DocumentId: contactDetails[7], Text: fullname, FullName: fullname, DocumentOwnedBy: 0, Type: "To",ContactID: contactDetails[0] });
                }
            }
        }
    }
    return arrayContacts;
}



function formatPhone(phonenum) {
    phonenum = phonenum.replace(/ +/g, "");
    var regexObj = /^(?:\+?1[-. ]?)?(?:\(?([0-9]{3})\)?[-. ]?)?([0-9]{3})[-. ]?([0-9]{4})$/;
    if (regexObj.test(phonenum)) {
        var parts = phonenum.match(regexObj);
        var phone = "";
        if (parts[1]) { phone += "(" + parts[1] + ") "; }
        phone += parts[2] + "-" + parts[3];
        return phone;
    }
    else {
        return phonenum;
    }
}

// 
$('input:radio[name="schedule"]').change(function () {
    if ($('#setschedule').is(':checked')) {
        $("#schedule-detail").removeClass('hide');

    }
    else {
        $("#schedule-detail").addClass('hide');
    }
});

$('#forgotpassword').click(function () {
    $('#logincontrols').fadeOut('');
    $('#forgotcontrols').fadeIn('');
});

$('#backtologin, #forgotsubmit').click(function () {
    $('#logincontrols').fadeIn('');
    $('#forgotcontrols').fadeOut('');
});


// Nav toggle

$(window).resize(function () {
    navtoggle();
    //mobileView();
});

// Dropdown Filkering 
$(window).on("scroll", function () {
    var scrollHeight = $(document).height();
    var scrollPosition = $(window).height() + $(window).scrollTop();
    if ((scrollHeight - scrollPosition) / scrollHeight === 0) {
        $("body").height($("body").height() + 1);
      //  $("body").height($("body").height() - 10);
    }
});

$('.nav-toggle').remove();
$('<a href="javascript:void(0)" class="icon nav-toggle st-icon-cross" style="opacity: 1; left: 82px;" data-original-title="" title=""></a>').insertAfter('.left-bar');
$('.main-container').addClass('container-open');
function navtoggle() {
    if ($(window).width() <= 1200) {
        $('.left-bar').removeClass('nav-close').addClass('nav-open').animate({ left: "0" }, 100);
        $('.nav-toggle').removeClass('st-icon-menu').addClass('st-icon-cross').animate({ left: "82px", opacity: "1" }, 100);

    } else {
        $('.left-bar').removeClass('nav-close').addClass('nav-open').animate({ left: "0" }, 100);
        $('.nav-toggle').removeClass('st-icon-cross').addClass('st-icon-menu').animate({ left: "-35px", opacity: "0" }, 100);
    }
}

navtoggle();

$('.nav-toggle').click(function () {
    $('.left-bar').toggleClass('nav-open nav-close');
    $(this).toggleClass('st-icon-menu st-icon-cross');
    $('.main-container').toggleClass('container-open container-close');
});
function utilitiestoggle() {
    if ($(window).width() <= 1000) {
        $('#nav-utilities-collapse').removeClass('in');
    } else {
        $('#nav-utilities-collapse').addClass('in');
    }
}
mobileView();

function mobileView() {
    if ($(window).width() <= 740) {
        if (sessionStorage.getItem("load") != "true") {
            $('body').append('<div class="mobile-view"><div class="mobile-view-content animat"><div><i class="icon st-icon-tablet"></i> <i class="icon st-icon-desktop"></i></div> <p>Switch to tab or desktop for better view</p> <div><a href="javascript:void(0)" class="close-mbl">Continue with this view</a></div></div></div>');
        }
        setTimeout(
            function () {
                $('.mobile-view-content.animat').removeClass('animat');
            }, 2000
            );
    } else {
        $('.mobile-view').remove();

    }
}

$(document).on('click', '.close-mbl', function () {
    $(this).closest('.mobile-view').addClass('append').hide();
    sessionStorage.setItem("load", "true");
});

// Validation Scroll

function validationScroll() {
    validationErrors = $(".validationMessage:visible");
    if (validationErrors && validationErrors.length > 0) {
        var offseetheight = validationErrors.first().offset().top - 120
        $('html, body').animate({
            scrollTop: offseetheight
        }, 1000);
    }
}

// Minimize

function minimize() {
    var minimized_elements = $('p.minimize');

    minimized_elements.each(function () {
        var t = $(this).text();
        if (t.length < 80) return;

        $(this).html(
            t.slice(0, 80) + '<span>... </span>'
        );
    });
}

function timeLineMinimize() {
    var minimized_elements = $('p.tm-minimize');

    minimized_elements.each(function () {
        var t = $(this).text();
        if (t.length < 150) return;

        $(this).html(
            t.slice(0, 150) + '<span>... </span><a href="#" class="more">More</a>' +
            '<span style="display:none;">' + t.slice(150, t.length) + ' <a href="#" class="less">Less</a></span>'
        );

    });

    $('a.more', minimized_elements).click(function (event) {
        event.preventDefault();
        $(this).hide().prev().hide();
        $(this).next().show();
    });

    $('a.less', minimized_elements).click(function (event) {
        event.preventDefault();
        $(this).parent().hide().prev().show().prev().show();
    });
}

function AddressToString(address) {
    addressToString = '';
    if (ko.isObservable(address)) {
        addressToString = address.AddressLine1() + ", " + address.AddressLine2() + ", " + address.City() + ", " + address.State.Name() + ", " + address.ZipCode() + ", " + address.Country.Name();
    }
    else {
        addressToString = address.AddressLine1 + ", " + address.AddressLine2 + ", " + address.City + ", " + address.State.Name + ", " + address.ZipCode + ", " + address.Country.Name;
    }
    addressToString = addressToString.replace(/null/g, "").replace(/ ,/g, ""); // "/g is the modifier used to replace all occurences that are mentioned within / and /

    if (addressToString.charAt(0) == ",")
        addressToString = addressToString.slice(1);

    if (addressToString.charAt(addressToString.length - 2) == ",") {
        addressToString = addressToString.substring(0, addressToString.length - 2);
    }

    return addressToString;
}

function readCookie(name) {
    var nameEQ = escape(name) + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1, c.length);
        }
        if (c.indexOf(nameEQ) === 0) {
            return unescape(c.substring(nameEQ.length, c.length));
        }
    }
    return null;
}
if (ko != null) {
    ko.kendo.bindingFactory.createBinding({
        name: "kendoMultiSelectBox",
        events: {
            change: 'value',
            open: {
                writeTo: 'isOpen',
                value: true
            },
            close: {
                writeTo: 'isOpen',
                value: false
            }
        },
        watch: {
            enabled: 'enable',
            isOpen: ['open', 'close'],
            data: function (value) {
                ko.kendo.setDataSource(this, value);

                //if nothing is selected and there is an optionLabel, select it
                if (value && value.length && this.options.optionLabel && this.select() < 0) {
                    this.select(0);
                }
            },
            value: 'value'
        }
    });

    generateNewUUID = function () {
        var d = new Date().getTime();
        var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = (d + Math.random() * 16) % 16 | 0;
            d = Math.floor(d / 16);
            return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
        });
        return uuid;
    };

    function displayDate(date) {
        if (date == null) {
            return "";
        }
        var requestFormat = date.toString().indexOf('/');
        var dateFormat = readCookie("dateformat").toUpperCase();
        if (requestFormat != -1) {
            var millSeconds = date.replace('/Date(', '').replace(')/', '');
            var value = new Date(parseFloat(millSeconds)).ToUtcUtzDate();
        }
        else {
            var value = new Date(date).ToUtcUtzDate();;
        }
        return moment(value).format(dateFormat + " hh:mm A");
    }

    function dispalyPotential(potential) {
        var currencyFormat = '@(CurrencyFormat)';
        if (currencyFormat == "$X,XXX.XX") {
            kendo.culture("en-US");
            return "$" + kendo.toString(potential, 'n');
        } else if (currencyFormat == "X XXX,XX $") {
            kendo.culture("fr-FR");
            return kendo.toString(potential, 'n') + " $";
        } else if (currencyFormat == "B/.X,XXX.XX") {
            kendo.culture("en-US");
            return "B/." + kendo.toString(potential, 'n');
        } else if (currencyFormat == "₹X,XXX.XX") {
            kendo.culture("en-US");
            return "₹" + kendo.toString(potential, 'n');
        } else {
            kendo.culture("en-US");
            return "$" + kendo.toString(potential, 'n');
        }
    }
}


//utilitiestoggle();

// Scroll Down

//function scrollDown(scrollDownClass) {
//    alert(scrollDownClass);
//    var $t = $('.' + scrollDownClass);
//    $t.animate({ "scrollTop": $('.'+ scrollDownClass)[0].scrollHeight }, "slow");
//}
dropdownTypes = {
    PhoneNumberType: 1,
    AddressType: 2,
    LifeCycle: 3,
    PartnerType: 4,
    LeadSources: 5,
    OpportunityStage: 6,
    Community: 7,
    TourType: 8,
    RelationshipType: 9
}

contactFields = {
    FirstNameField: 1,
    LastNameField: 2,
    CompanyNameField: 3,
    MobilePhoneField: 4,
    HomePhoneField: 5,
    WorkPhoneField: 6,
    PrimaryEmail: 7,
    TitleField: 8,
    FacebookUrl: 9,
    TwitterUrl: 10,
    LinkedInUrl: 11,
    GooglePlusUrl: 12,
    WebsiteUrl: 13,
    BlogUrl: 14,
    AddressLine1Field: 15,
    AddressLine2Field: 16,
    CityField: 17,
    StateField: 18,
    ZipCodeField: 19,
    CountryField: 20,
    PartnerTypeField: 21,
    LifecycleStageField: 22,
    DonotEmail: 23,
    LeadSource: 24,
    Owner: 25,
    LeadScore: 26,
    CreatedBy: 27,
    CreatedOn: 28,
    LastTouched: 29,
    FirstName_NotAnalyzed: 30,
    LastName_NotAnalyzed: 31,
    CompanyName_NotAnalyzed: 32,
    LastUpdateOn: 33,
    StateCodeField: 34,
    CountryCode: 35,
    ContactId: 36,
    SecondaryEmail: 40,
    LastTouchedThrough: 41,
    Community: 42
}

