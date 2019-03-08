var campaignListReport = function (data, url, dateFormat, runReport, itemsPerPage,accounturl) {
    var selfCampaign = this;

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
    selfCampaign.reputation = ko.observable(0);
    console.log("data account id");
    console.log(data.AccountId);

    selfCampaign.AccountId = ko.observable(data.AccountId);
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

        $.ajax({
            url: accounturl + 'GetReputationCount',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'accountId': selfCampaign.AccountId(), 'startDate': selfCampaign.CustomStartDate(),'endDate': selfCampaign.CustomEndDate()})
        }).then(function (response) {
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response);
            } else {
                filter.reject(response.error);
            }
            return filter.promise();
        }).done(function (data) {
            selfCampaign.reputation(data.response.ReputationCount);
        
        }).fail(function (error) {
            notifyError(error);
        });


        selfCampaign.errors.showAllMessages();

        if (selfCampaign.errors().length > 0) {
            notifyError("[|Please select valid Date Range.|]");
            return;
        }

        pageLoader();

        //var jsondata = ko.toJSON(selfCampaign);

        console.log("in run report");
        console.log(selfCampaign.AccountId());

        var grid = $('#Campaigngrid').data("kendoGrid");
        var newdataSource = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    $.ajax({
                        url: '/Reports/RunCampaignReputationReport',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            ShowTop: $('#Campaigngrid').data("kendoGrid").dataSource.pageSize(),
                            CustomStartDate: selfCampaign.CustomStartDate(),
                            CustomEndDate: selfCampaign.CustomEndDate(),
                            PageNumber: $('#Campaigngrid').data("kendoGrid").dataSource.page(),
                            ReportId: selfCampaign.ReportId(),
                            ReportName: selfCampaign.ReportName(),
                            PeriodId: selfCampaign.PeriodId(),
                            AccountId: selfCampaign.AccountId()
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
                                v.ComplaintRatePercent = cor[1];
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

                    }).fail(function(error) {
                        notifyError(error);
                    });
                }


            },
            schema: {
                data: "Data",
                total: "Total"
            },
            pageable: {
                pageSizes: [10, 25, 50, 100, 250, 500, 1000],
                messages: {
                    display: "[|Showing|] {0}-{1} [|from|] {2} [|Campaigns|]"
                }
            },
            serverPaging: true

        });
        grid.setDataSource(newdataSource);
        grid.dataSource.query({ page: 1, pageSize: itemsPerPage });

    };


    selfCampaign.TotalHits = ko.observable(0);
    selfCampaign.NoOfHits = ko.observable(0);

    if (runReport == 'True')
        selfCampaign.RunReportData();
}