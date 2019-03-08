var campaignViewModel = function (data, webApp, service, imageDomain, isSTadmin, campaignMode, wfRedirect, wfIdFound, hasLitmusTestPerm, includeDisclaimer, litmusAPIKey, mailTesterPermission, IsReplicatedCM) {
    selfCampaign = this;
    selfCampaign.Statuses = {
        DRAFT: "101",
        SCHEDULED: "102",
        CANCELED: "103",
        SENT: "105",
        QUEUED: "106",
        FAILED: "110",
        ACTIVE: "107"
    };
    selfCampaign.ImageDomain = imageDomain;
    selfCampaign.campaignSaveAsMode = ko.observable(false);
    if (window.location.href.indexOf('savecampaignas') > 0) {
        selfCampaign.campaignSaveAsMode(true);
        $('#campaignname-breadcrumb').click();
    }

    var recreateLinks = function () {
        var anchorsDiv = document.createElement("div");
        anchorsDiv.innerHTML = data.HTMLContent;
        var links = data.Links;
        var anchorTagsLinks = anchorsDiv.getElementsByTagName("a");
        var anchorTags = [];

        $.each(anchorTagsLinks, function (index, value) {
            var isNewLink = value.href != 'javascript:void(0)' && value.href != '' && value.href.indexOf("linkid") != -1 && value.href.indexOf("tel:") == -1;
            if (isNewLink) {
                anchorTags.push(value);
            }
        });

        $.each(anchorTags, function (index, value) {
            if (value.href.indexOf("linkid") != -1 && links[index] != undefined) {
                var existingIndex = links.map(function (e) { return e.LinkIndex }).indexOf(index);
                if (existingIndex != -1) {
                    value.id = links[existingIndex].CampaignLinkId;
                    value.href = links[existingIndex].URL.URL;
                }
            }
        });
        data.HTMLContent = anchorsDiv.innerHTML;
        data.links = [];
    };
    recreateLinks();

    //TODOD need to get this id from mailaddress
    var EMAILPROVIDERID = 11;

    ko.validation.rules['minimumLength'] = {
        validator: function (contacts, minimumLength) {
            return (contacts.length > minimumLength);
        },
        message: '[|Select at least one tag|]'
    };
    ko.validation.rules['cannotequal'] = {
        validator: function (template) {
            return (template != undefined);
        },
        message: '[|Select a template|]'
    };

    ko.validation.registerExtenders();

    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfCampaign));
    const MINIMUMALLOWEDWIDTH = 600;
    selfCampaign.getMaxWidth = function (htmlContent) {
        var outerHTML = document.createElement('div');
        outerHTML.innerHTML = htmlContent;
        var elementsWidth = $(outerHTML).find('*').map(function () {
            return ($(this).attr('width') || 0).toString().replace('%', '');
        });
        var maxWidth = elementsWidth.length > 0 ? Math.max.apply(Math, elementsWidth) : 600;
        return Math.max(maxWidth, MINIMUMALLOWEDWIDTH) + 40; // 40px padding;
    }
    selfCampaign.CampaignType = ko.observable(data.CampaignTypeId || campaignType.REGULAR);
    selfCampaign.Name = ko.observable(data.Name).extend({ required: { message: "[|Please enter campaign name|]" } });
    selfCampaign.LayoutName = ko.observable();
    selfCampaign.DateFormat = ko.observable(data.DateFormat);
    selfCampaign.SelectedLayout = ko.observable(data.SelectedLayout);
    selfCampaign.CampaignId = ko.observable(data.CampaignID);
    selfCampaign.Templates = ko.observableArray();
    selfCampaign.Designer = ko.observable(new campaignDesigner(data.ContactFields, data.CustomFields, imageDomain, data.HTMLContent, selfCampaign.CampaignType()));
    selfCampaign.Designer().PlainText(data.HTMLContent.replace(/&nbsp;/gi, ' '));
    selfCampaign.CurrentView = ko.observable(data.LastViewedState == 'R' ? 'review' : data.LastViewedState == 'D' ? 'designer' : 'layout');
    selfCampaign.SubView = ko.observable('desktop');
    selfCampaign.PerformLitmusTest = ko.observable(false);
    selfCampaign.PerFormMailTester = ko.observable(false);

    selfCampaign.SubView.subscribe(function (mode) {
        $('#reviewsendblock .campaign-viewbtn').find('a.active').removeClass('active');
        $('#reviewsendbock' + mode).addClass('active');
        $('#reviewcampaignacc .campaigns-body').removeClass('tablet mobile desktop plaintext');
        $('#reviewcampaignacc .campaigns-body').addClass(mode);
        if (mode == 'plaintextversion' && !selfCampaign.PlainTextContent()) {
            selfCampaign.generatePlainTextVersion(false);
        }
    });
    selfCampaign.showBackButton = ko.observable(true);
    selfCampaign.CampaignMode = ko.observable(campaignMode);
    selfCampaign.CurrentView.subscribe(function () {
        if (selfCampaign.CurrentView() == 'designer') {
            setTimeout(function () {
                bindImageGallery();
            }, 2000);
        };
        if (selfCampaign.CurrentView() == 'htmlview' || selfCampaign.CurrentView() == 'layout' || selfCampaign.CurrentView() == 'plaintext' || (selfCampaign.CurrentView() == 'designer' && selfCampaign.CampaignTypeId() == 132))
            selfCampaign.showBackButton(false)
        else
            selfCampaign.showBackButton(true);

    });
    if (selfCampaign.CurrentView() == 'designer') {
        setTimeout(function () {
            bindImageGallery();
        }, 2000)
    }
    selfCampaign.Links = ko.observableArray(selfCampaign.Designer().Links());
    selfCampaign.CurrentTemplateFilter = ko.observable('all');
    selfCampaign.SelectedTemplate = ko.observable(data.CampaignTemplate);
    selfCampaign.HTMLContent = ko.observable(data.HTMLContent || "");
    selfCampaign.PlainTextContent = ko.observable(data.PlainTextContent || "");
    selfCampaign.IncludePlainText = ko.observable(data.IncludePlainText);

    selfCampaign.LastViewedState = ko.observable('L');
    selfCampaign.DisplayCreatedDate = kendo.toString(kendo.parseDate(ConvertToDate(data.CreatedDate)), selfCampaign.DateFormat());
    selfCampaign.Designer().showDevices(false);
    selfCampaign.AccountId = ko.observable(data.AccountId);
    selfCampaign.LitmusGuid = ko.observable(data.LitmusGuid);
    selfCampaign.MailTesterGuid = ko.observable(data.MailTesterGuid);
    selfCampaign.HasListMusTest = ko.observable(hasLitmusTestPerm);
    selfCampaign.MailTesterPermission = ko.observable(mailTesterPermission);
    selfCampaign.RedirectToGrid = ko.observable(true);
    selfCampaign.HasDisclaimer = ko.observable(includeDisclaimer);
    selfCampaign.IsLitmusTestPerformed = ko.observable(data.IsLitmusTestPerformed);
    selfCampaign.IsRecipientsProcessed = ko.observable(data.IsRecipientsProcessed);
    if (data.CampaignTypeId == campaignType.REGULAR) {
        selfCampaign.LastViewedState(data.CampaignId == 0 ? 'L' : data.LastViewedState);
    }
    else if (data.CampaignTypeId == campaignType.HTMLCODE || data.CampaignTypeId == campaignType.PLAINTEXT) {
        selfCampaign.LastViewedState(data.CampaignId == 0 ? 'D' : data.LastViewedState);
        selfCampaign.CurrentView(selfCampaign.LastViewedState() == null ? 'htmlview' : selfCampaign.CurrentView());
        setTimeout(function () {
            var html = cleanHTML(selfCampaign.HTMLContent())
            selfCampaign.Designer().codeMirror.setValue(html);
        }, 2000)

    }

    selfCampaign.DisplayCreatedDate = ko.computed({
        read: function () {
            if (selfCampaign.CreatedDate() == null) {
                return new Date().toUtzDate();
            }
            else {
                var dateString = selfCampaign.CreatedDate().toString();
                var dateFormat = readCookie("dateformat").toUpperCase();
                if (dateString.indexOf('/Date') == 0) {
                    var utzdate = ConvertToDate(selfCampaign.CreatedDate()).toUtzDate();
                    selfCampaign.CreatedDate(utzdate);
                    return moment(utzdate).format(dateFormat);
                }
                else {
                    var date = Date.parse(selfCampaign.CreatedDate());
                    return ConvertToDate(date.toString());
                }
            }
        },
        write: function (newValue) {
            selfCampaign.CreatedDate(new Date(newValue));
        }
    });

    selfCampaign.CampaignTemplate = ko.observable();
    selfCampaign.Contacts = ko.observableArray(data.Contacts);
    selfCampaign.CampaignUnsubscribeStatus = ko.observable(true);
    selfCampaign.CampaignThemes = ko.observableArray([]);
    selfCampaign.isSTadmin = ko.observable(isSTadmin);

    selfCampaign.setUnsubscribe = function (data, event) {
        selfCampaign.CampaignUnsubscribeStatus(event.target.checked);
    }
    var applicationDomain = location.host;
    selfCampaign.unsubscribeLink = ko.observable(location.protocol + "//" + applicationDomain + "/campaignUnsubscribe?crid=*|CRID|*&acct=" + selfCampaign.AccountID() + "");
    selfCampaign.AccountPrivacyPolicy = ko.observable(location.protocol + "//" + applicationDomain);

    selfCampaign.Fields = ko.observable(data.ContactFields);
    selfCampaign.CustomFields = ko.observable(data.CustomFields);
    var linkFields = [1, 2, 3, 7, 17, 18, 19, 20, 25, 27]
    selfCampaign.MergeFieldsForLinks = ko.utils.arrayFilter(selfCampaign.Fields(), function (field) {
        return linkFields.indexOf(field.FieldId) != -1 || field.DropdownId == 1;
    })
    var mergeFieldHtml = '<div class="form-group medium"><label class="control-label">[|Fields|]</label>'
        + '<input data-bind="kendoDropDownList: {dataTextField:\'Title\',dataValueField:\'FieldId\', data: $root.MergeFieldsForLinks,select:function(){console.log(this._value)},optionLabel: \'[|Select|]...\' }"  /></div>';
    selfCampaign.unsubscribeLink = ko.observable(location.protocol + "//" + location.host + "/campaignUnsubscribe?crid=*|CRID|*&acct=" + selfCampaign.AccountID() + "");
    selfCampaign.AccountPrivacyPolicy = ko.observable(location.protocol + "//" + location.host);
    selfCampaign.IsLinkedToWorkflows = ko.observable(data.IsLinkedToWorkflows);

    selfCampaign.FacebookPost = ko.observable(data.FacebookPost).extend({
        required: {
            message: "[|Please enter message to post|]",
            onlyIf: function () {
                return selfCampaign.EnablePostToFacebook();
            }
        }
    });
    selfCampaign.FacebookAttachmentPath = ko.observable(data.FacebookAttachmentPath);
    selfCampaign.TwitterPost = ko.observable(data.TwitterPost).extend({
        maxLength: 140, required: {
            message: "[|Please enter message to tweet|]",
            onlyIf: function () {
                return selfCampaign.EnablePostToTwitter();
            }
        }
    });
    selfCampaign.EnablePostToFacebook = ko.observable(data.EnablePostToFacebook);
    selfCampaign.EnablePostToTwitter = ko.observable(data.EnablePostToTwitter);
    selfCampaign.CampaignStatus = ko.observable(data.CampaignStatus.toString());
    selfCampaign.IsWorkflowID = ko.observable(wfIdFound);
    selfCampaign.ToTagStatus = ko.observable(data.ToTagStatus.toString());
    selfCampaign.SSContactsStatus = ko.observable(data.SSContactsStatus.toString());
    selfCampaign.EnableFacebook = ko.observable(data.EnableFacebook);
    selfCampaign.ScheduleTimeUTC = ko.observable();
    selfCampaign.EnableTwitter = ko.observable(data.EnableTwitter);
    selfCampaign.EnableSocial = ko.pureComputed(function () {
        return selfCampaign.EnableFacebook() || selfCampaign.EnableTwitter();
    });
    if (selfCampaign.FacebookPost() === undefined) {
        selfCampaign.FacebookPost(selfCampaign.Subject());
    }
    if (selfCampaign.TwitterPost() === undefined) {
        selfCampaign.TwitterPost(selfCampaign.Subject());
    }
    function toggleSocial(selector, newValue) {
        $(selector).toggleClass('hide', !(newValue));
    }
    selfCampaign.EnablePostToFacebook.subscribe(function (newValue) {
        toggleSocial('#postonfb', newValue);
    });
    selfCampaign.EnablePostToTwitter.subscribe(function (newValue) {
        toggleSocial('#tweetontwitter', newValue);
    });
    if (selfCampaign.EnablePostToFacebook() == true) {
        $('#postonfb').removeClass('hide');
    }
    if (selfCampaign.EnablePostToTwitter() == true) {
        $('#tweetontwitter').removeClass('hide');
    }

    selfCampaign.Subject = ko.observable(data.Subject).extend({
        maxLength: 75, required: {
            message: "[|Please enter campaign subject|]",
            onlyIf: function () {
                return selfCampaign.CampaignStatus() == campaignStatus.QUEUED || selfCampaign.CampaignStatus() == campaignStatus.SCHEDULED || selfCampaign.CampaignStatus() == campaignStatus.ACTIVE;
            }
        }
    });

    selfCampaign.ScheduleTime = data.ScheduleTime == null ? ko.observable(data.ScheduleTime).extend({ required: { params: true, message: "[|Schedule time is required|]", onlyIf: function () { return selfCampaign.CampaignStatus() == campaignStatus.SCHEDULED } } }) : ko.observable(ConvertToDate(data.ScheduleTime).toUtzDate()).extend({
        required: {
            params: true,
            message: "[|Schedule time is required|]",
            onlyIf: function () {
                return selfCampaign.CampaignStatus() == campaignStatus.SCHEDULED;
            }
        }
    });
    selfCampaign.uploadfile = function () {
        if ($("#txtFriendlyName").val() === "") {
            notifyError("[|Please enter friendly name|]");
        }
        else {
            $('.k-upload-selected').click();
            $('.k-upload-status-total').css({ "position": "relative", "left": "15px" });

        }
    }
    ko.validation.rules.ScheduleDateTime = {
        validator: function (ScheduleTime) {
            if (ScheduleTime === null)
                return true;
        },
        message: "[|Schedule time is required|]"

    }

    selfCampaign.scheduletimeValidation = selfCampaign.ScheduleTime.extend({ ScheduleDateTime: 0 });

    selfCampaign.GetDateFormat = function () {
        return readCookie("dateformat" + " hh:mm tt");
    }

    $.ajax({
        url: '/Account/GetAccountPrivacyPolicy',
        type: 'get',
        dataType: 'json',
        contentType: "application/json; charset=utf-8",
        success: function (privacyPolicy) {
            selfCampaign.AccountPrivacyPolicy(privacyPolicy);
        },
        error: function () {
        }
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
    }).fail(function () {
    });

    selfCampaign.currentTime = new Date().toUtzDate();
    selfCampaign.sampleEmails = ko.observableArray([
    { EmailId: "sales@smarttouch.com", IsVerfied: true },
    { EmailId: "support@smarttouch.com", IsVerfied: true },
    { EmailId: "builders@smarttouch.com", IsVerfied: true },
    { EmailId: "realtors@smarttouch.com", IsVerfied: true }]);


    selfCampaign.ContactTags = ko.observableArray([]);
    selfCampaign.SearchDefinitions = ko.observableArray(data.SearchDefinitions);

    selfCampaign.Images = ko.observableArray([]);
    selfCampaign.searchImage = ko.observable();
    selfCampaign.openImageSelect = function () {
        $("#socialImages").find('div.st-img-gallery').first().remove();
        $('#imageSelect').modal();
    }

    var mappedContactTagsData = ko.utils.arrayMap(data.ContactTags, function (tag) {
        return new tagViewModel(tag.TagName, tag.TagID, WEBSERVICE_URL);
    });
    selfCampaign.ContactTags(mappedContactTagsData);

    var mappedSearchDefinitionsData = ko.utils.arrayMap(data.SearchDefinitions, function (searchDefinition) {
        return new searchDefinitionViewModel(searchDefinition.SearchDefinitionName, searchDefinition.SearchDefinitionID, WEBSERVICE_URL);
    });
    selfCampaign.SearchDefinitions(mappedSearchDefinitionsData);

    selfCampaign.TotalUniqueContacts = ko.observable(data.TotalUniqueContacts);
    selfCampaign.TotalActiveUniqueContacts = ko.observable(data.TotalActiveUniqueContacts);
    selfCampaign.TotalAllAndActiveUniqueContacts = ko.observable(data.TotalAllAndActiveUniqueContacts);
    selfCampaign.TotalActiveAndAllUniqueContacts = ko.observable(data.TotalActiveAndAllUniqueContacts);

    selfCampaign.To_All = ko.observable(data.To_All);
    selfCampaign.To_Active = ko.observable(data.To_Active);
    selfCampaign.SS_All = ko.observable(data.SS_All);
    selfCampaign.SS_Active = ko.observable(data.SS_Active);
    selfCampaign.TotalRecipientsCount = ko.observable();

    selfCampaign.contactsCountValidation = selfCampaign.TotalUniqueContacts.extend({
        min: {
            params: 1,
            message: "[|No recipients found|]",
            onlyIf: function () {
                return selfCampaign.CampaignStatus() == campaignStatus.QUEUED || selfCampaign.CampaignStatus() == campaignStatus.SCHEDULED;
            }
        }
    });

    var convertToCommaSeparatedString = function (tagsArray) {
        if (tagsArray == null)
            return "";
        var tags = [];
        $.each(tagsArray, function (index, value) {
            if (ko.isObservable(value.TagName))
                tags.push(value.TagName());
            else
                tags.push(value.TagName);
        });
        return tags.join(",");
    }

    //Revisit this function. Should use "convertToCommaSeparatedString" method to fulfill the requirement.
    var convertToCommaSeparatedString2 = function (tagsArray) {
        if (tagsArray == null)
            return "";
        var tags = [];
        $.each(tagsArray, function (index, value) {
            if (ko.isObservable(value.SearchDefinitionName))
                tags.push(value.SearchDefinitionName());
            else
                tags.push(value.SearchDefinitionName);
        });
        return tags.join(",");
    }

    selfCampaign.TagsList = ko.observableArray(data.TagsList);
    selfCampaign.CampaignTags = ko.pureComputed(function () {
        return convertToCommaSeparatedString(selfCampaign.TagsList());
    });

    selfCampaign.CampaignContactTags = ko.pureComputed(function () {
        return selfCampaign.ContactTags();
    });

    selfCampaign.CampaignSearchDefinitions = ko.pureComputed(function () {
        return convertToCommaSeparatedString2(selfCampaign.SearchDefinitions());
    });

    selfCampaign.templateValidation = selfCampaign.SelectedTemplate.extend({ cannotequal: "" });
    selfCampaign.From = ko.observable(data.From);//.extend({ required: { message: "[|Please select From email|]" } });
    selfCampaign.SenderName = ko.observable(data.SenderName);
    selfCampaign.From.subscribe(function (newFrom) {
        var newEmail = ko.utils.arrayFirst(selfCampaign.UserEmails(), function (item) {
            if (item.Email == newFrom)
                return true;
            else
                return false;
        });
        if (newEmail && newEmail.SenderName)
            selfCampaign.SenderName(newEmail.SenderName);
    });

    selfCampaign.UserEmails = ko.observableArray([]);
    function getCampaignServiceProviders() {
        $.ajax({
            url: '/getcommunicationproviders',
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
            var providerData = data.response;
            if (providerData != null) {
                selfCampaign.ServiceProviders(providerData.RegistrationListViewModel);
                if (selfCampaign.ServiceProviderID() > 0) {
                    selfCampaign.defaultServiceProvider = ko.utils.arrayFirst(selfCampaign.ServiceProviders(), function (e) {
                        return e.ServiceProviderID == selfCampaign.ServiceProviderID();
                    });

                }
                else {
                    selfCampaign.defaultServiceProvider = ko.utils.arrayFirst(providerData.RegistrationListViewModel, function (e) {
                        if (e.IsDefault == true)
                            return true;
                        else
                            return false;
                    });
                }
                if (selfCampaign.defaultServiceProvider && selfCampaign.defaultServiceProvider.Email) {
                    var item = selfCampaign.defaultServiceProvider;
                    var useremailExits = selfCampaign.UserEmails().filter(function (e) { return e.Email == selfCampaign.From() });
                    var newEmail = {
                        "Email": item.Email,
                        "MailProviderID": item.MailProviderID,
                        "IsDefault": item.IsDefault,
                        "ServiceProviderID": item.ServiceProviderID,
                        "SenderName": item.SenderFriendlyName
                    };
                    selfCampaign.UserEmails.push(newEmail);
                    if (useremailExits.length == 0)
                        selfCampaign.From(newEmail.Email);
                    if (selfCampaign.SenderName() == undefined || selfCampaign.SenderName() == "")
                        selfCampaign.SenderName(newEmail.SenderName);
                }

                var sameUser = selfCampaign.UserEmails().filter(function (e) { return e.Email == selfCampaign.From() });
                if (sameUser.length == 0 && selfCampaign.ServiceProviderID() > 0) {
                    selfCampaign.From(selfCampaign.UserEmails()[0].Email);
                    selfCampaign.SenderName(selfCampaign.UserEmails()[0].SenderName);
                }
            }
            else {
                notifyError("Please note that the account is not configured with campaign service provider");
            }
            selfCampaign.bindFromDropdown(true);

        }).fail(function () {
            selfCampaign.bindFromDropdown(true);
        });
    }


    $.ajax({
        url: '/User/CampaignGetEmails',
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
    }).done(function (responseData) {
        //emailData = responseData.response;
        ko.utils.arrayForEach(responseData.response, function (item) {
            var newEmail = {
                "Email": item.EmailId,
                "MailProviderID": 0,
                "IsDefault": item.IsPrimary,
                "ServiceProviderID": 0,
                "SenderName": ""
            };
            selfCampaign.UserEmails.push(newEmail);

            if (!selfCampaign.From() && newEmail.IsDefault == true && selfCampaign.CampaignId() == 0)
                selfCampaign.From(newEmail.Email);
        });
        getCampaignServiceProviders();
    }).fail(function () {
    });

    selfCampaign.ServiceProviderID = ko.observable(data.ServiceProviderID);
    selfCampaign.ServiceProviderID.subscribe(function (item) {
        selfCampaign.UserEmails(selfCampaign.UserEmails().filter(function (e) { return e.ServiceProviderID == 0 }));
        var newEmail;
        if (item) {
            var newServiceProvider = ko.utils.arrayFirst(selfCampaign.ServiceProviders(), function (e) {
                return e.ServiceProviderID == item;
            });
            newEmail = {
                "Email": newServiceProvider.Email,
                "MailProviderID": newServiceProvider.MailProviderID,
                "IsDefault": newServiceProvider.IsDefault,
                "ServiceProviderID": newServiceProvider.ServiceProviderID,
                "SenderName": newServiceProvider.SenderFriendlyName
            };
            if (newEmail.Email) {
                selfCampaign.UserEmails().push(newEmail);
                selfCampaign.From(newEmail.Email);
                selfCampaign.SenderName(newServiceProvider.SenderFriendlyName);
            }
        }
        else {
            var defaultServiceProvider = ko.utils.arrayFirst(selfCampaign.ServiceProviders(), function (e) {
                return e.IsDefault == true;
            });
            newEmail = {
                "Email": defaultServiceProvider.Email,
                "MailProviderID": defaultServiceProvider.MailProviderID,
                "IsDefault": defaultServiceProvider.IsDefault,
                "ServiceProviderID": defaultServiceProvider.ServiceProviderID,
                "SenderName": defaultServiceProvider.SenderFriendlyName
            };
            selfCampaign.UserEmails.push(newEmail);
            selfCampaign.From(newEmail.Email);
            selfCampaign.SenderName(newEmail.SenderName);
        }
    });
    selfCampaign.ServiceProviders = ko.observableArray([]);
    selfCampaign.bindFromDropdown = ko.observable(false);


    ko.bindingHandlers.checkbox = {
        init: function (element, valueAccessor, allBindings, data, context) {
            var $element, observable;
            observable = valueAccessor();
            $element = $(element);
            $element.on("click", function () {
                observable(!observable());
            });
            ko.pureComputed({
                disposeWhenNodeIsRemoved: element,
                read: function () {
                    $element.toggleClass("active", observable());
                }
            });
        }
    };





    ko.bindingHandlers.sortable.options = {
        helper: 'clone',
        axis: 'y',
        opacity: .5,
        cursor: 'move',
        start: function (e, ui) {
            ui.placeholder.height(ui.helper.outerHeight());
        },
        update: function (e, ui) {
        },
        placeholder: 'ui-sortable-placeholder-campaign'
    };

    ko.bindingHandlers.sortable.beforeMove = function (args) {
        //e = args;
    }

    var droppableControl = function (dropCallback, isImageBasedControl) {
        var droppableClass, acceptorClass;

        if (isImageBasedControl) {
            droppableClass = ".st-imgdroparea";
            acceptorClass = ".dropimg";
        }
        else {
            droppableClass = ".st-droparea";
            acceptorClass = ".item";
        }

        $(droppableClass).droppable({
            greedy: true,
            hoverClass: "st-drop-valid",
            accept: acceptorClass,
            drop: dropCallback
        });
    }

    getImageDim = function (ui, sourceImageUrl, container) {
        var pic_real_width = 0;
        var pic_real_height = 0;
        $("<img/>") // Make in memory copy of image to avoid css issues
            .attr("src", sourceImageUrl)
            .load(function () {
                pic_real_width = this.width > selfCampaign.Designer().campaignMaxWidth() - 45 ? selfCampaign.Designer().campaignMaxWidth() : this.width;   // Note: $(this).width() will not
                pic_real_height = this.height; // work for in memory images.
                containerId = container.find('img').attr('id');
                var containerWidth = container.attr('width');
                $("#canvas #" + containerId).attr('width', container.find('img').attr('width') == "50%" ? "50%" : '100%');
                $("#canvas #" + containerId).attr('height', 'auto');
                $("#canvas #" + containerId).css({ 'height': 'auto', 'width': container.find('img').attr('width') == "50%" ? "50%" : '100%' });
            });
    }

    selfCampaign.selectedImage = ko.observable('');

    selfCampaign.imageBeingChanged = ko.observable('');

    bindImageGallery = function () {
        setTimeout(function () {
            $('#designerControls img').off();
            $('#designerControls img').click(function () {
                selfCampaign.Designer().currentControlBox('contentimages');
                $('.campaigns-theme-controls a').removeClass('active');
                $('.campaigns-theme-controls #contentimages').addClass('active');
                GetImagessearch();
            });
        }, 500)
    };

    applyDroppables = function () {
        droppableControl(function (event, ui) {
            $Container = $(this);
            var controlBeingAdded = $(ui.draggable)[0].id;
            if ($('.st-droparea.st-drop-valid').length) {
                var controlId = selfDesigner.Controls().length;
                var control = new controlTemplate(controlId, 'control-table');
                $('#canvas #droppable').html($('#canvas #droppable').html());
            } else {
                var addAtIndex = $('.campaigns-list.st-droparea > div.stc-dropping-here').index();
                var index = selfCampaign.Designer().AddControl(controlBeingAdded, addAtIndex == -1 ? 0 : addAtIndex, true);
            }
            selfCampaign.Designer().CaptureAllEdits();
            setTimeout(function () {
                selfCampaign.applyUI();
                bindImageGallery();
            }, 500);
            applyDroppables();
        }, false);

        droppableControl(function (event, ui) {
            $Container = $(this);
            var sourceImageUrl = $($(ui.draggable)[0]).find('img').attr('src');
            $(this).find('img').attr('src', sourceImageUrl);
            debugger;
            getImageDim(ui, sourceImageUrl, $Container);
            selfCampaign.Designer().CaptureAllEdits();
            applyDroppables();
            selfCampaign.Designer().convertToWidgets();
        }, true);
    }
    ko.bindingHandlers.sortable.afterMove = function (args) {
        applyDroppables();
        bindImageGallery();
        setTimeout(function () {
            selfCampaign.applyUI();
        }, 500);
    }

    var applyDroppableToImg = function () {
        $("img").droppable({
            drop: function (event, ui) {
                //eve = event;
                //u = ui;
                //var currentLI = eve.target.closest("li")
                //var index = $("#designerControls").children().index(currentLI);
                //selfCampaign.Designer().Controls()[index].EditedHTML(currentLI.innerHTML);
            }
        });
    }
    var reloadContactTags = function () {
        convertToCommaSeparatedString(selfCampaign.ContactTags());
    }
    selfCampaign.ContactTags.subscribe(function () {
        reloadContactTags();
    });


    reloadContactTags();
    var getCampaignRecipientsCount = function () {
        var authToken = readCookie("accessToken");
        var jsonData = { 'tags': selfCampaign.ContactTags(), 'searchDefinitions': selfCampaign.SearchDefinitions(), 'accountId': selfCampaign.AccountId };
        $.ajax({
            url: service + '/CampaignRecipientsCount',
            type: 'post',
            data: ko.toJSON(jsonData),
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + authToken);
            },
            success: function () {
                //$("#add-definition-loader").addClass("hide");
            }
        });

    };
    selfCampaign.SearchDefinitions.subscribe(function () {
        getCampaignRecipientsCount();
    });

    selfCampaign.HeaderTemplate = kendo.template('#if (data.MailProviderID != 4 )  { # <span>#= data.Email #</span># } # #if (data.MailProviderID == 4 )  { # <span >#= data.Email #</span> <span style="font-style:italic">  (Do not reply email)</span># } #');
    selfCampaign.EmailTemplate = kendo.template('#if (data.MailProviderID != 4 )  { # <span>#= data.Email #</span># } # #if (data.MailProviderID == 4 )  { # <span >#= data.Email #</span> <span style="font-style:italic">  (Do not reply email)</span># } #');
    var reloadSearchDefinitions = function () {
        convertToCommaSeparatedString2(selfCampaign.SearchDefinitions());
    }
    reloadSearchDefinitions();



    selfCampaign.cancelCampaignSave = function () {
        alertifyReset();
        alertify.set({ buttonReverse: true });
        alertify.confirm("[|Are you sure you want to exit from this campaign without saving?|]", function (e) {
            if (e) {
                window.location.href = "/campaigns";
            }
        });
    }

    selfCampaign.SelectedTemplateId = ko.observable(data.CampaignTemplate.TemplateId);

    selfCampaign.PreviewTemplate = ko.observable();

    selfCampaign.ToAddress = ko.observable();

    selfCampaign.ProviderId = ko.observable(EMAILPROVIDERID);

    selfCampaign.searchTemplateQuery = ko.observable('');

    selfCampaign.FilteredTemplates = ko.pureComputed(function () {
        return ko.utils.arrayFilter(selfCampaign.Templates(), function (item) {
            var filteredTemplate = item.Name.toLowerCase().indexOf(selfCampaign.searchTemplateQuery().toLowerCase()) >= 0;
            if (selfCampaign.CurrentTemplateFilter() == 'all') {
                return filteredTemplate;
            }
            else if (selfCampaign.CurrentTemplateFilter() == 'layout') {
                return item.Type == 1 && filteredTemplate;
            }
            else if (selfCampaign.CurrentTemplateFilter() == 'predesigned') {
                return item.Type == 2 && filteredTemplate;
            }
            else if (selfCampaign.CurrentTemplateFilter() == 'saveTemplate') {
                return item.Type == 3 && filteredTemplate;
            }
            else {
                return true;
            }
            // Reset();
        });
    });




    selfCampaign.showAllTemplates = function (data, event) {
        selfCampaign.CurrentTemplateFilter('all');
        OpenTCview(event.target);
    }
    selfCampaign.showOnlyLayouts = function (data, event) {
        selfCampaign.CurrentTemplateFilter('layout');
        OpenTCview(event.target);
    }
    selfCampaign.showOnlyPredesigned = function (data, event) {
        selfCampaign.CurrentTemplateFilter('predesigned');
        OpenTCview(event.target);
    }
    selfCampaign.SavedTemplates = function (data, event) {
        selfCampaign.CurrentTemplateFilter('saveTemplate');
        OpenTCview(event.target);
    }
    var campaignSelectCount = 0;
    selfCampaign.selectTemplate = function (selectedTemplate) {
        $('#campaigntemplatealert').removeClass('in').addClass('hide');
        if (campaignSelectCount > 0 || selfCampaign.CampaignId() > 0) {
            alertify.set({ buttonReverse: true });
            alertify.confirm("[|You have unsaved changes in the Campaign. Changing the layout will result in your changes being lost. Would you like to continue with changes? |]", function (e) {
                var authToken = readCookie("accessToken");
                if (e) {
                    pageLoader();

                    $.ajax({
                        url: service + '/CampaignTemplate',
                        type: 'get',
                        data: {
                            'campaignTemplateID': selectedTemplate.TemplateId
                        },
                        dataType: 'json',
                        contentType: "application/json; charset=utf-8",
                        beforeSend: function (xhr) {
                            xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                        },
                        success: function (template) {
                            selfCampaign.LoadControls(template.CampaignTemplateViewModel.HTMLContent, template.CampaignTemplateViewModel);
                            campaignSelectCount += 1;
                            selfCampaign.Designer().StyleSheet(new campaignStyleSheet());
                            selfCampaign.showDesigner();
                            $("#contentdesign").removeClass('active');
                            $("#contentimages").removeClass('active');
                            $("#DesignControls").addClass('active');
                            $("#DesignControls").click();

                        },
                        error: function (error) {
                            notifyError(error);
                        }
                    });
                    removepageloader();
                }
                else {
                    notifyError("[|You've clicked Cancel|]");
                }

            });
        }
        else {
            pageLoader();
            var authToken = readCookie("accessToken");
            $.ajax({
                url: service + '/CampaignTemplate',
                type: 'get',
                data: {
                    'campaignTemplateID': selectedTemplate.TemplateId
                },
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                },
                success: function (template) {
                    selfCampaign.LoadControls(template.CampaignTemplateViewModel.HTMLContent, template.CampaignTemplateViewModel);
                    campaignSelectCount += 1;
                    selfCampaign.showDesigner();
                    selfCampaign.Designer().campaignMaxWidth(selfCampaign.getMaxWidth(selfCampaign.HTMLContent()));
                    selfCampaign.Designer().StyleSheet = ko.observable(new campaignStyleSheet(template.CampaignTemplateViewModel.HTMLContent));
                },
                error: function (error) {
                    notifyError(error);
                }
            });
            removepageloader();
        };

    }

    selfCampaign.previewTemplateMaxWidth = ko.observable(600);
    selfCampaign.previewTemplate = function (data) {
        var authToken = readCookie("accessToken");
        $.ajax({
            url: service + '/CampaignTemplate',
            type: 'get',
            data: { 'campaignTemplateID': data.TemplateId },
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + authToken);
            },
            success: function (template) {
                selfCampaign.PreviewTemplate(template.CampaignTemplateViewModel);
                selfCampaign.previewTemplateMaxWidth(selfCampaign.getMaxWidth(selfCampaign.PreviewTemplate().HTMLContent));
            }
        });
    }

    selfCampaign.showLayout = function () {
        $(".innerWrapper").remove();
        selfCampaign.CurrentView('layout');
        selfCampaign.LastViewedState('L');
    }
    selfCampaign.showDesigner = function () {
        selfCampaign.Designer().CaptureAllEdits();
        var html = cleanHTML(selfCampaign.Designer().IsFullEdit() == false ? selfCampaign.Designer().ViewableHTML().trim() : selfCampaign.HTMLContent());
        selfCampaign.HTMLContent(html);
        selfDesigner.codeMirror.setValue(selfCampaign.HTMLContent())
        setTimeout(function () {
            selfDesigner.codeMirror.refresh();
        }, 1);
        selfCampaign.CurrentView(selfCampaign.CampaignTypeId() == 133 ? 'htmlview' : 'designer');
        selfCampaign.LastViewedState('D');
        setTimeout(function () {
            if (selfCampaign.Designer().IsFullEdit() == false)
                selfCampaign.applyUI();
        }, 500);

        if (selfCampaign.Designer().IsFullEdit() == false)
            applyDroppables();

        $('#designer-preview .campaign-viewbtn #mobileview').removeClass('active');
        $('#designer-preview .campaign-viewbtn #tabletview').removeClass('active');
        $('#designer-preview .campaign-viewbtn #desktopview').addClass('active');
        setTimeout(function () {
            if (selfCampaign.Designer().IsFullEdit() == false)
            {
                bindImageGallery();
                selfDesigner.convertToWidgets();
            }
            
        }, 500);
    }

    selfCampaign.applyUI = function () {
        applyDroppables();
        $(".editable .k-br").remove();
    }

    selfCampaign.LoadControls = function (htmlContent, campaignTemplate) {
        newInnerHtml = document.createElement('div');
        newInnerHtml.innerHTML = htmlContent;
        selfCampaign.SelectedTemplate(campaignTemplate);
        selfCampaign.SelectedTemplateId(campaignTemplate.TemplateId);
        selfCampaign.CampaignTemplate(campaignTemplate);
        selfCampaign.Designer().Controls([]);
        var domElement = $(newInnerHtml).find("#htmlcontent_R");

        var count = 0;
        domElement.children().each(function () {

            var newTemplate = new controlTemplate(count++, 'fromLayout');
            var isStyleTag = $(this).find('style');
            if (!$(this).hasClass('innerWrapper')) {
                newTemplate.EditedHTML($(this).wrap('<p/>').parent().html());
                newTemplate.IsStyleTag($(this).find('style').length == 0 ? false : true);
                selfCampaign.Designer().Controls().push(newTemplate);
            }
        });
        selfCampaign.Designer().Controls.valueHasMutated();
        setTimeout(function () {
            $("#canvas img").parent().each(function () {
                $(this).addClass('st-imgdroparea');
            });
            $("#canvas img").removeClass('st-imgdroparea').removeClass('ui-droppable');
            selfCampaign.applyUI();

        }, 500);
        removepageloader();
    }

    selfCampaign.reviewCampaign = function () {
        $(".innerWrapper").remove();
        selfCampaign.Designer().CaptureAllEdits();
        selfCampaign.Designer().updateControls();
        if (selfCampaign.CampaignTypeId() == campaignType.HTMLCODE) {
            selfCampaign.HTMLContent(selfDesigner.codeMirror.getValue());
        }
        else if (selfCampaign.CampaignTypeId() == campaignType.REGULAR) {
            if (selfCampaign.Designer().IsFullEdit() == false)
                selfCampaign.HTMLContent(selfCampaign.Designer().ViewableHTML().trim());

        } else if (selfCampaign.CampaignTypeId() == campaignType.PLAINTEXT) {
            selfCampaign.HTMLContent(selfCampaign.Designer().PlainText());
        }

        selfCampaign.CurrentView('review');
        selfCampaign.LastViewedState('R');
        selfDesigner.isDesktopMode(true);
    }

    selfCampaign.deleteCampaign = function () {
        if (selfCampaign.CampaignId() === 0) { return; }
        alertifyReset("Delete Campaign", "Cancel");
        alertify.set({ buttonReverse: true });
        alertify.confirm("[|Are you sure you want to delete this Campaign?|]", function (e) {
            if (e) {
                var cid = [selfCampaign.CampaignId()];
                var jsondata = JSON.stringify({ 'CampaignID': cid });

                var varDeleteURL = webApp + "DeleteCampaign";

                jQuery.support.cors = true;
                $.ajax({
                    url: varDeleteURL,
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ 'campaignIDs': jsondata })

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
                    notifySuccess("[|Successfully deleted the campaign|]");
                    if (data.success === true) {
                        window.location.href = "/campaigns";
                    }
                }).fail(function () {
                    notifyError("[|Campaign could not be deleted.|]");
                });
            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }
        });
    }

    selfCampaign.GetTemplates = function () {
        pageLoader();
        var authToken = readCookie("accessToken");
        $.ajax({
            url: service + '/CampaignTemplates',
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + authToken);
            },
            success: function (data) {
                selfCampaign.Templates(data.Templates);
            }
        });
        removepageloader();
    }

    selfCampaign.errors = ko.validation.group(selfCampaign);

    selfCampaign.validateCampaign = function () {
        selfCampaign.errors.showAllMessages();
        selfCampaign.Designer().errors.showAllMessages();
        if (selfCampaign.Designer().errors().length > 0) {
            validationScroll();
            selfCampaign.showDesigner();
            return true;
        }
        if (selfCampaign.errors().length > 0) {
            validationScroll();
            selfCampaign.reviewCampaign();
            return true;
        }

        return false;
    }
    selfCampaign.clickback = function () {

        if (selfCampaign.CurrentView() == 'review') {
            if (selfCampaign.CampaignTypeId() == campaignType.REGULAR)
                selfCampaign.CurrentView('designer')
            else if (selfCampaign.CampaignTypeId() == campaignType.PLAINTEXT || selfCampaign.CampaignTypeId() == campaignType.HTMLCODE)
                selfCampaign.CurrentView('htmlview')
        }
        else if (selfCampaign.CurrentView() == 'designer') {
            selfCampaign.CurrentView('layout');
        }
    }

    selfCampaign.nextButtonEvent = ko.observable('saveCampaign')

    selfCampaign.clicknext = function () {
        if (selfCampaign.CurrentView() == 'layout') {
            selfCampaign.CurrentView('designer');
        }
        else if (selfCampaign.CurrentView() == 'designer' || selfCampaign.CurrentView() == 'htmlview') {
            selfCampaign.CurrentView('review');
        }
        else if (selfCampaign.CurrentView() == 'review') {
            if (selfCampaign.CampaignStatus() == campaignStatus.DRAFT) {
                $('.next-btn a').text('Save');
                selfCampaign.nextButtonEvent('saveCampaign');
            }
            else if (selfCampaign.CampaignStatus() == campaignStatus.SCHEDULED) {
                $('.next-btn a').text('Schedule');
                selfCampaign.nextButtonEvent('sendCampaign');
            }
            else if (selfCampaign.CampaignStatus() == campaignStatus.QUEUED) {
                $('.next-btn a').text('Send');
                selfCampaign.nextButtonEvent('sendCampaign');
            }
        }
    }

    selfCampaign.generateTrackableURLs = function () {

        var domainName = location.protocol + "//" + location.host;
        domainName = domainName.toLowerCase();
        codeHTML = document.createElement('div');
        if (selfCampaign.CampaignType() == campaignType.HTMLCODE) {
            $('#campaignsdesignarea').remove();
            codeHTML.id = 'campaignsdesignarea';
            codeHTML.innerHTML = selfCampaign.HTMLContent();
            codeHTML.style.display = 'none';
            $('.body-container').append(codeHTML);
        }
        anchorTags = $('#campaignsdesignarea a');
        $.each(anchorTags, function (index, value) {
            selfCampaign.Designer().temporaryLink = ko.observable(ko.mapping.fromJS(linkTemplate));
            var linkID = value.id;
            var isNewLink = (value.href && value.href != 'javascript:void(0)') && (value.href != '') && (value.href != '#') && (value.href.indexOf('crid=*|CRID|*') < 0) && value.href.indexOf('mailto:') < 0;
            if (isNewLink) {
                var httpIncluded = value.href.indexOf(location.origin);
                if (httpIncluded != -1) {
                    value.href = value.href.replace(location.origin, "http://");
                }
                selfCampaign.Designer().temporaryLink().URL.URL(value.href);
                value.href = domainName + "/campaignimage?crid=*|CRID|*&linkid=" + selfCampaign.Designer().Links().length;
                selfCampaign.Designer().temporaryLink().LinkIndex(selfCampaign.Designer().Links().length);
                if (linkID != 0 && linkID != null) {
                    if (data.CampaignID != 0)
                        selfCampaign.Designer().temporaryLink().CampaignLinkID(value.id);
                    selfCampaign.Designer().temporaryLink().CampaignId(data.CampaignID);
                }
                selfCampaign.Designer().Links.push(selfCampaign.Designer().temporaryLink())
            }
        });

        if (selfCampaign.CampaignType() == campaignType.HTMLCODE)
            selfCampaign.HTMLContent(codeHTML.innerHTML);
        else
            selfCampaign.Designer().CaptureAllEdits();
    }


    selfCampaign.generateTrackableHTML = function (isTestEmail) {
        var imageSource = selfDesigner.domainName + "/campaignimage?crid=*|CRID|*";

        selfCampaign.Designer().campaignMaxWidth(selfCampaign.getMaxWidth(selfCampaign.Designer().ViewableHTML()));
        var trackableHTML = '<table width="100%" cellspacing="0" cellpadding="0" border="0" style="margin:auto;"><tr><td align="center" valign="top" style="vertical-align:top;">'
                             + '<table valign="center" border="0" cellspacing="0" cellpadding="0" width="' + selfCampaign.Designer().campaignMaxWidth() + '" style=" margin:0 auto; background-color:none; border:none;">'
                             + '<tbody><tr><td><center>'
                             + '<table class="st-mail-temp"  style="background-color:' + selfDesigner.StyleSheet().BackGround.OuterBackGround() + ';padding:5px 20px 5px 20px;">'
                             + '<tbody>'
                                    + '<tr><td id="htmlcontent_R" style="margin:0 auto;min-height:200px; '
                                        + ' border-style:' + selfDesigner.StyleSheet().Border.BorderStyle()
                                        + '; border-width:' + selfDesigner.StyleSheet().Border.BorderThickness()
                                        + '; border-color:' + selfDesigner.StyleSheet().Border.BorderColor()
                                        + '; background-color:' + selfDesigner.StyleSheet().BackGround.InnerBackGround() + '">';
        //htmlContent =
        //(trackableHTML + selfCampaign.Designer().ViewableHTML()
        htmlContent =
        (trackableHTML + (selfCampaign.CampaignTypeId() == campaignType.HTMLCODE ? selfCampaign.HTMLContent() : selfCampaign.Designer().ViewableHTML())
            + '</td></tr></tbody></table></center></td></tr></tbody></table>'
            + '<table><tbody><tr><td><img alt="." src="https://pixel.monitor1.returnpath.net/pixel.gif?r=48e472570d0d8fa0e3254a6284ce1971302f56cd&c=*|CAMPID|*" width="1" height="1" />'
            + '<img alt="." src="https://pixel.app.returnpath.net/pixel.gif?r=48e472570d0d8fa0e3254a6284ce1971302f56cd" width="1" height="1" />'
            + '<img src="' + imageSource + '" width="1" height="1" alt="ptx"/>'
              + '<div id="campaignstyles" style="display:none !important;">'
                                + '<span id="outerbackground" style="background-color:' + selfDesigner.StyleSheet().BackGround.OuterBackGround() + '"></span>'
                                + '<span id="innerbackground" style="background-color:' + selfDesigner.StyleSheet().BackGround.InnerBackGround() + '"></span>'
                                + '<span id="bordercolor" style="border-color:' + selfDesigner.StyleSheet().Border.BorderColor() + '"></span>'
                                + '<span id="borderstyle" style="border-style:' + selfDesigner.StyleSheet().Border.BorderStyle() + '"></span>'
                                + '<span id="borderthickness" style="border-width:' + selfDesigner.StyleSheet().Border.BorderThickness() + '"></span>'
                             + '</div>'
            + '</td></tr></tbody></table></table>');
        //Returns html for test email
        if (isTestEmail)
            return htmlContent;
        else
            selfCampaign.HTMLContent(htmlContent);
    }

    if (selfCampaign.CampaignTypeId() == campaignType.PLAINTEXT) {
        $("#DesignControls").removeClass('active');
        $("#MergeFields").addClass('active');
    }
    if (selfCampaign.CampaignTypeId() == campaignType.HTMLCODE) {
        $("#DesignControls").removeClass('active');
        $("#contentdesign").removeClass('active');
        $("#MergeFields").addClass('active');
    }

    $("#camp-sched").click(function () {
        $("#sch-cam-ontm").removeClass('hide');
        $("#camp-sc").addClass('hide');
    });

    $("#cancelCampaignSchedule").click(function () {
        selfCampaign.ScheduleTime();
        if (selfCampaign.CampaignStatus() == "102" || selfCampaign.CampaignStatus("106"))
            selfCampaign.CampaignStatus("101");
        $("#sch-cam-ontm").addClass('hide');
        $("#camp-sc").removeClass('hide');
        $("#can-camp").removeClass('hide');
    });

    if (selfCampaign.ScheduleTime() != null && selfCampaign.CampaignMode() == "edit" && selfCampaign.CampaignStatus() == "102") {
        $("#camp-sc").addClass('hide');
        $("#sch-cam-ontm").removeClass('hide');
    }

    selfCampaign.CampaignStatus.subscribe(function (status) {
        if (selfCampaign.CampaignStatus() == "106")
            $("#cmpsch-tm").addClass('hide');
        else
            $("#cmpsch-tm").removeClass('hide');
    });

    //function RenderingImageWidth() {
    //    var images = $('#canvas').find('img').map(function () { return this; }).get();

    //    if (images.length > 0) {
    //        $.each(images, function (ind, val) {
    //            var style = $(val).attr("style");
    //            var acWidth = style.split('width');
    //            var acheight = style.split('height');
    //            var nwWidth = acWidth[2].replace(/[^0-9]/g, '');
    //            var nwheight = acheight[1].replace(/[^0-9]/g, '');
    //            console.log()
    //            console.log(nwWidth);
    //            console.log(nwheight)
    //            $(val).attr("width", nwWidth);
    //            $(val).attr("height", nwheight);
    //            $(val).attr('style', 'max-width: 100%; height: 100%; width: 100%;');
    //            console.log(val);
    //        })
    //    }

    //}

    selfCampaign.saveCampaign = function (status, isFrom) {
        if (status == 107 || selfCampaign.CampaignStatus() == "107")
            selfCampaign.CampaignStatus("107");
        else
            selfCampaign.CampaignStatus("101");

        if (status == 107 && selfCampaign.Subject() == null && selfCampaign.Subject() == undefined) {
            selfCampaign.CampaignStatus("101");
            notifyError('[|Subject cannot be empty|]');
            return true;
        }

        var currentControls = selfCampaign.Designer().Controls();
        selfDesigner.CaptureAllEdits();
        selfDesigner.updateControls()
        if (selfCampaign.CampaignType() == campaignType.HTMLCODE)
            selfCampaign.HTMLContent(selfCampaign.Designer().codeMirror.getValue())
        selfCampaign.Links(selfCampaign.Designer().Links);
        pageLoader();
        //Generate links for automation campaigns
        if (selfCampaign.CampaignStatus() == campaignStatus.DRAFT || (selfCampaign.IsLinkedToWorkflows() && selfCampaign.CampaignStatus() == campaignStatus.ACTIVE)) {
            selfCampaign.generateTrackableURLs();
        }

        if (selfCampaign.CampaignType() != campaignType.PLAINTEXT)
            selfCampaign.generateTrackableHTML();
        var type = 'post';
        if (selfCampaign.CampaignId() > 0)
            type = 'put';
        var jsondata = ko.toJSON(selfCampaign);

        var authToken = readCookie("accessToken");
        $.ajax({
            url: service + '/Campaign',
            type: type,
            data: jsondata,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + authToken);
            },
            success: function () {
                notifySuccess('[|Successfully saved the Campaign|]');

                if (isFrom == true) {
                    var authToken = readCookie("accessToken");
                    $.ajax({
                        url: service + '/mailtester/' + selfCampaign.CampaignId(),
                        type: 'post',
                        dataType: 'json',
                        contentType: 'application/json; charset=utf-8',
                        beforeSend: function (xhr) {
                            xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                        },
                        success: function (data) {
                            setTimeout(function () {
                                removepageloader();
                                if (selfCampaign.IsWorkflowID() == 'True')
                                    window.location.href = wfRedirect;
                                else if (!selfCampaign.RedirectToGrid())
                                    window.location.href = "/editcampaign?campaignId=" + selfCampaign.CampaignId();
                                else
                                    window.location.href = "/campaigns";

                            }, setTimeOutTimer);

                        },
                        error: function (response) {
                        }

                    })
                } else {
                    setTimeout(function () {
                        removepageloader();
                        if (selfCampaign.IsWorkflowID() == 'True')
                            window.location.href = wfRedirect;
                        else if (!selfCampaign.RedirectToGrid())
                            window.location.href = "/editcampaign?campaignId=" + selfCampaign.CampaignId();
                        else
                            window.location.href = "/campaigns";

                    }, setTimeOutTimer);
                }

              
            },
            error: function (response) {
                notifyError(response.responseText);
                setTimeout(function () {
                    removepageloader();
                    if (response.responseText == "The campaign cannot be saved   the deleted links are associated with automation workflow") {
                        alertifyReset();
                        alertify.confirm("[|Do you want to reload the page to get the original links?|]", function (e) {
                            if (e) {
                                location.reload();
                            }
                        });
                    }
                }, setTimeOutTimer);

                $("#performLitmusTest").attr('disabled', false);
                $("#performMailTester").attr('disabled', false);
                selfCampaign.Designer().Controls(currentControls);
                selfCampaign.Designer().Controls.valueHasMutated();
            }
        });

    }

    selfCampaign.sendCampaign = function (campaignstatus) {
       // RenderingImageWidth();

        if (campaignstatus == 106)
            selfCampaign.CampaignStatus('106');
        else if (campaignstatus == 102)
            selfCampaign.CampaignStatus('102');

        if (selfCampaign.TotalUniqueContacts() == 0) {
            notifyError('[|No recipients found|]');
            return;
        }
        selfCampaign.Designer().CaptureAllEdits();
        if (selfCampaign.CampaignTypeId() != campaignType.PLAINTEXT)
            selfCampaign.HTMLContent(selfCampaign.Designer().ViewableHTML().trim());

        selfCampaign.Links(selfCampaign.Designer().Links);
        if (selfCampaign.From() == "" || selfCampaign.From() == null) {
            notifyError("[|Sender Email is not configured for this Account.|]");
            return;
        }
        if (selfCampaign.validateCampaign())
            return;
        pageLoader();
        selfCampaign.generateTrackableURLs();
        if (selfCampaign.CampaignType() != campaignType.PLAINTEXT)
            selfCampaign.generateTrackableHTML();

        //var date = ConvertToDate(moment(selfCampaign.ScheduleTIme()).toDate());



        selfCampaign.ScheduleTimeUTC(selfCampaign.ScheduleTime());
        //selfCampaign.Sche
        var jsondata = ko.toJSON(selfCampaign);
        var currentControls = selfCampaign.Designer().Controls();
        var authToken = readCookie("accessToken");

        $.ajax({
            url: webApp + 'Queue',
            type: 'put',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'campaignViewModel': jsondata })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            notifySuccess('[|Campaign has been queued for sending. Please check the status in the Campaigns grid|]');
            setTimeout(function () {
            removepageloader();
             window.location.href = "/campaigns";
            }, setTimeOutTimer);
        }).fail(function (error) {
            removepageloader();
            notifyError(error);
            selfCampaign.Designer().Controls(currentControls);
            selfCampaign.Designer().Controls.valueHasMutated();
        });

    }

    selfCampaign.cancelCampaign = function () {
        alertify.set({ buttonReverse: true });
        if (selfCampaign.CampaignId() > 0 && (data.CampaignStatus == campaignStatus.SCHEDULED)) {
            alertify.confirm("[|You have unsaved changes in the Campaign. Changing the layout will result in your changes being lost. Would you like to continue with changes?|]", function (e) {
                if (e) {
                    selfCampaign.CampaignStatus(campaignStatus.CANCELED);
                    pageLoader();
                    var authToken = readCookie("accessToken");
                    $.ajax({
                        url: service + '/Campaign/Cancel',
                        type: 'get',
                        dataType: 'json',
                        contentType: "application/json; charset=utf-8",
                        beforeSend: function (xhr) {
                            xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                        },
                        data: { 'campaignId': selfCampaign.CampaignId() },
                        success: function () {

                            notifySuccess('[|Campaign has been cancelled|]');
                            removepageloader();
                            window.location.href = "/campaigns";
                        }
                    });

                } else {
                    window.location.href = "/campaigns";
                }
            });
        }
        else {
            alertifyReset();
            alertify.confirm("[|Are you sure you want to exit from this campaign without saving?|]", function (e) {
                if (e) {
                    window.location.href = "/campaigns";
                }
            });
        }
    };



    selfCampaign.saveCampaignAs = function () {
        if (selfCampaign.CampaignId() === 0) { return; }
        window.location.replace("/SaveCampaignAs?campaignId=" + selfCampaign.CampaignId());
    }
    selfCampaign.AddCampaignTemplate = function (data, event) {
        var menuItemId = $(event.target).attr('data-openid');
        //checkedvalues = fnGetChkvalGrid('chktag');
        removepageloader();


        if (selfCampaign.HTMLContent() == "" || selfCampaign.HTMLContent() == " ") {
            notifyError("[|Design one Campaign Layout.|]");
            return;
        }
        $("#menuPartialItemContent" + menuItemId).html('');
        OpenTopInner("menuItem" + menuItemId, "menuPartialItemContent" + menuItemId);

        $.ajax({
            url: webApp + "/_AddCampaignTemplate",
            type: 'get',
            contentType: "application/html; charset=utf-8",
            success: function (data) {

                $("#menuPartialItemContent" + menuItemId).html(data);
                selfCampaign.Designer().CaptureAllEdits();
                selfCampaign.HTMLContent(selfCampaign.Designer().ViewableHTML().trim());
                campaignTemplateViewModel = new campaignTemplateLayout(selfCampaign.HTMLContent(), webApp, menuItemId);
                ko.applyBindings(campaignTemplateViewModel, document.getElementById('SaveAsCampaignTemplate'));
            },
            error: function () {
            }
        });
    }
    selfCampaign.GetTemplate = function () {
        var authToken = readCookie("accessToken");
        $.ajax({
            url: service + '/CampaignTemplate',
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + authToken);
            },
            data: { 'campaignTemplateID': selfCampaign.Template().TemplateId },
            success: function (data) {
                selfCampaign.Template(data.CampaignTemplateViewModel);
            },
            error: function (response) {
            }
        });
    }

    selfCampaign.TestEmailText = ko.observable('[|Send|]');

    if (selfCampaign.IsLitmusTestPerformed()) {
        $("#performLitmusTest").attr('disabled', true);
        $("#re-run-tst").attr('disabled', true);
    }
    else {
        $("#performLitmusTest").attr('disabled', false);
        $("#re-run-tst").attr('disabled', false);

    }


    selfCampaign.redirectToLitmusResults = function () {
        window.location.href = "campaigns/litmusresults/" + selfCampaign.CampaignId();
    }
    selfCampaign.openLitmusModal = function () {
        if (litmusAPIKey == null || litmusAPIKey == "" || litmusAPIKey == undefined) {
            notifyError("[|Please configure Litmus API Key in AccountSettings.|]");
            return;
        }

        if (selfCampaign.LitmusGuid()) {
            selfCampaign.RedirectToGrid(false)
            $("#litmus-results").modal('toggle');
        }
        else
            $("#litmusTest").modal('toggle');

    }

    selfCampaign.openMailTesterModal = function () {
        $("#mailTesterTest").modal('toggle');
    }

    selfCampaign.afterLitmusTest = function () {
        $("#performLitmusTest").attr('disabled', true);
        $("#re-run-tst").attr('disabled', true);

        $("#litmusTest").modal('toggle');

        if (!selfCampaign.RedirectToGrid())
            $("#litmus-results").modal('toggle');

        if (IsReplicatedCM == "True")
            selfCampaign.PerformLitmusTest(true);

        selfCampaign.saveCampaign(101,false);

        var authToken = readCookie("accessToken");
        if (selfCampaign.CampaignId() > 1) {
            $.ajax({
                url: service + '/requestlitmuscheck/' + selfCampaign.CampaignId(),
                type: 'post',
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                },
                success: function (data) {

                },
                error: function (response) {
                }

            })
        } 
    }

    selfCampaign.performMailTester = function () {
        $("#performMailTester").attr('disabled', true);
        $("#mailTesterTest").modal('toggle');

        if (IsReplicatedCM == "True")
            selfCampaign.PerFormMailTester(true);

        selfCampaign.saveCampaign(101,true);

   
    }

    selfCampaign.ReRunLitmusTest = function () {
        $("#litmusTest").modal('toggle');
    }

    selfCampaign.sendTestMail = function () {

       //RenderingImageWidth();

        selfCampaign.Designer().CaptureAllEdits();
        if (!selfCampaign.ToAddress()) {
            notifyError("[|Please enter email|]");
            return;
        }


        if (selfCampaign.CampaignType() == campaignType.REGULAR && selfCampaign.Designer().Controls().length == 0) {

            notifyError("[|Cannot send empty email|]");
            return;
        }
        var emails = selfCampaign.ToAddress().toString();
        var emailIds = emails.split(",");
        var invalidEmail = false;
        var pattern = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;

        $.each(emailIds, function (index, str) {
            var email = str.trim();
            if (emailIds.length > 0) {

                if (!pattern.test(email))
                    return invalidEmail = true;
            }
            else {
                if (!pattern.test(email))
                    return invalidEmail = true;
            }
        });

        if (invalidEmail) {
            if (emailIds.length > 0)
                notifyError("One of the email is invalid.");
            else
                notifyError("Invalid Email.");
        }
        var content = selfCampaign.CampaignType() != campaignType.PLAINTEXT ? selfCampaign.generateTrackableHTML(true) : selfCampaign.HTMLContent();
        selfMail =
           {
               "To": selfCampaign.ToAddress(),
               "From": selfCampaign.From(),
               "Subject": (selfCampaign.Subject() == null || selfCampaign.Subject() == "") ? selfCampaign.Name() : selfCampaign.Subject(),
               "Body": content,
               "ProviderID": selfCampaign.ProviderId(),
               "SenderName": selfCampaign.SenderName(),
               "AccountPrivacyPolicy": selfCampaign.AccountPrivacyPolicy(),
               "CampaignTypeId": selfCampaign.CampaignType()
           }
        //var returnResult = [];
        var jsondata = ko.toJSON(selfMail);
        var authToken = readCookie("accessToken");
        console.log(selfCampaign.Subject());
        console.log(selfCampaign.Name())
        if (!invalidEmail) {
            selfCampaign.TestEmailText('[|Sending...|]');
            pageLoader();
            $.ajax({
                url: "/Campaign/SendTestEmail",
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'sendMailViewModel': jsondata, 'serviceProviderId': selfCampaign.ServiceProviderID(), 'hasDisCliamer': selfCampaign.HasDisclaimer() }),
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                },

            }).then(function (response) {
                removepageloader();
                $('#testCampaign').click();
                var filter = $.Deferred();
                if (response.success) {
                    filter.resolve(response);
                }
                else {
                    filter.reject(response.error);
                }
                return filter.promise();
            }).done(function (data) {
                if (data.success === true) { notifySuccess("Test email sent."); }
                selfCampaign.TestEmailText('[|Send|]');
                selfCampaign.ToAddress('');
                selfDesigner.convertToWidgets();
            }).fail(function () {
                notifyError("[|Test email failed.|]");
                selfCampaign.TestEmailText('[|Send|]');
                selfCampaign.ToAddress('');
                selfDesigner.convertToWidgets();
            });
        }
    }

    selfCampaign.cancelTestCampaign = function () {
        selfCampaign.ToAddress('');
    }

    // send Email to SeedList
    selfCampaign.SeedListEmailText = ko.observable('[|Send Now|]');
    selfCampaign.sendToSeedList = function () {
        if (selfCampaign.Designer().Controls().length == 0) {
            notifyError("[|Cannot send empty email|]");
            return;
        }
        if (selfCampaign.From() == "" || selfCampaign.From() == null) {
            notifyError("[|Sender Email is not configured for this Account.|]");
            return;
        }
        if (selfCampaign.Subject() == null || selfCampaign.Subject() == "") {
            notifyError("[|Campaign subject cannot be empty|]");
            return;
        }
        var Seedcontent = selfCampaign.HTMLContent();
        Seedcontent = selfCampaign.CampaignType() != campaignType.PLAINTEXT ? selfCampaign.generateTrackableHTML(true) : selfCampaign.HTMLContent();

        var SeedEmails = [];
        $.ajax({
            url: "/SeedList/GetList",
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                $.each(data.response, function (i, item) {
                    SeedEmails.push(item.Email);
                });
            },
            error: function (data) {
                notifyError(data.message);
            }
        }).then(function () {
            SendEmailToSeedList(Seedcontent, SeedEmails);
            $('#seedListMsg').show();
        });
    }

    var SendEmailToSeedList = function (Seedcontent, SeedEmails) {
        if (SeedEmails != null) {
            var selfMail =
                {
                    "Bcc": SeedEmails.toString(),
                    "From": selfCampaign.From(),
                    "Subject": selfCampaign.Subject(),
                    "Body": Seedcontent,
                    "ProviderID": selfCampaign.ProviderId(),
                    "SenderName": selfCampaign.SenderName(),
                    "AccountPrivacyPolicy": selfCampaign.AccountPrivacyPolicy()
                }
            // var returnResult = [];
            var jsondata = ko.toJSON(selfMail);
            var authToken = readCookie("accessToken");
            selfCampaign.SeedListEmailText('[|Sending...|]');
            $.ajax({
                url: "/Campaign/SendTestEmail",
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'sendMailViewModel': jsondata, 'serviceProviderId': selfCampaign.ServiceProviderID(), 'hasDisCliamer': selfCampaign.HasDisclaimer() }),
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                }

            }).then(function (response) {
                $('#seedListMsg').hide();
                var filter = $.Deferred();
                if (response.success) {
                    filter.resolve(response);
                }
                else {
                    filter.reject(response.error);
                }
                return filter.promise();
            }).done(function (data) {
                if (data.success === true) { notifySuccess("Email sent to SeedList."); }
                selfCampaign.SeedListEmailText('[|Send To SeedList|]');
            }).fail(function () {
                notifyError("[|Sending email to SeedList failed.|]");
                selfCampaign.SeedListEmailText('[|Send To SeedList|]');
            });
        }
        else
            notifyError("[|SeedList is Empty Please Contact smarttouch Administrator.|]");
    }

    ko.bindingHandlers.codemirror = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            //valueaccess = valueAccessor;
            $.extend(valueAccessor(), {
                onChange: function (cm) {
                    allBindingsAccessor().value(cm.getValue());
                }
            });
            var codeEditor = CodeMirror.fromTextArea(element, valueAccessor());
            element.editor = codeEditor;
            var innerHtml = document.createElement('div');
            innerHtml.innerHTML = valueAccessor().value();
            codeEditor.setValue(innerHtml.firstChild.innerHTML());
            codeEditor.refresh();
        }

    };


    selfCampaign.changeToTagStatus = function (data, event) {
        selfCampaign.ToTagStatus(event.currentTarget.value)
        var totalRecipientsCount = $(".tol-rpt-cnt").text();
        selfCampaign.TotalRecipientsCount(totalRecipientsCount);
    }

    selfCampaign.changeSsStatus = function (data, event) {
        selfCampaign.SSContactsStatus(event.currentTarget.value)
        var totalRecipientsCount = $(".tol-rpt-cnt").text();
        selfCampaign.TotalRecipientsCount(totalRecipientsCount);
    }

    selfCampaign.changeAutomationStatus = function (data, event) {
        selfCampaign.IsLinkedToWorkflows(event.target.id == '1' ? false : true);
    }

    selfCampaign.editCampaignName = function () {
        $('#campaignnewname').val(selfCampaign.Name());
        $('#editCampaignName').show();
    }

    selfCampaign.saveCampaignName = function () {
        var newName = $('#campaignnewname').val();
        if (newName) {
            selfCampaign.Name(newName);
            $('#editCampaignName').hide();
        }
        else
            notifyError("Campaign name cannot be empty");
    }
    selfCampaign.cancelCampaignNameEdit = function () {
        $('#editCampaignName').hide();
    }

    var subContainerHeight = function () {
        var windowHeight = $(window).outerHeight();
        var headerHeight = $('header').outerHeight();
        var footerHeight = $('.steps-new').outerHeight();
        var padding = 20 + 20;
        var computedDesignerHeight = windowHeight - (headerHeight + footerHeight + padding);
        return computedDesignerHeight;
    }

    var designerHeight = function () {
        var breadcrumbHeight = $('.campaigns').outerHeight() + 10;
        var previewBarHeight = $('#campaign-tabs').outerHeight();
        var subContainerHeight = $('.sub-container.st-subcampaign.mbxxl').outerHeight();
        return subContainerHeight - (breadcrumbHeight + previewBarHeight + 70 + 20); // 65 - .mbxxl bottom-margin + 20 padding
    }

    var imageGalleryHeight = function () {
        var uploaderHeight = $('.k-widget.k-upload.k-header').outerHeight();
        return designerHeight() - uploaderHeight - 60;
    }

    var adjustWindows = function () {
        setTimeout(function () {
            $('.sub-container.st-subcampaign.mbxxl').css({ 'max-height': subContainerHeight(), 'min-height': subContainerHeight() - 40 });
            $('#canvas').css({ 'max-height': designerHeight() });
            $('.st-widgets-edit-controls').css({ 'max-height': designerHeight() - 5, 'overflow-y': 'auto' });
            $('#campaignsdesignarea').css({ 'min-height': designerHeight() - 40 });
            $('#contentimageselements').css({ 'min-height': imageGalleryHeight() - 10, 'max-height': imageGalleryHeight() });
        }, 3000)
    }

    $(window).resize(function () {
        adjustWindows();
    });

    adjustWindows();

    if (selfCampaign.CampaignTypeId() == campaignType.PLAINTEXT) {
        $('#menuItem302').remove();
        $('#menuItem297').remove();
    }

    function extractContent(html) {
        pageLoader();
        var tempElement = document.createElement("div");
        tempElement.innerHTML = selfCampaign.HTMLContent();
        $(tempElement).find("style").remove();
        var encodedContent = encodeURI($(tempElement).text());
        while (encodedContent.indexOf("%0A%20") >= 0
            || encodedContent.indexOf("%0A%0A") >= 0
            || encodedContent.indexOf("%0A%09") >= 0
            || encodedContent.indexOf("%0A%22") >= 0
            || encodedContent.indexOf("%C2%A0%0A") >= 0
            || encodedContent.indexOf("%09%09") >= 0
            || encodedContent.indexOf("%20%20") >= 0
            || encodedContent.indexOf("%22%22") >= 0) {
            encodedContent = encodedContent.replace(/%0A%09|%0A%20|%0A%22|%C2%A0%0A|%0A%0A|%20%20|%09%09|%22%22/g, "%0A").trim("%0A");
        }
        removepageloader();
        return decodeURI(encodedContent);
    };

    selfCampaign.generatePlainTextVersion = function (prompt) {
        if (prompt && selfCampaign.PlainTextContent()) {
            alertify.set({ buttonReverse: true });
            alertify.confirm("[|Previous changes made will be lost. Do you want to continue?|]",
                function (e) {
                    if (e) {
                        selfCampaign.PlainTextContent(extractContent(selfCampaign.HTMLContent()));
                    } else {
                        notifyError("[|You've clicked Cancel|]");
                    }
                });
        }
        else
            selfCampaign.PlainTextContent(extractContent(selfCampaign.HTMLContent()));
    }

    $('#plaintexteditor').blur(function () {
        selfDesigner.PlainText($('#plaintexteditor')[0].innerText);
    })
};

