var customFieldsViewModel = function (data, webService) {
    selfCustomFields = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfCustomFields));


    var fieldStatus = {
        ACTIVE: 201,
        DRAFT: 202,
        CLOSED: 203,
        DELETED: 204
    };

    //var tabTemplate = ko.mapping.fromJS(data.TabTemplate);
    //var sectionTemplate = ko.mapping.fromJS(data.SectionTemplate);
    //var customFieldTemplate = ko.mapping.fromJS(data.CustomFieldTemplate);
    //var valueOptionTemplate = ko.mapping.fromJS(data.ValueOptionTemplate);
    selfCustomFields.TempValueOptions = ko.observableArray([]);
    selfCustomFields.CurrentView = ko.observable(data.CustomFieldTabs.length > 0 ? 0 : -1);
    selfCustomFields.temporaryField = ko.observableArray([]);
    selfCustomFields.CurrentFieldIndex = ko.observable();
    //var makeChildrenObservable = {
    //    children: {
    //        create: function (options) {
    //            if (!options.data)
    //                return null;
    //            return ko.mapping.fromJS(options.data);
    //        }
    //    }
    //}

    selfCustomFields.CustomFieldTabs = ko.mapping.fromJS(data.CustomFieldTabs, { observableArray: 'Sections' });
    selfCustomFields.CustomFieldTabs.subscribe(function (changes) {
        //When sorted the dragged item is removed and added by knockout. changes[0].status gives us the item being sorted
        if(changes[0].status == "added")
            selfCustomFields.CurrentView(changes[0].index);
        else
            selfCustomFields.CurrentView(0);
    }, null, "arrayChange");

    selfCustomFields.Dateformat = function () {
        return readCookie("dateformat");
    }

    selfCustomFields.FieldStatuses = [
    { Id: 201, Status: '[|Active|]' },
    { Id: 202, Status: '[|Paused|]' },
    { Id: 203, Status: '[|Closed|]' }
    ];

    selfCustomFields.FieldTypes = ko.observableArray([
        { Id: 1, Type: '[|Check Box|]' },
        //{ Id: 2, Type: 'DateTime' },
        { Id: 3, Type: '[|Email|]' },
        { Id: 5, Type: '[|Number|]' },
        { Id: 6, Type: '[|Radio|]' },
        { Id: 8, Type: '[|Text|]' },
        { Id: 9, Type: '[|Time|]' },
        { Id: 10, Type: '[|URL|]' },
        { Id: 11, Type: '[|Dropdown|]' },
        { Id: 12, Type: '[|Multi select dropdown|]' },
        { Id: 13, Type: '[|Date|]' },
        { Id: 14, Type: '[|Text Area|]' }
    ]);

   

    selfCustomFields.SortorderID = ko.observable();
    selfCustomFields.Sortorderlist = ko.observableArray([
     { SortorderID: 1, SortorderName: "[|A to Z|]" },
     { SortorderID: 2, SortorderName: "[|Z to A|]" },

    ]);

  

    selfCustomFields.changeTab = function (tabIndex) {
        selfCustomFields.CurrentView(tabIndex);
    }
    
    ko.utils.arrayForEach(selfCustomFields.CustomFieldTabs(), function (tab) {
        ko.utils.arrayForEach(tab.Sections(), function (section) {
            ko.utils.arrayForEach(section.CustomFields(), function (fields) {
                
                if(fields.ValueOptions().length > 0)
                {
                    fields.ValueOptions(fields.ValueOptions().sort(function (a, b) {
                        return a.Order() > b.Order() ? 1 : -1;
                    }));
                }
            })
        })
    })

    ko.utils.arrayForEach(selfCustomFields.CustomFieldTabs(), function (tab) {
        ko.utils.arrayForEach(tab.Sections(), function (section) {
            section.SortorderID.subscribe(function (val) {
                {
                    if(val == 1)
                    {
                        section.CustomFields(section.CustomFields().sort(function (a, b) {
                            return a.Title().toLowerCase() > b.Title().toLowerCase() ? 1 : -1;
                        }));
                    }
                    else 
                    {
                        section.CustomFields(section.CustomFields().sort(function (a, b) {
                            return a.Title().toLowerCase() > b.Title().toLowerCase() ? 1 : -1;
                        }).reverse());
                    }

                }
            })
        })
    })

    selfCustomFields.addTab = function () {
        var newTab = ko.mapping.fromJS(data.TabTemplate);
        newTab.Name = ko.observable("New Tab " + (selfCustomFields.CustomFieldTabs().length + 1));
        selfCustomFields.CustomFieldTabs.push(newTab);
        var newTabIndex = selfCustomFields.CustomFieldTabs().length - 1;
        selfCustomFields.CustomFieldTabs()[selfCustomFields.CustomFieldTabs().length - 1].SortID($("#tab-" + newTabIndex).index());
        selfCustomFields.changeTab(newTabIndex);
    }

    selfCustomFields.addSection = function (tabIndex) {
        var newSection = ko.mapping.fromJS(data.SectionTemplate);
        newSection.TabId(selfCustomFields.CustomFieldTabs()[tabIndex].CustomFieldTabId());
        newSection.Name = ko.observable("New Section " + (selfCustomFields.CustomFieldTabs()[tabIndex].Sections().length + 1));
        newSection.SortorderID = 0;
        selfCustomFields.CustomFieldTabs()[tabIndex].Sections.push(newSection);
    }

    var temp;

    selfCustomFields.addField = function (tabIndex, sectionIndex) {
        var newCustomField = ko.mapping.fromJS(data.CustomFieldTemplate);
        newCustomField.SectionId(selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFieldSectionId());
        newCustomField.FieldCode(newCustomField.Id);
        newCustomField.Title = ko.observable("New Field " + (selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields().length + 1));
        selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields.push(newCustomField);
        temp = false;
        selfCustomFields.editCustomField(tabIndex, sectionIndex, selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields().length - 1);
        delete newCustomField;
    }

    selfCustomFields.addValueOption = function (tabId, sectionId, fieldId) {
        var valueOption = ko.mapping.fromJS(data.ValueOptionTemplate);
        selfCustomFields.CustomFieldTabs()[tabId].Sections()[sectionId].CustomFields()[fieldId].ValueOptions.push(valueOption);
    }

    selfCustomFields.temporaryStorage = ko.observable('');

    var Index; var varvalue = ""; var TabIndex;

    selfCustomFields.editTabName = function (index) {
        Index = index;
        varvalue = "";
        selfCustomFields.temporaryStorage(selfCustomFields.CustomFieldTabs()[index].Name());
        $("#tabname" + index).addClass("hide");
        $("#tabnameinput" + index).removeClass("hide");
        $(".tab-controls, #addcustomtab, .addcustomfield, #addcustomfieldsection").addClass("hide");
    }

    selfCustomFields.optSave = function (index) {
        var isValidName = ko.utils.arrayFilter(selfCustomFields.CustomFieldTabs(), function (item) {
            if (item.CustomFieldTabId() != selfCustomFields.CustomFieldTabs()[index].CustomFieldTabId()) {
                return item.Name() == selfCustomFields.temporaryStorage();
            }
        }).length == 0;
        if (selfCustomFields.temporaryStorage().trim() == "") {
            notifyError("[|Tab name cannot be empty|]");
        }
        else
            if (isValidName) {
                selfCustomFields.CustomFieldTabs()[index].Name(selfCustomFields.temporaryStorage());
                $("#tabnameinput" + index).addClass("hide");
                $("#tabname" + index).removeClass("hide");
                $(".tab-controls, #addcustomtab, .addcustomfield, #addcustomfieldsection").removeClass("hide");
            }
            else {
                notifyError("[|Tab name cannot be duplicate|]");
            }
    };


    selfCustomFields.tabkeyup = function (index, event) {
        if (event.keyCode == 13) {
            varvalue = $('#tabnameinput' + Index).find('input').val();

            if (varvalue.trim() != "") {
                selfCustomFields.temporaryStorage(varvalue);
                if (selfCustomFields.temporaryStorage().trim() != "") {
                    var isValidName = ko.utils.arrayFilter(selfCustomFields.CustomFieldTabs(), function (item) {
                        if (item.CustomFieldTabId() != selfCustomFields.CustomFieldTabs()[Index].CustomFieldTabId()) {
                            return item.Name() == selfCustomFields.temporaryStorage();
                        }
                    }).length == 0;
                    if (isValidName) {
                        selfCustomFields.CustomFieldTabs()[Index].Name(selfCustomFields.temporaryStorage());
                        $("#tabnameinput" + Index).addClass("hide");
                        $("#tabname" + Index).removeClass("hide");
                        $(".tab-controls, #addcustomtab, .addcustomfield, #addcustomfieldsection").removeClass("hide");
                    }
                    else {
                        notifyError("[|Tab name cannot be duplicate|]");
                    }
                }
            }
            else {
                notifyError("[|Tab name cannot be empty|]");
            }
        }
    }

    selfCustomFields.optCancel = function (index) {
        $("#tabnameinput" + index).addClass("hide");
        $("#tabname" + index).removeClass("hide");
        $(".tab-controls, #addcustomtab, .addcustomfield, #addcustomfieldsection").removeClass("hide");
    };

    selfCustomFields.editSectionName = function (tabIndex, index) {
        Index = index; TabIndex = tabIndex; varvalue = "";
        selfCustomFields.temporaryStorage(selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[index].Name());
        $("#sectionname" + index).addClass("hide");
        $("#sectionnameinput-" + index).removeClass("hide");
        $(".tab-controls, #addcustomtab, .addcustomfield, #addcustomfieldsection").addClass("hide");
    }

    selfCustomFields.optSaveSectionName = function (tabIndex, index) {
        var isValidName = ko.utils.arrayFilter(selfCustomFields.CustomFieldTabs()[tabIndex].Sections(), function (item) {
            if (item.CustomFieldSectionId() != selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[index].CustomFieldSectionId()) {
                return item.Name() == selfCustomFields.temporaryStorage();
            }
        }).length == 0;
        if (selfCustomFields.temporaryStorage().trim() == "") {
            notifyError("[|Section name cannot be empty|]");
        }
        else if (isValidName) {
            selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[index].Name(selfCustomFields.temporaryStorage());
            $("#sectionname" + index).removeClass("hide");
            $("#sectionnameinput-" + index).addClass("hide");
            $(".tab-controls, #addcustomtab, .addcustomfield, #addcustomfieldsection").removeClass("hide");
        }
        else {
            notifyError("[|Section name cannot be duplicate|]");
        }
    };

    selfCustomFields.sectionkeyup = function (tabIndex, event) {
        if (event.keyCode == 13) {
            varvalue = $("#sectionnameinput-" + Index).find('input').val();

            if (varvalue.trim() != "") {
                selfCustomFields.temporaryStorage(varvalue);
                var isValidName = ko.utils.arrayFilter(selfCustomFields.CustomFieldTabs()[TabIndex].Sections(), function (item) {
                    if (item.CustomFieldSectionId() != selfCustomFields.CustomFieldTabs()[TabIndex].Sections()[Index].CustomFieldSectionId()) {
                        return item.Name() == selfCustomFields.temporaryStorage();
                    }
                }).length == 0;

                if (isValidName) {
                    selfCustomFields.CustomFieldTabs()[TabIndex].Sections()[Index].Name(selfCustomFields.temporaryStorage());
                    $("#sectionname" + Index).removeClass("hide");
                    $("#sectionnameinput-" + Index).addClass("hide");
                    $(".tab-controls, #addcustomtab, .addcustomfield, #addcustomfieldsection").removeClass("hide");
                }
                else {
                    notifyError("[|Section name cannot be duplicate|]");
                }
            }
            else {
                notifyError("[|Section name cannot be empty|]");
            }
        }
    }


    selfCustomFields.optCancelSectionName = function (tabIndex, index) {
        $("#sectionname" + index).removeClass("hide");
        $("#sectionnameinput-" + index).addClass("hide");
        $(".tab-controls, #addcustomtab, .addcustomfield, #addcustomfieldsection").removeClass("hide");
    };

    selfCustomFields.optSaveField = function (tabIndex, sectionIndex, fieldIndex) {
        var valueOptions = ko.observableArray([]);
        valueOptions(selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields()[fieldIndex].ValueOptions());
        var inputFieldTypeId = selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields()[fieldIndex].FieldInputTypeId();
        var duplicateOptionValue = [];
        duplicateOptionValue = selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields()[fieldIndex].ValueOptions();
        if (valueOptions().length == 0 && (inputFieldTypeId == 1 || inputFieldTypeId == 6 || inputFieldTypeId == 11 || inputFieldTypeId == 12)) {
            notifyError("[|Add at least one Option Value|]");
            return;
        }

        var emptyOptionValue = valueOptions().map(function (e) { return e.Value() == "" }).indexOf(true);
        if (emptyOptionValue >= 0) {
            notifyError("[|Option Value cannot be empty|]");
            return;
        }
        var OptionValueLength = valueOptions().map(function (e) { return e.Value().length > 120 }).indexOf(true);
        if (OptionValueLength >= 0) {
            notifyError("[|Option Value exceeding 120 characters|]");
            return;
        }
        if (duplicateOptionValue.length > 0) {
            for (var i = 0; i < duplicateOptionValue.length; i++) {
                for (var j = 0; j < duplicateOptionValue.length; j++) {
                    if (i != j) {
                        if (duplicateOptionValue[i].Value().toLocaleLowerCase() == duplicateOptionValue[j].Value().toLocaleLowerCase()) {
                            notifyError("[|Option Values cannot be duplicate|]");
                            return;
                        }
                    }
                }
            }
        }

        var fieldName = selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields()[fieldIndex];
        if (fieldName.Title().length > 75) {
            notifyError("[|Field Name exceeding 75 characters|]");
            return;
        }
        if (fieldName.Title() == "") {
            notifyError("[|Field Name cannot be empty|]");
            return;
        }

        temp = true;

        var duplicates = ko.utils.arrayFilter(selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields(), function (item) {
            if (selfCustomFields.temporaryField().length > 0)
                return item.Title() == selfCustomFields.temporaryField().Title();
        });
        var isValidName = duplicates.length <= 1;
        if (selfCustomFields.temporaryField().length > 0 && selfCustomFields.temporaryField().Title().trim() == "") {
            notifyError("[|Field Name cannot be empty|]");
        }
        else if (isValidName) {
            selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields()[fieldIndex] = this;
            $("#control-edit-" + sectionIndex + "-" + fieldIndex).addClass("hide");
            $("#control-form-group-" + sectionIndex + "-" + fieldIndex).removeClass("hide");
            selfCustomFields.temporaryField([]);
            selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields()[fieldIndex].Value(null);
            $(".tab-controls, #addcustomtab, .addcustomfield, #addcustomfieldsection").removeClass("hide");
        }
        else if(!isValidName) {
            notifyError("[|Field name cannot be duplicate|]");
        }

    };

    selfCustomFields.cancelAddingTheField = function (tabIndex, sectionIndex, fieldIndex) {
        selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields()[fieldIndex].ValueOptions(selfCustomFields.TempValueOptions());
        selfCustomFields.TempValueOptions([]);
        if (fieldIndex == selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields().length - 1 && temp == false) {
            $("#control-edit-" + sectionIndex + "-" + fieldIndex).addClass("hide");
            $(".tab-controls, #addcustomtab, .addcustomfield, #addcustomfieldsection").removeClass("hide");
            selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields.pop();
        }
        else {
             //selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields()[fieldIndex] = selfCustomFields.temporaryField;
            $("#control-edit-" + sectionIndex + "-" + fieldIndex).addClass("hide");
            $("#control-form-group-" + sectionIndex + "-" + fieldIndex).removeClass("hide");
            $(".addcustomfield").removeClass("hide");
            $(".tab-controls, #addcustomtab, .addcustomfield, #addcustomfieldsection").removeClass("hide");
        }

    };
    selfCustomFields.editCustomField = function (tabIndex, sectionIndex, fieldIndex) {
        selfCustomFields.TempValueOptions(ko.mapping.fromJS(selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields()[fieldIndex].ValueOptions()));
        selfCustomFields.temporaryField(ko.mapping.fromJS(ko.mapping.toJS(selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields()[fieldIndex])));
        $("#control-edit-" + sectionIndex + "-" + fieldIndex).removeClass("hide");
        $("#control-form-group-" + sectionIndex + "-" + fieldIndex).addClass("hide");
        $(".tab-controls, #addcustomtab, .addcustomfield, #addcustomfieldsection").addClass("hide");
    };

    selfCustomFields.deleteSection = function (tabIndex, index, sectionId) {
       
        if (sectionId > 0) {
            alertifyReset("Yes", "No");
            alertify.confirm("[|Deleting this section will delete all its corresponding custom fields data in contacts and forms.</br>Are you sure you want to delete this section|]?", function (e) {
                if (e) {
                    selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[index].StatusId(1);
                    $.each(selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[index].CustomFields(), function(index, value) {
                        value.StatusId(fieldStatus.DELETED);
                    });
                    $("#section" + index).addClass("hide");
                };
            });
        }
        else {
            selfCustomFields.CustomFieldTabs()[tabIndex].Sections().splice(index);
            if ($("#section" + index))
                $("#section" + index).addClass("hide");
        }

    };

    selfCustomFields.deleteField = function (tabIndex, sectionIndex, index, fieldId) {
        if (fieldId > 0) {
            var authToken = readCookie("accessToken");
            var valueOptionID = null;
            $.ajax({
                url: webService + '/savedsearchcustomfields',
                type: 'get',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                beforeSend: function(xhr) {
                    xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                },
                data: { 'fieldId': fieldId, 'valueOptionId': valueOptionID },
                success: function(data) {
                    if (data.Count > 0)
                        notifyError("[|The selected custom filed can not be deleted - The field is being used in " + data.Count + " Saved Search(s)|]");
                    else {
                        alertifyReset("Yes", "No");
                        alertify.confirm("[|Deleting this custom field will delete all its corresponding data in contacts and forms.</br>Are you sure you want to delete this custom field|]?", function(e) {
                            if (e) {
                                selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields.splice(index, 1);
                            };
                        });
                    }

                },
                error: function(data) {
                    console.log(data);
                }

            });

        }
        else
            selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields.splice(index, 1);
    };

    selfCustomFields.deleteValueOption = function (tabIndex, sectionIndex, fieldIndex, index) {
        
        var fieldId = selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields()[fieldIndex].FieldId();
        var valueOptionId = selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields()[fieldIndex].ValueOptions()[index].CustomFieldValueOptionId();
        var authToken = readCookie("accessToken");
        $.ajax({
            url: webService + '/savedsearchcustomfields',
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            beforeSend: function(xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + authToken);
            },
            data: { 'fieldId': fieldId, 'valueOptionId': valueOptionId },
            success: function(data) {
                if (data.Count > 0)
                    notifyError("[|The option value can not be deleted - The option value is being used in " + data.Count + " Saved Search(s)|]");
                else {
                    selfCustomFields.CustomFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields()[fieldIndex].ValueOptions.splice(index, 1);
                }

            },
            error: function(data) {
                console.log(data);
            }

        });
    };

    selfCustomFields.deleteTabContent = function (tabIndex) {
        $.each(selfCustomFields.CustomFieldTabs()[tabIndex].Sections(), function (index, value) {
            selfCustomFields.deleteSection(tabIndex, index, value.CustomFieldSectionId());
        });
    }

    selfCustomFields.errors = ko.validation.group(selfCustomFields);

    selfCustomFields.cancelChanges = function () {
        alertifyReset("Yes", "No");
        alertify.confirm("[|Any changes you made will be lost. Are you sure you want to cancel|]?", function (e) {
            if (e) {
                window.location.href = "/customfields";
            };
        });
    };

    selfCustomFields.deleteTab = function (index, tabId) {
        alertifyReset("Delete Tab", "Cancel");
        alertify.confirm("[|You will not be able to retrieve the tab back if you delete. </br> All its corresponding data in contacts and forms will be deleted.</br>Are you sure you want to delete this tab|]?", function (e) {
            if (e) {
                if (tabId == 0) {
                    window.location.href = "/customfields";
                    return;
                }
                $.each(selfCustomFields.CustomFieldTabs()[index].Sections(), function (sectionIndex, deletedSection) {
                    $.each(deletedSection.CustomFields(), function(fieldIndex, deletedField) {
                        deletedField.StatusId(fieldStatus.DELETED);
                    });
                });
                //var jsondata = JSON.stringify({ 'customFieldTabId': tabId });
                $.ajax({
                    url: "/customfields/delete",
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ 'customFieldTabId': tabId })
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
                    window.location.href = "/customfields";
                    notifySuccess("[|Successfully deleted the tab.|]");
                }).fail(function () {
                    notifyError("[|Tab could not be deleted.|]");
                });;
            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }
        });
    };

    selfCustomFields.saveAllCustomTabs = function () {

        selfCustomFields.errors.showAllMessages();
        if (selfCustomFields.errors().length > 0) {
            return;
        }

        var jsondata = ko.toJSON(selfCustomFields);
        pageLoader();
        var authToken = readCookie("accessToken");

        $.ajax({
            url: webService + '/customtabs/save',
            type: 'put',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + authToken);
            },
            data: jsondata,
            success: function () {
                notifySuccess('[|Successfully saved the custom fields|]');
                removepageloader();
                window.location.href = "/customfields";
            },
            error: function (response) {
                removepageloader();
                notifyError(response.responseText);
            }
        }).done(function () {
            removepageloader();
        });
    };
}