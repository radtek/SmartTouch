var RolePermissionsViewModel = function (data, weburl) {

    var selfRolePermissions = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfRolePermissions));

    selfRolePermissions.Roles = ko.observableArray(data.Roles);
    selfRolePermissions.Modules = ko.observableArray(data.Modules);
    selfRolePermissions.SelectedRole = ko.observable(data.SelectedRole);
    selfRolePermissions.roleValidation = selfRolePermissions.SelectedRole.extend({ required: { message: "[|Please select Role|]" } });
    selfRolePermissions.SelectedRole.subscribe(function (roleId) {
        if (roleId != null && roleId != "") {
            $.ajax({
                url: weburl + "GetRolePermissionsForRole",
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'roleId': roleId })
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
                ko.utils.arrayForEach(selfRolePermissions.Modules(), function (item) {
                    item.IsSelected(false);
                    checkedEvent(item.ModuleId(), 'remove');
                    if (item.SubModules != null) {
                        ko.utils.arrayForEach(item.SubModules(), function (subItem) {
                            subItem.IsSelected(false);
                            checkedEvent(subItem.ModuleId(), 'remove');
                        })
                    }
                });
                $.each(data.response, function (index, value) {
                    var module = ko.utils.arrayFilter(selfRolePermissions.Modules(), function (item) {
                        if (item.ModuleId() == value) {
                            item.IsSelected(true);
                            checkedEvent(item.ModuleId(), 'add');
                        }
                        if (item.SubModules != null) {
                            ko.utils.arrayFilter(item.SubModules(), function (subItem) {
                                if (subItem.ModuleId() == value) {
                                    subItem.IsSelected(true);
                                    checkedEvent(subItem.ModuleId(), 'add');
                                }
                            });
                        }
                        return item.ModuleId() == value;
                    });
                    appendCheckbox();
                });

                $('.overlay').remove();
            }).fail(function (error) {
                $('.success-msg').remove();
                $('.overlay').remove();
                notifyError(error);
            });
            return true;
        }
        else return false;
    });

    checkedEvent = function (moduleId, addOrRemove) {
        var roleID = moduleId + 1;
        var roleName = ('#Checkbox2' + roleID);

        if (addOrRemove == 'remove') {
            $(roleName).removeAttr('checked');
            $(roleName).parent('label.checkbox').removeClass('checked');
        }
        else {
            $(roleName).attr('checked', 'checked');
            $(roleName).parent('label').addClass('checked');
        }
        appendCheckbox();
    };

    selfRolePermissions.parentModuleChecked = function (module) {
        if (module !== null) {
            if (module.SubModules().length > 0) {
                $.each(module.SubModules(), function (index, item) {
                    item.IsSelected(module.IsSelected());
                    if (module.IsSelected() === true) {
                        checkedEvent(item.ModuleId(), 'add');
                    }
                    else checkedEvent(item.ModuleId(), 'remove');
                })
            }
        }
        return true;
    };

    selfRolePermissions.childModuleChecked = function (parent, child) {
        if (child.IsSelected() === true) {
            parent.IsSelected(true);
            checkedEvent(parent.ModuleId(), 'add');
        }
        return true;
    }

    selfRolePermissions.editRole = function () {
        checkedvalues = fnGetChkvalGrid('chkrole');
        if (checkedvalues != "") {
            if (checkedvalues.length == 1) {
                checkedvaluesstatus = fnGetChkvalStatus('chkrole')
                window.location.href = "roleconfiguration/" + checkedvalues;
            }
            else {
                notifyError("[|Please select only one role|]");
            }
        }
        else {
            notifyError("[|Please select only one role|]");
        }
    };

    selfRolePermissions.errors = ko.validation.group(selfRolePermissions);

    selfRolePermissions.saveUserPermissions = function () {
        selfRolePermissions.errors.showAllMessages();
        if (selfRolePermissions.errors().length > 0)
            return;
        var jsondata = ko.toJSON(selfRolePermissions);
        pageLoader();
        $.ajax({
            url: weburl + "InsertRolePermissions",
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'rolePermissionViewodel': jsondata })
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
            notifySuccess('[|Successfully saved Role Permissions|]');
            removepageloader();
            setTimeout(function () { window.location.href = '/roles' }, setTimeOutTimer);
        }).fail(function (error) {
            self.saveText('[|Save|]');
            removepageloader();
            notifyError(data.response);
        });
    };
};