///Campaign Designer
var campaignDesigner = function (ContactFields, Customfields, imageDomain, htmlContent, CmpType) {

    selfDesigner = this;
    applicationDomain1 = location.host;

    ko.validatedObservable(selfCampaign);

    ko.validation.rules['cannotEqual'] = {
        validator: function (controls, length) {
            if (selfCampaign.CampaignTypeId() != campaignType.PLAINTEXT && (selfCampaign.CampaignStatus() == campaignStatus.QUEUED || selfCampaign.CampaignStatus() == campaignStatus.SCHEDULED))
                return controls.length != length;
            else
                return true;
        },
        message: '[|Campaign body cannot be empty|]'
    };

    ko.validation.registerExtenders();

    selfDesigner.addRow = function (event, ui) {
        currentHTML = $(this.EditedHTML()).find('.w3-table-all');
        tableId = $(this.EditedHTML()).find('.w3-table-all').attr('id');
        tdStyle = $("#canvas #" + tableId + " tbody tr td").attr('style');
        $('#canvas #' + tableId).append($("#canvas #" + tableId + " tbody tr:last").clone());
        $("#canvas #" + tableId + " tbody tr:last").find('td').each(function () { $(this)[0].innerText = "" });

    }

    selfDesigner.addColumn = function () {
        currentHTML = $(this.EditedHTML()).find('.w3-table-all');
        tableId = $(this.EditedHTML()).find('.w3-table-all').attr('id');
        tdWidth = $("#canvas #" + tableId + " tbody tr:last td:first").width();

        clonedTh = $("#canvas #" + tableId + " tbody th:last").clone();
        clonedTd = $("#canvas #" + tableId + " tbody tr:last td:last").clone();

        clonedTd[0].innerText = "";
        clonedTh[0].innerText = "";

        clonedTd.width = tdWidth;
        clonedTh.width = tdWidth;

        //$('#canvas #' + tableId + " tr:first-child").append(clonedTh);
        $('#canvas #' + tableId + " tr:not(:first-child)").append(clonedTd);
        //$('#canvas #' + tableId + " th:last").width = tdWidth;
        //$("#canvas #" + tableId).find('td').each(function () { $(this)[0].css(tdStyle)});
    }

    selfDesigner.Controls = ko.observableArray();

    selfDesigner.Controls.subscribe(function () {
        selfDesigner.convertToWidgets();
    });

    selfDesigner.campaignMaxWidth = ko.observable(600);

    selfDesigner.updateControl = function (data, event) {
        actualControl = data;
        updatedControl = $("#designerControls").find("#control-content-" + data.Id());
    };

    selfDesigner.showDevices = ko.observable($("#campaign-tabs>.btn-lg.active").attr('name'));

    selfDesigner.controlsValidation = selfDesigner.Controls.extend({ cannotEqual: 0 });

    selfDesigner.Links = ko.observableArray();

    selfDesigner.EditableHTML = ko.observable();
    selfDesigner.IsFullEdit = ko.observable(false);

    selfDesigner.StyleSheet = ko.observable(new campaignStyleSheet(htmlContent));
    selfDesigner.CampaignType = ko.observable(CmpType);

    selfDesigner.applyStyles = function () {

        alertify.confirm("[|Are you sure you want to apply these styles|]", function (e) {
            var authToken = readCookie("accessToken");
            if (e) {
                selfDesigner.StyleSheet.valueHasMutated();
            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }

        });
    }
    selfDesigner.controlBeingAdded = ko.observable();
    selfDesigner.removeCancelledTableWidget = ko.observable(false);
    selfDesigner.AddControl = function (controlTemplateSourceId, pushAtIndex, isNew) {
        var controlId = selfDesigner.Controls().length;
        var control = new controlTemplate(controlId, controlTemplateSourceId);
        if (controlTemplateSourceId == 'control-table')
            control.IsTableWidget(true);
        if (pushAtIndex >= 0 && pushAtIndex < selfDesigner.Controls().length && !isNew) {
            control = new controlTemplate(controlId, 'fromLayout');
            newElement = document.createElement('div');
            newElement.innerHTML = selfDesigner.Controls()[pushAtIndex].EditedHTML();

            $(newElement).find('img').each(function () {
                $(this)[0].id = selfDesigner.generateUUID();
            });
            control.EditedHTML(newElement.innerHTML);
        }
        var htmlWithNewIds = document.createElement('div');
        htmlWithNewIds.innerHTML = control.EditedHTML();
        $(htmlWithNewIds).find('table,td').each(function (index, td) {
            $(td).attr({ 'id': selfDesigner.generateUUID() });
        });

        control.EditedHTML(htmlWithNewIds.innerHTML);
        selfDesigner.Controls().splice(pushAtIndex, 0, control);
        selfDesigner.Controls.valueHasMutated();
        selfDesigner.CaptureAllEdits();
        if (control.IsTableWidget()) {
            selfDesigner.controlBeingAdded = ko.observable('control-table');
            setTimeout(function () {
                var editableElement = $(selfDesigner.Controls()[pushAtIndex < -1 ? 0 : pushAtIndex].EditedHTML()).find('td').first();
                selfDesigner.controlBeingEdited.id = $('#li-' + control.Id()).find('.table-widget td:first').attr('id');
                selfDesigner.EditControl(editableElement, true);
                setTimeout(function () {
                    $('.redactor-toolbar .re-icon.re-table').trigger('click');
                }, 2);
                setTimeout(function () {
                    $('.redactor-dropdown-insert_table').trigger('click')
                    $('.redactor-modal-btn.redactor-modal-close-btn').click(function () { })
                }, 2);
            }, 00);
        }
    }

    selfDesigner.generateUUID = function () {
        var d = new Date().getTime();
        var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = (d + Math.random() * 16) % 16 | 0;
            d = Math.floor(d / 16);
            return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
        });
        return uuid;
    };

    selfDesigner.domainName = location.protocol + "//" + location.host;

    selfDesigner.ViewableHTML = ko.computed({
        read: function () {

            $(".k-br").remove();
            var viewableHtml = "";//'<div style="margin:0 auto; font-family:Calibri; max-width:800px;" id="myown">';
            ko.utils.arrayForEach(selfDesigner.Controls(), function (item) {
                var currentControl = document.createElement("div");
                currentControl.innerHTML = item.EditedHTML();
                currentHtml = $(currentControl).removeAttr("data-bind").removeAttr("contenteditable");
                if (currentHtml.length > 0) {
                    viewableHtml = viewableHtml + currentHtml[0].innerHTML;
                }
            });

            setTimeout(function () {
                $('#unsubScribe').remove();
            }, 500);
            viewableHtml = viewableHtml; // + "</div>";
            selfDesigner.campaignMaxWidth(selfCampaign.getMaxWidth(viewableHtml));
            return viewableHtml.toString();
        },
        write: function (newValue) {
        }
    });

    selfDesigner.ViewableHTML.subscribe(function (e) {
        selfCampaign.HTMLContent(selfDesigner.ViewableHTML().trim());
        $(".k-br").remove();
    });

    selfDesigner.ShowDesign = function (data, event) {
        if (selfCampaign.CurrentView() == 'htmlview')
            selfCampaign.HTMLContent(selfDesigner.codeMirror.getValue());
        selfDesigner.CaptureAllEdits();
        selfDesigner.showDevices(false);
        OpenCPview(event.target);
        selfDesigner.updateControls();
        setTimeout(function () {
            selfCampaign.applyUI();
        }, 500);
        $('.campaigns-droparea .campaigns-body').removeClass('tablet mobile desktop');
        $('.campaigns-droparea .campaigns-body').addClass('desktop');
        $('.campaigns-theme-controls').removeClass('hide');
        selfCampaign.CurrentView('designer');
        applyWidgetOutline();
    }

    selfDesigner.ShowHTML = function (data, event) {
        $(".k-br").remove();
        selfCampaign.HTMLContent().replace("\n", "");

        selfCampaign.HTMLContent().replace(">", ">\n");
        selfDesigner.CaptureAllEdits();
        OpenCPview(event.target);
        selfDesigner.showDevices(false);
        var html = cleanHTML(selfCampaign.HTMLContent())
        selfDesigner.codeMirror.setValue(html);
        setTimeout(function () {
            selfDesigner.codeMirror.refresh();
        }, 1);
        selfCampaign.CurrentView('htmlview');
        $('.campaigns-theme-controls').addClass('hide');
    }

    selfDesigner.Preview = function (data, event) {
        if (selfCampaign.CurrentView() == 'htmlview')
            selfCampaign.HTMLContent(selfDesigner.codeMirror.getValue());
        selfDesigner.showDevices(true);
        selfDesigner.CaptureAllEdits();
        OpenCPview(event.target);
        selfDesigner.updateControls();
        selfCampaign.CurrentView('previewDesign');
        selfDesigner.isDesktopMode(true);
        $('.campaigns-droparea .campaigns-body').removeClass('tablet mobile desktop');
        $('.campaigns-droparea .campaigns-body').addClass('desktop');
        $('.campaigns-theme-controls').addClass('hide');
    }
    selfDesigner.CaptureAllEdits = function () {
        selfDesigner.removeWrappers();
        selfDesigner.setImageDimensions();
        $('#unsubScribe').remove();
        if (selfCampaign.CampaignTypeId() == campaignType.REGULAR && selfDesigner.IsFullEdit() == false) {
            ko.utils.arrayForEach(selfDesigner.Controls(), function (item) {
                var htmlToShow = $("#control-content-" + item.Id());
                htmlToShow.find("*").removeAttr("data-bind").removeAttr("contenteditable");
                item.EditedHTML(htmlToShow.html());
                item.TemplateSourceId("fromLayout");
            });
        }
        else if (selfCampaign.CampaignTypeId() == campaignType.HTMLCODE) {
            selfCampaign.HTMLContent(selfDesigner.codeMirror.getValue());
        }
        else if (selfCampaign.CampaignTypeId() == campaignType.PLAINTEXT) {
            selfCampaign.HTMLContent(selfCampaign.Designer().PlainText());
        }
        applyWidgetOutline();
        $(".innerWrapper").remove();
    }

    selfDesigner.CopyControl = function (index, data, event) {
        selfDesigner.AddControl("control-content-" + data.Id(), index, false);
        bindImageGallery();
    }

    selfDesigner.getEditedHTML = function (event, data) {
        selfDesigner.Controls()[data.Id()].EditedHTML($("#control-content-" + data.Id()).html());
    }

    selfDesigner.DeleteControl = function (data, event) {
        var control = this;
        alertify.set({ buttonReverse: true });
        alertify.confirm("[|Are you sure you want to delete this widget?|]", function (e) {
            if (e) {
                selfDesigner.Controls.remove(control);
                bindImageGallery();
            }
        })
    }

    selfDesigner.isHTMLEditable = ko.observable(true);

    selfDesigner.editHtml = function (data, event) {
        selfCampaign.HTMLContent(selfDesigner.ViewableHTML().trim());
    };

    selfDesigner.updateControls = function (data, event) {
        setTimeout(function () {
            currentHTML = $.parseHTML(selfCampaign.HTMLContent());
            selfCampaign.Designer().Controls([]);
            var count = 0;

            if (currentHTML != null) {
                $.each(currentHTML, function (index, value) {
                    var newTemplate = new controlTemplate(count++, 'fromLayout');
                    newTemplate.EditedHTML($(this).wrap('<p>').parent().html());
                    newTemplate.IsStyleTag($(this).find('style').length == 0 ? false : true);
                    if (newTemplate.EditedHTML().trim() != "")
                        selfCampaign.Designer().Controls().push(newTemplate);
                });
            }
            selfCampaign.Designer().Controls.valueHasMutated();
            selfCampaign.applyUI();

        }, 500);
    };

    selfDesigner.errors = ko.validation.group(selfDesigner);

    selfDesigner.applyOuterBgColor = function (e) {
        selfDesigner.StyleSheet().BackGround.OuterBackGround(e.value);
    }

    selfDesigner.applyInnerBgColor = function (e) {
        selfDesigner.StyleSheet().BackGround.InnerBackGround(e.value);
    }

    selfDesigner.applyBorderColor = function (e) {
        selfDesigner.StyleSheet().Border.BorderColor(e.value);
    }

    selfDesigner.applyHeadingColor = function (e) {
        selfDesigner.StyleSheet().Headers()[selfDesigner.StyleSheet().SelectedHeader()].HeadingColor(e.value);
    }

    selfDesigner.applyFontColor = function (e) {
        selfDesigner.StyleSheet().Headers()[selfDesigner.StyleSheet().SelectedHeader()].FontColor(e.value);
    }

    selfDesigner.searchQuery = ko.observable('');

    selfDesigner.Fields = ko.observableArray(ContactFields);

    selfDesigner.CustomFields = ko.observableArray(Customfields);

    selfDesigner.computedFieldSearch = ko.pureComputed(function () {
        return ko.utils.arrayFilter(selfDesigner.Fields(), function (item) {
            var filteredFields = (item.Title.toLowerCase().indexOf(selfDesigner.searchQuery().toLowerCase()) >= 0
                               || item.FieldCode.toLowerCase().indexOf(selfDesigner.searchQuery().toLowerCase()) >= 0);
            return filteredFields;
        });
    });

    selfDesigner.computedCustomFieldSearch = ko.pureComputed(function () {
        return ko.utils.arrayFilter(selfDesigner.CustomFields(), function (item) {
            var filteredFields = (item.Title.toLowerCase().indexOf(selfDesigner.searchQuery().toLowerCase()) >= 0
                                  || item.FieldCode.toLowerCase().indexOf(selfDesigner.searchQuery().toLowerCase()) >= 0);
            return filteredFields;

        });
    });

    selfDesigner.IncludeField = function (field, el) {
        if (selfDesigner.IsFullEdit() == false) {
            if (selfDesigner.Controls().length == 0 && selfCampaign.CampaignTypeId() == campaignType.REGULAR)
                return;
            else if (selfCampaign.CampaignTypeId() == campaignType.HTMLCODE) {
                selfDesigner.codeMirror.replaceSelection("*|" + field.FieldCode + "|*");
                return;
            }
            container = selfCampaign.CampaignTypeId() == campaignType.REGULAR ? 'controlbeingedited' : selfCampaign.CampaignTypeId() == campaignType.HTMLCODE ? 'campaigntemplatehtml' : 'plaintextcampaign';
            var id = el.target.id;
            ele = el;
            $("#" + id).blur();
            var selected = window.getSelection;
            var selected23 = window.getSelection();
            var sel, range, html;
            if (window.getSelection) {
                sel = window.getSelection();
                if (sel.getRangeAt && sel.rangeCount) {
                    var addelement = elementContainsSelection(document.getElementById(container));
                    if (addelement == true) {
                        range = sel.getRangeAt(0);
                        range.deleteContents();
                        element = document.createElement('span');
                        element.id = field.FieldId;
                        if (field.IsDropdownField == true) {
                            element.setAttribute("fieldtype", "dropdownfield");
                        } else if (field.IsCustomField == true) {
                            element.setAttribute("fieldtype", "customfield");
                        } else {
                            element.setAttribute("fieldtype", "basicfield");
                        }
                        element.textContent = "*|" + field.FieldCode + "|*";
                        range.insertNode(element);
                        element.focus();
                    }
                }
            } else if (document.selection && document.selection.createRange) {
                addelement = elementContainsSelection(document.getElementById(container));

                if (addelement == true) {
                    element = document.createElement('span');
                    element.id = field.FieldId;
                    if (field.IsDropdownField == true) {
                        element.setAttribute("fieldtype", "dropdownfield");
                    } else if (field.IsCustomField == true) {
                        element.setAttribute("fieldtype", "customfield");
                    } else {
                        element.setAttribute("fieldtype", "basicfield");
                    }
                    element.textContent = "*|" + field.FieldCode + "|*";

                    document.selection.createRange().insertNode(element);
                    element.focus();
                }
            }
            if (selfCampaign.CampaignTypeId() == campaignType.PLAINTEXT) {
                selfDesigner.PlainText($('#plaintexteditor')[0].innerText);
            }
        }
        else {
            container = 'fulledithtml';
            var id = el.target.id;
            ele = el;
            $("#" + id).blur();
            var selected = window.getSelection;
            var selected23 = window.getSelection();
            var sel, range, html;
            if (window.getSelection) {
                console.log("1");

                sel = window.getSelection();
                if (sel.getRangeAt && sel.rangeCount) {
                    console.log("2");

                    var addelement = elementContainsSelection(document.getElementById(container));
                    if (addelement == true) {
                        console.log("3");

                        range = sel.getRangeAt(0);
                        range.deleteContents();
                        element = document.createElement('span');
                        element.id = field.FieldId;
                        if (field.IsDropdownField == true) {
                            element.setAttribute("fieldtype", "dropdownfield");
                        } else if (field.IsCustomField == true) {
                            element.setAttribute("fieldtype", "customfield");
                        } else {
                            element.setAttribute("fieldtype", "basicfield");
                        }
                        element.textContent = "*|" + field.FieldCode + "|*";
                        range.insertNode(element);
                        element.focus();
                    }
                }
            } else if (document.selection && document.selection.createRange) {
                addelement = elementContainsSelection(document.getElementById(container));
                if (addelement == true) {
                    element = document.createElement('span');
                    element.id = field.FieldId;
                    if (field.IsDropdownField == true) {
                        element.setAttribute("fieldtype", "dropdownfield");
                    } else if (field.IsCustomField == true) {
                        element.setAttribute("fieldtype", "customfield");
                    } else {
                        element.setAttribute("fieldtype", "basicfield");
                    }
                    element.textContent = "*|" + field.FieldCode + "|*";

                    document.selection.createRange().insertNode(element);
                    element.focus();
                }
            }
            if (selfCampaign.CampaignTypeId() == campaignType.PLAINTEXT) {
                selfDesigner.PlainText($('#plaintexteditor')[0].innerText);
            }
        }
        
    }

    function isOrContains(node, container) {
        while (node) {
            if (node === container) {
                return true;
            }
            node = node.parentNode;
        }
        return false;
    }

    function elementContainsSelection(el) {
        var sel;
        if (window.getSelection) {
            sel = window.getSelection();
            if (sel.rangeCount > 0) {
                for (var i = 0; i < sel.rangeCount; ++i) {
                    if (!isOrContains(sel.getRangeAt(i).commonAncestorContainer, el)) {
                        return false;
                    }
                }
                return true;
            }
        } else if ((sel = document.selection) && sel.type != "Control") {
            return isOrContains(sel.createRange().parentElement(), el);
        }
        return false;
    }

    function InititalizeLazyLoad() {
        $('body').on('click', 'a[rel="tab2"]', function () {
            $('img.lazy').lazyload({
                effect: "fadeIn",
                container: $('#redactor-image-manager-box'),
                skip_invisible: true
            });
            setTimeout(function () {
                $('#redactor-image-manager-box').scroll();
            }, 200);
        });
    }

    function applyWidgetOutline() {
        $('.st-droparea-controls').mouseenter(function () {
            $(this).closest('.st-layout').addClass('widget-outline');
        });
        $('.st-droparea-controls').mouseleave(function () {
            $(this).closest('.st-layout').removeClass('widget-outline');
        });
    }
    setTimeout(function () { applyWidgetOutline() }, 1000);

    selfDesigner.editMode = ko.observable(false);

    selfDesigner.editMode.subscribe(function () {
        //if (selfDesigner.editMode() == true) {
        //        $("#editanimate").animate({ left: '800px' });
        //}
        //else
        //    $("#editanimate").animate({ right: '800px' });

    });

    selfDesigner.removeWrappers = function () {
        var wrappers = $('#canvas .widget-editable');
        $.each(wrappers, function (index, value) {
            if ($(value).parent().hasClass('secondWrapper'))
                $(value).unwrap();
            if ($(value).parent().hasClass('firstWrapper'))
                $(value).unwrap();
        })
    }

    selfDesigner.convertToWidgets = function () {
        setTimeout(function () {
            selfDesigner.removeWrappers();
            var prepender = '<span class="fa fa-camera fa-3x"><span class="title">Image Title</span><span class="desc">Image description</span></span><span class="fa fa-times-circle fa-3x"></span>';
            var innerHtmlWrapper = '<div class="st-layout">' +
                                   '<div class="widget-controls">' +
                                       '<a href="#" class="editControl" ><i class="icon st-icon-edit"></i></a>' +
                                       '<a href="#" class="deleteControl" ><i class="icon st-icon-bin-3"></i></a>' +
                                   '</div>' +
                               '</div>'
            //$('#canvas td').each(function () {
            //    var tdWidth = $(this).attr('width');
            //    var tdHasSiblings = $(this).closest('tr').children('td').length>1;
            //    if (!tdWidth || !tdHasSiblings)
            //        $(this).attr({ 'width': '100%' });
            //})
            var firstWrapper = '<span class="firstWrapper" ></span>';
            var secondWrapper = '<span class="secondWrapper"></span>';

            $('#canvas td:not(".table-widget td"),#canvas .table-widget').each(function () {
                var container = this;
                var containerWidth = $(this).width();
                var hasChildTDs = $(container).find('td').length > 0;
                var isTableWidget = $(this).hasClass('table-widget');
                var hasContent = $(this).text().trim() != "" || $(this).find('*').length > 0;
                if ((!hasChildTDs && hasContent) || isTableWidget) {
                    $(container).addClass('widget-editable');
                    $(container).css({ 'word-break': 'break-word' });
                    $(container).attr('id', $(container).attr('id') || selfCampaign.Designer().generateUUID());
                    $(container).find(".innerWrapper").remove();
                    if (!$(container).parent().hasClass('secondWrapper'))
                        $(container).wrap(firstWrapper).wrap(secondWrapper);
                    if ($(container).find('.innerWrapper').length == 0) {
                        $(container).prepend("<span class='innerWrapper'></span>").find('.innerWrapper').wrapInner(innerHtmlWrapper);
                        $(container).find(".editControl").click(function () {
                            selfDesigner.IsFullEdit(false);
                            selfDesigner.EditControl(isTableWidget ? $(container).find('.editControl').closest('.table-widget') : $(container).find('.editControl').closest('td'), isTableWidget, container);

                        });
                        $(this).find(".deleteControl").click(function (event, ui) {
                            var widgetBeingDeleted = $(container).closest('td')
                            alertify.confirm("[|Are you sure you want to delete this widget block?|]", function (e) {
                                if (e) {
                                    widgetBeingDeleted.remove();
                                    selfDesigner.CaptureAllEdits();
                                    selfDesigner.convertToWidgets();
                                    bindImageGallery();
                                }
                            })
                        });
                    }
                }
            });
            $('.table-widget .deleteControl').remove();
        }, 500);
    };
    selfDesigner.controlBeingEdited = {
        'id': '',
        'content': '',
        'index': -1,
        'controlName': ''
    };

    var resetControlBeingAdded = function () {
        selfDesigner.controlBeingEdited = {
            'id': '',
            'content': '',
            'index': -1,
            'controlName': ''
        };
    }

    selfDesigner.editableContent = ko.observable();
    selfDesigner.editableContent.subscribe(function () {
        $('#' + selfDesigner.controlBeingEdited.id).html(selfDesigner.editableContent());
    });
    var startUpdate = function () { };
    var autoUpdateCanvas = function (action) {
        if (action == 'stop')
            clearInterval(startUpdate);
        else
            startUpdate = setInterval(function () {
                $('#canvas #' + selfDesigner.controlBeingEdited.id).html($('#controlbeingedited').redactor('code.get'));
                var lineNumbers = Math.round($('#canvas #campaignsdesignarea').height() / 15, 0);
                var scrollerText = ''
                for (i = 0; i <= lineNumbers; i++) {
                    scrollerText = scrollerText + '\n';
                }
                $('#controlbeingedited').find('#htmlcontent_R').removeAttr('id');
                $('.st-editor-scroller').text(scrollerText);
                $('.st-editor-scroller').css({ 'max-height': $('#canvas').height() + 30, 'height': $('#canvas #campaignsdesignarea').height(), 'width': $('#canvas').width(), 'left': -$('#canvas').width() });
            }, 1000);
    }
    selfDesigner.EditControl = function (control, isTableWidget, container) {
        autoUpdateCanvas('start');
        selfDesigner.controlBeingEdited.id = $(control).attr('id');
        selfDesigner.controlBeingEdited.index = $('#canvas #' + selfDesigner.controlBeingEdited.id).closest('[name="campaign-control"]').index();
        $('#redactor-modal-box, .redactor-dropdown.redactor-dropdown-box-table').remove();
        $(control).find('.innerWrapper').remove();
        var editableContent = "";
        editableContent = $("#canvas #" + selfDesigner.controlBeingEdited.id).html();
        selfDesigner.editableContent(editableContent);
        selfDesigner.controlBeingEdited.content = editableContent;
        selfCampaign.applyUI();
        selfDesigner.editMode(true);
        red = $('#controlbeingedited').redactor({
            plugins: ['bufferbuttons', 'fontcolor', 'fontsize', 'fontname', 'fontfamily', 'table', 'imagemanager'],
            buttons: ['html', 'formatting', 'bold', 'italic', 'underline', 'deleted', 'unorderedlist', 'orderedlist', 'outdent', 'indent', 'image', 'link', 'alignment', 'horizontalrule', 'html'],
            replaceDivs: false,
            focusend: true,
            paragraphize: false,
            //uploadImageField: true,
            imageManagerJson: "/Campaign/ImagesListView",
            imageUpload: "/Contact/UploadImage",
            imageUploadErrorCallback: function (json) {
                notifyError(json.error);
            },
            //imageUpload: "/Image/UploadImages",
            dragImageUpload: false,
        });
        $("#Widgetedit .redactor-box ul").first().append('<li><a href="javascript:void(0)" id="editorMergeFields" ><span class="redactor-mergefied-disabled"><i class="icon st-icon-converge"></i> [|Merge Fields|]</span></a></li>');
        $('#editorMergeFields').click(function () {
            $('#contactfieldsintoolbar').toggle();
        });
        $(".redactor-toolbar li a.re-html").click(function () {
            $(".redactor-toolbar li span.redactor-mergefied-disabled").toggle();
            $("#cmp-htm-hd").toggle();
            $("#cmp-htm-hd1").toggle();

        });

        $("#controleditor").resizable({
            handles: "w",
            maxWidth: 700,
            minWidth: 380
        });


        InititalizeLazyLoad();
        applyTableResizable();
        selfDesigner.editorHeight($('#canvas #campaignsdesignarea').height());

        (function () {
            var target = $("#canvas");
            $(".st-editor-scroller").scroll(function () {
                target.prop("scrollTop", this.scrollTop)
                      .prop("scrollLeft", this.scrollLeft);
            });
        })();
    };

    selfDesigner.saveControl = function (data, event) {
        pageLoader();
        autoUpdateCanvas('stop');
        $('#canvas img').each(function () {
            var image = this;
            if ($(image).css("width")) {
                var width= parseInt($(image).css("width"),10);
                var height = parseInt($(image).css("height"), 10);
                $(image).removeAttr('width');
                $(image).removeAttr('height');
                $(image).attr('width', width);
                if (height > 0)
                    $(image).attr('height', height);

            }
            if ($(image).parent().attr('id') == 'redactor-image-box') {
                $(image).unwrap();
            }
        })
        $('#redactor-image-resizer, #redactor-image-editter, #redactor-image-resizer').remove();
        $('#controlbeingedited, #canvas').find('.ui-resizable-handle').remove();
        $('#controlbeingedited').redactor('image.hideResize');
        var editedHTML = $('#controlbeingedited').html();
        $('#controlbeingedited').redactor('core.destroy');
        $('#canvas').find('*').removeClass('ui-resizable');
        $('#canvas #' + selfDesigner.controlBeingEdited.id + ' .innerWrapper').remove();
        removepageloader();
        selfDesigner.editMode(false);
        selfDesigner.CaptureAllEdits();
        selfDesigner.ViewableHTML();
        //selfDesigner.saveHtml();
        setTimeout(function () {
            bindImageGallery();
        }, 2000);
        St_Draggable('item');
        selfDesigner.convertToWidgets();
        $('.ui-resizable-handle.ui-resizable-s, .ui-resizable-handle.ui-resizable-s').remove();
        selfDesigner.removeCancelledTableWidget(false);
        selfCampaign.applyUI();

        selfDesigner.controlBeingAdded('');
    }

    selfDesigner.cancelEditing = function (data, event) {
        $('#canvas #' + selfDesigner.controlBeingEdited.id + ' .innerWrapper').remove();

        autoUpdateCanvas('stop');
        selfDesigner.editMode(false);
        $('#canvas #' + selfDesigner.controlBeingEdited.id).html(selfDesigner.controlBeingEdited.content);
        setTimeout(function () {
            bindImageGallery();
            St_Draggable('item');
        }, 2000);
        selfDesigner.convertToWidgets();
        $('#canvas').find('*').removeClass('ui-resizable');
        $('.ui-resizable-handle.ui-resizable-s, .ui-resizable-handle.ui-resizable-s').remove();

        if (selfDesigner.Controls()[selfDesigner.controlBeingEdited.index].IsTableWidget() == true && selfDesigner.controlBeingAdded() == 'control-table') {
            selfDesigner.Controls().splice(selfDesigner.controlBeingEdited.index, 1);
        }
        selfDesigner.CaptureAllEdits();
        selfDesigner.Controls.valueHasMutated();
        selfDesigner.controlBeingEdited = {
            'id': '',
            'content': '',
            'index': -1
        };
        selfDesigner.removeCancelledTableWidget(false);
        selfDesigner.controlBeingAdded('');
        selfCampaign.applyUI();

    }

    selfDesigner.setImageDimensions = function () {
        setTimeout(function () {
            $('#canvas img').each(function (index, image) {
                if ($(image).attr('data-idp') == 'undefined') {
                    imageHelper.dimensions().set(image);
                }
            });
        }, 1000);
    }
    selfDesigner.codeMirror = CodeMirror.fromTextArea(document.getElementById("editablehtml"), {
        lineNumbers: true,
        lineWrapping: false,
        scrollbarStyle: "simple",
        smartIndent: true,
        fixedGutter: false,
        noHScroll: true
    });

    var windowWidth = $(window).width();
    selfDesigner.codeMirror.setSize("100%", windowWidth < 1400 ? 500 : 700);
    htmlElement = $('<div>');
    htmlElement[0].innerHTML = selfCampaign.HTMLContent();
    htmlCode = htmlElement.find('td#htmlcontent_R');
    var html = cleanHTML(htmlCode.length > 0 ? htmlCode[0].innerHTML : "");

    selfDesigner.codeMirror.setValue(html);

    //selfDesigner.codeMirror.setOption("theme", 'monokai')

    selfDesigner.isDesktopMode = ko.observable(true);

    setTimeout(function () {
        selfDesigner.isDesktopMode(true);
    }, 4000);

    var codeFormatter = function () {
        var totalLines = selfDesigner.codeMirror.lineCount();
        var totalChars = selfDesigner.codeMirror.getTextArea().value.length;
    }

    codeFormatter();

    selfDesigner.PlainText = ko.observable(selfCampaign.HTMLContent());

    selfDesigner.PlainText.subscribe(function () {
        selfDesigner.PlainText(selfDesigner.PlainText().replace(/&nbsp;/gi, ' '));
        selfCampaign.HTMLContent(selfDesigner.PlainText().replace(/&nbsp;/gi, ' '));
    });

    selfDesigner.currentControlBox = ko.observable(selfCampaign.CampaignTypeId() == campaignType.REGULAR ? 'DesignControls' : 'MergeFields');

    selfDesigner.selectedImage = ko.observable('');

    selfDesigner.showContactFields = function () {
        $("#contactfields").toggle();
    }

    $('#plaintexteditor').blur(function () {
        selfDesigner.PlainText($('#plaintexteditor')[0].innerText);
    })

    selfDesigner.expandEditor = function (action) {
        if ($('#controleditor').hasClass('expanded-editor')) {
            $('#controleditor').removeClass('expanded-editor');
            $('#controleditor i').removeClass('glyphicon-chevron-right').addClass('glyphicon-chevron-left');
        }
        else {
            $('#controleditor').addClass('expanded-editor');
            $('#controleditor i').removeClass('glyphicon-chevron-left').addClass('glyphicon-chevron-right');
        }
        //$("#controleditor").switchClass("expanded-editor", "minimized-editor", 1000);
        //$("#controleditor").switchClass("minimized-editor", "expanded-editor", 1000);
        //$("#controleditor i").switchClass("glyphicon-chevron-left", "glyphicon-chevron-right", 1000);
        //$("#controleditor i").switchClass("glyphicon-chevron-right", "glyphicon-chevron-left", 1000);
    }

    selfDesigner.editorHeight = ko.observable(400);

    setTimeout(function () {
        $('.main-container').css({ 'min-height': 500 });
    }, 2000)

    selfDesigner.showPreview = function () {
        selfDesigner.CaptureAllEdits();
        selfDesigner.convertToWidgets();
    }

    var i = 0;
    selfDesigner.SaveEditableChanges = function () {
        if (i != 0) {
            $("#fulledit-View").modal('hide');
            $("#FEModal").modal('toggle');
        }
        i++;
    }

    selfDesigner.FEConfirm = function () {
        
        var html = $("#fulledithtml").redactor('code.get');
        if (i != 0) {
            $("#campaignsdesignarea").html(html);
            selfDesigner.IsFullEdit(true);
            selfCampaign.HTMLContent(html);
            selfCampaign.CurrentView(selfCampaign.CampaignTypeId() == 133 ? 'htmlview' : 'designer');
            selfCampaign.LastViewedState('D');
        }
        $("#FEModal").modal('hide');
        i++;
    }

    selfDesigner.FECancel = function () {
        $("#fulledit-View").modal('hide');
        $("#FEModal").modal('hide');
    }



    selfDesigner.fulleditcampaign = function () {
        $("#fulledithtml").redactor('code.set', selfCampaign.HTMLContent());
        $("#fulledithtml").css('display', 'inline-block');
        if (i != 0)
            selfDesigner.IsFullEdit(true);

    }

    var j = 0;
    selfDesigner.CancelFullEdit = function () {
            selfDesigner.IsFullEdit(false);
            $("#fulledit-View").modal('hide');
            if (j == 1)
                $('#fulledithtml').redactor('code.toggle');

            j++;
    }

    selfDesigner.CodeDownlaod = function () {
        var contentType = selfDesigner.CampaignType() != campaignType.PLAINTEXT ? 'text/HTML' : 'text/plain';
        var encodedString = btoa(unescape(encodeURIComponent(selfCampaign.Designer().ViewableHTML().trim())));
        var htmlContent = "data:" + contentType + ";base64," + encodedString;
        kendo.saveAs({
            dataURI: htmlContent,
            fileName: "HtmlCode.txt",
            proxyURL: "/Campaign/DownloadHtml",
            forceProxy: true
        });

    }

    selfDesigner.showUrlMergeFields = function (showFields) {
        if (showFields == true) {
            $('#redactor-modal-link-insert').append('<div class="form-group medium"><label class="control-label">[|Fields|] </label><input id="mergefields" /></div><div class="st-insert-link-btn"><a class="btn btn-lg btn-primary" id="appendToURL">Save</a></div>')

            $("#mergefields").kendoDropDownList({
                dataTextField: "Title",
                dataValueField: "FieldCode",
                change: function (e) {
                    fieldCode = this.value();
                },
                dataSource: selfCampaign.MergeFieldsForLinks,
                optionLabel: "Select a Field..."
            });
            $("#appendToURL").click(function () {
                currentURL.link.$inputUrl.val($('#redactor-link-url').val() + "*|" + fieldCode + "|*");
            })
        }
        $('#url-mergefields').parent().remove();
    }
};


