var TourViewModel = function (data, url) {
    var selfTour = this;

    var DAY_OF_TOUR = 1;
    var ONE_DAY_BEFORE = 2;
    var TWO_DAYS_BEFORE = 3;
    var ON_SELECTED_DATE = 4;

    var USER_LOGGED_IN = 1;
    var QUARTER = 15;

    ko.mapping.fromJS(data, {}, selfTour);

    selfTour.Contacts = ko.observableArray(data.Contacts).extend({ require: { message: "Select alteast one contact" } });
    selfTour.TourDate = ko.observable(data.TourDate);
    selfTour.ReminderDate = ko.observable(data.ReminderDate);
    selfTour.ReminderTimeFrame = ko.observable();
    selfTour.CreatedBy = ko.observable(USER_LOGGED_IN);
    var date = new Date();
    alert(0);
    selfTour.TourDate.subscribe(function (newDate) {

        var mins = selfTour.TourDate().getMinutes();
        mins = QUARTER - mins % QUARTER;
        date = new Date(selfTour.TourDate().getFullYear(), selfTour.TourDate().getMonth(), selfTour.TourDate().getDate());
        console.log(date);
        if (selfTour.ReminderTimeFrame() == DAY_OF_TOUR) {
            date.setDate(date.getDate() - 0);
            selfTour.ReminderDate(date);
        }
        if (selfTour.ReminderTimeFrame() == ONE_DAY_BEFORE) {
            date.setDate(date.getDate() - 1);
            selfTour.ReminderDate(date);
        }
        if (selfTour.ReminderTimeFrame() == TWO_DAYS_BEFORE) {
            date.setDate(date.getDate() - 2);
            selfTour.ReminderDate(date);
        }
        if (selfTour.ReminderTimeFrame() == ON_SELECTED_DATE) {
            date.setDate(date.getDate());
            selfTour.ReminderDate(date);
        }
    });

    selfTour.ReminderTimeFrame.subscribe(function (newValue) {
        alert("Hour");
        var mins = selfTour.TourDate().getMinutes();
        mins = QUARTER - mins % QUARTER;
        
        date = new Date(selfTour.TourDate().getFullYear(), selfTour.TourDate().getMonth(), selfTour.TourDate().getDate(),0, selfTour.TourDate().getMinutes() + mins);
        if (newValue == DAY_OF_TOUR) {
            date.setDate(date.getDate() - 0);
            selfTour.ReminderDate(date);
        }
        if (newValue == ONE_DAY_BEFORE) {
            date.setDate(date.getDate() - 1);
            selfTour.ReminderDate(date);
        }
        if (newValue == TWO_DAYS_BEFORE) {
            date.setDate(date.getDate() - 2);
            selfTour.ReminderDate(date);
        }
        if (newValue == ON_SELECTED_DATE) {
            date.setDate(date.getDate());
            selfTour.ReminderDate(date);
        }
    });

    selfTour.ReminderTimeframes = [{ Id: DAY_OF_TOUR, name: "Day of Tour" },
             { Id: ONE_DAY_BEFORE, name: "1 day before" },
             { Id: TWO_DAYS_BEFORE, name: "2 days before" },
             { Id: ON_SELECTED_DATE, name: "Custom" }];

    selfTour.TemporaryContacts = ko.observableArray([
            { id: "2423", name: "Usain Bolt" },
            { id: "2424", name: "My Cousin Vinni" },
            { id: "8", name: "Ryan McVinney" },
            { id: "17", name: "Green Star Builders" },
            { id: "19", name: "Paul Monaco" },
            { id: "21", name: "shirleym Alexander" },
            { id: "24", name: "Michael Suyama" },
            { id: "25", name: "Anne King" },
            { id: "28", name: "Laura Peacock" },
            { id: "1082", name: "Siva K Kumar" },
            { id: "1094", name: "Avinash Mallidi" },
            { id: "29", name: "Robert Fuller" },
            { id: "29", name: "Robert Fuller" }]);

    selfTour.errors = ko.validation.group(selfTour);

    selfTour.saveTour = function () {
    
        selfTour.TagLists = tagsViewModel;
        var jsondata = ko.toJSON(selfTour);
        if (selfTour.TourID() == 0) {
            alert(1);
            var tour = "InsertTour";
        }
        else
            if (selfTour.TourID() > 0) {
                alert(2)
                var tour = "UpdateTour";
            }

        selfTour.errors.showAllMessages();

        if (selfTour.errors().length > 0 && selfTour.Contacts().length < 0)
            return;
        alert(3)
        $.ajax({url: url + tour,type: 'put',dataType: 'json',contentType: "application/json; charset=utf-8",data: JSON.stringify({ 'tourViewModel': jsondata }),
            success: function (data) {
                //$('.success-msg').remove();
                alert("Entered Ajax call");
                if (data.success === true) {
                    $('.overlay').remove();
                    $('body .main-container').append('<div class="alert alert-success success-msg"><a class="close icon st-icon-delete" data-dismiss="alert" href="#" aria-hidden="true"></a> <i class="icon st-icon-checkmark"></i> Successfully saved the tour</div>');
                    alert("Saved the tour");
                    setTimeout(function () {
                        $('.success-msg').remove();
                    }, setTimeOutTimer);
                    window.location.href = "/Contact/Index";
                }
                if (data.success === false) {
                    $('.overlay').remove();
                    $('body .main-container').append('<div class="alert alert-error success-msg"><a class="close icon st-icon-delete" data-dismiss="alert" href="#" aria-hidden="true"></a> <i class="icon st-icon-spam"></i> ' + data.response + '</div>');

                    setTimeout(function () {
                        $('.success-msg').remove();
                    }, 10000);
                }
            },
            error: function (data) {
            }
        });
    };
}