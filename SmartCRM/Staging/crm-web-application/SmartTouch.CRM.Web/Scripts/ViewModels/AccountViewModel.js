
var accountViewModel = function (data, countries, url, accounturl, newAddress, userbaseurl, AccountbaseURL, mode, pagename, webserbviceurl, accountCreation, IsAccountStAdmin, smartTouchPartnerKey, dateFormat) {
   var selfAccount = this;

    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfAccount));
    ko.validation.configure({
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null,
        grouping: {
            deep: true
        }
    });


    ko.validation.rules['url'] = {
        validator: function (val, required) {
            if (!val)
                return required;
            val = val.replace(/^\s+|\s+$/, ''); //Strip whitespace
            return val.match(/^(?:(?:https?|ftp):\/\/)(?:\S+(?::\S*)?@)?(?:(?!10(?:\.\d{1,3}){3})(?!127(?:\.‌​\d{1,3}){3})(?!169\.254(?:\.\d{1,3}){2})(?!192\.168(?:\.\d{1,3}){2})(?!172\.(?:1[‌​6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1‌​,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00‌​a1-\uffff0-9]+-?)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]+-?)*[a-z\u‌​00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})))(?::\d{2,5})?(?:\/[^\s]*)?$/i);
        },
        message: '[|This field has to be a valid URL|]'
    };


    ko.validation.registerExtenders();

    //Returns a copy of newAddress
    selfAccount.getAddressTemplate = function () {
        var addressTemplate = jQuery.extend(true, {}, JSON.parse(newAddress));
        addressTemplate.Countries = countries;
        addressTemplate.States = ko.observableArray();
        addressTemplate.Country.Code = ko.observable("");
        addressTemplate.State.Code = ko.observable();
        addressTemplate.AddressTypeID = ko.observable(addressTemplate.AddressTypeID);
        addressTemplate.IsDefault = ko.observable(addressTemplate.IsDefault);
        addressTemplate.countrySelect = function () {
            addressTemplate.State.Code('');
        }
        return addressTemplate;
    };

    selfAccount.IsEnabled = ko.observable(data.AccountID == 1 ? false : true);
    selfAccount.SubscriptionId = ko.observable(data.SubscriptionId == 0 ? '' : data.SubscriptionId);
    selfAccount.SubscriptionValidation = selfAccount.SubscriptionId.extend({
        required: {
            message: 'Subscription is required'
        }
    });
    selfAccount.visiStatStatuses = ko.observableArray([
    { Status: "[|Active|]", Id: 1 },
    { Status: "[|Inactive|]", Id: 0 }
    ]);

    selfAccount.UserLimit = ko.observable(data.UserLimit);
    selfAccount.PreviousUserLimit = data.UserLimit;
    selfAccount.IsAccountStAdmin = IsAccountStAdmin;
    selfAccount.HelpURL.extend({
        required: {
            message: "[|Help URL is required|]"
        },
        pattern: {
            params: /(http(s)?:\\)?([\w-]+\.)+[\w-]+[.com|.in|.org]+(\[\?%&=]*)?/, message: "[|Invalid Web Site URL|]",
        }
    });
    selfAccount.Image = ko.observable(data.Image);
    selfAccount.Roles = ko.observableArray(data.Roles);
    selfAccount.SelectedRoles = ko.observableArray(data.SelectedRoles);

    selfAccount.ShowTC = ko.observable(data.ShowTC);
    selfAccount.Disclaimer = ko.observable(data.Disclaimer == null ? false : data.Disclaimer);

    if (selfAccount.Image().ImageContent != null && selfAccount.Image().ImageContent != "undefined") {
    }
    else {
        selfAccount.Image().ImageContent = ImagePath;
    }

    selfAccount.Subscriptions = ko.observableArray([
        { SubscriptionID: 2, SubscriptionName: 'Standard Subscription' },
        { SubscriptionID: 3, SubscriptionName: 'BDX Subscription' }
    ]);

    if (IsAccountStAdmin == "True") {
        selfAccount.WebAnalyticsProvider = data.WebAnalyticsProvider;
        selfAccount.WebAnalyticsProvider.NotificationStatus = ko.observable(selfAccount.WebAnalyticsProvider.NotificationStatus);
        selfAccount.WebAnalyticsProvider.DailyStatusEmailOpted = ko.observable(selfAccount.WebAnalyticsProvider.DailyStatusEmailOpted);
        selfAccount.WebAnalyticsProvider.InstantNotificationGroup = ko.observableArray(selfAccount.WebAnalyticsProvider.InstantNotificationGroup);
        selfAccount.WebAnalyticsProvider.DailySummaryNotificationGroup = ko.observableArray(selfAccount.WebAnalyticsProvider.DailySummaryNotificationGroup);
        selfAccount.WebAnalyticsProvider.StatusID = ko.observable(selfAccount.WebAnalyticsProvider.StatusID);
        selfAccount.WebAnalyticsProvider.APIKey = ko.observable(selfAccount.WebAnalyticsProvider.APIKey).extend({
            required:
                {
                    message: "[|Key is required if status is active|]",
                    onlyIf: function () {
                        return (selfAccount.WebAnalyticsProvider.StatusID() == "1");
                    }
                }
        });
        selfAccount.WebAnalyticsProvider.TrackingDomain = ko.observable(selfAccount.WebAnalyticsProvider.TrackingDomain).extend({
            required:
                {
                    message: "[|Domain is required if VisiStat status is active|]",
                    onlyIf: function () {
                        return (selfAccount.WebAnalyticsProvider.StatusID() == "1");
                    }
                }
        });
        selfAccount.ActiveUsers = ko.observableArray([]);
        selfAccount.Users = ko.observableArray();
        $.ajax({
            url: '/getusers',
            type: 'get',
            dataType: 'json',
            data: { 'Id': 0 },
            contentType: "application/json; charset=utf-8"
        }).then(function (response) {
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response);
            } else {
                filter.reject(response.error);
            }
            return filter.promise();
        }).done(function (data) {
            //var owner = data.response;
            selfAccount.Users(data.response);
            selfAccount.ActiveUsers(ko.utils.arrayFilter(selfAccount.Users(), function (user) {
                return user.IsDeleted == false;
            }));
        }).fail(function (error) {
            notifyError(error);
        });
    }

    function onSelect(e) {
        $('.k-file').hide();
        //var varImageType = "";
        $.each(e.files, function (index, value) {
            var ok = value.extension.toLowerCase() == ".jpg"
                     || value.extension.toLowerCase() == ".jpeg"
                     || value.extension.toLowerCase() == ".png"
                     || value.extension.toLowerCase() == ".bmp";

            if (!ok) {
                e.preventDefault();
                notifyError("[|Please upload jpg, jpeg, png, bmp files|]"); return false;
            }
            else if (bytesToSize(e.files[0].size) > 1) {
                e.preventDefault();
                notifyError("[|Image size should not be more than 1 mb|]");
                return false;
            }

            var friendlyname = value.name;



            var fileReader = new FileReader();
            fileReader.onload = function (event) {
                //  self.imagePath(event.target.result);
                var image = document.getElementById("contactimage");
                image.src = event.target.result;
                selfAccount.Image().ImageContent = event.target.result;
                selfAccount.Image().OriginalName = friendlyname;
                selfAccount.Image().ImageType = value.extension.toLowerCase();
            }
            fileReader.readAsDataURL(e.files[0].rawFile);
        });
    }

    $("#images").kendoUpload({
        async: {
            saveUrl: "/Image/UploadCampaignImages",
            removeUrl: "remove",
            autoUpload: false
        },
        select: onSelect
    });

    selfAccount.AccountName = ko.observable(data.AccountName).extend({ required: { message: "[|Account Name is required|]" }, maxLength: 75 });
    selfAccount.FirstName = ko.observable(data.FirstName).extend({ required: { message: "[|First Name is required|]" }, maxLength: 75 });
    selfAccount.LastName = ko.observable(data.LastName).extend({ required: { message: "[|Last Name is required|]" }, maxLength: 75 });
    selfAccount.Company = ko.observable(data.Company).extend({ maxLength: 75 });
    selfAccount.GoogleDriveClientID = ko.observable(data.GoogleDriveClientID);
    selfAccount.GoogleDriveAPIKey = ko.observable(data.GoogleDriveAPIKey);
    selfAccount.DropboxAppKey = ko.observable(data.DropboxAppKey);
    selfAccount.PrimaryEmail = ko.observable(data.PrimaryEmail).extend({ required: { message: "[|Email is required|]" }, maxLength: 256, email: true });
    selfAccount.VisiStats = ko.mapping.fromJS(data.VisiStats, {});
    selfAccount.DateFormat = ko.observable(dateFormat);

    selfAccount.DomainURL = ko.observable(data.DomainURL).extend({ required: { message: "[|Domain URL is required|]" }, maxLength: 63 });
    selfAccount.PreviousDomainURL = ko.observable(data.DomainURL);
    selfAccount.DomainAvailability = ko.observable();
    selfAccount.DomainURL.subscribe(function (value) {
        if (value != null) {
            $("#add-tag-loader").removeClass("hide");
            $.ajax({
                url: '/checkdomainurl',
                type: 'get',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: { 'domainURL': value }
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
                if (data.response.Available === true) {
                    //alert(data.Available);

                    setTimeout(function () {
                        $("#add-tag-loader").addClass("hide");
                        selfAccount.DomainAvailability("[|Domain name is available|]");
                    }, 500);
                    setTimeout(function () {
                        selfAccount.DomainAvailability("");
                    }, 2000);
                }
                else {
                    if (data.Message != null)
                        notifyError(data.Message);
                    else {
                        setTimeout(function () {
                            $("#add-tag-loader").addClass("hide");
                            selfAccount.DomainAvailability("[|Domain name is not available|]");
                        }, 500);
                        setTimeout(function () {
                            selfAccount.DomainAvailability("");
                        }, 2000);
                    }
                }
            }).fail(function (error) {
                // Display error message to user  
                notifyError(error);
            });
        }
    });

    selfAccount.PrivacyPolicy = ko.observable(data.PrivacyPolicy).extend({
        url: true, required: {
            message: "[|Privacy policy is required |]",
            onlyIf: function () {
                return (parseInt(selfAccount.AccountID()) == 1);
            }
        }
    });
    selfAccount.AccountCountries = ko.observableArray([]);
    selfAccount.DateFormats = ko.observableArray([]);
    selfAccount.Currency = ko.observableArray([]);
    selfAccount.TimeZone = ko.observable(data.TimeZone);
    selfAccount.TimeZones = ko.observableArray([]);

    $.ajax({
        url: AccountbaseURL + 'GetTimeZones',
        type: 'get',
        dataType: 'json',
        contentType: "application/json; charset=utf-8",
        success: function (timezonedata) {
            selfAccount.TimeZones(timezonedata.response);
            var code = selfAccount.TimeZone();
            selfAccount.TimeZone("");
            selfAccount.TimeZone(code);
        }
    });

    $.ajax({
        url: url + 'GetCountries',
        type: 'get',
        dataType: 'json',
        contentType: "application/json; charset=utf-8",
        success: function (countriesdata) {
            selfAccount.AccountCountries(countriesdata.response);
            var code = selfAccount.CountryID();
            selfAccount.CountryID("");
            selfAccount.CountryID(code);

        }
    });


    selfAccount.CurrencyID = ko.observable(data.CurrencyID).extend({ required: { message: "[|Currency is required|]" } });
    selfAccount.CurrencyFormat = ko.observable(data.CurrencyFormat);

    selfAccount.Modules = ko.observableArray(data.Modules);
    selfAccount.SubscribedModules = ko.observableArray(data.SubscribedModules);

    if (selfAccount.CurrencyID() == 1 || selfAccount.CurrencyID() == 2) {
        selfAccount.CurrencyFormat("$X,XXX.XX");
    }
    else if (selfAccount.CurrencyID() == 3) {
        selfAccount.CurrencyFormat("X XXX,XX $");
    }
    else if (selfAccount.CurrencyID() == 5) {
        selfAccount.CurrencyFormat("₹X,XXX.XX");
    }
    else {
        selfAccount.CurrencyFormat("B/.X,XXX.XX");
    }


    $.ajax({
        url: userbaseurl + 'GetDateFormats',
        type: 'get',
        dataType: 'json',
        contentType: "application/json; charset=utf-8",
        success: function (dateformatdata) {
            selfAccount.DateFormats(dateformatdata.response);
            var code = selfAccount.DateFormatID();
            selfAccount.DateFormatID("");
            selfAccount.DateFormatID(code);
        }
    });

    $.ajax({
        url: userbaseurl + 'GetCurrencies',
        type: 'get',
        dataType: 'json',
        contentType: "application/json; charset=utf-8",
        success: function (currencies) {
            selfAccount.Currency(currencies.response);
            var code = selfAccount.CurrencyID();
            selfAccount.CurrencyID("");
            selfAccount.CurrencyID(code);
        }
    });


    selfAccount.DateFormatID = ko.observable(data.DateFormatID).extend({ required: { message: "[|Date Format is required|]" } });
    selfAccount.CountryID = ko.observable(data.CountryID).extend({ required: { message: "[|Country is required|]" } });

    selfAccount.PhoneTypes = ko.observableArray(['Mobile', 'Work', 'Home']);
    selfAccount.Phones = ko.observableArray(data.Phones);
    if (accountCreation == 'True')
        selfAccount.AccountStatus = ko.observableArray([{ Id: 105, Type: '[|Draft|]' }, { Id: 1, Type: '[|Active|]' }]);
    else
        selfAccount.AccountStatus = ko.observableArray([{ Id: 3, Type: '[|Paused|]' }, { Id: 1, Type: '[|Active|]' }, { Id: 4, Type: '[|Inactive|]' }, { Id: 5, Type: '[|Maintenance|]' }]);

    selfAccount.OpportunityCustomers = ko.observable(data.OpportunityCustomers);

    selfAccount.Status = ko.observable(data.Status);
    selfAccount.PreviousStatus = ko.observable(data.Status);
    selfAccount.StatusMessage = ko.observable(data.StatusMessage).extend({
        required: {
            onlyIf: function () {
                if (selfAccount.Status() == 3 || selfAccount.Status() == 5)
                    return true;
                else
                    return false;
            },
            message: "[|Status Message is requried|]"
        }
    });
    selfAccount.StatusSelect = function () {
        selfAccount.StatusMessage('');
    }
    selfAccount.addPhone = function () {
        var phoneTypes = ["Mobile", "Work", "Home"];
        var newPhone = {
            PhoneType: ko.observable(''),
            PhoneNumber: ko.observable('')
        };
        $.each(selfAccount.Phones(), function (index, phone) {
            var phoneTypeIndex = phoneTypes.indexOf(phone.PhoneType());
            if (phoneTypeIndex > -1) {
                phoneTypes.splice(phoneTypeIndex, 1);
            }
        });
        newPhone.PhoneType(phoneTypes[0]);
        selfAccount.Phones.push(newPhone);
    };

    selfAccount.removePhone = function () {
        selfAccount.Phones.remove(this);
    };

    selfAccount.countryChanged = function (address) {
        $.ajax({
            url: url + 'GetStates',
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: { 'countryCode': address.Country.Code() },
            success: function (data) {
                address.States(data.response);
                var code = address.State.Code();
                address.State.Code("");
                address.State.Code(code);

            }
        });
    };

    selfAccount.AddressTypes = ko.observableArray(data.AddressTypes);
    selfAccount.Addresses = ko.observableArray(data.Addresses);

    selfAccount.primaryAddress = ko.observable();


    selfAccount.addAddress = function () {
        var newAddressCopy = selfAccount.getAddressTemplate();
        newAddressCopy.Country.Code.subscribe(function () {
            selfAccount.countryChanged(newAddressCopy);
        });
        newAddressCopy.IsDefault = ko.pureComputed(function () {
            return newAddressCopy === selfAccount.primaryAddress();
        });
        selfAccount.countryChanged(newAddressCopy);
        selfAccount.Addresses.push(newAddressCopy);
    };
    selfAccount.removeAddress = function () {
        selfAccount.Addresses.remove(this);
    };

    $.each(selfAccount.Addresses(), function (index, address) {
        address.AddressTypeID = ko.observable(address.AddressTypeID);
        address.States = ko.observableArray(address.States);
        address.Country.Code = ko.observable(address.Country.Code);
        selfAccount.countryChanged(address);
        address.State.Code = ko.observable(address.State.Code);
        address.IsDefault = ko.observable(address.IsDefault);
        address.countrySelect = function () {
            address.State.Code('');
        };
        address.Country.Code.subscribe(function () {
            selfAccount.countryChanged(address);
        });
        address.Countries = countries;

        if (address.IsDefault() === true) {
            selfAccount.primaryAddress(address);
        }
        address.IsDefault = ko.pureComputed(function () {
            return address === selfAccount.primaryAddress();
        });
    });

    selfAccount.SocialMediaUrlTypes = ko.observableArray(["[|Website|]", "[|LinkedIn|]", "[|Facebook|]", "[|Twitter|]", "[|Google+|]", "[|Blog|]"]);

    selfAccount.SocialMediaUrls = ko.observableArray(data.SocialMediaUrls);
    var index = 0;
    selfAccount.addSocialMediaUrl = function () {
        if (index == 5) {
            index = 0;
        }
        else {
            index++;
        }
        selfAccount.SocialMediaUrls.push({ MediaType: ko.observable(selfAccount.SocialMediaUrlTypes()[index]), Url: ko.observable('') });
        $("select[name='mediumdf']").selectpicker({ style: 'btn-default' });
    };
    selfAccount.removeSocialMediaUrl = function () {
        selfAccount.SocialMediaUrls.remove(this);
    };

    selfAccount.SecondaryEmails = ko.observableArray(data.SecondaryEmails);

    selfAccount.addEmail = function () {
        selfAccount.SecondaryEmails.push({ SecondaryEmailId: ko.observable('') });
    };
    selfAccount.removeEmail = function () {
        selfAccount.SecondaryEmails.remove(this);
    };

    //*communication providers
    selfAccount.transactionalCommunicationModes = ko.observableArray([
      { CommunicationTypeId: "1", CommunicationTypeName: '[|Mail|]' },
      { CommunicationTypeId: "2", CommunicationTypeName: '[|Text|]' }
    ]);
    selfAccount.campaignCommunicationModes = ko.observableArray([
     { CommunicationTypeId: "1", CommunicationTypeName: '[|Mail|]' }
    ]);

    selfAccount.emailServiceProviders = ko.observableArray(data.CampaignProviders);

    selfAccount.transactionalEmailServiceProviders = ko.observableArray([
       { Name: "[|SendGrid|]", Id: 2 },
       { Name: "[|SmartTouch|]", Id: 4 }]);

    ko.bindingHandlers.readOnly = {
        update: function (element, valueAccessor) {
            var value = ko.utils.unwrapObservable(valueAccessor());
            if (value) {
                element.setAttribute("readOnly", true);
            } else {
                element.removeAttribute("readOnly");
            }
        }

    }

    selfAccount.EmailProviderType = ko.observable();
    selfAccount.CampaignProviderType = ko.observable();
    selfAccount.CampaignCommunicationType = ko.observable();
    selfAccount.TextProviderType = ko.observable();
    selfAccount.TextServiceProviders = ko.observableArray([{ Name: "[|Twillio|]", Id: 1 }]);
    selfAccount.ServiceProviderRegistrationDetails = ko.observableArray(data.ServiceProviderRegistrationDetails);//.extend({ tServiceProviders: true, cServiceProviders: true });

    var defaulfSPIndex = selfAccount.ServiceProviderRegistrationDetails().map(function (e) { return e.IsDefault && (e.MailProviderType == 3 || e.MailProviderType == 4 || e.MailProviderType == 1) }).indexOf(true);

    if (selfAccount.ServiceProviderRegistrationDetails().length > 0)
        selfAccount.SelectedServiceProviderID = ko.observable(selfAccount.ServiceProviderRegistrationDetails()[defaulfSPIndex != -1 ? defaulfSPIndex : 0].ServiceProviderID);
    else
        selfAccount.SelectedServiceProviderID = ko.observable();
    selfAccount.CampaignTextProviderType = ko.observable();
    selfAccount.CommunicationType = ko.observable();
    selfAccount.TextEnableProvider = ko.observable('');
    selfAccount.EmailEnableProvider = ko.observable('');
    selfAccount.CampaignTextEnableProvider = ko.observable('');
    selfAccount.CampaignEmailEnableProvider = ko.observable('');

    if (data.ServiceProviderRegistrationDetails != null) {
        $.each(data.ServiceProviderRegistrationDetails, function (index, value) {
            if (value.MailProviderID == 2) {

                value.UserName = ko.observable(value.UserName).extend({ required: { message: "[|UserName is required|]" }, maxLength: 50 });
                value.Password = ko.observable(value.Password).extend({ required: { message: "[|Password is required|]" } });
                value.Host = ko.observable(value.Host).extend({ required: { message: "[|Host is required|]" } });
                value.Port = ko.observable(value.Port).extend({ required: { message: "[|Port is required|]" } });
                value.Email = ko.observable(value.Email).extend({ maxLength: 256, email: true });

            }
            else if (value.MailProviderID == 3) {

                value.ProviderName = ko.observable(value.ProviderName).extend({ required: { message: "[|MailChimp Provider Name is required|]" } });
                value.UserName = ko.observable(value.UserName);
                value.Password = ko.observable(value.Password);
                value.MailChimpListID = ko.observable(value.MailChimpListID);
                value.SenderFriendlyName = ko.observable(value.SenderFriendlyName);
                value.IsDefault = ko.observable(value.IsDefault);
                value.Email = ko.observable(value.Email).extend({
                    required: {
                        onlyIf: function () {
                            return (value.UserName() != "" || value.Password() != "" || (value.MailChimpListID() != "" && value.MailChimpListID() != null)
                                || value.SenderFriendlyName() != "" || value.IsDefault() == true);

                        },
                        message: "[|Email is Required|]"
                    },
                    email: true
                });

                value.ApiKey = ko.observable(value.ApiKey).extend({
                    required: {
                        onlyIf: function () {
                            return (value.UserName() != "" || value.Password() != "" || (value.MailChimpListID() != "" && value.MailChimpListID() != null)
                                || value.SenderFriendlyName() != "" || (value.Email() != "" && value.Email() != null) || value.IsDefault() == true);

                        },
                        message: "[|API Key is required|]"
                    }
                });
            } else if (value.MailProviderID == 1) {
                value.UserName = ko.observable(value.UserName).extend({ required: { message: "[|UserName is required|]" }, maxLength: 50 });
                value.Password = ko.observable(value.Password).extend({ required: { message: "[|Password is required|]" } });
                value.Host = ko.observable(value.Host).extend({ required: { message: "[|Host is required|]" } });
                value.Port = ko.observable(value.Port).extend({ required: { message: "[|Port is required|]" } });
                value.Email = ko.observable(value.Email).extend({ email: true });
            } else if (value.MailProviderID == 4) {
                if (value.MailProviderType == 3)
                    value.ProviderName = ko.observable(value.ProviderName).extend({ required: { message: "[|VMTA Provider Name is required|]" } });
                value.VMTA = ko.observable(value.VMTA).extend({ required: { message: "[|SmartTouch is required|]" } });
                value.Host = ko.observable(value.Host).extend({ required: { message: "[|Host is required|]" } });
                value.Port = ko.observable(value.Port).extend({ required: { message: "[|Port is required|]" } });
                value.UserName = ko.observable(value.UserName).extend({ required: { message: "[|UserName is required|]" } });
                value.Password = ko.observable(value.Password).extend({ required: { message: "[|Password is required|]" } });
                value.Email = ko.observable(value.Email).extend({ required: { message: "[|Email is required|]" }, maxLength: 256, email: true });
                value.ImageDomainId = ko.observable(value.ImageDomainId).extend({ required: { message: "[|Image domain is required|]" } });
            } else if (value.TextProviderID == 1) {
                value.UserName = ko.observable(value.UserName).extend({ required: { message: "[|UserName is required|]" }, maxLength: 50 });
                value.Password = ko.observable(value.Password).extend({ required: { message: "[|Password is required|]" } });
                value.ApiKey = ko.observable(value.ApiKey).extend({ required: { message: "[|API Key is required|]" } });
                value.LoginToken = ko.observable(value.LoginToken).extend({ required: { message: "[|LoginToken is required|]" } });
                value.SenderPhoneNumber = ko.observable(value.SenderPhoneNumber).extend({ required: { message: "[|PhoneNumber is required|]" }, number: { message: "[|Please Enter Number|]" }, digit: 0 });
            }
            if (ko.utils.unwrapObservable(value.IsDefault) == true && parseInt(value.MailProviderType) == 2 && (parseInt(value.MailProviderID) == 2 || parseInt(value.MailProviderID) == 4)) {
                selfAccount.CommunicationType(1);
                if (value.MailProviderID == "2")
                    selfAccount.EmailProviderType(2);
                else
                    selfAccount.EmailProviderType(4);
                selfAccount.TextEnableProvider(false);
                selfAccount.EmailEnableProvider(true);

            }
            if (ko.utils.unwrapObservable(value.IsDefault) == true && parseInt(value.MailProviderType) == 3 && parseInt(value.MailProviderID) > 2) {
                selfAccount.CampaignCommunicationType(1);
                selfAccount.CampaignProviderType(value.MailProviderID);
                selfAccount.CampaignTextEnableProvider(false);
                selfAccount.CampaignEmailEnableProvider(true);
            }
            value.NewCommunicationType = ko.observable(value.CampaignProviderType);

        });
    }

    selfAccount.drpselect = function (e) {
        var dataItem = this.dataItem(e.item);
    }

    selfAccount.CommunicationType.subscribe(function () {
        var Defaultprovider = ko.utils.arrayFirst(selfAccount.ServiceProviderRegistrationDetails(), function (ServiceProvider) {
            if (ServiceProvider.MailProviderType == 2 && (ServiceProvider.MailProviderID > 0) && (ko.utils.unwrapObservable(ServiceProvider.IsDefault) == true || ko.utils.unwrapObservable(ServiceProvider.IsDefault) == 'true'))
                return ServiceProvider;
        });
        var DefaultTextprovider = ko.utils.arrayFirst(selfAccount.ServiceProviderRegistrationDetails(), function (ServiceProvider) {
            if (ServiceProvider.MailProviderType == 2 && (ServiceProvider.TextProviderID == 1) && (ko.utils.unwrapObservable(ServiceProvider.IsDefault) == true || ko.utils.unwrapObservable(ServiceProvider.IsDefault) == 'true'))
                return ServiceProvider;
        });
        if (selfAccount.CommunicationType() == "1") {
            selfAccount.TextEnableProvider(false);
            selfAccount.EmailEnableProvider(true);
            if (Defaultprovider)
                selfAccount.EmailProviderType(Defaultprovider.MailProviderID);
        }
        else if (selfAccount.CommunicationType() == "2") {
            selfAccount.TextEnableProvider(true);
            selfAccount.EmailEnableProvider(false);
            if (DefaultTextprovider)
                selfAccount.TextProviderType(DefaultTextprovider.TextProviderID);
        }
        else {
            selfAccount.TextEnableProvider(false);
            selfAccount.EmailEnableProvider(false);
        }
    });
    selfAccount.CampaignCommunicationType.subscribe(function (selectedValue) {

        var DefaultEmailprovider = ko.utils.arrayFirst(selfAccount.ServiceProviderRegistrationDetails(), function (ServiceProvider) {
            if (ServiceProvider.MailProviderType == 3 && (ServiceProvider.MailProviderID > 0) && (ko.utils.unwrapObservable(ServiceProvider.IsDefault == true) || ko.utils.unwrapObservable(ServiceProvider.IsDefault) == 'true'))
                return ServiceProvider;
        });
        var DefaultTextprovider = ko.utils.arrayFirst(selfAccount.ServiceProviderRegistrationDetails(), function (ServiceProvider) {
            if (ServiceProvider.MailProviderType == 3 && (ServiceProvider.TextProviderID == 1) && (ko.utils.unwrapObservable(ServiceProvider.IsDefault) == true || ServiceProvider.IsDefault == 'true'))
                return ServiceProvider;
        });
        if (selfAccount.CampaignCommunicationType() == "1") {
            selfAccount.CampaignTextEnableProvider(false);
            selfAccount.CampaignEmailEnableProvider(true);
            if (DefaultEmailprovider)
                selfAccount.CampaignProviderType(DefaultEmailprovider.MailProviderID);
        }
        else {

            selfAccount.CampaignTextEnableProvider(true);
            selfAccount.CampaignEmailEnableProvider(false);
            if (DefaultTextprovider)
                selfAccount.CampaignTextProviderType(DefaultTextprovider.TextProviderID);
        }
    });
    selfAccount.TransactionaltemporaryStorage = ko.observableArray();

    selfAccount.EmailProviderType.subscribe(function (selectedValue) {
        var provider = ko.utils.arrayFirst(selfAccount.ServiceProviderRegistrationDetails(), function (ServiceProvider) {
            if (ServiceProvider.MailProviderID == selectedValue && ServiceProvider.MailProviderType == 2)
                return ServiceProvider;
        });


        var temporaryprovider = ko.utils.arrayFirst(selfAccount.ServiceProviderRegistrationDetails(), function (ServiceProvider) {
            if (
                ko.utils.unwrapObservable(ServiceProvider.UserName) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.Password) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.SenderFriendlyName) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.Email) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.ApiKey) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.Host) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.Port) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.IsDefault) == false)
                return ServiceProvider;
        });
        if (temporaryprovider) {
            selfAccount.ServiceProviderRegistrationDetails.remove(temporaryprovider);
        }
        if (!provider) {
            if (selectedValue == 4) {
                selfAccount.ServiceProviderRegistrationDetails.push({
                    UserName: ko.observable('').extend({ required: { message: "[|User Name is required|]" } }),
                    Password: ko.observable('').extend({ required: { message: "[|Password is required|]" } }),
                    ApiKey: "",
                    SenderFriendlyName: "", MailProviderID: parseInt(selectedValue),
                    CommunicationType: 1, MailProviderType: 2,
                    Host: ko.observable('').extend({ required: { message: "[|Host is required|]" } }),
                    IsDefault: false, Email: ko.observable('').extend({ required: { message: "[|Email is required|]" }, maxLength: 256, email: true }),
                    VMTA: ko.observable(data.DefaultVMTA).extend({ required: { message: "[|SmartTouch is required|]" } }),
                    SenderDomain: "dfasdfaa", TextProviderID: 0,
                    ImageDomain: "", Port: ko.observable('').extend({ required: { message: "[|Port is required|]" } }),
                    ImageDomainId: ko.observable(""),
                    ServiceProviderID: 0
                });
            }
            else if (selectedValue == 2) {
                selfAccount.ServiceProviderRegistrationDetails.push({
                    UserName: ko.observable('').extend({ required: { message: "[|UserName is required|]" } }),
                    Password: ko.observable('').extend({ required: { message: "[|Password is required|]" } }),
                    ApiKey: "", SenderFriendlyName: "", MailProviderID: parseInt(selectedValue),
                    CommunicationType: 1, MailProviderType: 2, Host: ko.observable('').extend({ required: { message: "[|Host is required|]" } }),
                    IsDefault: false, Email: ko.observable('').extend({ maxLength: 256, email: true }), VMTA: "",
                    Port: ko.observable('').extend({ required: { message: "[|Port is required|]" } }), TextProviderID: 0
                });
            }
            else if (selectedValue == 3) {
                selfAccount.ServiceProviderRegistrationDetails.push({
                    UserName: "",
                    Password: "", ApiKey: ko.observable('').extend({ required: { message: "[|API Key is required|]" } }),
                    SenderFriendlyName: "", MailProviderID: parseInt(selectedValue),
                    CommunicationType: 1, MailProviderType: 2, Host: "", IsDefault: false, Email: ko.observable('').extend({ required: { message: "[|Email is required|]" }, maxLength: 256, email: true }), VMTA: "",
                    Port: '', TextProviderID: 0
                });
            }

        }

    });
    selfAccount.DefaultProvider = function (data) {
        $.each(selfAccount.ServiceProviderRegistrationDetails(), function (index, provider) {
            if (provider.ServiceProviderID == data.ServiceProviderID && provider.MailProviderType == data.MailProviderType) {
                if (provider.MailProviderID == 3)
                    provider.IsDefault(true);
                else
                    provider.IsDefault = true;

            } else if (provider.MailProviderType == data.MailProviderType && parseInt(provider.MailProviderID) > 0) {
                if (provider.MailProviderID == 3)
                    provider.IsDefault(false);
                else
                    provider.IsDefault = false;

            }
        });
        return true;
    };
    selfAccount.DefaultTextProvider = function (data) {

        $.each(selfAccount.ServiceProviderRegistrationDetails(), function (index, provider) {

            if (provider.TextProviderID == data.TextProviderID && provider.MailProviderType == data.MailProviderType) {

                if (provider.MailProviderID == 3)
                    provider.IsDefault(true);
                else
                    provider.IsDefault = true;

            }
            else if (provider.MailProviderType == data.MailProviderType && parseInt(provider.TextProviderID) > 0) {
                if (provider.MailProviderID == 3)
                    provider.IsDefault(false);
                else
                    provider.IsDefault = false;


            }
        })
        return true;
    };

    selfAccount.CampaignProviderType.subscribe(function (selectedValue) {

        var provider = ko.utils.arrayFirst(selfAccount.ServiceProviderRegistrationDetails(), function (ServiceProvider) {
            if (ServiceProvider.MailProviderID == selectedValue && ServiceProvider.MailProviderType == 3)
                return ServiceProvider;
        });

        var temporaryprovider = ko.utils.arrayFirst(selfAccount.ServiceProviderRegistrationDetails(), function (ServiceProvider) {
            if (ServiceProvider.MailProviderID == 3) {

            }
            if (
                ko.utils.unwrapObservable(ServiceProvider.UserName) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.Password) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.SenderFriendlyName) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.Email) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.ApiKey) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.Host) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.Port) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.IsDefault) == false) {

                return ServiceProvider;
            }

        });
        if (temporaryprovider) {
            selfAccount.ServiceProviderRegistrationDetails.remove(temporaryprovider);
        }


        if (!provider) {
            if (selectedValue == 4) {
                selfAccount.ServiceProviderRegistrationDetails.push({
                    UserName: ko.observable('').extend({ required: { message: "[|User Name is required|]" } }),
                    Password: ko.observable('').extend({ required: { message: "[|Password is required|]" } }),
                    ApiKey: "",
                    SenderFriendlyName: "",
                    MailProviderID: parseInt(selectedValue),
                    CommunicationType: 1, MailProviderType: 3,
                    Host: ko.observable('').extend({ required: { message: "[|Host is required|]" } }),
                    IsDefault: false, Email: ko.observable('').extend({ required: { message: "[|Email is required|]" }, maxLength: 256, email: true }),
                    VMTA: ko.observable(data.DefaultVMTA).extend({ required: { message: "[|SmartTouch is required|]" } }),
                    SenderDomain: "", MailChimpListID: "",
                    ImageDomain: "", Port: ko.observable("").extend({ required: { message: "[|Port is required|]" } }),
                    TextProviderID: 0, ImageDomainId: ko.observable("")
                });
                var vmtaIndex = selfAccount.ServiceProviderRegistrationDetails().length - 1;
                selfAccount.ServiceProviderRegistrationDetails[vmtaIndex].ImageDomainId.extend({ required: { message: "Image domain is required" } });
            }
            else if (selectedValue == 3) {
                selfAccount.ServiceProviderRegistrationDetails.push({
                    UserName: "",
                    Password: "", ApiKey: ko.observable("").extend({ required: { message: "[|API Key is required|]" } }),
                    SenderFriendlyName: "", MailProviderID: parseInt(selectedValue),
                    CommunicationType: 1, MailProviderType: 3, Host: "", IsDefault: false, Email: ko.observable('').extend({ required: { message: "[|Email is required|]" }, maxLength: 256, email: true }),
                    MailChimpListID: "",
                    Port: "", TextProviderID: 0
                });
            }
            else if (selectedValue == 2) {
                selfAccount.ServiceProviderRegistrationDetails.push({
                    UserName: ko.observable("").extend({ required: { message: "[|UserName is required|]" } }),
                    Password: ko.observable("").extend({ required: { message: "[|Password is required|]" } }),
                    ApiKey: "", SenderFriendlyName: "", MailProviderID: parseInt(selectedValue),
                    CommunicationType: 1, MailProviderType: 3, Host: ko.observable('').extend({ required: { message: "[|Host is required|]" } }),
                    IsDefault: false, Email: ko.observable('').extend({ maxLength: 256, email: true }), MailChimpListID: "",
                    Port: ko.observable('').extend({ required: { message: "[|Port is required|]" } }), TextProviderID: 0
                });
            }
            else if (selectedValue == 1) {
                selfAccount.ServiceProviderRegistrationDetails.push({
                    UserName: ko.observable('').extend({ required: { message: "[|UserName is required|]" } }),
                    Password: ko.observable('').extend({ required: { message: "[|Password is required|]" } }),
                    ApiKey: "", SenderFriendlyName: "SMTP", MailProviderID: parseInt(selectedValue),
                    CommunicationType: 1, MailProviderType: 3, Host: ko.observable('').extend({ required: { message: "[|Host is required|]" } }),
                    IsDefault: false, Email: ko.observable('').extend({ required: { message: "[|Email is required|]" }, maxLength: 256, email: true }), MailChimpListID: "",
                    Port: ko.observable('').extend({ required: { message: "[|Port is required|]" } }), TextProviderID: 0
                });
            }
        }
    });

    selfAccount.SelectedServiceProviderID.subscribe(function (val) {
        var value = $.grep(selfAccount.emailServiceProviders(), function (e) { return e.ServiceProviderId.toString() == val.toString(); })[0];
        selfAccount.CampaignProviderType(value.Id);
    });

    selfAccount.TextProviderType.subscribe(function (selectedValue) {

        var provider = ko.utils.arrayFirst(selfAccount.ServiceProviderRegistrationDetails(), function (ServiceProvider) {
            if (ServiceProvider.TextProviderID == selectedValue && ServiceProvider.MailProviderType == 2)
                return ServiceProvider.TextProviderID == selectedValue;
        });
        var temporaryprovider = ko.utils.arrayFirst(selfAccount.ServiceProviderRegistrationDetails(), function (ServiceProvider) {
            if (
                ko.utils.unwrapObservable(ServiceProvider.UserName) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.Password) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.SenderFriendlyName) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.Email) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.ApiKey) == "" &&
                 ko.utils.unwrapObservable(ServiceProvider.SenderPhoneNumber) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.IsDefault) == false)
                return ServiceProvider;
        });
        if (temporaryprovider) {
            selfAccount.ServiceProviderRegistrationDetails.remove(temporaryprovider);
        }
        if (!provider) {
            selfAccount.ServiceProviderRegistrationDetails.push({
                UserName: ko.observable("").extend({ required: { message: "[|UserName is required|]" } }),
                Password: ko.observable("").extend({ required: { message: "[|Password is required|]" } }),
                ApiKey: ko.observable("").extend({ required: { message: "[|API Key is required|]" } }),
                SenderFriendlyName: "", LoginToken: ko.observable("").extend({ required: { message: "[|LoginToken is required|]" } }), MailProviderID: 0,
                SenderPhoneNumber: ko.observable("").extend({ required: { message: "[|PhoneNumber is required|]" }, number: { message: "[|Please Enter Number|]" }, digit: 0 }),
                TextProviderID: parseInt(selectedValue), CommunicationType: 2, MailProviderType: 2, IsDefault: false, Email: "",
                ServiceProviderID: 0
            });
        }
    });
    selfAccount.CampaignTextProviderType.subscribe(function (selectedValue) {

        var provider = ko.utils.arrayFirst(selfAccount.ServiceProviderRegistrationDetails(), function (ServiceProvider) {
            if (ServiceProvider.TextProviderID == selectedValue && ServiceProvider.MailProviderType == 3)
                return ServiceProvider.TextProviderID == selectedValue;
        });
        var temporaryprovider = ko.utils.arrayFirst(selfAccount.ServiceProviderRegistrationDetails(), function (ServiceProvider) {
            if (
                ko.utils.unwrapObservable(ServiceProvider.UserName) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.Password) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.SenderFriendlyName) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.Email) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.ApiKey) == "" &&
                ko.utils.unwrapObservable(ServiceProvider.IsDefault) == false)
                return ServiceProvider;
        });
        if (temporaryprovider) {
            selfAccount.ServiceProviderRegistrationDetails.remove(temporaryprovider);
        }

        if (!provider) {
            selfAccount.ServiceProviderRegistrationDetails.push({
                UserName: ko.observable("").extend({ required: { message: "[|UserName is required|]" } }),
                Password: ko.observable("").extend({ required: { message: "[|Password is required|]" } }),
                ApiKey: ko.observable("").extend({ required: { message: "[|API Key is required|]" } }),
                SenderFriendlyName: "", LoginToken: "", MailProviderID: 0,
                TextProviderID: parseInt(selectedValue), CommunicationType: 2, MailProviderType: 3, IsDefault: false, Email: ko.observable("").extend({ required: { message: "[|Email is required|]" }, maxLength: 256, email: true })
            });
        }
    });

    //*End
    selfAccount.enablePartnerType = ko.observable(false);
    selfAccount.PartnerType = ko.observable(data.PartnerType);

    selfAccount.LifecycleStage = ko.observable(data.LifecycleStage);
    selfAccount.LifecycleStage.subscribe(function (selectedValue) {
        if (selectedValue == 65) {
            selfAccount.enablePartnerType(true);
        }
        else {
            selfAccount.enablePartnerType(false);
        }
    });

    selfAccount.errors = ko.validation.group(selfAccount, { deep: true });
    if (mode == "E")
        selfAccount.saveText = ko.observable('[|Save Account|]');
    else
        selfAccount.saveText = ko.observable('[|Create Account|]');
    selfAccount.createText = ko.observable('[|Create Account|]');

    selfAccount.deleteAccount = function () {

        alertifyReset("Delete Account", "Cancel");
        alertify.confirm("[|Are you sure you want to permanently DELETE the selected Account(s)|]?", function (e) {

            if (e) {
                pageLoader();
                var aid = [selfAccount.AccountID()];
                var aname = [selfAccount.AccountName()];
                var jsondata = JSON.stringify({ 'AccountID': aid, 'StatusID': 6, 'AccountNames': aname });
                var varDeleteURL = "DeleteAccount";

                jQuery.support.cors = true;
                $.ajax({
                    url: accounturl + varDeleteURL,
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ 'accountData': jsondata })
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

                    notifySuccess("[|Successfully deleted the account|]");
                    if (pagename != "AccountSettings") {
                        removepageloader();
                        window.location.href = "/accounts";
                    }
                }).fail(function (error) {
                    removepageloader();
                    notifyError(error);
                });

            }
            else {
                notifyError("[|Delete Account requested canceled|]");
            }
        });
    };

    selfAccount.saveAccount = function (data) {
        selfAccount.errors.showAllMessages();

        var errorstring = "";
        var errors = unique(selfAccount.errors());

        for (var p = 0; p < errors.length; p++) {

            if (errors.length > 1)
                errorstring += "\u2022  " + errors[p] + ". </br>";
            else
                errorstring += errors[p] + ".";
        }

        if (selfAccount.errors().length > 0) {
            validationScroll();
            notifyError(errorstring);
            return;
        }

        if (pagename != "AccountSettings") {                //update TC if this page is not account-settings and only if add/edit account
            var termsAndCond = $("#termsEditor").redactor('code.get');
            var div = document.createElement('div');
            div.innerHTML = termsAndCond;
            var htmlContent = div.outerHTML;
            var plainData = JSON.stringify(htmlContent);
            if (/\S/.test(plainData)) {
                selfAccount.TC(htmlContent);
            }
            else { console.log("String is empty or of empty space"); selfAccount.TC(""); }
        }
        
        var jsondata = ko.toJSON(selfAccount);
        var action;
        if (selfAccount.AccountID() != null && selfAccount.AccountID() > 0 && mode == "E")
            action = "UpdateAccount";
        else
            action = "InsertAccount";

        if (data == "AccountSettings" || data == null || typeof data === 'undefined')
            action = "UpdateAccountSettings";

        if (selfAccount.ShowTC() && !selfAccount.TC()) {
            notifyError("[|Please enter Terms and conditions|]");
            return;
        }
        selfAccount.saveText('Saving..');
        pageLoader();
        $.ajax({
            url: accounturl + action,
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                'accountViewModel': jsondata
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
            selfAccount.saveText('Save Account');
            notifySuccess('[|Successfully saved the Account|]');
            if (pagename != "AccountSettings") {
                setTimeout(function () {
                    window.location.href = "/";
                }, setTimeOutTimer);
                //removepageloader();
            }
            else {
                // setTimeout(function () {
                window.location.reload();
                //}, 200);
            }
        }).fail(function (error) {
            if (mode == "E")
                selfAccount.saveText('[|Save Account|]');
            else
                selfAccount.saveText('[|Create Account|]');
            notifyError(error);
            removepageloader();
        });
    };

    selfAccount.providerErrors = ko.validation.group(selfAccount.ServiceProviderRegistrationDetails(), {
        deep: true
    });
    function CheckProviderValidation() {
        if (selfAccount.ServiceProviderRegistrationDetails().length > 0) {

            $.each(selfAccount.ServiceProviderRegistrationDetails(), function (index, value) {
                selfAccount.providerErrors = ko.validation.group(value);
            });
        }
    }


    function unique(list) {
        var result = [];
        $.each(list, function (i, e) {
            if ($.inArray(e, result) == -1) result.push(e);
        });
        return result;
    } 

    selfAccount.DisclaimerClick = function (value) {
        if (value.Disclaimer() === true)
            selfAccount.Disclaimer(value.Disclaimer());
        else
            selfAccount.Disclaimer(value.Disclaimer());
    }

    selfAccount.saveAccountSettings = function () {
        var tServiceProvider = ko.utils.arrayFirst(selfAccount.ServiceProviderRegistrationDetails(), function (item) {
            return item.MailProviderType == 2 && (item.IsDefault == true || item.IsDefault == 'true');
        });

        if (tServiceProvider == null || tServiceProvider.length == 0) {
            notifyError("[|Please add aleast one default service provider for sending transactional emails|]");
            return;
        }

        CheckProviderValidation();
        selfAccount.providerErrors.showAllMessages();
        selfAccount.errors.showAllMessages();

        var errorstring = "";

        var totalerrors = (selfAccount.providerErrors()).concat(selfAccount.errors());

        var errors = unique(totalerrors);

        for (var p = 0; p < errors.length ; p++) {
            if (errors.length > 1)
                errorstring += "\u2022  " + errors[p] + ". </br>";
            else
                errorstring += errors[p] + ".";

        }

        if (selfAccount.providerErrors().length > 0) {
            notifyError(errorstring);
            return;
        }

        if (selfAccount.errors().length > 0) {
            notifyError(errorstring);
            return;
        }
        var jsondata = ko.toJSON(selfAccount);

        var action = "UpdateAccountSettings";

        selfAccount.saveText('[|Saving|]..');
        pageLoader();

        $.ajax({
            url: accounturl + action,
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                'accountViewModel': jsondata
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

            selfAccount.saveText('Save Account');
            notifySuccess('Successfully saved the Account');
            setTimeout(function () {
                removepageloader();
                window.location.reload();
            }, setTimeOutTimer);
        }).fail(function (error) {
            removepageloader();
            if (mode == "E")
                selfAccount.saveText('[|Save Account|]');
            else
                selfAccount.saveText('[|Create Account|]');
            notifyError(error);
        });
    };

    selfAccount.suspendAccount = function () {

        alertifyReset("Delete Account", "Cancel");
        alertify.confirm("[|Are you sure you want to Pause this Account|]?", function (e) {
            if (e) {
                pageLoader();
                var aid = [selfAccount.AccountID()];
                var aname = [selfAccount.AccountName()];
                var jsondata = JSON.stringify({
                    'AccountID': aid, 'StatusID': 3, 'AccountNames': aname
                });
                var varDeleteURL = "DeleteAccount";

                jQuery.support.cors = true;
                $.ajax({
                    url: accounturl + varDeleteURL,
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({
                        'accountData': jsondata
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

                    notifySuccess("[|Successfully Paused the account|]");
                    removepageloader();
                    if (pagename != "AccountSettings") {
                        window.location.href = "/accounts";
                    }
                }).fail(function (error) {
                    removepageloader();
                    notifyError(error);
                });

            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }
        });
    };

    selfAccount.terminateAccount = function () {

        alertifyReset("Delete Account", "Cancel");
        //alertify.confirm("Are you sure you want to Close this Account?", function (e) {
        alertify.confirm(" [|Are you sure you want to close the selected Account(s)|]?", function (e) {
            if (e) {
                pageLoader();
                var aid = [selfAccount.AccountID()];
                var aname = [selfAccount.AccountName()];
                var jsondata = JSON.stringify({
                    'AccountID': aid, 'StatusID': 4, 'AccountNames': aname
                });
                var varDeleteURL = "DeleteAccount";

                jQuery.support.cors = true;
                $.ajax({
                    url: accounturl + varDeleteURL,
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({
                        'accountData': jsondata
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

                    notifySuccess("[|Successfully Closed the account|]");

                    if (pagename != "AccountSettings") {

                        setTimeout(
                               function () {
                                   removepageloader();
                                   window.location.href = "/accounts";
                               }, setTimeOutTimer);

                        //window.location.href = "/accounts"
                    }
                }).fail(function (error) {
                    removepageloader();
                    notifyError(error);
                });

            }
            else {
                //notifyError("You've clicked Cancel");
                notifyError("[|Close Account requested canceled|]");
            }
        });
    };

    selfAccount.CurrencyID.subscribe(function (selectedValue) {
        if (selectedValue == 1 || selectedValue == 2) {
            selfAccount.CurrencyFormat("$X,XXX.XX");
        }
        else if (selectedValue == 3) {
            selfAccount.CurrencyFormat("X XXX,XX $");
        }
        else if (selectedValue == 5) {
            selfAccount.CurrencyFormat("₹X,XXX.XX");
        }
        else {
            selfAccount.CurrencyFormat("B/.X,XXX.XX");
        }
    });

    selfAccount.OppCustClick = function (module) {
        module.IsSelected(true);
        var subConfId = '#confmainyes' + module.ModuleId();
        radiobtnActive(subConfId);
        return true;
    };

    selfAccount.ParentClicked = function (module) {
        if (module !== null) {
            if (module.SubModules().length > 0) {
                $.each(module.SubModules(), function (index, item) {
                    item.IsSelected(module.IsSelected());
                    item.IsPrivate(module.IsPrivate());
                    var subConfId;
                    var subsharedId;
                    if (module.IsSelected() === 'true' || module.IsPrivate() === 'false') {
                        subConfId = '#confsubyes' + item.ModuleId();
                        subsharedId = '#subshared' + item.ModuleId();
                        radiobtnActive(subConfId);
                        radiobtnActive(subsharedId);
                    }
                    else {
                        subConfId = '#confsubno' + item.ModuleId();
                        subsharedId = '#subprivate' + item.ModuleId();
                        radiobtnActive(subConfId);
                        radiobtnActive(subsharedId);
                    }
                });
            }
        }
        return true;
    };

    selfAccount.ChildClicked = function (module, subModule) {
        if (subModule.IsSelected() === 'true') {
            module.IsSelected(true);
            var subConfId = '#confmainyes' + module.ModuleId();
            radiobtnActive(subConfId);
        }
        return true;
    };

    selfAccount.sharingChildClick = function (module, subModule) {
        if (subModule.IsPrivate() === 'false' && module.IsPrivate() === 'true') {
            setTimeout(function () {
                subModule.IsPrivate(false);
                var subConfId = '#subprivate' + subModule.ModuleId();
                radiobtnActive(subConfId);
            }, 100);
            return null;
        }
        return true;
    };

    selfAccount.sharingParentClick = function (module) {
        if (module.IsPrivate() === 'false') {
            $('#sharingfieldset').prop('disabled', false);
        }
        else {
            $('#sharingfieldset').prop('disabled', true);
        }
        return true;
    };

    selfAccount.ImageDomains = ko.observableArray(data.ImageDomains);

    selfAccount.TC = ko.observable(data.TC || "<!DOCTYPE html>	<title>Terms & Conditions - SmartTouch Interactive</title></head><body class='page page-id-3089 page-template-default'><div id='subnav'><div class='container'><div class='col-xs-12 subnav'></div></div></div>	</header><div class='container'>    <div class='row p30'>        <div class='content col-xs-12'>            <h1>Privacy Policy</h1><h2>What information do we collect?</h2><p>We collect information from you when you register on our site, place an order, subscribe to our newsletter, respond to a survey or fill out a form.</p><p>When ordering or registering on our site, as appropriate, you may be asked to enter your: name, email address, mailing address or phone number. You may, however, visit our site anonymously.</p><p>Google, as a third party vendor, uses cookies to serve ads on your site. Google&#8217;s use of the DART cookie enables it to serve ads to your users based on their visit to your sites and other sites on the Internet. Users may opt out of the use of the DART cookie by visiting the Google ad and content network privacy policy.</p><h2>What do we use your information for?</h2><p>Any of the information we collect from you may be used in one of the following ways:</p><ul><li>To personalize your experience(your information helps us to better respond to your individual needs)</li><li>To improve our website(we continually strive to improve our website offerings based on the information and feedback we receive from you)</li><li>To improve customer service(your information helps us to more effectively respond to your customer service requests and support needs)</li><li>To administer a contest, promotion, survey or other site feature</li><li>To send periodic emails</li></ul><p>The email address you provide for order processing, will only be used to send you information and updates pertaining to your order.</p><p>If you decide to opt-in to our mailing list, you will receive emails that may include company news, updates, related product or service information, etc.</p><p>Note: If at any time you would like to unsubscribe from receiving future emails, we include detailed unsubscribe instructions at the bottom of each email.</p><h2>How do we protect your information?</h2><p>We implement a variety of security measures to maintain the safety of your personal information when you place an order or access your personal information.</p><p>We offer the use of a secure server. All supplied sensitive/credit information is transmitted via Secure Socket Layer (SSL) technology and then encrypted into our Database to be only accessed by those authorized with special access rights to our systems, and are required to=EF=BF=BDkeep the information confidential.</p><p>After a transaction, your private information (credit cards, social security numbers, financials, etc.) will not be stored on our servers.</p><h2>Do we use cookies?</h2><p>Yes (Cookies are small files that a site or its service provider transfers to your computers hard drive through your Web browser (if you allow) that enables the sites or service providers systems to recognize your browser and capture and remember certain information</p><p>We use cookies to understand and save your preferences for future visits and compile aggregate data about site traffic and site interaction so that we can offer better site experiences and tools in the future.</p><p>If you prefer, you can choose to have your computer warn you each time a cookie is being sent, or you can choose to turn off all cookies via your browser settings. Like most websites, if you turn your cookies off, some of our services may not function properly. However, you can still place orders over the telephone or by contacting customer service.</p><h2>Do we disclose any information to outside parties?</h2><p>We do not sell, trade, or otherwise transfer to outside parties your personally identifiable information. This does not include trusted third parties who assist us in operating our website, conducting our business, or servicing you, so long as those parties agree to keep this information confidential. We may also release your information when we believe release is appropriate to comply with the law, enforce our site policies, or protect ours or others rights, property, or safety. However, non-personally identifiable visitor information may be provided to other parties for marketing, advertising, or other uses.</p><h2>Third party links</h2><p>Occasionally, at our discretion, we may include or offer third party products or services on our website. These third party sites have separate and independent privacy policies. We therefore have no responsibility or liability for the content and activities of these linked sites. Nonetheless, we seek to protect the integrity of our site and welcome any feedback about these sites.</p><h2>California Online Privacy Protection Act Compliance</h2><p>Because we value your privacy we have taken the necessary precautions to be in compliance with the California Online Privacy Protection Act. We therefore will not distribute your personal information to outside parties without your consent.</p><h2>Childrens Online Privacy Protection Act Compliance</h2><p>We are in compliance with the requirements of COPPA (Childrens Online Privacy Protection Act), we do not collect any information from anyone under 13 years of age. Our website, products and services are all directed to people who are at least 13 years old or older.</p><h2>Online Privacy Policy Only</h2><p>This online privacy policy applies only to information collected through our website and not to information collected offline.</p><h2>Terms and Conditions</h2><p>Please also visit our Terms and Conditions section establishing the use, disclaimers, and limitations of liability governing the use of our website at <a href='http://www.smarttouchinteractive.com'>www.smarttouchinteractive.com</a></p><h2>Your Consent</h2><p>By using our site, you consent to our <a href='http://www.smarttouchinteractive.com/privacy-policy.php'>web site privacy policy.</a></p><h2>Changes to our Privacy Policy</h2><p>If we decide to change our privacy policy, we will post those changes on this page.</p><h2>Contacting Us</h2><p>If there are any questions regarding this privacy policy you may contact us using the information below.</p><p>	<a href='http://www.smarttouchinteractive.com'>www.smarttouchinteractive.com</a><br />	4833 Spicewood Springs Road<br />	Suite 102<br />	Austin, Texas 78759<br />USA<br />Phone: (512) 333-4008<br />	<a href='mailto:support@smarttouchinteractive.com'>support@smarttouchinteractive.com</a></p></div></div></div></body></html>");

    if (selfAccount.TC() != 'undefined' && selfAccount.TC() != null) {
        $("#termsEditor").redactor('code.set', selfAccount.TC());
    }
}


