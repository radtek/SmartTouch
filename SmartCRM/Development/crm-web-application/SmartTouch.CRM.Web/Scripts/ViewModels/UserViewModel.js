var removeMailRules;
var userViewModel = function (data, roles, url) {

    selfUser = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfUser));
    selfUser.FirstName = ko.observable(data.FirstName).extend({ required: { message: "[|First Name is required|]" }, maxLength: 75 });
    selfUser.LastName = ko.observable(data.LastName).extend({ required: { message: "[|Last Name is required|]" }, maxLength: 75 });
    selfUser.PrimaryEmail = ko.observable(data.PrimaryEmail).extend({ required: { message: "[|Email is required|]" }, maxLength: 256, email: true });
    selfUser.Roles = ko.observableArray(roles);
    selfUser.Status = ko.observable("").extend({ required: { message: "[|Please select the status|]" } });
    selfUser.RoleID = ko.observable(roles[0].Id).extend({ required: { message: "[|Please select the role|]" } });
    selfUser.saveText = ko.observable("Save");
    selfUser.errors = ko.validation.group(selfUser);
    

    selfUser.DoNotEmail = ko.observable(data.DoNotEmail);
    selfUser.setDoNotEmail = function (data, event) {
        selfUser.DoNotEmail(event.target.checked);
    }

    selfUser.saveUser = function () {
        selfUser.Status("2");
        selfUser.errors.showAllMessages();
        if (selfUser.errors().length > 0)
            return;
        var jsondata = ko.toJSON(selfUser);
        var action = "InsertUser";
        selfUser.saveText('[|Saving..|]');

        pageLoader();

        $.ajax({
            url: url + action,
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
            $('.success-msg').remove();
            selfUser.saveText('Save');
            notifySuccess('[|Successfully saved the User|]');
            setTimeout(
                function () {
                    removepageloader();
                    window.location.href = "/users";
                }, setTimeOutTimer);
            if (page != "MyProfile") {

                window.location.href = "/User/UserList";
            }
        }).fail(function (error) {
            selfUser.saveText('Save');
            removepageloader();
            notifyError(error);
        });
    };

    selfUser.updateStatus = function () {

        if ($(".activeStatus").attr("checked") == "checked")
            selfUser.Status($(".activeStatus").val());
        if ($(".inactiveStatus").attr("checked") == "checked")
            selfUser.Status($(".inactiveStatus").val());

        removeMailRules();
        selfUser.RoleID.rules.remove(function (item) {
            return item.rule = "required";
        });

        selfUser.Password.rules.remove(function (item) {
            return item.rule = "required";
        });

        var result = ko.validation.group(selfUser, { deep: true });
        if (!selfUser.isValid()) {
            result.showAllMessages(true);
            return false;
        } else {
            if (checkedvalues != "") {
                //var aid = checkedvalues;
                var jsondata = JSON.stringify({ 'UserID': checkedvalues, 'Status': selfUser.Status() });
                var varRoleURL = "UpdateUserStatus";
                jQuery.support.cors = true;
                pageLoader();
                $.ajax({
                    url: url + varRoleURL,
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ 'userData': jsondata })
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
                    notifySuccess("[|Successfully Updated the Status to User(s)|]");
                    setTimeout(
                        function () {
                            removepageloader();
                            window.location.href = "../users";
                            if (page != "MyProfile") {
                                window.location.href = "/User/UserList";
                            }
                        }, setTimeOutTimer);
                }).fail(function (error) {
                    notifyError(error);
                    removepageloader();
                });
            }
            else {
                notifyError("[|Please select at least one user|]");
            }
        }
    };

    selfUser.updateRole = function () {
        removeMailRules();
        selfUser.Status.rules.remove(function (item) {
            return item.rule = "required";
        });

        selfUser.Password.rules.remove(function (item) {
            return item.rule = "required";
        });

        var result = ko.validation.group(selfUser, { deep: true });

        if (!selfUser.isValid()) {
            result.showAllMessages(true);
            return false;
        } else {
            if (checkedvalues != "") {
                var aid = checkedvalues;
                var jsondata = JSON.stringify({ 'UserID': aid, 'RoleID': selfUser.RoleID() });
                var varRoleURL = "UpdateUserRole";
                jQuery.support.cors = true;
                pageLoader();
                $.ajax({
                    url: url + varRoleURL,
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ 'userData': jsondata })
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
                    notifySuccess("[|Successfully Updated the role to User(s)|]");
                    setTimeout(
                        function () {
                            removepageloader();
                            window.location.href = "../users";
                        }, setTimeOutTimer);
                }).fail(function (error) {
                    notifyError(error);
                    setTimeout(
                        function () {
                            removepageloader();
                        }, setTimeOutTimer);
                });

            }
            else {
                notifyError("[|Please select at least one user|]");
            }
        }
    };

    removeMailRules = function () {
        selfUser.FirstName.rules.remove(function (item) {
            return item.rule = "required";
        });
        selfUser.LastName.rules.remove(function (item) {
            return item.rule = "required";
        });
        selfUser.PrimaryEmail.rules.remove(function (item) {
            return item.rule = "required";
        });
    };

    selfUser.resendInvite = function () {
        if (checkedvalues != "") {
            var aid = checkedvalues;
            var jsondata = JSON.stringify({ 'UserID': aid });
            var varRoleURL = "ResendInvite";
            jQuery.support.cors = true;
            pageLoader();
            $.ajax({
                url: url + varRoleURL,
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'userData': jsondata })
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
                removepageloader();
                notifySuccess(data.response);
                setTimeout(
                    function () {
                        window.location.href = "../users";
                    },
                           setTimeOutTimer);
            }).fail(function (error) {
                $('.overlay').remove();
                removepageloader();
                notifyError(error);
            });
        }
        else {
            notifyError("[|Please select at least one user|]");
        }
    };

    selfUser.Password = ko.observable().extend({ required: { message: "[|Password is required|]" }});
    //selfExport.UserIds = ko.observable(fnGetCheckedContactIDs())


    selfUser.saveUserPassword = function () {
        removeMailRules();
        selfUser.Status.rules.remove(function (item) {
            return item.rule = "required";
        });
        var result = ko.validation.group(selfUser, { deep: true });
        if (!selfUser.isValid()) {
            result.showAllMessages(true);
            return false;
        }

        var statusList = fnGetSelectedUsers('chkuser');

        if (statusList.length == 0) {
            notifyError("[|Please select at least one User|]");
            return;
        }

        var stat = [];

        $.each(statusList, function (index, data) {           
            stat.push(data.Status);            
        });

        var searchStr1 = "1";
        var searchStr2 = "2";

        //var aid = checkedvalues;
        var jsondata = JSON.stringify({ 'UserIDs': checkedvalues, 'Password': selfUser.Password() });
        var action = "SaveUserPassword";
        jQuery.support.cors = true;
        pageLoader();
        $.ajax({
            url: url + action,
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'userData': jsondata })
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
            if ((stat.indexOf(searchStr1) > -1 && stat.indexOf(searchStr2) > -1) || stat.indexOf(searchStr2) > -1)
                notifySuccess("[|Users are made active and passwords has been set.|]");
            else if (stat.indexOf(searchStr1) > -1)
                notifySuccess("[|Passwords have been set to the selected User(s).|]");
            setTimeout(
                function () {
                    removepageloader();
                    window.location.href = "../users";
                }, setTimeOutTimer);
        }).fail(function (error) {
            removepageloader();
            notifyError(error);
        });
    };
}

