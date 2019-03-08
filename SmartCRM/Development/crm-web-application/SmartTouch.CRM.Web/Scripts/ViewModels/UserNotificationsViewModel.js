var userNotificationViewModel = function (data, User_BASE_URL) {
    var selfNotifications = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfNotifications));

    selfNotifications.DailySummary = ko.observable(data.DailySummary);
    selfNotifications.AlertNotification = ko.observable(data.AlertNotification);
    selfNotifications.TextNotification = ko.observable(data.TextNotification);
    selfNotifications.EmailNotification = ko.observable(data.EmailNotification);
    selfNotifications.LeadScoreNotification = ko.observable(data.LeadScoreNotification);
    selfNotifications.LeadScoreValue = ko.observable(data.LeadScoreValue).extend({
        required: {
            message: "[|Please enter LeadScore|]",
            onlyIf: function () {  return (selfNotifications.LeadScoreNotification() === true); }
        },
        number: true,
        min : 0
    });

    selfNotifications.AlertNotification.subscribe(function (data) {
        if (ko.toJSON(data) == 'false') {
            selfNotifications.EmailNotification(false);
            selfNotifications.TextNotification(false);
            selfNotifications.LeadScoreNotification(false);
            selfNotifications.LeadScoreValue(0);
        }
    });
    
    selfNotifications.LeadScoreNotification.subscribe(function (data) {
        if (ko.toJSON(data) == 'false')
        {
            selfNotifications.LeadScoreValue(0);
        }
    });

    selfNotifications.AlertNotificationsComputed = ko.pureComputed({
        read: function () {
            return selfNotifications.EmailNotification() || selfNotifications.TextNotification();
        },
        write: function () {

        }
    }).extend({
        equal: {
            message: "[|Select at least one notification method|]",
            onlyIf: function () { return selfNotifications.AlertNotification() === true;}
        }
    });
     
    selfNotifications.errors = ko.validation.group(selfNotifications);

    selfNotifications.SaveSettings = function () {

        selfNotifications.errors.showAllMessages();

        if (selfNotifications.errors().length > 0)
            return;
        var jsondata = ko.toJSON(selfNotifications);

        pageLoader();

        //setTimeout(function () {
        //    $("#dse").attr("disabled", "disabled");
        //}, 1000);

        $.ajax({
            url: User_BASE_URL + "SaveSettings",
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'userSettingsViewModel': jsondata })
        }).then(function (response) {
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response);
            }
            else {
                filter.reject(response.error);
            }
            return filter.promise();
        }).done(function (data) {           
            notifySuccess('[|Successfully Saved your settings|]');
            removepageloader();
            selfNotifications.UserSettingId(data.response);
            window.location.reload();
        }).fail(function () {
            removepageloader();
            notifyError(settingssuccess.response);
        });;
    };

}