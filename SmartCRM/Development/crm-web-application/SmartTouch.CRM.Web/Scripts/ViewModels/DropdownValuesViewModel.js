var DropdownValuesViewModel = function (data, url, service, isSTAdmin) {
    selfdropdownValues = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfdropdownValues));

    selfdropdownValues.DropdownValue = ko.observable();
    selfdropdownValues.DropdownName = ko.observable(data.DropdownName);
    selfdropdownValues.DropdownID = ko.observable(data.DropdownID);
    selfdropdownValues.DropdownValueID = ko.observable();
    selfdropdownValues.IsDefault = ko.observable(true);
    selfdropdownValues.IsSTAdmin = ko.observable(isSTAdmin);
    selfdropdownValues.SortID = ko.observable();
    selfdropdownValues.DropdownValuesList = ko.observableArray([]);

    selfdropdownValues.StageGroups = ko.observableArray();
    selfdropdownValues.SortorderID = ko.observable();
    selfdropdownValues.SortorderName = ko.observable();

    selfdropdownValues.OpportunityGroupID = ko.observable(1);
    //console.log(data.DropdownValuesList);
    selfdropdownValues.addDropdownObservable = function (dd) {
        var newDropdown = {
            DropdownValue: ko.observable(dd.DropdownValue),
            DropdownValueID: ko.observable(dd.DropdownValueID),
            DropdownID: ko.observable(dd.DropdownID),
            SortID: ko.observable(selfdropdownValues.DropdownValuesList().length + 1),
            IsDefault: ko.observable(dd.IsDefault),
            IsActive: ko.observable(dd.IsActive),
            Isvisible: ko.observable(true),
            DropdownValueTypeID: ko.observable(dd.DropdownValueTypeID),
            OpportunityGroupID: ko.observable(dd.OpportunityGroupID)
        };
        selfdropdownValues.DropdownValuesList.push(newDropdown);
    };

    selfdropdownValues.DropdownValuesList([])

    $.ajax({
        url: url + 'GetOppoertunityStageGroups',
        type: 'get',
        dataType: 'json',
        contentType: "application/json; charset=utf-8"
    }).then(function (response) {
        var filter = $.Deferred()
        if (response.success) {
            filter.resolve(response)
        }
        else {
            filter.reject(response.error)
        }
        return filter.promise()
    }).done(function (result) {
        var Groups = result.response;
        selfdropdownValues.StageGroups(Groups);
        $.grep(data.DropdownValuesList, function (e) {
            selfdropdownValues.addDropdownObservable(e);
        })
    }).fail(function (error) {
        notifyError(error);
    });

    selfdropdownValues.sortedInstances = ko.dependentObservable(function () {
        return selfdropdownValues.DropdownValuesList().slice().sort(selfdropdownValues.sortFunction);
    }, selfdropdownValues);

    selfdropdownValues.sortFunction = function (a, b) {
        return a.DropdownValue().toLowerCase() > b.DropdownValue().toLowerCase() ? 1 : -1;
    };


    selfdropdownValues.SortorderID.subscribe(function (val) {
        if (val == 1) {
            selfdropdownValues.sortedInstances = ko.dependentObservable(function () {
                return selfdropdownValues.DropdownValuesList().slice().sort(selfdropdownValues.sortFunction);
            }, selfdropdownValues);

            selfdropdownValues.sortFunction = function (a, b) {
                return a.DropdownValue().toLowerCase() > b.DropdownValue().toLowerCase() ? 1 : -1;
            };
            selfdropdownValues.DropdownValuesList(selfdropdownValues.sortedInstances());
        }
        else if (val == 2) {
            selfdropdownValues.DropdownValuesList(selfdropdownValues.sortedInstances().reverse());
        }
        else if (val == 3) {
            console.log(selfdropdownValues.DropdownValuesList());
            selfdropdownValues.DropdownValuesList(selfdropdownValues.DropdownValuesList().sort(function (a, b) {
                return a.IsActive() - b.IsActive()
            }).reverse());

        }
        else if (val == 4) {
            selfdropdownValues.DropdownValuesList(selfdropdownValues.DropdownValuesList().sort(function (a, b) {
                return a.IsActive() - b.IsActive()
            }));
        }
        else { }
    });


    selfdropdownValues.Sortorderlist = ko.observableArray([
     { SortorderID: 1, SortorderName: "[|A to Z|]" },
     { SortorderID: 2, SortorderName: "[|Z to A|]" },
     { SortorderID: 3, SortorderName: "[|Active|]" },
     { SortorderID: 4, SortorderName: "[|Inactive|]" },
    ]);

    selfdropdownValues.selectedDropdownValue = ko.observable();
    selfdropdownValues.selectDropdownValue = function (dropdownvalue) {
        selfdropdownValues.selectedDropdownValue(dropdownvalue);
    };

    selfdropdownValues.addDropdown = function () {

        var newDropdown = {
            DropdownValue: ko.observable(''),
            DropdownValueID: ko.observable(selfdropdownValues.DropdownValuesList().length + 1),
            DropdownID: ko.observable(data.DropdownID),
            SortID: ko.observable(selfdropdownValues.DropdownValuesList().length + 1),
            IsDefault: ko.observable(false),
            IsActive: ko.observable(true),
            Isvisible: ko.observable(true),
            DropdownValueTypeID: ko.observable(3),
            OpportunityGroupID: ko.observable(1),
            IsNewField: true
        };
        selfdropdownValues.DropdownValuesList.push(newDropdown);

        var $t = $('.modal-body');
        $t.animate({ "scrollTop": $('.modal-body')[0].scrollHeight }, "slow");

    };

    selfdropdownValues.dropdowndefaultvalue = ko.observable();
    selfdropdownValues.StateChanged = function (status, id) {
        $.each(selfdropdownValues.DropdownValuesList(), function (index, dropdownvalue) {
            if (dropdownvalue.DropdownValueID() == id) {
                if (status == true)
                    dropdownvalue.IsActive(false);
                else
                    dropdownvalue.IsActive(true);
            }
        });
    };

    selfdropdownValues.removeDropdownValue = function (obj) {

        var confirmmessage = "[|Are you sure you want to delete this record|]?"
        if (obj.DropdownID() == 5)
            confirmmessage = "[|Are you sure you want to delete this Lead Source|]?"
        if (obj.DropdownID() == 1)
            confirmmessage = "[|Are you sure you want to delete this Phone Number Type|]?"
        if (obj.DropdownID() == 2)
            confirmmessage = "[|Are you sure you want to delete this Address Type|]?"
        if (obj.DropdownID() == 3)
            confirmmessage = "[|Are you sure you want to delete this Life Cycle|]?"
        if (obj.DropdownID() == 4)
            confirmmessage = "[|Are you sure you want to delete this Partner Type|]?"
        if (obj.DropdownID() == 6)
            confirmmessage = "[|Are you sure you want to delete this Opportunity Stage|]?"
        if (obj.DropdownID() == 7)
            confirmmessage = "[|Are you sure you want to delete this Community|]?"
        if (obj.DropdownID() == 8)
            confirmmessage = "[|Are you sure you want to delete this Tour Type|]?"
        if (obj.DropdownID() == 9)
            confirmmessage = "[|Are you sure you want to delete this Relationship Type|]?"
        if (obj.DropdownID() == 11)
            confirmmessage = "[|Are you sure you want to delete this Note Category|]?"

        alertifyReset("Delete record", "Cancel");
        alertify.confirm(confirmmessage, function (e) {
            if (e) {

                if (obj.IsDefault() == false)

                    selfdropdownValues.DropdownValuesList.remove(obj);
                else
                    notifyError("[|Default value can't be deleted|]");
            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }
        });

    };


    selfdropdownValues.saveText = ko.observable('Save');
    $.each(selfdropdownValues.DropdownValuesList(), function (index, value) {
        value.SortID.extend({ number: true, min: 1, required: true });
        value.DropdownValue.extend({ required: { message: "[|DropdownValue is required|]" }, maxLength: 50 });
        value.Isvisible = ko.observable(false);
        if (parseInt(value.DropdownValueTypeID()) == 3)
            value.Isvisible(true);
        selfdropdownValues.errors = ko.validation.group(value);
    });

    selfdropdownValues.errors = ko.validation.group(selfdropdownValues.DropdownValuesList(), { deep: true });

    selfdropdownValues.saveDropdownValue = function () {
        selfdropdownValues.errors.showAllMessages();
        if (selfdropdownValues.errors().length > 0)
            return;

        // To check the Duplicate DropdownValues,SortOrdrId's
        selfdropdownValues.checkDuplicatesDropdownvalue = ko.observableArray();
        selfdropdownValues.checkDuplicateSortOrder = ko.observableArray();
        //To Check Empty text of Newly Added Dropdown Value
        var CheckEmpty = false;
        var CheckActive = false;
        var CheckDefault = true;
        var CheckIsDefault = false;
        $.each(selfdropdownValues.DropdownValuesList(), function (index, dropdown) {

            if (dropdown.IsActive() == true)
                CheckActive = true;
            if (dropdown.IsDefault() == true && dropdown.IsActive() == false)
                CheckDefault = false;
            if (dropdown.IsDefault() == true)
                CheckIsDefault = true;

            if (dropdown.DropdownValue() != "") {
                selfdropdownValues.checkDuplicatesDropdownvalue().push(dropdown.DropdownValue().toLowerCase());
                selfdropdownValues.checkDuplicateSortOrder().push(parseInt(dropdown.SortID));
            }
            else {
                CheckEmpty = true;
            }
            //if (dropdown.IsNewField)
            //    dropdown.DropdownValueID(0);
        });

        if (CheckEmpty) {
            notifyError("[|Please enter values in newly added dropdown value.|]");
            return;
        }
        if (!CheckActive) {
            notifyError("[|Please make at least one dropdown value as active.|]");
            return;
        }
        if (!CheckDefault) {
            notifyError("[|Please make at least one dropdown value as default.|]");
            return;
        }
        if (!CheckIsDefault) {
            notifyError("[|Please make at least one dropdown value as default.|]");
            return;
        }

        selfdropdownValues.IsduplicateDrodownValue = ko.dependentObservable(function () {
            return ko.utils.arrayGetDistinctValues(selfdropdownValues.checkDuplicatesDropdownvalue());
        }, selfdropdownValues);

        selfdropdownValues.IsduplicateSortorder = ko.dependentObservable(function () {
            return ko.utils.arrayGetDistinctValues(selfdropdownValues.checkDuplicateSortOrder());
        }, selfdropdownValues);

        if (selfdropdownValues.IsduplicateDrodownValue().length != selfdropdownValues.DropdownValuesList().length
            || selfdropdownValues.IsduplicateSortorder().length != selfdropdownValues.DropdownValuesList().length) {
            notifyError("[|Duplicate values are not allowed.|]");
            return;
        }

        var jsondata = ko.toJSON(selfdropdownValues);
        var action;
        action = "InsertDropdown";

        pageLoader();

        $.ajax({
            url: url + action,
            type: "post",
            dataType: 'json',
            data: { 'dropdownViewModel': jsondata }
        }).then(function (response) {

            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            removepageloader();
            setTimeout(function () {
                appendCheckbox();
            }, 500);
            // Process success result      
            notifySuccess('[|Saved successfully.|]');
            setTimeout(
                function () {
                    window.location.href = "/dropdownfields";
                }, setTimeOutTimer);

        }).fail(function (error) {
            removepageloader();
            notifyError(error);
        });
    };

    selfdropdownValues.DefaultDropdownValueIndex = ko.observable();
    selfdropdownValues.DefaultDropdownValueID = ko.computed({
        read: function () {
            var tempID = 0;

            $.each(selfdropdownValues.DropdownValuesList(), function (index, dropdown) {
                if (dropdown.IsDefault() == true) {
                    tempID = dropdown.DropdownValueID();
                    selfdropdownValues.DefaultDropdownValueIndex(index);
                }
            });
            return tempID + ""; //Converted to string because the checked property in cshtml will work only the comparable property for its 'value' is string.
        },
        write: function (newValue) {
            $.each(selfdropdownValues.DropdownValuesList(), function (index, dropdown) {
                if (dropdown.DropdownValueID() == newValue) {
                    dropdown.IsDefault(true);
                }
                else {
                    dropdown.IsDefault(false);
                }
            });
        },
        owner: this
    });

    ko.bindingHandlers.sortableList = {
        init: function (element, valueAccessor) {
            var list = valueAccessor();
            $(element).sortable({
                update: function (event, ui) {
                    var item = ui.item;
                    var position = ko.utils.arrayIndexOf(ui.item.parent().children(), ui.item[0]);
                    if (position >= 0) {
                        list.remove(item);
                        list.splice(position, 0, item);
                    }
                    ui.item.remove();
                }
            });
        }
    };
};