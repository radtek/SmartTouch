var WorkflowReportDataViewModel = function (data, url, webservice_url, reportdata) {
    selfReport = this;
    selfReport.WorkflowGroup = ko.observableArray(reportdata);
    selfReport.ShowingWorkflowID = ko.observable(0);
    selfReport.ShowingWorkflowID.subscribe(function () {
    });

    ko.utils.arrayForEach(selfReport.WorkflowGroup(), function (item) {
        item.workflowViewModel = ko.observable();
        item.Status = ko.observable(item.Status);
        item.WorkflowName = ko.observable(item.WorkflowName);
        item.IsExpanded = ko.observable(false);
        if (item.WorkflowID == data.WorkflowID) {
            item.workflowViewModel(new workflowReportViewModel(data, url, webservice_url));
            item.workflowViewModel().selectedDays(0);
            selfReport.ShowingWorkflowID(data.WorkflowID);
            item.IsExpanded(true);
        };
        item.IsExpanded.subscribe(function () {
            if (item.IsExpanded() == true && item.workflowViewModel())
                item.workflowViewModel().applyCarousel();
        })

    })

    selfReport.saveWorkflowAs = function () {
        window.location.replace("copyworkflow?WorkflowID=" + selfReport.WorkflowGroup()[0].WorkflowID);
    }
    var auth = readCookie("accessToken");
    function getRelatedCampagins(workflowIndex) {
        return $.ajax({
            url: WEBSERVICE_URL + '/getrelatedcampaigns',
            type: 'get',
            dataType: 'json',
            data: { 'workflowID': selfReport.WorkflowGroup()[workflowIndex].workflowViewModel().workflowID() },
            contentType: "application/json; charset=utf-8",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + auth);
            }
        });
    }
    selfReport.getRelatedCampaigns = function (workflowIndex) {
        getRelatedCampagins(workflowIndex).done(function (campaigns) {
            selfReport.WorkflowGroup()[workflowIndex].workflowViewModel().relatedCampaigns(campaigns);
            if (campaigns.length == 1)
                selfReport.WorkflowGroup()[workflowIndex].workflowViewModel().selectedCampaignId(campaigns.length > 0 ? campaigns[0].CampaignID : 0);
            selfReport.WorkflowGroup()[workflowIndex].workflowViewModel().getEmailStats();
            ddl = $("#campaigns-0").data("kendoDropDownList");
            //if ($(campaigns).length > 0 && ddl) {
            //    ddl.trigger("select", { item: $("li.k-state-selected", $("#campaign-list")) });
            //}
        }).fail(function (err) {
            removepageloader();
            notifyError(err.responseText);
        });
    };
    //selfReport.getRelatedCampaigns();
    selfReport.getDashboard = function (workflowIndex, data) {
        selfReport.WorkflowGroup()[workflowIndex].IsExpanded(!selfReport.WorkflowGroup()[workflowIndex].IsExpanded());
        if (!data.workflowViewModel()) {
            pageLoader();
            $.ajax({
                url: "/workflowreport",
                type: 'get',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: { "workflowID": selfReport.WorkflowGroup()[workflowIndex].WorkflowID },

            }).then(function (response) {
                var filter = $.Deferred();
                if (response.success) {
                    filter.resolve(response);
                } else {
                    filter.reject(response.error);
                }
                return filter.promise();
            }).done(function (response) {
                removepageloader();
                selfReport.WorkflowGroup()[workflowIndex].workflowViewModel(new workflowReportViewModel(response.response, url, webservice_url));
                selfReport.getRelatedCampaigns(workflowIndex);
                selfReport.WorkflowGroup()[workflowIndex].workflowViewModel().applyCarousel();
            }).fail(function (error) {
                notifyError(error);
            });
        }
    }
    selfReport.dateRanges = ko.observableArray([
                { Range: "[|All Time|]", Value: 0 },
                { Range: "[|Last 7 days|]", Value: 7 },
                { Range: "[|Last 30 days|]", Value: 30 },
                { Range: "[|Last 60 days|]", Value: 60 },
                { Range: "[|Last 90 days|]", Value: 90 },
                { Range: "[|Custom|]", Value: 5 }]);

}
var workflowActivity = function () {
    var self = this;
    self.name = ko.observable();
    self.icon = ko.observable();
    self.templateName = ko.observable();
    self.data = ko.observableArray([]);
    self.isTrigger = ko.observable(false);
    self.isDataOnModal = ko.observable(false);
}
var emailEngagement = function () {
    var self = this;
    self.name = ko.observable();
    self.data = ko.observableArray([]);
    self.total = ko.observable();
}

