var ExportPersonViewModel = function (data, Contact_BASE_URL, notselectall) {

    var selfExport = this;
    //console.log("C + ontact_BASE_URL" + Contact_BASE_URL);

    ko.validation.init();
    ko.validation.configure({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null,
        grouping: { deep: true, observable: true, live: true }
    });

    if (notselectall == false)
        data.SelectAll = true;

    ko.validatedObservable(ko.mapping.fromJS({}, selfExport));

    ko.validation.rules['minimumLength'] = {
        validator: function (val, contactsField) {
            if (selfExport.selectedFields == null)
                return false;
            return selfExport.selectedFields().length > 0;
        },
        message: '[|Select at least one field|]'
    };
    ko.validation.registerExtenders();

    selfExport.notselectall = ko.observable(notselectall);
    selfExport.SelectAll = ko.observable(data.SelectAll);

    var sfields = [15, 16, 17, 3, 20, 7, 1, 2, 29, 41, 24, 22, 18, 19];   
    for (var l = 0; l < (data.SearchFields).length; l++) {
        if ((data.SearchFields)[l].IsDropdownField == true)
            sfields.push((data.SearchFields)[l].FieldId);
    }    
    // defined the observable for the selected fields
    selfExport.selectedFields = ko.observable().extend({ minimumLength: 1 });
    if (typeof selfmodel != 'undefined') {
        selfExport.selectedFields = ko.observableArray(selfmodel.SelectedFields());
        selfExport.SearchDefinitionId = ko.observable(selfmodel.EntityId());
    }
    else {
        selfExport.selectedFields = ko.observableArray(sfields);
    }
    selfExport.PageSize = ko.observable(module == "Contacts" ? getgridpagesize("grid") : 10000);
    //selfExport.ContactFields = ko.observableArray([
    //"First Name", "Last Name", "Company", "Phone Number", "Phone Number Type", "Email"]);


    //[Description("Name")]
    //FullName = 1,
    //[Description("Email")]
    //Email = 2,
    //[Description("Company")]
    //Company = 3,
    //[Description("Phone")]
    //Phonenumber = 4,
    //[Description("Life cycle")]
    //Lifecycle = 7,
    //[Description("Last contacted")]
    //LastContacted = 8,
    //[Description("PhoneType")]
    //PhoneType = 5,
    //[Description("LastTouchedMethod")]
    //LastTouchedMethod = 9,
    ////[Description("Custom Fields")]
    ////CustomFields = 10,
    //[Description("Address")]
    //Address = 6,
    //[Description("CreatedDate")]
    //CreatedDate = 10

    //{ Name: 'FullName', ID: 1 },
    //{ Name: 'Email', ID: 2 },
    //{ Name: 'Company', ID: 3 },
    //{ Name: 'Phone Number', ID: 4 },
    //{ Name: 'Life cycle', ID: 7 },
    //{ Name: 'FullName', ID: 1 },
    //{ Name: 'FullName', ID: 1 },
    //{ Name: 'FullName', ID: 1 },
    //{ Name: 'FullName', ID: 1 },
    //{ Name: 'FullName', ID: 1 }

    selfExport.SearchFields = ko.observableArray(data.SearchFields);

    //$.ajax({
    //    url: Contact_BASE_URL + "GetExportFieldTypes",
    //    type: 'get',
    //    dataType: 'json',
    //    contentType: "application/json; charset=utf-8"
    //}).then(function (response) {            
    //    var filter = $.Deferred()            
    //    if (response.success) {                
    //    filter.resolve(response)            
    //    } else {                
    //    filter.reject(response.error)            
    //    }            
    //    return filter.promise()        
    //}).done(function (data) {
    //    selfExport.ContactFields(data.response);
    //    var selectedfields = data.response.map(function (item) {
    //        return item['TypeId'];
    //    });
    //    selfExport.selectedFields(selectedfields);
    //}).fail(function (error) {        
    //    notifyError(error);
    //})
    //    success: function (data) {
    //        selfExport.ContactFields(data);            
    //        var selectedfields = data.map(function (item) {
    //            return item['TypeId'];
    //        });            
    //        selfExport.selectedFields(selectedfields);            
    //    },
    //    error: function (data) {
    //    }
    //});

    selfExport.SortBy = ko.observable(1);
    selfExport.selectedDropDownItems = ko.observable(selfExport.SearchFields());

    selfExport.selectedFields.subscribe(function (value) {
        
        //var selectedfields = $.grep(selfExport.ContactFields(), function (e)
        //{
        //    return $.inArray(e.TypeId, selfExport.selectedFields());
        //});
        var selectedfields = [];
        for (var i = 0; i < selfExport.selectedFields().length; i++) {
            selectedfields.push($.grep(selfExport.SearchFields(), function (e) {
                return e.FieldId == selfExport.selectedFields()[i];
            })[0]);
        }

        
        selfExport.selectedDropDownItems(selectedfields);
        if (selfExport.selectedDropDownItems().length > 0) {
            selfExport.SortBy(selfExport.selectedDropDownItems()[0].FieldId);
        }
    });

    selfExport.SortOrderDropDownItems = ko.observableArray([
        { OrderID: 1, OrderName: 'Ascending' },
        { OrderID: 2, OrderName: 'Descending' }
    ]);

    selfExport.SortOrder = ko.observable(1);

    selfExport.FormatDropDownItems = ko.observableArray(["CSV" , "Excel", "PDF" ]);
    selfExport.DownLoadAs = ko.observable("CSV");
    //$.ajax({
    //    url: Contact_BASE_URL + "GetDownLoadFormat",
    //    type: 'get',
    //    dataType: 'json',
    //    contentType: "application/json; charset=utf-8"
    //}).then(function (response) {            
    //    var filter = $.Deferred()            
    //    if (response.success) {                
    //    filter.resolve(response)            
    //    } else {                
    //    filter.reject(response.error)            
    //    }            
    //    return filter.promise()        
    //}).done(function (data) {
    //    console.log("GetDownloadFormat");
    //    selfExport.FormatDropDownItems(data.response);
    //}).fail(function (error) {                    
    //    notifyError(error);
    //})    

    selfExport.ContactID = ko.observable(fnGetCheckedContactIDs());
    //selfExport.FormContactIds = ko.observableArray();

    selfExport.ExportContacts = function (obj) {

        //getFormRelatedContacts();    
        if (fnGetCheckedContactIDs().length == 0 && readCookie("contactid") == null  && selfExport.notselectall() == true)
        {
            notifyError("[|Please select at least one contact|]");
            return;
        }
        else
        {
            if (selfExport.notselectall() == false) {
                selfExport.SelectAll = ko.observable(true);
            }
            var contactidarray = [];

            if (readCookie("contactid") == null && fnGetCheckedContactIDs().length == 0) {
                if (selfExport.SelectAll() == true) {
                    notifyError("[|Please select at least one contact|]");
                    return;
                }
            }

            else if (readCookie("contactid") == null && fnGetCheckedContactIDs().length > 0) {
                contactidarray = fnGetCheckedContactIDs();
                selfExport.ContactID(contactidarray);
            }

            else if (readCookie("contactid") != null && fnGetCheckedContactIDs().length == 0) {
                contactidarray.push(readCookie("contactid"));
                selfExport.ContactID(contactidarray);
            }

            if (selfExport.selectedDropDownItems().length == 0) {
                notifyError("[|Please select at least one field|]");
                return;
            }

            pageLoader();

            var jsondata = ko.toJSON(selfExport);


            $.ajax({
                url: Contact_BASE_URL + 'ExprortAsFile',
                type: 'post',
                data: jsondata,
                dataType: 'json',
                contentType: "application/json; charset=utf-8"
            }).then(function (response) {
                var filter = $.Deferred()
                if (response.success) {
                    filter.resolve(response)
                } else {
                    filter.reject(response.error)
                }
                return filter.promise()
            }).done(function (data) {
                removepageloader();
                if (data.fileKey != undefined) {
                    window.location = '/Contact/DownloadFile?fileKey=' + data.fileKey + '&fileName=' + data.fileName;
                }
                else {
                    notifySuccess('[|Your bulk data export request has been scheduled successfully.|]');
                    setTimeout(function () {
                        createCookie('log', false, 1);
                        window.location.href = document.URL;
                    }, setTimeOutTimer);
                }
                $('.btn-group.showtopinner').removeClass('showtopinner');
            }).fail(function (error) {
                removepageloader();
                notifyError(error);
                $('.btn-group.showtopinner').removeClass('showtopinner');
            })
            CloseTopInner(obj);
            return true;
        }
    }

    selfExport.ExportFormContacts = function (obj) {
        //getFormRelatedContacts();

        var contactidarray = [];

        checkedvalues = fnGetChkvalGrid('chkform');
        if (checkedvalues.length == 0 || checkedvalues.length > 1) {
            notifyError("[|Select only one form|]");
            return;
        }

        if (selfExport.selectedDropDownItems().length == 0) {
            notifyError("[|Please select at least one field|]");
            return;
        }
        innerLoader('more2');
        $.ajax({
            url: Contact_BASE_URL + 'GetFormContactIds',
            type: 'get',
            data: { 'FormID': checkedvalues[0] },
            contentType: "application/json; charset=utf-8"
        }).then(function (response) {
            var filter = $.Deferred()            
            if (response.success) {                
            filter.resolve(response)            
            } else {                
            filter.reject(response.error)            
            }            
            return filter.promise()        
        }).done(function (data) {
            if (data.response == "" || data.response == []) {
                $('.btn-group.showtopinner').removeClass('showtopinner');
                notifyError("[|There is no contacts related to this form|]");
            }
            else {
                selfExport.ContactID(data.response);
                var json = ko.toJSON(selfExport);
                contactidarray = data.response;
                selfExport.FormContactIds = ko.observableArray(data.response);
                selfExport.FormContactIds(contactidarray);
                $.ajax({
                    url: Contact_BASE_URL + 'ExprortAsFile',
                    type: 'post',
                    dataType: 'json',
                    data: json,
                    contentType: "application/json; charset=utf-8"
                }).then(function (response) {            
                    var filter = $.Deferred()
                    if (response.success) {
                        filter.resolve(response)
                    } else {
                        filter.reject(response.error)
                    } return filter.promise()
                }).done(function (data) {
                    removeinnerLoader('more2');
                    window.location = '/Contact/DownloadFile?fileKey=' + data.fileKey + '&fileName=' + data.fileName;
                    $('.btn-group.showtopinner').removeClass('showtopinner');
                    CloseTopInner(obj);
                }).fail(function (error) {
                    removeinnerLoader('more2');
                    $('.btn-group.showtopinner').removeClass('showtopinner');
                    notifyError(error);
                })                
            }
        }).fail(function (error) {            
            notifyError(error);
        })        
        return true;
    }

    selfExport.SomeField = ko.observable("");
}