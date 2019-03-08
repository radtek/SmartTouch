var bdxContactsViewModel = function (data, url, dateFormat, itemsPerPage) {
    var selfBDXReport = this;
   
    ko.validation.configure({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null
    });
    selfBDXReport.Periods = ko.observableArray();
    selfBDXReport.ReportId = ko.observable(data.ReportId);
    selfBDXReport.ReportName = ko.observable(data.ReportName);
    selfBDXReport.SearchResults = ko.observableArray([]);
    selfBDXReport.gridvisible = ko.observable(false);
    selfBDXReport.PageNumber = ko.observable();
    selfBDXReport.periodID = ko.observable(2);
    selfBDXReport.Periods = ko.observableArray([
      { PeriodText: '[|Last 7 Days|]', Period: moment().subtract(7, 'days').format(), periodID: 1 },
       { PeriodText: '[|Last 30 Days|]', Period: moment().subtract(30, 'days').format(), periodID: 2 },
       { PeriodText: '[|Last 3 Months|]', Period: moment().subtract(90, 'days').format(), periodID: 3 },
       { PeriodText: '[|Month to Date|]', Period: moment().startOf('month').format(), periodID: 4 },
       { PeriodText: '[|Year to Date|]', Period: moment().startOf('year').format(), periodID: 5 },
       { PeriodText: '[|Last Year|]', Period: moment().subtract(1, 'years').format(), periodID: 6 },
       { PeriodText: '[|Custom|]', Period: moment(new Date()).format('DD/MM/YYYY'), periodID: 7 }
    ]);

    selfBDXReport.CustomEndDate = ko.observable(moment(new Date()).format()).extend({ required: { message: "" } });
    selfBDXReport.CustomStartDate = ko.observable(moment().subtract(30, 'days').format()).extend({ required: { message: "" } });

    selfBDXReport.periodID.subscribe(function (newValue) {
       
        if (newValue != 7) {
            selfBDXReport.CustomEndDate(moment(new Date()).format());
            var tempPreviousPeriod = ko.utils.arrayFirst(selfBDXReport.Periods(), function (type) {
                return type.periodID == newValue;
            });
            selfBDXReport.CustomStartDate(tempPreviousPeriod.Period);
        }
    });

    var maxdate = new Date();
    maxdate.setDate(maxdate.getDate() + 1);
    selfBDXReport.toMaxDate = ko.observable(maxdate);
    var fromMaxDate = new Date();
    fromMaxDate.setDate(fromMaxDate.getDate() - 1);
    selfBDXReport.fromMaxDate = ko.observable(fromMaxDate);

    selfBDXReport.DateFormat = ko.observable(dateFormat);

    selfBDXReport.fromDateChangeEvent = function () {
        var fromDate = this.value();
        selfBDXReport.CustomStartDate(moment(fromDate).format());
        var toDate = selfBDXReport.CustomEndDate();
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }
    selfBDXReport.periodChange = function () {
        selfBDXReport.toMaxDate(maxdate);
        var value = this.value();
        console.log(value);
        if (value == 7) {
            var toStartDate = new Date();
            toStartDate.setDate(toStartDate.getDate());
            selfBDXReport.CustomEndDate(toStartDate);
            var fromdate = new Date();
            fromdate.setDate(fromdate.getDate() - 7);
            selfBDXReport.CustomStartDate(fromdate);
            selfBDXReport.toMaxDate(new Date());
        }
    }
    selfBDXReport.toDateChangeEvent = function () {
        var fromDate = selfBDXReport.CustomStartDate();
        var toDate = this.value();
        selfBDXReport.CustomEndDate(moment(toDate).format("DD/MM/YYYY"));
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }

    selfBDXReport.CustomDateDisplay = ko.pureComputed(function () {
        if (selfBDXReport.periodID() == 7) {
            return true;
        } else {
            return false;
        }
    });


    selfBDXReport.errors = ko.validation.group(selfBDXReport, true);
    selfBDXReport.Runlist = function () {       
        selfBDXReport.errors.showAllMessages();

        if (selfBDXReport.errors().length > 0) {
            notifyError("[|Please select valid Date Range.|]");
            return;
        }
        pageLoader();
        selfBDXReport.PageNumber(1);
        //var jsondata = ko.toJSON(selfBDXReport);
        //var authToken = readCookie("accessToken");

        var grid = $('#contactList').data("kendoGrid");
        var newdataSource = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    $.ajax({
                        url: '/Reports/BDXFreemiumCustomLeadReport',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            ShowTop: $('#contactList').data("kendoGrid").dataSource.pageSize(),
                            CustomStartDate: selfBDXReport.CustomStartDate(),
                            CustomEndDate: selfBDXReport.CustomEndDate(),
                            PageNumber: $('#contactList').data("kendoGrid").dataSource.page(),
                            ReportId: selfBDXReport.ReportId(),
                            ReportName: selfBDXReport.ReportName(),
                            PeriodId: selfBDXReport.periodID()
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
                        selfBDXReport.gridvisible("True");
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
            serverPaging: true,
            pageable: {
                pageSizes: [10, 25, 50, 100, 250],
                messages: {
                    display: "[|Showing|] {0}-{1} [|from|] {2} [|Contact(s)|]"
                },
            },
        });
        grid.setDataSource(newdataSource);
        $('#contactList').data("kendoGrid").dataSource.query({ page: 1, pageSize: itemsPerPage });
    };

   


}