var workflowReportViewModel = function (data, url, webservice_url) {
    selfWorkflow = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfWorkflow));

    ko.bindingHandlers.kendoTooltip.options = {
        autoHide: true,
        position: "top"
    };

    DaysOfWeeks = {
        0: 'Sunday',
        1: 'Monday',
        2: 'Tuesday',
        3: 'Wednesday',
        4: 'Thursday',
        5: 'Friday',
        6: 'Saturday'
    }

    /* storing default settings  */
    var StatusID = data.StatusID, AllowParallelWorkflows = data.AllowParallelWorkflows,
        DeactivatedOn = data.DeactivatedOn, IsWorkflowAllowedMoreThanOnce = data.IsWorkflowAllowedMoreThanOnce,
        RemoveFromWorkflows = data.RemoveFromWorkflows, TimeSensitive = data.DeactivatedOn == null ? "0" : "1";
    selfWorkflow.IsInactiveInitially = ko.observable(data.StatusID == 404 ? true : false);
    //selfWorkflow.selectedCampaignId = ko.observable(data.selectedCampaignId);
    //***variables diclaration****//
    selfWorkflow.saveWorkflowAs = function () {

    },
    selfWorkflow.editWorkflow = function () {

    },
    selfWorkflow.deleteWorkflow = function () {

    },
    selfWorkflow.getEmailStats = function () {
    },
    selfWorkflow.selectedCampaign = function () {
    },

    selfWorkflow.workflowActions = ko.observableArray(data.WorkflowActions);
    selfWorkflow.workflowTriggers = ko.observableArray(data.Triggers);
    selfWorkflow.IsTimeSensitive = ko.observable(data.DeactivatedOn == null ? "0" : "1");
    selfWorkflow.IsWorkflowAllowedMoreThanOnce = ko.observable(data.IsWorkflowAllowedMoreThanOnce == null ? "" : data.IsWorkflowAllowedMoreThanOnce.toString());
    selfWorkflow.AllowParallelWorkflows = ko.observable(data.AllowParallelWorkflows == null ? "" : data.AllowParallelWorkflows.toString());
    selfWorkflow.DeactivateOn = ko.observable(data.DeactivatedOn);
    selfWorkflow.activities = ko.observableArray([]);
    selfWorkflow.contactsStarted = ko.observable(data.ContactsStarted);
    selfWorkflow.contactsInProcess = ko.observable(data.ContactsInProgress);
    selfWorkflow.contactsLost = ko.observable(data.ContactsOptedOut);
    selfWorkflow.contactsFinished = ko.observable(data.ContactsFinished);
    selfWorkflow.workflowID = ko.observable(data.WorkflowID);
    selfWorkflow.relatedCampaigns = ko.observableArray([]);
    selfWorkflow.StatusID = ko.observable(data.StatusID.toString());
    selfWorkflow.selectedCampaignId = ko.observable(0);
    selfWorkflow.selectedDays = ko.observable(),
    selfWorkflow.emailStatFromDate = ko.observable(),
    selfWorkflow.emailStatToDate = ko.observable(),
    selfWorkflow.emailEngagementData = ko.observableArray([]),
    selfWorkflow.ActiveWorkflows = ko.observableArray([]);
    selfWorkflow.RemoveFromWorkflows = ko.observable();
    selfWorkflow.NoWorkflows = ko.observable(false);
    selfWorkflow.FromDate = ko.observable();
    selfWorkflow.ToDate = ko.observable();
    selfWorkflow.tipFilter = "a[title]";
    selfWorkflow.OtherWorkflowNames = ko.observable('');
    selfWorkflow.PeriodValues = ko.observableArray([
     { PeriodID: 6, PeriodValue: '[|Minutes|]' },
     { PeriodID: 5, PeriodValue: '[|Hours|]' },
     { PeriodID: 4, PeriodValue: '[|Days|]' },
     { PeriodID: 3, PeriodValue: '[|Weeks|]' },
     { PeriodID: 2, PeriodValue: '[|Months|]' }
    ]);
    selfWorkflow.RemoveFromWorkflows.subscribe(function (data) {
        if (data) {
            $.each(data, function (i, d) {
                if (selfWorkflow.ActiveWorkflows().map(function (e) { return e.WorkflowID; }).indexOf(d) < 0)
                    selfWorkflow.NoWorkflows(true);
                else
                    selfWorkflow.NoWorkflows(false);
            });
        }
    });
    //***Code starts from here****///
    selfWorkflow.triggerTypes = ko.observableArray([
         { id: 1, name: '[|Contact in Smart Search|]', iconclass: 'st-icon-search-2', isDataOnModal: true },
         { id: 2, name: '[|Form Submitted|]', iconclass: 'st-icon-form-submitted', isDataOnModal: true },
         { id: 3, name: '[|Life Cycle Changed|]', iconclass: 'st-icon-lifecycle-changes', isDataOnModal: false },
         { id: 4, name: '[|Tags Applied|]', iconclass: 'st-icon-tag', isDataOnModal: true },
         { id: 5, name: '[|Campaign Sent|]', iconclass: ' st-icon-bullhorn-2', isDataOnModal: true },
         { id: 6, name: '[|Opportunity Status Changed|]', iconclass: 'st-icon-opputunities', isDataOnModal: false },
         { id: 7, name: '[|Link Clicked|]', iconclass: ' st-icon-link', isDataOnModal: true },
         { id: 9, name: '[|A Lead Adapter is Submitted|]', iconclass: 'st-icon-polaroid-2', isDataOnModal: true },
         { id: 10, name: '[|Lead Score is reached|]', iconclass: 'st-icon-database-add', isDataOnModal: true },
         { id: 11, name: '[|Web Page visited|]', iconclass: 'st-icon-browser-download', isDataOnModal: true },
         { id: 12, name: '[|Action Completed|]', iconclass: 'st-icon-tick', isDataOnModal: true },
         { id: 13, name: '[|Tour Completed|]', iconclass: 'st-icon-pin-2 ', isDataOnModal: true }
    ]);

    selfWorkflow.actionTypes = ko.observableArray([
        { id: 1, name: '[|Send Campaign|]', iconclass: 'st-icon-campaign-sent', isDataOnModal: true },
        { id: 2, name: '[|Send Text|]', iconclass: 'st-icon-speech-bubble-center-2', isDataOnModal: false },
        { id: 3, name: '[|Set Timer|]', iconclass: 'st-icon-stopwatch', isDataOnModal: true },
        { id: 4, name: '[|Add Tags|]', iconclass: 'st-icon-tags-applied', isDataOnModal: true },
        { id: 5, name: '[|Remove Tags|]', iconclass: 'st-icon-tags-remove', isDataOnModal: true },
        { id: 6, name: '[|Adjust Lead Score|]', iconclass: 'st-icon-database-add', isDataOnModal: false },
        { id: 7, name: '[|Change Life Cycle|]', iconclass: ' st-icon-lifecycle-changes', isDataOnModal: false },
        { id: 8, name: '[|Update Field|]', iconclass: 'st-icon-marquee-plus', isDataOnModal: true },
        { id: 9, name: '[|Assign to User(s)|]', iconclass: ' st-icon-user-2-add', isDataOnModal: true },
        { id: 10, name: '[|Notify User|]', iconclass: 'st-icon-notify-team', isDataOnModal: true },
        { id: 12, name: '[|Send Email|]', iconclass: 'st-icon-envelope', isDataOnModal: true },
        { id: 13, name: '[|Trigger Workflow|]', iconclass: 'st-icon-split', isDataOnModal: true },
        { id: 14, name: '[|Link Actions|]', iconclass: 'st-icon-campaign-sent', isDataOnModal: true }
    ]);


    var getTriggerByFlag = function (flag) {
        var trigger = '';
        $.each(selfWorkflow.workflowTriggers(), function (index, value) { if (value.IsStartTrigger == flag) trigger = value; });
        return trigger;
    };

    var buildDataFromAction = function (action) {
        var data = '';
        if (action.Action == null)
            return data;

        if (selfWorkflow.actionTypes()[0].id == action.WorkflowActionTypeID)
            data = action.Action.CampaignName;
        else if (selfWorkflow.actionTypes()[2].id == action.WorkflowActionTypeID) {

            if (action.Action.TimerType == 1) {
                // var RunAt = ConvertToDate(action.Action.RunAt);
                data += 'Wait at least: ' + action.Action.DelayPeriod;
                var period = $.grep(selfWorkflow.PeriodValues(), function (e) {
                    return e.PeriodID == action.Action.DelayUnit;
                })[0];
                data += ' ' + period.PeriodValue + ' </br>';
                data += 'Run On: ';
                data += action.Action.RunOn == 1 ? " Any Day" : " Week Day";
                if (action.Action.RunAt)
                    data += '</br> Run At: ' + moment(action.Action.RunAt).format('hh:mm A');

            } else if (action.Action.TimerType == 2) {
                //var RunAtTime = ConvertToDate(action.Action.RunAtTime);
                if (action.Action.RunType == 1) {
                    data += 'Run On the date ' + moment(action.Action.RunOnDate).format('MM/DD/YYYY') + ' at ' + moment(action.Action.RunAtTime).format('hh:mm A');
                } else {
                    data += 'Run between dates ' + moment(action.Action.StartDate).format('MM/DD/YYYY') + ' & ' + moment(action.Action.EndDate).format('MM/DD/YYYY');
                }
            } else {
                var daysofweek = action.Action.DaysOfWeek;
                for (var i = 0; i < daysofweek.length; i++) {
                    if (i == (daysofweek.length - 1))
                        data += DaysOfWeeks[daysofweek[i]];
                    else
                        data += DaysOfWeeks[daysofweek[i]] + ',';
                }
            }
        }
        else if (selfWorkflow.actionTypes()[3].id == action.WorkflowActionTypeID)
            data = action.Action.TagName;
        else if (selfWorkflow.actionTypes()[4].id == action.WorkflowActionTypeID)
            data = action.Action.TagName;
        else if (selfWorkflow.actionTypes()[5].id == action.WorkflowActionTypeID)
            data = action.Action.LeadScoreValue;
        else if (selfWorkflow.actionTypes()[6].id == action.WorkflowActionTypeID)
            data = action.Action.LifecycleName;
        else if (selfWorkflow.actionTypes()[7].id == action.WorkflowActionTypeID) {
            var name = "";
            if (action.Action.FieldID > 200 || action.Action.FieldID == 22 || action.Action.FieldID == 23 || action.Action.FieldID == 24)
                name = action.Action.Name;
            else
                name = action.Action.FieldValue;
            data = action.Action.FieldName + " : " + name;
        }
        else if (selfWorkflow.actionTypes()[8].id == action.WorkflowActionTypeID) {
            var assignmentAction = action.Action;
            if (assignmentAction.ScheduledID == 1) {
                data = "Schedule: Daily" + "</br>";
                var roundRobinAssignment = assignmentAction.RoundRobinContactAssignments[0];
                if (roundRobinAssignment && roundRobinAssignment.IsRoundRobinAssignment == "0") {
                    var singleUser = roundRobinAssignment.UserNames[0] != null ? roundRobinAssignment.UserNames[0] : "";
                    data += "Assignment: Single User Contact Assignment" + "</br>" + "User: " + singleUser;
                }
                else if (roundRobinAssignment && roundRobinAssignment.IsRoundRobinAssignment == "1")
                    data += "Assignment: Round Robin Contact Assignment" + "</br>" + "Users: " + roundRobinAssignment.UserNames.join(', ');
            }
            else if (assignmentAction.ScheduledID == 2) {
                data = "Schedule: Weekdays & Weekends" + "</br>";
                var roundRobinAssignment = assignmentAction.RoundRobinContactAssignments;
                $.each(roundRobinAssignment, function (i, v) {
                    data += v.DayOfWeek == 8 ? "   Weekday: </br>" : "</br>   Weekend: </br>";
                    var singleUser = v.UserNames[0] != null ? v.UserNames[0] : "";
                    data += v.IsRoundRobinAssignment == "0" ? "Assignment: Single User Contact Assignment" + "</br>" + "User: " + singleUser : "Assignment: Round Robin Contact Assignment" + "</br>" + "Users: " + v.UserNames.join(', ');
                });
            }
            else if (assignmentAction.ScheduledID == 3) {
                data = "Schedule: Specific Days" + "</br>";
                var roundRobinAssignment = assignmentAction.RoundRobinContactAssignments;
                $.each(roundRobinAssignment, function (i, v) {
                    data += v.DayOfWeek == 1 ? "   Monday: </br>" : v.DayOfWeek == 2 ? "</br>   Tuesday: </br>" : v.DayOfWeek == 3 ? "</br>   Wednesday: </br>" : v.DayOfWeek == 4 ? "</br>   Thursday: </br>" : v.DayOfWeek == 5 ?
                        "</br>   Friday: </br>" : v.DayOfWeek == 6 ? "</br>   Saturday: </br>" : v.DayOfWeek == 7 ? "</br>   Sunday: </br>" : "";
                    var singleUser = v.UserNames[0] != null ? v.UserNames[0] : "";
                    data += v.IsRoundRobinAssignment == "0" ? "Assignment: Single User Contact Assignment" + "</br>" + "User: " + singleUser : "Assignment: Round Robin Contact Assignment" + "</br>" + "Users: " + v.UserNames.join(', ');
                });
            }
        }
        else if (selfWorkflow.actionTypes()[9].id == action.WorkflowActionTypeID)
            data = action.Action.UserName;
        else if (selfWorkflow.actionTypes()[10].id == action.WorkflowActionTypeID) {
            data = "Subject: " + action.Action.Subject + "</br>";
            data += "Body: " + action.Action.Body;
        }
        else if (selfWorkflow.actionTypes()[11].id == action.WorkflowActionTypeID)
            data = action.Action.WorkflowName;
        else if (selfWorkflow.actionTypes()[12].id == action.WorkflowActionTypeID)
            data = "[|Multiple Link Actions|]";

        return data;
    }
    var buildDataFromTrigger = function (trigger) {
        var data = '';
        if (selfWorkflow.triggerTypes()[0].id == trigger.TriggerTypeID)
            data = trigger.SearchDefinitionNames.join();
        else if (selfWorkflow.triggerTypes()[1].id == trigger.TriggerTypeID)
            data = trigger.FormNames.join();
        else if (selfWorkflow.triggerTypes()[2].id == trigger.TriggerTypeID)
            data = trigger.LifecycleName;
        else if (selfWorkflow.triggerTypes()[3].id == trigger.TriggerTypeID)
            data = trigger.TagNames.join();
        else if (selfWorkflow.triggerTypes()[4].id == trigger.TriggerTypeID)
            data = trigger.CampaignName;
        else if (selfWorkflow.triggerTypes()[5].id == trigger.TriggerTypeID)
            data = trigger.OpportunityStageName;
        else if (selfWorkflow.triggerTypes()[6].id == trigger.TriggerTypeID) {
            data = "Campaign Name: " + trigger.CampaignName + "</br>";
            data += "Links: " + trigger.SelectedURLs;
        }
        else if (selfWorkflow.triggerTypes()[7].id == trigger.TriggerTypeID)
            data = trigger.LeadAdapterNames.join();
        else if (selfWorkflow.triggerTypes()[8].id == trigger.TriggerTypeID)
            data = trigger.LeadScore;
        else if (selfWorkflow.triggerTypes()[9].id == trigger.TriggerTypeID)
            data = trigger.WebPage;
        else if (selfWorkflow.triggerTypes()[10].id == trigger.TriggerTypeID)
            data = "Action Type : " + trigger.ActionTypeName;
        else if (selfWorkflow.triggerTypes()[11].id == trigger.TriggerTypeID)
            data = "Tour Type : " + trigger.TourTypeName;
        return data;
    }

    var getActionData = function (action) {
        var type = $.grep(selfWorkflow.actionTypes(), function (e) {
            return (e.id == action.WorkflowActionTypeID);
        })[0];
        var activity = new workflowActivity();
        activity.name = type.name;
        activity.icon = type.iconclass;
        activity.data = buildDataFromAction(action);
        activity.ActionTypeID = action.WorkflowActionTypeID;
        activity.WorkflowActionId = action.WorkflowActionID;
        activity.workflowId = selfWorkflow.workflowID();
        activity.isTrigger = false;
        activity.isDataOnModal = type.isDataOnModal;
        activity.isOpen = ko.observable(false);
        activity.myWidget = ko.observable();
        activity.CampaignID = ko.observable(0);
        activity.WorkflowStatus = selfWorkflow.StatusID();
        if (action.Action)
            activity.CampaignID(action.Action.CampaignID);
        activity.isOpen.subscribe(function (newValue) {
            if (newValue) {
                activity.myWidget().center();
            }
        });
        activity.OpenWindow = function () {
            activity.isOpen(true);
        }
        return activity;
    };

    var getTriggerData = function (trigger) {
        var type = $.grep(selfWorkflow.triggerTypes(), function (e) {
            return (e.id == trigger.TriggerTypeID);
        })[0];
        var activity = new workflowActivity();
        activity.name = type.name;
        activity.icon = type.iconclass;
        activity.data = buildDataFromTrigger(trigger);
        activity.ActionTypeID = 1;
        activity.WorkflowActionId = 1;
        activity.workflowId = selfWorkflow.workflowID();
        activity.isTrigger = true;
        activity.isDataOnModal = type.isDataOnModal;
        activity.isOpen = ko.observable(false);
        activity.myWidget = ko.observable();
        activity.CampaignID = ko.observable(trigger.CampaignID);
        activity.WorkflowStatus = "1";
        activity.isOpen.subscribe(function (newValue) {
            if (newValue) {
                activity.myWidget().center();
            }
        });
        activity.OpenWindow = function () {
            activity.isOpen(true);
        }
        return activity;
    };

    selfWorkflow.activities.push(getTriggerData(getTriggerByFlag(true)));
    $.each(selfWorkflow.workflowActions(), function (index, value) {
        selfWorkflow.activities.push(getActionData(value));
    });
    selfWorkflow.activities.push(getTriggerData(getTriggerByFlag(false)));
    selfWorkflow.applyCarousel = function () {
        setTimeout(function () {
            jQuery('#workflowreportview-' + selfWorkflow.WorkflowID()).jcarousel({
                horizantol: true,
                scroll: 2
            });
        }, 500);

    };
    setTimeout(function () {
        selfWorkflow.applyCarousel();
    }, 500);
    selfWorkflow.createChart = function () {
        $('#chart').kendoChart({
            title: {
                text: ""
            }, legend: {
                position: "top"
            },
            seriesDefaults: {
                type: "column"
            },
            theme: 'Metro',
            series: selfWorkflow.emailEngagementData(),
            valueAxis: {
                labels: {
                    format: "{0}"
                },
                line: {
                    visible: false
                },
                axisCrossingValue: 0
            },
            categoryAxis: {
                categories: ['Delivered', 'Opened', 'Clicked', 'Complained', 'Unsubscribed']
            },
            tooltip: {
                visible: true,
                format: "{0}%",
                template: "#= category #: #= value #"
            }
        });
    }
    selfWorkflow.getEmailStats = function () {
        var toDate = new Date();
        var fromDate = new Date();
        var processChartData = function (response) {
            selfWorkflow.emailEngagementData([]);
            if (response != null) {
                $.each(response, function (index, value) {
                    var emailData = new emailEngagement();
                    emailData.data = [];
                    emailData.CampaignID = value.CampaignID;
                    emailData.name = value.CampaignName;
                    emailData.data.push(value.Delivered, value.Opened, value.Clicked, value.Complained, value.Unsubscribed);
                    selfWorkflow.emailEngagementData.push(emailData);
                });
            }
            selfWorkflow.createChart();
        }
        var days = (selfWorkflow.selectedDays() || 0);
        fromDate.setDate(fromDate.getDate() - days);
        selfWorkflow.FromDate(moment(fromDate).format('MM/DD/YYYY'));
        selfWorkflow.ToDate(moment(toDate).format('MM/DD/YYYY'));
        if (days == 5) {
            fromDate = selfWorkflow.emailStatFromDate();
            toDate = selfWorkflow.emailStatToDate();
            selfWorkflow.FromDate(moment(fromDate).format('MM/DD/YYYY'));
            selfWorkflow.ToDate(moment(toDate).format('MM/DD/YYYY'));
        }

        if (selfWorkflow.selectedCampaignId() == "")
            selfWorkflow.selectedCampaignId(0);

        $.ajax({
            url: url + "GetEmailStaistics",
            type: 'get',
            data: { 'campaignID': selfWorkflow.selectedCampaignId(), 'workflowID': selfWorkflow.workflowID(), 'from': moment(fromDate).format('MM/DD/YYYY'), 'to': moment(toDate).format('MM/DD/YYYY') },
            contentType: "application/json; charset=utf-8"
        }).then(function (response) {
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response);
            } else {
                filter.reject(response.error);
            }
            return filter.promise();
        }).done(function (data) {
            processChartData(data.response);
        }).fail(function (error) {
            notifyError(error);
        });
    }


    selfWorkflow.selectedCampaign = function (e) {
        var index = e.item.index();
        var item = e.item[0];
        if (item && item.innerText == "All Time") {
            index = -1;
        }

        if (index >= 0)
            selfWorkflow.selectedCampaignId(this.dataItem(index + 1).CampaignID);
        else if (index < 0)
            selfWorkflow.selectedCampaignId(0);

        selfWorkflow.getEmailStats();
    }
    selfWorkflow.deleteWorkflow = function () {
        alertifyReset("Delete Workflow", "Cancel");
        var message = "Are you sure you want to delete this Workflow?";
        alertify.confirm(message, function (e) {
            if (e) {
                var cid = [selfWorkflow.WorkflowID()];
                var authToken = readCookie("accessToken");
                var jsondata = JSON.stringify({ 'WorkflowIDs': cid });
                pageLoader();
                $.ajax({
                    url: webserviceurl + '/Workflows',
                    type: 'delete',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                    },
                    data: jsondata,
                    success: function () {

                        notifySuccess('Successfully deleted the workflow');
                        setTimeout(
                                function () {
                                    removepageloader();
                                    window.location.href = "/workflows";
                                }, setTimeOutTimer);
                    },
                    error: function (response) {
                        removepageloader();
                        notifyError(response.responseText);
                    }
                });
            }
            else {
                notifyError("You've clicked Cancel");
            }
        });

    }
    var wfName = selfWorkflow.WorkflowName();
    selfWorkflow.editWorkflowName = function () {
        $('#workflownewname').val(selfWorkflow.WorkflowName());
        $('#wf-nm').hide();
        $('#editWorkflowName').show();
    }

    selfWorkflow.editWorkflowCampaign = function (id) {
        window.location.href = "/editcampaign?campaignId=" + id;
    }

    selfWorkflow.saveWorkflowName = function () {
        var newName = $('#workflownewname').val();
        var nameLength = newName.length;
        if (nameLength > 75)
            return notifyError("Please enter no more than 75 characters");

        if (newName) {
            selfWorkflow.WorkflowName(newName);
            $.ajax({
                url: url + "UpdateWorkflowName",
                type: "post",
                dataType: 'json',
                data: { 'name': newName, 'workflowID': selfWorkflow.WorkflowID() }
            }).then(function (response) {
                var filter = $.Deferred();
                if (response.success) {
                    filter.resolve(response);
                } else {
                    filter.reject(response.error);
                }
                return filter.promise();
            }).done(function (data) {
                if (data.success == true) {
                    $('#editWorkflowName').hide();
                    $('#wf-nm').show();
                }

            }).fail(function (err) {
                removepageloader();
                notifyError(err);
            });
        }
        else
            notifyError("Workflow name cannot be empty");
    }

    selfWorkflow.cancelWorkflowNameEdit = function () {
        selfWorkflow.WorkflowName(wfName);
        $('#editWorkflowName').hide();
        $('#wf-nm').show();
    }

    selfWorkflow.showSettings = function (e) {
        $("#workflow-settings").toggleClass('open', 'close')
        $('#workflow-settings-popup').fadeToggle('');
    }
    selfWorkflow.changeStatus = function () {
        pageLoader();
        $.ajax({
            url: url + "UpdateWorkflowStatus",
            type: "post",
            dataType: 'json',
            data: { 'status': selfWorkflow.StatusID(), 'workflowID': selfWorkflow.WorkflowID() }
        }).then(function (response) {
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response);
            } else {
                filter.reject(response.error);
            }
            return filter.promise();
        }).done(function (data) {
            removepageloader();
            selfWorkflow.showSettings();
            if (data.success == true) {
                notifySuccess('Successfully saved the settings');
                if (selfWorkflow.StatusID() == "404")
                    selfWorkflow.IsInactiveInitially(true);
            }
        }).fail(function (err) {
            removepageloader();
            notifyError(err);
        });
    }

    selfWorkflow.cancelSettings = function () {
        selfWorkflow.DeactivatedOn(DeactivatedOn);
        if (selfWorkflow.AllowParallelWorkflows() != null)
            selfWorkflow.AllowParallelWorkflows(data.AllowParallelWorkflows == null ? "" : data.AllowParallelWorkflows.toString());
        selfWorkflow.IsWorkflowAllowedMoreThanOnce(IsWorkflowAllowedMoreThanOnce.toString());
        selfWorkflow.StatusID(StatusID.toString());
        selfWorkflow.RemoveFromWorkflows(RemoveFromWorkflows);
        selfWorkflow.IsTimeSensitive(TimeSensitive);
        selfWorkflow.showSettings();
    };

    if (selfWorkflow.AllowParallelWorkflows() == "3") {
        var cookie = readCookie("accessToken");
        $.ajax({
            url: webservice_url + '/getworkflows',
            type: 'get',
            data: { WorkflowID: selfWorkflow.workflowID() },
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + cookie);
            },
            success: function (workflows) {
                selfWorkflow.ActiveWorkflows(workflows);
                selfWorkflow.RemoveFromWorkflows(data.RemoveFromWorkflows);
                var otherworkflowsname = "";
                for (var i = 0; i < data.RemoveFromWorkflows.length; i++) {
                    var type = $.grep(workflows, function (e) {
                        return (data.RemoveFromWorkflows[i] == e.WorkflowID);
                    })[0];
                    if (type != undefined) {
                        if (i == data.RemoveFromWorkflows.length - 1)
                            otherworkflowsname += type.WorkflowName;
                        else
                            otherworkflowsname += type.WorkflowName + ",";
                    }
                }
                selfWorkflow.OtherWorkflowNames(otherworkflowsname);
            },
            error: function (err) {
                removepageloader();
                notifyError(err.responseText);
            }
        });


    }


    return selfWorkflow;
};

//function ConvertToDate(date) {
//    var milli = date.replace(/\/Date\((-?\d+)\)\//, '$1');
//    var time = new Date(parseInt(milli));
//    time = toLtUtcDate(time);
//    return time;
//}

//function toLtUtcDate(time) {

//    return new Date(this.getUTCFullYear(), this.getUTCMonth(), this.getUTCDate(), this.getUTCHours(), this.getUTCMinutes(), this.getUTCSeconds());
//}

