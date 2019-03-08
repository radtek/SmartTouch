var ContactsResultsViewModel = function (searchFields, searchName, isSavedSearch, searchId, isPreconfigured, isFavorite, searchTitle, entityId, entityType, selectedFields, contactUrl, showingType, fromSSGrid, reportName, IsDynamicGrid) {
    selfContactsResults = this;
    ko.mapping.fromJS(searchFields, {}, selfContactsResults);

    selfContactsResults.IsDynamicGrid = ko.observable(IsDynamicGrid || false);
    var showingFields = [{ Text: "People", Value: "0" }, { Text: "Companies", Value:"1" },
        { Text: "People and Companies", Value: "2"}, { Text: "My Contacts", Value:"3" }];
    
    selfContactsResults.ShowingFields = ko.observableArray(showingFields);
    selfContactsResults.ReportName = ko.observable(reportName);
    selfContactsResults.ShowingType = ko.observable(showingType || 2);

    selfContactsResults.ShowingType.subscribe(function (value) {
        localStorage.setItem("searchcontent", value);
        $("#grid").data("kendoGrid").dataSource.read();
        appendCheckbox();
        selfContactsResults.SaveColumnPreferences(selfContactsResults.Fields());
    });
    
    selfContactsResults.SearchName = ko.pureComputed({
        read: function () {
            if (searchName != null || searchName != 'undefined') {
                if (searchName.charAt(0) == '"')
                    searchName = searchName.replace('"', '');
                if (searchName.charAt(searchName.length - 1) == '"')
                    searchName = searchName.replace(/"$/, '');
            }
            return searchName;
        }
    });

    selfContactsResults.SearchTitle = ko.pureComputed({
        read: function () {
            if (searchTitle != null || searchTitle != 'undefined') {
                if (searchTitle.charAt(0) == '"')
                    searchTitle = searchTitle.replace('"', '');
                if (searchTitle.charAt(searchTitle.length - 1) == '"')
                    searchTitle = searchTitle.replace(/"$/, '');
            }
            return searchTitle;
        }
    });

    selfContactsResults.IsSavedSearch = ko.observable(isSavedSearch);
    selfContactsResults.SearchId = ko.observable(searchId);
    selfContactsResults.FromSSGrid = ko.observable(fromSSGrid);
    selfContactsResults.IsPreconfigured = ko.observable(isPreconfigured);
    selfContactsResults.IsFavorite = ko.observable(isFavorite);
    
    selfContactsResults.EntityId = ko.observable(entityId);
    selfContactsResults.EntityType = ko.observable(entityType);
    
    selfContactsResults.ValueOptions = ko.observableArray([]);
    selfContactsResults.SearchFields = ko.observableArray(JSON.parse(searchFields));

    selfContactsResults.selectedFields = ko.observableArray(["1", "2", "3", "7"]);

    selfContactsResults.Fields = ko.computed({
        read: function () { return selfContactsResults.selectedFields() },
        write: function (n) {
            var myGrid = $('#grid').data('kendoGrid');
            $.each(n, function (i,val) {
                if (selfContactsResults.selectedFields().indexOf(val) > -1) { }     //Field already exist in selectedFields
                else {
                    if (val > 200)
                        selfContactsResults.BindColumns(val);
                    myGrid.thead.find('th[data-fieldid="' + val + '"]').toggle();
                    myGrid.tbody.find('td[data-fieldid="' + val + '"]').toggle();
                }
            });
            $.each(selfContactsResults.selectedFields(), function (i, val) {    //This condition is used to hide the de-selected column by comparing previously selected fields with new fields(n).
                if (n.indexOf(val) > -1) { }
                else {
                    myGrid.thead.find('th[data-fieldid="' + val + '"]').toggle();
                    myGrid.tbody.find('td[data-fieldid="' + val + '"]').toggle();
                }
            });
            var selectedFieldsStringArray = n.map(String);  //maps every element in n to String 
            selfContactsResults.selectedFields(selectedFieldsStringArray);
            selfContactsResults.SaveColumnPreferences(n);
        },
        owner : this
    });

    selfContactsResults.BindColumns = function (id) {
        var grid = $("#grid").data("kendoGrid");
        var tableHeaders = grid.thead.find("th[data-type='custom']");
        var fields = tableHeaders.filter(function () { return $(this).data("fieldid") == id; });
        var hasDataItems = grid.dataItems().length > 0;
        selfContactsResults.processColumnsBinding(grid, fields, hasDataItems);
    };

    selfContactsResults.processColumnsBinding = function (grid, fields, hasDataItems) {
        var startTimeInner = new Date().getTime();
        if (fields) {
            $.each(fields, function (i, v) {
                var index = $(this).data('index');
                var fieldId = $(this).data('fieldid');
                var inputTypeId = $(this).data('fieldinputtypeid');
                var isPhoneField = $(this).data('isphonefield');
                var tableRows = grid.tbody.find('tr');
                $.each(tableRows, function (ix, val) {
                    var fieldValue = "";
                    if (hasDataItems) {
                        var customFields = grid.dataItems()[ix].CustomFields;
                        var valueOptions = selfContactsResults.ValueOptions();
                        if (customFields != null && customFields.length > 0) {
                            var matchedField = ko.utils.arrayFirst(customFields, function (field, ai) {
                                return field.CustomFieldId == fieldId;
                            });
                            if (matchedField != null) {
                                if (inputTypeId == 'checkbox' || inputTypeId == 'radio' || inputTypeId == 'dropdown' || inputTypeId == 'multiselectdropdown')
                                    fieldValue = getCustomFieldValueOption(matchedField.Value, valueOptions);
                                else if (inputTypeId == 'time')
                                    fieldValue = getTime(matchedField.Value_Date);
                                else if (inputTypeId == 'date' || inputTypeId == 'datetime')
                                    fieldValue = formatDate(matchedField.Value, inputTypeId);
                                else
                                    fieldValue = matchedField.Value;
                            }
                        }
                        if (isPhoneField == "True") {
                            fieldValue = getPhoneNumber(fieldId, grid.dataItems()[ix].Phones);
                        }
                        $(this).find('td').eq(index).text(fieldValue).attr('title', fieldValue);
                    }
                });
                appendCheckbox();
            });
        }
        var endTimeInner = new Date().getTime();
        var time = endTimeInner - startTimeInner;
    };

    var getCustomFieldValueOption = function (valueOptionId, valueOptions) {
        var optionText = "";
        var optionIds = valueOptionId.toString().split('|');
        $.each(optionIds, function (outerIndex, optionId) {
            var option = ko.utils.arrayFirst(valueOptions, function (opt, i) {
                return opt.CustomFieldValueOptionId == optionId;
            });
            if (option != null)
                optionText = outerIndex > 0 ? optionText + ", " + option.Value : option.Value;
        });
        return optionText;
    }

    var getTime = function (value) {
        var time = value;
        var date = new Date(value).ToUtcUtzDate();
        if (date != 'Invalid Date') {
            time = kendo.toString(new Date(moment(time).toDate()).ToUtcUtzDate(), "T")
        }
        return time;
    }

    function formatDate(date, inputTypeId) {
        if (isNaN(Date.parse(date))) {
            return "";
        }
        else {
            var dateFormat = readCookie("dateformat");
            if (dateFormat == null || dateFormat == 'undefined')
                return new Date(Date.parse(date)).ToUtcUtzDate().toDateString();
            else {
                var utzDate = new Date(moment(date).toDate()).ToUtcUtzDate();
                return kendo.toString(utzDate, dateFormat + (inputTypeId == 'date' ? "" : " hh:mm tt"));
            }
        }
    }

    var getPhoneNumber = function (fieldId, phones) {
        var number = "";
        if (phones == null)
            return number;

        var phonesList = ko.utils.arrayFilter(phones, function (phone) {
            return phone.PhoneType == fieldId;
        });
        var phoneNumbers = "";
        if (phonesList.length > 0)
            phoneNumbers = ko.utils.arrayMap(phonesList, function (phone) {
                if (phone.IsPrimary == true)
                    return (phone.CountryCode != null ? "+" + phone.CountryCode + " " : "") + formatPhone(phone.Number) + (phone.Extension != null ? " Ext. " + phone.Extension + " " : "") + " *";
                else
                    return (phone.CountryCode != null ? "+" + phone.CountryCode + " " : "") + formatPhone(phone.Number) + (phone.Extension != null ? " Ext. " + phone.Extension + " " : "");
            }).join(", ");
        number = phoneNumbers;
        return number;
    };

    function formatPhone(phonenum) {
        var regexObj = /^(?:\+?1[-. ]?)?(?:\(?([0-9]{3})\)?[-. ]?)?([0-9]{3})[-. ]?([0-9]{4})$/;
        if (regexObj.test(phonenum)) {
            var parts = phonenum.match(regexObj);
            var phone = "";
            if (parts[1]) { phone += "(" + parts[1] + ") "; }
            phone += parts[2] + "-" + parts[3];
            return phone;
        }
        else {
            return phonenum;
        }
    }

    selfContactsResults.SaveColumnPreferences = function (n) {
        var entityId = 0;
        var type = 0;
        var fields = n;
        var showingType = 1;
        if (selfContactsResults.IsSavedSearch() == "True") {
            entityId = selfContactsResults.SearchId();
            type = 1;
        }
        else {
            entityId = selfContactsResults.EntityId();
            type = selfContactsResults.EntityType();
        }

        if (entityId != undefined && entityId != null && entityId != 0 && n != null && n.length > 0) {
            //showingType = $('#contactTypes').data('kendoDropDownList').value();
            showingType = selfContactsResults.ShowingType();
            $.ajax({
                url: contactUrl + "/SaveColumnPreferences",
                type: 'get',
                dataType: 'json',
                traditional: true,
                data: { 'entityId': entityId, 'entityType': type, 'fields': fields, 'showingType': showingType },
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    console.log("Successfully saved your column configuration");
                },
                error: function (error) {
                    console.log(error);
                }
            });
        }
    };
}