var campaignStyleSheet = function (htmlContent) {
    selfStyleSheet = this;
    htmlContentElement = document.createElement('html');
    htmlContentElement.innerHTML = htmlContent;
    campaignStyles = $(htmlContentElement).find("#campaignstyles");
    selfStyleSheet.BackGround = {
        OuterBackGround: ko.observable($(htmlContentElement).find("#outerbackground").css('background-Color') || "#fff"),
        InnerBackGround: ko.observable($(htmlContentElement).find("#innerbackground").css('background-Color') || "#ffffff")
    };

    selfStyleSheet.Border = {
        BorderColor: ko.observable($(htmlContentElement).find("#bordercolor").css('border-color') || "black"),
        BorderStyle: ko.observable($(htmlContentElement).find("#borderstyle").css('border-style') || "none"),
        BorderThickness: ko.observable($(htmlContentElement).find("#borderthickness").css('border-width') || "none")
    };

    selfStyleSheet.BorderStyles = ["none", "dotted", "dashed", "solid", "double"];
    selfStyleSheet.BorderThicknessStyles = ["thin", "medium", "thick"];
    selfStyleSheet.Headers = ko.observableArray([
        {
            HeaderSize: "H1",
            HeadingColor: ko.observable(""),
            FontSize: ko.observable(""),
            FontColor: ko.observable("")
        },
        {
            HeaderSize: "H2",
            HeadingColor: ko.observable(""),
            FontSize: ko.observable(""),
            FontColor: ko.observable("")
        },
        {
            HeaderSize: "H3",
            HeadingColor: ko.observable(""),
            FontSize: ko.observable(""),
            FontColor: ko.observable("")
        },
        {
            HeaderSize: "H4",
            HeadingColor: ko.observable(""),
            FontSize: ko.observable(""),
            FontColor: ko.observable("")
        },
        {
            HeaderSize: "H5",
            HeadingColor: ko.observable(""),
            FontSize: ko.observable(""),
            FontColor: ko.observable("")
        },
        {
            HeaderSize: "H6",
            HeadingColor: ko.observable(""),
            FontSize: ko.observable(""),
            FontColor: ko.observable("")
        }
    ]);
    selfStyleSheet.Headers()[0].HeadingColor.subscribe(function (e) {
        $("#designerControls h1").css({
            background: e
        });
    });
    selfStyleSheet.Headers()[0].FontSize.subscribe(function (e) {
        $("#designerControls h1").css({
            'font-size': e + 'px'
        });
    });
    selfStyleSheet.Headers()[0].FontColor.subscribe(function (e) {
        $("#designerControls h1").css({
            color: e
        });
    });

    selfStyleSheet.Headers()[1].HeadingColor.subscribe(function (e) {
        $("#designerControls h2").css({
            background: e
        });
    });
    selfStyleSheet.Headers()[1].FontSize.subscribe(function (e) {
        $("#designerControls h2").css({
            'font-size': e + 'px'
        });
    });
    selfStyleSheet.Headers()[1].FontColor.subscribe(function (e) {
        $("#designerControls h2").css({
            color: e
        });
    });

    selfStyleSheet.Headers()[2].HeadingColor.subscribe(function (e) {
        $("#designerControls h3").css({
            background: e
        });
    });
    selfStyleSheet.Headers()[2].FontSize.subscribe(function (e) {
        $("#designerControls h3").css({
            'font-size': e + 'px'
        });
    });
    selfStyleSheet.Headers()[2].FontColor.subscribe(function (e) {
        $("#designerControls h3").css({
            color: e
        });
    });

    selfStyleSheet.Headers()[3].HeadingColor.subscribe(function (e) {
        $("#designerControls h4").css({
            background: e
        });
    });
    selfStyleSheet.Headers()[3].FontSize.subscribe(function (e) {
        $("#designerControls h4").css({
            'font-size': e + 'px'
        });
    });
    selfStyleSheet.Headers()[3].FontColor.subscribe(function (e) {
        $("#designerControls h4").css({
            color: e
        });
    });

    selfStyleSheet.Headers()[4].HeadingColor.subscribe(function (e) {
        $("#designerControls h5").css({
            background: e
        });
    });
    selfStyleSheet.Headers()[4].FontSize.subscribe(function (e) {
        $("#designerControls h5").css({
            'font-size': e + 'px'
        });
    });
    selfStyleSheet.Headers()[4].FontColor.subscribe(function (e) {
        $("#designerControls h5").css({
            color: e
        });
    });

    selfStyleSheet.Headers()[5].HeadingColor.subscribe(function (e) {
        $("#designerControls h6").css({
            background: e
        });
    });
    selfStyleSheet.Headers()[5].FontSize.subscribe(function (e) {
        $("#designerControls h6").css({
            'font-size': e + 'px'
        });
    });
    selfStyleSheet.Headers()[5].FontColor.subscribe(function (e) {
        $("#designerControls h6").css({
            color: e
        });
    });


    selfStyleSheet.SelectedHeader = ko.observable(0)
    selfStyleSheet.HeaderTypes = ko.observableArray([
     { TypeId: 0, Name: "H1" },
     { TypeId: 1, Name: "H2" },
     { TypeId: 2, Name: "H3" },
            {
                TypeId: 3, Name: "H4"
            },
     { TypeId: 4, Name: "H5" },
     { TypeId: 5, Name: "H6" }
    ]);
};


