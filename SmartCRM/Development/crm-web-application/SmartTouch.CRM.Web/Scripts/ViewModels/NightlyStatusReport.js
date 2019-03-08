var nStatusReport = function (url, data, dateFormat, itemsPerPage, runReport) {
    nsReport = this;
    nsReport.ReportType = ko.observable(data.ReportType);
    nsReport.ReportId = ko.observable(data.ReportId);
    nsReport.ReportName = ko.observable(data.ReportName);
    nsReport.LoginFrequencyReports = ko.observableArray([
        { Id: 1, Value: '[|Most Active Users|]' },
        { Id: 2, Value: '[|Most Recent Users|]' }
    ]);
    nsReport.LoginFrequencyReportID = ko.observable(1);
    nsReport.Subscriptions = ko.observableArray([
        { SubscriptionID: 0, Value: '[|All|]' },
        { SubscriptionID: 2, Value: '[|Standard|]' },
        { SubscriptionID: 3, Value: '[|BDX|]' }
    ]);
    nsReport.SubscriptionID = ko.observable(2);
    nsReport.Periods = ko.observableArray([
      { PeriodId: 1, Period: '[|Last 7 Days|]' },
      { PeriodId: 2, Period: '[|Last 30 Days|]' },
      { PeriodId: 3, Period: '[|Last 3 Months|]' },
      { PeriodId: 4, Period: '[|Month to Date|]' },
      { PeriodId: 5, Period: '[|Year to Date|]' },
      { PeriodId: 6, Period: '[|Last Year|]' },
      { PeriodId: 7, Period: '[|Custom|]' }
    ]);
    nsReport.PeriodId = ko.observable(2);

    nsReport.CustomDateDisplay = ko.pureComputed(function () {
        if (nsReport.PeriodId() == 7)
            return true;
        else
            return false;
    });

    var toStartDate = new Date();
    toStartDate.setDate(toStartDate.getDate());
    nsReport.CustomEndDate = ko.observable(kendo.toString(toStartDate, dateFormat)).extend({ required: { message: "" } });
    nsReport.CustomEndDatePrev = ko.observable();

    var fromdate = new Date();
    fromdate.setMonth(fromdate.getMonth() - 1);
    nsReport.CustomStartDate = ko.observable(kendo.toString(fromdate, dateFormat)).extend({ required: { message: "" } });
    nsReport.CustomStartDatePrev = ko.observable();

    nsReport.CustomEndDate(moment(new Date()).format());
    nsReport.CustomStartDate(moment().subtract(30, 'days').format());
    var maxdate = new Date();
    maxdate.setDate(maxdate.getDate() + 1);
    nsReport.toMaxDate = ko.observable(maxdate);
    var fromMaxDate = new Date();
    fromMaxDate.setDate(fromMaxDate.getDate() - 1);
    nsReport.fromMaxDate = ko.observable(fromMaxDate);
    nsReport.DateFormat = ko.observable(dateFormat);
    nsReport.DateFormat = ko.observable(dateFormat);
    nsReport.DateRange = ko.observable('D');

    nsReport.periodChange = function () {
        var value = this.value();
        nsReport.toMaxDate(maxdate);
        if (value == "1")  // last 7 days
            nsReport.CustomStartDate(moment().subtract(7, 'days').format());
        else if (value == "2")  // last 30 days
            nsReport.CustomStartDate(moment().subtract(30, 'days').format());
        else if (value == "3")  //last 3 months
            nsReport.CustomStartDate(moment().subtract(3, 'months').format());
        else if (value == "4")  //month to date
            nsReport.CustomStartDate(moment().startOf('month').format());
        else if (value == "5")   // month to year
            nsReport.CustomStartDate(moment().startOf('year').format());
        else if (value == "6") { //last year
            nsReport.CustomStartDate(moment().subtract(1, 'years').format());
        }
        if (value == "7") { // custom
            var toStartDate = new Date();
            toStartDate.setDate(toStartDate.getDate());
            nsReport.CustomEndDate(toStartDate);
            var fromdate = new Date();
            fromdate.setDate(fromdate.getDate() - 7);
            nsReport.CustomStartDate(fromdate);
            nsReport.toMaxDate(new Date());
        }
    }

    nsReport.fromDateChangeEvent = function () {
        var fromDate = this.value();
        nsReport.CustomStartDate(kendo.toString(fromDate, dateFormat));
        var toDate = nsReport.CustomEndDate();
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }

    nsReport.toDateChangeEvent = function () {
        var fromDate = nsReport.CustomStartDate();
        var toDate = this.value();
        nsReport.CustomEndDate(kendo.toString(toDate, dateFormat));
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }

    nsReport.AccountsList = ko.observableArray(data.AccountsList);
    nsReport.AccountIdsList = ko.observable([]);
    nsReport.AccountIds = ko.observable();
    nsReport.GetAccounts = function () {
        $.ajax({
            url: '../gaa/' + nsReport.SubscriptionID(),
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                removeinnerLoader('loader');
                nsReport.AccountsList(data.response);
                //ko.applyBindings(nsReport);
            },
            error: function () {
                // console.log(errors);
            }
        });
    };
    nsReport.SubscriptionID.subscribe(function (id) {
        if (id) {
            nsReport.GetAccounts();
        }
    });

    nsReport.StatusGridVisible = ko.observable(true);
    nsReport.CampaignGridVisible = ko.observable(false);


    nsReport.RunReport = function () {
        pageLoader();

        var Ids = nsReport.AccountsList().map(function (list) { return list.Id });
        $.each(nsReport.AccountIdsList(), function (i, v) {
            if (Ids.indexOf(v) <= -1) {
                nsReport.AccountIds(Ids);
            }
            else
                nsReport.AccountIds(nsReport.AccountIdsList());
        });
        if (nsReport.AccountIdsList().length == 0) {
            nsReport.AccountIds(Ids);
        }

        if (nsReport.ReportType() == 22)
            nsReport.createNightlyStatusGrid();
        else if (nsReport.ReportType() == 21)
            nsReport.createNightlyCampaignGrid();
        else if (nsReport.ReportType() == 23)
            nsReport.createLoginReport();
        else if (nsReport.ReportType() == 24)
            nsReport.createBouncedEmailReport();
    }

    nsReport.ExcelReport = function () {
        if (nsReport.ReportType() == 24)
            nsReport.bouncedEmailReportExport();
        else {
            var grid = nsReport.ReportType() == 21 ? $('#nightlyCampaignGrid').data("kendoGrid") : nsReport.ReportType() == 22 ? $('#nightlyStatusGrid').data("kendoGrid") : nsReport.ReportType() == 24 ? $('#bouncedEmailGrid').data("kendoGrid")
            : $('#loginReportGrid').data("kendoGrid");
            grid.saveAsExcel();
        }
    };

    nsReport.createNightlyStatusGrid = function () {
        var grid = $('#nightlyStatusGrid').data("kendoGrid");
        var newdataSource = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    $.ajax({
                        url: '/Reports/GetNightlyStatusReport',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            CustomStartDate: nsReport.CustomStartDate(),
                            CustomEndDate: nsReport.CustomEndDate(),
                            ReportId: nsReport.ReportId(),
                            ReportName: nsReport.ReportName(),
                            AccountIds: nsReport.AccountIds(),
                            SubscriptionID: nsReport.SubscriptionID(),
                            ReportType: nsReport.ReportType()
                        }),
                        type: 'post'
                    }).then(function (response) {
                        removepageloader();
                        var filter = $.Deferred();
                        if (response.success) {
                            filter.resolve(response);
                        }
                        else {
                            filter.reject(response.error);
                        }
                        return filter.promise();
                    }).done(function (data) {
                        //nsReport.gridvisible("True");
                        if (data.response != null) {
                        }
                        options.success(data.response);

                    }).fail(function (error) {
                        removepageloader();
                        notifyError(error);
                    })
                },
            },
            schema: {
                data: "Data",
                total: "Total",
                model: {
                    fields: {
                        AccountName: { type: "string" },
                        SenderReputationCount: { type: "number" },
                        CampaignsSent: { type: "number" },
                        Recipients: { type: "number" },
                        Sent: { type: "number" },
                        Delivered: { type: "number" },
                        Bounced: { type: "string" },
                        Opened: { type: "string" },
                        Clicked: { type: "string" },
                        TagsAll: { type: "number" },
                        TagsActive: { type: "number" },
                        SSAll: { type: "number" },
                        SSActive: { type: "number" }

                    }
                }
            }
        });
        grid.setDataSource(newdataSource);
        grid.dataSource.query({ page: 1, pageSize: parseInt(itemsPerPage) });

    };

    nsReport.createNightlyCampaignGrid = function () {
        var grid = $('#nightlyCampaignGrid').data("kendoGrid");
        var newdataSource = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    $.ajax({
                        url: '/Reports/GetNightlyStatusReport',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            CustomStartDate: nsReport.CustomStartDate(),
                            CustomEndDate: nsReport.CustomEndDate(),
                            ReportId: nsReport.ReportId(),
                            ReportName: nsReport.ReportName(),
                            AccountIds: nsReport.AccountIds(),
                            SubscriptionID: nsReport.SubscriptionID(),
                            ReportType: nsReport.ReportType()
                        }),
                        type: 'post'
                    }).then(function (response) {
                        removepageloader();
                        var filter = $.Deferred();
                        if (response.success) {
                            filter.resolve(response);
                        }
                        else {
                            filter.reject(response.error);
                        }
                        return filter.promise();
                    }).done(function (data) {
                        //nsReport.gridvisible("True");
                        if (data.response != null) {
                        }
                        options.success(data.response);

                    }).fail(function (error) {
                        removepageloader();
                        notifyError(error);
                    })
                },
            },
            schema: {
                data: "Data",
                total: "Total",
                model: {
                    fields: {
                        //Day: { type: "number" },
                        AccountName: { type: "string" },
                        CampaignId: { type: "number" },
                        CampaignSubject: { type: "string" },
                        Vmta: { type: "string" },
                        Recipients: { type: "number" },
                        Sent: { type: "number" },
                        Delivered: { type: "string" },
                        Bounced: { type: "string" },
                        Opened: { type: "string" },
                        Clicked: { type: "string" },
                        TagsAll: { type: "number" },
                        TagsActive: { type: "number" },
                        SavedSearchAll: { type: "number" },
                        SavedSearchActive: { type: "number" }

                    }
                }
            }
        });
        grid.setDataSource(newdataSource);
        grid.dataSource.query({ page: 1, pageSize: parseInt(itemsPerPage) });
        grid.refresh();
    };

    nsReport.createLoginReport = function () {
        var grid = $('#loginReportGrid').data("kendoGrid");
        var newdataSource = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    $.ajax({
                        url: '/Reports/GetLoginFrequncyReport',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            CustomStartDate: nsReport.CustomStartDate(),
                            CustomEndDate: nsReport.CustomEndDate(),
                            ReportId: nsReport.ReportId(),
                            ReportName: nsReport.ReportName(),
                            AccountIds: nsReport.AccountIds(),
                            SubscriptionID: nsReport.SubscriptionID(),
                            ReportType: nsReport.ReportType(),
                            LoginFrequencyReportID: nsReport.LoginFrequencyReportID()
                        }),
                        type: 'post'
                    }).then(function (response) {
                        removepageloader();
                        var filter = $.Deferred();
                        if (response.success) {
                            filter.resolve(response);
                        }
                        else {
                            filter.reject(response.error);
                        }
                        return filter.promise();
                    }).done(function (data) {
                        //nsReport.gridvisible("True");
                        if (data.response != null) {
                        }
                        options.success(data.response);

                    }).fail(function (error) {
                        removepageloader();
                        notifyError(error);
                    })
                },
            },
            schema: {
                data: "Data",
                total: "Total",
                model: {
                    fields: {
                        AccountName: { type: "string" },
                        FirstName: { type: "string" },
                        LastName: { type: "string" },
                        Email: { type: "string" },
                        RecentLoginDate: { type: "Date" },
                        LoggedInCount: { type: "number" }
                    }
                }
            }
        });

        grid.setDataSource(newdataSource);
        grid.dataSource.query({ page: 1, pageSize: parseInt(itemsPerPage) });
        grid.refresh();
        if (nsReport.LoginFrequencyReportID() == 1) {
            //Active Users
            grid = $('#loginReportGrid').data("kendoGrid");
            grid.showColumn(5);
            grid.bind("dataBound", function () {
                $.each($("#loginReportGrid tr td.loggedincount"), function (i, v) {
                    $("#loginReportGrid tr td.loggedincount")[i].style.display = "table-cell";
                });
            });
        }
        else {
            //Recent Users
            grid = $('#loginReportGrid').data("kendoGrid");
            grid.hideColumn(5);
            grid.bind("dataBound", function () {
                $.each($("#loginReportGrid tr td.loggedincount"), function (i, v) {
                    $("#loginReportGrid tr td.loggedincount")[i].style.display = "none";
                });
            });
        }
    };

    nsReport.createBouncedEmailReport = function () {
        var grid = $('#bouncedEmailGrid').data("kendoGrid");
        var newdataSource = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    $.ajax({
                        url: '/Reports/GetNightlyStatusReport',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            ShowTop: $('#bouncedEmailGrid').data("kendoGrid").dataSource.pageSize(),
                            CustomStartDate: nsReport.CustomStartDate(),
                            CustomEndDate: nsReport.CustomEndDate(),
                            ReportId: nsReport.ReportId(),
                            ReportName: nsReport.ReportName(),
                            AccountIds: nsReport.AccountIds(),
                            SubscriptionID: nsReport.SubscriptionID(),
                            ReportType: nsReport.ReportType(),
                            PageNumber: $('#bouncedEmailGrid').data("kendoGrid").dataSource.page()
                        }),
                        type: 'post'
                    }).then(function (response) {
                        removepageloader();
                        var filter = $.Deferred();
                        if (response.success) {
                            filter.resolve(response);
                        }
                        else {
                            filter.reject(response.error);
                        }
                        return filter.promise();
                    }).done(function (data) {
                        //nsReport.gridvisible("True");
                        if (data.response != null) {
                        }
                        options.success(data.response);

                    }).fail(function (error) {
                        removepageloader();
                        notifyError(error);
                    })
                },
            },
            serverPaging: true,
            schema: {
                data: "Data",
                total: "Total",
                model: {
                    fields: {
                        Email: { type: "string" },
                        Account: { type: "string" },
                        BounceType: { type: "string" },
                        Date: { type: "Date" },
                        BouncedReason: { type: "string" }
                    }
                }
            }
        });
        grid.setDataSource(newdataSource);
        grid.dataSource.query({ page: 1, pageSize: parseInt(itemsPerPage) });
    };

    nsReport.bouncedEmailReportExport = function () {
        var ds = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    $.ajax({
                        url: '/Reports/GetNightlyStatusReport',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            ShowTop: 500000,
                            CustomStartDate: nsReport.CustomStartDate(),
                            CustomEndDate: nsReport.CustomEndDate(),
                            ReportId: nsReport.ReportId(),
                            ReportName: nsReport.ReportName(),
                            AccountIds: nsReport.AccountIds(),
                            SubscriptionID: nsReport.SubscriptionID(),
                            ReportType: nsReport.ReportType(),
                            PageNumber: 1

                        }),
                        type: 'post'
                    }).then(function (response) {
                        removepageloader();
                        var filter = $.Deferred();
                        if (response.success) {
                            filter.resolve(response);
                        } else {
                            filter.reject(response.error);
                        }
                        return filter.promise();
                    }).done(function (data) {
                        options.success(data.response.Data);

                    }).fail(function (error) {
                        notifyError(error);
                    });
                }
            },
            schema: {
                model: {
                    fields: {
                        Email: { type: "string" },
                        Account: { type: "string" },
                        BounceType: { type: "string" },
                        Date: { type: "Date" },
                        BouncedReason: { type: "string" }
                    }
                }
            }
        });

        var rows = [{
            cells: [
              { value: "Email Id" },
              { value: "Account Name" },
              { value: "Bounce Type" },
              { value: "Date" },
              { value: "Bounced Reason"}
            ]
        }];

        //using fetch, so we can process the data when the request is successfully completed
        ds.fetch(function () {
            var data = this.data();
            for (var i = 0; i < data.length; i++) {
                //push single row for every record
                rows.push({
                    cells: [
                      { value: data[i].Email },
                      { value: data[i].Account },
                      { value: data[i].BounceType },
                      { value: data[i].Date },
                      { value: data[i].BouncedReason }
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
                        { autoWidth: true }
                      ],
                      // Title of the sheet
                      //title: "Orders",
                      // Rows of the sheet
                      rows: rows
                  }
                ]
            });
            //save the file as Excel file with extension xlsx
            kendo.saveAs({
                dataURI: workbook.toDataURL(),
                fileName: "BouncedEmail_Report.xlsx",
                //for Exporting the file in IE9 and Safari Browsers
                proxyURL: "/Reports/ExportToExcel",
                forceProxy: true
            });
        });
    };

    console.log(runReport);
    if (runReport == 'True')
        nsReport.RunReport();
}
