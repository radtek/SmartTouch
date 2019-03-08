var bulkSendEmailViewModel = function (data, weburl, webserviceurl, IsIncludeSignature) {
    BulkSendEmail = this;
    ko.mapping.fromJS(data, {}, BulkSendEmail);

    ko.validation.init();
    ko.validation.configure({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null,
        grouping: { deep: true, observable: true, live: true }
    });
    ko.validatedObservable(ko.mapping.fromJS(data, {}, BulkSendEmail));

    BulkSendEmail.Subject = ko.observable(data.Subject).extend({ required: { message: "[|Subject is required|]" } });
    BulkSendEmail.Body = ko.observable(data.Body).extend({ required: { message: "[|Body is required|]" } });
    //BulkSendEmail.bodyValidation = BulkSendEmail.Body.extend({
    //    validation: {
    //        validator: function (val) {
    //            if (val == null || val == "" || val == undefined)
    //                return false;
    //            else
    //                return true;
    //        },
    //        message: '[|Body is required|]'
    //    }
    //});

    BulkSendEmail.CampaignTemplateType = ko.observable(0);
    BulkSendEmail.TemplateSearch = ko.observable('');
    BulkSendEmail.CampaignTemplates = ko.observableArray([]);
    BulkSendEmail.EmailSignature = ko.observable();
    BulkSendEmail.IsChecked = ko.observable(IsIncludeSignature == "True" ? true : false);
    BulkSendEmail.HideSignature = ko.observable(true);
    BulkSendEmail.Emails = ko.observableArray([]);

    //ko.bindingHandlers.editableText = {
    //    init: function (element, valueAccessor) {
    //        $(element).on('blur', function () {
    //            var observable = valueAccessor();
    //            observable($(this).html());
    //        });
    //    },
    //    update: function (element, valueAccessor) {
    //        var value = ko.utils.unwrapObservable(valueAccessor());
    //        $(element).html(value);
    //    }
    //};

    $.ajax({
        url: '/User/GetEmails',
        type: 'get',
        dataType: 'json',
        contentType: "application/json; charset=utf-8",
        success: function (emaildata) {
            BulkSendEmail.Emails(emaildata.response);
            $.each(BulkSendEmail.Emails(), function (index, value) {
                if (value.IsPrimary == true) {
                    BulkSendEmail.EmailSignature(value.EmailSignature);
                }
            });
        }
    });


    if (IsIncludeSignature) {
        if (IsIncludeSignature == "False")
            RemoveEmailSignatureFromEmailBody()

        if (IsIncludeSignature == "True" && BulkSendEmail.EmailSignature() !== null) {
            setTimeout(function () {
                if ($("#bulkeditor").redactor('code.get') == "") {
                    var html = "<div></div></br></br><div id='signature'> </br></br>" + BulkSendEmail.EmailSignature() + "</div>";
                    $("#bulkeditor").redactor('code.set', html);
                }
                else {
                    BindingEmailSignatureToEmailBody();
                }
            }, 500);

        }
    }

    BulkSendEmail.IsChecked.subscribe(function (newValue) {
        console.log(BulkSendEmail.EmailSignature());
        if (newValue == false)
            RemoveEmailSignatureFromEmailBody()

        if (newValue == true && BulkSendEmail.EmailSignature() !== null) {
            if ($("#bulkeditor").redactor('code.get') == "") {
                var html = "<div></div></br></br><div id='signature'> </br></br>" + BulkSendEmail.EmailSignature() + "</div>";
                $("#bulkeditor").redactor('code.set', html);
            }
            else {
                BindingEmailSignatureToEmailBody();
            }
        }
    });

    function RemoveEmailSignatureFromEmailBody() {
        Emailbody = $("#bulkeditor").redactor('code.get');
        if (Emailbody != null) {
            var replaced = Emailbody.replace('id="signature"', 'id="signature" style="display: none;"');
            $("#bulkeditor").redactor('code.set', replaced);
        }
        else {

        }
    }

    function BindingEmailSignatureToEmailBody() {
        Emailbody = $("#bulkeditor").redactor('code.get');
        //Need to Do (If already appended do not append again)
        var signatureExist = Emailbody.toString().search("signature");
        console.log(signatureExist);
        if (signatureExist < 1) {
            $("#bulkeditor").redactor('code.set', Emailbody + "<div></div></br></br><div id='signature'> </br></br>" + BulkSendEmail.EmailSignature() + "</div>");
        }
        else if (signatureExist > -1) {
            var replaced = Emailbody.replace('id="signature" style="display: none;"', 'id="signature"');
            $("#bulkeditor").redactor('code.set', replaced);
        }
        
    }



    BulkSendEmail.GetEmailTemplates = function () {

        $.ajax({
            url: '/Campaign/GetTemplatesForEmails',
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8"
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            BulkSendEmail.CampaignTemplates(data.response);
        }).fail(function (error) {
            notifyError(error);
        });
    }
    BulkSendEmail.GetEmailTemplates();

    BulkSendEmail.FilteredTemplates = ko.pureComputed(function (i) {
        if (BulkSendEmail.CampaignTemplateType() == 0) {
            return ko.utils.arrayFilter(BulkSendEmail.CampaignTemplates(), function (temp) {
                return temp.Name.toLowerCase().indexOf(BulkSendEmail.TemplateSearch().toLowerCase()) >= 0;
            });
        }
        else {
            return ko.utils.arrayFilter(BulkSendEmail.CampaignTemplates(), function (temp) {
                return temp.Type == BulkSendEmail.CampaignTemplateType() && temp.Name.toLowerCase().indexOf(BulkSendEmail.TemplateSearch().toLowerCase()) >= 0;
            });
        }
    });

    BulkSendEmail.showAllTemplates = function (data, event) {
        BulkSendEmail.CampaignTemplateType(0);
        OpenTCview(event.target);
    }

    BulkSendEmail.showOnlyPredesigned = function (data, event) {
        BulkSendEmail.CampaignTemplateType(2);
        OpenTCview(event.target);
    }
    BulkSendEmail.showOnlySavedTemplates = function (data, event) {
        BulkSendEmail.CampaignTemplateType(3);
        OpenTCview(event.target);
    }
    BulkSendEmail.showSentCampaigns = function (data, event) {
        BulkSendEmail.CampaignTemplateType(4);
        OpenTCview(event.target);
    }


    BulkSendEmail.selectTemplate = function (template) {
        $.ajax({
            url: '/Contact/GetEmailTemplate',
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: { 'templateId': template.TemplateId, 'type': template.Type }
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            if (data.response != null) {
                var htmlContent = data.response;
                BulkSendEmail.Body(htmlContent);
                var signature = "";
                if (BulkSendEmail.IsChecked())
                    signature = BulkSendEmail.EmailSignature();

                $("#bulkeditor").redactor('code.set', htmlContent + signature);
                BulkSendEmail.TemplateSearch('');
                BulkSendEmail.CampaignTemplateType(0);
                $('#sendemailmodal').modal('hide');
            }
        }).fail(function (error) {
            notifyError(error);
        });
    }


    BulkSendEmail.errors = ko.validation.group(BulkSendEmail);

    BulkSendEmail.bulkSendEmails = function () {
        BulkSendEmail.Body($("#bulkeditor").redactor('code.get'))

        if (BulkSendEmail.Body() == "" && BulkSendEmail.errors().length == 0) {
            notifyError("Please enter body");
            return;
        }

        if (BulkSendEmail.errors().length == 0) {
            checkedvaluesforactionsgrid = fnGetChkvalforActionGrid('chkaction');
            var array = new Array();
            checkedvaluesforactionsgrid = fnGetChkvalforActionGrid('chkaction');
            $.each(checkedvaluesforactionsgrid, function (ind, val) {
                array.push(val.ActionID);
            })
            if (checkedvaluesforactionsgrid.length == 0) {
                notifyError("[|Please select at least one Action(s).|]");
                return;
            }
            
                alertifyReset("OK", "Cancel");
                alertify.confirm("[|Are you sure you want to send email to  Action(s) Contacts|]?", function (e) {
                    if (e) {
                        pageLoader();
                        jQuery.support.cors = true;
                        $.ajax({
                            url: Contacts_BASE_URL + "BulkSendEmail",
                            data: JSON.stringify({ actionIds: array, subject: BulkSendEmail.Subject(), body: BulkSendEmail.Body() }),
                            type: 'post',
                            dataType: 'json',
                            contentType: 'application/json; charset=utf-8'
                        }).then(function (response) {
                            var filter = $.Deferred()
                            if (response.success) {
                                filter.resolve(response)
                            } else {
                                filter.reject(response.error)
                            }
                            return filter.promise()
                        }).done(function (data) {
                            setTimeout(function () {
                                window.location.href = document.URL;
                            }, setTimeOutTimer);
                        }).fail(function (error) {
                            notifyError(error);
                            removepageloader();
                        })

                    }
                    else {
                        notifyError("[|You've clicked Cancel|].");
                        // checkedvaluesforactionsgrid;
                    }
                });
            
        }
        else {
            BulkSendEmail.errors.showAllMessages();
        }
    }
   
}