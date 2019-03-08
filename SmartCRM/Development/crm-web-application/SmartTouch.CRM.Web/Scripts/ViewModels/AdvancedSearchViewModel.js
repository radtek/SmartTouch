var AdvancedSearchViewModel = function (data, url, WEBSERVICE_URL, emailPermissions, gridVisible, isRunSearch, pageSize) {
    selfSearch = this;

    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfSearch));
    ko.validation.rules.minimumLength = {
        validator: function (filters, minimumLength) {
            return (filters.length >= minimumLength);
        },
        message: '[|Minimum one filter is required.|]'
    };
    ko.validation.registerExtenders();
    selfSearch.EmailPermission = ko.observable(emailPermissions);

    var PARTNER_TYPE_FIELD = 21;
    var LIFECYCLE_STAGE_FIELD = 22;
    var LEAD_SOURCE_FIELD = 24;
    var FIRST_LEAD_SOURCE_FIELD = 51;
    var LEAD_SCORE_FIELD = 26;
    var DO_NOT_EMAIL_FIELDID = 23;
    var PRIMARYEMAIL_FIELDID = 7;
    var FIRSTNAME_FIELDID = 1;
    var LASTNAME_FIELDID = 2;
    var LASTCONTACTED_THROUGH = 41;
    var LEADADAPTER = 61;
    //var CREATED_ON = 28;
    var OWNER_FIELD = 25;
    var ACTION_ASSIGNED_TO_FIELD = 67;
    var COUNTRY_FIELD = 20;
    var STATE_FIELD = 18;
    var NOTE_SUMMARY = 54;
    var LAST_NOTE_DATE = 55;
    var TOURTYPE_FIELDID = 56;
    var COMMUNITY_FIELDID = 42;
    var EMAIL_STATUS = 59;
    var ACTIONTYPE_FIELDID = 64;
    var NOTECATEGORY_FIELDID = 71;
    var LASTNOTECATEGORY_FIELDID = 72;

    var PARTNER_TYPE_DROPDOWN_ID = 4;
    var LIFECYCLE_TYPE_DROPDOWN_ID = 3;
    var LEAD_SOURCE_DROPDOWN_ID = 5;
    var TOURTYPE_DROPDOWNID = 8;
    var COMMUNITY_DROPDOWNID = 7;
    var ACTIONTYPE_DROPDOWNID = 10;
    var LAST_NOTE = 69;
    var NOTECATEGORY_DROPDOWNID = 11;

    //function setCookie(cname, cvalue, exdays) {
    //    var d = new Date();
    //    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    //    var expires = "expires=" + d.toUTCString();
    //    document.cookie = cname + "=" + cvalue + "; " + expires;
    //}

    var size = readCookie("savedsearchpagesize");
    selfSearch.PageSize = ko.observable(parseInt(size) || 10);
    selfSearch.SearchDefinitionName = ko.observable(data.SearchDefinitionName).extend({ required: { message: "[|Search Name is required.|]" } });
    selfSearch.IsInEditMode = ko.observable(false);
    selfSearch.SearchDefinitionID = ko.observable(data.SearchDefinitionID);
    selfSearch.IsFavoriteSearch = ko.observable(data.IsFavoriteSearch);
    selfSearch.IsPreConfiguredSearch = ko.observable(data.IsPreConfiguredSearch);
    selfSearch.LastRunDate = ko.observable(data.LastRunDate);
    selfSearch.CreatedOn = ko.observable(data.CreatedOn);
    selfSearch.TagsList = ko.observableArray(data.TagsList);
    selfSearch.SearchTags = ko.computed({
        read: function () {
            var tags = "";
            if (selfSearch.TagsList() != null) {
                $.each(selfSearch.TagsList(), function (index, value) {
                    if (value != null && value != "") {
                        if (tags != null && tags != "")
                            tags = tags + "," + value.TagName;
                        else
                            tags = tags + value.TagName;
                    }
                });
            }
            return tags;
        },
        write: function () { },
        owner: this
    });
    selfSearch.CustomFieldValueOptions = ko.observable([]);

    selfSearch.SearchPredicates = ko.observableArray([
       { SearchPredicateTypeID: 1, PredicateType: 'AND' },
       { SearchPredicateTypeID: 2, PredicateType: 'OR' },
       { SearchPredicateTypeID: 4, PredicateType: 'Custom' }
    ]);

    selfSearch.SearchPredicateTypeID = ko.observable(data.SearchPredicateTypeID);

    var getCustomPredicateScript = function () {
        if (selfSearch.SearchPredicateTypeID() == 4)
            return selfSearch.CustomPredicateScript();

        var operator = ko.utils.arrayFirst(selfSearch.SearchPredicates(), function (predicates) { return predicates.SearchPredicateTypeID == selfSearch.SearchPredicateTypeID() }).PredicateType;

        if (selfSearch.customScript() != null && selfSearch.customScript() != "" && selfSearch.SearchPredicateTypeID() == 4) {
            return selfSearch.customScript();
        }

        else if (selfSearch.SearchFilters().length > 0 && selfSearch.SearchPredicateTypeID() != 4) {
            var indices = [];
            $.each(selfSearch.SearchFilters(), function (index) {
                indices.push(index + 1);
            });
            return indices.join(" " + operator + " ");
        }
        else {
            return "1 " + operator + " 2";
        }

    };
    selfSearch.SearchPredicateTypeID.subscribe(function () {
        var script = getCustomPredicateScript();
        selfSearch.CustomPredicateScript(script);
    });

    selfSearch.enableCustomScript = ko.pureComputed({
        read: function () {
            if (selfSearch.SearchPredicateTypeID() == 4)
                return true;
            else
                return false;
        },
        write: function () { },
        owner: this
    });

    selfSearch.SearchFields = ko.computed({
        read: function () {
            ko.utils.arrayForEach(data.SearchFields, function (item) {
                //if (item.AccountID == null)
                //    item.Title = item.Title + " *";
                item.ComputedFieldId = item.FieldId + "" + item.IsDropdownField;
            });

            return data.SearchFields;
        },
        write: function () { },
        owner: this
    });

    selfSearch.templa = kendo.template('#if(data.AccountID == null){# <div class="defaults"> #= data.Title #</div> #} else if(data.AccountID != null && data.IsLeadAdapterField == false)' +
            '{# <div class = "custom"> #= data.Title # </div> #} else if(data.AccountID != null && data.IsLeadAdapterField == true){# <div class = "custom"> #= data.Title # </div> #}#');

    selfSearch.SearchQualifiers = ko.observableArray([   //2 - number, 1 - string [FilterTypeID]
        { SearchQualifierTypeID: 1, FilterTypeID: 1, DropdownFilterTypeID: 1, SearchQualifier: 'Is' },
        { SearchQualifierTypeID: 2, FilterTypeID: 1, DropdownFilterTypeID: 1, SearchQualifier: 'Is Not' },
        { SearchQualifierTypeID: 1, FilterTypeID: 2, DropdownFilterTypeID: 0, SearchQualifier: 'Is Equal To' },
        { SearchQualifierTypeID: 2, FilterTypeID: 2, DropdownFilterTypeID: 0, SearchQualifier: 'Is Not Equal To' },
        { SearchQualifierTypeID: 3, FilterTypeID: 1, DropdownFilterTypeID: 1, SearchQualifier: 'Is Empty' },
        { SearchQualifierTypeID: 4, FilterTypeID: 1, DropdownFilterTypeID: 1, SearchQualifier: 'Is Not Empty' },
        { SearchQualifierTypeID: 5, FilterTypeID: 2, DropdownFilterTypeID: 0, SearchQualifier: 'Is Greater Than' },
        { SearchQualifierTypeID: 6, FilterTypeID: 2, DropdownFilterTypeID: 0, SearchQualifier: 'Is Greater Than Or Equal To' },
        { SearchQualifierTypeID: 7, FilterTypeID: 2, DropdownFilterTypeID: 0, SearchQualifier: 'Is Less Than' },
        { SearchQualifierTypeID: 8, FilterTypeID: 2, DropdownFilterTypeID: 0, SearchQualifier: 'Is Less Than Or Equal To' },
        { SearchQualifierTypeID: 9, FilterTypeID: 1, DropdownFilterTypeID: 0, SearchQualifier: 'Contains' },
        { SearchQualifierTypeID: 10, FilterTypeID: 1, DropdownFilterTypeID: 0, SearchQualifier: 'Does Not Contain' }
    ]);


    selfSearch.gridvisible = ko.observable(gridVisible == "True" ? 'true' : 'false');
    selfSearch.IsCustom = ko.observable(false);

   

    selfSearch.customScript = ko.observable(data.CustomPredicateScript);
    selfSearch.CustomPredicateScript = ko.observable(data.CustomPredicateScript);
   
    selfSearch.copySearch = function () {
        if (selfSearch.SearchDefinitionID() === 0) { return; }
        window.location.replace("/copysearch?SearchDefinitionID=" + selfSearch.SearchDefinitionID());
    };

    selfSearch.FilterLogic = ko.pureComputed(function () {
        var SearchPredicateType = ko.utils.arrayFirst(selfSearch.SearchPredicates(), function (choice) {
            return choice.SearchPredicateTypeID == selfSearch.SearchPredicateTypeID();
        });

        return SearchPredicateType.PredicateType;
    });

    function createCookie(name, value, days) {
        var expires;
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = "; expires=" + date.toGMTString();
        } else expires = "";
        document.cookie = escape(name) + "=" + escape(value) + expires + "; path=/";
    };

    function eraseCookie(name) {
        if (name != null && name != 'undefined')
            createCookie(name, "", -1);
    };

    eraseCookie("savedsearchid");
    eraseCookie("savedsearchname");
    createCookie("searchfilters", ko.toJSON(selfSearch), 1);
    selfSearch.SearchFilters = ko.observableArray(data.SearchFilters);
    //selfSearch.ActionSearchFilters = ko.observableArray(data.SearchFilters);
    //selfSearch.TourSearchFilters = ko.observableArray(data.SearchFilters);
    //selfSearch.WebVisitSearchFilters = ko.observableArray(data.SearchFilters);

    selfSearch.SearchFilters.subscribe(function () {
        selfSearch.CustomPredicateScript(getCustomPredicateScript());
        //createCookie("searchfilters", ko.toJSON(selfSearch), 1);
    });

    if (selfSearch.CustomPredicateScript() == null || selfSearch.CustomPredicateScript() == "")
        selfSearch.CustomPredicateScript(getCustomPredicateScript());

    selfSearch.SearchResults = ko.observableArray(data.SearchResult == null ? [] : data.SearchResult.Results);

    if (isRunSearch == "True") {
        var grid = $("#grid").data("kendoGrid");
        grid.dataSource.read({ page: 1, pageSize: pageSize, aviewModel: ko.toJSON(selfSearch) });
    }

    selfSearch.SearchColumns = ko.observableArray(selfSearch.SearchFields());
    var DefaultColIds = [];
    var columns = ko.utils.arrayFilter(selfSearch.SearchFields(), function (item) {
        //|| item.FieldId == PHONE_FIELDID || item.FieldId == LEAD_SCORE_FIELD
        if ((item.FieldId == FIRSTNAME_FIELDID && item.IsDropdownField == false && item.IsCustomField == false) ||
             (item.FieldId == PRIMARYEMAIL_FIELDID && item.IsDropdownField == false && item.IsCustomField == false) ||
             (item.FieldId == 3 && item.IsDropdownField == false && item.IsCustomField == false) ||
             (item.FieldId == LASTNAME_FIELDID && item.IsDropdownField == false && item.IsCustomField == false) ||
             (item.IsDropdownField == true)) //&& item.IsDropdownField == false
        {
            DefaultColIds.push(item.FieldId);
            return item.FieldId;
        }

    });

    selfSearch.DefaultCols = ko.observableArray(DefaultColIds);

    selfSearch.SelectedColumns = ko.observableArray(columns);

    selfSearch.IsDeleteVisible = ko.pureComputed(function () {
        if (selfSearch.SearchFilters().length > 1) {
            return true;
        } else {
            return false;
        }
    });

    selfSearch.InputTypeId = ko.observable(8);

    selfSearch.valueOptionsNeeded = ko.pureComputed({
        read: function () {
            var fields = ko.utils.arrayFilter(selfSearch.SearchFields(), function (item) {
                if (item.FieldInputTypeId == '1' || item.FieldInputTypeId == '6' || item.FieldInputTypeId == '11' || item.FieldInputTypeId == '12')
                    return item;
            });
            return fields;
        },
        write: function () { }
    });

    selfSearch.SelectedField = ko.observableArray([]);   // The field which is selected from search-fields, this will be updated by kendo dropdown change event
    var fieldIdSubscribe = function (fieldId, value) {
        // When searchFields contains duplicate fieldids, in this subscribe function we wont get the updated value as the FieldId is same. So we have to fetch those fieldtypes using kendoGrid methods. 
        if (selfSearch.SelectedField().length > 0) {
            value.IsCustomField(selfSearch.SelectedField()[0].IsCustomField);
            value.IsDropdownField(selfSearch.SelectedField()[0].IsDropdownField);
        }

        var contactDropdown = null;
        var IsAjaxCallNeeded = ko.utils.arrayFilter(selfSearch.valueOptionsNeeded(), function (item) {
            if (item.FieldId == fieldId && value.IsCustomField() == item.IsCustomField && value.IsDropdownField() == item.IsDropdownField)
                return item;
            else
                return false;
        });

        if (fieldId != null) {
            
            var field = ko.utils.arrayFirst(selfSearch.SearchFields(), function (item) {
                if (item.FieldId == fieldId && value.IsCustomField() == item.IsCustomField && value.IsDropdownField() == item.IsDropdownField)
                    return item;
            });
            //If a field is changed with qualifier 'contains' to another then searchQualifiers set is updatd, but we need to update SearchQualifierTypeID to change it from 'contains' to default 'IS'
            value.SearchQualifierTypeID("1");
            value.IsCustomField(field.IsCustomField);
            value.IsDropdownField(field.IsDropdownField);
            value.DropdownId(field.DropdownId);
            value.InputTypeId(field.FieldInputTypeId);

            if (field.FieldId == PARTNER_TYPE_FIELD)
                contactDropdown = PARTNER_TYPE_DROPDOWN_ID;
            else if (field.FieldId == LIFECYCLE_STAGE_FIELD)
                contactDropdown = LIFECYCLE_TYPE_DROPDOWN_ID;
            else if (field.FieldId == LEAD_SOURCE_FIELD || field.FieldId == FIRST_LEAD_SOURCE_FIELD)
                contactDropdown = LEAD_SOURCE_DROPDOWN_ID;
            else if (field.FieldId == TOURTYPE_FIELDID)
                contactDropdown = TOURTYPE_DROPDOWNID;
            else if (field.FieldId == COMMUNITY_FIELDID)
                contactDropdown = COMMUNITY_DROPDOWNID;
            else if (field.FieldId == ACTIONTYPE_FIELDID)
                contactDropdown = ACTIONTYPE_DROPDOWNID;
            else if (field.FieldId == NOTECATEGORY_FIELDID)
                contactDropdown = NOTECATEGORY_DROPDOWNID;
            else if (field.FieldId == LASTNOTECATEGORY_FIELDID)
                contactDropdown = NOTECATEGORY_DROPDOWNID;

            if (fieldId == DO_NOT_EMAIL_FIELDID) {
                value.InputTypeId(11);
                value.ValueOptions(doNotEmailValueOptions);
                IsAjaxCallNeeded = [];
            }
            else if (fieldId == LASTCONTACTED_THROUGH) {
                value.InputTypeId(11);
                value.ValueOptions(lastTouchedValueOptions);
                IsAjaxCallNeeded = [];
            }
            else if (fieldId == EMAIL_STATUS) {
                value.InputTypeId(11);
                value.ValueOptions(emailStatusValueOptions);
                IsAjaxCallNeeded = [];
            }
            if (IsAjaxCallNeeded.length > 0) {
                $.ajax({
                    url: url + 'GetSearchQualifiers',
                    type: 'get',
                    dataType: 'json',
                    data: { 'fieldId': field.FieldId, 'contactDropdown': contactDropdown },
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
                    value.ValueOptions(data.response);
                    
                }).fail(function (error) {
                    notifyError(error);
                });
            }
            if (field == null || field == 'undefined') {
                value.InputTypeId(8);
            }
            value.SearchText('');
        }
        var filteredData;        
        if (fieldId == LASTCONTACTED_THROUGH || value.InputTypeId() == 1) {    // values dropdown  InputTypeId 1 - Checkbox
            filteredData = ko.utils.arrayFilter(selfSearch.SearchQualifiers(), function (item) {
                return item.DropdownFilterTypeID == 1;
            });
            value.SearchQualifiers(filteredData);
        }
        else if (value.IsDropdownField() == true && value.InputTypeId() == 8) {    //  used for phone fields
            filteredData = ko.utils.arrayFilter(selfSearch.SearchQualifiers(), function (item) {
                return item.DropdownFilterTypeID == 1;
            });
            value.SearchQualifiers(filteredData);
        }
        else if (fieldId == LEADADAPTER) {
            filteredData = ko.utils.arrayFilter(selfSearch.SearchQualifiers(), function (item) {
                return item.DropdownFilterTypeID == 1 && item.FilterTypeID == 1 && (item.SearchQualifierTypeID != 3 && item.SearchQualifierTypeID != 4);
            });
            value.SearchQualifiers(filteredData);
        }
        else if (value.FieldId() == OWNER_FIELD || value.FieldId() == DO_NOT_EMAIL_FIELDID || value.FieldId() == COUNTRY_FIELD || value.FieldId() == STATE_FIELD
            || value.FieldId() == LIFECYCLE_STAGE_FIELD || value.FieldId() == PARTNER_TYPE_FIELD || value.FieldId() == LEAD_SOURCE_FIELD || value.FieldId() == FIRST_LEAD_SOURCE_FIELD  || value.InputTypeId() == 11) {
            filteredData = ko.utils.arrayFilter(selfSearch.SearchQualifiers(), function (item) {
                return item.DropdownFilterTypeID == 1;
            });
            value.SearchQualifiers(filteredData);
        }
        else if (value.FieldId() == NOTE_SUMMARY || value.FieldId() == LAST_NOTE) {
            filteredData = ko.utils.arrayFilter(selfSearch.SearchQualifiers(), function (item) {
                return item.SearchQualifierTypeID == 3 || item.SearchQualifierTypeID == 4;
            });
            value.SearchQualifiers(filteredData);
            //If a field is changed with qualifier 'contains' to another then searchQualifiers set is updatd, but we need to update SearchQualifierTypeID to change it from 'contains' to default 'IS'
            value.SearchQualifierTypeID("3");
        }
        else {
            filteredData = ko.utils.arrayFilter(selfSearch.SearchQualifiers(), function (item) {
                return item.FilterTypeID == 1;
            });
            value.SearchQualifiers(filteredData);
        }

        if (value.IsCustomField() && value.InputTypeId() == 1) {
            filteredData = ko.utils.arrayFilter(selfSearch.SearchQualifiers(), function (item) {
                return item.FilterTypeID == 1;
            });
            value.SearchQualifiers(filteredData);
        }

        if (value.InputTypeId() == 2 || value.InputTypeId() == 9 || value.InputTypeId() == 13) {
            filteredData = ko.utils.arrayFilter(selfSearch.SearchQualifiers(), function (item) {
                return ((item.SearchQualifierTypeID != 9 && item.SearchQualifierTypeID != 10 && !((item.SearchQualifierTypeID == 1 || item.SearchQualifierTypeID == 2) && item.FilterTypeID == 2)));
            });
            value.SearchQualifiers(filteredData);
        }
        else if (value.InputTypeId() == 5) {             // number's dropdown
            filteredData = [];
            if (fieldId != null && fieldId != LEAD_SCORE_FIELD)
                filteredData = ko.utils.arrayFilter(selfSearch.SearchQualifiers(), function (item) {
                    return item.FilterTypeID == 2 || item.SearchQualifierTypeID == 3 || item.SearchQualifierTypeID == 4;
                });
            else
                filteredData = ko.utils.arrayFilter(selfSearch.SearchQualifiers(), function (item) {
                    return item.FilterTypeID == 2;
                });
            value.SearchQualifiers(filteredData);
        }

        var notPresent = ko.utils.arrayFilter(selfSearch.SelectedColumns(), function (column) {
            return column.FieldId == fieldId;
        });
        if (notPresent.length < 1) {
            var insertColumn = ko.utils.arrayFirst(selfSearch.SearchFields(), function (field) { return field.FieldId == fieldId; });
            selfSearch.SelectedColumns.push(insertColumn);
            selfSearch.DefaultCols.push(parseInt(fieldId));
            //var multiselect = $("#multiselect").data("kendoMultiSelect");
            //multiselect.refresh();
        }
    };
    $.each(selfSearch.SearchFilters(), function (index, searchFilter) {
        var value = searchFilter;

        value.templ = kendo.template('#if (data.Code == 0 || (data.Id == 0 && data.Value != "No"))  { # <span>[|Select..|]</span># } ##if (data.IsDeleted == false )  { # <span>#= Value #</span># } # #if (data.IsDeleted == true )  { # <span style="color:red;">#= Value #</span># } #');

        value.FieldId = ko.observable(value.FieldId).extend({ notify: 'always' });
        value.ComputedFieldId = ko.observable(value.FieldId() + "" + value.IsDropdownField);
        value.SearchQualifierTypeID = ko.observable(value.SearchQualifierTypeID);
        value.SearchText = ko.observable(value.SearchText);
        if (value.InputTypeId == 2 || value.InputTypeId == 13) {
            value.SearchText(kendo.toString(new Date(selfSearch.SearchFilters()[index].SearchText())));
        }
        else if (value.InputTypeId == 9) {
            value.SearchText(kendo.toString(new Date(selfSearch.SearchFilters()[index].SearchText())), 't');
        }

        value.Enabled = ko.observable(value.SearchQualifierTypeID == 3 || value.SearchQualifierTypeID == 4 ? false : true);
        value.SearchQualifiers = ko.observable();
        value.ValueOptions = ko.observableArray(value.ValueOptions);
        value.InputTypeId = ko.observable(value.InputTypeId || 8);
        value.IsCustomField = ko.observable(value.IsCustomField);
        value.IsDropdownField = ko.observable(value.IsDropdownField);
        value.DropdownId = ko.observable(value.DropdownId);
        value.Minutes = ko.observable(0);
        value.Seconds = ko.observable(0);

        var minutes = 0;
        var seconds = 0;
        if (value.SearchText != null && value.FieldId() == 46) {
            minutes = Math.floor(value.SearchText() / 60, 2);
            seconds = value.SearchText() % 60;
            value.Minutes(minutes);
            value.Seconds(seconds);
        }
        value.searchFieldSelect = function (e) {

            var dataItem = this.dataItem(e.item.index());

            if (dataItem.IsCustomField) {
                value.IsCustomField(true);
            } else if (dataItem.IsDropdownField) {
                value.DropdownId(dataItem.DropdownId);
                value.IsDropdownField(true);
            } else {
                value.IsDropdownField(false);
                value.IsCustomField(false);
            }
        }

        value.SearchEmail = ko.observable();
        var filteredData;
        if (value.FieldId() == 26 || value.FieldId() == 46) {
             filteredData = ko.utils.arrayFilter(selfSearch.SearchQualifiers(), function (item) {
                 return item.FilterTypeID == 2 || item.SearchQualifierTypeID == 3 || item.SearchQualifierTypeID == 4;
            });
            value.SearchQualifiers(filteredData);
        }
        else if (value.IsDropdownField() == true && value.InputTypeId() == 8) {    //  used for phone fields
             filteredData = ko.utils.arrayFilter(selfSearch.SearchQualifiers(), function (item) {
                return item.DropdownFilterTypeID == 1;
            });
            value.SearchQualifiers(filteredData);
        }
        else if (value.FieldId() == LEADADAPTER) {
            filteredData = ko.utils.arrayFilter(selfSearch.SearchQualifiers(), function (item) {
                return item.DropdownFilterTypeID == 1 && item.FilterTypeID == 1 && (item.SearchQualifierTypeID != 3 && item.SearchQualifierTypeID != 4);
            });
            value.SearchQualifiers(filteredData);
        }
        else if (value.FieldId() == OWNER_FIELD || value.FieldId() == DO_NOT_EMAIL_FIELDID || value.FieldId() == COUNTRY_FIELD || value.FieldId() == STATE_FIELD
            || value.FieldId() == LIFECYCLE_STAGE_FIELD || value.FieldId() == PARTNER_TYPE_FIELD || value.FieldId() == LEAD_SOURCE_FIELD || value.FieldId() == FIRST_LEAD_SOURCE_FIELD || value.InputTypeId() == 11) {
            filteredData = ko.utils.arrayFilter(selfSearch.SearchQualifiers(), function (item) {
                return item.DropdownFilterTypeID == 1;
            });
            value.SearchQualifiers(filteredData);
        }
        else if (value.FieldId() == NOTE_SUMMARY || value.FieldId() == LAST_NOTE) {
            filteredData = ko.utils.arrayFilter(selfSearch.SearchQualifiers(), function (item) {
                return item.SearchQualifierTypeID == 3 || item.SearchQualifierTypeID == 4;
            });
            value.SearchQualifiers(filteredData);
            //If a field is changed with qualifier 'contains' to another then searchQualifiers set is updatd, but we need to update SearchQualifierTypeID to change it from 'contains' to default 'IS'
        }
        else {
            filteredData = ko.utils.arrayFilter(selfSearch.SearchQualifiers(), function (item) {
                return item.FilterTypeID == 1;
            });
            value.SearchQualifiers(filteredData);
        }
        if (value.InputTypeId() == 2 || value.InputTypeId() == 9 || value.InputTypeId() == 13) {        //date, time, datetime
            filteredData = ko.utils.arrayFilter(selfSearch.SearchQualifiers(), function (item) {
                return ((item.SearchQualifierTypeID != 9 && item.SearchQualifierTypeID != 10 && !((item.SearchQualifierTypeID == 1 || item.SearchQualifierTypeID == 2) && item.FilterTypeID == 2)));
            });
            value.SearchQualifiers(filteredData);
        }
        value.SearchQualifierTypeID.subscribe(function(val) {
            if (val == 3 || val == 4) {
                value.Enabled(false);
                value.SearchText("");
                //var dropdownId = "#" + index + "";
                //var dropdownlist = $(dropdownId).data("kendoDropDownList");
                //dropdownlist.enable(false);
            } else {
                value.Enabled(true);
            }
        });
        if (value.SearchQualifierTypeID() == 3 || value.SearchQualifierTypeID() == 4) {
            value.Enabled(false);
            value.SearchText("");
        }

        value.ComputedFieldId.subscribe(function (computedFieldID) {
            var fieldId = computedFieldID.replace('true', '').replace('false', '');
            value.FieldId(fieldId);       // For newely added filter by default FieldId = 1; if changed to another field we need to update for refelection in the search
            fieldIdSubscribe(fieldId, value);
        });

        //If searchText contains values seperated by '|' process the text to display the values in UI
        if (value.InputTypeId() == 1 || value.InputTypeId() == 12) {
            if (value.SearchText().indexOf('|') > -1) {
                var text = value.SearchText().split('|');
                value.SearchText(text);
            }
        }

        if (value.FieldId() == DO_NOT_EMAIL_FIELDID) {     //donotemail is checkbox(1) so we need to convert it to dropdown(11) 
            value.InputTypeId(11);
        }
    });

    var doNotEmailValueOptions = [{ Id: 1, Value: 'Yes', IsDeleted: false }, { Id: 0, Value: 'No', IsDeleted: false }];
    var lastTouchedValueOptions = [
        { Id: 4, Value: 'Campaign', IsDeleted: false },
        { Id: 26, Value: 'Send Text', IsDeleted: false },
        { Id: 25, Value: 'Send Mail', IsDeleted: false },
        { Id: 46, Value: 'Phone Call', IsDeleted: false },
        { Id: 47, Value: 'Email', IsDeleted: false },
        { Id: 48, Value: 'Appointment', IsDeleted: false },
        { Id: 3, Value: 'Action-Other', IsDeleted: false },
        { Id: 6, Value: 'Note', IsDeleted: false },
        { Id: 7, Value: 'Tour', IsDeleted: false }

    ];
    var emailStatusValueOptions = [{ Id: 50, Value: 'Not Verified', IsDeleted: false }, { Id: 51, Value: 'Verified', IsDeleted: false }, { Id: 52, Value: 'Soft Bounce', IsDeleted: false },
        { Id: 53, Value: 'Hard Bounce', IsDeleted: false }, { Id: 54, Value: 'Unsubscribed', IsDeleted: false }, { Id: 56, Value: 'Complained', IsDeleted: false },
        { Id: 57, Value: 'Suppressed', IsDeleted: false }];
    var actionStatusValueOptions = [{}, {}, {}];

    selfSearch.dropdownlist_change = function () {
        selfSearch.SelectedField([{ IsCustomField: this.dataItem().IsCustomField, IsDropdownField: this.dataItem().IsDropdownField }]);
    }

    selfSearch.GetDateFormat = function () {
        var dateFormat = readCookie("dateformat");
        return dateFormat;
    }

    selfSearch.FilterValidation = selfSearch.SearchFilters.extend({ minimumLength: 1 });

    //this is the code for multiselect binding with datatextfield and value field
    ko.bindingHandlers.multiselect = {
        init: function (element, valueAccessor) {
            var value = valueAccessor();
            ko.applyBindingsToNode(element, {
                kendoMultiSelect: {
                    value: value,
                    dataTextField: 'Title',
                    data: selfSearch.SearchFields(),
                    placeholder: '[|Select Columns|]'
                    //valuePrimitive: true
                    //itemTemplate: kendo.template($("#itemTemplate").html())
                }
            });

            //var src = $(element).data('kendoMultiSelect').dataSource.data();
            //var selected = $.grep(src, function (e, i) {
            //    return ko.utils.arrayFilter(value(), function (item) {
            //        return (item.FieldId == e.FieldId && e.IsCustomField == false); // && e.IsDropdownField == true
            //    }).length > 0;
            //});
            //if (selected.length <= 1)
            //    $('.k-delete').css('display', 'none');
            //else
            //    $('.k-delete').css('display', 'block');

            //value(selected);
        },
        update: function (element, valueAccessor) {
            var value = valueAccessor();
            var src = $(element).data('kendoMultiSelect').dataSource.data();
            var selected = $.grep(src, function (e) {
                return ko.utils.arrayFilter(value(), function (item) {
                    return (item.FieldId == e.FieldId); // && e.IsDropdownField == true && e.IsCustomField == false
                }).length > 0;
            });
            if (selected.length <= 1)
                $('.k-delete').css('display', 'none');
            else
                $('.k-delete').css('display', 'block');

            var multiselect = $(element).data('kendoMultiSelect');
            multiselect.dataSource.filter({});
            //value([]);
            value(selected);
        }
    }

    selfSearch.AddCustomFilter = function () {
        var newFilter = {
            FieldId: ko.observable(1).extend({ notify: 'always' }),
            SearchQualifierTypeID: ko.observable(1),
            SearchText: ko.observable(""),
            Enabled: ko.observable(true),
            ValueOptions: ko.observableArray(),
            InputTypeId: ko.observable(8),
            SearchEmail: ko.observable(),
            IsCustomField: ko.observable(false),
            IsDropdownField: ko.observable(false),
            DropdownId: ko.observable(1),
            ComputedFieldId: ko.observable(1 + "" + "false"),
            Minutes: ko.observable(00),
            Seconds: ko.observable(00),

            SearchQualifiers: ko.observable(ko.utils.arrayFilter(selfSearch.SearchQualifiers(), function (item) {
                return item.FilterTypeID == 1;
            })),
            searchFieldSelect: function (e) {
                var dataItem = this.dataItem(e.item);
                if (dataItem.IsCustomField) {
                    newFilter.IsCustomField(true);
                } else if (dataItem.IsDropdownField) {
                    newFilter.DropdownId(dataItem.DropdownId);
                    newFilter.IsDropdownField(true);
                } else {
                    newFilter.IsDropdownField(false);
                    newFilter.IsCustomField(false);
                }
            }
        };

        //if (newFilter.FieldId == 25 || newFilter.FieldId == 27)
        newFilter.templ = kendo.template('#if (data.Code == 0 ||( data.Id == 0 && data.Value != "No"))  { # <span>[|Select..|]</span># } ##if (data.IsDeleted == false )  { # <span>#= Value #</span># } # #if (data.IsDeleted == true )  { # <span style="color:red;">#= Value #</span># } #');
        //else
        //    newFilter.templ = kendo.template('<span >#= Value #</span>');

        newFilter.SearchQualifierTypeID.subscribe(function (val) {
            if (val == 3 || val == 4) {
                newFilter.Enabled(false);
                newFilter.SearchText("");
            } else {
                newFilter.Enabled(true);
            }
        });

        newFilter.ComputedFieldId.subscribe(function (computedFieldID) {
            var fieldId = computedFieldID.replace('true', '').replace('false', '');
            newFilter.FieldId(fieldId);       // For newely added filter by default FieldId = 1; if changed to another field we need to update for refelection in the search
            fieldIdSubscribe(fieldId, newFilter);
        });

        selfSearch.SearchFilters.push(newFilter);

        if (selfSearch.SearchPredicateTypeID() == 4)
            selfSearch.CustomPredicateScript(selfSearch.CustomPredicateScript() + " AND " + selfSearch.SearchFilters().length);
        return true;
    };

    selfSearch.DeleteCustomFilter = function (filter) {
        selfSearch.SearchFilters.remove(filter);
        var isDefault = ko.utils.arrayFilter(columns, function (item) {
            if (item.FieldId == filter.FieldId())
                return item;
        });
        if (isDefault.length < 1) {
            ko.utils.arrayRemoveItem(selfSearch.SelectedColumns(), ko.utils.arrayFilter(selfSearch.SelectedColumns(), function (field) { return field.FieldId == filter.FieldId() })[0]);
            selfSearch.SelectedColumns(selfSearch.SelectedColumns());
            selfSearch.DefaultCols.remove(parseInt(filter.FieldId()));
        }
    };

    selfSearch.TotalHits = ko.observable(data.SearchResult == null ? "" : data.SearchResult.TotalHits == 0 ? "[|No results found|]" : data.SearchResult.TotalHits +
        (data.SearchResult.TotalHits > 1 ? " [|Results|]" : " [|Result|]"));
    selfSearch.NoOfHits = ko.observable(data.SearchResult == null ? 0 : data.SearchResult.TotalHits);

    selfSearch.ShowMoreResults = ko.pureComputed(function () {
        var resultsDisplayed = selfSearch.SearchResults().length;
        if (resultsDisplayed < selfSearch.NoOfHits())
            return true;
        else
            return false;
    });

    selfSearch.PageNumber = ko.observable(1);
    selfSearch.TotalResultsCount = ko.observable(data.SearchResult == null ? "" : data.SearchResult.TotalHits);
    selfSearch.ResultsCount = ko.observable(data.SearchResult == null ? "" : data.SearchResult.TotalHits > selfSearch.PageSize() ? selfSearch.PageSize() : data.SearchResult.TotalHits);

    selfSearch.CSVFile = function () {
        if (selfSearch.SearchResults().length == 0) {
            notifyError("[|No contacts to export.|]");
            return;
        }

        pageLoader();


        var searchFields = selfSearch.SearchFields();
        selfSearch.SearchFields([]);

        var jsondata = ko.toJSON(selfSearch);

        $.ajax({
            url: url + 'ExportToCSVFile',
            type: 'post',
            data: jsondata,
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
            selfSearch.SearchFields(searchFields);
            removepageloader();
            window.location = '/AdvancedSearch/DownloadFile?fileKey=' + data.fileKey + '&fileName=' + data.fileName;
        }).fail(function (error) {
            selfSearch.SearchFields(searchFields);
            removepageloader();
            notifyError(error);
        })

        return true;
    };
    selfSearch.ExcelFile = function () {
        if (selfSearch.SearchResults().length == 0) {
            notifyError("[|No contacts to export.|]");
            return;
        }

        pageLoader();

        var searchFields = selfSearch.SearchFields();
        selfSearch.SearchFields([]);

        var jsondata = ko.toJSON(selfSearch);

        $.ajax({
            url: url + 'ExportToExcelFile',
            type: 'post',
            data: jsondata,
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
            selfSearch.SearchFields(searchFields);
            removepageloader();
            window.location = '/AdvancedSearch/DownloadFile?fileKey=' + data.fileKey + '&fileName=' + data.fileName;
        }).fail(function (error) {
            selfSearch.SearchFields(searchFields);
            removepageloader();
            notifyError(error);
        })
        return true;
    };
    selfSearch.PDFFile = function () {
        if (selfSearch.SearchResults().length == 0) {
            notifyError("[|No contacts to export.|]");
            return;
        }

        pageLoader();

        var searchFields = selfSearch.SearchFields();
        selfSearch.SearchFields([]);

        var jsondata = ko.toJSON(selfSearch);

        $.ajax({
            url: url + 'ExportToPDFFile',
            type: 'post',
            data: jsondata,
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
            selfSearch.SearchFields(searchFields);
            removepageloader();
            window.location = '/AdvancedSearch/DownloadFile?fileKey=' + data.fileKey + '&fileName=' + data.fileName;
        }).fail(function (error) {
            selfSearch.SearchFields(searchFields);
            removepageloader();
            notifyError(error);
        })
        return true;
    };

    selfSearch.Email = ko.observable();
    selfSearch.HomePhone = ko.observable();
    selfSearch.MobilePhone = ko.observable();
    selfSearch.WorkPhone = ko.observable();
    function processViewModel() {
        $.each(selfSearch.SearchFilters(), function (index, searchFilter) {
            if (searchFilter.InputTypeId() == 1 || searchFilter.InputTypeId() == 12) {  // 1 - Checkbox , 2 - Multiselect
                if (searchFilter.SearchText() != null && Array.isArray(searchFilter.SearchText())) {
                    var searchText = searchFilter.SearchText().join('|');
                    searchFilter.SearchText(searchText);
                }
            }
            if (searchFilter.FieldId() == 46) {
                searchFilter.SearchText((searchFilter.Minutes() * 60) + searchFilter.Seconds());
            }
        });
    };

    selfSearch.viewContacts = function () {
        pageLoader();
        processViewModel();

        selfSearch.ResultsCount(size);

        //selfSearch.PageSize(size);
        var jsondata = ko.toJSON(selfSearch);

        $.cookie.raw = true;
        $.cookie.json = true;
        $.cookie("advancedsvm", JSON.stringify(jsondata), { expires: 1 });

        var data = $.cookie();
        removepageloader();
    };

    //Date.prototype.toJSON = function () {
    //    return kendo.toString(this, "u");
    //};
    
    selfSearch.RunAdvancedSearch = function (data, pageNumber) {
        pageLoader();
        // Need to pass page number when clicked from runsearch and need to pass actual pagenumber when 'more results' clicked 
        var RunSearch = ko.toJSON(data);
        if (RunSearch === '"RunSearch"') {
            selfSearch.PageNumber(pageNumber);
        }

        processViewModel();
        var grid = $("#grid").data("kendoGrid");
        var pageSize = typeof ($("#grid").data("kendoGrid").dataSource.pageSize()) != "undefined" ? $("#grid").data("kendoGrid").dataSource.pageSize() : selfSearch.PageSize();
        //var pageSize = selfSearch.PageSize();
        grid.dataSource.read({ page: 1, pageSize: pageSize, aviewModel: ko.toJSON(selfSearch) });
        removepageloader();
        $('.btn-group.showtopinner').removeClass('showtopinner');
        processValueOptions();
        selfSearch.gridvisible("true");
        processValueOptions();
        setTimeout(function () {
            selfSearch.TotalHits(grid.dataSource._total == 0 ? "[|No records found|]" : grid.dataSource._total + " Contact(s)");
        }, 3000);
        eraseCookie("savedsearchid");
        eraseCookie("savedsearchname");
    };
  

    function processValueOptions() {
        $.each(selfSearch.SearchFilters(), function (index, searchFilter) {
            if (searchFilter.InputTypeId() == 1 || searchFilter.InputTypeId() == 12) {
                if (searchFilter.SearchText().indexOf('|') > -1) {
                    var text = searchFilter.SearchText().split('|');
                    searchFilter.SearchText(text);
                }
            }
        });
    };

    function processVales(value) {
        if (value.InputTypeId == 1 || value.InputTypeId == 12) {
            if (value.SearchText.indexOf('|') > -1) {
                var text = value.SearchText.split('|');
                value.SearchText(text);
            }
        }
    };

    selfSearch.MoreResults = function () {
        pageLoader();
        var pageNumber = selfSearch.PageNumber() + 1;
        selfSearch.PageNumber(pageNumber);
        selfSearch.RunAdvancedSearch();
    };

    selfSearch.GetUtcData = function (date) {
        return new Date(date).toUTCString();
    }

    selfSearch.GetUserDate = function (date) {
        if (date == null) {
            return "";
        }
        var dateFormat = readCookie("dateformat");
        if (dateFormat == null || dateFormat == 'undefined')
            return new Date(moment(date).toDate()).ToUtcUtzDate();
        else {
            var utzDate = new Date(moment(date).toDate()).ToUtcUtzDate();
            //return moment(utzDate).format(dateFormat + " hh:mm A");
            return kendo.toString(utzDate, dateFormat + " hh:mm tt");
        }
    }

    selfSearch.SomeField = ko.observable("");
    selfSearch.errors = ko.validation.group([selfSearch.SearchDefinitionName, selfSearch.FilterValidation]);

    selfSearch.saveAdvancedSearch = function () {
        selfSearch.errors.showAllMessages();
        if (selfSearch.errors().length > 0) {
            validationScroll();
            return;
        }
        var operation = "", type = "";
        if (selfSearch.SearchDefinitionID() > 0) {
            selfSearch.CreatedOn(new Date(moment(selfSearch.CreatedOn()).toDate()).toUtzUtcDate());
            if (selfSearch.SearchDefinitionID() > 0 && selfSearch.IsPreConfiguredSearch() == true) {

                $.each(selfSearch.SearchFilters(), function (index, filter) {

                    filter.SearchDefinitionID = 0;
                    filter.SearchFilterID = 0;

                });
                selfSearch.SearchDefinitionID(0);
                operation = "InsertSearch";
                type = "post";
            }
            else {
                operation = "UpdateSearch";
                type = "put";
            }
        }
        else {
            selfSearch.IsPreConfiguredSearch(0);
            selfSearch.IsFavoriteSearch(0);
            operation = "InsertSearch";
            type = "post";
        }
        processViewModel();
        var jsondata = ko.toJSON(selfSearch);
        pageLoader();

        var authToken = readCookie("accessToken");
        $.ajax({
            url: WEBSERVICE_URL + '/AdvancedSearch',
            type: type,
            data: jsondata,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + authToken);
            },
            success: function (data) {
                if (data.AdvancedSearchViewModel != null) {
                    createCookie("savedsearchid", data.AdvancedSearchViewModel.SearchDefinitionID, 1);
                    createCookie("savedsearchname", data.AdvancedSearchViewModel.SearchDefinitionName, 1);
                }

                notifySuccess('[|Successfully saved the search.|]');
                removepageloader();
                window.location.href = "../advancedsearch/search";
            },
            error: function (response) {
                removepageloader();
                notifyError(response.responseText);
            }
        });

    };

    selfSearch.getEmail = function (fullname, primaryemail, contactEmailID) {

        var name = "";
        //var email = "";
        var nameString = fullname + " " + "<" + primaryemail + ">" + "*";
        name = encodeURIComponent(nameString);
        //email = encodeURIComponent(primaryemail);
        return "contact/_SendMailModel?contactName=" + name + "&email=" + contactEmailID;
    };

    selfSearch.IsPhoneField = function (fieldId) {

        var phoneField = ko.utils.arrayFilter(selfSearch.SearchFields(), function (searchField) {
            return searchField.FieldId == fieldId && searchField.IsDropdownField;
        });

        if (phoneField.length > 0)
            return true;
        else
            return false;
    }

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

    selfSearch.GetPhoneNumber = function (fieldId, phones) {
        var number = "";
        if (phones == null)
            return number;
        //$.each(phones, function (index, phone) {
        //    if (fieldId == phone.PhoneType) {
        //        number = phone.Number;
        //        return;
        //    }
        //});
        var phonesList = ko.utils.arrayFilter(phones, function (phone) {
            return phone.PhoneType == fieldId;
        });
        var phoneNumbers = "";
        if (phonesList.length > 0)
            phoneNumbers = ko.utils.arrayMap(phonesList, function (phone) {
                if (phone.IsPrimary == true)
                    return formatPhone(phone.Number) + " *";
                else
                    return formatPhone(phone.Number);
            }).join(", ");
        number = phoneNumbers;
        return number;
    }

   

    selfSearch.IsCustomField = function (fieldId) {
        var customField = ko.utils.arrayFilter(selfSearch.SearchFields(), function (searchField) {
            return searchField.FieldId == fieldId && searchField.IsCustomField;
        });
        if (customField.length > 0)
            return true;
        else
            return false;
    }

    var getCustomFieldValueOption = function (valueOptionId) {
        var optionText = "";

        $.each(valueOptionId.toString().split('|'), function (outerIndex, optionId) {
            $.each(selfSearch.CustomFieldValueOptions(), function (index, valueOption) {
                if (valueOption.CustomFieldValueOptionId == optionId) {

                    optionText = outerIndex > 0 ? optionText + ", " + valueOption.Value : valueOption.Value;
                    return;
                }
            });
        });
        return optionText;
    }
    selfSearch.GetCustomFieldValue = function (fieldId, customFields, fieldInputTypeId) {
        var fieldValue = "";

        if (customFields == null)
            return fieldValue;

        if (customFields.length == 0)
            return fieldValue;

        $.each(customFields, function (index, customField) {
            if (fieldId == customField.CustomFieldId) {
                if (fieldInputTypeId == '1' || fieldInputTypeId == '6' || fieldInputTypeId == '11' || fieldInputTypeId == '12') {
                    fieldValue = getCustomFieldValueOption(customField.Value);
                    return;
                }
                else if (fieldInputTypeId == '13') {
                    var dateFormat = readCookie("dateformat").toUpperCase();
                    var utzDate = new Date(moment(Date.parse(customField.Value)).toDate());
                    fieldValue = moment(utzDate).format(dateFormat);
                    return;
                }
                else {
                    fieldValue = customField.Value;
                    return;
                }
            }
        });

        return fieldValue;
    }

  
}

var searchDefinitionViewModel = function (searchDefinitionName, searchDefinitionId) {
    var selfSearchDefinition = this;

    selfSearchDefinition.SearchDefinitionName = ko.observable(searchDefinitionName).extend({ required: { message: "[|Search Definition Name is required.|]" }, maxLength: 75 });
    selfSearchDefinition.SearchDefinitionID = ko.observable(searchDefinitionId);
    selfSearchDefinition.DisplayName = ko.observable();
    selfSearchDefinition.TotalContacts = ko.observable();
    //console.log(selfSearchDefinition);
}