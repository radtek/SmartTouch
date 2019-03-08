var tourByContactsReportViewModel = function (data, BASE_URL, runReport, itemsPerPage, dateFormat) {
    TourByContact = this;

    ko.validation.configure({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null
    });

    TourByContact.ReportId = ko.observable(data.ReportId);
    TourByContact.ReportName = ko.observable(data.ReportName);
    TourByContact.PeriodId = ko.observable(8);
    TourByContact.DateRange = ko.observable('D');
    TourByContact.CustomStartDate = ko.observable();
    TourByContact.CustomEndDate = ko.observable();
    TourByContact.ReportType = ko.observable(20);
    TourByContact.TourTypes = ko.observableArray(data.TourTypes);
    TourByContact.Communities = ko.observableArray(data.Communities);
    TourByContact.TourStatuses = ko.observableArray([
    { Status: '[|Not Completed|]', StatusID: 0 },
    { Status: '[|Completed|]', StatusID: 1 }
    ]);
    TourByContact.TourTypeIds = ko.observable([]);
    TourByContact.CommunityIds = ko.observableArray([]);
    TourByContact.TourStatusIds = ko.observableArray([]);

    TourByContact.TourType = ko.observable(0);
    TourByContact.Community = ko.observable(0);
    TourByContact.TourStatus = ko.observable('');

    TourByContact.gridvisible = ko.observable("False");



    var today = new Date();
    var toStartDate = new Date();
    toStartDate.setDate(toStartDate.getDate());
    TourByContact.CustomEndDate = ko.observable(kendo.toString(toStartDate, dateFormat)).extend({ required: { message: "" } });


    var fromdate = new Date();
    fromdate.setMonth(fromdate.getMonth() - 1);
    TourByContact.CustomStartDate = ko.observable(kendo.toString(fromdate, dateFormat)).extend({ required: { message: "" } });

    TourByContact.Periods = ko.observableArray([
      { PeriodId: 1, Period: '[|Last 7 Days|]' },
      { PeriodId: 2, Period: '[|Last 30 Days|]' },
      { PeriodId: 3, Period: '[|Last 3 Months|]' },
      { PeriodId: 4, Period: '[|Month to Date|]' },
      { PeriodId: 5, Period: '[|Year to Date|]' },
      { PeriodId: 6, Period: '[|Last Year|]' },
      { PeriodId: 7, Period: '[|Custom|]' },
      { PeriodId: 8, Period: '[|Upcoming 30 Days|]' }
    ]);

    var toStartDate = new Date();
    TourByContact.CustomEndDate(moment().add(30, 'days').format());
    TourByContact.CustomStartDate(moment(toStartDate).format());

    TourByContact.periodChange = function () {
        var value = this.value();
        var toStartDate = new Date();
        TourByContact.toMaxDate(maxdate);
        if (value != "8")
            TourByContact.CustomEndDate(moment(toStartDate).format());

        if (value == "1")  // last 7 days
            TourByContact.CustomStartDate(moment().subtract(7, 'days').format());

        else if (value == "2")  // last 30 days
            TourByContact.CustomStartDate(moment().subtract(30, 'days').format());

        else if (value == "3")  //last 3 months
            TourByContact.CustomStartDate(moment().subtract(3, 'months').format());


        else if (value == "4")  //month to date
            TourByContact.CustomStartDate(moment().startOf('month').format());


        else if (value == "5")   // month to year
            TourByContact.CustomStartDate(moment().startOf('year').format());


        else if (value == "6") { //last year
            TourByContact.CustomStartDate(moment().subtract(1, 'years').format());
        }
        else if (value == "8") { //Upcoming 30 days

            TourByContact.CustomStartDate(moment(toStartDate).format());
            TourByContact.CustomEndDate(moment().add(30, 'days').format());
        }
        if (value == "7") { // custom

            toStartDate.setDate(toStartDate.getDate());
            TourByContact.CustomEndDate(toStartDate);
            var fromdate = new Date();
            fromdate.setDate(fromdate.getDate() - 7);
            TourByContact.CustomStartDate(fromdate);
            TourByContact.toMaxDate(new Date());
        }
    }

    TourByContact.CustomDateDisplay = ko.pureComputed(function () {
        if (TourByContact.PeriodId() == 7) {
            return true;
        } else {
            return false;
        }
    });

    var maxdate = new Date();
    maxdate.setDate(maxdate.getDate() + 1);
    TourByContact.toMaxDate = ko.observable(maxdate);
    var fromMaxDate = new Date();
    fromMaxDate.setDate(fromMaxDate.getDate() - 1);
    TourByContact.fromMaxDate = ko.observable(fromMaxDate);
    TourByContact.DateFormat = ko.observable(dateFormat);

    TourByContact.fromDateChangeEvent = function () {
        var fromDate = this.value();
        TourByContact.CustomStartDate(kendo.toString(fromDate, dateFormat));
        var toDate = TourByContact.CustomEndDate();
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            fromDate.setDate(fromDate.getDate() + 7);
            TourByContact.CustomEndDate(kendo.toString(fromDate, dateFormat));
        }
    }

    TourByContact.toDateChangeEvent = function () {
        var fromDate = TourByContact.CustomStartDate();
        var toDate = this.value();
        TourByContact.CustomEndDate(kendo.toString(toDate, dateFormat));
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            toDate.setDate(toDate.getDate() - 7);
            TourByContact.CustomStartDate(kendo.toString(toDate, dateFormat));
        }
    }

    TourByContact.errors = ko.validation.group(TourByContact);

    TourByContact.Runlist = function () {

        TourByContact.errors.showAllMessages();

        if (TourByContact.errors().length > 0) {
            notifyError("[|Please select valid Date Range.|]");
            return;
        }

        pageLoader();
        var selectedCommunities = [];
        var selectedTourTypes = [];
        var selectedTourStatuses = [];

        if (TourByContact.Community() == 0) {
            for (var l = 0; l < (TourByContact.Communities()).length; l++) {
                selectedCommunities.push((TourByContact.Communities())[l].DropdownValueID);
            }
        }
        else
            selectedCommunities = (TourByContact.Community()).length > 0 ? (TourByContact.Community()).map(Number) : [];

        if (TourByContact.TourType() == 0) {
            for (var l = 0; l < (TourByContact.TourTypes()).length; l++) {
                selectedTourTypes.push((TourByContact.TourTypes())[l].DropdownValueID);
            }
        }
        else
            selectedTourTypes = (TourByContact.TourType()).length > 0 ? (TourByContact.TourType()).map(Number) : [];

        TourByContact.CommunityIds(selectedCommunities);
        TourByContact.TourTypeIds(selectedTourTypes);
        if (TourByContact.TourStatus().length == 0)
            TourByContact.TourStatusIds([0, 1]);
        else
            TourByContact.TourStatusIds(TourByContact.TourStatus());

        var jsondata = ko.toJSON(TourByContact);

        var grid = $('#TourByContactsGrid').data("kendoGrid");
        var newdataSource = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    $.ajax({
                        url: '/Reports/GetTourByContactsReportData',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            ShowTop: $('#TourByContactsGrid').data("kendoGrid").dataSource.pageSize(),
                            CustomStartDate: TourByContact.CustomStartDate(),
                            CustomEndDate: TourByContact.CustomEndDate(),
                            PageNumber: $('#TourByContactsGrid').data("kendoGrid").dataSource.page(),
                            ReportId: TourByContact.ReportId(),
                            ReportName: TourByContact.ReportName(),
                            TourTypeIds: TourByContact.TourTypeIds(),
                            CommunityIds: TourByContact.CommunityIds(),
                            TourStatusIds: TourByContact.TourStatusIds(),
                            Sorts: $('#TourByContactsGrid').data("kendoGrid").dataSource.sort()
                        }),
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
                        TourByContact.gridvisible("True");
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
            serverPaging: true,

        });
        grid.setDataSource(newdataSource);
        grid.dataSource.query({ page: 1, pageSize: itemsPerPage });


        var pager = grid.pager;
        pager.bind('change', test_pagechange);

        function test_pagechange(e) {
            console.log(e);
        }
    };

    TourByContact.ExcelExport = function () {

        console.log("in function");
        var ds = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    $.ajax({
                        url: '/Reports/GetTourByContactsReportData',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            ShowTop: 100000,
                            CustomStartDate: TourByContact.CustomStartDate(),
                            CustomEndDate: TourByContact.CustomEndDate(),
                            PageNumber: 1,
                            ReportId: TourByContact.ReportId(),
                            ReportName: TourByContact.ReportName(),
                            TourTypeIds: TourByContact.TourTypeIds(),
                            CommunityIds: TourByContact.CommunityIds(),
                            TourStatusIds: TourByContact.TourStatusIds()
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
                        ContactName: { type: "string" },
                        PRimaryEmail: { type: "string" },
                        PrimaryPhoneNumber: { type: "string" },
                        TourType: { type: "string" },
                        TourDetails: { type: "string" },
                        TourDate: { type: "datetime" },
                        TourStatus: { type: "string" },
                        Commnity: { type: "string" },
                        LifeCycleStage: { type: "string" },
                        //OpportunityStage: { type: "string" },
                        assignedTo: { type: "string" },
                        CreatedBy: { type: "string" },
                        CreatedOn: { type: "datetime" },
                    }
                }
            }
        });

        var rows = [{
            cells: [
              { value: "Contact Name" },
              { value: "Email" },
              { value: "Primary Phone" },
              { value: "Tour Type" },
              { value: "Tour Details" },
              { value: "Tour Date" },
              { value: "Tour Status" },
              { value: "Tour Community" },
              { value: "Life Cycle Stage" },
              //{ value: "Opportunity Stage" },
              { value: "Assigned To" },
              { value: "Created By" },
              { value: "Created Date" }
            ]
        }];

        //using fetch, so we can process the data when the request is successfully completed
        ds.fetch(function () {
            var data = this.data();
            console.log("in fetch");
            for (var i = 0; i < data.length; i++) {
                //push single row for every record
                if (data[i].TourDate) {

                    var tourDate = data[i].TourDate.match(/\d+/)[0] * 1;
                    data[i].TourDate = kendo.toString(new Date(tourDate), dateFormat)
                }
                if (data[i].CreatedOn) {
                    var createdDate = data[i].CreatedOn.match(/\d+/)[0] * 1;
                    data[i].CreatedOn = kendo.toString(new Date(createdDate), dateFormat)
                }
                rows.push({
                    cells: [
                      { value: data[i].ContactName },
                      { value: data[i].PRimaryEmail },
                      { value: data[i].PrimaryPhoneNumber },
                      { value: data[i].TourType },
                      { value: data[i].TourDetails },
                      { value: data[i].TourDate },
                      { value: (data[i].TourStatus == "false" ? "Not Completed" : "Completed") },
                      { value: data[i].Commnity },
                      { value: data[i].LifeCycleStage },
                      //{ value: data[i].OpportunityStage },
                      { value: data[i].assignedTo },
                      { value: data[i].CreatedBy },
                      { value: data[i].CreatedOn }
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
                        { autoWidth: true },
                        { autoWidth: true },
                        { autoWidth: true },
                        //{ autoWidth: true },
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
                fileName: "TourByContactsReport.xlsx",
                //for Exporting the file in IE9 and Safari Browsers
                proxyURL: "/Reports/ExportToExcel",
                forceProxy: true
            });
        });

    }

    TourByContact.RunReport = function () {
        TourByContact.Runlist();
    }

    if (runReport == 'True')
        TourByContact.Runlist();
}