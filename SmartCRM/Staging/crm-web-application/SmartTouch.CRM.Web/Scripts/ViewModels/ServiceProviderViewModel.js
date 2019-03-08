var serviceProviderViewModel = function (providerID) {


    ko.validation.init();
    ko.validation.configure({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null,
        grouping: { deep: true, observable: true, live: true }
    });
    console.log(providerID);
    var selfProvider = this;
    selfProvider.IsVMTA = ko.observable(true);
    if (providerID == 3)
        selfProvider.IsVMTA(false);


    selfProvider.ApiKey = ko.observable().extend({
        required: { message: "[|APIKey is required|]", onlyIf: function () { return providerID == 3 } }
    });
    selfProvider.MailChimpListID = ko.observable();
    selfProvider.UserName = ko.observable('').extend({ required: { message: "[|User Name is required|]", onlyIf: function () { return providerID == 4 } } });
    selfProvider.Password = ko.observable('').extend({ required: { message: "[|password is required|]", onlyIf: function () { return providerID == 4 } } });
    selfProvider.Email = ko.observable();
    selfProvider.SenderFriendlyName = ko.observable();
    selfProvider.Host = ko.observable('').extend({ required: { message: "[|Host is required|]", onlyIf: function () { return providerID == 4 } } });
    selfProvider.Email = ko.observable();
    selfProvider.Port = ko.observable().extend({ required: { message: "[|Port is required|]", number: { message: "[|Please Enter Number|]" }, digit: 0, onlyIf: function () { return providerID == 4 } } });
    selfProvider.Email = ko.observable('').extend({ required: { message: "[|Email is required|]" }, maxLength: 256, email: true });
    selfProvider.ImageDomains = ko.observableArray([]);
    selfProvider.MailProviderID = ko.observable(providerID);
    selfProvider.ProviderName = ko.observable().extend({ required: { message: "[|Provider Name is required|]", onlyIf: function () { return providerID == 4 } } });
    selfProvider.Domain = ko.observable();
    selfProvider.ImageDomainId = ko.observable().extend({ required: { message: "[|ImageDomain is required|]", onlyIf: function () { return providerID == 4 } } });
    selfProvider.SenderDomain = ko.observable();
    selfProvider.VMTA = ko.observable().extend({ required: { message: "[|VMTA is required|]", onlyIf: function () { return providerID == 4 } } });
    selfProvider.ImageDomain = ko.observable();
    ko.validation.registerExtenders();


    $.ajax({
        url: 'Account/GetActiveImageDomains',
        type: 'get',
        dataType: 'json',
        contentType: "application/json; charset=utf-8"
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

        selfProvider.ImageDomains(data.response)
    }).fail(function (error) {
        notifyError(error);
    });
    selfProvider.errors = ko.validation.group(selfProvider, {
        observable: true,


    });
    selfProvider.addServicProvider = function () {
        selfProvider.ImageDomain(ko.utils.arrayFirst(selfProvider.ImageDomains(), function (d) { return d.ImageDomainId == selfProvider.ImageDomainId() }).Domain);
        
        selfProvider.errors.showAllMessages();
        if (selfProvider.errors().length > 0)
            return;
        var jsondata = ko.toJSON(selfProvider);
        $.ajax({
            url: "/Communication/InsertServiceProvider",
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'providerViewModel': jsondata })
        }).then(function (response) {
            //removeinnerLoader('divMails');
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            notifySuccess(data.response);
            setTimeout(function () {
               window.location.href = "/accountsettings";
            }, setTimeOutTimer);

        }).fail(function (error) {
            notifyError(error);
        });

    }


}