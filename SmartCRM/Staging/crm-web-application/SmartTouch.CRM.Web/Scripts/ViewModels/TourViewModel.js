var TourViewModel = function (data, url, isEditView,notselectall) {
    selfTour = this;

    var QUARTER = 15;

    var DAY_OF_TOUR = 1;
    var ONE_DAY_BEFORE = 2;
    var TWO_DAYS_BEFORE = 3;
    var ON_SELECTED_DATE = 4;

    //var NO_REMINDER = 0;
    //var EMAIL = 1;
    //var POP_UP = 2;
    //var TEXT_MESSAGE = 4;

    console.log("in tour js");
    console.log(notselectall);

    if (notselectall == false)
        data.SelectAll = true;

    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfTour));
    selfTour.PreviesComplete = ko.observable(data.IsCompleted);
    selfTour.DateFormat = ko.observable(data.DateFormat);
    selfTour.CreatedBy = ko.observable(data.CreatedBy);
    selfTour.UtcTourDate = ko.observable();
    selfTour.UtcReminderDate = ko.observable();
    selfTour.currentTime = ConvertToDate(data.currentTime).toUtzDate();
    selfTour.IcsCanlender = ko.observable(false);
    selfTour.AddToContactSummary = ko.observable(false);

    var userId = [];
    userId.push(data.CreatedBy);
    selfTour.OwnerIds = ko.observableArray(data.OwnerIds.length == 0 ? userId : data.OwnerIds);
    selfTour.UsersValidation = selfTour.OwnerIds.extend({ required: { message: '[|Select at least one User|]' } });
    selfTour.Users = ko.observableArray([]);

    selfTour.notselectall = ko.observable(notselectall);
    selfTour.SelectAll = ko.observable(data.SelectAll);

    selfTour.DisplayCreatedDate = kendo.toString(kendo.parseDate(ConvertToDate(data.CreatedOn)), selfTour.DateFormat());

    selfTour.DisplayCreatedDate = ko.pureComputed({
        read: function () {
            if (selfTour.CreatedOn() == null) {
                return new Date().toUtzDate();
            }
            else {
                var dateString = selfTour.CreatedOn().toString();
                var dateFormat = readCookie("dateformat").toUpperCase();
                if (dateString.indexOf('/Date') == 0) {
                    var utzdate = ConvertToDate(selfTour.CreatedOn()).toUtzDate();
                    selfTour.CreatedOn(utzdate);
                    return moment(utzdate).format(dateFormat);
                }
                else {
                    var date = Date.parse(selfTour.CreatedOn());
                    return ConvertToDate(date.toString());
                }
            }
        },
        write: function (newValue) {
            selfTour.CreatedOn(new Date(newValue));
        }
    });
    /* START - CUSTOM KNOCKOUT EXTENDERS */
    ko.validation.rules['communityCannotEqual'] = {
        validator: function (communityID, otherVal) {
            return (communityID != otherVal || communityID != "");
        },
        message: '[|Select community|]'
    };
    ko.validation.rules['tourtypeCannotEqual'] = {
        validator: function (tourType, otherVal) {
            return (tourType != otherVal || tourType != "");
        },
        message: '[|Select Tour type|]'
    };
    ko.validation.rules['minimumLength'] = {
        validator: function (contacts, minimumLength) {
            return (contacts.length > minimumLength);
        },
        message: '[|Select at least one Contact|]'
    };
    ko.validation.rules['validReminderDate'] = {
        validator: function (date, reminderTypes) {
            if (reminderTypes != null && reminderTypes.length > 0) {
                return (date != null && date.getFullYear() != 1970);
            }
            else {
                return true;
            }
        },
        message: '[|Select a valid reminder date|]'
    }

    ko.validation.registerExtenders();
    /* END - CUSTOM KNOCKOUT EXTENDERS */


    /* DECLARE DROPDOWNS */
    selfTour.ReminderTypes = ko.observableArray([
         { TypeId: 1, Name: "[|Email|]" },
         { TypeId: 2, Name: "[|Notification|]" },
         { TypeId: 3, Name: "[|Text|]" }
    ]);

    selfTour.ReminderTimeframes = ko.observableArray([
        { Id: DAY_OF_TOUR, name: "[|Day of Tour|]" },
        { Id: ONE_DAY_BEFORE, name: "[|1 day before|]" },
        { Id: TWO_DAYS_BEFORE, name: "[|2 days before|]" },
        { Id: ON_SELECTED_DATE, name: "[|On a date|]" }]);
    selfTour.TourTypes = ko.observableArray(data.TourTypes);
    selfTour.Communities = ko.observableArray(data.Communities);
    var Communitydata = ko.utils.arrayFirst(selfTour.Communities(), function (Communitychoice) {
        if (Communitychoice.IsDefault == true)
            return Communitychoice;
    });

    //var convertDate = function (theDate) {
    //    var returnDate;
    //    var dateString = theDate.toString();
    //    if (dateString.indexOf('/Date') == 0) {
    //        returnDate = ConvertToDate(theDate);
    //    }
    //    else {
    //        var date = Date.parse(theDate);
    //        returnDate = ConvertToDate(date.toString());
    //    }
    //    return returnDate;
    //}

    /* TOUR INITIALIZATION */
    selfTour.TourID = ko.observable(data.TourID);
    selfTour.Contacts = ko.observableArray(data.Contacts);//.extend({ minimumLength: 1 });
    //var tourDate = data.TourDate
    //if (data.TourDate != null)
    //    tourDate = tourDate.ToUtcUtzDate();
    selfTour.TourDate = ko.observable(data.TourDate).extend({ required: { message: "[|Select a valid Tour date|]" } });

    selfTour.CommunityID = ko.observable(data.CommunityID);

    if (data.CommunityID == "") {
        selfTour.CommunityID(Communitydata.DropdownValueID);
    }
    selfTour.TourDetails = ko.observable(data.TourDetails).extend({ maxLength: 1000 });
    var tourdata = ko.utils.arrayFirst(selfTour.TourTypes(), function (choice) {
        if (choice.IsDefault == true)
            return choice;
    });

    selfTour.AddContactSummary = function (val) {
        if (val.AddToContactSummary() === true)
            selfTour.AddToContactSummary(true);
        else
            selfTour.AddToContactSummary(false);
    }

    selfTour.IsCompleted = ko.observable(data.IsCompleted);

    selfTour.MarkAsComplete = function (value) {
        if (value.IsCompleted()) {

            //selfAction.IsReminderValid(false);
            //selfAction.isReminderTypeValid(false);
            //selfAction.SelectedReminderTypes([]);
            ////selfAction.ReminderMethod(1);
            //// selfAction.RemindOnDate([]);
        }
        else if (value.IsCompleted() === false) {


            //selfAction.IsReminderValid(true);
            //selfAction.isReminderTypeValid(true);
        }

        return true;
    };

    selfTour.allowIcsCalender = function (value) {
        if (value.IcsCanlender() === true) {
            selfTour.IcsCanlender(true);
        }
        else {
            selfTour.IcsCanlender(false);
        }

    }

    selfTour.ReminderDate = ko.observable(data.ReminderDate);
    selfTour.TourType = ko.observable(data.TourType).extend({ tourtypeCannotEqual: 0 });
    if (data.TourType == "")
        selfTour.TourType(tourdata.DropdownValueID);

    selfTour.ReminderTimeFrame = ko.observable(DAY_OF_TOUR);

    if (isEditView == 'True') {

        var pattern = /Date\(([^)]+)\)/;
        var tourdateresults = pattern.exec(selfTour.TourDate());
        var tourdate = new Date(parseFloat(tourdateresults[1]));

        if (selfTour.ReminderDate() != null && selfTour.ReminderDate() != undefined && selfTour.ReminderDate() != "null") {
            var reminderdateresults = pattern.exec(selfTour.ReminderDate());
            var remiderdate = new Date(parseFloat(reminderdateresults[1]));

            var differenceOfTourAndReminder = GetDifferenceOfTwoDatesInDays(tourdate, remiderdate);

            if (differenceOfTourAndReminder == 0) {
                selfTour.ReminderTimeFrame(DAY_OF_TOUR);
            }
            else
                if (differenceOfTourAndReminder == 1) {
                    selfTour.ReminderTimeFrame(ONE_DAY_BEFORE);
                }
                else
                    if (differenceOfTourAndReminder == 2) {
                        selfTour.ReminderTimeFrame(TWO_DAYS_BEFORE);
                    }
                    else {
                        selfTour.ReminderTimeFrame(ON_SELECTED_DATE);
                    }
        }
    }

    selfTour.RemindOnDate = ko.observable();
    selfTour.TourOnDate = ko.observable();



    selfTour.reminderChange = function () {
        if (this.value().length == 0) {
            selfTour.isReminderTypeValid(false);
        } else {
            selfTour.isReminderTypeValid(true);
        }
    };
    console.log("in js");
    console.log(data.SelectedReminderTypes);

    selfTour.SelectedReminderTypes = ko.observableArray(data.SelectedReminderTypes == null ? [] : data.SelectedReminderTypes);
    selfTour.isReminderTypeValid = ko.observable(selfTour.SelectedReminderTypes() > 0);
    selfTour.SelectedReminderTypes.subscribe(function () {
        if (selfTour.SelectedReminderTypes() != null && selfTour.SelectedReminderTypes().length > 0)
            selfTour.isReminderTypeValid(true);
        else
            selfTour.isReminderTypeValid(false);
    });

    /* AUTO POPULATE CONTACTS SELECTED IN THE GRID or AUTO POPULATE THE CONTACT IF TOUR IS BEING ADDED FROM CONTACT DETAILS PAGE */
    if (checkedContactValues.length > 0 || localStorage.getItem("contactdetails") != null) {
        if (selfTour.SelectAll() == false)
            selfTour.Contacts(selectedContacts(data.TourID, checkedContactValues, data.Contacts));
        else if(selfTour.SelectAll() == true)
            selfTour.Contacts(selectedContacts(0, checkedContactValues, data.Contacts));
    }

    /* DISPLAY CONTACT FULL NAMES IN 'PEOPLE' TEXTBOX */
    selfTour.ContactFullNames = ko.computed({
        read: function () {
            var contactFullNames = "";
            if (selfTour.Contacts() != null) {
                $.each(selfTour.Contacts(), function (index, value) {
                    if (contactFullNames != null && contactFullNames != "" && value.FullName != null)
                        contactFullNames = contactFullNames + "," + value.FullName;
                    else
                        contactFullNames = contactFullNames + value.FullName;
                });
            }
            return contactFullNames;
        },
        write: function () { },
        owner: this
    });

    selfTour.toursList = "";
    //var remainingCommunities = "";

    if (localStorage.getItem("contactdetails") != null) {
        var contactdtls = localStorage.getItem("contactdetails");
        var contactDetails = contactdtls.split('|');
        selfTour.defaultCommunity;

        if (selfTour.TourID() == 0) {
            $.ajax({
                url: url + 'GetContactCommunityExist',
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'contactId': contactDetails[0] })
            }).then(function (response) {
                removeinnerLoader('Tour');
                var filter = $.Deferred();
                if (response.success) {
                    filter.resolve(response);
                } else {
                    filter.reject(response.error);
                }
                return filter.promise();
            }).done(function (response) {
                selfTour.toursList = response.response.ContactCommunities;
                var remainingCommunities = ko.utils.arrayFilter(selfTour.Communities(), function (item) {
                    var index = response.response.ContactCommunities.map(function (e) { return e; }).indexOf(item.DropdownValueID);
                    return index == -1;
                });

                var bebackIndex = selfTour.TourTypes().map(function (e) { return e.DropdownValueTypeID }).indexOf(32);
                var firstIndex = selfTour.TourTypes().map(function (e) { return e.DropdownValueTypeID }).indexOf(31);
                var tourType;
                if (selfTour.toursList.map(function (e) { return e; }).indexOf(parseInt(selfTour.CommunityID())) >= 0 && selfTour.TourID() == 0) {
                    if (bebackIndex != -1) {
                        tourType = selfTour.TourTypes()[bebackIndex].DropdownValueID;

                        selfTour.TourType(tourType);
                    }
                }
                else {
                    if (firstIndex != -1) {
                        tourType = selfTour.TourTypes()[firstIndex].DropdownValueID;
                        selfTour.TourType(tourType);
                    }
                }
            }).fail(function (error) {
                notifyError(error);
            });
        }
    }

    if (checkedContactValues.length != null && checkedContactValues.length == 1) {

        var contactIdValue = checkedContactValues;

        var contactDetails1 = contactIdValue[0].split('|');
        selfTour.defaultCommunity;

        if (selfTour.TourID() == 0) {
            $.ajax({
                url: url + 'GetContactCommunityExist',
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'contactId': contactDetails1[0] })
            }).then(function (response) {
                // removeinnerLoader('Tour');
                var filter = $.Deferred();
                if (response.success) {
                    filter.resolve(response);
                } else {
                    filter.reject(response.error);
                }
                return filter.promise();
            }).done(function (response) {
                selfTour.toursList = response.response.ContactCommunities;
                var remainingCommunities = ko.utils.arrayFilter(selfTour.Communities(), function (item) {
                    var index = response.response.ContactCommunities.map(function (e) { return e; }).indexOf(item.DropdownValueID);
                    return index == -1;
                });
                var bebackIndex = selfTour.TourTypes().map(function (e) { return e.DropdownValueTypeID }).indexOf(32);
                var firstIndex = selfTour.TourTypes().map(function (e) { return e.DropdownValueTypeID }).indexOf(31);
                var tourType;
                if (selfTour.toursList.map(function (e) { return e; }).indexOf(parseInt(selfTour.CommunityID())) >= 0 && selfTour.TourID() == 0 && bebackIndex != -1) {
                    if (bebackIndex != -1) {
                        tourType = selfTour.TourTypes()[selfTour.TourTypes().map(function (e) { return e.DropdownValueTypeID }).indexOf(32)].DropdownValueID;
                        selfTour.TourType(tourType);
                    }
                }
                else {
                    if (firstIndex != -1) {
                        tourType = selfTour.TourTypes()[selfTour.TourTypes().map(function (e) { return e.DropdownValueTypeID }).indexOf(31)].DropdownValueID;
                        selfTour.TourType(tourType);
                    }
                }
            }).fail(function (error) {
                notifyError(error);
            });
        }
    }

    selfTour.CommunityID.subscribe(function () {
        //if (selfTour.toursList.map(function (e) { return e; }).indexOf(parseInt(selfTour.CommunityID())) >= 0)
        //{
        //    selfTour.TourType(59)
        //}
        //else
        //{
        //    selfTour.TourType(58)
        //}

        if (selfTour.TourID() == 0) {
            if (selfTour.toursList != "") {
                var bebackIndex = selfTour.TourTypes().map(function (e) { return e.DropdownValueTypeID }).indexOf(32);
                var firstIndex = selfTour.TourTypes().map(function (e) { return e.DropdownValueTypeID }).indexOf(31);
                var tourType;
                if (selfTour.toursList.map(function (e) { return e; }).indexOf(parseInt(selfTour.CommunityID())) >= 0 && selfTour.TourID() == 0 && bebackIndex != -1) {
                    if (bebackIndex != -1) {
                        tourType = selfTour.TourTypes()[selfTour.TourTypes().map(function (e) { return e.DropdownValueTypeID }).indexOf(32)].DropdownValueID;
                        //alert("sample" + tourType);
                        selfTour.TourType(tourType);
                    }
                }
                else {
                    if (firstIndex != -1) {
                        tourType = selfTour.TourTypes()[selfTour.TourTypes().map(function (e) { return e.DropdownValueTypeID }).indexOf(31)].DropdownValueID;
                        //alert("sample2" + tourType);    
                        //if (tourType != null) {
                        selfTour.TourType(tourType);
                    }
                    //}
                }
            }
            //selfTour.TourType(data.TourType);
        }

    });
    if (selfTour.SelectAll() == false) {
        selfTour.contactsValidation = selfTour.ContactFullNames.extend({ minimumLength: 1 });
    }
    selfTour.communityValidation = selfTour.CommunityID.extend({ communityCannotEqual: 0 });
    selfTour.tourTypeValidation = selfTour.TourType.extend({ tourtypeCannotEqual: 0 });


    /* THIS VALUE IS ASSIGNED TO REMINDER DATE */
    //selfTour.RemindOnDate = ko.observable();
    selfTour.RemindOnDate = ko.pureComputed({
        read: function () {
            var returnDate;
            if (!selfTour.ReminderDate()) {
                returnDate = new Date();
            }
            else {
                var dateString = selfTour.ReminderDate().toString();
                if (dateString.indexOf('/Date') == 0) {
                    returnDate = isEditView == true ? ConvertToDate(selfTour.ReminderDate()).ToUtcUtzDate() : ConvertToDate(selfTour.ReminderDate()).toUtzDate();
                }
                else {
                    var date = Date.parse(selfTour.ReminderDate());
                    returnDate = ConvertToDate(date.toString());
                }
            }
            selfTour.ReminderDate(returnDate);
            return returnDate;
        },
        write: function (newValue) {

            selfTour.ReminderDate(new Date(newValue));
        }
    });//.extend({ validReminderDate: selfTour.SelectedReminderTypes() });


    selfTour.TourOnDate = ko.pureComputed({
        read: function () {

            var returnDate;
            if (!selfTour.TourDate()) {
                returnDate = new Date();
            }

            else {

                var dateString = selfTour.TourDate().toString();
                if (dateString.indexOf('/Date') == 0) {
                    //returnDate = isEditView == "True" ? ConvertToDate(selfTour.TourDate()).toUtzDate() : ConvertToDate(selfTour.TourDate()).toUtzDate();
                    returnDate = ConvertToDate(selfTour.TourDate()).toUtzDate();
                }
                else {

                    var date = Date.parse(selfTour.TourDate());
                    returnDate = ConvertToDate(date.toString());
                }
            }
            selfTour.TourDate(returnDate);
            return returnDate;
        },
        write: function (newValue) {

            selfTour.TourDate(new Date(newValue));
        }
    });


    //selfTour.reminderDateValidation = selfTour.RemindOnDate.extend({ validReminderDate: selfTour.SelectedReminderTypes() });

    selfTour.isReminderApplicable = ko.pureComputed({
        read: function () {
            var tourDate;
            var dateString = selfTour.TourDate().toString();
            if (dateString.indexOf('/Date') == 0) {

                tourDate = ConvertToDate(selfTour.TourDate());
            }
            else {
                var date = Date.parse(selfTour.TourDate());
                tourDate = ConvertToDate(date.toString());
            }

            if (tourDate > new Date().toUtzDate())
                return true;
            else
                return false;
        }
    });

    //selfTour.isReminderTypeValid = ko.pureComputed({
    //    read: function () {
    //        if (selfTour.ReminderType() != 0 && selfTour.TourDate() > new Date())
    //            return true;
    //        else
    //            return false;
    //    }
    //});


    /* SUBSCRIPTIONS - REGION START
     * =============================== */

    //Common function to get the Reminder date - DRY principle applied.
    function GetTourReminderDate(tourDate, selectedReminderTypes, reminderDate, reminderTimeFrame) {

        if (selectedReminderTypes != null && selectedReminderTypes.length > 0) {
            if (reminderDate == null) {
                reminderDate = new Date();
            }
            var mins = tourDate().getMinutes();
            if (mins % QUARTER != 0)
            { mins = mins % QUARTER; }
            else
            { mins = mins % QUARTER + QUARTER }
            var date = new Date(tourDate().getFullYear(), tourDate().getMonth(), tourDate().getDate(), tourDate().getHours(), tourDate().getMinutes() - mins);
            if (reminderTimeFrame == DAY_OF_TOUR) {
                date.setDate(date.getDate() - 0);
                reminderDate = date;
            }
            else if (reminderTimeFrame == ONE_DAY_BEFORE) {
                date.setDate(date.getDate() - 1);
                date.setMinutes(tourDate().getMinutes());
                reminderDate = date;
            }
            else if (reminderTimeFrame == TWO_DAYS_BEFORE) {
                date.setDate(date.getDate() - 2);
                date.setMinutes(tourDate().getMinutes());
                reminderDate = date;
            }
            else if (reminderTimeFrame == ON_SELECTED_DATE) {
                date.setDate(date.getDate() - 3);
                reminderDate = date;
            }
            return reminderDate;
        }
        else {
            reminderDate = null;
            return reminderDate;
        }
    }

    /* REMINDER DATE CHANGES ACCORDING TO THE CHANGE IN TOUR DATE */
    //selfTour.TourDate.subscribe(function () {

    //    var tempDate = GetTourReminderDate(selfTour.TourDate, selfTour.SelectedReminderTypes(), selfTour.ReminderDate, selfTour.ReminderTimeFrame());
    //    selfTour.ReminderDate(tempDate);
    //});

    /* REMINDER DATE CHANGES ACCORDING TO THE CHANGE IN REMINDER TYPE */
    //selfTour.ReminderType.subscribe(function () {
    //    var tempDate = GetTourReminderDate(selfTour.TourDate, selfTour.ReminderType, selfTour.ReminderDate, selfTour.ReminderTimeFrame());
    //    selfTour.ReminderDate(tempDate);
    //});

    /* REMINDER DATE CHANGES ACCORDING TO THE CHANGE IN REMINDER TIMEFRAME */
    //selfTour.ReminderTimeFrame.subscribe(function () {

    //    var tempDate = GetTourReminderDate(selfTour.TourDate, selfTour.SelectedReminderTypes(), selfTour.ReminderDate, selfTour.ReminderTimeFrame());
    //    selfTour.ReminderDate(tempDate);
    //});

    selfTour.ReminderTimeFrameForTour = ko.observable("[|Day of Tour|]");
    selfTour.RemindOnDate.subscribe(function () {

        var differenceOfTourAndReminder = GetDifferenceOfTwoDatesInDays(selfTour.TourDate(), selfTour.ReminderDate());
        if (differenceOfTourAndReminder == 0) {
            selfTour.ReminderTimeFrame(DAY_OF_TOUR);
            selfTour.ReminderTimeFrameForTour("[|Day of Tour|]");
        }
        else
            if (differenceOfTourAndReminder == 1) {
                selfTour.ReminderTimeFrame(ONE_DAY_BEFORE);
                selfTour.ReminderTimeFrameForTour("[|1 day before|]");
            }
            else
                if (differenceOfTourAndReminder == 2) {
                    selfTour.ReminderTimeFrame(TWO_DAYS_BEFORE);
                    selfTour.ReminderTimeFrameForTour("[|2 days before|]");
                }
                else {
                    selfTour.ReminderTimeFrame(ON_SELECTED_DATE);
                    selfTour.ReminderTimeFrameForTour("[|On a date|]");
                }
    });

    

    /* SUBSCRIPTIONS - REGION END*/

    selfTour.errors = ko.validation.group(selfTour);


    /* TOUR SAVE FUNCTIONALITY */
    var saveTour = function (saveType) {
        if (selfTour.TourID() == 0 && checkedContactValues.length == 0 && selfTour.SelectAll() == true)
        {
            notifyError("Select at least one Contact");
        }
        else
        {
            var modifiedDate = selfTour.TourDate();
            selfTour.UtcTourDate(modifiedDate);

            modifiedDate = selfTour.ReminderDate();
            if (modifiedDate != null)
                selfTour.UtcReminderDate(modifiedDate.toLtUtcDate());


            var jsondata = ko.toJSON(selfTour);
            var tour = saveType;

            if (selfTour.notselectall() == false) {
                selfTour.SelectAll = ko.observable(true);
            }

            selfTour.errors.showAllMessages();
            if (selfTour.errors().length > 0)
                return;

            innerLoader('Tour');
            $.ajax({
                url: url + tour,
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'tourViewModel': jsondata })
            }).then(function (response) {
                // removeinnerLoader('Tour');
                var filter = $.Deferred();
                if (response.success) {
                    filter.resolve(response);
                } else {
                    filter.reject(response.error);
                }
                return filter.promise();
            }).done(function () {
                notifySuccess('[|Successfully saved the Tour|]');

                setTimeout(
                    function () {
                        createCookie('log', false, 1);
                        window.location.href = document.URL;  //Reload the page to reset "Add Tour" view. Revisit.
                    }, setTimeOutTimer);

            }).fail(function (error) {
                notifyError(error);
                removeinnerLoader('Tour');
            });
        }
      
    };
    selfTour.insertTour = function () {
        saveTour("InsertTour");
    };
    selfTour.updatingtour = function () {
        saveTour("UpdateTour");
    };
    selfTour.updateTour = function () {
        var id = ko.utils.arrayFirst(selfTour.Contacts(), function (item) {
            return item.Id == selfDetails.ContactID();
        });
        if (id != null && selfTour.Contacts().length > 1 && selfTour.TourID() > 0 && selfTour.PreviesComplete() != selfTour.IsCompleted()) {
            $('#modal').modal('hide');
            selfDetails.TourCompletedOperation_TourEdit(selfTour);

        }
        else {
            saveTour("UpdateTour");
        }

    };
}

checkedContactValues = fnGetCheckedValues();

