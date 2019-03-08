var personViewModel = function (data, countries, url, newAddress, DropDownURL, ImagePath, Users) {
    selfContact = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfContact));

    var DATATYPE_CHECKBOX = 1;
    var DATATYPE_MULTISELECT = 12;
    var DATATYPE_DATE = 13;
    var DATATYPE_TIME = 9;

    selfContact.Users = ko.observableArray();

    if (selfContact.ContactID() == 0) {
        Users = ko.utils.arrayFilter(Users, function (usr) {
            return usr.IsDeleted == false;
        });
    }
    selfContact.Users(Users);

    selfContact.templ = kendo.template('#if (data.OwnerId == 0 )  { # <span>[|Select..|]</span># } ##if (data.IsDeleted == false )  { # <span>#= OwnerName #</span># } # #if (data.IsDeleted == true )  { # <span style="color:red;">#= OwnerName #</span># } #');

    selfContact.CurrentView = ko.observable('generalfields');
    selfContact.CurrentTabView = ko.observable(0);

    selfContact.showGeneralFields = function () {
        selfContact.CurrentView('generalfields');
    }

    selfContact.EmailExist = ko.observable(false);
    selfContact.NameExist = ko.observable(false);
    selfContact.HasPermission = ko.observable(true);
    selfContact.ContactEmail = ko.observable();
    selfContact.ContactName = ko.observable();
    selfContact.DuplicateContactId = ko.observable();
    selfContact.ContactType = ko.observable("1");

    ko.validation.rules['validationforEmail'] = {
        validator: function (emailId) {
            if (emailId == null) {
                return false;
            }
        },
        message: 'required'
    };

    selfContact.showCustomFields = function () {
        selfContact.CurrentView('customfields');
    }

    selfContact.showCurrentTab = function (tabIndex) {
        selfContact.CurrentTabView(tabIndex);
    }

    selfContact.CustomFields = ko.observableArray(data.CustomFields);
    $.each(selfContact.CustomFieldTabs(), function (index, tab) {
        $.each(tab.Sections(), function (sectionIndex, section) {
            $.each(section.CustomFields(), function (fieldIndex, field) {

                var customFieldIndex = selfContact.CustomFields().map(function (e) { return e.CustomFieldId; }).indexOf(parseInt(field.FieldId()));

                if (customFieldIndex >= 0) {

                    if (field.FieldInputTypeId() == DATATYPE_DATE) {
                        if (selfContact.CustomFields()[customFieldIndex].Value != "") {
                            var dateFormat = readCookie("dateformat").toUpperCase();
                            field.Value(kendo.toString(new Date(moment(selfContact.CustomFields()[customFieldIndex].Value_Date).toDate()).ToUtcUtzDate()), readCookie("dateformat").toUpperCase());
                        }
                    }
                    else if (field.FieldInputTypeId() == DATATYPE_TIME) {
                        if (selfContact.CustomFields()[customFieldIndex].Value_Date) {
                            field.Value(kendo.toString(new Date(moment(selfContact.CustomFields()[customFieldIndex].Value_Date).toDate()).ToUtcUtzDate(), "t"));
                        }
                    }
                    else if (field.FieldInputTypeId() == DATATYPE_CHECKBOX || field.FieldInputTypeId() == DATATYPE_MULTISELECT) {
                        field.Value(selfContact.CustomFields()[customFieldIndex].Value.split("|"));
                    }
                    else
                        field.Value(selfContact.CustomFields()[customFieldIndex].Value);
                }
                else {
                    if (field.FieldInputTypeId() == DATATYPE_CHECKBOX) {
                        field.Value([]);
                    }
                    else
                        field.Value("");
                }
            })
        })
    })
    selfContact.MapCustomFields = function () {
        $.each(selfContact.CustomFieldTabs(), function (index, tab) {
            $.each(tab.Sections(), function (sectionIndex, section) {

                $.each(section.CustomFields(), function (fieldIndex, field) {
                    var customFieldIndex = selfContact.CustomFields().map(function (e) { return e.CustomFieldId; }).indexOf(parseInt(field.FieldId()));

                    //If the value is empty then remove it from the array.
                    if (customFieldIndex != -1 && (field.Value() == "" || field.Value() == null)) {
                        selfContact.CustomFields().splice(customFieldIndex, 1);
                    }
                    else if (customFieldIndex == -1 && (field.Value() != "" || field.Value() != null)) {
                        if (field.FieldInputTypeId() == DATATYPE_CHECKBOX || field.FieldInputTypeId() == DATATYPE_MULTISELECT) {

                            selfContact.CustomFields.push({
                                ContactCustomFieldMapId: 0,
                                ContactId: selfContact.ContactID,
                                CustomFieldId: field.FieldId(),
                                Value: field.Value() == "" ? "" : field.Value().join('|'),
                                FieldInputTypeId: field.FieldInputTypeId()
                            })
                            field.Value(field.Value() == "" ? "" : field.Value().join('|'));
                        }
                        else if (field.FieldInputTypeId() == DATATYPE_DATE) {
                            selfContact.CustomFields.push({
                                ContactCustomFieldMapId: 0,
                                ContactId: selfContact.ContactID,
                                CustomFieldId: field.FieldId(),
                                Value: $('#' + field.FieldId()).val() == "" ? "" : $('#' + field.FieldId()).val(),
                                FieldInputTypeId: field.FieldInputTypeId()
                            })
                            field.Value($('#' + field.FieldId()).val() == "" ? "" : $('#' + field.FieldId()).val());

                        }
                        else if (field.FieldInputTypeId() == DATATYPE_TIME) {
                            selfContact.CustomFields.push({
                                ContactCustomFieldMapId: 0,
                                ContactId: selfContact.ContactID,
                                CustomFieldId: field.FieldId(),
                                Value: $('#' + field.FieldId()).val() == "" ? "" : $('#' + field.FieldId()).val(),
                                FieldInputTypeId: field.FieldInputTypeId()
                            })
                            field.Value($('#' + field.FieldId()).val() == "" ? "" : $('#' + field.FieldId()).val());
                        }
                        else {
                            selfContact.CustomFields.push({
                                ContactCustomFieldMapId: 0,
                                ContactId: selfContact.ContactID,
                                CustomFieldId: field.FieldId(),

                                Value: field.Value(),
                                FieldInputTypeId: field.FieldInputTypeId()
                            })
                        }
                    }
                    else if (customFieldIndex != -1) {
                        if (field.FieldInputTypeId() == DATATYPE_DATE) {
                            selfContact.CustomFields()[customFieldIndex].Value = $('#' + field.FieldId()).val() != null ? $('#' + field.FieldId()).val() : "";
                            selfContact.CustomFields()[customFieldIndex].FieldInputTypeId = field.FieldInputTypeId();
                            field.Value($('#' + field.FieldId()).val() == "" ? "" : $('#' + field.FieldId()).val());
                        }
                        else if (field.FieldInputTypeId() == DATATYPE_TIME) {
                            selfContact.CustomFields()[customFieldIndex].Value = $('#' + field.FieldId()).val() != null ? $('#' + field.FieldId()).val() : "";

                            selfContact.CustomFields()[customFieldIndex].FieldInputTypeId = field.FieldInputTypeId();
                            field.Value($('#' + field.FieldId()).val() == "" ? "" : $('#' + field.FieldId()).val());
                        }
                        else if (field.FieldInputTypeId() == DATATYPE_CHECKBOX || field.FieldInputTypeId() == DATATYPE_MULTISELECT) {

                            if (field.Value().indexOf('|') == -1) {
                                selfContact.CustomFields()[customFieldIndex].Value = field.Value() ? field.Value().join('|') : "";
                                selfContact.CustomFields()[customFieldIndex].FieldInputTypeId = field.FieldInputTypeId();
                                field.Value(field.Value() == "" ? "" : field.Value().join('|'));
                            }
                        }
                        else {
                            selfContact.CustomFields()[customFieldIndex].FieldInputTypeId = field.FieldInputTypeId();
                            selfContact.CustomFields()[customFieldIndex].Value = field.Value();
                        }
                    }
                    field.Value(field.Value() != null ? field.Value().toString() : "");
                })
            })
        })
    }

    selfContact.Dateformat = function () {
        return readCookie("dateformat");
    }

    selfContact.primaryEmail = ko.observable();

    selfContact.EmailStatuses = ko.observableArray([
        { Id: 50, Type: '[|Not Verified|]' },
        { Id: 51, Type: '[|Verified|]' },
        { Id: 52, Type: '[|Soft Bounce|]' },
        { Id: 53, Type: '[|Hard Bounce|]' },
        { Id: 54, Type: '[|Unsubscribed|]' },
        { Id: 56, Type: '[|Complained|]' },
        { Id: 57, Type: '[|Suppressed|]'}
    ]);


    selfContact.Emails = ko.observableArray(data.Emails);
    $.each(selfContact.Emails(), function (index, email) {
        email.EmailStatusValue = ko.observable(email.EmailStatusValue);
        email.EmailId = ko.observable(email.EmailId);
        if (email.IsPrimary === true) {
            selfContact.primaryEmail(email)
        }

        email.IsPrimary = ko.pureComputed(function () {
            return email === selfContact.primaryEmail()
        });
    });
    selfContact.FirstName = ko.observable(data.FirstName).extend({
        required: {
            message: "[|First Name is required|]",
            onlyIf: function () {
                return selfContact.primaryEmail == undefined || selfContact.primaryEmail() == undefined || selfContact.primaryEmail == null || selfContact.primaryEmail().EmailId() == null || selfContact.primaryEmail().EmailId() == "";
            }
        },
        maxLength: 75
    });
    selfContact.LastName = ko.observable(data.LastName).extend({
        required: {
            message: "[|Last Name is required|]",
            onlyIf: function () {
                return selfContact.primaryEmail == undefined || selfContact.primaryEmail() == undefined || selfContact.primaryEmail == null || selfContact.primaryEmail().EmailId() == null || selfContact.primaryEmail().EmailId() == "";
            }
        },
        maxLength: 75
    });

    selfContact.Title = ko.observable(data.Title).extend({ maxLength: 100 });
    selfContact.CompanyName = ko.observable(data.CompanyName).extend({ maxLength: 75 });
    selfContact.CompanyData = ko.observableArray();
    selfContact.CompanyID = ko.observable(data.CompanyID);
    selfContact.IncludeInReports = ko.observable(data.IncludeInReports);

    selfContact.CompanyName.subscribe(function (text) {
        var company = $(selfContact.CompanyData()).filter(function () {
            return this.Text.toString().toLowerCase() == text.toString().toLowerCase();
        })[0];
        if (company != null) {
            selfContact.CompanyID = ko.observable(company.DocumentId);
        }
        else {
            selfContact.CompanyID = ko.observable();
        }
    });

    selfContact.ContactImageUrl = ko.observable(data.ContactImageUrl);
    selfContact.SecondaryLeadSources = ko.observableArray([]);
    selfContact.LeadSources = ko.observableArray(data.LeadSources);
    selfContact.LifecycleStages = ko.observableArray(data.LifecycleStages);
    ko.bindingHandlers.multiselect = {
        init: function (element, valueAccessor) {
            // selectedRoles
            var value = valueAccessor();
            // Create the mutliselect
            ko.applyBindingsToNode(element, {
                kendoMultiSelect: {
                    value: value,
                    dataTextField: 'DropdownValue',
                    dataValueField: 'DropdownValueID',
                    data: selfContact.SecondaryLeadSources,
                    autoClose: false
                }
            });

            var src = $(element).data('kendoMultiSelect').dataSource.data();
            var selected = $.grep(src, function (e, i) {
                return ko.utils.arrayFilter(value(), function (item) {
                    return item.DropdownValueID == e.DropdownValueID;
                }).length > 0;
            });
            value(selected);
        }
    }

    selfContact.IsFirstLeadSourceVisible = ko.observable(data.ContactID === 0 ? true : false);

    var dataItem = null;
    var oldSource = null;
    var selectedsources = [];
    selfContact.ContactSourceSelect = function (e) {
        dataItem = this.dataItem(e.item);
        oldSource = selfContact.ContactSourceTypeID();
        selectedsources = selfContact.ContactSecondarySource();
        openModal();
    };

    function leadSourceOperation() {
        if (dataItem != null) {
            secondaryleadsources = getSecondaryLeadSources(dataItem.DropdownValueID);
            if (dataItem.DropdownValueID.toString() != oldSource) {
                selfContact.SecondaryLeadSources(secondaryleadsources);
            }

            var leadsourceindex = selectedsources.map(String).indexOf(dataItem.DropdownValueID.toString());
            if (leadsourceindex != -1) {
                selectedsources.splice(leadsourceindex, 1);
                if (oldSource != "") {
                    if (selectedsources.map(String).indexOf(oldSource.toString() < 0))
                        selectedsources.unshift(oldSource);
                }
                setTimeout(function () {
                    selfContact.ContactSecondarySource(selectedsources);
                }, 100);
            }
            else {
                if (oldSource != "" && dataItem.DropdownValueID.toString() != oldSource) {
                    if (selectedsources.map(String).indexOf(oldSource.toString() < 0))
                        selectedsources.unshift(oldSource);
                    setTimeout(function () {
                        selfContact.ContactSecondarySource(selectedsources);
                    }, 100);
                }
            }
        }
    };

    $('#leadSourceModal').on('hidden.bs.modal', function (event) {
        leadSourceOperation();
    });

    function openModal() {
        if (selfContact.ContactID() != null && selfContact.ContactID() > 0)
            $("#leadSourceModal").modal({ show: true, keyboard: false, backdrop: 'static' });
        else
            leadSourceOperation();
    }

    selfContact.changeLeadSourceVisibility = function(data) {
        selfContact.IsFirstLeadSourceVisible(true);
        selfContact.IsFirstLeadSourceVisible.valueHasMutated();
        $("#leadSourceModal").modal({ show: true, keyboard: false, backdrop: 'static'});
        return true;
    };

    var selectedleadsrc = data.SelectedLeadSource;
    var selectedcontactsourceid;
    if (selectedleadsrc != null) {
        selectedcontactsourceid = $.map(selectedleadsrc, function (n, i) {
            return n.DropdownValueID;
        });
    }

    selfContact.ContactSourceTypeID = ko.observable("");
    selfContact.ContactSecondarySource = ko.observable();

    if (selfContact.ContactID() > 0 && selectedcontactsourceid && selectedcontactsourceid.length > 0) {

        selfContact.ContactSourceTypeID(selectedcontactsourceid[0]);
        var secondaryleadsources = getSecondaryLeadSources(selectedcontactsourceid[0]);
        selfContact.SecondaryLeadSources(secondaryleadsources);
        selectedcontactsourceid.shift();
        setTimeout(function () {
            selfContact.ContactSecondarySource(selectedcontactsourceid);
        }, 500);

    } else {
        selfContact.SecondaryLeadSources(selfContact.LeadSources());
    }

    selfContact.ContactSourceValidation = selfContact.ContactSourceTypeID.extend({
        required: {
            message: "[|Contact Source is required|]"
        }
    });
    selfContact.SelectedLeadSource = ko.observableArray(data.SelectedLeadSource);

    selfContact.SSN = ko.observable(data.SSN).extend({
        pattern: { params: "(^[0-9]{3}-? *?[0-9]{2}-? *?[0-9]{4}$)|(^[0-9]{3}-? *?[0-9]{3}-? *?[0-9]{3}$)|(^[0-9]{9}$)", message: "[|Enter a valid 9 digit SSN or SIN|]" }
    });

    selfContact.PhoneTypes = ko.observableArray(data.PhoneTypes);

    selfContact.Phones = ko.observableArray(data.Phones);
    selfContact.primaryPhone = ko.observable();
    selfContact.primaryPhone.subscribe(function (item) {

    });

    selfContact.OwnerId = ko.observable(data.OwnerId);
    selfContact.PreviousOwnerId = ko.observable(data.OwnerId);

    var phoneTypes = [];
    var defaultPhoneTypeIndex = ko.utils.arrayFirst(selfContact.PhoneTypes(), function (type) {
        return type.IsDefault === true;
    });

    phoneTypes.push(defaultPhoneTypeIndex);
    for (var p = 0; p < (selfContact.PhoneTypes()).length; p++) {
        if (selfContact.PhoneTypes()[p].IsDefault != true)
            phoneTypes.push(selfContact.PhoneTypes()[p]);
    }

    $.each(selfContact.Phones(), function (index, phone) {

        var type = ko.utils.arrayFirst(selfContact.PhoneTypes(), function (type) {
            return type.DropdownValueID == phone.PhoneType;
        });
        if(type)
            phone.PhoneType = ko.observable(phone.PhoneType);
        else
            phone.PhoneType = ko.observable(defaultPhoneTypeIndex.DropdownValueID);
        phone.Number = ko.observable(phone.Number);
        phone.PhoneTypeName = ko.observable(phone.PhoneTypeName);
        phone.CountryCode = ko.observable(phone.CountryCode);
        phone.Extension = ko.observable(phone.Extension);
        if (phone.IsPrimary === true) {
            selfContact.primaryPhone(phone);
            var phoneId = '#primaryPhone' + index
            setTimeout(function () {
                $(phoneId).closest('label.radio').addClass('checked');
            }, 200);
        }

        phone.IsPrimary = ko.pureComputed(function () {
            return phone === selfContact.primaryPhone()
        });
        phone.PhoneType.subscribe(function (newValue) {
            var phoneType = ko.utils.arrayFirst(selfContact.PhoneTypes(), function (type) {
                return type.DropdownValueID == newValue;
            });
            phone.PhoneTypeName(phoneType.DropdownValue);
        });
    });

    //selfContact.DefaultPhoneIndex = ko.observable();

    var index = 0;
    selfContact.addPhone = function () {
        var defaultindex = ko.utils.arrayFirst(phoneTypes, function (type) {
            return type.DropdownValueID == selfContact.Phones()[selfContact.Phones().length - 1].PhoneType();
        });
        index = phoneTypes.indexOf(defaultindex);
        index++;
        if (index >= phoneTypes.length) {
            index = 0;
        }
        var newPhone = {
            PhoneType: ko.observable(phoneTypes[index].DropdownValueID),
            PhoneTypeName: ko.observable(phoneTypes[index].DropdownValue),
            CountryCode : ko.observable(),
            Extension : ko.observable(),
            Number: ko.observable(),
            ContactPhoneNumberID: 0
        };
        newPhone.IsPrimary = ko.pureComputed(function () {
            return newPhone === selfContact.primaryPhone()
        });
        newPhone.PhoneType.subscribe(function (newValue) {
            var phoneType = ko.utils.arrayFirst(phoneTypes, function (type) {
                return type.DropdownValueID == newValue;
            });
            newPhone.PhoneTypeName(phoneType.DropdownValue);
        });
        selfContact.Phones.push(newPhone);
    };

    selfContact.removePhone = function () {
        if (index != 0) index--;
        else index = phoneTypes.length - 1;
        selfContact.Phones.remove(this);
    };

    selfContact.Addresses = ko.observableArray(data.Addresses);
    selfContact.primaryAddress = ko.observable();
    var addressIndex = -1;
    var newIndex = false;
    selfContact.addAddress = function () {
        var defaultAddressTypeIndex = ko.utils.arrayFirst(selfContact.AddressTypes(), function (type) {
            return type.IsDefault === true;
        });
        if (addressIndex != -1 && selfContact.AddressTypes()[addressIndex].IsDefault == true && newIndex == true) {
            addressIndex = 0;
            newIndex = false;
        }
        else {
            addressIndex++;
        }
        if (addressIndex == selfContact.AddressTypes().length) {
            newIndex = true;

            addressIndex = selfContact.AddressTypes().indexOf(defaultAddressTypeIndex);
        }
        if (selfContact.AddressTypes()[addressIndex].IsDefault == true && newIndex == false) {
            addressIndex++;
            if (addressIndex == selfContact.AddressTypes().length) {
                newIndex = true;
                addressIndex = selfContact.AddressTypes().indexOf(defaultAddressTypeIndex);
            }
        }

        //Returns a copy of newAddress
        selfContact.getAddressTemplate = function () {
            var addressTemplate = jQuery.extend(true, {}, JSON.parse(newAddress));
            addressTemplate.Countries = countries;
            addressTemplate.States = ko.observableArray();
            addressTemplate.Country.Code = ko.observable("");
            addressTemplate.State.Code = ko.observable();
            addressTemplate.AddressTypeID = ko.observable(selfContact.AddressTypes()[addressIndex].DropdownValueID);
            addressTemplate.IsDefault = ko.observable(selfContact.AddressTypes()[addressIndex].IsDefault);
            addressTemplate.countrySelect = function (e) {
                addressTemplate.State.Code('');
            }
            return addressTemplate;
        };
        var newAddressCopy = selfContact.getAddressTemplate();

        newAddressCopy.Country.Code.subscribe(function (selectedCountry) {
            selfContact.countryChanged(newAddressCopy);
        });
        newAddressCopy.IsDefault = ko.pureComputed(function () {
            return newAddressCopy === selfContact.primaryAddress()
        });
        selfContact.countryChanged(newAddressCopy);
        selfContact.Addresses.push(newAddressCopy);
    };


    selfContact.removeAddress = function () {
        selfContact.Addresses.remove(this);
    };

    selfContact.addEmail = function () {
        var emailStatuses = selfContact.EmailStatuses();
        var newEmail = {
            EmailStatusValue: ko.observable(50),
            EmailId: ko.observable(''),
            ContactEmailID: 0
        };
        newEmail.IsPrimary = ko.pureComputed(function () {
            return newEmail === selfContact.primaryEmail()
        });
        selfContact.Emails.push(newEmail);
    };

    selfContact.removeEmail = function () {
        selfContact.Emails.remove(this);
    };

    selfContact.SocialMediaUrlTypes = ko.observableArray(["[|Website|]", "[|LinkedIn|]", "[|Facebook|]", "[|Twitter|]", "[|Google+|]", "[|Blog|]"]);
    selfContact.tempUrlTypes = ko.observableArray(["[|Website|]", "[|LinkedIn|]", "[|Facebook|]", "[|Twitter|]", "[|Google+|]", "[|Blog|]"]);
    selfContact.SocialMediaUrls = ko.observableArray(data.SocialMediaUrls);

    ko.utils.arrayFilter(selfContact.SocialMediaUrls(), function (url) {
        selfContact.tempUrlTypes.remove(url.MediaType);
    });

    var index = 0;
    selfContact.widget = ko.observable();
    selfContact.addSocialMediaUrl = function () {
        selfContact.SocialMediaUrls.push({ MediaType: ko.observable(selfContact.tempUrlTypes()[0]), URL: ko.observable('') });
        $("select[name='mediumdf']").selectpicker({ style: 'btn-default' });
        selfContact.tempUrlTypes.remove(selfContact.tempUrlTypes()[0]);
    };

    selfContact.removeSocialMediaUrl = function () {
        selfContact.tempUrlTypes.push(this.MediaType());
        selfContact.SocialMediaUrls.remove(this);
    };

    selfContact.updateURLType = function (urlType) {
        selfContact.tempUrlTypes.remove(selfContact.widget()._selectedValue);
        if (selfContact.tempUrlTypes().map(function (e) { return e }).indexOf(urlType()) == -1) {
            selfContact.tempUrlTypes.push(urlType());
        }
    };

    selfContact.AddressTypes = ko.observableArray(data.AddressTypes);
    //Gets states of the selected country.
    selfContact.countryChanged = function (address) {
        $.ajax({
            url: url + 'GetStates',
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: { 'countryCode': address.Country.Code() }
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            address.States(data.response);
            var code = address.State.Code();
            address.State.Code("");
            address.State.Code(code);
        }).fail(function (error) {
            notifyError(error);
        })
    };

    selfContact.DoNotEmail = ko.observable(data.DoNotEmail);
    selfContact.setDoNotEmail = function (data, event) {
        selfContact.DoNotEmail(event.target.checked);
    }

    $.each(selfContact.Addresses(), function (index, address) {
        address.AddressTypeID = ko.observable(address.AddressTypeID);
        address.States = ko.observableArray(address.States);
        address.Country.Code = ko.observable(address.Country.Code);
        selfContact.countryChanged(address);
        address.State.Code = ko.observable(address.State.Code);
        address.IsDefault = ko.observable(address.IsDefault);
        address.Country.Code.subscribe(function (selectedCountry) {
            selfContact.countryChanged(address);
        });
        address.countrySelect = function (e) {
            address.State.Code('');
        };
        address.Countries = countries;

        if (address.IsDefault() === true) {
            selfContact.primaryAddress(address);
        }
        address.IsDefault = ko.pureComputed(function () {
            return address === selfContact.primaryAddress()
        });
    });
    selfContact.enablePartnerType = ko.observable(false);
    selfContact.PartnerType = ko.observable(data.PartnerType);
    selfContact.showPartner = ko.observable();
    selfContact.LifecycleStage = ko.observable(data.LifecycleStage);
    selfContact.PreviousLifecycleStage = ko.observable(data.LifecycleStage);
    var initiallifecyclestage = data.LifecycleStage;
    selfContact.IsLifecycleChanged = ko.observable(false);
    selfContact.LifeCycleStageSelect = function (e) {
        var dataItem = this.dataItem(e.item);
        if (selfContact.ContactID() == 0) {
            selfContact.IsLifecycleChanged(false);
        } else if (selfContact.ContactID() > 0 && dataItem.DropdownValueID.toString() != initiallifecyclestage.toString()) {
            selfContact.IsLifecycleChanged(true);
        } else {
            selfContact.IsLifecycleChanged(false);
        }
    }

    var partnerTypeDefault = ko.utils.arrayFirst(data.PartnerTypes, function (choice) {
        if (choice.IsDefault == true)
            return choice;
    });

    var lifecycleDefault = ko.utils.arrayFirst(selfContact.LifecycleStages(), function (choice) {
        if (choice.IsDefault == true)
            return choice;
    });
    var lifecyclestage = [];
    if (data.LifecycleStage > 0) {
        selfContact.LifecycleStage = ko.observable(data.LifecycleStage);
        var lifecycle = ko.utils.arrayFirst(selfContact.LifecycleStages(), function (choice) {
            if (choice.DropdownValueID == selfContact.LifecycleStage()) {
                lifecyclestage.push(choice.DropdownValueTypeID);
            }
        });
        if (lifecyclestage != null) {
            if (lifecyclestage == 16) {
                selfContact.enablePartnerType(true);
                selfContact.showPartner(true);
                //selfContact.PartnerType(DropdownValueID);
                //Todo: default the selfContact.PartnerType to value based on account configuration(According to specs)
            }
        }
        else {
            selfContact.enablePartnerType(false);
            selfContact.showPartner(false);
            selfContact.PartnerType(null);
        }
    };

    selfContact.LifecycleStage.subscribe(function (selectedValue) {
        var lifecycle = ko.utils.arrayFirst(selfContact.LifecycleStages(), function (choice) {
            if (choice.DropdownValueID == selectedValue)
                return choice;
        });

        if (lifecycle.DropdownValueTypeID == 16) {
            selfContact.enablePartnerType(true);
            selfContact.showPartner(true);
            selfContact.PartnerType(partnerTypeDefault.DropdownValueID);
            //Todo: default the selfContact.PartnerType to value based on account configuration(According to specs)
        }
        else {
            selfContact.enablePartnerType(false);
            selfContact.showPartner(false);
            selfContact.PartnerType(null);
        }
    });

    selfContact.errors = ko.validation.group(selfContact);

    selfContact.saveText = ko.observable('[|Save|]');

    selfContact.phonevalidation = function () {

      var isVaidPhoneNumber = true;
        ko.utils.arrayForEach(selfContact.Phones(), function (phn) {
            if ((phn.Number() == null || phn.Number() == "") && ((phn.Extension() != null && phn.Extension() != "") || (phn.CountryCode() != null && phn.CountryCode() != ""))) {
                notifyError("[|Phone Number is Invalid|]");
                isVaidPhoneNumber = false;
                return false;
            }
            if (phn.Number() != null && phn.Number().length > 0) {
                var numbers = phn.Number().match(/\d/g);
                if (numbers != null && numbers != 'undefined') {
                    if (numbers.length != 10) {
                        notifyError("[|Phone Number length is Invalid|]");
                        isVaidPhoneNumber = false;
                        return false;
                    }
            }
                else {
                    notifyError("[|Please enter only numerics for Phone Number|]");
                    isVaidPhoneNumber = false;
                    return false;
                }
            }
            if (phn.CountryCode() != null && phn.CountryCode().length > 0) {
                var numbers = phn.CountryCode().match(/\d/g);
                var alpha = phn.CountryCode().match(/[a-z]/i);
                if ((numbers != null && numbers != 'undefined') || (alpha != null && alpha != 'undefined'))
                    if ((numbers != null && numbers.length < 1) || alpha) {
                        notifyError("[|Country Code is Invalid|]");
                        isVaidPhoneNumber = false;
                        return false;
                    }
            }
        });
        if (!isVaidPhoneNumber)
            return false;
        return true;
    }


    selfContact.savePerson = function (id) {

        selfContact.widget(null);

        for (var p = 0; p < selfContact.Phones().length; p++) {
            //if (selfContact.Phones()[p].Number() != null && selfContact.Phones()[p].Number() != undefined && selfContact.Phones()[p].Number().trim() != "") {
                var value = selfContact.phonevalidation();
                if (value == false)
                    return;
            //}
        }
        selfContact.errors.showAllMessages();
        if (selfContact.errors().length > 0) {
            validationScroll();
            return;
        }
        selfContact.MapCustomFields();
        var contactsource = $.grep(selfContact.LeadSources(),
                            function (e) {
                                return e.DropdownValueID == selfContact.ContactSourceTypeID();
                            });

        var selectedsources = [];
        var contactsecsrc = selfContact.ContactSecondarySource();
        for (var i = 0; i < contactsecsrc.length; i++) {
            selectedsources.push($.grep(selfContact.LeadSources(),
                              function (e) {
                                  return e.DropdownValueID == contactsecsrc[i];
                              })[0]);
        }
        var allsources = contactsource.concat(selectedsources);

        selfContact.SelectedLeadSource(allsources);
        
        var jsondata = ko.toJSON(selfContact);
        var action;
        if (selfContact.ContactID() != null && selfContact.ContactID() > 0)
            action = "UpdatePerson";
        else
            action = "InsertPerson";

        if (id == 1)
            selfContact.saveText('[|Saving|]..');

        pageLoader();
        var contactType = 1;

        $.ajax({
            url: url + 'ContactDuplicateCheck',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'person': jsondata, 'contactType': 1 }),
            success: function (data) {
                removepageloader();
                if (data.response != undefined && data.response.DuplicateContactId > 0)
                {
                        if (data.response.Name != null && data.response.Name != " ")
                        {
                            selfContact.EmailExist(false);
                            selfContact.NameExist(true);
                        }
                        else
                        {
                            selfContact.EmailExist(true);
                            selfContact.NameExist(false);
                        }
                        selfContact.ContactName(data.response.Name);
                        selfContact.ContactEmail(data.response.Email);
                        selfContact.HasPermission(data.response.HasPermission);
                        selfContact.DuplicateContactId(data.response.DuplicateContactId);
                        $('#myModal').modal('toggle');
                        selfContact.saveText('Save');
                }
                else {
                    pageLoader();
                    $.ajax({
                        url: url + action,
                        type: 'post',
                        dataType: 'json',
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify({ 'personViewModel': jsondata })
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
                        $('.success-msg').remove();
                        selfContact.saveText('Save');
                        notifySuccess('Successfully saved the Contact');
                        setTimeout(function () {
                            removepageloader();
                            if (id == 2)
                                window.location.href = "/addperson";
                            else
                                window.location.href = "/person/" + data.response + "/" + contactType;
                        }, setTimeOutTimer);
                    }).fail(function (error) {
                        $('.success-msg').remove();
                        selfContact.saveText('Save');
                        removepageloader();
                        notifyError(error);
                    });
                }
            },
            error: function (error) {
                console.log(error);
            }
        });

        
    };

    selfContact.deletePerson = function () {
        if (selfContact.ContactID() > 0) {
            alertifyReset("Delete Contact", "Cancel");
            alertify.confirm("[|Are you sure you want to delete this Contact|]?", function (e) {
                if (e) {
                    jQuery.support.cors = true;
                    $.ajax({
                        url: url + 'DeleteContact',
                        type: 'post',
                        dataType: 'json',
                        contentType: 'application/json; charset=utf-8',
                        data: JSON.stringify({ 'id': selfContact.ContactID() })
                    }).then(function (response) {
                        var filter = $.Deferred()
                        if (response.success) {
                            filter.resolve(response)
                        } else {
                            filter.reject(response.error)
                        }
                        return filter.promise()
                    }).done(function (data) {
                        if (data.success === false) {
                            notifyError(error);
                        }
                        else {
                            notifySuccess("[|Contact deleted successfully|]");
                            window.location.href = document.URL;
                        }
                    }).fail(function (error) {
                        notifyError(error);
                    })
                    //    success: function (data) {
                    //        if (data.success === false) {
                    //            selfContact.saveText('Save');
                    //            $('body').append('<div class="alert alert-error success-msg"><a class="close" data-dismiss="alert" href="#" aria-hidden="true">&times;</a> ' + data.response + '</div>');
                    //        }
                    //        else {
                    //            window.location.href = document.URL;
                    //        }
                    //    },
                    //    error: function (data)
                    //    { }
                    //});
                    notifySuccess("[|Contact deleted successfully|]");
                }
                else {
                    notifyError("[|You've clicked cancel|]");
                }
            });
        }
        else { return; }
    };

    selfContact.imagePath = ko.observable();
    selfContact.ProfileImage = ko.observable();
    selfContact.ImageAssign = function () {

        selfContact.Image = ko.observable(data.Image);
        if (selfContact.Image().ImageContent != null && selfContact.Image().ImageContent != "undefined") {
            // selfContact.imagePath(selfContact.Image().ImageContent);
            // selfContact.Image.ImageContent = null;
        }
        else {
            // selfContact.imagePath(ImagePath);
            selfContact.Image().ImageContent = ImagePath;
        }
    }
    selfContact.ImageAssign();
    selfContact.uploadProfileImage = function () {
        var filename = selfContact.ProfileImage();
        selfContact.ProfileImageKey = null;
        var extension = filename.replace(/^.*\./, '');

        if (extension.toLowerCase() == "jpeg" || extension.toLowerCase() == "jpg" || extension.toLowerCase() == "png" || extension.toLowerCase() == "bmp") {
            var image = document.getElementById("contactimage");
            image.src = filename;
            selfContact.Image.ImageContent = filename;
            selfContact.ContactImageUrl = filename;
        }
        else {
            notifyError("[|Please upload jpg, jpeg, png, bmp files|]");
            return false;
        }
    }

    function getSecondaryLeadSources(SelectedContactSourceID) {
        var secondaryleadsources = ko.utils.arrayFilter(selfContact.LeadSources(), function (leadsource) {
            return leadsource.DropdownValueID != SelectedContactSourceID;
        });
        return secondaryleadsources;
    }
}

