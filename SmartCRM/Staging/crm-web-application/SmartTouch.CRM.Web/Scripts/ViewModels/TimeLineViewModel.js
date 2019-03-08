var TimeLineViewModel = function (url, opportunityurl, pageName, dateFormat, Activities, contactid, opportunityid) {

    var TimeLine = this;
    // this is the code from the github link : https://github.com/sobelk/knockout-groupby/blob/master/knockout.groupby.js
    ko.bindingHandlers.groupby = {
        makeTemplateValueAccessor: function (valueAccessor) {
            var bindingValue, groupedArrays, groups, key, keys, obj, objectsInGroup, _i, _j, _len, _len1, _ref,
              _this = this;
            bindingValue = ko.utils.unwrapObservable(valueAccessor());
            groups = {};
            _ref = bindingValue.group;
            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                obj = _ref[_i];
                key = bindingValue.by(obj);
                if (groups[key] === void 0) {
                    groups[key] = [obj];
                } else {
                    groups[key].push(obj);
                }
            }
            keys = (function () {
                var _results;
                _results = [];
                for (key in groups) {
                    _results.push(key);
                }
                return _results;
            })();
            if (typeof bindingValue.sort === 'function') {
                keys.sort(bindingValue.sort);
            } else if (typeof bindingValue.sort === 'string') {
                if (bindingValue.sort === 'ascending') {
                    keys.sort();
                } else if (bindingValue.sort === 'descending') {
                    keys.sort();
                    keys.reverse();
                }
            }
            groupedArrays = [];
            for (_j = 0, _len1 = keys.length; _j < _len1; _j++) {
                key = keys[_j];
                objectsInGroup = groups[key];
                objectsInGroup.$key = key;
                groupedArrays.push(objectsInGroup);
            }
            return function () {
                return {
                    foreach: groupedArrays,
                    templateEngine: ko.nativeTemplateEngine.instance
                };
            };
        },
        init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            return ko.bindingHandlers.template.init(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
        },
        update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            var newValueAccessor;
            newValueAccessor = ko.bindingHandlers.groupby.makeTemplateValueAccessor(valueAccessor);
            return ko.bindingHandlers.template.update(element, newValueAccessor, allBindingsAccessor, viewModel, bindingContext);
        }
    };


    //this is the code for multiselect binding with datatextfield and value field
    ko.bindingHandlers.multiselect = {
        init: function (element, valueAccessor) {
            var value = valueAccessor();
            // Create the mutliselect
            ko.applyBindingsToNode(element, {
                kendoMultiSelect: {
                    value: value,
                    dataTextField: 'ActivityValue',
                    data: TimeLine.Activities(),
                    placeholder: '[|Select Activities|]',
                    change: function () {
                        TimeLine.SelectedActivities(this.value());
                        TimeLine.TimeLineData([]);
                        TimeLine.PageNumber(1);
                        GetDataFromNextSection();
                    }
                }
            });
            var src = $(element).data('kendoMultiSelect').dataSource.data();
            var selected = $.grep(src, function (e, i) {
                return ko.utils.arrayFilter(value(), function (item) {
                    return item.ActivityID == e.ActivityID;
                }).length > 0;
            });
            value(selected);

        }
    }

    TimeLine.Activities = ko.observableArray(Activities);
    TimeLine.DateFormat = ko.observable(dateFormat);

    if (pageName == "contacts") {

        //if (contactactionsPermission == "True")
        //    TimeLine.Activities.push({ ActivityID: 'Action', ActivityValue: "Actions" });
        //if (contactnotePermission == "True")
        //    TimeLine.Activities.push({ ActivityID: 'Note', ActivityValue: "Notes" });
        //if (contacttourPermission == "True")
        //    TimeLine.Activities.push({ ActivityID: 'Tour', ActivityValue: "Tours" });
        //if (campaingnPermission == "True")
        //    TimeLine.Activities.push({ ActivityID: 'Campaign', ActivityValue: "Campaigns" });
        //if (formPermission == "True")
        //    TimeLine.Activities.push({ ActivityID: 'Form', ActivityValue: "Forms" });

        //TimeLine.Activities.push({ ActivityID: 'Contact', ActivityValue: "Profile Updates" });
        //if (contactrelationshipPermission == "True")
        //    TimeLine.Activities.push({ ActivityID: 'Relationship', ActivityValue: "Relationships" });

        //TimeLine.Activities.push({ ActivityID: 'Attachment', ActivityValue: "Attachments" });

        //if (sendemailPermission == "True")
        //    TimeLine.Activities.push({ ActivityID: 'Email', ActivityValue: "Emails" });
        //if (sendtextPermission == "True")
        //    TimeLine.Activities.push({ ActivityID: 'Text', ActivityValue: "Text" });


        // TimeLine.Activities([
        ////{ ActivityID: '', ActivityValue: "All Activities" },
        ////{ ActivityID: 2, ActivityValue: "Campaigns" },
        ////{ ActivityID: 3, ActivityValue: "Emails", },
        ////{ ActivityID: 4, ActivityValue: "Form Submissions" },

        //{ ActivityID: 'Action', ActivityValue: "Actions" },
        //{ ActivityID: 'Note', ActivityValue: "Notes" },
        //{ ActivityID: 'Tour', ActivityValue: "Tours" },
        //{ ActivityID: 'Campaign', ActivityValue: "Campaigns" },
        //{ ActivityID: 'Form', ActivityValue: "Forms" },
        //{ ActivityID: 'Contact', ActivityValue: "Profile Updates" },
        //{ ActivityID: 'Relationship', ActivityValue: "Relationships" },
        //{ ActivityID: 'Attachment', ActivityValue: "Attachments" },
        //{ ActivityID: 'Email', ActivityValue: "Emails" },
        //{ ActivityID: 'Text', ActivityValue: "Text" },
        ////{ ActivityID: 8, ActivityValue: "Activities" },
        ////{ ActivityID: 9, ActivityValue: "Web visits" }
        // ]);
    } else {
        //    TimeLine.Activities([
        ////{ ActivityID: '', ActivityValue: "All Activities" },
        ////{ ActivityID: 2, ActivityValue: "Campaigns" },
        ////{ ActivityID: 3, ActivityValue: "Emails", },
        ////{ ActivityID: 4, ActivityValue: "Form Submissions" },
        //{ ActivityID: 'Action', ActivityValue: "Actions" },
        //{ ActivityID: 'Note', ActivityValue: "Notes" },
        //{ ActivityID: 'Opportunity', ActivityValue: "Profile Updates" },
        ////{ ActivityID: 'Tour', ActivityValue: "Tours" },
        ////{ ActivityID: 8, ActivityValue: "Activities" },
        ////{ ActivityID: 9, ActivityValue: "Web visits" }
        //    ]);
    }

    TimeLine.SelectedActivities = ko.observableArray(TimeLine.Activities());
    TimeLine.PreviousActivities = ko.observableArray();
    TimeLine.ActivityIDS = ko.computed({
        read: function () {
            var ActivityIDs = [];
            for (var g = 0; g < TimeLine.SelectedActivities().length; g++) {
                ActivityIDs.push(TimeLine.SelectedActivities()[g].ActivityID);
            }
            return ActivityIDs;
        },
        write: function (n) {
            var differences = ko.utils.compareArrays(TimeLine.PreviousActivities, n);
            var deleted = [];
            var added = [];
            ko.utils.arrayForEach(differences, function (difference) {
                if (difference.status === "deleted") {
                    deleted.push(difference.value);
                }
                else if (difference.status === "added") {
                    added.push(difference.value);
                }
            });
            TimeLine.ActivityIDS = [];
            TimeLine.ActivityIDS = n;
            TimeLine.PreviousActivities = n;
            TimeLine.PageNumber(1);
            if (deleted.length > 0 || added.length > 0 || n.length == 0) {
                GetDataFromNextSection();
            }
        }
    });

    TimeLine.Limit = 20;
    TimeLine.PageNumber = ko.observable(1);
    TimeLine.PeriodRanges = ko.observableArray([
       { PeriodRangeID: 1, PeriodRangeValue: "[|All|]" },
       { PeriodRangeID: 2, PeriodRangeValue: "[|Last 7 Days|]" },
       { PeriodRangeID: 3, PeriodRangeValue: "[|Last 30 Days|]" },
       { PeriodRangeID: 4, PeriodRangeValue: "[|Last 60 Days|]" },
       { PeriodRangeID: 5, PeriodRangeValue: "[|Last 90 Days|]" },
       { PeriodRangeID: 6, PeriodRangeValue: "[|Custom|]" }
    ]);

    TimeLine.Activity = ko.observable("");
    TimeLine.Period = ko.observable("");
    TimeLine.CustomDateDisplay = ko.pureComputed(function () {
        if (TimeLine.Period() == "6") {
            return true;
        } else {
            return false;
        }
    });



    TimeLine.TimeLineData = ko.observableArray();
    TimeLine.TotalHits = ko.observable(0);
    TimeLine.GroupedData = ko.observableArray();
    TimeLine.DisplayEndMessage = ko.pureComputed(function () {
        if (TimeLine.TimeLineData().length == TimeLine.TotalHits()) {

            return true;
        } else {
            return false;
        }
    });
    var toStartDate = new Date();
    toStartDate.setDate(toStartDate.getDate());
    TimeLine.CustomEndDate = ko.observable(toStartDate);
    var fromdate = new Date();
    fromdate.setMonth(fromdate.getMonth() - 1);
    TimeLine.CustomStartDate = ko.observable(fromdate);

    TimeLine.toMaxDate = ko.observable(new Date());
    var fromMaxDate = new Date();

    fromMaxDate.setDate(fromMaxDate.getDate() - 1);
    TimeLine.fromMaxDate = ko.observable(fromMaxDate);

    TimeLine.moduleChange = function () {
        TimeLine.TimeLineData([]);
        TimeLine.PageNumber(1);
        GetDataFromNextSection();
    }

    TimeLine.fromDateChangeEvent = function () {
        var fromDate = this.value();
        TimeLine.CustomStartDate(moment(fromDate).format());
        var toDate = TimeLine.CustomEndDate();
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
        TimeLine.TimeLineData([]);
        TimeLine.PageNumber(1);
        GetDataFromNextSection();
    }

    TimeLine.toDateChangeEvent = function () {
        var fromDate = TimeLine.CustomStartDate();
        var toDate = this.value();
        TimeLine.CustomEndDate(moment(toDate).format());
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
        TimeLine.TimeLineData([]);
        TimeLine.PageNumber(1);
        GetDataFromNextSection();
    }


    TimeLine.periodChange = function () {
        var value = this.value();
        eraseCookie("period");
        createCookie("period", value, 1);
        if (value == "6") {
            var toStartDate = new Date();
            toStartDate.setDate(toStartDate.getDate());
            TimeLine.CustomEndDate(moment(toStartDate).format());
            var fromdate = new Date();
            fromdate.setMonth(fromdate.getMonth() - 1);
            TimeLine.CustomStartDate(moment(fromdate).format());
        }
        TimeLine.TimeLineData([]);
        TimeLine.PageNumber(1);
        GetDataFromNextSection();
    }

    var GetDataFromNextSection = function () {

        var timelineViewModel = {
            PageNumber: TimeLine.PageNumber(),
            PageName: pageName,
            Activities: TimeLine.ActivityIDS,
            CustomStartDate: TimeLine.CustomStartDate(),
            CustomEndDate: TimeLine.CustomEndDate(),
            ContactID: contactid,
            OpportunityID: opportunityid,
            Limit: TimeLine.Limit
        };

        $.ajax({
            url: url + "TimeLineData",
            type: 'post',
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: ko.toJSON(timelineViewModel)
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            if (TimeLine.PageNumber() == 1) {
                TimeLine.GroupedData(data.response.timeLineGroup);
                TimeLine.TotalHits(data.response.TotalRecords);
                TimeLine.TimeLineData(data.response.lsttimelineViewModel);
            }
            else {
                ko.utils.arrayPushAll(TimeLine.TimeLineData, data.response.lsttimelineViewModel);
            }
            timeLine();
            timeLineMinimize();
            removeinnerLoader("timeline");
        }).fail(function (error) {
            notifyError(error);
        })
    }
    GetDataFromNextSection();

    $(document).scroll(function () {
        if ($(window).scrollTop() == ($(document).height() - $(window).height())) {
            if (TimeLine.TimeLineData().length == TimeLine.TotalHits()) {
                return;
            } else {
                innerLoader("timeline");
                TimeLine.PageNumber(TimeLine.PageNumber() + 1);
                GetDataFromNextSection();
            }
        }
    });

    function timeLine() {
        var $timeline_block = $('.cd-timeline-block');

        //hide timeline blocks which are outside the viewport
        $timeline_block.each(function () {
            if ($(this).offset().top > $(window).scrollTop() + $(window).height() * 1) {
                $(this).find('.cd-timeline-img, .cd-timeline-content').addClass('is-hidden');
            }
        });

        //on scolling, show/animate timeline blocks when enter the viewport
        $(window).on('scroll', function () {
            $timeline_block.each(function () {
                if ($(this).offset().top <= $(window).scrollTop() + $(window).height() * 1 && $(this).find('.cd-timeline-img').hasClass('is-hidden')) {
                    $(this).find('.cd-timeline-img, .cd-timeline-content').removeClass('is-hidden').addClass('bounce-in');
                }
            });
        });
    }

    jQuery(document).ready(function ($) {
        timeLine();
    });

    TimeLine.GetName = function (value) {
        return value.split('~')[0];
    };
    TimeLine.GetUrl = function (value) {
        return value.split('~')[1];
    };

    TimeLine.GetFullLength = function (year, month) {
        for (var g = 0; g < TimeLine.GroupedData().length; g++) {
            if (TimeLine.GroupedData()[g].Year.toString() == year.toString()) {
                for (var m = 0; m < TimeLine.GroupedData()[g].Months.length; m++) {
                    if (TimeLine.GroupedData()[g].Months[m].Month == month) {
                        return TimeLine.GroupedData()[g].Months[m].MonthCount;
                    }
                }
            }
        }
        return TimeLine.TimeLineData().length;
    }

    TimeLine.GetFullYearLength = function (year) {
        var yearcount = 0;
        for (var g = 0; g < TimeLine.GroupedData().length; g++) {
            if (TimeLine.GroupedData()[g].Year.toString() == year.toString()) {
                for (var m = 0; m < TimeLine.GroupedData()[g].Months.length; m++) {
                    yearcount += TimeLine.GroupedData()[g].Months[m].MonthCount;
                }
                return yearcount;
            }
        }
        return yearcount;
    }


    TimeLine.GetAttachmentAttributes = function (value, type) {
        var returncontent = "";
        if (type == 'name') {
            return value.split('~')[0];
        } else if (type == 'url') {
            return value.split('~')[1];
        } else if (type == 'icon') {
            return value.split('~')[2];
        } else if (type == 'size') {
            return value.split('~')[3];
        }
        return returncontent;
    };


    TimeLine.DeleteNote = function (noteId) {
        var note = "GetNoteContactsCount";
        $.ajax({
            url: url + note,
            type: 'post',
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({ 'noteId': noteId })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            if (data.response.Count == 1 || data.response.SelectAll == true) {
                DeleteNoteConfirmation("Delete Note", "Cancel", "[|Are you sure you want to delete this Note|]?", noteId, contactid);
            }
            else {
                DeleteNoteConfirmation("Delete Note", "Cancel", "[|Note added to more than one Contact. Deleting this Note will remove it from all associated Contacts. Are you sure you want to delete?|]", noteId, 0);
            }
        }).fail(function (error) {
            notifyError(error);
        })
    }

    var DeleteNoteConfirmation = function (okText, cancelText, confirmMessage, noteId,CId) {
        alertifyReset(okText, cancelText);
        alertify.confirm(confirmMessage, function (e) {
            
            if (e) {
                var actionUrl = (pageName == "opportunities" ? (opportunityurl + "OpportunityDeleteNote") : (url + "DeleteNote"));
                $.ajax({
                    url: actionUrl,
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ 'noteId': noteId, 'contactId': CId })
                }).then(function (response) {
                    var filter = $.Deferred()
                    if (response.success) {
                        filter.resolve(response)
                    } else {
                        filter.reject(response.error)
                    }
                    return filter.promise()
                }).done(function (data) {
                    $('.success-msg').remove();
                    notifySuccess('[|Successfully deleted Note|]');
                    setTimeout(
                        function () {
                            window.location.href = document.URL;
                        }, setTimeOutTimer);
                }).fail(function (error) {
                    notifyError(error);
                })
            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }
        });
    }


    TimeLine.DeleteTour = function (tourId) {
        var tour = "GetTourContactsCount";
        $.ajax({
            url: url + tour,
            type: 'post',
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({ 'tourId': tourId })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            if (data.response.Count == 1 || data.response.SelectAll == true) {
                DeleteTourConfirmation("Delete Tour", "Cancel", "[|Are you sure you want to delete this Tour|]?", tourId,data.response.Count == 0 ? contactid:0);
            }
            else {
                DeleteTourConfirmation("Delete Tour", "Cancel", "[|More than one contact tagged for this Tour, are you sure you want to delete this Tour|]?", tourId,0);
            }
        }).fail(function (error) {
            if (error == undefined)
                $('#tourmodeltrigger').modal('toggle');
            else
                notifyError(error);
            
        })
    }


    var DeleteTourConfirmation = function (okText, cancelText, confirmMessage, TourId,Cid) {
        alertifyReset(okText, cancelText);
        alertify.confirm(confirmMessage, function (e) {
            if (e) {
                var tour = "DeleteTour";
                $.ajax({
                    url: url + tour,
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ 'tourId': TourId ,'contactId': Cid})
                }).then(function (response) {
                    var filter = $.Deferred()
                    if (response.success) {
                        filter.resolve(response)
                    } else {
                        filter.reject(response.error)
                    }
                    return filter.promise()
                }).done(function (data) {
                    $('.success-msg').remove();
                    notifySuccess('[|Tour deleted successfully|]');
                    window.location.href = document.URL;
                }).fail(function (error) {
                    notifyError(error);
                })
            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }
        });
    }


    TimeLine.DeleteAction = function (actionId) {
        var action = "GetActionContactsCount";
        $.ajax({
            url: url + action,
            type: 'post',
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({ 'actionId': actionId, 'contactId': contactid })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            if (data.response == 1) {
                DeleteConfirmation("Delete Action", "Cancel", "You’re about to delete this Action. Are you sure you want to delete?", actionId)
            }
            else {
                DeleteConfirmation("Delete Action", "Cancel", "[|More than one Contact is included for this Action,|]" + "</br>" + "[|How do you want to delete this Action?|]?", actionId);
            }
        }).fail(function (error) {
            notifyError(error);
        })
    }


    var DeleteConfirmation = function (okText, cancelText, confirmMessage, ActionId) {
        alertifyReset(okText, cancelText);
        alertify.confirm(confirmMessage, function (e) {
            if (e) {
                var actionUrl = (pageName == "opportunities" ? (opportunityurl + "OpportunityDeleteAction") : (url + "DeleteAction"));
                $.ajax({
                    url: actionUrl,
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ 'actionId': ActionId })
                }).then(function (response) {
                    var filter = $.Deferred()
                    if (response.success) {
                        filter.resolve(response)
                    } else {
                        filter.reject(response.error)
                    }
                    return filter.promise()
                }).done(function (data) {
                    $('.success-msg').remove();
                    if (data.success === true) {
                        notifySuccess('[|Action deleted successfully|]');
                        window.location.href = document.URL;
                    }
                    if (data.success === false) {
                        notifyError(data.response);
                    }
                }).fail(function (error) {
                    notifyError(error);
                })
            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }
        });
    }

    TimeLine.DeleteRelationship = function (relationshipID) {
        alertifyReset("Delete Relation", "Cancel");
        alertify.confirm("[|Are you sure you want to delete this Relationship(s)|]?", function (e) {
            if (e) {
                var action = "DeleteRelation";
                $.ajax({
                    url: url + action,
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ 'relationId': relationshipID })
                }).then(function (response) {
                    var filter = $.Deferred()
                    if (response.success) {
                        filter.resolve(response)
                    } else {
                        filter.reject(response.error)
                    }
                    return filter.promise()
                }).done(function (data) {
                    $('.success-msg').remove();
                    notifySuccess('[|Relationship(s) deleted successfully|]');
                    window.location.href = document.URL;
                }).fail(function (error) {
                    notifyError(error);
                })
            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }
        });
    }

    //var clicksCount = 0;
    //TimeLine.multiselectClick = function (e) {
    //    if (clicksCount % 2 == 0)
    //        console.log("Multiselect opened");
    //    else
    //        console.log("Multiselect closed");
    //    clicksCount += 1;
    //}

    //TimeLine.multiselectChange = function (e) {
    //    clicksCount += 1;
    //}
}