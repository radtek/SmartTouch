$(document).ready(function () {
    $('#sendemailmodal').on('show.bs.modal', function () {
        $(this).find('#templatechange > a:first-child').trigger('click');
    });
});

var SendMailViewModel = function (data, url, isModal, userName, IsIncludeSignature) {
   var selfEmail = this;
    ko.mapping.fromJS(data, {}, selfEmail);


    /////////////////////////////////////////EMAIL VALIDATIONS FOR ARRAY/////////////////////
    var Editor = "";
    if (isModal == "True")
        Editor = "editorModal";
    else
        Editor = "editor";

    ko.validation.rules['minimumLength'] = {
        validator: function (contacts, minimumLength) {
            return (contacts.length > minimumLength);
        },
        message: 'Select at least one recipient'
    };
    ko.validation.registerExtenders();
    selfEmail.sendText = ko.observable('Send');
    selfEmail.From = ko.observable().extend({ required: { message: "Please select From" } });
    selfEmail.SenderName = userName;
    selfEmail.To = ko.observable(data.To);

    selfEmail.CC = ko.observable(data.CC);
    selfEmail.BCC = ko.observable(data.Bcc);
    selfEmail.body = ko.observable(data.body);

    selfEmail.Subject = ko.observable(data.Subject).extend({ required: { message: "Please enter subject" } });


    selfEmail.IsChecked = ko.observable(IsIncludeSignature == "True" ? true : false);
    selfEmail.HideSignature = ko.observable(true);
    selfEmail.IsSendMeCopy = ko.observable(false);
    selfEmail.Emails = ko.observableArray();
    selfEmail.EmailID = ko.observable();
    selfEmail.EmailId = ko.observable('');
    selfEmail.EmailSignature = ko.observable();
    selfEmail.TemplateNames = ko.observableArray();

    selfEmail.Template = ko.observable();

    selfEmail.EmailTemplates = ko.observableArray([]);
    selfEmail.CampaignTemplateType = ko.observable(0);
    selfEmail.selectTemplate = function (template) {
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
            selfEmail.Template(data.response);
            if (data.response != null) {
                var htmlContent = data.response;
                var signature = "";
                if (selfEmail.IsChecked())
                    signature = selfEmail.EmailSignature();
                $("#" + Editor).redactor('code.set', htmlContent + signature);
                selfEmail.TemplateSearch('');
                selfEmail.CampaignTemplateType(0);
                $('#sendemailmodal').modal('hide');
            }
        }).fail(function (error) {
            notifyError(error);
        });
    }
    selfEmail.TemplateSearch = ko.observable('');
    selfEmail.GetEmailTemplates = function () {

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
            selfEmail.EmailTemplates(data.response);
        }).fail(function (error) {
            notifyError(error);
        });
    }
    selfEmail.GetEmailTemplates();
    selfEmail.showAllTemplates = function (data, event) {
        selfEmail.CampaignTemplateType(0);
        OpenTCview(event.target);
    }

    selfEmail.showOnlyPredesigned = function (data, event) {
        selfEmail.CampaignTemplateType(2);
        OpenTCview(event.target);
    }
    selfEmail.showOnlySavedTemplates = function (data, event) {
        selfEmail.CampaignTemplateType(3);
        OpenTCview(event.target);
    }
    selfEmail.showSentCampaigns = function (data, event) {
        selfEmail.CampaignTemplateType(4);
        OpenTCview(event.target);
    }
    selfEmail.FilteredTemplates = ko.pureComputed(function (i) {
        if (selfEmail.CampaignTemplateType() == 0) {
            return ko.utils.arrayFilter(selfEmail.EmailTemplates(), function (temp) {
                return temp.Name.toLowerCase().indexOf(selfEmail.TemplateSearch().toLowerCase()) >= 0;
            });
        }
        else {
            return ko.utils.arrayFilter(selfEmail.EmailTemplates(), function (temp) {
                return temp.Type == selfEmail.CampaignTemplateType() && temp.Name.toLowerCase().indexOf(selfEmail.TemplateSearch().toLowerCase()) >= 0;
            });
        }
    });


    selfEmail.PreviousEmailSignature = ko.observable('');
    $.ajax({
        url: '/User/GetEmails',
        type: 'get',
        dataType: 'json',
        contentType: "application/json; charset=utf-8",
        success: function (emaildata) {
            selfEmail.Emails(emaildata.response);
            $.each(selfEmail.Emails(), function (index, value) {
                value.DisplayEmail = selfEmail.SenderName + " <" + value.EmailId + ">";
                if (value.IsPrimary == true) {
                    selfEmail.EmailID(value.EmailID);
                    selfEmail.EmailId(value.EmailId);
                }
            });
        }
    });

    selfEmail.SendMeaCopy = function (data, event) {
    }

    ko.validation.configure({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null
    });

    selfEmail.Contacts = ko.observableArray(data.Contacts);

    selfEmail.EmailID.subscribe(function (selectedValue) {
        if (selectedValue == "" || selectedValue == null) {
        }
        else {
            selfEmail.EmailID(selectedValue);
            if (selfEmail.EmailSignature() != "")
                selfEmail.PreviousEmailSignature(selfEmail.EmailSignature());

            $.each(selfEmail.Emails(), function (index, value) {
                var signature;
                if (value.EmailSignature)
                    signature = value.EmailSignature.replace(' />', '>');

                var html = $(value.EmailSignature);
                if (html != null && html.length > 0) {
                    var imgTags = $(value.EmailSignature).find('img');
                    if (imgTags) {
                        $.each(imgTags, function (i, v) {
                            var tag = $(v).prop('outerHTML');
                            //v.setAttribute("style", "width:" + width + "px ; height:" + height + "px");
                            v.style.width = v.getAttribute("width") + "px";
                            v.style.height = v.getAttribute("height") + "px";
                            signature = signature.replace(tag, $(v).prop('outerHTML'));
                        });
                    }
                }
                if (value.EmailID == selfEmail.EmailID()) {
                    selfEmail.EmailSignature(signature);
                    selfEmail.EmailId(value.EmailId);
                    selfEmail.From(value.EmailId);
                    selfEmail.ProviderID(value.ServiceProviderID);
                }
            });

            if (selfEmail.IsSendMeCopy() == true)
                selfEmail.BCC(selfEmail.From());
        }
    });

    selfEmail.FromValidation = selfEmail.EmailID.extend({ required: { message: "Please select From" } });

    if (IsIncludeSignature) {
        if (IsIncludeSignature == "False")
            RemoveEmailSignatureFromEmailBody()

        if (IsIncludeSignature == "True") {
            setTimeout(function () {
                if (typeof (selfEmail.EmailID()) == "undefined" || selfEmail.EmailID == '') {
                    //notifyError("Please select an email");
                }
                if (selfEmail.EmailSignature() != null) {
                    if ($("#" + Editor).redactor('code.get') == "") {
                        var html = "<div></div></br></br><div id='signature'> </br></br>" + selfEmail.EmailSignature() + "</div>";
                        $("#" + Editor).redactor('code.set', html);
                    }
                    else {
                        BindingEmailSignatureToEmailBody();
                    }
                }
            }, 1000);

        }
    }


    selfEmail.IsChecked.subscribe(function (newValue) {
        if (newValue == false)
            RemoveEmailSignatureFromEmailBody()

        if (newValue == true) {
            if (typeof (selfEmail.EmailID()) == "undefined" || selfEmail.EmailID == '') {
                // notifyError("Please select an email");
            }
            if (selfEmail.EmailSignature() != null) {
                if ($("#" + Editor).redactor('code.get') == "") {
                    var html = "<div></div></br></br><div id='signature'> </br></br>" + selfEmail.EmailSignature() + "</div>";
                    $("#" + Editor).redactor('code.set', html);
                }
                else {
                    BindingEmailSignatureToEmailBody();
                }
            }

        }
    });


    selfEmail.IsSendMeCopy.subscribe(function (newValue) {
        if (newValue == true)
            selfEmail.BCC(selfEmail.From());
    });
    selfEmail.InsertSignature = function (data, event) {
    };
    if (data.Contacts == null) {
        if (checkedContactEmailValues.length > 0 || localStorage.getItem("contactdetails") != null)
            selfEmail.Contacts(selectedContactEmails(checkedContactEmailValues, data.Contacts, data.ServiceProvider));
    }

    function BindingEmailSignatureToEmailBody() {              //content exist append signature
        Emailbody = $("#" + Editor).redactor('code.get');
        //Need to Do (If already appended do not append again)
        var signatureExist = Emailbody.toString().search("signature");
        console.log(signatureExist);
        if (signatureExist < 1) {
            $("#" + Editor).redactor('code.set', Emailbody + "<div></div></br></br><div id='signature'> </br></br>" + selfEmail.EmailSignature() + "</div>");
        }
        else if (signatureExist > -1) {
            var replaced = Emailbody.replace('id="signature" style="display: none;"', 'id="signature"');
            $("#" + Editor).redactor('code.set', replaced);
        }
    }

    Element.prototype.remove = function () {
        this.parentElement.removeChild(this);
    }
    NodeList.prototype.remove = HTMLCollection.prototype.remove = function () {
        for (var i = 0, len = this.length; i < len; i++) {
            if (this[i] && this[i].parentElement) {
                this[i].parentElement.removeChild(this[i]);
            }
        }
    }


    function RemoveEmailSignatureFromEmailBody() {
        Emailbody = $("#" + Editor).redactor('code.get');
        if (Emailbody != null) {
            var replaced = Emailbody.replace('id="signature"', 'id="signature" style="display: none;"');
            $("#" + Editor).redactor('code.set', replaced);
        }
        else {

        }
    }

    function RemoveEmailSignatureFromExistingEmailBody() {
        var Emailbody = $("#" + Editor).redactor('code.get');

        var matcharray = Emailbody.match("<div> </br></br>" + selfEmail.PreviousEmailSignature() + "</div>");

        if (matcharray != null)
            $("#" + Editor).redactor('code.set', Emailbody.replace("<div> </br></br>" + selfEmail.PreviousEmailSignature() + "</div>", "<p id='sign'> </br></br>" + selfEmail.EmailSignature() + "</p>"));
    }

    function GetEmailByType(contact, index) {
        if (contact.Email != "") {
            if (parseInt(index) == 0)
                Tofield = contact.Email;
            if (parseInt(index) > 0)
                Tofield = Tofield + ";" + contact.Email;
            return Tofield;
        }
    }

    selfEmail.ContactFullNames = ko.computed({
        read: function () {
            var contactFullNames = "";
            if (selfEmail.Contacts() != null) {
                $.each(selfEmail.Contacts(), function (index, value) {
                    if (contactFullNames != null && contactFullNames != "" && value.FullName != null)
                        contactFullNames = contactFullNames + "," + value.FullName;
                    else
                        contactFullNames = contactFullNames + value.FullName;
                });
            }
            return contactFullNames;
        },
        write: function (newValue) {

        },
        owner: this
    });
    selfEmail.contactsValidation = selfEmail.ContactFullNames.extend({ minimumLength: 1 });

    selfEmail.errors = ko.validation.group(selfEmail, {
        observable: true,
        deep: true,
        live: true
    });

    selfEmail.sendEmails = function () {

        if (isModal == "True")
            selfEmail.body($("#editorModal").redactor('code.get'));
        else
            selfEmail.body($('#editor').redactor('code.get'));

        var toField = [];
        var ccField = [];
        var bccField = [];

        if (selfEmail.ServiceProvider() !== 4) {
            $.each(selfEmail.Contacts(), function (index, contact) {
                if (contact.Type == "To") {
                    toField.push(contact.Email);
                }
                else if (contact.Type == "BCC") {
                    bccField.push(contact.Email);
                }
                else if (contact.Type == "CC") {
                    ccField.push(contact.Email);
                }

                selfEmail.To(toField.toString());
                selfEmail.BCC(bccField.toString());
                selfEmail.CC(ccField.toString());
            });
        }

        else {
            $.each(selfEmail.Contacts(), function (index, contact) {
                toField.push(contact.Email);
            });
            selfEmail.To(toField.toString());
        }

        if (selfEmail.IsSendMeCopy() == true) {
            bccField[bccField.length] = selfEmail.From();
            selfEmail.BCC(bccField.toString());
        }

        selfEmail.errors.showAllMessages();
        if (selfEmail.body() == "") {
            notifyError("Please enter body");
            return;
        }

        if (selfEmail.errors().length > 0)
            return;
        selfEmail.EmailTemplates([]);

        var jsondata = ko.toJSON(selfEmail);
        var action = "SendMail";

        innerLoader('redactor-toolbar-0');
        $.ajax({
            url: "/Communication/SendMail",
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'sendMailViewModel': jsondata })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            notifySuccess(data.response);
            createCookie('log', false, 1);
            setTimeout(function () {
                removeinnerLoader('redactor-toolbar-0');
                window.location.href = document.URL;
            }, setTimeOutTimer);
        }).fail(function (error) {
            notifyError(error);
            removeinnerLoader('redactor-toolbar-0');
        });

    }

}

var emailTemplates = function (url) {
    var selfEmailTempaltes = this;
    selfEmailTempaltes.EmailTemplates = ko.observableArray([]);
    selfEmailTempaltes.GetEmailTemplates = function () {
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
            selfEmailTempaltes.EmailTemplates(data.response);
        }).fail(function (error) {
            notifyError(error);
        });
    }
    if (selfEmailTempaltes.EmailTemplates().length == 0)
        selfEmailTempaltes.GetEmailTemplates();
}


checkedContactEmailValues = fnGetCheckedContactEmails();
