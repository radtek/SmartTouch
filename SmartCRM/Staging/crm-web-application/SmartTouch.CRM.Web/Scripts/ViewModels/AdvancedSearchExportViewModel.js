var AdvancedSearchExportViewModel = function(data, url) {
    searchExport = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, searchExport));
    ko.validation.registerExtenders();

    searchExport.SearchFields = ko.observableArray(data.SearchFields);
    
    //searchExport.DefaultFields = ko.observableArray([1, 2, 3, 7]);

    if (typeof selfContactsResults != 'undefined')
        searchExport.SelectedFields = ko.observableArray(selfContactsResults.selectedFields());
    else if (typeof selfSearch != 'undefined')
    {
        var defaultfields = [1, 2, 3, 7];
        ko.utils.arrayForEach(selfSearch.SearchFilters(), function (selectedField) {

            console.log("in");
            console.log(defaultfields.indexOf(selectedField.FieldId()));

            if (defaultfields.indexOf(selectedField.FieldId()) < 0) {
                defaultfields.push(selectedField.FieldId());
            }
        });
        console.log(defaultfields);
        searchExport.SelectedFields = ko.observableArray(defaultfields);
    }
    else if (localStorage.getItem("ContactsGrid") == "1")
        searchExport.SelectedFields = ko.observableArray([15, 16, 17, 3, 20, 7, 1, 2, 29, 41, 24, 22, 18, 19]);
    else
    searchExport.SelectedFields = ko.observableArray([1, 2, 3, 7]);

    searchExport.ExportTypes = ko.observableArray([{ Id: 1, Value: "CSV" }, { Id: 2, Value: "Microsoft Excel" }, { Id: 3, Value: "PDF" }]);
    searchExport.DownloadType = ko.observable(1);
    searchExport.SelectedContactIds = ko.observableArray([]);
    searchExport.SelectedContactIds(fnGetCheckedContactIDs());

    searchExport.ExportContacts = function () {
        
        if (localStorage.getItem("ContactsGrid") == "1" || localStorage.getItem("ContactsGrid") == "2") {
            if (searchExport.SelectedContactIds().length == 0) {
                notifyError("Please select at least one contact");
                return;
            }
        }

        console.log($("#grid").data("kendoGrid").dataSource.data().length);
        if (searchExport.SelectedFields().length < 1)
            notifyError("Please select at least one field");
        else if ($("#grid").data("kendoGrid").dataSource.data().length == 0)
            notifyError("No Contacts to Export");       

        else {
            var jsondata = ko.toJSON(searchExport);
            pageLoader();
            $.ajax({
                url: url + 'ExportResults',
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
                removepageloader();
                localStorage.removeItem("ContactsGrid");
                //window.location = '/AdvancedSearch/DownloadFile?fileKey=' + "64576457" + '&fileName=' + "FORM.pdf";
                window.location = '/AdvancedSearch/DownloadFile?fileKey=' + data.fileKey + '&fileName=' + data.fileName;
            }).fail(function (error) {
                removepageloader();
                notifyError(error);
            })
            //removepageloader();
        }
        return true;
    }

    //var getSearchFields = function () {
    //    var fields = [];
    //    var columns = ko.utils.arrayForEach(searchExport.DefaultFields(), function (selectedField) {
    //       var column = ko.utils.arrayFilter(searchExport.SearchFields(), function (searchField) {
    //            return searchField.FieldId == selectedField;
    //        })[0];
    //        fields.push(column);
    //    });
    //    return fields;
    //};
}