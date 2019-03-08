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
    
    selfContactsResults.SearchFields = ko.observableArray(JSON.parse(searchFields));

    selfContactsResults.selectedFields = ko.observableArray(["1", "2", "3", "7"]);

    selfContactsResults.Fields = ko.computed({
        read: function () { return selfContactsResults.selectedFields() },
        write: function (n) {
            var myGrid = $('#grid').data('kendoGrid');
            $.each(n, function (i,val) {
                if (selfContactsResults.selectedFields().indexOf(val) > -1) { }     //Field already exist in selectedFields
                else {
                    myGrid.thead.find('th[data-fieldid="' + val + '"]').toggle();
                    myGrid.tbody.find('td[data-fieldid="' + val + '"]').toggle();
                }
            });
            $.each(selfContactsResults.selectedFields(), function (i, val) {
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