var campaignStyleSheetOld = function () {
    var selfStyleSheet = this;
    selfStyleSheet.OuterBackGround = ko.observable("#fff");
    selfStyleSheet.InnerBackGround = ko.observable(ko.observable(""));

    selfStyleSheet.BorderColor = ko.observable("None");

    selfStyleSheet.BorderThickness = ko.observable("None");

    selfStyleSheet.H1HeadingColor = ko.observable("");

    selfStyleSheet.H1 = ko.observable({
        HeadingColor: ko.observable(""),
        FontSize: ko.observable(""),
        FontColor: ko.observable(""),
        FooterSize: ko.observable(""),
        FooterColor: ko.observable("")
    });

    selfStyleSheet.H1.each(function (index, value) {
        var h1Elements = document.getElementsByTagName("h1");

        for (var i = 0; i < h1Elements.length; i++) {
            h1Elements[i].style.color = selfStyleSheet.H1().HeadingColor();
            h1Elements[i].style.fontSize = selfStyleSheet.H1().FontSize();


        }
    })
    selfStyleSheet.H2 = {
        HeadingColor: ko.observable(""),
        FontSize: ko.observable(""),
        FontColor: ko.observable(""),
        FooterSize: ko.observable(""),
        FooterColor: ko.observable("")
    };
    selfStyleSheet.H3 = {
        HeadingColor: ko.observable(""),
        FontSize: ko.observable(""),
        FontColor: ko.observable(""),
        FooterSize: ko.observable(""),
        FooterColor: ko.observable("")
    };
    selfStyleSheet.H4 = {
        HeadingColor: ko.observable(""),
        FontSize: ko.observable(""),
        FontColor: ko.observable(""),
        FooterSize: ko.observable(""),
        FooterColor: ko.observable("")
    };
    selfStyleSheet.H5 = {
        HeadingColor: ko.observable(""),
        FontSize: ko.observable(""),
        FontColor: ko.observable(""),
        FooterSize: ko.observable(""),
        FooterColor: ko.observable("")
    };
    selfStyleSheet.H6 = {
        HeadingColor: ko.observable(""),
        FontSize: ko.observable(""),
        FontColor: ko.observable(""),
        FooterSize: ko.observable(""),
        FooterColor: ko.observable("")
    };

};


