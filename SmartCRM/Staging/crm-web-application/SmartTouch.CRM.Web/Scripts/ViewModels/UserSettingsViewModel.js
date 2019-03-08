var userSettingViewModel = function (data, Account_Base_URL, Contact_BASE_URL, User_BASE_URL, Login_Base_URL) {
var  selfUserSettings = this;
    ko.mapping.fromJS(data, {}, selfUserSettings);
    ko.validation.rules['CheckPasswordEntered'] = {
        async: true,
        validator: function (val, otherValue, callback) {
            if ($.trim(val) == "") {
                callback({ isValid: true, message: "[|yay it worked|]" });
            } else {
                $.ajax({
                    url: User_BASE_URL + 'CheckPassword',
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({
                        password: val
                    }),
                    //contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        if (data.response == "") {
                            callback({ isValid: true, message: "[|yay it worked|]" });
                        } else {
                            callback({ isValid: false, message: data.response });
                        }
                    }
                });
            }
        }
    };
    ko.validation.rules['emailtypeCannotEqual'] = {
        validator: function (EmailId) {
            return (EmailId != "Select...");
        },
        message: '[|Please select Email Id|]'
    };
    ko.validation.registerExtenders();
    selfUserSettings.DateFormat = ko.observable(data.DateFormat);
    selfUserSettings.Emails = ko.observableArray(data.Emails);
    selfUserSettings.EmailId = ko.observable();
    selfUserSettings.Countries = ko.observableArray([]);
    selfUserSettings.CountryId = ko.observable(data.CountryId);
    selfUserSettings.IsIncludeSignature = ko.observable(data.IsIncludeSignature);


    selfUserSettings.EmailSignature = ko.observable();
    selfUserSettings.emailSelect = function (e) {
        var dataItem = this.dataItem(e.item);
        selfUserSettings.EmailSignature(dataItem.EmailSignature == null ? "" : dataItem.EmailSignature);
    };
    selfUserSettings.IsUpdated = ko.observable(false);

    var DefaultEmail = ko.utils.arrayFirst(selfUserSettings.Emails(), function (type) {
       // console.log(type);
        return type.IsPrimary === true;
    });
    if (DefaultEmail != null) {
        selfUserSettings.EmailId(DefaultEmail.EmailId);
        selfUserSettings.EmailSignature(DefaultEmail.EmailSignature);
    }
    selfUserSettings.emailTypeValidation = selfUserSettings.EmailId.extend({ emailtypeCannotEqual: 0 });
    //selfUserSettings.emailSignatureValidation = selfUserSettings.EmailSignature.extend({ checkEmailSignatureValidation: true });

    //selfUserSettings.EmailId.subscribe(function (index, value) {
    //    console.log("index: " + index);
    //   // console.log("value: " + value);
    //    //selfUserSettings.Emails()[1] = 
    //});



    //  selfUserSettings.DateFormat = ko.observable(data.DateFormat);
    //  selfUserSettings.Emails = ko.observableArray([]);

    //$.ajax({
    //    url: User_BASE_URL + 'GetEmails',
    //    type: 'get',
    //    dataType: 'json',
    //    contentType: "application/json; charset=utf-8",
    //    success: function (emaildata) {
    //        selfUserSettings.Emails(emaildata);
    //    }
    //});
    // console.log(ko.toJSON(selfUserSettings.Emails()));
    // selfUserSettings.Emails = ko.observableArray(data.Emails);
    selfUserSettings.DateFormats = ko.observableArray([]);
    selfUserSettings.currentPassword = ko.observable("").extend({ CheckPasswordEntered: "" });
    selfUserSettings.newPassword = ko.observable("").extend({
        validation: [
            {
                validator: function (val) {
                    if (selfUserSettings.currentPassword() == "") {
                        return true;
                    } else {
                        var regex2 = new RegExp(/^(?=.*?[A-Z])(?=(.*[a-z]){1,})(?=(.*[\d]){1,})(?=(.*[\W_]){1,})(?!.*\s).{6,}$/);
                        if (regex2.test(val)) {
                            return true;
                        } else {
                            return false;
                        }
                    }
                },
                message: '[|Password must contain one uppercase letter,one lowercase letter, one number, one special character and minimum 6 characters required|]'
            },
            {
                validator: function (val) {
                    if (selfUserSettings.currentPassword() == "") {
                        return true;
                    } else {
                        if (val == selfUserSettings.currentPassword()) {
                            return false;
                        } else {
                            return true;
                        }
                    }
                },
                message: '[|Please reenter password|]'
            }

        ]
    });

    selfUserSettings.ItemsPerPage = ko.observable(data.ItemsPerPage);
    //selfUserSettings.emailTypeValidation = selfUserSettings.EmailId.extend({ emailtypeCannotEqual: 0 });
    //selfUserSettings.EmailId = ko.observable(data.EmailId).extend({ emailtypeCannotEqual: 0 })

    selfUserSettings.confirmPassword = ko.observable("").extend({
        validation: [{
            validator: function (val) {
                return val === selfUserSettings.newPassword();
            },
            message: '[|Passwords do not match|]'
        },
          {
              validator: function (val) {
                  if (selfUserSettings.newPassword().length > 0 && val.length > 0) {
                      return $.trim(selfUserSettings.currentPassword()).length > 0;
                  } else {
                      return true;
                  }
              },
              message: '[|Please enter the current password|]'
          }
        ]
    });


    selfUserSettings.Currencies = ko.observableArray([]);
    selfUserSettings.CurrencyID = ko.observable(data.CurrencyID);
    selfUserSettings.TimeZone = ko.observable(data.TimeZone);
    selfUserSettings.TimeZones = ko.observableArray([]);
    selfUserSettings.errors = ko.validation.group(selfUserSettings);
    selfUserSettings.Items = ko.observableArray(ToPageDropdown());

    selfUserSettings.CurrencyFormat = ko.pureComputed(function () {
        if (selfUserSettings.CurrencyID() == 1 || selfUserSettings.CurrencyID() == 2) {
            return "$X,XXX.XX";
        }
        else if (selfUserSettings.CurrencyID() == 3) {
            return "X XXX,XX $";
        }
        else if (selfUserSettings.CurrencyID() == 5) {
            return "₹X,XXX.XX";
        }
        else {
            return "B/.X,XXX.XX";
        }
    });

    $.ajax({
        url: Account_Base_URL + 'GetTimeZones',
        type: 'get',
        dataType: 'json',
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
        selfUserSettings.TimeZones(data.response);
        var code = selfUserSettings.TimeZone();
        selfUserSettings.TimeZone("");
        selfUserSettings.TimeZone(code)
    }).fail(function (error) {
        // Display error message to user   
        notifyError(error);
    });

    $.ajax({
        url: User_BASE_URL + 'GetCurrencies',
        type: 'get',
        dataType: 'json',
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
        selfUserSettings.Currencies(data.response);
        var code = selfUserSettings.CurrencyID();
        selfUserSettings.CurrencyID("");
        selfUserSettings.CurrencyID(code)
    }).fail(function (error) {
        // Display error message to user   
        notifyError(error);
    });

    $.ajax({
        url: User_BASE_URL + 'GetDateFormats',
        type: 'get',
        dataType: 'json',
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
        selfUserSettings.DateFormats(data.response);
        var code = selfUserSettings.DateFormat();
        selfUserSettings.DateFormat("");
        selfUserSettings.DateFormat(code)
    }).fail(function (error) {
        // Display error message to user   
        notifyError(error);
    });


    $.ajax({
        url: Contact_BASE_URL + 'GetCountries',
        type: 'get',
        dataType: 'json',
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
        selfUserSettings.Countries(data.response);
        var code = selfUserSettings.CountryId();
        selfUserSettings.CountryId("");
        selfUserSettings.CountryId(code)
    }).fail(function (error) {
        // Display error message to user   
        notifyError(error);
    });

    selfUserSettings.saveSettings = function () {
        var result = ko.validation.group(selfUserSettings, { deep: true });
        if (selfUserSettings.errors().length > 0) {
            result.showAllMessages(true);
            return false;
        } else {

            $.each(selfUserSettings.Emails(), function (index, value) {
                if (value.EmailId == selfUserSettings.EmailId()) {
                    value.EmailSignature = selfUserSettings.EmailSignature();
                    value.IsUpdated = true;
                    return false;
                }
            });

            pageLoader();
            var jsondata = ko.toJSON(selfUserSettings);
            $.ajax({
                url: User_BASE_URL + "SaveSettings",
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'userSettingsViewModel': jsondata })
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
                if (data.success == false) {
                    notifyError(data.response);
                }
                else {
                    notifySuccess('[|Successfully Saved your settings|]');
                    setTimeout(
                        function () {
                            removepageloader();
                            window.location.href = Login_Base_URL + "LogOff";
                        }, setTimeOutTimer);
                }
            }).fail(function (error) {
                removepageloader();
                notifyError(error);
            });;
        }
    }
};