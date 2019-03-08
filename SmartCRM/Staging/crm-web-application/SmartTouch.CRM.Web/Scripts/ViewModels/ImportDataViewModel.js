var ImportDataViewModel = function (data, url, Users, neverBouncePermission) {
    selfImportdata = this;
    ko.mapping.fromJS(data, {}, selfImportdata);
    selfImportdata.Fields = ko.observableArray(data.Fields);


    selfImportdata.Users = ko.observableArray();
    selfImportdata.Users(Users);
    selfImportdata.templ = kendo.template('#if (data.OwnerId == 0 )  { # <span>[|Select..|]</span># } ##if (data.IsDeleted == false )  { # <span>#= OwnerName #</span># } # #if (data.IsDeleted == true )  { # <span style="color:red;">#= OwnerName #</span># } #');

    selfImportdata.OwnerId = ko.observable(data.OwnerId);

    duplicateLogic = {
        OnlyEmail: 1,
        EmailAndFullName: 2
    }

    var mappedcolumnsids = [];
    selfImportdata.AllfiledsWithoutAsterik = ko.observableArray(data.Fields);
    selfImportdata.TagsList = ko.observableArray(data.TagsList);
    selfImportdata.AllFields = ko.computed({
        read: function () {
            ko.utils.arrayForEach(data.Fields, function (item) {
                if (item.AccountID == null)
                    item.Title = item.Title + " *";
                else if (item.AccountID != null && item.IsDropdownField == true)
                    item.Title = item.Title + " *";
            });
            return data.Fields;
        },
        write: function (newValue) { },
        owner: this
    });

    selfImportdata.NeverBounceValidation = ko.observable(data.NeverBounceValidation);
    selfImportdata.IncludeInReports = ko.observable(true);
    selfImportdata.UpdateOnDuplicate = ko.observable(data.UpdateOnDuplicate.toString().toLowerCase());
    selfImportdata.DuplicateLogic = ko.observable(data.DuplicateLogic.toString());
    selfImportdata.Messge = ko.observable();
    selfImportdata.setNeverBounce = function (data, event) {
        selfImportdata.NeverBounceValidation(event.target.checked);
        console.log(event.target.checked);
        if (event.target.checked)
            document.getElementById("nbprice").style.display = "block";
        else
            document.getElementById("nbprice").style.display = "none";
    }
    masterFields = function () {
        return new Object(selfImportdata.AllFields())
    };
    selfImportdata.Imports = ko.observableArray(data.Imports);
    selfImportdata.tempFields = ko.observableArray([]);
    selfImportdata.saveText = ko.observable('[|Import|]');
    var rem = function (value) {
        var fieldIndex = value.DropdownFields().map(function (e) { return e.Id + ''; }).indexOf(value.ContactFieldName() + '');
        if (fieldIndex > -1) {
            $.each(selfImportdata.Imports(), function (index, otherValue) {
                if (otherValue.ContactFieldName() != value.Id) {
                    var otherFieldIndex = otherValue.DropdownFields().map(function (e) { return e.Id + ''; }).indexOf(value.ContactFieldName() + '');
                    if (otherFieldIndex > -1) {
                        otherValue.DropdownFields().splice(otherFieldIndex, 1);
                        return true;
                    }
                }
            });
        }
    };

    selfImportdata.AllowInReports = function (value) {
        console.log(value.IncludeInReports());
        if (value.IncludeInReports()) {
            selfImportdata.Messge("[| All Contacts on this Import will appear and be counted in Dashboard Counts and the lead source Reports.|]");
            $("#myIncludeModal").modal('show');

        }
        else {
            selfImportdata.Messge("[| All Contacts on this Import will not appear or be counted in Dashboard Counts and Reports. |]");
            $("#myExcludeModal").modal('show');
        }
    }

    selfImportdata.IncludeInOkay = function () {
        $("#myIncludeModal").modal('hide');
    }

    selfImportdata.ExcludeInOkay = function () {
        $("#myExcludeModal").modal('hide');
    }

    selfImportdata.cancelInInclude = function () {
        selfImportdata.IncludeInReports(false);
    }

    selfImportdata.cancelInExclude = function () {
        selfImportdata.IncludeInReports(true);
    }

    var temp = function () {
        var errorCount = 0;
        $.each(selfImportdata.Imports(), function (index, value) {
            value.Title = ko.observable('');
            value.ContactFieldName = ko.observable(value.ContactFieldName);
            value.IsCustomField = ko.observable(false);
            value.IsDropDownField = ko.observable(false);
            var sheetcolumnname = value.SheetColumnName;
            value.SheetColumnName = ko.observable(sheetcolumnname);
            value.onSelect = function (e) {
                var dataItem = this.dataItem(e.item);
                value.IsCustomField(dataItem.IsCustomField);
                value.IsDropDownField(dataItem.IsDropdownField);
                value.Title(dataItem.Title);
            }

            if (sheetcolumnname.length > 2) {
                var fielddata = ko.utils.arrayFilter(data.Fields, function (item) {
                    return $.trim(item.Title).split(' ').join('').toLowerCase().indexOf($.trim(sheetcolumnname).split(' ').join('').toLowerCase()) !== -1;
                });

                var subfielddata = $.grep(fielddata, function (i, index) {
                    return mappedcolumnsids.indexOf(i.Id) == -1;
                });

                var field = subfielddata.length > 0 ? subfielddata[0] : null;
                if (field != null) {
                    value.ContactFieldName(field.Id);
                    value.IsDropDownField(field.IsDropdownField);
                    value.IsCustomField(field.IsCustomField);
                    mappedcolumnsids.push(field.Id);
                }
            }

            value.ContactFieldName.subscribe(function (e1) {

                if ($.trim(e1) != "") {
                    errorCount = 0;
                    var resultSet = $.grep(selfImportdata.Imports(), function (e) {
                        return e.ContactFieldName() == e1 && e.IsDropDownField() == false;
                    });

                    var resultSet2 = $.grep(selfImportdata.Imports(), function (e) {
                        return e.ContactFieldName() == e1 && e.IsDropDownField() == true;
                    });

                    if (resultSet.length > 1 || resultSet2.length > 1) {
                        value.ContactFieldName('');
                        if (errorCount == 0)
                            notifyError('[|Duplicate map|]');
                        errorCount = errorCount + 1;
                    }
                }

            });
        });
    };
    temp(selfImportdata.Imports());

    selfImportdata.fileName = ko.observable(data.FileName);
    selfImportdata.saveImportdata = function () {
        var resultSet = $.grep(selfImportdata.Imports(), function (e) {
            return e.ContactFieldName() != "";
        });
        var continuetosave = true;
        if (selfImportdata.DuplicateLogic() == duplicateLogic.OnlyEmail) {
            var resultSet2 = $.grep(resultSet, function (e) {
                return e.ContactFieldName() == "7" && e.IsDropDownField() == false;
            });
            if (resultSet2.length < 1) {
                notifyError("[|Please select Email field|]");
                continuetosave = false;
            }

        } else if (selfImportdata.DuplicateLogic() == duplicateLogic.EmailAndFullName) {
            var resultSet2 = $.grep(resultSet, function (e) {
                return (e.ContactFieldName() == "1" || e.ContactFieldName() == "2" || e.ContactFieldName() == "7") && e.IsDropDownField() == false;
            });
            if (resultSet2.length < 3) {
                notifyError("[|Please select Email, First Name and Last Name fields|]");
                continuetosave = false;
            }
        }

        if (continuetosave) {
            var confirmationMessage = "A Lead Source for each Contact is required to import a list. If you do not have a Lead Source mapped, the default value used will be Imports. To proceed click OK, or Cancel to exit.";
            alertifyReset();
            alertify.confirm(confirmationMessage, function (e) {
                if (e) {
                    if (neverBouncePermission == 'False')
                        selfImportdata.NeverBounceValidation(false);

                    selfImportdata.Fields([]);
                    var jsondata = ko.toJSON(selfImportdata);
                    var action;
                    action = "InsertImportData";
                    selfImportdata.saveText('[|Saving..|]');

                    pageLoader();

                    $.ajax({
                        url: url + action,
                        type: 'post',
                        dataType: 'json',
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify({ 'importViewModel': jsondata })
                    }).then(function (data) {
                        var filter = $.Deferred();
                        if (data.success)
                            filter.resolve(data);
                        else
                            filter.reject(data.error);
                        return filter.promise();
                    }).done(function (data) {
                        selfImportdata.saveText('[|Import|]');
                        notifySuccess('[|File successfully queued for Import. Please check Import screen for progress.|]');
                        setTimeout(
                                      function () {
                                          removepageloader();
                                          window.location.href = "/importdata";
                                      }, 5000);
                    }).fail(function (err) {
                        removepageloader();
                        notifyError(err);
                    });

                }
                else {
                    continuetosave = false;
                    window.location.href = "/importdata";
                }
            });
        }

    };

}
