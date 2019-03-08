var WorkflowNotifyUserViewModel = function (data, Workflow_BASE_URL, WEBSERVICE_URL,wfId) {
     selfNotifyUser = this;
    if (!data) {
        data = {};
    }

    ko.validation.rules['maxLengthDataValidation'] = {
        validator: function (messagebody) {
            if ((selfNotifyUser.NotifyType().toString() == '3' || selfNotifyUser.NotifyType().toString() == '2') && messagebody.length > 160) {
                return false;
            } else if (selfNotifyUser.NotifyType().toString() == '1' && messagebody.length > 512) {
                return false;
            } else {
                return true;
            }
        },
        message: ''
    };
    ko.validation.registerExtenders();

    var sfields = [1, 2, 7, 3, 24, 25, 22, 26];

    selfNotifyUser.WorkflowActionID = ko.observable(data.WorkflowActionID);
    selfNotifyUser.WorkflowNotifyUserActionID = ko.observable(data.WorkflowNotifyUserActionID || 0);
    selfNotifyUser.WorkflowActionTypeID = 10;
    selfNotifyUser.Users = ko.observableArray([]);
    selfNotifyUser.UserID = ko.observable(data.UserID);
    selfNotifyUser.UserIds = ko.observableArray(data.UserIds);
    selfNotifyUser.userValidation = selfNotifyUser.UserIds.extend({ required: { message: "[|User is required|]" } });
    selfNotifyUser.NotifyType = ko.observable(data.NotifyType == undefined ? '3' : data.NotifyType.toString());
    selfNotifyUser.NotifyTypeValidation = selfNotifyUser.NotifyType.extend({ required: { message: "[|Notify type is required|]" } });
    selfNotifyUser.NotificationFields = ko.observableArray([]);
    selfNotifyUser.NotificationFieldIds = ko.observableArray(data.NotificationFieldIds == undefined ? sfields : data.NotificationFieldIds);
    selfNotifyUser.fieldValidation = selfNotifyUser.NotificationFieldIds.extend({ required: { message: "[|Select atleast one field|]" } });
    selfNotifyUser.Order = ko.observable(data.Order);
    selfNotifyUser.MessageBody = ko.observable(data.MessageBody == undefined ? '' : data.MessageBody).extend({
        required: {
            message: "[|Message is required|]"
        }
    });
    selfNotifyUser.charactersremaining = ko.pureComputed({
        read: function () {
            if (selfNotifyUser.NotifyType() == '3' || selfNotifyUser.NotifyType() == '2') {
                return (160 - selfNotifyUser.MessageBody().length) + ((160 - selfNotifyUser.MessageBody().length) > 0 ? ' [|characters remaining|]' : ' [|Too many characters|]');
            } else {
                return (512 - selfNotifyUser.MessageBody().length) + ((512 - selfNotifyUser.MessageBody().length) > 0 ? ' [|characters remaining|]' : ' [|Too many characters|]');
            }
        }
    });
    selfNotifyUser.classname = ko.pureComputed({
        read: function () {

            if (selfNotifyUser.NotifyType().toString() == '3' || selfNotifyUser.NotifyType().toString() == '2') {
                if (selfNotifyUser.MessageBody().length > 160)
                    return 'red num-characters';
                else
                    return 'green num-characters';
            } else {
                if (selfNotifyUser.MessageBody().length > 512)
                    return 'red num-characters';
                else
                    return 'green num-characters';
            }
        }
    });
    selfNotifyUser.MaxLengthValidation = selfNotifyUser.MessageBody.extend({
        maxLengthDataValidation: selfNotifyUser.NotifyType()
    });

    selfNotifyUser.errors = ko.validation.group(selfNotifyUser);
    
    selfNotifyUser.saveNotifyUserAction = function () {
        var jsondata = ko.toJSON(selfNotifyUser);
        pageLoader();
        if (selfNotifyUser.errors().length == 0) {
            $.ajax({
                url: Workflow_BASE_URL + "UpdateNotifyUserAction",
                type: "post",
                dataType: 'json',
                data: { 'notifyuseraction': jsondata }
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
                notifySuccess('Successfully saved the notify user action');
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
            selfNotifyUser.errors.showAllMessages();
        }
       
    }
}