var BDXCustomLeadReport = function (data, url, dateFormat, itemsPerPage) {
    selfBDXCustomLeadReport = this;

    ko.validation.configure({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null
    });
    selfBDXCustomLeadReport.Periods = ko.observableArray();
    selfBDXCustomLeadReport.ReportId = ko.observable(data.ReportId);
    selfBDXCustomLeadReport.ReportName = ko.observable(data.ReportName);
    selfBDXCustomLeadReport.SearchResults = ko.observableArray([]);
    selfBDXCustomLeadReport.gridvisible = ko.observable(false);
    selfBDXCustomLeadReport.PageNumber = ko.observable();
    selfBDXCustomLeadReport.periodID = ko.observable(2);
    selfBDXCustomLeadReport.Periods = ko.observableArray([
      { PeriodText: '[|Last 7 Days|]', Period: moment().subtract(7, 'days').format(), periodID: 1 },
       { PeriodText: '[|Last 30 Days|]', Period: moment().subtract(30, 'days').format(), periodID: 2 },
       { PeriodText: '[|Last 3 Months|]', Period: moment().subtract(90, 'days').format(), periodID: 3 },
       { PeriodText: '[|Month to Date|]', Period: moment().startOf('month').format(), periodID: 4 },
       { PeriodText: '[|Year to Date|]', Period: moment().startOf('year').format(), periodID: 5 },
       { PeriodText: '[|Last Year|]', Period: moment().subtract(1, 'years').format(), periodID: 6 },
       { PeriodText: '[|Custom|]', Period: moment(new Date()).format('DD/MM/YYYY'), periodID: 7 }
    ]);

    selfBDXCustomLeadReport.CustomEndDate = ko.observable(moment(new Date()).format()).extend({ required: { message: "" } });
    selfBDXCustomLeadReport.CustomStartDate = ko.observable(moment().subtract(30, 'days').format()).extend({ required: { message: "" } });

    selfBDXCustomLeadReport.periodID.subscribe(function (newValue) {

        if (newValue != 7) {
            selfBDXCustomLeadReport.CustomEndDate(moment(new Date()).format());
            var tempPreviousPeriod = ko.utils.arrayFirst(selfBDXCustomLeadReport.Periods(), function (type) {
                return type.periodID == newValue;
            });
            selfBDXCustomLeadReport.CustomStartDate(tempPreviousPeriod.Period);
        }
    });

    var maxdate = new Date();
    maxdate.setDate(maxdate.getDate() + 1);
    selfBDXCustomLeadReport.toMaxDate = ko.observable(maxdate);
    var fromMaxDate = new Date();
    fromMaxDate.setDate(fromMaxDate.getDate() - 1);
    selfBDXCustomLeadReport.fromMaxDate = ko.observable(fromMaxDate);

    selfBDXCustomLeadReport.DateFormat = ko.observable(dateFormat);

    selfBDXCustomLeadReport.fromDateChangeEvent = function () {
        var fromDate = this.value();
        selfBDXCustomLeadReport.CustomStartDate(moment(fromDate).format());
        var toDate = selfBDXCustomLeadReport.CustomEndDate();
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }
    selfBDXCustomLeadReport.periodChange = function () {
        selfBDXCustomLeadReport.toMaxDate(maxdate);
        var value = this.value();
       
        if (value == 7) {
            var toStartDate = new Date();
            toStartDate.setDate(toStartDate.getDate());
            selfBDXCustomLeadReport.CustomEndDate(toStartDate);
            var fromdate = new Date();
            fromdate.setDate(fromdate.getDate() - 7);
            selfBDXCustomLeadReport.CustomStartDate(fromdate);
            selfBDXCustomLeadReport.toMaxDate(new Date());
        }
    }
    selfBDXCustomLeadReport.toDateChangeEvent = function () {
        var fromDate = selfBDXCustomLeadReport.CustomStartDate();
        var toDate = this.value();
        selfBDXCustomLeadReport.CustomEndDate(moment(toDate).format("DD/MM/YYYY"));
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }

    selfBDXCustomLeadReport.CustomDateDisplay = ko.pureComputed(function () {
        if (selfBDXCustomLeadReport.periodID() == 7) {
            return true;
        } else {
            return false;
        }
    });

    selfBDXCustomLeadReport.TotalHits = ko.observable(0);
    selfBDXCustomLeadReport.errors = ko.validation.group(selfBDXCustomLeadReport, true);
    selfBDXCustomLeadReport.Runlist = function () {
        selfBDXCustomLeadReport.errors.showAllMessages();

        if (selfBDXCustomLeadReport.errors().length > 0) {
            notifyError("[|Please select valid Date Range.|]");
            return;
        }
        pageLoader();
        selfBDXCustomLeadReport.PageNumber(1);
        //var jsondata = ko.toJSON(selfBDXCustomLeadReport);
        //var authToken = readCookie("accessToken");

        var grid = $('#contactList').data("kendoGrid");
        var newdataSource = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    $.ajax({
                        url: '/Reports/BDXCustomLeadReport',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            ShowTop: $('#contactList').data("kendoGrid").dataSource.pageSize(),
                            CustomStartDate: selfBDXCustomLeadReport.CustomStartDate(),
                            CustomEndDate: selfBDXCustomLeadReport.CustomEndDate(),
                            PageNumber: $('#contactList').data("kendoGrid").dataSource.page(),
                            ReportId: selfBDXCustomLeadReport.ReportId(),
                            ReportName: selfBDXCustomLeadReport.ReportName(),
                            PeriodId: selfBDXCustomLeadReport.periodID()
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
                        selfBDXCustomLeadReport.gridvisible("True");
                        options.success(data.response);
                        if (data.response != null)
                        selfBDXCustomLeadReport.TotalHits(data.response.Total);

                    }).fail(function(error) {
                        notifyError(error);
                    });
                }
            },
            schema: {
                data: "Data",
                total: "Total",
                model: {
                    fields: {
                        LeadAdapterJobLogDetailID: { type: "number", defaultValue: 0 }
                    }
                }
            },
            pageable: {
                pageSizes: [10, 25, 50, 100, 250],
                pageSize: parseInt(itemsPerPage),
                messages: {
                    display: "[|Showing|] {0}-{1} [|from|] {2} [|Contact(s)|]"
                }
            }
        });
        grid.setDataSource(newdataSource);

   
        $(".k-grid table").wrap("<div class='cu-table-responsive bdx-report-grid'></div>");

        $('#contactList').data("kendoGrid").dataSource.query({ page: 1, pageSize: itemsPerPage });
    };

    selfBDXCustomLeadReport.ExcelExport = function(){
        var ds = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    $.ajax({
                        url: '/Reports/BDXCustomLeadReport',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            ShowTop: 10000,
                            CustomStartDate: selfBDXCustomLeadReport.CustomStartDate(),
                            CustomEndDate: selfBDXCustomLeadReport.CustomEndDate(),
                            PageNumber: 1,
                            ReportId: selfBDXCustomLeadReport.ReportId(),
                            ReportName: selfBDXCustomLeadReport.ReportName(),
                            PeriodId: selfBDXCustomLeadReport.periodID()
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
                        CreatedDate: { type: "string" },
                        LeadAdapterJobLogDetailID: { type: "string" },
                        ContactCreated: { type: "string" },
                        FullName: { type: "string" },
                        PrimaryEmail: { type: "string" },
                        FileName: { type: "string" },
                        LeadType: { type: "string" },
                        LeadSource: { type: "string" },
                        CommunityName: { type: "string" }
                    }
                }
            }
        });

        var rows = [{
            cells: [
              { value: "Date" },
              { value: "Lead Source" },
              { value: "Lead Type" },
              { value: "Contact Created" },
              { value: "Name" },
              { value: "Email" },
              { value: "Phone" },
              { value: "Street Address" },
              { value: "City" },

              { value: "State" },
              { value: "Postal Code" },
              { value: "Comments" },
              { value: "Market Name" },
              { value: "State Name" },
              { value: "Builder Name" },
              { value: "Builder Number" },
              { value: "Community Number" },
              { value: "Community Name" },
              { value: "Plan Number" },
              { value: "Plan Name" },
              { value: "File Name" },
              { value: "Lead Submission" }
              
            ]
        }];
        function displayDate(date) {
            if (date == null) {
                return "";
            }
            var utzdate = ConvertToDate(date).ToUtcUtzDate();;
          
            return kendo.toString(utzdate, dateFormat + " hh:mm");
        }
        //using fetch, so we can process the data when the request is successfully completed
        ds.fetch(function () {
            var data = this.data();
           
            for (var i = 0; i < data.length; i++) {
                //push single row for every record
                rows.push({
                    cells: [
                        { value: displayDate(data[i].CreatedDate) },
                        { value: data[i].LeadSource },
                        { value: data[i].LeadType },
                        { value: data[i].ContactCreated },
                        { value: displayName(data[i].FullName,data[i].PrimaryEmail)},
                        { value: data[i].PrimaryEmail },
                        { value: data[i].Phone },
                        { value: data[i].StreetAddress },
                        { value: data[i].City },

                        { value: data[i].State },
                        { value: data[i].PostalCode },
                        { value: data[i].Comments },
                        { value: data[i].MarketName },
                        { value: data[i].StateName },
                        { value: data[i].BuilderName },
                        { value: data[i].BuilderNumber },
                        { value: data[i].CommunityNumber },
                        { value: data[i].CommunityName },
                        { value: data[i].PlanNumber },
                        { value: data[i].PlanName },
                        { value:getFileName(data[i].FileName)},
                        { value: data[i].LeadAdapterJobLogDetailID }

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
                    //  title: "Orders",
                      // Rows of the sheet
                      rows: rows
                  }
                ]
            });
            //save the file as Excel file with extension xlsx
            kendo.saveAs({
                dataURI: workbook.toDataURL(),
                fileName: "BDXCustomLead.xlsx",
                //for Exporting the file in IE9 and Safari Browsers
                proxyURL: "/Reports/ExportToExcel",
                forceProxy: true
            });
        });

    }


}