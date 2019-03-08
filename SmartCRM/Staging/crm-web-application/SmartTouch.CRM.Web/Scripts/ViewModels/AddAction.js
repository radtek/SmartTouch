$(document).ready(function () {
    $('#sendemailmodal').on('show.bs.modal', function () {
        $(this).find('#templatechange > a:first-child').trigger('click');
    });
});

var ActionViewModel = function (data, url, reminderTypes, _dateformat, notselectall, IsIncludeSignature, actionMode) {
    selfAction = this;
    ko.mapping.fromJS(data, {}, selfAction);

    ko.validation.rules['minimumLength'] = {
        validator: function (contacts, minimumLength) {
            return (contacts.length > minimumLength);
        },
        message: '[|Select at least one Contact|]'
    };
    ko.validation.registerExtenders();

    if (notselectall == false)
        data.SelectAll = true;

    selfAction.Contacts = ko.observableArray(data.Contacts);
    selfAction.dateFormat = ko.observable(data.DateFormat);
    selfAction.ReminderTypes = ko.observableArray(reminderTypes);
    selfAction.isIcsCanlender = ko.observable(data.isIcsCanlender);
    selfAction.IcsCanlender = ko.observable(false);
    selfAction._dateformat = ko.observable(_dateformat);
    selfAction.notselectall = ko.observable(notselectall);
    selfAction.SelectAll = ko.observable(data.SelectAll);
    selfAction.PreviousActionType = ko.observable(data.PreviousActionType);
    selfAction.ActionTemplateHtml = ko.observable(data.ActionTemplateHtml);
    selfAction.ShowTemplateIcon = ko.observable(false);
    selfAction.ShowTemplateBody = ko.observable(false);
    selfAction.CampaignTemplateType = ko.observable(0);
    selfAction.TemplateSearch = ko.observable('');
    selfAction.CampaignTemplates = ko.observableArray([]);
    selfAction.MailBulkId = ko.observable(data.MailBulkId);
    selfAction.IncludeSignature = ko.observable(IsIncludeSignature == "True" ? true : false);
    selfAction.HideSignature = ko.observable(true);
    selfAction.Emails = ko.observableArray([]);
    selfAction.ActionTypeValue = ko.observable(data.ActionTypeValue);
    selfAction.EmailSignature = ko.observable();
    selfAction.IsHtmlSave = ko.observable(false);
    selfAction.AddToNoteSummary = ko.observable(false);
    var now = new Date().toUtzDate();
    selfAction.utc_now = ko.observable(new Date(now.getTime() - 1000 * 60));
    //if (!data.ActionTemplateHtml) {
    //}
    //else {
    //   $("#actioneditor").redactor('code.set', data.ActionTemplateHtml);
    //    selfAction.ShowTemplateBody(true);

    //}



    var userId = [];
    userId.push(data.CreatedBy);
    selfAction.OwnerIds = ko.observableArray(data.OwnerIds.length == 0 ? userId : data.OwnerIds);
    selfAction.UsersValidation = selfAction.OwnerIds.extend({ required: { message: '[|Select at least one User|]' } });
    selfAction.Users = ko.observableArray([]);

    selfAction.ReminderMethods = [
        { TypeId: 1, Name: "[|Today|]" },
        { TypeId: 2, Name: "[|Tomorrow|]" },
        { TypeId: 3, Name: "[|Next Week|]" },
        { TypeId: 4, Name: "[|Two Weeks|]" },
        { TypeId: 5, Name: "[|On a Date|]" }
    ];

    selfAction.EmailRequestGuid = ko.observable(data.EmailRequestGuid);
    selfAction.TextRequestGuid = ko.observable(data.TextRequestGuid);

    var selectedOpportunity = GetSelectedOpportunities('chkopportunity');
    selfAction.OppurtunityId = selectedOpportunity.length == 1 ? selectedOpportunity[0].OpportunityID : (readCookie("opportunityid") == null ? 0 : readCookie("opportunityid"));
    selfAction.ActionMessage = ko.observable(data.ActionMessage).extend({ required: { message: "[|Action Details is required|]" }, maxLength: 1000 });

    selfAction.RemindOn = ko.observable(data.RemindOn);
    selfAction.RemindOnDate = ko.pureComputed({
        read: function () {
            if (selfAction.RemindOn() == null) {
                return new Date().toUtzDate();
            }
            else {
                var dateString = selfAction.RemindOn().toString();
                if (dateString.indexOf('/Date') == 0) {
                    var utzdate = ConvertToDate(selfAction.RemindOn()).toUtzDate();
                    selfAction.RemindOn(utzdate);
                    return utzdate;
                }
                else {
                    var date = Date.parse(selfAction.RemindOn());
                    return ConvertToDate(date.toString());
                }
            }
        },
        write: function (newValue) {
            selfAction.RemindOn(new Date(newValue));
        }
    });

    selfAction.ReminderMethod = ko.computed({
        read: function () {
            return computeReminderMethod();
        },
        write: function (newValue) {
        }
    });

    selfAction.PreviesComplete = ko.observable(data.IsCompleted);

    selfAction.ActionTypes = ko.observableArray(data.ActionTypes);
    selfAction.ActionType = ko.observable(data.ActionType);

    var EmialActionTypeId;

    //$.each(selfAction.ActionTypes(), function (ind, val) {
    //    if (val.DropdownValue == "Email")
    //        EmialActionTypeId = val.DropdownValueID;
    //})

    //if (selfAction.ActionType() === EmialActionTypeId) {
    //   // selfAction.ShowTemplateIcon(true);
    //    if (IsIncludeSignature == "False")
    //        RemoveEmailSignatureFromEmailBody()

    //    if (IsIncludeSignature == "True" && selfAction.EmailSignature() !== null) {
    //        setTimeout(function () {
    //            if ($("#actioneditor").redactor('code.get') == "") {
    //                var html = "<div></div></br></br><div id='signature'> </br></br>" + selfAction.EmailSignature() + "</div>";
    //                $("#actioneditor").redactor('code.set', html);
    //                //selfAction.ActionTemplateHtml($("#actioneditor").redactor('code.get'));
    //            }
    //            else {
    //                BindingEmailSignatureToEmailBody();
    //            }
    //        }, 500);

    //    }
    //}
    //else
    //    selfAction.ShowTemplateIcon(false);


    selfAction.IsCompleted = ko.observable(data.IsCompleted);

    selfAction.ActionId = ko.observable(data.ActionId);

    selfAction.DisplayCreatedDate = kendo.toString(kendo.parseDate(ConvertToDate(data.CreatedOn)), selfAction.DateFormat());

    selfAction.DisplayCreatedDate = ko.pureComputed({
        read: function () {
            if (selfAction.CreatedOn() == null) {
                return new Date().toUtzDate();
            }
            else {
                var dateString = selfAction.CreatedOn().toString();
                var dateFormat = readCookie("dateformat").toUpperCase();
                if (dateString.indexOf('/Date') == 0) {
                    var utzdate = ConvertToDate(selfAction.CreatedOn()).toUtzDate();
                    selfAction.CreatedOn(utzdate);
                    return moment(utzdate).format(dateFormat);
                }
                else {
                    var date = Date.parse(selfAction.CreatedOn());
                    return ConvertToDate(date.toString());
                }
            }
        },
        write: function (newValue) {
            selfAction.CreatedOn(new Date(newValue));
        }
    });

    selfAction.IsFromDashboard = ko.observable(false);

    selfAction.MarkAsComplete = function (value) {
        if (value.IsCompleted()) {
            selfAction.IsReminderValid(false);
            selfAction.isReminderTypeValid(false);
            selfAction.SelectedReminderTypes([]);
        }
        else if (value.IsCompleted() === false) {
            selfAction.IsReminderValid(true);
            selfAction.isReminderTypeValid(true);
        }
        return true;
    };

    selfAction.allowIcsCalender = function (value) {
        if (value.IcsCanlender() === true)
            selfAction.IcsCanlender(true);
        else
            selfAction.IcsCanlender(false);
    }

    selfAction.AllowActionToNoteSumarry = function (value) {
        if (value.AddToNoteSummary() === true)
            selfAction.AddToNoteSummary(true);
        else
            selfAction.AddToNoteSummary(false);
    }

    selfAction.IsReminderValid = ko.observable(true);

    selfAction.minDate = ko.pureComputed({
        read: function () {
            if (data.ActionId == 0) {
                return new Date().toUtzDate();
            }
            else
                return new Date(1970, 1, 1);
        }
    });

    ko.validation.configure({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null
    });


    ko.bindingHandlers.editableText = {
        init: function (element, valueAccessor) {
            $(element).on('blur', function () {
                var observable = valueAccessor();
                observable($(this).html());
            });
        },
        update: function (element, valueAccessor) {
            var value = ko.utils.unwrapObservable(valueAccessor());
            $(element).html(value);
        }
    };

    $.ajax({
        url: '/User/GetEmails',
        type: 'get',
        dataType: 'json',
        contentType: "application/json; charset=utf-8",
        success: function (emaildata) {
            selfAction.Emails(emaildata.response);
            $.each(selfAction.Emails(), function (index, value) {
                if (value.IsPrimary == true) {
                    selfAction.EmailSignature(value.EmailSignature);
                }
            });
        }
    });

    selfAction.ActionDate = ko.observable(data.ActionDate);
    selfAction.ActionStartTime = ko.observable(data.ActionStartTime);
    selfAction.ActionEndTime = ko.observable(data.ActionEndTime);

    selfAction.ActionDateOn = ko.pureComputed({
        read: function () {
            if (selfAction.ActionDate() == null) {
                return new Date().toUtzDate();
            }
            else {
                var dateString = selfAction.ActionDate().toString();
                if (dateString.indexOf('/Date') == 0) {
                    var utzdate = ConvertToDate(selfAction.ActionDate()).toUtzDate();
                    selfAction.ActionDate(utzdate);
                    return utzdate;
                }
                else {
                    var date = Date.parse(selfAction.ActionDate());
                    return ConvertToDate(date.toString());
                }
            }
        },
        write: function (newValue) {
            selfAction.ActionDate(new Date(newValue));
            //if (selfAction.ActionDateOn().getTime() > selfAction.utc_now().getTime())
            //    selfAction.ShowTemplateBody(true);
            //else
            //    selfAction.ShowTemplateBody(false);

        }
    });

    //selfAction.templateSelectionValidation = selfAction.ActionTemplateHtml.extend({
    //    validation: {
    //        validator: function (val) {
    //            if ((val == null || val == "" || val == undefined) && selfAction.ActionType() == EmialActionTypeId &&
    //                (selfAction.ActionDateOn().getTime() > selfAction.utc_now().getTime()))
    //                return false;
    //            else
    //                return true;
    //        },
    //        message: '[|Action Type of Email requires content in the Email Body.|]'
    //    }
    //});

    var actionBody = $("#ac-tm-bdy");
    actionBody.bind('keydown keypress', function () {
        setTimeout(function () {
            selfAction.ActionTemplateHtml($("#actioneditor").redactor('code.get'));
        }, 1000);
    });

    selfAction.ActionStartTimeOn = ko.computed({
        read: function () {
            if (selfAction.ActionStartTime() == null) {
                return new Date().toUtzDate();
            }
            else {
                var dateString = selfAction.ActionStartTime().toString();
                if (dateString.indexOf('/Date') == 0) {
                    var utzdate = ConvertToDate(selfAction.ActionStartTime()).toUtzDate();
                    selfAction.ActionStartTime(utzdate);
                    return utzdate;
                }
                else {
                    var date = Date.parse(selfAction.ActionStartTime());
                    return ConvertToDate(date.toString());
                }
            }
        },
        write: function (newValue) {
            selfAction.ActionStartTime(new Date(newValue));
            var startTime = new Date(newValue);
            var endTime = selfAction.ActionEndTimeOn();
            selfAction.ActionEndTimeOn(startTime.setMinutes(startTime.getMinutes() + 30))
            //selfAction.ActionDateOn(new Date(newValue));

        }
    }).extend({ notify: 'always' });

    selfAction.ActionEndTimeOn = ko.computed({
        read: function () {
            if (selfAction.ActionEndTime() == null) {
                var endate = new Date().toUtzDate();
                var newDate = endate.setMinutes(endate.getMinutes() + 30);
                return ConvertToDate(newDate.toString());;
            }
            else {
                var dateString = selfAction.ActionEndTime().toString();
                if (dateString.indexOf('/Date') == 0) {
                    var utzdate = ConvertToDate(selfAction.ActionEndTime()).toUtzDate();
                    selfAction.ActionEndTime(utzdate);
                    return utzdate;
                }
                else {
                    var date = Date.parse(selfAction.ActionEndTime());
                    return ConvertToDate(date.toString());
                }
            }
        },
        write: function (newValue) {
            selfAction.ActionEndTime(new Date(newValue));
        }
    }).extend({ notify: 'always' });

    selfAction.reminderDateValidation = selfAction.RemindOnDate.extend({
        validation: {
            validator: function (val) {
                if (val <= new Date().toUtzDate() && selfAction.SelectedReminderTypes.length > 0)
                    return false;
                else
                    return true;
            },
            message: '[|Remind On time should be later than current time|]'
        }
    });

    selfAction.isReminderTypeValid = ko.observable();

    selfAction.reminderChange = function () {
        if (this.value().length == 0) {
            selfAction.isReminderTypeValid(false);
        } else {
            selfAction.isReminderTypeValid(true);
        }
    };

    selfAction.RemindOnDate.subscribe(function () {
        selfAction.ReminderMethod(computeReminderMethod());
    });
    function computeReminderMethod() {
        var today = new Date().toUtzDate();
        var differenceOfTodayAndReminderDate = GetDifferenceOfTwoDatesInDays(selfAction.RemindOnDate(), today);
        if (differenceOfTodayAndReminderDate == 0) {
            return selfAction.ReminderMethods[0].Name;
        }
        else if (differenceOfTodayAndReminderDate == 1) {
            return selfAction.ReminderMethods[1].Name;
        }
        else if (differenceOfTodayAndReminderDate == 7) {
            return selfAction.ReminderMethods[2].Name;
        }
        else if (differenceOfTodayAndReminderDate == 14) {
            return selfAction.ReminderMethods[3].Name;
        }
        else {
            return selfAction.ReminderMethods[4].Name;
        }
    };
    if ((checkedContactValues.length > 0 || localStorage.getItem("contactdetails") != null)) {
        if (selfAction.SelectAll() == false)
            selfAction.Contacts(selectedContacts(data.ActionId, checkedContactValues, data.Contacts));
        else if (selfAction.SelectAll() == true)
            selfAction.Contacts(selectedContacts(0, checkedContactValues, data.Contacts));
    }

    var GetContacts;
    var arrayContacts;
    var contact;
    if (selectedOpportunity.length == 1) {
        GetContacts = (selectedOpportunity[0].PeopleInvolved).split(",");
        arrayContacts = [];
        if (typeof selfBuyer != "undefined") {
            for (contact = 0; contact < GetContacts.length; contact++) {
                if (GetContacts[contact].split("|")[0] != "")
                    arrayContacts.push({ Address: null, CompanyName: null, ContactType: 0, FullName: GetContacts[contact].split("|")[0], Id: GetContacts[contact].split("|")[1] });
            }
        }
        selfAction.Contacts(arrayContacts);
    }

    if (localStorage.getItem("contactsData") != null && (selfAction.Contacts() == undefined || selfAction.Contacts().length == 0)) {
        GetContacts = [];
        GetContacts = localStorage.getItem("contactsData").split("||");
        arrayContacts = [];
        if (typeof selfBuyer != "undefined") {
            for (contact = 0; contact < GetContacts.length; contact++) {
                if (GetContacts[contact].split("|")[0] != "") {
                    arrayContacts.push({ Address: null, CompanyName: null, ContactType: 0, FullName: GetContacts[contact].split("|")[0], Id: GetContacts[contact].split("|")[1] });
                }
            }
        }
        selfAction.Contacts(arrayContacts);
    }

    selfAction.ContactFullNames = ko.computed({
        read: function () {
            var contactFullNames = "";
            if (selfAction.Contacts() != null) {
                $.each(selfAction.Contacts(), function (index, value) {
                    if (contactFullNames != null && contactFullNames != "" && value.FullName != null)
                        contactFullNames = contactFullNames + "," + value.FullName;
                    else
                        contactFullNames = contactFullNames + value.FullName;
                });
            }
            return contactFullNames;
        },
        write: function () { },
        owner: this
    });
    selfAction.SelectedReminderTypes = ko.observable(data.SelectedReminderTypes);

    selfAction.SelectedReminderTypes.subscribe(function () {
        if (selfAction.SelectedReminderTypes().length > 0)
            selfAction.isReminderTypeValid(true);
        else
            selfAction.isReminderTypeValid(false);
    });

    if (selfAction.SelectAll() == false) {
        selfAction.contactsValidation = selfAction.ContactFullNames.extend({
            minimumLength: 1
        });
    }
    selfAction.TagsList = ko.observableArray(data.TagsList);

    selfAction.Tags = ko.computed({
        read: function () {
            var tags = "";
            if (selfAction.TagsList() != null) {
                $.each(selfAction.TagsList(), function (index, value) {
                    if (tags != null && tags != "")
                        tags = tags + "," + value.TagName;
                    else
                        tags = tags + value.TagName;
                });
            }
            return tags;
        },
        write: function () { },
        owner: this
    });

    //if (IsIncludeSignature == "True" && selfAction.ShowTemplateIcon() == true) {
    //    console.log(IsIncludeSignature);
    //    console.log(selfAction.ShowTemplateIcon())
    //    if (IsIncludeSignature == "False")
    //        RemoveEmailSignatureFromEmailBody()

    //    if (IsIncludeSignature == "True" && selfAction.EmailSignature() !== null) {
    //        setTimeout(function () {
    //            if ($("#actioneditor").redactor('code.get') == "") {
    //                var html = "<div></div></br></br><div id='signature'> </br></br>" + selfAction.EmailSignature() + "</div>";
    //                $("#actioneditor").redactor('code.set', html);
    //            }
    //            else {
    //                BindingEmailSignatureToEmailBody();
    //            }
    //        }, 500);

    //    }
    //}


    selfAction.IncludeSignature.subscribe(function (newValue) {
        if (newValue == false)
            RemoveEmailSignatureFromEmailBody()

        if (newValue == true && selfAction.EmailSignature() !== null) {
            if ($("#actioneditor").redactor('code.get') == "") {
                var html = "<div></div></br></br><div id='signature'> </br></br>" + selfAction.EmailSignature() + "</div>";
                $("#actioneditor").redactor('code.set', html);
                setTimeout(function () {
                    selfAction.ActionTemplateHtml($("#actioneditor").redactor('code.get'));
                }, 1000)
            }
            else {
                BindingEmailSignatureToEmailBody();
            }
        }
    });

    function RemoveEmailSignatureFromEmailBody() {
        var Emailbody = $("#actioneditor").redactor('code.get');
        if (Emailbody) {
            var replaced = Emailbody.replace('id="signature"', 'id="signature" style="display: none;"');
            $("#actioneditor").redactor('code.set', replaced);
            setTimeout(function () {
                selfAction.ActionTemplateHtml($("#actioneditor").redactor('code.get'));
            }, 1000)
        }
        else {

        }
    }

    function BindingEmailSignatureToEmailBody() {
        var Emailbody = $("#actioneditor").redactor('code.get');
        //Need to Do (If already appended do not append again)
        var signatureExist = Emailbody.toString().search("signature");
        if (signatureExist < 1) {
            $("#actioneditor").redactor('code.set', Emailbody + "<div></div></br></br><div id='signature'> </br></br>" + selfAction.EmailSignature() + "</div>");
            setTimeout(function () {
                selfAction.ActionTemplateHtml($("#actioneditor").redactor('code.get'));
            }, 1000)
        }
        else if (signatureExist > -1) {
            var replaced = Emailbody.replace('id="signature" style="display: none;"', 'id="signature"');
            $("#actioneditor").redactor('code.set', replaced);
            setTimeout(function () {
                selfAction.ActionTemplateHtml($("#actioneditor").redactor('code.get'));
            }, 1000)
        }

    }


    selfAction.GetEmailTemplates = function () {

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
            selfAction.CampaignTemplates(data.response);
        }).fail(function (error) {
            notifyError(error);
        });
    }
    selfAction.GetEmailTemplates();

    selfAction.FilteredTemplates = ko.pureComputed(function (i) {
        if (selfAction.CampaignTemplateType() == 0) {
            return ko.utils.arrayFilter(selfAction.CampaignTemplates(), function (temp) {
                return temp.Name.toLowerCase().indexOf(selfAction.TemplateSearch().toLowerCase()) >= 0;
            });
        }
        else {
            return ko.utils.arrayFilter(selfAction.CampaignTemplates(), function (temp) {
                return temp.Type == selfAction.CampaignTemplateType() && temp.Name.toLowerCase().indexOf(selfAction.TemplateSearch().toLowerCase()) >= 0;
            });
        }
    });

    selfAction.showAllTemplates = function (data, event) {
        selfAction.CampaignTemplateType(0);
        OpenTCview(event.target);
    }

    selfAction.showOnlyPredesigned = function (data, event) {
        selfAction.CampaignTemplateType(2);
        OpenTCview(event.target);
    }
    selfAction.showOnlySavedTemplates = function (data, event) {
        selfAction.CampaignTemplateType(3);
        OpenTCview(event.target);
    }
    selfAction.showSentCampaigns = function (data, event) {
        selfAction.CampaignTemplateType(4);
        OpenTCview(event.target);
    }


    selfAction.selectTemplate = function (template) {
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
                var signature = "";
                if (selfAction.IncludeSignature())
                    signature = selfAction.EmailSignature();

                $("#actioneditor").redactor('code.set', htmlContent + signature);
                setTimeout(function () {
                    selfAction.ActionTemplateHtml($("#actioneditor").redactor('code.get'));
                }, 1000)

                selfAction.TemplateSearch('');
                selfAction.CampaignTemplateType(0);
                $('#sendemailmodal').modal('hide');
                $("#bigmodal2").toggle('modal');
            }
        }).fail(function (error) {
            notifyError(error);
        });
    }

    //selfAction.ActionType.subscribe(function (value) {
    //    if (value == EmialActionTypeId) {
    //       // selfAction.ShowTemplateIcon(true);
    //        if (IsIncludeSignature == "False")
    //            RemoveEmailSignatureFromEmailBody()

    //        if (IsIncludeSignature == "True" && selfAction.EmailSignature() !== null) {
    //            setTimeout(function () {
    //                if ($("#actioneditor").redactor('code.get') == "") {
    //                    var html = "<div></div></br></br><div id='signature'> </br></br>" + selfAction.EmailSignature() + "</div>";
    //                    $("#actioneditor").redactor('code.set', html);
    //                    setTimeout(function () {
    //                        selfAction.ActionTemplateHtml($("#actioneditor").redactor('code.get'))
    //                    }, 1000);

    //                }
    //                else {
    //                    BindingEmailSignatureToEmailBody();
    //                }
    //            }, 500);

    //        }
    //    }
    //    else {
    //        selfAction.ShowTemplateIcon(false);
    //        selfAction.ActionTemplateHtml(null);
    //        $("#actioneditor").redactor('code.set', '');
    //    }

    //})



    //if (data.IsCompleted == true && actionMode == "True") {
    //    $(".savebtn").addClass('act-sav');

    //}
    //else
    //    $(".savebtn").removeClass('act-sav');


    selfAction.errors = ko.validation.group(selfAction, true);

    

    var saveAction = function (saveType) {

        //selfAction.ActionTemplateHtml($("#actioneditor").redactor('code.get'));
        //selfAction.IsHtmlSave(selfAction.ShowTemplateIcon());

        if (selfAction.ActionId() == 0 && checkedContactValues.length == 0 && selfAction.SelectAll() == true) {
            notifyError("Select at least one Contact");
        }
        else {
            selfAction.ActionDate(selfAction.ActionDateOn());
            selfAction.ActionStartTime(selfAction.ActionStartTimeOn());
            selfAction.ActionEndTime(selfAction.ActionEndTimeOn());

            var jsondata = ko.toJSON(selfAction);

            if (selfAction.notselectall() == false) {
                selfAction.SelectAll = ko.observable(true);
            }
            var action = saveType;
            selfAction.errors.showAllMessages();
            if (selfAction.errors().length > 0)
                return;
            if (selectedOpportunity.length > 1) {
                notifyError("[|Please select only one Opportunity|]");
                return;
            }

            if ($("#actionremindersection").hasClass('hide') == false && selfAction.SelectedReminderTypes().length == 0 && selfAction.IsReminderValid() == true) {
                notifyError("[|Please select reminder type|]");
                return;
            }

            innerLoader('Action');
            $.ajax({
                url: url + action,
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({
                    'actionViewModel': jsondata
                })
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
                notifySuccess('[|Successfully saved the action|]');
                if (typeof selfDashBoard != 'undefined') {
                    var actionelement = document.getElementById("Action");
                    actionelement.setAttribute("aria-hidden", "true");
                    actionelement.setAttribute("data-dismiss", "modal");
                    actionelement.click();
                    removeinnerLoader('Action');
                    setTimeout(
                        function () {
                            var grid = $('#actiongrid').data('kendoGrid');
                            removepageloader();
                            grid.dataSource.query({
                                sort: { field: "RemindOn", dir: "desc" }, page: 1, pageSize: 10
                            });

                        }, 200);
                }
                else {
                    setTimeout(
                         function () {
                             var location = window.location.href;
                             if (location.indexOf('person') > -1 || location.indexOf('company') > -1)
                                 createCookie('log', false, 1);
                             window.location.href = document.URL;
                         }, setTimeOutTimer);
                }
            }).fail(function (error) {
                // Display error message to user
                removeinnerLoader('Action');
                notifyError(error);
            });
        }
    };

    selfAction.insertAction = function (item, event) {
        saveAction("InsertAction", event.target);
    };

    selfAction.updatingAction = function () {
        saveAction("UpdateAction");
    }

    selfAction.updateAction = function (item, event) {
        if (typeof selfDetails != 'undefined') {
            var id = ko.utils.arrayFirst(selfAction.Contacts(), function (item) {
                return item.Id == selfDetails.ContactID();
            });
            if (id != null && selfAction.Contacts().length > 1 && selfAction.ActionId() > 0 && selfAction.PreviesComplete() != selfAction.IsCompleted()) {
                $('#modal').modal('hide');
                selfDetails.CompletedOperation_ActionEdit(selfAction);
            }
            else {
                saveAction("UpdateAction", event.target);
            }
        }
        else {
            selfAction.IsFromDashboard(true);
            saveAction("UpdateAction", event.target);
        }
    };
}
checkedContactValues = fnGetCheckedValues();