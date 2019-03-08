var hotlistViewModel = function (url, data, users, dateFormat, runReport,itemsPerPage) {
    selfContact = this;

    ko.mapping.fromJS(data, {}, selfContact);

    selfContact.Users = ko.observableArray();
    selfContact.Users(users);

    ko.validation.configure({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null
    });

    selfContact.templ = kendo.template('#if (data.OwnerId == 0 )  { # <span>[|Select..|]</span># } ##if (data.IsDeleted == false )  { # <span>#= OwnerName #</span># } # #if (data.IsDeleted == true )  { # <span style="color:red;">#= OwnerName #</span># } #');


    selfContact.OwnerId = ko.observable(data.OwnerId);
    selfContact.gridvisible = ko.observable("False");
    var lifecycles = [];
    var defaultlifecycle = { "DropdownValueID": 0, "DropdownValue": "[|All|]" };

    for (var l = 0; l < (data.LifecycleStages).length; l++) {
        lifecycles.push((data.LifecycleStages)[l]);
    }

    selfContact.LifecycleStages = ko.observableArray(lifecycles);
    selfContact.OwnerIds = ko.observableArray([]);
    selfContact.LifeStageIds = ko.observableArray([]);
    selfContact.LifecycleStage = ko.observable(data.LifecycleStage);
    selfContact.Periods = ko.observableArray([
       { PeriodId: 1, Period: '[|Last 7 Days|]' },
       { PeriodId: 2, Period: '[|Last 30 Days|]' },
       { PeriodId: 3, Period: '[|Last 3 Months|]' },
       { PeriodId: 4, Period: '[|Month to Date|]' },
       { PeriodId: 5, Period: '[|Year to Date|]' },
       { PeriodId: 6, Period: '[|Last Year|]' },
       { PeriodId: 7, Period: '[|Custom|]' }
    ]);

    selfContact.ShowList = ko.observableArray([
      { ShowTop: 10, Text: '[|Top 10|]' },
      { ShowTop: 25, Text: '[|Top 25|]' },
      { ShowTop: 50, Text: '[|Top 50|]' }
    ]);

    selfContact.ShowTop = ko.observable(10);
    selfContact.PeriodId = ko.observable(2);
    selfContact.ReportId = ko.observable(data.ReportId);
    selfContact.ReportName = ko.observable(data.ReportName);
    selfContact.Columns = ko.observableArray([
       { FieldId: 1, DisplayName: "[|Name|]", Title: "[|Name|]" },
       { FieldId: 2, DisplayName: "[|Account Exec|]", Title: "[|Account Exec|]" },
       { FieldId: 3, DisplayName: "[|Lead Source|]", Title: "[|Lead Source|]" },
       { FieldId: 4, DisplayName: "[|Lifecycle|]", Title: "[|Life Cycle|]" },
       { FieldId: 5, DisplayName: "[|Lead Score|]", Title: "[|Lead Score|]" },
       { FieldId: 6, DisplayName: "[|New Points|]", Title: "[|New Points|]" }
    ]);

    selfContact.GetPhoneNumber = function (phones) {
        var number = "";
        if (phones == null)
            return number;
        else {
            var phoneNumbers = "";
            if (phones.length > 0)
                phoneNumbers = ko.utils.arrayMap(phones, function (phone) {
                    if (phone.IsPrimary == true)
                        return phone.Number + " *";
                    else
                        return phone.Number;
                }).join();
            number = phoneNumbers;
        }
        return number;
    }
    selfContact.getEmail = function (fullname, primaryemail) {
        var name = "";
        var email = "";
        name = encodeURIComponent(fullname);
        email = encodeURIComponent(primaryemail);
        return "contact/_ContactSendMailModel?contactName=" + name + "&emailID=" + email;
    };
    selfContact.Filters = ko.observableArray();
    selfContact.SearchResults = ko.observableArray();
    selfContact.TotalHits = ko.observable(0);
    selfContact.NoOfHits = ko.observable();
    selfContact.SearchPredicateTypeID = ko.observable(4);
    selfContact.CustomPredicateScript = ko.observable();
    selfContact.PageNumber = ko.observable(1);
    selfContact.ResultsCount = ko.observable(10);
    selfContact.TotalResultsCount = ko.observable();

    selfContact.ShowMoreResults = ko.pureComputed(function () {
        var resultsDisplayed = selfContact.SearchResults().length;



        if (resultsDisplayed < selfContact.NoOfHits())
            return true;
        else
            return false;
    });


    var toStartDate = new Date();
    toStartDate.setDate(toStartDate.getDate());
    selfContact.CustomEndDate = ko.observable(moment(new Date()).format()).extend({ required: { message: "" } });

    var fromdate = new Date();
    fromdate.setMonth(fromdate.getMonth() - 1);
    selfContact.CustomStartDate = ko.observable(moment().subtract(30, 'days').format()).extend({ required: { message: "" } });

    var maxdate = new Date();
    maxdate.setDate(maxdate.getDate() + 1);
    selfContact.toMaxDate = ko.observable(maxdate);
    var fromMaxDate = new Date();
    fromMaxDate.setDate(fromMaxDate.getDate() - 1);
    selfContact.fromMaxDate = ko.observable(fromMaxDate);

    selfContact.DateFormat = ko.observable(dateFormat);

    selfContact.fromDateChangeEvent = function () {
        var fromDate = this.value();
        selfContact.CustomStartDate(moment(fromDate).format());
        var toDate = selfContact.CustomEndDate();
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("To date should be greater than From date");
            return false;
        }
    }

    selfContact.toDateChangeEvent = function () {
        var fromDate = selfContact.CustomStartDate();
        var toDate = this.value();
        selfContact.CustomEndDate(moment(toDate).format());
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }

    selfContact.periodChange = function () {
        var value = this.value();
        selfContact.toMaxDate(maxdate);      
        selfContact.CustomEndDate(moment(new Date()).format());
        if (value == 1) {
            selfContact.CustomStartDate(moment().subtract(7, 'days').format());
        }
        else if (value == 2)
            selfContact.CustomStartDate(moment().subtract(30, 'days').format());

        else if (value == 3)
            selfContact.CustomStartDate(moment().subtract(3, 'months').format());

        else if (value == 4)
            selfContact.CustomStartDate(moment().startOf('month').format());

        else if (value == 5)
            selfContact.CustomStartDate(moment().startOf('year').format());

        else if (value == 6) {
            selfContact.CustomStartDate(moment().subtract(1, 'years').format());
        }
        else if (value == 7) {

            var toStartDate = new Date();
            toStartDate.setDate(new Date());
            selfContact.CustomEndDate(new Date());
            var fromdate = new Date();
            fromdate.setDate(fromdate.getDate() - 7);
            selfContact.CustomStartDate(fromdate);          
            selfContact.toMaxDate(new Date());
        }
    }

    selfContact.CustomDateDisplay = ko.pureComputed(function () {
        if (selfContact.PeriodId() == 7) {
            return true;
        } else {
            return false;
        }
    });
    //selfContact.MoreResults = function () {
    //    pageLoader();
    //    var pageNumber = selfContact.PageNumber() + 1;
    //    selfContact.PageNumber(pageNumber);

    //    console.log();
    //    selfContact.Runlist();
    //};
    selfContact.errors = ko.validation.group(selfContact, true);
    selfContact.Runlist = function () {
        selfContact.errors.showAllMessages();

        if (selfContact.errors().length > 0) {
            notifyError("[|Please select valid Date Range.|]");
            return;
        }


        pageLoader();
      
        var selectedOwners = [];
        var selectedlifecycles = [];




        if (selfContact.OwnerId() == 0) {
            for (var l = 0; l < (users).length; l++) {
                selectedOwners.push((users)[l].OwnerId);
            }
        }
        else
            selectedOwners = (selfContact.OwnerId()).length > 0 ? (selfContact.OwnerId()).map(Number) : [];

        if (selfContact.LifecycleStage() == 0) {
            for (var l = 0; l < (lifecycles).length; l++) {
                selectedlifecycles.push((lifecycles)[l].DropdownValueID);
            }
        }
        else
            selectedlifecycles = (selfContact.LifecycleStage()).length > 0 ? (selfContact.LifecycleStage()).map(Number) : [];

        selfContact.OwnerIds(selectedOwners);
        selfContact.LifeStageIds(selectedlifecycles);
    
        var jsondata = ko.toJSON(selfContact);   

        var grid = $('#HotListGrid').data("kendoGrid");
        var newdataSource = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    $.ajax({
                        url: '/Reports/RunHotList',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            ShowTop: $('#HotListGrid').data("kendoGrid").dataSource.pageSize(),
                            CustomStartDate: selfContact.CustomStartDate(),
                            CustomEndDate: selfContact.CustomEndDate(),
                            PageNumber: $('#HotListGrid').data("kendoGrid").dataSource.page(),
                            ReportId: selfContact.ReportId(),
                            OwnerIds: selfContact.OwnerIds(),
                            LifeStageIds: selfContact.LifeStageIds(),
                            ReportName: selfContact.ReportName(),
                            PeriodId: selfContact.PeriodId()
                        }),
                        //  data: jsondata,
                        type: 'post'


                    }).then(function (response) {
                        removepageloader();
                        var filter = $.Deferred()
                        if (response.success) {
                            filter.resolve(response)
                        }
                        else {
                            filter.reject(response.error)
                        }
                        return filter.promise()
                    }).done(function (data) {
                        selfContact.gridvisible("True");
                        selfContact.TotalHits(data.response.Total);
                        options.success(data.response);

                    }).fail(function (error) {
                        notifyError(error);
                    })
                },


            },
            schema: {
                data: "Data",
                total: "Total"
            },
            //pageable : true,
            //pageable: {
            //    pageSizes: ToPageSizeArray(),
            //    messages: {
            //        display: "[|Showing|] {0}-{1} [|from|] {2} [|Contacts|]"
            //    },
            //},
            serverPaging: true,
            // pageSize: 10


        });
        grid.setDataSource(newdataSource);
        grid.dataSource.query({ page: 1, pageSize: itemsPerPage });
       
       
        var pager = grid.pager;
        pager.bind('change', test_pagechange);

        function test_pagechange(e) {
            console.log(e);
        }

    }
    selfContact.ExcelExport = function () {
      
            console.log("in function");
            var ds = new kendo.data.DataSource({
                transport: {
                    read: function (options) {
                        $.ajax({
                            url: '/Reports/RunHotList',
                            contentType: "application/json; charset=utf-8",
                            dataType: "json",
                            data: JSON.stringify({
                                ShowTop: 10000,
                                CustomStartDate: selfContact.CustomStartDate(),
                                CustomEndDate: selfContact.CustomEndDate(),
                                PageNumber: 1,
                                ReportId: selfContact.ReportId(),
                                OwnerIds: selfContact.OwnerIds(),
                                LifeStageIds: selfContact.LifeStageIds(),
                                ReportName: selfContact.ReportName(),
                                PeriodId: selfContact.PeriodId()
                            }),
                            //  data: jsondata,
                            type: 'post'


                        }).then(function (response) {
                            removepageloader();
                            var filter = $.Deferred()
                            if (response.success) {
                                filter.resolve(response)
                            }
                            else {
                                filter.reject(response.error)
                            }
                            return filter.promise()
                        }).done(function (data) {
                          
                            options.success(data.response.Data);

                        }).fail(function (error) {
                            notifyError(error);
                        })
                    }
                },
                schema: {
                    model: {
                        fields: {
                            FullName: { type: "string" },
                            AccountExec: { type: "string" },
                            PhoneNumber: { type: "string" },
                            Email: { type: "string" },
                            LeadSource: { type: "string" },
                            Lifecycle: { type: "string" },
                            LeadScore: { type: "string" },
                            NewPoints: { type: "string" }
                        }
                    }
                }
            });

            var rows = [{
                cells: [
                  { value: "Name" },
                  { value: "Account Executive" },
                  { value: "Phone" },
                  { value: "Email Address" },
                  { value: "Lead Source" },
                  { value: "Life Cycle" },
                  { value: "Lead Score" },
                  { value: "New Points" }
                ]
            }];

            //using fetch, so we can process the data when the request is successfully completed
            ds.fetch(function () {
                var data = this.data();
                console.log("in fetch");
                for (var i = 0; i < data.length; i++) {
                    //push single row for every record
                    rows.push({
                        cells: [
                          { value: data[i].FullName },
                          { value: data[i].AccountExec },
                          { value: data[i].PhoneNumber },
                          { value: data[i].Email },
                          { value: data[i].LeadSource },
                          { value: data[i].Lifecycle },
                          { value: data[i].LeadScore },
                           { value: data[i].NewPoints }
                        ]
                    })
                }
                var workbook = new kendo.ooxml.Workbook({
                    sheets: [
                      {
                          columns: [
                            // Column settings (width)
                            { autoWidth: true },
                            { autoWidth: true },
                            { autoWidth: true },
                            { autoWidth: true },
                            { autoWidth: true },
                            { autoWidth: true },
                           { autoWidth: true },
                            { autoWidth: true }

                          ],
                          // Title of the sheet
                        //  title: "Orders",
                          // Rows of the sheet
                          rows: rows
                      }
                    ]
                });
                //save the file as Excel file with extension xlsx
                kendo.saveAs({
                    dataURI: workbook.toDataURL(),
                    fileName: "HotList.xlsx",
                    //for Exporting the file in IE9 and Safari Browsers
                    proxyURL: "/Reports/ExportToExcel",
                    forceProxy: true
                });
            });

   
    }

    selfContact.viewContacts = function ()
    {

        $.ajax({
            url: '/Reports/HotListReportContactIds',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({
                ShowTop: $('#HotListGrid').data("kendoGrid").dataSource.TotalHits,
                CustomStartDate: selfContact.CustomStartDate(),
                CustomEndDate: selfContact.CustomEndDate(),
                PageNumber: 1,
                ReportId: selfContact.ReportId(),
                OwnerIds: selfContact.OwnerIds(),
                LifeStageIds: selfContact.LifeStageIds(),
                ReportName: selfContact.ReportName(),
                PeriodId: selfContact.PeriodId()
            }),
            //  data: jsondata,
            type: 'post'


        }).then(function (response) {
            removepageloader();
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            }
            else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            //   selfContact.gridvisible("True");
            removepageloader();
          
            localStorage.setItem("ContactsGuid", data.response);
            window.location.href = '../reportcontacts?guid=' + data.response + '&reportType=' + selfContact.ReportType() + '&reportId=' + selfContact.ReportId();

        }).fail(function (error) {
            notifyError(error);
        })

    }
    if (runReport == 'True')
        selfContact.Runlist();

}