var webVisitReportViewModel = function (BASE_URL, data, users, dateFormat, runReport, itemsPerPage) {
    selfWebVisits = this;
    ko.mapping.fromJS(data, {}, selfWebVisits);

    selfWebVisits.Users = ko.observableArray();
    selfWebVisits.Users(users);

    ko.validation.configure({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null
    });

    selfWebVisits.templ = kendo.template('#if (data.OwnerId == 0 )  { # <span>[|Select..|]</span># } ##if (data.IsDeleted == false )  { # <span>#= OwnerName #</span># } # #if (data.IsDeleted == true )  { # <span style="color:red;">#= OwnerName #</span># } #');


    selfWebVisits.OwnerId = ko.observable(data.OwnerId);
    selfWebVisits.gridvisible = ko.observable("False");
    var lifecycles = [];
    var defaultlifecycle = { "DropdownValueID": 0, "DropdownValue": "[|All|]" };

    for (var l = 0; l < (data.LifecycleStages).length; l++) {
        lifecycles.push((data.LifecycleStages)[l]);
    }

    selfWebVisits.LifecycleStages = ko.observableArray(lifecycles);
    selfWebVisits.OwnerIds = ko.observableArray([]);
    selfWebVisits.LifeStageIds = ko.observableArray([]);
    selfWebVisits.LifecycleStage = ko.observable(data.LifecycleStage);
    selfWebVisits.Periods = ko.observableArray([
       { PeriodId: 1, Period: '[|Last 7 Days|]' },
       { PeriodId: 2, Period: '[|Last 30 Days|]' },
       { PeriodId: 3, Period: '[|Last 3 Months|]' },
       { PeriodId: 4, Period: '[|Month to Date|]' },
       { PeriodId: 5, Period: '[|Year to Date|]' },
       { PeriodId: 6, Period: '[|Last Year|]' },
       { PeriodId: 7, Period: '[|Custom|]' }
    ]);

    selfWebVisits.ShowList = ko.observableArray([
      { ShowTop: 10, Text: '[|Top 10|]' },
      { ShowTop: 25, Text: '[|Top 25|]' },
      { ShowTop: 50, Text: '[|Top 50|]' }
    ]);

    selfWebVisits.ShowTop = ko.observable(10);
    selfWebVisits.PeriodId = ko.observable(2);
    selfWebVisits.ReportId = ko.observable(data.ReportId);
    selfWebVisits.ReportName = ko.observable(data.ReportName);
    selfWebVisits.Columns = ko.observableArray([
       { FieldId: 1, DisplayName: "[|Name|]", Title: "[|Name|]" },
       { FieldId: 2, DisplayName: "[|Account Exec|]", Title: "[|Account Exec|]" },
       { FieldId: 3, DisplayName: "[|Lead Source|]", Title: "[|Lead Source|]" },
       { FieldId: 4, DisplayName: "[|Lifecycle|]", Title: "[|Life Cycle|]" },
       { FieldId: 5, DisplayName: "[|Lead Score|]", Title: "[|Lead Score|]" },
       { FieldId: 6, DisplayName: "[|New Points|]", Title: "[|New Points|]" }
    ]);

    selfWebVisits.GetPhoneNumber = function (phones) {
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
    selfWebVisits.getEmail = function (fullname, primaryemail) {
        var name = "";
        var email = "";
        name = encodeURIComponent(fullname);
        email = encodeURIComponent(primaryemail);
        return "contact/_ContactSendMailModel?contactName=" + name + "&emailID=" + email;
    };
    selfWebVisits.Filters = ko.observableArray();
    selfWebVisits.SearchResults = ko.observableArray();
    selfWebVisits.TotalHits = ko.observable(0);
    selfWebVisits.NoOfHits = ko.observable();
    selfWebVisits.SearchPredicateTypeID = ko.observable(4);
    selfWebVisits.CustomPredicateScript = ko.observable();
    selfWebVisits.PageNumber = ko.observable(1);
    selfWebVisits.ResultsCount = ko.observable(10);
    selfWebVisits.TotalResultsCount = ko.observable();

    selfWebVisits.ShowMoreResults = ko.pureComputed(function () {
        var resultsDisplayed = selfWebVisits.SearchResults().length;



        if (resultsDisplayed < selfWebVisits.NoOfHits())
            return true;
        else
            return false;
    });


    var toStartDate = new Date();
    toStartDate.setDate(toStartDate.getDate());
    selfWebVisits.CustomEndDate = ko.observable(moment(new Date()).format()).extend({ required: { message: "" } });

    var fromdate = new Date();
    fromdate.setMonth(fromdate.getMonth() - 1);
    selfWebVisits.CustomStartDate = ko.observable(moment().subtract(30, 'days').format()).extend({ required: { message: "" } });

    var maxdate = new Date();
    maxdate.setDate(maxdate.getDate() + 1);
    selfWebVisits.toMaxDate = ko.observable(maxdate);
    var fromMaxDate = new Date();
    fromMaxDate.setDate(fromMaxDate.getDate() - 1);
    selfWebVisits.fromMaxDate = ko.observable(fromMaxDate);

    selfWebVisits.DateFormat = ko.observable(dateFormat);

    selfWebVisits.fromDateChangeEvent = function () {
        var fromDate = this.value();
        selfWebVisits.CustomStartDate(moment(fromDate).format());
        var toDate = selfWebVisits.CustomEndDate();
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("To date should be greater than From date");
            return false;
        }
    }

    selfWebVisits.toDateChangeEvent = function () {
        var fromDate = selfWebVisits.CustomStartDate();
        var toDate = this.value();
        selfWebVisits.CustomEndDate(moment(toDate).format());
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }

    selfWebVisits.periodChange = function () {
        var value = this.value();
        selfWebVisits.toMaxDate(maxdate);
        selfWebVisits.CustomEndDate(moment(new Date()).format());
        if (value == 1) {
            selfWebVisits.CustomStartDate(moment().subtract(7, 'days').format());
        }
        else if (value == 2)
            selfWebVisits.CustomStartDate(moment().subtract(30, 'days').format());

        else if (value == 3)
            selfWebVisits.CustomStartDate(moment().subtract(3, 'months').format());

        else if (value == 4)
            selfWebVisits.CustomStartDate(moment().startOf('month').format());

        else if (value == 5)
            selfWebVisits.CustomStartDate(moment().startOf('year').format());

        else if (value == 6) {
            selfWebVisits.CustomStartDate(moment().subtract(1, 'years').format());
        }
        else if (value == 7) {

            var toStartDate = new Date();
            toStartDate.setDate(new Date());
            selfWebVisits.CustomEndDate(new Date());
            var fromdate = new Date();
            fromdate.setDate(fromdate.getDate() - 7);
            selfWebVisits.CustomStartDate(fromdate);
            selfWebVisits.toMaxDate(new Date());
        }
    }

    selfWebVisits.CustomDateDisplay = ko.pureComputed(function () {
        if (selfWebVisits.PeriodId() == 7) {
            return true;
        } else {
            return false;
        }
    });
    //selfWebVisits.MoreResults = function () {
    //    pageLoader();
    //    var pageNumber = selfWebVisits.PageNumber() + 1;
    //    selfWebVisits.PageNumber(pageNumber);

    //    console.log();
    //    selfWebVisits.Runlist();
    //};
    selfWebVisits.errors = ko.validation.group(selfWebVisits, true);
    selfWebVisits.Runlist = function () {
        selfWebVisits.errors.showAllMessages();

        if (selfWebVisits.errors().length > 0) {
            notifyError("[|Please select valid Date Range.|]");
            return;
        }
        pageLoader();
        var selectedOwners = [];
        var selectedlifecycles = [];

        if (selfWebVisits.OwnerId() == 0) {
            for (var k = 0; k < (users).length; k++) {
                selectedOwners.push((users)[k].OwnerId);
            }
        }
        else
            selectedOwners = (selfWebVisits.OwnerId()).length > 0 ? (selfWebVisits.OwnerId()).map(Number) : [];

        if (selfWebVisits.LifecycleStage() == 0) {
            for (var l = 0; l < (lifecycles).length; l++) {
                selectedlifecycles.push((lifecycles)[l].DropdownValueID);
            }
        }
        else
            selectedlifecycles = (selfWebVisits.LifecycleStage()).length > 0 ? (selfWebVisits.LifecycleStage()).map(Number) : [];

        selfWebVisits.OwnerIds(selectedOwners);
        selfWebVisits.LifeStageIds(selectedlifecycles);

        var jsondata = ko.toJSON(selfWebVisits);

        var grid = $('#webvisitsreport').data("kendoGrid");
        var newdataSource = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    var sort = grid.dataSource.sort();
                    var field, direction = "";
                    if (sort) {
                        field = sort[0].field;
                        direction = sort[0].dir;
                    }

                    $.ajax({
                        url: '/Reports/WebVisitReport',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            ShowTop: $('#webvisitsreport').data("kendoGrid").dataSource.pageSize(),
                            CustomStartDate: selfWebVisits.CustomStartDate(),
                            CustomEndDate: selfWebVisits.CustomEndDate(),
                            PageNumber: $('#webvisitsreport').data("kendoGrid").dataSource.page(),
                            ReportId: selfWebVisits.ReportId(),
                            OwnerIds: selfWebVisits.OwnerIds(),
                            ReportName: selfWebVisits.ReportName(),
                            PeriodId: selfWebVisits.PeriodId(),
                            SortField: field,
                            SortDirection: direction
                        }),
                        type: 'post'
                    }).then(function(response) {
                        removepageloader();
                        var filter = $.Deferred();
                        if (response.success) {
                            filter.resolve(response);
                        } else {
                            filter.reject(response.error);
                        }
                        return filter.promise();
                    }).done(function(data) {
                        selfWebVisits.gridvisible("True");
                        selfWebVisits.TotalHits(data.response.Total);
                        options.success(data.response);

                    }).fail(function(error) {
                        notifyError(error);
                    });
                }
            },
            schema: {
                data: "Data",
                total: "Total"
            },
            //pageable : true,
            pageable: {
                pageSizes: ToPageSizeArray(),
                messages: {
                    display: "[|Showing|] {0}-{1} [|from|] {2} [|Web Visits|]"
                },
                pageSize: itemsPerPage
            },
            serverSorting: true,
            serverPaging: true
        });
        grid.setDataSource(newdataSource);
        grid.dataSource.query({ page: 1, pageSize: itemsPerPage });


        var pager = grid.pager;
        pager.bind('change', test_pagechange);

        function test_pagechange(e) {
            console.log(e);
        }

    }

    selfWebVisits.ExcelExport = function () {
        var ds = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    $.ajax({
                        url: '/Reports/WebVisitReport',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            ShowTop: 10000,
                            CustomStartDate: selfWebVisits.CustomStartDate(),
                            CustomEndDate: selfWebVisits.CustomEndDate(),
                            PageNumber: 1,
                            ReportId: selfWebVisits.ReportId(),
                            OwnerIds: selfWebVisits.OwnerIds(),
                            ReportName: selfWebVisits.ReportName(),
                            PeriodId: selfWebVisits.PeriodId()
                        }),
                        //  data: jsondata,
                        type: 'post'


                    }).then(function(response) {
                        removepageloader();
                        var filter = $.Deferred();
                        if (response.success) {
                            filter.resolve(response);
                        } else {
                            filter.reject(response.error);
                        }
                        return filter.promise();
                    }).done(function(data) {
                        options.success(data.response.Data);
                    }).fail(function(error) {
                        notifyError(error);
                    });
                }
            },
            schema: {
                model: {
                    fields: {
                        FirstName: { type: "string" },
                        LastName: { type: "string" },
                        Email: { type: "string" },
                        Phone: { type: "string" },
                        Zip: { type: "string" },
                        LifecycleStage: { type: "string" },
                        PageViews: { type: "string" },
                        VisitedOn: { type: "string" },
                        Duration: { type: "string" },
                        Page1: { type: "string" },
                        Page2: { type: "string" },
                        Page3: { type: "string" },
                        Source: { type: "string" },
                        Location: { type: "string" }
                                           
                    }
                }
            }
        });

        var rows = [{
            cells: [
              { value: "First Name" },
              { value: "Last Name" },
              { value: "Email" },
              { value: "Phone" },
              { value: "Zip" },
              { value: "Lifecycle Stage" },
              { value: "Page Views" },
              { value: "Visited On" },
              { value: "Duration" },
              { value: "Top Page1" },
              { value: "Top Page2" },
              { value: "Top Page3" },
              { value: "Source" },
              { value: "Location" }
            ]
        }];
        function convertDate(date) {
            if (date == null) {
                return "";
            }
            var dateFormat = readCookie("dateformat").toUpperCase();
            var millSeconds = date.replace('/Date(', '').replace(')/', '');
            var value = new Date(parseFloat(millSeconds)).ToUtcUtzDate();
            return moment(value).format(dateFormat + " hh:mm A");
        }
        //using fetch, so we can process the data when the request is successfully completed
        ds.fetch(function () {
            var data = this.data();
            console.log("in fetch");
            for (var i = 0; i < data.length; i++) {
                //push single row for every record
                rows.push({
                    cells: [
                        { value: data[i].FirstName },
                        { value: data[i].LastName },
                        { value: data[i].Email },
                        { value: data[i].Phone },
                        { value: data[i].Zip },
                        { value: data[i].LifecycleStage },
                        { value: data[i].PageViews },
                        { value: convertDate(data[i].VisitedOn) },
                        { value: data[i].Duration },
                        { value: data[i].Page1 },
                        { value: data[i].Page2 },
                        { value: data[i].Page3 },
                        { value: data[i].Source },
                        { value: data[i].Location }
                    ]
                });
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
                        { autoWidth: true },
                        { autoWidth: true },
                        { autoWidth: true },
                        { autoWidth: true },
                        { autoWidth: true },
                        { autoWidth: true },
                        { autoWidth: true }
                       
                      ],
                      // Title of the sheet
                     // title: "Orders",
                      // Rows of the sheet
                      rows: rows
                  }
                ]
            });
            //save the file as Excel file with extension xlsx
            kendo.saveAs({
                dataURI: workbook.toDataURL(),
                fileName: "WebVisits.xlsx",
                //for Exporting the file in IE9 and Safari Browsers
                proxyURL: "/Reports/ExportToExcel",
                forceProxy: true
            });
        });
    }



    selfWebVisits.viewContacts = function () {

        $.ajax({
            url: '/Reports/HotListReportContactIds',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({
                ShowTop: $('#HotListGrid').data("kendoGrid").dataSource.TotalHits,
                CustomStartDate: selfWebVisits.CustomStartDate(),
                CustomEndDate: selfWebVisits.CustomEndDate(),
                PageNumber: 1,
                ReportId: selfWebVisits.ReportId(),
                OwnerIds: selfWebVisits.OwnerIds(),
                LifeStageIds: selfWebVisits.LifeStageIds(),
                ReportName: selfWebVisits.ReportName(),
                PeriodId: selfWebVisits.PeriodId()
            }),
            //  data: jsondata,
            type: 'post'


        }).then(function(response) {
            removepageloader();
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response);
            } else {
                filter.reject(response.error);
            }
            return filter.promise();
        }).done(function(data) {
            //   selfWebVisits.gridvisible("True");
            removepageloader();

            localStorage.setItem("ContactsGuid", data.response);
            window.location.href = '../reportcontacts?guid=' + data.response + '&reportType=' + selfWebVisits.ReportType() + '&reportId=' + selfWebVisits.ReportId();

        }).fail(function(error) {
            notifyError(error);
        });

    }
    if (runReport == 'True')
        selfWebVisits.Runlist();

};