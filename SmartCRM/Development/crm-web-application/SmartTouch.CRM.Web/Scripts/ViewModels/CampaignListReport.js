var campaignListReport = function (data, url, dateFormat, runReport, itemsPerPage) {
     selfCampaign = this;

    ko.validation.configure({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null
    });

    selfCampaign.dateRange = ko.observableArray();
    selfCampaign.Period = ko.observable();
    selfCampaign.ShowTop = ko.observable(10);
    selfCampaign.SearchResults = ko.observableArray([]);
    selfCampaign.gridvisible = ko.observable(false);
    selfCampaign.PageNumber = ko.observable(1);
    selfCampaign.ReportId = ko.observable(data.ReportId);
    selfCampaign.ReportName = ko.observable(data.ReportName);
    selfCampaign.LastRunOn = ko.observable(moment(new Date()).format());
    selfCampaign.PeriodId = ko.observable(2);
    selfCampaign.dateRange = ko.observableArray([
      { PeriodText: '[|Last 7 Days|]', Period: moment().subtract(7, 'days').format(), PeriodId: 1 },
       { PeriodText: '[|Last 30 Days|]', Period: moment().subtract(30, 'days').format(), PeriodId: 2 },
       { PeriodText: '[|Last 3 Months|]', Period: moment().subtract(90, 'days').format(), PeriodId: 3 },
       { PeriodText: '[|Month to Date|]', Period: moment().startOf('month').format(), PeriodId: 4 },
       { PeriodText: '[|Year to Date|]', Period: moment().startOf('year').format(), PeriodId: 5 },
       { PeriodText: '[|Last Year|]', Period: moment().subtract(1, 'years').format(), PeriodId: 6 },
       { PeriodText: '[|Custom|]', Period: moment(new Date()).format('DD/MM/YYYY'), PeriodId: 7 }
    ]);

    selfCampaign.CustomEndDate = ko.observable(moment(new Date()).format()).extend({ required: { message: "" } });
    selfCampaign.CustomStartDate = ko.observable(moment().subtract(30, 'days').format()).extend({ required: { message: "" } });
   
    selfCampaign.PeriodId.subscribe(function (newValue) { 
        if (selfCampaign.PeriodId() != 7) {
            selfCampaign.CustomEndDate(moment(new Date()).format());
            if (newValue != 7) {
                var tempPreviousPeriod = ko.utils.arrayFirst(selfCampaign.dateRange(), function (type) {
                    return type.PeriodId == newValue;
                });
                selfCampaign.CustomStartDate(tempPreviousPeriod.Period);
            }
        }
    });

    selfCampaign.selectionTopRange = ko.observableArray([
       { Name: '[|Top 5|]', Id: 5 },
       { Name: '[|Top 10|]', Id: 10 },
       { Name: '[|Top 25|]', Id: 25 }
    ]);

    var maxdate = new Date();
    maxdate.setDate(maxdate.getDate() + 1);
    selfCampaign.toMaxDate = ko.observable(maxdate);
    var fromMaxDate = new Date();
    fromMaxDate.setDate(fromMaxDate.getDate() - 1);
    selfCampaign.fromMaxDate = ko.observable(fromMaxDate);

    selfCampaign.DateFormat = ko.observable(dateFormat);

    selfCampaign.fromDateChangeEvent = function () {
        var fromDate = this.value();
        selfCampaign.CustomStartDate(moment(fromDate).format("DD/MM/YYYY"));
        var toDate = selfCampaign.CustomEndDate();
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }
    selfCampaign.periodChange = function () {
        var value = this.value();       
        selfCampaign.toMaxDate(maxdate);
        if (parseInt(value) == 7) {           
            var toStartDate = new Date();
            toStartDate.setDate(toStartDate.getDate());
            selfCampaign.CustomEndDate(toStartDate);
            var fromdate = new Date();
            fromdate.setDate(fromdate.getDate() - 7);
            selfCampaign.CustomStartDate(fromdate);
            selfCampaign.toMaxDate(new Date());
        }
    }
    selfCampaign.toDateChangeEvent = function () {
        var fromDate = selfCampaign.CustomStartDate();
        var toDate = this.value();
        selfCampaign.CustomEndDate(moment(toDate).format("DD/MM/YYYY"));
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }

    selfCampaign.CustomDateDisplay = ko.pureComputed(function () {
        if (parseInt(selfCampaign.PeriodId()) == 7) {
            return true;
        } else {
            return false;
        }
    });

    selfCampaign.errors = ko.validation.group(selfCampaign, true);
    selfCampaign.RunReportData = function () {
        selfCampaign.errors.showAllMessages();

        if (selfCampaign.errors().length > 0) {
            notifyError("[|Please select valid Date Range.|]");
            return;
        }
        pageLoader();

        var grid = $('#Campaigngrid').data("kendoGrid");
        var newdataSource = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    $.ajax({
                        url: '/Reports/RunCampaignReport',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            CustomStartDate: selfCampaign.CustomStartDate(),
                            CustomEndDate: selfCampaign.CustomEndDate(),
                            ReportId: selfCampaign.ReportId(),
                            ReportName: selfCampaign.ReportName(), PeriodId: selfCampaign.PeriodId()
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
                        selfCampaign.gridvisible("True");
                        if (data.response != null) {
                            $.each(data.response.Data, function (i, v) {
                                var cr = v.ClickRate.split("|");
                                var or = v.OpenRate.split("|");
                                var cor = v.ComplaintRate.split("|");
                                v.ClickRate = parseInt(cr[0]);
                                v.ClickRatePercent = cr[1];
                                v.OpenRate = parseInt(or[0]);
                                v.OpenRatePercent = or[1];
                                v.ComplaintRate = parseInt(cor[0]);
                                v.CompliantRatePercent = cor[1];
                                if (v.ProcessedDate) {
                                    var prosDate = v.ProcessedDate.match(/\d+/)[0] * 1;
                                    var date = kendo.toString(new Date(prosDate), dateFormat);
                                    v.ProcessedDate = date != null ? date : "";
                                }
                                else
                                    v.ProcessedDate = '';
                            });
                        }
                        options.success(data.response);

                    }).fail(function (error) {
                        notifyError(error);
                    })
                },
            },
            schema: {
                data: "Data",
                total: "Total",
                model: {
                    fields: {
                        OpenRate: { type: "number" },
                        ClickRate: { type: "number" },
                        ComplaintRate: { type: "number" },
                        Name: { type: "string"},
                        Subject: { type: "string" },
                        ProcessedDate: { type: "string" },
                        ProviderName: { type: "string" },
                        JobId: { type: "string" },
                        TotalSends: {type: "number"},
                        WorkflowsCount: {type: "number"}
                    }
                }
            },
            pageable: {
                pageSizes: [10, 25, 50, 100, 250, 500],
                pageSize: parseInt(itemsPerPage),
                messages: {
                    display: "[|Showing|] {0}-{1} [|from|] {2} [|Campaigns|]"
                }
            }
        });
        grid.setDataSource(newdataSource);
        grid.dataSource.query({ page: 1, pageSize: itemsPerPage });
    };
    
    selfCampaign.ExcelExport = function () {
        var ds = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    $.ajax({
                        url: '/Reports/RunCampaignReport',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            ShowTop: 10000,
                            CustomStartDate: selfCampaign.CustomStartDate(),
                            CustomEndDate: selfCampaign.CustomEndDate(),
                            PageNumber: 1,
                            ReportId: selfCampaign.ReportId(),
                            ReportName: selfCampaign.ReportName(),
                            PeriodId: selfCampaign.PeriodId()

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
                        options.success(data.response.Data);

                    }).fail(function(error) {
                        notifyError(error);
                    });
                }
            },
            schema: {
                model: {
                    fields: {
                        Name: { type: "string" },
                        Subject: { type: "string" },
                        TotalSends: { type: "string" },
                        OpenRate: { type: "string" },
                        WorkflowsCount: { type: "number" },
                        ClickRate: { type: "string" },
                        ComplaintRate: { type: "string" },
                        ProcessedDate: { type: "datetime" },
                        ProviderName: { type: "string" },
                        JobId: { type: "string" }
                    }
                }
            }
        });

        var rows = [{
            cells: [
              { value: "Campaign Name" },
              { value: "Campaign Email Subject" },
              { value: "Sent Count" },
              { value: "Open Rate" },
              { value: "Workflows"},
              { value: "Click Rate" },
              { value: "Complaint Rate" },
              { value: "Date Sent" },
              { value: "VMTA" },
              { value: "JobID" }
            ]
        }];

        //using fetch, so we can process the data when the request is successfully completed
        ds.fetch(function () {
            var data = this.data();
            for (var i = 0; i < data.length; i++) {
                //push single row for every record
                if (data[i].ProcessedDate) {
                    var prosDate = data[i].ProcessedDate.match(/\d+/)[0] * 1;
                    data[i].ProcessedDate = kendo.toString(new Date(prosDate), dateFormat)
                }
                rows.push({
                    cells: [
                      { value: data[i].Name },
                      { value: data[i].Subject },
                      { value: data[i].TotalSends },
                      { value: data[i].OpenRate },
                      { value: data[i].WorkflowsCount },
                      { value: data[i].ClickRate },
                      { value: data[i].ComplaintRate },
                      { value: data[i].ProcessedDate },
                      { value: data[i].ProviderName },
                      { value: data[i].JobId }
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
                fileName: "CampaignReport.xlsx",
                //for Exporting the file in IE9 and Safari Browsers
                proxyURL: "/Reports/ExportToExcel",
                forceProxy: true
            });
        });

    }


    selfCampaign.TotalHits = ko.observable(0);
    selfCampaign.NoOfHits = ko.observable(0);

    if (runReport == 'True')
        selfCampaign.RunReportData();
}