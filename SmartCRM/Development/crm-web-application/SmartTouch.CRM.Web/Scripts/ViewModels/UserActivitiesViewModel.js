var UserActivitiesViewModel = function (data) {
    var selfActivities = this;
    ko.mapping.fromJS(data, {}, selfActivities);

    selfActivities.CurrentUserActivity = ko.observable();
    selfActivities.ViewUserActivity = function (data) {
        selfActivities.CurrentUserActivity(data);
        return true;
    };

    var DeleteActivity = function () {
        alert("User Activity deletion need to be deleted");
    };
}