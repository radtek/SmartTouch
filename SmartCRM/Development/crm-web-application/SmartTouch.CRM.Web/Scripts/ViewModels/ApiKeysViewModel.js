var apiKeysViewModel = function (data, BASE_URL, accountId, userId, guid) {

    var selfApiKey = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfApiKey));

    var today = new Date();
    var dd = today.getDate();
    var mm = today.getMonth() + 1; //January is 0!

    var yyyy = today.getFullYear();
    if (dd < 10) {
        dd = '0' + dd;
    }
    if (mm < 10) {
        mm = '0' + mm;
    }
    var customdate = mm + '-' + dd + '-' + yyyy;

    selfApiKey.ID = data.ID == null ? ko.observable(guid) : ko.observable(data.ID).extend({ required: { message: "[|Api Key is required|]" } });
    selfApiKey.Name = ko.observable(data.Name).extend({ required: { message: "[|Application Name is required|]" } });
    selfApiKey.AccountID =data.AccountID == 0 ? ko.observable("") : ko.observable(data.AccountID);
    selfApiKey.IsActive = ko.observable(data.IsActive.toString()).extend({ required: { message: "[|Status is required|]" } });
    selfApiKey.RefreshTokenLifeTime = data.RefreshTokenLifeTime == 0 ? ko.observable("").extend({
        required: { message: "[|Refresh Token Life Time is required|]" }, pattern: {
            params: '^0*[1-9][0-9]*$',
            message: "[|Refresh Token Life Time must be a positive number|]"
        }
    }) : ko.observable(data.RefreshTokenLifeTime).extend({
        required: { message: "[|Refresh Token Life Time is required|]" }, pattern: {
            params: '^0*[1-9][0-9]*$',
            message: "[|Refresh Token Life Time must be a positive number|]"
        }
    });
    selfApiKey.AllowedOrigin =data.AllowedOrigin == null ? ko.observable("*") : ko.observable(data.AllowedOrigin).extend({ required: { message: "[|Allowed Origin is required|]" }});
    selfApiKey.LastUpdatedBy = ko.observable(userId);
    selfApiKey.LastUpdatedOn = ko.observable(customdate);
    selfApiKey.Accounts = ko.observableArray([]);
   
    $.ajax({
        url: BASE_URL + 'GetAllAccounts',
        type: 'get',
        dataType: 'json',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            selfApiKey.Accounts(data.response);
        },
        error: function () {
            console.log(errors);
        }
    });

    selfApiKey.AccountValidation = selfApiKey.AccountID.extend({ required: { message: "[|Account  is required|]" } });

    //var ApiKey = {

    //    ID: selfApiKey.ID,
    //    Name: selfApiKey.Name,
    //    AccountId: selfApiKey.AccountId,
    //    IsActive: selfApiKey.IsActive,
    //    RefreshTokenLifeTime: selfApiKey.RefreshTokenLifeTime,
    //    AllowedOrigin: selfApiKey.AllowedOrigin,
    //    LastUpdatedBy: selfApiKey.LastUpdatedBy,
    //    LastUpdatedOn: selfApiKey.LastUpdatedOn
    //}

    ko.validation.configure({
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null,
        grouping: {
            deep: true
        }
    });

    ko.validation.registerExtenders();

    selfApiKey.errors = ko.validation.group(selfApiKey);

    selfApiKey.saveApiKey = function () {

        var jsondata = ko.toJSON(selfApiKey);
        var action;
        if (data.ID !== null)
            action = "UpdateApiKey";
        else
            action = "InsertApiKeys";

        if (selfApiKey.errors().length == 0) {

            $.ajax({
                url: BASE_URL + action,
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'apiKeys': jsondata })
            }).then(function (response) {
                var filter = $.Deferred();
                if (response.success) {
                    filter.resolve(response);
                }
                else {
                    filter.reject(response.error);
                }
                return filter.promise();
            }).done(function () {
                $('.success-msg').remove();
                pageLoader();
                notifySuccess('Successfully saved the Api Key');
                setTimeout(function () {
                    removepageloader();
                    window.location.href = "/apikeys";
                }, setTimeOutTimer);
            }).fail(function (error) {
                $('.success-msg').remove();
                removepageloader();
                notifyError(error);
            });

        }
        else {
            selfApiKey.errors.showAllMessages();
        }

    }

    selfApiKey.cancel = function () {
        window.location = "/apikeys";
    }
}
 