var controlTemplate = function (id, templateSourceId) {
    var selfTemplate = this;
    selfTemplate.Id = ko.observable(id);
    selfTemplate.EditedHTML = ko.observable();
    selfTemplate.TemplateSourceId = ko.observable(templateSourceId);
    selfTemplate.SortId = ko.observable();
    selfTemplate.IsStyleTag = ko.observable(false);
    selfTemplate.IsTableWidget = ko.observable(false);
};


var campaignLayout = function (layoutData) {
    var selfLayout = this;
    selfLayout.Id = ko.observable(layoutData.Id);
    selfLayout.Name = ko.observable(layoutData.Name);
    selfLayout.ThumbnailImageUrl = ko.observable(layoutData.ThumbnailImageUrl);
    selfLayout.LayoutType = ko.observable(layoutData.LayoutType);
}


var campaignTemplateLayout = function (htmlcontect, webApp, menuItemId) {
    var selfLayoutTemplate = this;
    selfLayoutTemplate.Name = ko.observable().extend({ required: { message: "[|Please enter Campaign Template Name|]" } });;
    selfLayoutTemplate.HTMLContent = ko.observable(htmlcontect);
    selfLayoutTemplate.OriginalName = ko.observable();
    selfLayoutTemplate.ImageContent = ko.observable("");
    selfLayoutTemplate.ImageType = ko.observable();
    selfLayoutTemplate.ThumbnailImageUrl = ko.observable();

    selfLayoutTemplate.saveCampaignTemplate = function () {
        selfLayoutTemplate.errors = ko.validation.group(selfLayoutTemplate);
        if (selfLayoutTemplate.errors().length > 0) {
            selfLayoutTemplate.errors.showAllMessages();
            return;
        }
        if (selfLayoutTemplate.ImageContent() == "" || selfLayoutTemplate.ImageContent() == " ") {
            notifyError("[|Upload one Campaign Layout Image.|]");
            return;
        }

        innerLoader('SaveAsCampaignTemplate');
        var jsondata = ko.toJSON(selfLayoutTemplate);
        $.ajax({
            url: webApp + 'InsertCampaignTemplate',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'campaignTemplteViewModel': jsondata }),

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
            removeinnerLoader('SaveAsCampaignTemplate');
            notifySuccess("[|Successfully saved the Template|]");
            //   CloseTopInner("menuItem" + menuItemId, "menuPartialItemContent" + menuItemId);
            $('#cancel')[0].click();
            // CloseTopInner(this);
            if (data.success === true) {
                // window.location.href = "/campaigns";
            }
        }).fail(function (error) {
            notifyError(error);
        });
    }

}


var campaignStatus = {
    DRAFT: "101",
    SCHEDULED: "102",
    CANCELED: "103",
    SENT: "105",
    QUEUED: "106",
    FAILED: "110",
    ACTIVE: "107"
};


var linkTemplate = {
    CampaignId: 0,
    CampaignLinkID: 0,
    URL: { URL: "" },
    LinkIndex: 0,
    Name: ""
}

var campaignType = {
    REGULAR: "131",
    PLAINTEXT: "132",
    HTMLCODE: "133"
};

applyTableResizable = function () {
    $("#controlbeingedited td").off('resizable');
    $("#controlbeingedited td").resizable({
        handles: "e, s",
        resize: function (event, ui) {
            var sizerID = "#" + $(event.target).attr("id");
            $(sizerID).width(ui.size.width);
        }
    });
}
