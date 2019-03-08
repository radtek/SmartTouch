var userViewModel = function (data, countries, url, newAddress, userurl, myJsVariable,IsAccountStAdmin) {
    selfUser = this;

    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfUser));

  //  Returns a copy of newAddress
    selfUser.getAddressTemplate = function () {
        var addressTemplate = jQuery.extend(true, {}, JSON.parse(newAddress));
        addressTemplate.Countries = countries;
        addressTemplate.States = ko.observableArray();
        addressTemplate.Country.Code = ko.observable(addressTemplate.Country.Code);
        addressTemplate.State.Code = ko.observable(addressTemplate.State.Code);
        addressTemplate.AddressTypeID = ko.observable(addressTemplate.AddressTypeID);
        addressTemplate.IsDefault = ko.observable(addressTemplate.IsDefault);
        addressTemplate.countrySelect = function () {
            addressTemplate.State.Code('');
        }
        return addressTemplate; 
    };

    selfUser.FirstName = ko.observable(data.FirstName).extend({ required: { message: "[|First Name is required|]" }, maxLength: 75 });
    selfUser.LastName = ko.observable(data.LastName).extend({ required: { message: "[|Last Name is required|]" }, maxLength: 75 });
    selfUser.Company = ko.observable(data.Company).extend({ maxLength: 75 });
    selfUser.PrimaryEmail = ko.observable(data.PrimaryEmail).extend({ required: { message: "[|Email is required|]" }, maxLength: 256, email: true });
    
 //  selfUser.PhoneTypes = ko.observableArray(['Mobile', 'Work', 'Home']);
   selfUser.PhoneTypes = ko.observableArray(data.PhoneNumberTypes);
    selfUser.userStatus = ko.observableArray([{ Id: 2, Type: '[|Inactive|]' }, { Id: 1, Type: '[|Active|]' }]);
    selfUser.Status = ko.observable(data.Status);
    selfUser.Phones = ko.observableArray(data.Phones);   
    
    selfUser.Roles = ko.observableArray(data.Roles);
    selfUser.RoleID = ko.observable(data.RoleID);
    selfUser.DropdownValueID = ko.observable();
    selfUser.DropdownValue = ko.observable();
  
    selfUser.primaryPhone = ko.observable();

    selfUser.CreatedOn = ko.pureComputed({
        read: function () {
            
            return moment(data.CreatedOn).format();
        }
    });
 
    $.each(selfUser.Phones(), function (index, phone) {
        
        if (phone.IsPrimary === true) {
            selfUser.primaryPhone(phone);
            var phoneId = '#primaryPhone' + index;
            setTimeout(function () {
                $(phoneId).closest('label.radio').addClass('checked');
            }, 200);
        }

        phone.IsPrimary = ko.pureComputed(function () {
            return phone === selfUser.primaryPhone();
        });
       
    });
    selfUser.getreadonlyState = ko.observable(null);
    selfUser.DefaultPhoneIndex = ko.observable();
    var index = 0;
    selfUser.addPhone = function () {
        //var defaultPhoneTypeIndex = ko.utils.arrayFirst(selfUser.PhoneTypes(), function (type) {
        //    return type.IsDefault === true;
        //});

        if (index == selfUser.PhoneTypes().length - 1) {
            index = 0;
        }
        else {
            index++;
        }

        var newPhone = {
            PhoneTypeName: ko.observable(selfUser.PhoneTypes()[index].DropdownValue),
            Number: ko.observable(),
            ContactPhoneNumberID: 0,
            PhoneType: ko.observable(selfUser.PhoneTypes()[index].DropdownValueTypeID)
        };
        newPhone.IsPrimary = ko.pureComputed(function () {
            return newPhone === selfUser.primaryPhone();
        });
        newPhone.PhoneType.subscribe(function (newValue) {
            var phoneType = ko.utils.arrayFirst(selfUser.PhoneTypes(), function (type) {
                return type.DropdownValueTypeID == newValue;
            });
            newPhone.PhoneTypeName(phoneType.DropdownValue);
            newPhone.PhoneType(phoneType.DropdownValueTypeID);
        });
        selfUser.Phones.push(newPhone);
    };

    selfUser.removePhone = function () {
        selfUser.Phones.remove(this);
    };

    selfUser.SocialMediaUrlTypes = ko.observableArray(["Website", "LinkedIn", "Facebook", "Twitter", "Google+", "Blog"]);
    selfUser.SocialMediaUrls = ko.observableArray(data.SocialMediaUrls);

    selfUser.tempUrlTypes = ko.observableArray(["[|Website|]", "[|LinkedIn|]", "[|Facebook|]", "[|Twitter|]", "[|Google+|]", "[|Blog|]"]);

    ko.utils.arrayFilter(selfUser.SocialMediaUrls(), function (url) {
        selfUser.tempUrlTypes.remove(url.MediaType);
    });
        
    selfUser.widget = ko.observable();
    selfUser.addSocialMediaUrl = function () {
        selfUser.SocialMediaUrls.push({ MediaType: ko.observable(selfUser.tempUrlTypes()[0]), Url: ko.observable('') });
        $("select[name='mediumdf']").selectpicker({ style: 'btn-default' });
        selfUser.tempUrlTypes.remove(selfUser.tempUrlTypes()[0]);
    };

    selfUser.removeSocialMediaUrl = function () {
        selfUser.tempUrlTypes.push(this.MediaType());
        selfUser.SocialMediaUrls.remove(this);
    };

    selfUser.updateURLType = function (urlType) {
        selfUser.tempUrlTypes.remove(selfUser.widget()._selectedValue);
        if (selfUser.tempUrlTypes().map(function (e) { return e }).indexOf(urlType()) == -1) {
            selfUser.tempUrlTypes.push(urlType());
        }
    };

    selfUser.SecondaryEmails = ko.observableArray(data.SecondaryEmails);

    selfUser.Emails = ko.observableArray(data.Emails);

    selfUser.addEmail = function () {    
        if(IsAccountStAdmin==true)
        selfUser.Emails.push({ EmailId: ko.observable(''), IsPrimary: ko.observable(false),AccountID:1, UserID:data.UserID });
        else
            selfUser.Emails.push({ EmailId: ko.observable(''), IsPrimary: ko.observable(false),AccountID:data.AccountID, UserID:data.UserID });
    };
    selfUser.removeEmail = function () {
        $.ajax({
            url: '/User/GetUserEmailAssociatedWorkFlow',
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                selfUser.Roles(data);
            }
        });
        selfUser.Emails.remove(this);
    };

    selfUser.AddressTypes = ko.observableArray(data.AddressTypes);
    selfUser.Addresses = ko.observableArray(data.Addresses);
    
    selfUser.primaryAddress = ko.observable();

    selfUser.addAddress = function () {

        var newAddressCopy = selfUser.getAddressTemplate();

        newAddressCopy.Country.Code.subscribe(function () {
            selfUser.countryChanged(newAddressCopy);
        });
        newAddressCopy.IsDefault = ko.pureComputed(function () {
            return newAddressCopy === selfUser.primaryAddress();
        });
        selfUser.countryChanged(newAddressCopy);

        selfUser.Addresses.push(newAddressCopy);
    };

    selfUser.removeAddress = function () {
        selfUser.Addresses.remove(this);
    };

   $.ajax({
        url: userurl + 'GetRoles',
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
       selfUser.Roles(data.response);
       var roleId = selfUser.RoleID();
       selfUser.RoleID("");
       selfUser.RoleID(roleId);
   }).fail(function (error) {
       notifyError(error);
   });
   
    // Gets states of the selected country.
   selfUser.countryChanged = function (address) {
       $.ajax({
           url: url + 'GetStates',
           type: 'get',
           dataType: 'json',
           contentType: "application/json; charset=utf-8",
           data: { 'countryCode': address.Country.Code() }
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
           address.States(data.response);
           var code = address.State.Code();
           address.State.Code("");
           address.State.Code(code);
       }).fail(function (error) {
           notifyError(error);
       });
   };

   $.each(selfUser.Addresses(), function (index, address) {
        address.AddressTypeID = ko.observable(address.AddressTypeID);
        address.States = ko.observableArray(address.States);
        address.Country.Code = ko.observable(address.Country.Code);
        
        selfUser.countryChanged(address);
        address.State.Code = ko.observable(address.State.Code);
        address.IsDefault = ko.observable(address.IsDefault);
        address.Country.Code.subscribe(function () {
            selfUser.countryChanged(address);
        });
        address.Countries = countries;
        address.countrySelect = function () {
            address.State.Code('');
        };
        if (address.IsDefault() === true) {
            selfUser.primaryAddress(address);
        }
        address.IsDefault = ko.pureComputed(function () {
            return address === selfUser.primaryAddress();
        });
    });

   selfUser.readonly = function () {
       if (selfUser.IsPrimary)
           selfUser.getreadonlyState(undefined);
       else
           selfUser.getreadonlyState('readonly');
   };

    selfUser.errors = ko.validation.group(selfUser);

    selfUser.saveText = ko.observable('[|Save|]');

    selfUser.saveUser = function () {
        selfUser.errors.showAllMessages();
        if (selfUser.errors().length > 0)
            return;
        
        selfUser.widget(null);
        var jsondata = ko.toJSON(selfUser);
        var action;
      
        action = "UpdateUser";
        selfUser.saveText('[|Saving..|]');
        pageLoader();
        $.ajax({
            url: userurl + action,
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'userViewModel': jsondata })
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
           
            selfUser.saveText('[|Save|]');
            notifySuccess('[|Successfully saved the User|]');

            if (myJsVariable != "MyProfile") {
                removepageloader();
                window.location.href = "/users";
            }
            location.reload();
        }).fail(function (error) {
            removepageloader();
            selfUser.saveText('[|Save|]');
            notifyError(error);
        });
    };

    selfUser.deleteUser = function () {
        alertifyReset("Delete User", "Cancel");
        alertify.confirm("[|Are you sure you want to delete this User?|]", function (e) {
           
            if (e) {
                var deleteUsers = [selfUser.UserID()];
                var varDeleteURL = "DeleteUsers?id=" + deleteUsers;
                jQuery.support.cors = true;
                $.ajax({
                    url: userurl + varDeleteURL,
                    type: 'post',
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
                    notifySuccess("[|Successfully deleted the user|]");
                    if (data.success === true) {
                        window.location.href = "/users";
                    }
                }).fail(function (error) {
                    notifyError(error);
                });
            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }
        });
    };
}