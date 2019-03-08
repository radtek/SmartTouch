var companyViewModel = function (data, countries, url, newAddress, DropDownURL, ImagePath, Users) {
    selfContact = this;
    var DATATYPE_CHECKBOX = 1;
    var DATATYPE_MULTISELECT = 12;
    var DATATYPE_DATE = 13;
    var DATATYPE_TIME = 9;

    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfContact));
    selfContact.CurrentView = ko.observable('generalfields');
    selfContact.CurrentTabView = ko.observable(0);
    selfContact.showGeneralFields = function () {
        selfContact.CurrentView('generalfields');
    }

    selfContact.showCustomFields = function () {
        selfContact.CurrentView('customfields');
    }

    selfContact.showCurrentTab = function (tabIndex) {
        selfContact.CurrentTabView(tabIndex);
    }

    selfContact.Users = ko.observableArray();
    if (selfContact.ContactID() == 0) {
        Users = ko.utils.arrayFilter(Users, function (usr) {
            return usr.IsDeleted == false;
        });
    }
    selfContact.Users(Users);

    selfContact.OwnerId = ko.observable(data.OwnerId);
    selfContact.PreviousOwnerId = ko.observable(data.OwnerId);
    selfContact.EmailExist = ko.observable(false);
    selfContact.NameExist = ko.observable(false);
    selfContact.HasPermission = ko.observable(true);
    selfContact.CompanyEmail = ko.observable();
    selfContact.Name = ko.observable();
    selfContact.DuplicateContactId = ko.observable();
  

    selfContact.templ = kendo.template('#if (data.OwnerId == 0 )  { # <span>[|Select..|]</span># } ##if (data.IsDeleted == false )  { # <span>#= OwnerName #</span># } # #if (data.IsDeleted == true )  { # <span style="color:red;">#= OwnerName #</span># } #');



    selfContact.CustomFields = ko.observableArray(data.CustomFields);
    $.each(selfContact.CustomFieldTabs(), function(index, tab) {
        $.each(tab.Sections(), function(sectionIndex, section) {
            $.each(section.CustomFields(), function(fieldIndex, field) {

                var customFieldIndex = selfContact.CustomFields().map(function(e) { return e.CustomFieldId; }).indexOf(parseInt(field.FieldId()));

                if (customFieldIndex >= 0) {
                    if (field.FieldInputTypeId() == DATATYPE_DATE) {
                        if (selfContact.CustomFields()[customFieldIndex].Value != "") {
                            //var dateFormat = readCookie("dateformat").toUpperCase();
                            field.Value(kendo.toString(new Date(selfContact.CustomFields()[customFieldIndex].Value)));
                        }

                    } else if (field.FieldInputTypeId() == DATATYPE_TIME) {
                        field.Value(kendo.toString(selfContact.CustomFields()[customFieldIndex].Value));
                    } else if (field.FieldInputTypeId() == DATATYPE_CHECKBOX || field.FieldInputTypeId() == DATATYPE_MULTISELECT) {
                        field.Value(selfContact.CustomFields()[customFieldIndex].Value.split("|"));
                    } else
                        field.Value(selfContact.CustomFields()[customFieldIndex].Value);
                } else {
                    if (field.FieldInputTypeId() == DATATYPE_CHECKBOX) {
                        field.Value([]);
                    } else
                        field.Value("");
                }
            });
        });
    });
    selfContact.MapCustomFields = function () {
        $.each(selfContact.CustomFieldTabs(), function(index, tab) {
            $.each(tab.Sections(), function(sectionIndex, section) {
                $.each(section.CustomFields(), function(fieldIndex, field) {
                    // customFieldIndex checks if this contact has any value associated to this particular custom field.
                    var customFieldIndex = selfContact.CustomFields().map(function(e) { return e.CustomFieldId; }).indexOf(parseInt(field.FieldId()));
                    //If the value is empty then remove it from the array.
                    if (customFieldIndex != -1 && (field.Value() == "" || field.Value() == null)) {
                        selfContact.CustomFields().splice(customFieldIndex, 1);
                    } else if (customFieldIndex == -1 && (field.Value() != "" || field.Value() != null)) {
                        if (field.FieldInputTypeId() == DATATYPE_CHECKBOX || field.FieldInputTypeId() == DATATYPE_MULTISELECT) {
                            
                            selfContact.CustomFields.push({
                                ContactCustomFieldMapId: 0,
                                ContactId: selfContact.ContactID,
                                CustomFieldId: field.FieldId(),
                                Value: field.Value() == "" ? "" : field.Value().join('|'),
                                FieldInputTypeId: field.FieldInputTypeId()
                            });
                            field.Value(field.Value() == "" ? "" : field.Value().join('|'));
                        } else if (field.FieldInputTypeId() == DATATYPE_DATE) {
                            selfContact.CustomFields.push({
                                ContactCustomFieldMapId: 0,
                                ContactId: selfContact.ContactID,
                                CustomFieldId: field.FieldId(),
                                Value: $('#' + field.FieldId()).val() == "" ? "" : $('#' + field.FieldId()).val(),
                                FieldInputTypeId: field.FieldInputTypeId()
                            });
                            field.Value($('#' + field.FieldId()).val() == "" ? "" : $('#' + field.FieldId()).val());
                        } else if (field.FieldInputTypeId() == DATATYPE_TIME) {
                            selfContact.CustomFields.push({
                                ContactCustomFieldMapId: 0,
                                ContactId: selfContact.ContactID,
                                CustomFieldId: field.FieldId(),
                                Value: $('#' + field.FieldId()).val() == "" ? "" : $('#' + field.FieldId()).val(),
                                FieldInputTypeId: field.FieldInputTypeId()
                            });
                            field.Value($('#' + field.FieldId()).val() == "" ? "" : $('#' + field.FieldId()).val());
                        } else {
                            selfContact.CustomFields.push({
                                ContactCustomFieldMapId: 0,
                                ContactId: selfContact.ContactID,
                                CustomFieldId: field.FieldId(),
                                Value: field.Value(),
                                FieldInputTypeId: field.FieldInputTypeId()
                            });
                        }
                    } else if (customFieldIndex != -1) {
                        if (field.FieldInputTypeId() == DATATYPE_DATE) {
                            selfContact.CustomFields()[customFieldIndex].Value = $('#' + field.FieldId()).val() != null ? $('#' + field.FieldId()).val() : "";
                            selfContact.CustomFields()[customFieldIndex].FieldInputTypeId = field.FieldInputTypeId();
                            field.Value($('#' + field.FieldId()).val() == "" ? "" : $('#' + field.FieldId()).val());
                        } else if (field.FieldInputTypeId() == DATATYPE_TIME) {
                            selfContact.CustomFields()[customFieldIndex].Value = $('#' + field.FieldId()).val() != null ? $('#' + field.FieldId()).val() : "";

                            selfContact.CustomFields()[customFieldIndex].FieldInputTypeId = field.FieldInputTypeId();
                            field.Value($('#' + field.FieldId()).val() == "" ? "" : $('#' + field.FieldId()).val());
                        } else if (field.FieldInputTypeId() == DATATYPE_CHECKBOX || field.FieldInputTypeId() == DATATYPE_MULTISELECT) {
                            selfContact.CustomFields()[customFieldIndex].Value = field.Value() == "" ? "" : field.Value().join('|');
                            selfContact.CustomFields()[customFieldIndex].FieldInputTypeId = field.FieldInputTypeId();
                            field.Value(field.Value() == "" ? "" : field.Value().join('|'));
                        } else {
                            selfContact.CustomFields()[customFieldIndex].Value = field.Value();
                            selfContact.CustomFields()[customFieldIndex].FieldInputTypeId = field.FieldInputTypeId();
                        }
                    }
                    field.Value(field.Value() != null ? field.Value().toString() : "");
                });
            });
        });
    }

    selfContact.Dateformat = function () {
        return readCookie("dateformat");
    }

    selfContact.CompanyName = ko.observable(data.CompanyName).extend({ required: { message: "[|Company name is required|]" } });
    selfContact.PrimaryEmail = ko.observable(data.PrimaryEmail).extend({ maxLength: 256, email: true });

    selfContact.Phones = ko.observableArray(data.Phones);
    selfContact.SocialMediaUrlTypes = ko.observableArray(["[|Website|]", "[|LinkedIn|]", "[|Facebook|]", "[|Twitter|]", "[|Google+|]", "[|Blog|]"]);
    selfContact.SocialMediaUrls = ko.observableArray(data.SocialMediaUrls);


    selfContact.PhoneTypes = ko.observableArray();
    selfContact.PhoneTypes(data.PhoneTypes);

    selfContact.Phones = ko.observableArray(data.Phones);
    selfContact.primaryPhone = ko.observable();
    
    var defaultPhoneTypeIndex = ko.utils.arrayFirst(selfContact.PhoneTypes(), function (type) {
        return type.IsDefault === true;
    });

    $.each(selfContact.Phones(), function (index, phone) {
        var type = ko.utils.arrayFirst(selfContact.PhoneTypes(), function (type) {
            return type.DropdownValueID == phone.PhoneType;
        });
        if (type)
            phone.PhoneType = ko.observable(phone.PhoneType);
        else
            phone.PhoneType = ko.observable(defaultPhoneTypeIndex.DropdownValueID);
        phone.Number = ko.observable(phone.Number);
        phone.PhoneTypeName = ko.observable(phone.PhoneTypeName);
        phone.CountryCode = ko.observable(phone.CountryCode);
        phone.Extension = ko.observable(phone.Extension);
        if (phone.IsPrimary === true) {
            selfContact.primaryPhone(phone);
            var phoneId = '#primaryPhone' + index;
            setTimeout(function () {
                $(phoneId).closest('label.radio').addClass('checked');
            }, 200);
        }
        phone.IsPrimary = ko.pureComputed(function () {
            return phone === selfContact.primaryPhone();
        });
        phone.PhoneType.subscribe(function (newValue) {
            var phoneType = ko.utils.arrayFirst(selfContact.PhoneTypes(), function (type) {
                return type.DropdownValueID == newValue;
            });
            phone.PhoneTypeName(phoneType.DropdownValue);
        });
    });
    var index = 0;
    selfContact.addPhone = function () {
        if (index == selfContact.PhoneTypes().length - 1) {
            index = 0;
        }
        else {
            index++;
        }

        var newPhone = {
            PhoneType: ko.observable(selfContact.PhoneTypes()[index].DropdownValueID),
            PhoneTypeName: ko.observable(selfContact.PhoneTypes()[index].DropdownValue),
            CountryCode: ko.observable(),
            Extension: ko.observable(),
            Number: ko.observable(),
            ContactPhoneNumberID: 0
        };
        newPhone.IsPrimary = ko.pureComputed(function () {
            return newPhone === selfContact.primaryPhone();
        });
        newPhone.PhoneType.subscribe(function (newValue) {
            var phoneType = ko.utils.arrayFirst(selfContact.PhoneTypes(), function (type) {
                return type.DropdownValueID == newValue;
            });
            newPhone.PhoneTypeName(phoneType.DropdownValue);
        });
        selfContact.Phones.push(newPhone);
    };

    selfContact.removePhone = function () {
        selfContact.Phones.remove(this);
    };

    selfContact.Emails = ko.observableArray(data.Emails);
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

    $.each(selfContact.Emails(), function (index, email) {
        email.EmailStatusValue = ko.observable(email.EmailStatusValue);
        email.EmailId = ko.observable(email.EmailId);
        if (email.IsPrimary === true) {
            selfContact.primaryEmail(email);
        }

        email.IsPrimary = ko.pureComputed(function () {
            return email === selfContact.primaryEmail();
        });
    });

    selfContact.addEmail = function () {
   //     var emailStatuses = selfContact.EmailStatuses();
        var newEmail = {
            EmailStatusValue: ko.observable(50),
            EmailId: ko.observable(''),
            ContactEmailID: 0
        };
        newEmail.IsPrimary = ko.pureComputed(function () {
            return newEmail === selfContact.primaryEmail();
        });
        selfContact.Emails.push(newEmail);
    };

    selfContact.removeEmail = function () {
        selfContact.Emails.remove(this);
    };

    selfContact.DoNotEmail = ko.observable(data.DoNotEmail);
    selfContact.setDoNotEmail = function (data, event) {
        selfContact.DoNotEmail(event.target.checked);
    }

    selfContact.SocialMediaUrlTypes = ko.observableArray(["[|Website|]", "[|LinkedIn|]", "[|Facebook|]", "[|Twitter|]", "[|Google+|]", "[|Blog|]"]);
    selfContact.tempUrlTypes = ko.observableArray(["[|Website|]", "[|LinkedIn|]", "[|Facebook|]", "[|Twitter|]", "[|Google+|]", "[|Blog|]"]);
    selfContact.SocialMediaUrls = ko.observableArray(data.SocialMediaUrls);

    ko.utils.arrayFilter(selfContact.SocialMediaUrls(), function (url) {
        selfContact.tempUrlTypes.remove(url.MediaType);
    });

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
    selfContact.removeSocialMediaUrl = function () {
        selfContact.SocialMediaUrls.remove(this);
    };

    selfContact.ProfileImage = ko.observable();
    selfContact.ImageAssign = function () {
        selfContact.imagePath = ko.observable();
        selfContact.Image = ko.observable(data.Image);
        if (selfContact.Image().ImageContent != null && selfContact.Image().ImageContent != "undefined") {
            //selfContact.imagePath(selfContact.Image().ImageContent);
            //selfContact.Image.ImageContent=null;
        }
        else {
            selfContact.Image().ImageContent = ImagePath;
            // selfContact.imagePath(ImagePath);
        }
    }

    selfContact.ImageAssign();

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
            addressTemplate.countrySelect = function () {
                addressTemplate.State.Code('');
            }
            return addressTemplate;
        };
        var newAddressCopy = selfContact.getAddressTemplate();

        newAddressCopy.Country.Code.subscribe(function () {
            selfContact.countryChanged(newAddressCopy);
        });
        newAddressCopy.IsDefault = ko.pureComputed(function () {
            return newAddressCopy === selfContact.primaryAddress();
        });
        selfContact.countryChanged(newAddressCopy);
        selfContact.Addresses.push(newAddressCopy);
    };
    selfContact.removeAddress = function () {
        selfContact.Addresses.remove(this);
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
        }).then(function(response) {
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response);
            } else {
                filter.reject(response.error);
            }
            return filter.promise();
        }).done(function (data) {
            address.States(data.response);
            var code = address.State.Code();
            address.State.Code("");
            address.State.Code(code);
            
        }).fail(function(error) {
            notifyError(error);
        });
    };

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

    selfContact.saveText = ko.observable('[|Save|]');
    selfContact.errors = ko.validation.group(selfContact);

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


    selfContact.saveCompany = function (id) {
        selfContact.widget(null);

        for (var p = 0; p < selfContact.Phones().length; p++) {
            //if (selfContact.Phones()[p].Number() != null && selfContact.Phones()[p].Number() != undefined && selfContact.Phones()[p].Number().trim() != "") {
                var value = selfContact.phonevalidation();
                if (value == false)
                    return;
            //}
        }

        selfContact.MapCustomFields();
        selfContact.errors.showAllMessages();
        if (selfContact.errors().length > 0) {
            validationScroll();
            return;
        }


        var jsondata = ko.toJSON(selfContact);
        var action;
        if (selfContact.ContactID() != null && selfContact.ContactID() > 0)
            action = "UpdateCompany";
        else
            action = "InsertCompany";

        if (id == 1)
            selfContact.saveText('[|Saving|]..');
        pageLoader();
        var contactType = 2;

        $.ajax({
            url: url + 'ContactDuplicateCheck',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'person': jsondata, 'contactType': 2 }),
            success: function (data) {
                removepageloader();
               if (data.response != undefined && data.response.DuplicateContactId > 0)
               {
                        selfContact.EmailExist(false);
                        selfContact.NameExist(true);
                        selfContact.HasPermission(data.response.HasPermission);
                        selfContact.Name(data.response.Name)
                        selfContact.CompanyEmail(data.response.Email)
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
                        data: JSON.stringify({ 'companyViewModel': jsondata })
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
                        $('.success-msg').remove();
                        selfContact.saveText('Save');

                        notifySuccess('[|Successfully saved the Contact|]');
                        setTimeout(
                            function () {
                                removepageloader();
                                if (id == 2)
                                    window.location.href = "/addcompany";
                                else
                                    window.location.href = "/company/" + data.response + "/" + contactType;
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

    selfContact.deleteCompany = function () {
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
                    }).then(function(response) {
                        var filter = $.Deferred();
                        if (response.success) {
                            filter.resolve(response);
                        } else {
                            filter.reject(response.error);
                        }
                        return filter.promise();
                    }).done(function() {
                        notifySuccess("[|Contact deleted successfully|]");
                        window.location.href = document.URL;
                    }).fail(function(error) {
                        notifyError(error);
                    });
                    notifySuccess("[|Contact deleted successfully|]");
                }
                else {
                    notifyError("[|You've clicked cancel|]");
                }
            });
        }
        else { return; }
    };

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
    //selfContact.onImageselect = function (e) {
    //}
}
