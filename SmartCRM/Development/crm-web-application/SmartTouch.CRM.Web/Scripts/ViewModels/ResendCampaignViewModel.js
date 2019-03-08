var ResendCampaignViewModel = function (data, url) {
    var self = this;

    self.From = ko.observable(data.From);
    self.SenderName = ko.observable(data.SenderName);
    self.Subject = ko.observable(data.Subject);
    self.CampaignId = ko.observable(data.CampaignId);
    self.ParentCampaignId = ko.observable(data.ParentCampaignId);
    self.HeaderTemplate = kendo.template('#if (data.MailProviderID != 4 )  { # <span>#= data.EmailId #</span># } # #if (data.MailProviderID == 4 )  { # <span >#= data.EmailId #</span> <span style="font-style:italic">  (Do not reply email)</span># } #');
    self.EmailTemplate = kendo.template('#if (data.MailProviderID != 4 )  { # <span>#= data.EmailId #</span># } # #if (data.MailProviderID == 4 )  { # <span >#= data.EmailId #</span> <span style="font-style:italic">  (Do not reply email)</span># } #');
    self.CampaignResentTo = ko.observable("122");
    self.heading = ko.pureComputed(function () {
        return self.CampaignResentTo() == '123' ? "[|Resend to not Viewed Contacts|]" : "[|Resend to New Contacts|]"
    });


    self.UserEmails = ko.observableArray([]);
    $.ajax({
        url: '/User/CampaignGetEmails',
        type: 'get',
        dataType: 'json',
        contentType: "application/json; charset=utf-8"
    }).then(function (response) {
        var filter = $.Deferred();
        if (response.success) {
            filter.resolve(response)
        }
        else {
            filter.reject(response.error)
        }
        return filter.promise()
    }).done(function (responseData) {
        emailData = responseData.response;
        emailData = emailData.filter(function (e) { return e.EmailId != null })
        self.UserEmails(emailData);
        var userPrimaryEmail = "";
        var userSecondaryEmail = "";
        if (self.From() == "") {
            ko.utils.arrayFirst(self.UserEmails(), function (item) {
                if (item.IsPrimary === false) {
                    userSecondaryEmail = item.EmailId;
                    self.From(userSecondaryEmail);
                }
            });

            ko.utils.arrayFirst(self.UserEmails(), function (item) {
                if (item.IsPrimary === true) {
                    userPrimaryEmail = item.EmailId;
                    self.From(userPrimaryEmail);
                }
            });
        }

        getDefaultCampaignServiceProvider();
    }).fail(function (error) {
    });


    function getDefaultCampaignServiceProvider() {
        $.ajax({
            url: '/Campaign/GetDefaultCampaignServiceProvider',
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8"
        }).then(function (response) {
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response)
            }
            else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            providerData = data.response;
            if (providerData != null && providerData.SenderEmail != null && providerData.SenderEmail.EmailId != null) {
                self.UserEmails.push(providerData.SenderEmail);

                if (self.CampaignId() == 0) {
                    self.From(providerData.SenderEmail.EmailId);
                }
            }
            if (providerData != null && providerData.SenderName != null && providerData.SenderName != "" && self.CampaignId() == 0) {
                self.SenderName(providerData.SenderName);
            }
        }).fail(function (error) {
        });
    }

    self.ResendCamapign = function ResendCamapign() {
        if ((self.Subject()) != null && self.Subject() != "") {
            innerLoader('resendcampaign');
            var jsondata = ko.toJSON(self);
            $.ajax({
                url: url + 'ResendNewCampaign',
                type: 'post',
                dataType: 'json',
                data: JSON.stringify({ 'viewmodel': jsondata }),
                contentType: "application/json; charset=utf-8"
            }).then(function (response) {
                var filter = $.Deferred();
                if (response.success) {
                    filter.resolve(response)
                } else {
                    filter.reject(response.error)
                }
                return filter.promise()
            }).done(function (data) {
                notifySuccess('[|Successfully resend the campaign.|]');
                setTimeout(
                    function () {
                        window.location.href = document.URL;
                    }, setTimeOutTimer);
                removeinnerLoader('resendcampaign');
            }).fail(function (error) {
                removeinnerLoader('resendcampaign');
                notifyError(error);
            });
        }
        else {
            notifyError("[|Please enter subject.|]");
        }

    }
}