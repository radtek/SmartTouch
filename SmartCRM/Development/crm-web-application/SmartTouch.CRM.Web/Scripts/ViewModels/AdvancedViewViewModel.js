var advancedViewViewModel = function (model, WEBSERVICE_URL, contactUrl) {
    selfmodel = this;
    ko.mapping.fromJS(model, {}, selfmodel);
    selfmodel.AccountID = ko.observable(model.AccountID);
    selfmodel.IsDynamicGrid = ko.observable(model.IsDynamicGrid || 'False');
    selfmodel.IsSavedSearch = ko.observable(model.IsSavedSearch);
    var showingFields = [{ Text: "People", Value: "2" }, { Text: "Companies", Value: "3" },
        { Text: "People and Companies", Value: "1" }, { Text: "My Contacts", Value: "4" }];

    selfmodel.ShowingFields = ko.observableArray(showingFields);
    localStorage.removeItem("ShowingTypeADV");
    selfmodel.ShowingType = ko.observable(model.ShowingType || 1);
    localStorage.setItem("ShowingTypeADV", selfmodel.ShowingType());
    selfmodel.ShowingType.subscribe(function (value) {
        localStorage.setItem("ShowingTypeADV", value);
        selfmodel.grid.destroy();
        selfmodel.BindGrid();
        selfmodel.SaveColumnPreferences(selfmodel.Fields());
    });
    selfmodel.grid = ko.observable();

    selfmodel.SearchDescription = ko.pureComputed({
        read: function () {
            if (model.SearchDescription != null && model.SearchDescription != 'undefined') {
                if (model.SearchDescription.charAt(0) == '"')
                    model.SearchDescription = model.SearchDescription.replace('"', '');
                if (model.SearchDescription.charAt(model.SearchDescription.length - 1) == '"')
                    model.SearchDescription = model.SearchDescription.replace(/"$/, '');
            } else {
                model.SearchDescription = "";
            }
            return model.SearchDescription;
        }
    });

    selfmodel.SearchName = ko.pureComputed({
        read: function () {
            if (model.SearchName != null && model.SearchName != 'undefined') {
                if (model.SearchName.charAt(0) == '"')
                    model.SearchName = model.SearchName.replace('"', '');
                if (model.SearchName.charAt(model.SearchName.length - 1) == '"')
                    model.SearchName = model.SearchName.replace(/"$/, '');
            } else {
                model.SearchName = "";
            }
            return model.SearchName;
        }
    });
    selfmodel.SearchText = ko.observable("");
    selfmodel.SearchFields = ko.observableArray(model.SearchFields);
    selfmodel.ItemsPerPage = ko.observable(model.ItemsPerPage);
    selfmodel.PageNumber = ko.observable(model.PageNumber || 1);
    selfmodel.SortField = ko.observable();
    selfmodel.SortDirection = ko.observable();
    selfmodel.IsAccountAdmin = ko.observable(model.IsAccountAdmin);
    selfmodel.GridTotal = ko.observable();
    selfmodel.EntityType = ko.observable(model.EntityType);
    selfmodel.GridBindCount = ko.observable(0);
    if (model.SelectedFields != null && model.SelectedFields.length > 0) {
        if (model.SelectedFields.indexOf(1) < 0) {
            model.SelectedFields.push(1);
        }
        if (model.SelectedFields.indexOf(2) < 0) {
            model.SelectedFields.push(2);
        }
        if (model.SelectedFields.indexOf(3) < 0) {
            model.SelectedFields.push(3);
        }
        if (model.SelectedFields.indexOf(7) < 0) {
            model.SelectedFields.push(7);
        }
    }

    selfmodel.SelectedFields = (model.SelectedFields != null && model.SelectedFields.length > 0) ? ko.observableArray(model.SelectedFields) : ko.observableArray(["1", "2", "3", "7"]);
    selfmodel.Fields = ko.computed({
        read: function () { return selfmodel.SelectedFields() },
        write: function (n) {
            $.each(n, function (i, val) {
                if (selfmodel.SelectedFields().indexOf(val) > -1) { }
                else {
                    selfmodel.SelectedFields().push(n[0]);
                }
            });
            $.each(selfmodel.SelectedFields(), function (i, val) {
                if (n.indexOf(val) > -1) { }
                else {
                    var index = selfmodel.SelectedFields().indexOf(n[0]);
                    if (index > -1) {
                        selfmodel.SelectedFields().splice(index, 1);
                    }
                }
            });
            
            var SelectedFieldsStringArray = n.map(String);  //maps every element in n to String 
            selfmodel.SelectedFields(SelectedFieldsStringArray);

            //var table = $('#resultsGrid').DataTable();
            //table.destroy();
            //table.clear();
            //$('#resultsGrid').remove();
            //selfmodel.grid = new Object();
            //$('#resultsGrid').empty();
            //setTimeout(function () {
            //    selfmodel.BindGrid();
            //}, 1000);
            //selfmodel.grid.clear();
            selfmodel.grid.rows().remove();
            selfmodel.grid.destroy();
            $('#resultsGrid').empty();
            selfmodel.BindGrid();
            selfmodel.SaveColumnPreferences(n);
            
        },
        owner: this
    });

    selfmodel.ValueOptions = ko.observableArray(model.CustomFieldValueOptions);
    selfmodel.GetValueOptions = function () {
        var authToken = readCookie("accessToken");
        return $.ajax({
            url: WEBSERVICE_URL + '/customfieldsvalueoptions?accountId=' + selfmodel.AccountID(),
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + authToken);
            },
            success: function (response) {
                selfmodel.ValueOptions(response);
            },
            error: function (response) {
                notifyError(response.responseText);
            }
        });
    };

    selfmodel.SaveColumnPreferences = function (n) {
        var entityId = 0;
        var type = 0;
        var fields = n;
        var showingType = 1;
        
        entityId = selfmodel.EntityId();
        type = selfmodel.EntityType();

        if (entityId != undefined && entityId != null && entityId != 0 && n != null && n.length > 0) {
            showingType = selfmodel.ShowingType();
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
    selfmodel.BindColumns = function () { };

    selfmodel.companyName = function (name, contactId, companyId, companyName, fullName, type) {
        if (type == 1 && companyId != null)
            return "<a title=" + companyName +" href= /company/" + companyId + ">" + companyName + "</a>";
        else if (type == 2 && contactId != null)
            return "<a title=" + companyName + " href= /company/" + contactId + ">" + name + "</a>";
        else
            return companyName;
    };
    selfmodel.firstName = function (name, contactId) {
        if (name != null)
            return "<a title="+ name + " href= /person/" + contactId + ">" + name + "</a>";
        else
            return "";
    };
    selfmodel.lastName = function (name, contactId) {
        if (name != null)
            return "<a title=" + name + " href= /person/" + contactId + ">" + name + "</a>";
        else
            return "";
    };
    selfmodel.emailFormat = function (name, emailID, email, doNotEmail, emailStatus) {
        var isSendEmail = model.HasEmailPermission;
        if (email == 'Email Not Available')
            return "Email Not Available";
        else {
            if (isSendEmail == 'False' || doNotEmail == 'true' || emailStatus == 53 || emailStatus == 54 || emailStatus == 57)
                return "<label style='margin-top:0px; margin-bottom:0px' title="+ email +">" + email + "</label>"
            else
                return "<a title=" + email +" data-toggle='modal' data-target='#modal' href='/contact/_SendMailModel?contactName=" + selfmodel.getEmailData(name, email) + "&email=" + emailID + "'>" + email + "</a>";
        }
    };
    selfmodel.getEmailData = function (contactname, PrimaryEmialID) {
        var Text = contactname + " " + "<" + PrimaryEmialID + ">" + " " + "*";
        var name = encodeURIComponent(Text);
        return name;
    }
    selfmodel.doNotEmail = function (data) {
        return data == true ? "Yes" : "No";
    };
    selfmodel.formatDate = function (date, inputTypeId) {
        if (date) {
            if (date.indexOf("/Date(") > -1)
                date = new Date(parseInt(date.substr(6)));
            if (isNaN(Date.parse(date))) {
                return "";
            }
            else {
                var dateFormat = readCookie("dateformat");
                if (dateFormat == null || dateFormat == 'undefined')
                    return new Date(Date.parse(date)).ToUtcUtzDate().toDateString();
                else {
                    var utzDate = new Date(moment(date).toDate()).ToUtcUtzDate();
                    return kendo.toString(utzDate, dateFormat + (inputTypeId == 13 ? "" : " hh:mm tt"));
                }
            }
        }
        else
            return "";
    }
    selfmodel.formatUrl = function (data) {
        if (data != null)
            return "<a title="+ data +" href=" + data + ">" + data + "</a>";
        else
            return "";
    };
    selfmodel.formatDateList = function (date) {
        var dateString = "";
        if (date != null && date != 'undefined' && date.toString().indexOf(',') > -1) {
            var dateArray = date.toString().split(',');
            $.each(dateArray, function (i, v) {

                dateString += selfmodel.formatDate(v) + ", ";
            });
            return dateString;
        }
        else
            return selfmodel.formatDate(date);
    };
    selfmodel.address = function (primaryAddress, val) {
        if (primaryAddress != null) {
            if (val == 1)
                return (primaryAddress.AddressLine1 != null) ? "<label title='" + primaryAddress.AddressLine1 + "'> " + primaryAddress.AddressLine1 + "</label>": "";
            else if (val == 2)
                return (primaryAddress.AddressLine2 != null) ? "<label title='" + primaryAddress.AddressLine2 + "'> " + primaryAddress.AddressLine2 + "</label>": "";
            else if (val == 3)
                return (primaryAddress.City != null) ? "<label title='" + primaryAddress.City + "'> " + primaryAddress.City + "</label>" : "";
            else if (val == 4)
                if (primaryAddress.State != null)
                    return primaryAddress.State.Name ? "<label title='" + primaryAddress.State.Name + "'> " + primaryAddress.State.Name + "</label>" : "";
                else
                    return "";
            else if (val == 6)
                if (primaryAddress.Country != null)
                    return primaryAddress.Country.Name ? "<label title='" + primaryAddress.Country.Name + "'> " + primaryAddress.Country.Name + "</label>" : "";
                else
                    return "";
            else if (val == 5)
                return (primaryAddress.ZipCode != null) ? "<label title='" + primaryAddress.ZipCode + "'> " + primaryAddress.ZipCode + "</label>" : "";
        }
        else
            return "";
    };
    selfmodel.emailStatuses = function (status) {
        if (status == "50")
            return "Not Verified";
        else if (status == "51")
            return "Verified";
        else if (status == "52")
            return "Soft Bounce";
        else if (status == "53")
            return "Hard Bounce";
        else if (status == "54")
            return "Unsubscribed";
        else if (status == "55")
            return "Subscribed";
        else if (status == "56")
            return "Complained";
        else if (status == "57")
            return "Suppressed";
        else
            return "";
    }
    selfmodel.GetLeadAdapterTypeName = function (typeId) {
        var leadAdapterData = model.LeadAdapterTypes;
        var LeadAdapterName = "";
        if (typeId) {
            if (typeId > 0) {
                $.each(leadAdapterData, function (ind, val) {
                    if (val.LeadAdapterTypeID == 11)
                        val.Name = "";

                    if (val.LeadAdapterTypeID == typeId)
                        LeadAdapterName = val.Name;
                });
                return LeadAdapterName
            }
            else
                return LeadAdapterName;
        }
        else
            return LeadAdapterName;

    };
    selfmodel.DisplayLastNote = function (lastNote) {
        if (lastNote)
            return "<label title='"+ lastNote + "'>"+ lastNote +"<label>";
        else
            return "";
    };
    selfmodel.getPhoneNumber = function (fieldId, phones) {
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
                    return (phone.CountryCode != null ? "+" + phone.CountryCode + " " : "") + selmodel.formatPhone(phone.Number) + (phone.Extension != null ? " Ext. " + phone.Extension + " " : "") + " *";
                else
                    return (phone.CountryCode != null ? "+" + phone.CountryCode + " " : "") + selmodel.formatPhone(phone.Number) + (phone.Extension != null ? " Ext. " + phone.Extension + " " : "");
            }).join(", ");
        number = phoneNumbers;
        return number;
    };
    selfmodel.formatPhone = function (phonenum) {
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
    selfmodel.getTime = function (value) {
        var time = value;
        var date = new Date(value).ToUtcUtzDate();
        if (date != 'Invalid Date') {
            time = kendo.toString(new Date(moment(time).toDate()).ToUtcUtzDate(), "T")
        }
        return time;
    };
    selfmodel.getCustomFieldValueOption = function (valueOptionId) {
        var optionText = "";
        var optionIds = valueOptionId.toString().split('|');
        if (selfmodel.ValueOptions().length > 0) {
            $.each(optionIds, function (outerIndex, optionId) {
                var option = ko.utils.arrayFirst(selfmodel.ValueOptions(), function (opt, i) {
                    return opt.CustomFieldValueOptionId.toString() == optionId;
                });
                if (option != null)
                    optionText = outerIndex > 0 ? optionText + ", " + option.Value : option.Value;
            });
            return optionText;
        }
        else
            selfmodel.GetValueOptions().done(function () {
                $.each(optionIds, function (outerIndex, optionId) {
                    var option = ko.utils.arrayFirst(selfmodel.ValueOptions(), function (opt, i) {
                        return opt.CustomFieldValueOptionId.toString() == optionId;
                    });
                    if (option != null)
                        optionText = outerIndex > 0 ? optionText + ", " + option.Value : option.Value;
                });
                return optionText;
            });
    };
    selfmodel.GetCustomFieldValue = function (customFields, phones, fieldId, inputTypeId, isPhoneField) {
        var customFields = customFields;
        var valueOptions = [];
        var fieldValue = "";
        if (customFields != null && customFields.length > 0) {
            var matchedField = ko.utils.arrayFirst(customFields, function (field, ai) {
                return field.CustomFieldId == fieldId;
            });
            if (matchedField != null) {
                if (inputTypeId == 1 || inputTypeId == 6 || inputTypeId == 11 || inputTypeId == 12)
                    fieldValue = selfmodel.getCustomFieldValueOption(matchedField.Value);
                else if (inputTypeId == 9)
                    fieldValue = selfmodel.getTime(matchedField.Value_Date);
                else if (inputTypeId == 2 || inputTypeId == 13)
                    fieldValue = selfmodel.formatDate(matchedField.Value, inputTypeId);
                else
                    fieldValue = matchedField.Value;
            }
        }
        if (isPhoneField == "True") {
            fieldValue = selfmodel.getPhoneNumber(fieldId, phones);
        }
        return  "<label title='" + fieldValue +"'>" + fieldValue +"</label>";
    };
    
    selfmodel.AddCheckBoxTemplate = function (companyname, email, contactType, phone, emailStatus, doNotEmail, isDelete, contactId, phoneNumberID, emailID, name) {
        return "<label class='checkbox'>" +
                        "<input type='checkbox' class='chkcontacts' id='" + contactId + "' data-name= '" + name + "' " + "data-company='" + companyname + "' " + "data-Email='" + email + "' " +
                        "data-email-id='" + emailID + "' " + "data-phone-id='" + phoneNumberID + "' " + "data-contacttype='" + contactType + "' " + "data-phone='" + phone + "' " + "data-EmailStatus='" +
                        emailStatus + "' " + "data-DonotEmail='" + doNotEmail + "' " + "data-IsDelete='" + isDelete + "' " + "data-IsAccountAdmin='" + selfmodel.IsAccountAdmin() + "' " + "data-toggle='checkbox'/>" +
                        "</label>";
    }

    selfmodel.getColumns = function () {
        var columns = [];
        columns.push({ data: "checkbox", title: "", FieldId: 0, className: "", orderable: false });
        columns.push({ data: "FirstName", title: "First Name", FieldId: 1, className: "advancedgrid-tablehead", orderable: true, orderSequence: ["asc", "desc", ""] });
        columns.push({ data: "LastName", title: "Last Name", FieldId: 2, className: "advancedgrid-tablehead", orderable: true, orderSequence: ["asc", "desc", ""] });
        columns.push({ data: "CompanyName", title: "Company Name", FieldId: 3, className: "advancedgrid-tablehead", orderable: true, orderSequence: ["asc", "desc", ""] });
        columns.push({ data: "PrimaryEmail", title: "Primary Email", FieldId: 7, className: "advancedgrid-tablehead", orderable: true, orderSequence: ["asc", "desc", ""] });
        columns.push({ data: "Title", title: "Title", FieldId: 8, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "LifecycleName", title: "Life Cycle", FieldId: 22, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "DoNotEmail", title: "Do not email", FieldId: 23, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "LastTouched", title: "Last Contacted", FieldId: 29, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "PartnerTypeName", title: "Partner Type", FieldId: 21, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "LastTouchedThrough", title: "Last Touched Method", FieldId: 41, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "LeadScore", title: "Lead Score", FieldId: 26, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "FacebookUrl", title: "Facebook Url", FieldId: 9, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "LinkedInUrl", title: "Linkedln Url", FieldId: 11, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "GooglePlusUrl", title: "GooglePlus Url", FieldId: 12, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "WebsiteUrl", title: "Website Url", FieldId: 13, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "BlogUrl", title: "Blog Url", FieldId: 14, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "TwitterUrl", title: "Twitter Url", FieldId: 10, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "LeadSources", title: "Lead Source", FieldId: 24, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "LeadSourceDate", title: "Lead Source Date", FieldId: 50, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "FirstLeadSource", title: "First Lead Source", FieldId: 51, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "FirstLeadSourceDate", title: "First Lead Source Date", FieldId: 52, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "SourceType", title: "First Source Type", FieldId: 44, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "OwnerName", title: "Owner Name", FieldId: 25, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "CreatedByUser", title: "Created By", FieldId: 27, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "CreatedOn", title: "Created On", FieldId: 28, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "AddressLine1", title: "Address Line 1", FieldId: 15, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "AddressLine2", title: "Address Line 2", FieldId: 16, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "City", title: "City", FieldId: 17, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "State", title: "State", FieldId: 18, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "Country", title: "Country", FieldId: 20, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "Zipcode", title: "Zipcode", FieldId: 19, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "ContactSummary", title: "Contact Summary", FieldId: 54, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "LastNoteDate", title: "Last Note Date", FieldId: 55, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "EmailStatus", title: "Email Status", FieldId: 59, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "LeadAdapter", title: "Lead Adapter", FieldId: 61, className: "advancedgrid-tablehead" , orderable: false });
        columns.push({ data: "LastNote", title: "Last Note", FieldId: 69, className: "advancedgrid-tablehead" , orderable: false });
        var customFields = [];
        customFields = selfmodel.SearchFields().filter(function (f) {
            return f.FieldId > 200;
        });
        if (customFields) {
            $.each(customFields, function (i, v) {
                columns.push({ data: v.FieldId.toString(), title: v.Title, FieldId: v.FieldId, className: "advancedgrid-tablehead", orderable: false });
            });
        }
        return columns;
    };

    selfmodel.CustomFields = function () {
        var result = [];
        var customs = [];
        customs = selfmodel.SearchFields().filter(function (f) {
            return f.FieldId > 200;
        });
        if (customs) {
            $.each(customs, function (i, v) {
                result.push({
                    FieldId: v.FieldId, Value: selfmodel.GetCustomFieldValue(customs, v.Phones, v.FieldId, v.FieldInputTypeId, v.IsDropdownField)
                })
            })
        }
        return result;
    }

    selfmodel.getSelectedColumns = function () {
        var columns = [];
        var columnsSet = selfmodel.getColumns();
        columns.push(columnsSet[0]);
        if (selfmodel.SelectedFields().length > 0) {
            $.each(selfmodel.SelectedFields(), function (i, val) {
                var col = ko.utils.arrayFirst(columnsSet, function (v, index) {
                    return v.FieldId == val;
                });
                if (col)
                    columns.push(col);
            });
        }
        else {
            var firstName = ko.utils.arrayFirst(columnsSet, function (v, index) {
                return v.FieldId == 1;
            });
            columns.push(firstName);
            var lastName = ko.utils.arrayFirst(columnsSet, function (v, index) {
                return v.FieldId == 2;
            });
            columns.push(lastName);
            var companyName = ko.utils.arrayFirst(columnsSet, function (v, index) {
                return v.FieldId == 3;
            });
            columns.push(companyName);
            var email = ko.utils.arrayFirst(columnsSet, function (v, index) {
                return v.FieldId == 7;
            });
            columns.push(email);
        }
        columns.sort(function (a, b) {
            return a.FieldId - b.FieldId;
        });
        return columns;
    };

    selfmodel.formatDataCustomFields = function (filteredData) {
        var jsonData = filteredData.Data;
        var selectedColumns = selfmodel.getSelectedColumns();
        var customFields = [];
        customFields = selfmodel.SearchFields().filter(function (f) {
            return f.FieldId > 200;
        });
        for (i = 0; i < jsonData.length; i++) {
            var obj = jsonData[i];
            obj["checkbox"] = selfmodel.AddCheckBoxTemplate(obj.CompanyName, obj.PrimaryEmail, obj.ContactType, obj.Phone, obj.PrimaryEmailStatus,
                obj.DoNotEmail, obj.IsDelete, obj.ContactID, obj.PrimaryContactPhoneNumberID, obj.PrimaryContactEmailID, obj.Name);
            for (var j = 0; j < selectedColumns.length; j++) {
                var field = selectedColumns[j];
                if (selfmodel.BindProperty(field.FieldId)) {
                    if (field.FieldId == 1)
                        obj[field.data] = selfmodel.firstName(obj.FirstName, obj.ContactID);
                    else if (field.FieldId == 2)
                        obj[field.data] = selfmodel.lastName(obj.LastName, obj.ContactID);
                    else if (field.FieldId == 3)
                        obj[field.data] = selfmodel.companyName(obj.CompanyName, obj.ContactID, obj.CompanyID, obj.CompanyName, obj.Name, obj.ContactType);
                    else if (field.FieldId == 7)
                        obj[field.data] = selfmodel.emailFormat(obj.Name, obj.PrimaryContactEmailID, obj.PrimaryEmail, obj.DoNotEmail, obj.PrimaryEmailStatus);

                    else if (field.FieldId == 9)
                        obj[field.data] = selfmodel.formatUrl(obj.FacebookUrl);
                    else if (field.FieldId == 10)
                        obj[field.data] = selfmodel.formatUrl(obj.TwitterUrl);
                    else if (field.FieldId == 11)
                        obj[field.data] = selfmodel.formatUrl(obj.LinkedInUrl);
                    else if (field.FieldId == 12)
                        obj[field.data] = selfmodel.formatUrl(obj.GooglePlusUrl);
                    else if (field.FieldId == 13)
                        obj[field.data] = selfmodel.formatUrl(obj.WebsiteUrl);
                    else if (field.FieldId == 14)
                        obj[field.data] = selfmodel.formatUrl(obj.BlogUrl);

                    else if (field.FieldId == 15)
                        obj[field.data] = selfmodel.address(obj.PrimaryAddress, 1);
                    else if (field.FieldId == 16)
                        obj[field.data] = selfmodel.address(obj.PrimaryAddress, 2);
                    else if (field.FieldId == 17)
                        obj[field.data] = selfmodel.address(obj.PrimaryAddress, 3);
                    else if (field.FieldId == 18)
                        obj[field.data] = selfmodel.address(obj.PrimaryAddress, 4);
                    else if (field.FieldId == 19)
                        obj[field.data] = selfmodel.address(obj.PrimaryAddress, 5);
                    else if (field.FieldId == 20)
                        obj[field.data] = selfmodel.address(obj.PrimaryAddress, 6);
                    else if (field.FieldId == 23)
                        obj[field.data] = selfmodel.doNotEmail(obj.DoNotEmail);

                    else if (field.FieldId == 28) 
                        obj.CreatedOn = selfmodel.formatDate(obj.CreatedOn, 13);
                    else if (field.FieldId == 29) 
                        obj[field.data] = selfmodel.formatDate(obj.LastTouched, 13);
                    else if (field.FieldId == 50)
                        obj.LeadSourceDate = obj.LeadSourceDate;
                    else if (field.FieldId == 52)
                        obj.FirstLeadSourceDate = selfmodel.formatDate(obj.FirstLeadSourceDate, 13);

                    else if (field.FieldId == 54)
                        obj.ContactSummary = "<label title='" + obj.NoteSummary + "'>" + obj.NoteSummary + "</label>";
                    else if (field.FieldId == 55)
                        obj.LastNoteDate = selfmodel.formatDate(obj.LastNoteDate, 13);
                    else if (field.FieldId == 59)
                        obj[field.data] = selfmodel.emailStatuses(obj.PrimaryEmailStatus);
                    else if (field.FieldId == 61)
                        obj[field.data] = selfmodel.GetLeadAdapterTypeName(obj.FirstSourceType);
                    else if (field.FieldId == 69)
                        obj[field.data] = selfmodel.DisplayLastNote(obj.LastNote);
                    else if (field.FieldId > 200) {
                        var col = ko.utils.arrayFirst(customFields, function (v, index) {
                            return v.FieldId == field.FieldId;
                        });
                        obj[field.data] = selfmodel.GetCustomFieldValue(obj.CustomFields, obj.Phones, field.FieldId, col.FieldInputTypeId, col.IsDropdownField);
                    }
                }
            }
        }
        return jsonData;
    }

    selfmodel.BindProperty = function (fieldId) {
        var bindableProps = [1, 2, 3, 7, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 23, 28, 29, 50, 52, 54, 55, 59, 61, 69];
        if (bindableProps.indexOf(fieldId) != -1 || fieldId > 200)
            return true;
        return false;
    }

    selfmodel.BindGrid = function () {

        pageLoader();
        if ($.fn.DataTable.isDataTable('#resultsGrid')) {
            var table = $('#resultsGrid').DataTable();
            var info = table.page.info();
            selfmodel.PageNumber(info.page + 1);
            selfmodel.ItemsPerPage(info.length);
        }
        selfmodel.grid = $('#resultsGrid').DataTable({
            "destroy": true,
            "serverSide": true,
            "processing": true,
            "sDom": "ltipr",
            "paging": true,
            "pagingType": "full_numbers",
            "lengthChange": "true",
            "pageLength": selfmodel.ItemsPerPage(),
            "order": [],
            "language": {
                "zeroRecords": "<div class='notecordsfound'><div><i class='icon st-icon-browser-windows-2'></i></div><span class='bolder smaller-90'>No records found</span></div>",
                "info": "[|Showing|] _START_ - _END_ [|from|] _MAX_ [|Contact(s)|]",
                "infoEmpty": "[|No Contacts to display|]",
                "sInfoFiltered": "",
                "sLengthMenu": "[|Contacts per page _MENU_|]",
                "sProcessing": '<div class="pageloader-mask"><span class="pageloader-text">Loading...</span><div class="pageloader-image"></div><div class="pageloader-color"></div></div>',
                "paginate": {
                    "previous": "<span class='k-icon k-i-arrow-w'>arrow-w</span>" ,
                    "next": "<span class='k-icon k-i-arrow-e'>arrow-e</span>",
                    "first": "<span><span class='k-icon k-i-seek-w'></span></span>",
                    "last": "<span class='k-icon k-i-seek-e'>seek-e</span>"
                }
                //"lengthMenu": '<select class="k-widget k-dropdown k-header">' +
                //              '<option value="10">10</option>' +
                //              '<option value="25">25</option>' +
                //              '<option value="50">50</option>' +
                //              '<option value="100">100</option>' +
                //              '<option value="250">250</option>' +
                //              '</select>'
            },
            "ajax": {
                "url": '/Contact/ContactsAdvancedView1', 
                "contentType": "application/json",
                "type": "POST",
                "data": function () {
                    if (selfmodel.GridBindCount() > 0) {
                        var order = selfmodel.grid.order();
                        if (order.length > 0) {
                            var columnName = order[0][0] == "1" ? "FirstName" : order[0][0] == "2" ? "LastName" : order[0][0] == "3" ? "CompanyName" : order[0][0] == "4" ? "PrimaryEmail" : "";
                            var direction = order[0][1] == "asc" ? 0 : 1;
                            if (order[0][1] == "")
                                columnName = "";
                            selfmodel.SortField(columnName);
                            selfmodel.SortDirection(direction);
                        }
                    }
                    return JSON.stringify({
                        'advancedSearchModel': ko.toJSON({
                            ShowingType: selfmodel.ShowingType(), ItemsPerPage: selfmodel.ItemsPerPage(), SortField: selfmodel.SortField(), SortDirection: selfmodel.SortDirection(),
                            PageNumber: selfmodel.PageNumber(), SearchText: selfmodel.SearchText(), EntityType: selfmodel.EntityType(), TagID: selfmodel.TagID(), ActionID: selfmodel.ActionID(), OpportunityID: selfmodel.OpportunityID(),
                            Guid: selfmodel.Guid()
                        })
                    })
                },
                "dataFilter": function (data) {
                    var json = jQuery.parseJSON(data);
                    json.recordsTotal = json.Total;
                    json.recordsFiltered = json.Total;
                    json.data = selfmodel.formatDataCustomFields(json);
                    removepageloader();
                    var count = selfmodel.GridBindCount();
                    selfmodel.GridBindCount(count + 1);
                    return JSON.stringify(json);
                }
            },
            "columns": selfmodel.getSelectedColumns(),
            "lengthMenu": [10, 25, 50, 100, 250]
        });
        selfmodel.grid.on('draw.dt', function () {
            setTimeout(function () {
                appendCheckbox();
            }, 10);
        });
        selfmodel.grid.on('page.dt', function () {
            var table = $('#resultsGrid').DataTable();
            var info = table.page.info();
            console.log(table.order());
            selfmodel.PageNumber(info.page + 1);
        });
        selfmodel.grid.on('length.dt', function (e, settings, len) {
            selfmodel.PageNumber(1);
            selfmodel.ItemsPerPage(len);
        });
        selfmodel.grid.on('processing.dt', function (e, settings, processing) {
            if (processing) 
                pageLoader();
            else 
                removepageloader();
        });

        $('#masterCheckBox').attr('checked', false);
        $('#masterCheckBox').parent('label.checkbox').removeClass('checked');

        $('#masterCheckBox_all').attr('checked', false);
        $('#masterCheckBox_all').parent('label.checkbox').removeClass('checked');
    };
};