var WorkflowUserAssignmentActionViewModel = function (data, Workflow_BASE_URL, WEBSERVICE_URL, wfId, users) {
    selfUserAssignment = this;
    if (!data) {
        data = {};
    }
    selfUserAssignment.WorkflowActionID = ko.observable(data.WorkflowActionID);
    selfUserAssignment.Users = ko.observableArray(users);
    selfUserAssignment.WorkflowUserAssignmentActionID = ko.observable(data.WorkflowUserAssignmentActionID);
    selfUserAssignment.WorkflowActionTypeID = 9;
    selfUserAssignment.Order = ko.observable(data.Order);
    selfUserAssignment.Weekdays = ko.observableArray([{ Id: 1, Day: "Monday" }, { Id: 2, Day: "Tuesday" }, { Id: 3, Day: "Wednesday" }, { Id: 4, Day: "Thursday" }, { Id: 5, Day: "Friday" },
        { Id: 6, Day: "Saturday" }, { Id: 7, Day: "Sunday" }, { Id: 8, Day: "Week Day" }, { Id: 9, Day: "Weekend" }, { Id: 10, Day: "Daily" }]);

    var assignment_action = [];
    if (data.RoundRobinContactAssignments) {
        $.each(data.RoundRobinContactAssignments, function (index, action) {
            var assigment = new RoundRobinContactAssignment(action);
            assignment_action.push(assigment);
        });
    }
    if (assignment_action.length == 0)
        assignment_action.push(new RoundRobinContactAssignment());
    selfUserAssignment.RoundRobinContactAssignments = ko.observableArray(assignment_action);

    selfUserAssignment.ScheduledID = ko.observable(data.ScheduledID ? data.ScheduledID.toString() : "1");
    selfUserAssignment.WorkflowID = ko.observable(wfId);
    selfUserAssignment.scheduleValidation = selfUserAssignment.ScheduledID.extend({});
    selfUserAssignment.ScheduledID.subscribe(function (id) {
        id = parseInt(id);
        if (id == 1) {
            selfUserAssignment.RoundRobinContactAssignments([]);
            $.each(selfUserAssignment.Weekdays(), function (i, d) {
                if (d.Id == 10) {
                    var data = {};
                    data.DayOfWeek = d.Id;
                    var viewModel = new RoundRobinContactAssignment(data);
                    selfUserAssignment.RoundRobinContactAssignments.push(viewModel);
                }
            });
        }
        else if (id == 2) {
            selfUserAssignment.RoundRobinContactAssignments([]);
            $.each(selfUserAssignment.Weekdays(), function (i, d) {
                if (d.Id > 7 && d.Id < 10) {
                    var data = {};
                    data.DayOfWeek = d.Id;
                    var viewModel = new RoundRobinContactAssignment(data);
                    selfUserAssignment.RoundRobinContactAssignments.push(viewModel);
                }
            });
        }
        else if (id == 3) {
            selfUserAssignment.RoundRobinContactAssignments([]);
            $.each(selfUserAssignment.Weekdays(), function (i, d) {
                if (d.Id < 8) {
                    var data = {};
                    data.DayOfWeek = d.Id;
                    var viewModel = new RoundRobinContactAssignment(data);
                    selfUserAssignment.RoundRobinContactAssignments.push(viewModel);
                }
            });
        }
    });

    selfUserAssignment.errors = ko.validation.group(selfUserAssignment);

    selfUserAssignment.saveUserAssignmentAction = function () {
        var jsondata = ko.toJSON(selfUserAssignment);
        pageLoader();
        if (selfUserAssignment.errors().length == 0) {
            $.ajax({
                url: Workflow_BASE_URL + "UpdateUserAssignmentAction",
                type: "post",
                dataType: 'json',
                data: { 'userassignmentaction': jsondata }
            }).then(function (response) {
                var filter = $.Deferred();
                if (response.success) {
                    filter.resolve(response);
                } else {
                    filter.reject(response.error);
                }
                return filter.promise();
            }).done(function (data) {
                $('.success-msg').remove();
                notifySuccess('Successfully saved the user assignment action');
                setTimeout(function () {
                    removepageloader();
                    window.location.href = "/workflowreport?workflowid=" + wfId;
                }, setTimeOutTimer);
            }).fail(function (err) {
                removepageloader();
                notifyError(err);
            });
        }
        else {
            removepageloader();
            selfUserAssignment.errors.showAllMessages();
        }
    }

    
};

var RoundRobinContactAssignment = function (data) {
    var selfRoundRobin = this;

    if (!data) {
        data = {};
    }

    selfRoundRobin.WorkFlowUserAssignmentActionID = ko.observable(data.WorkFlowUserAssignmentActionID);
    selfRoundRobin.RoundRobinContactAssignmentID = ko.observable(data.RoundRobinContactAssignmentID);
    selfRoundRobin.DayOfWeek = ko.observable(data.DayOfWeek ? data.DayOfWeek : "10");
    selfRoundRobin.IsRoundRobinAssignment = ko.observable(data.IsRoundRobinAssignment ? data.IsRoundRobinAssignment : "0");
    selfRoundRobin.CheckboxOrder = Math.floor(Math.random() * 999) + 1;
    selfRoundRobin.UserID = ko.observable(data.UserID);
    //.extend({
    //    required: {
    //        message: "[|Please select user|]",
    //        onlyIf: function () {
    //            return selfUserAssignment.IsRoundRobinAssignment() == "0";
    //        }
    //    }
    //});
    selfRoundRobin.UserIds = ko.observableArray(data.UserIds);
    //.extend({
    //    required: {
    //        message: "[|Please select users|]",
    //        onlyIf: function () {
    //            return selfUserAssignment.IsRoundRobinAssignment() == "1";
    //        }
    //    }
    //});
    //selfUserAssignment.userIdValidation = selfUserAssignment.UserID.extend({ UserRequired: "" });
    //selfUserAssignment.userIdsValidation = selfUserAssignment.UserIds.extend({ UserRequired: "" });
    selfRoundRobin.errors = ko.validation.group(selfRoundRobin);
};

