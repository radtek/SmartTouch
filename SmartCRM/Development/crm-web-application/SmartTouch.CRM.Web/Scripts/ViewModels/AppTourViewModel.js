var AppTourViewModel = function (isFirstLogin, isTourCompleted, userId, base_url) {

    var tourDetails = [];
    tour = new Tour({});
    var url = base_url;
    getAppTourByDivision = function (divisionId) {
        if (divisionId != 0) {
            $.ajax({
                url: url + '/GetAppTourByDivision',
                type: 'get',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: { 'divisionId': divisionId },
                success: function (data) {
                    tourDetails = data.response;
                    initiateTour(tourDetails);
                },
                error: function (error) {
                    console.log(error);
                }
            });
        }
    }

    function initiateTour(tourDetails) {
        if (tourDetails != null && tourDetails.length > 0) {
            tour = new Tour({
                backdrop: false,
                debug: false,
                onNext: tourOnNext,
                onStart: tourStart,
                onEnd: tourEnd,
                onHide: tourHide,
                onPause: tourPause,
                orphan: false,
                duration: false,
                steps: generateAppTourSteps(tourDetails)
            });
            tour.init();
            localStorage.removeItem("tour_current_step");
            localStorage.setItem("tour_current_step", 0);
            tour.restart();
        }
    };

    function generateAppTourSteps(details) {
        var handlePermissions = [];
        
        $.each(details, function (i, e) {
            if ($(e.HTMLID).length > 0) {
                handlePermissions.push(e)
            }
        })
        var steps = [];
        //var leftMenuIdsElement = ["#leftMenu", "#advancedsearch"];
        $.each(handlePermissions, function (ind, detail) {
            var template = "";
            if (ind == (handlePermissions.length - 1))
                template = "<div class='popover tour stt-tour'><div class='arrow'><div class='stc-tour-pin'><div class='stc-pin-fade'></div><div class='stc-pin'></div></div></div><h3 class='popover-title'></h3><div class='popover-content'></div><div class='popover-navigation'><div class='stt-close' data-role='end'>×</div><button class='btn btn-default' data-role='prev'>PREV</button><button class='btn btn-primary' data-role='end'>END TOUR</button></div>";
            else if (isFirstLogin == "1")
                template = "<div class='popover tour stt-tour'><div class='arrow'><div class='stc-tour-pin'><div class='stc-pin-fade'></div><div class='stc-pin'></div></div></div><h3 class='popover-title'></h3><div class='popover-content'></div><div class='popover-navigation'><div class='stt-close' data-role='end'>×</div><button class='btn btn-default' data-role='prev'>PREV</button><button class='btn btn-primary' data-role='next'>NEXT</button></div>";
            else
                template = "<div class='popover tour stt-tour'><div class='arrow'><div class='stc-tour-pin'><div class='stc-pin-fade'></div><div class='stc-pin'></div></div></div><h3 class='popover-title'></h3><div class='popover-content'></div><div class='popover-navigation'><button class='btn btn-default' data-role='prev'>PREV</button><button class='btn btn-primary' data-role='next'>NEXT</button></div>";
            var item = new Object();
            item.template = template;
            item.element = detail.SectionID == 1 ? "#tourwelcome" : detail.HTMLID.trim();  //content-area
            item.placement = detail.PopUpPlacement.trim();
            item.title = detail.Title;
            item.content = detail.Content;
            item.orphan = false;
            item.onPrev = function (tour) {
               // var step = tour.getStep(tour.getCurrentStep() -1);
               // if (step != undefined && leftMenuIdsElement.indexOf(step.element) > -1)
               //     $("body").css("overflow", "hidden");
               //else
               //     $("body").css("overflow", "");
            };
            item.onNext = function (tour) {
                //var step = tour.getStep(tour.getCurrentStep() +1);
                //if (step != undefined && leftMenuIdsElement.indexOf(step.element) > -1)
                //    $("body").css("overflow", "hidden");
                //    else
                //    $("body").css("overflow", "");
            };
            steps.push(item);
        });
        return steps;
    };

    function tourOnNext(data) {

    }

    function tourStart(data) {
        $("body").css("overflow", "hidden");     //This will remove scroll for body.
    }
    function tourEnd(data) {
        $("body").css("overflow", "");
        createCookie("IsFirstLogin", 1, 1);

        if (userId != 0) {
            $.ajax({
                url: url + '/UpdateTourVisit',
                type: 'get',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: { 'userId': userId },
                success: function (data) {

                },
                error: function (error) {
                    console.log(error);
                }
            });
        }
    }
    function tourHide(data) {

    }
    function tourPause(data) {

    }
    function tourStepShow(tour) {

    }

    function createCookie(name, value, days) {
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            var expires = "; expires=" + date.toGMTString();
        } else var expires = "";
        document.cookie = escape(name) + "=" + escape(value) + expires + "; path=/";
    }
}