var reEngagementReportViewModel = function (dateFormat, itemsPerPage, BASE_URL, reportId) {

    selfReport = this;

    selfReport.ReportId = ko.observable(reportId);

    selfReport.ReportName = ko.observable("Re-engagement Report");

    selfReport.ReportType = ko.observable(18);

    selfReport.LinkActions = ko.observableArray([]);

    selfReport.SelectedLinkActions = ko.observableArray([]);

    selfReport.ReEngagementInfo = ko.observableArray([]);

    selfReport.DateFormat = dateFormat;

    selfReport.HasSelectedLinks = ko.observable(false);

    selfReport.HasSelectedLinks.subscribe(function () {
        selfReport.getActionLinks();
    })

    selfReport.hasFetchedActionLinks = ko.observable(false);

    selfReport.dateFilters = {
        defaultRange: 1,
        customRange: 0
    };

    selfReport.Periods = ko.observableArray([
       { PeriodText: '[|Last 30 Days|]', PeriodId: 1 },
       { PeriodText: '[|Custom Date Range|]', PeriodId: 0 }
    ]);

    selfReport.PeriodId = ko.observable(selfReport.dateFilters.defaultRange);
    selfReport.PeriodId.subscribe(function () {
        selfReport.IsDefaultDateRange(selfReport.PeriodId() == 0 ? false : true);
    })

    selfReport.IsDefaultDateRange = ko.observable(true);

    selfReport.StartDate = ko.observable().extend({ required: { message: "[|Start date is required|]" } });
    selfReport.StartDate.subscribe(function () {
        selfReport.StartDate(moment(selfReport.StartDate()).format());
    })

    selfReport.EndDate = ko.observable();

    selfReport.EndDate.subscribe(function () {
        selfReport.EndDate(moment(selfReport.EndDate()).format());
    })

    selfReport.CampaignId = ko.observable();

    selfReport.RunReport = function () {
        selfReport.ReEngagementInfo([]);
        uniqueColumns = ["Name", "CurrentMonthClicks", "LastMonthClicks", "Last12MonthsClicks", "TotalClicks"];
        pageLoader();

        if ($('#reengagementInfoGrid').data().kendoGrid != undefined) {
            $("#reengagementInfoGrid").data('kendoGrid').dataSource.data([]);
        }

        if (selfReport.HasSelectedLinks() == true && selfReport.SelectedLinkActions().length == 0) {
            notifyError('[|Please select at least on link|]');
            removepageloader();
            return;
        }
        if (selfReport.IsDefaultDateRange() == false) {
            if (!selfReport.StartDate() || selfReport.StartDate() == "Invalid date" || !selfReport.EndDate() || selfReport.EndDate() == "Invalid date") {
                notifyError("[|Start date and End date are required and should be in |]" + dateformat + '[| format|]');
                removepageloader();
                return;
            }
            else if (selfReport.StartDate() > selfReport.EndDate()) {
                notifyError("[|End date should be greater than Start date|]");
                removepageloader();
                return;
            }
        }
        var dataSource = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    $.ajax(
                        {
                            url: "/getreengagementinfo",
                            contentType: "application/json; charset=utf-8",
                            dataType: "json",
                            type: 'get',
                            traditional: true,
                            data: {
                                'StartDate': kendo.toString(selfReport.StartDate(), dateFormat),
                                'EndDate': kendo.toString(selfReport.EndDate(), dateFormat),
                                'IsDefaultDateRange': selfReport.IsDefaultDateRange(),
                                'LinkIds': selfReport.HasSelectedLinks() ? selfReport.SelectedLinkActions() : "",
                                'ReportId': selfReport.ReportId()
                            }
                        }).then(function (response) {
                            var filter = $.Deferred()
                            if (response.success == true) {
                                filter.resolve(response)
                            }
                            else {
                                filter.reject(response.error)
                            }
                            return filter.promise()
                        }).done(function (data) {
                            if (data.response && data.response.CampaignStats)
                                selfReport.ReEngagementInfo(data.response.CampaignStats);
                            options.success(data.response.CampaignStats);
                            removepageloader();
                        }).fail(function (error) {
                            notifyError(error);
                            removepageloader();
                        });

                }
            },
            pageSize: parseInt(itemsPerPage)
        });
        if ($('#reengagementInfoGrid').data().kendoGrid != undefined) {
            $('#reengagementInfoGrid').data().kendoGrid.destroy();
            if ($('#reengagementInfoGrid').data().kendoGrid != undefined) {
                $('#reengagementInfoGrid').data().kendoGrid.destroy();
                $(".k-grid-pager").remove();
                $(".k-grid-toolbar").remove();
                $('#reengagementInfoGrid').empty();
            }
        }
        $("#reengagementInfoGrid").kendoGrid({
            dataSource: dataSource,
            sortable: false,
            columns: getColumnNames(),
            scrollable: false,
            autoBind: true,
            pageable: {
                pageSizes: true,
                messages: {
                    display: "[|Showing|] {0}-{1} [|from|] {2} Campaigns"
                },
            },
            toolbar: [{ name: "excel", text: "Export to Excel" }],
            excelExport: function (e) {
                var sheet = e.workbook.sheets[0];
                for (var i = 1; i < sheet.rows.length; i++) {
                    for (var ci = 1; ci < sheet.rows[i].cells.length; ci++) {
                        sheet.rows[i].cells[ci].value = (new DOMParser).parseFromString(getDisplayValue(i - 1, ci, selfReport.IsDefaultDateRange()), "text/html").documentElement.textContent;
                    }
                }
            },
            excel: {
                fileName: "Re-engagement Report.xlsx",
                allPages: true,
                //for Exporting the file in IE9 and Safari Browsers
                proxyURL: "/Reports/ExportToExcel",
                forceProxy: true
            },

            dataBound: function (e) {
                onDataBound(e);
            },
        });
        $("#reengagementInfoGrid").wrap("<div class='cu-table-responsive bdx-report-grid'></div>");
        var grid = $("#reengagementInfoGrid").data("kendoGrid");
        grid.setOptions({
            sortable: true
        });
        $(".k-grid-excel").addClass("cu-grid-excel");

    }

    var getDisplayValue = function (index, statIndex, isDefaultView) {
        if (selfReport.ReEngagementInfo().length > 0) {
            var propertyName = '';
            switch (statIndex) {
                case 1:
                    propertyName = isDefaultView == true ? 'CurrentMonthClicks' : 'TotalClicks';
                    break;
                case 2:
                    propertyName = 'LastMonthClicks';
                    break;
                case 3:
                    propertyName = 'Last12MonthsClicks';
                    break;
                default:
                    propertyName = 'Name';
            }

            var campaignId = selfReport.ReEngagementInfo()[index]['CampaignID'];
            var clicksValue = selfReport.ReEngagementInfo()[index][propertyName] > 0 ? '<a href="/CampaignStatistics?campaignId=' + campaignId + '" title="Clicks" >'
                + selfReport.ReEngagementInfo()[index][propertyName] + '<span class="icon stc-one-finger-click-black" style="color:#ccc"></span></a> ' : selfReport.ReEngagementInfo()[index][propertyName];
            var countValue = selfReport.ReEngagementInfo()[index][propertyName] > 0 ?
                '<a href="javascript:void(0)" cellid="' + statIndex + '" rowid="' + campaignId + '" title="Contacts" onclick="viewContacts(this)">' + selfReport.ReEngagementInfo()[index][propertyName.replace('Clicks', 'Contacts')] + ' </a> '
                : selfReport.ReEngagementInfo()[index][propertyName.replace('Clicks', 'Contacts')] + '<span class="icon icon-stc-photo" style="color:#ccc"></span>';
            var percentageValue = selfReport.ReEngagementInfo()[index][propertyName] > 0 ?
                '<a href="/CampaignStatistics?campaignId=' + selfReport.ReEngagementInfo()[index]['CampaignID'] + '" >' + (selfReport.ReEngagementInfo()[index][propertyName.replace('Clicks', 'Contacts')] / +selfReport.ReEngagementInfo()[index][propertyName] * 100).toFixed(1) + '%' + '</a>'
                : '0.0%';
            var displayValue = clicksValue + ' - ' + countValue + ' - ' + percentageValue;
            return displayValue
        }
        else
            return "";
    }

    var getDisplayScript = function (propertyName) {
        var clicksValue = "#if (propertyName > 0){}"
        selfReport.ReEngagementInfo()[index][propertyName] > 0 ? '<a href="/CampaignStatistics?campaignId=' + campaignId + '"  title="Clicks">'
            + selfReport.ReEngagementInfo()[index][propertyName] + '<span class="icon stc-one-finger-click-black" style="color:#ccc"></span></a> '
    : selfReport.ReEngagementInfo()[index][propertyName] + '<span class="icon stc-one-finger-click-black" style="color:#ccc"></span>';
        var countValue = selfReport.ReEngagementInfo()[index][propertyName] > 0 ?
            '<a href="javascript:void(0)"  title="Contacts" cellid="' + statIndex + '" rowid="' + campaignId + '" onclick="viewContacts(this)">' + selfReport.ReEngagementInfo()[index][propertyName.replace('Clicks', 'Contacts')] + '<span class="icon icon-stc-photo" style="color:#ccc"></span></a> '
            : selfReport.ReEngagementInfo()[index][propertyName.replace('Clicks', 'Contacts')] + '<span class="icon icon-stc-photo" style="color:#ccc"></span>';
        var percentageValue = selfReport.ReEngagementInfo()[index][propertyName] > 0 ?
            '<a href="/CampaignStatistics?campaignId=' + selfReport.ReEngagementInfo()[index]['CampaignID'] + '" >' + (selfReport.ReEngagementInfo()[index][propertyName.replace('Clicks', 'Contacts')] / +selfReport.ReEngagementInfo()[index][propertyName] * 100).toFixed(1) + '%' + '</a>'
            : '0.0%';
        var displayValue = clicksValue + ' - ' + countValue + ' - ' + percentageValue;

        return clicksValue;
    }

    var getColumnNames = function () {
        columnDisplayNames = ['Campaign Name', 'Clicks in Current Month', 'Clicks in Previous Month', 'Clicks in Previous 12 Months', 'Total Clicks'];
        gridColumns = [];
        columns = [];
        var nameColumn = {
            field: 'Name',
            title: 'Campaign Name',
            width: '100px',
            template: '<a href="/CampaignStatistics?campaignId=#:CampaignID# ">#:Name#</a>'
        }
        if (selfReport.IsDefaultDateRange() == false) {
            gridColumns = ["Name", "TotalClicks"];
            columnDisplayNames = ['Campaign Name', 'Total Clicks'];
            var totalColumn = {
                field: 'TotalClicks',
                title: 'Total Clicks',
                width: '100px',
                headerAttributes: {
                    "class": "text-center"
                },
                attributes: {
                    "class": "text-center"
                },
                template: '#if(TotalClicks==0){#<span>--</span>#} else{#' + '<a  title="Clicks" href="/CampaignStatistics?campaignId=#:CampaignID#"> #:TotalClicks# </span></a>' +
                        ' - <a href="javascript:void(0)"  title="Contacts" cellid="4" rowid="#:CampaignID#" onclick="viewContacts(this)"> #:TotalContacts# </a>' +
                        ' - <a href="/CampaignStatistics?campaignId=#:CampaignID#"> #:(TotalContacts/TotalClicks*100).toFixed(1)#% </a>#}#'
            }
            columns.push(nameColumn, totalColumn);
        }
        else {
            gridColumns = ["Name", "CurrentMonthClicks", "LastMonthClicks", "Last12MonthsClicks"];
            columnDisplayNames = ['Campaign Name', 'Clicks in Current Month', 'Clicks in Previous Month', 'Clicks in Previous 12 Months'];
            var currentMonthColumn = {
                field: 'CurrentMonthClicks',
                title: 'Clicks in Current Month',
                width: '100px',
                headerAttributes: {
                    "class": "text-center"
                },
                attributes: {
                    "class": "text-center"
                },
                template: '#if(CurrentMonthContacts==0){#<span>--</span>#} else{#' +
                    '<a href="/CampaignStatistics?campaignId=#:CampaignID#"  title="Clicks"> #:CurrentMonthClicks# </a>' +
                        ' - <a href="javascript:void(0)" cellid="1" rowid="#:CampaignID#"  title="Contacts" onclick="viewContacts(this)"> #:CurrentMonthContacts# </a>' +
                        ' - <a href="/CampaignStatistics?campaignId=#:CampaignID#"> #:(CurrentMonthContacts/CurrentMonthClicks*100).toFixed(1)#% </a>#}#'

            }
            var lastMonthColumn = {
                field: 'LastMonthClicks',
                title: 'Clicks in Previous Month',
                width: '100px',
                headerAttributes: {
                    "class": "text-center"
                },
                attributes: {
                    "class": "text-center "
                },
                template: '#if(LastMonthContacts==0){#<span>--</span>#} else{#' +
                    '<a href="/CampaignStatistics?campaignId=#:CampaignID#" title="Clicks" > #:LastMonthClicks# </a>' +
                        ' - <a href="javascript:void(0)" cellid="2" rowid="#:CampaignID#" title="Contacts" onclick="viewContacts(this)"> #:LastMonthContacts# </a>' +
                        ' - <a href="/CampaignStatistics?campaignId=#:CampaignID#"> #:(LastMonthContacts/LastMonthClicks*100).toFixed(1)#% </a>#}#'

            }
            var last12MonthsColumn = {
                field: 'Last12MonthsClicks',
                title: 'Clicks in Previous 12 Months',
                width: '100px',
                headerAttributes: {
                    "class": "text-center"
                },
                attributes: {
                    "class": "text-center "
                },
                template: '#if(Last12MonthsClicks==0){#<span>--</span>#} else{#' +
  '<a href="/CampaignStatistics?campaignId=#:CampaignID#" title="Clicks" > #:Last12MonthsClicks# </a>' +
        ' - <a href="javascript:void(0)" cellid="3" rowid="#:CampaignID#" title="Contacts"  onclick="viewContacts(this)"> #:Last12MonthsContacts# <span class="icon icon-icon-stc-photo"></span></a>' +
        ' - <a href="/CampaignStatistics?campaignId=#:CampaignID#"> #:(Last12MonthsContacts/Last12MonthsClicks*100).toFixed(1)#% </a>#}#'

            }
            columns.push(nameColumn, currentMonthColumn, lastMonthColumn, last12MonthsColumn);
        }
        return columns;
    }

    selfReport.RunReport();

    selfReport.getActionLinks = function () {
        if (!selfReport.hasFetchedActionLinks()) {
            $.ajax({
                url: '/getactionlinks',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                type: 'get'
            }).then(function (response) {
                var filter = $.Deferred()
                if (response.success == true) {
                    filter.resolve(response)
                }
                else {
                    filter.reject(response.error)
                }
                return filter.promise()
            }).done(function (data) {
                selfReport.LinkActions(data.response);
                selfReport.hasFetchedActionLinks(true);
            }).fail(function (error) {
                notifyError(error);
            })
        }
    }

    selfReport.DrillDownPeriod = ko.observable();

    viewContacts = function (item) {
        var campaignId = $(item)[0].getAttribute('rowid');
        var statIndex = $(item)[0].getAttribute('cellid');
        selfReport.CampaignId(campaignId);
        selfReport.DrillDownPeriod(selfReport.IsDefaultDateRange() == 0 ? 4 : statIndex);
        if (statIndex == 0)
            window.location.href = "/CampaignStatistics?campaignId=" + thisCampaign.CampaignID;
        else {
            var jsondata = ko.toJSON(selfReport);
            $.ajax({
                url: BASE_URL + '/GetReEngagedContacts',
                type: 'post',
                data: jsondata,
                dataType: 'json',
                contentType: "application/json; charset=utf-8"
            }).then(function (response) {
                var filter = $.Deferred();
                if (response.success) {
                    filter.resolve(response)
                } else {
                    filter.reject(response.error)
                }
                removepageloader();
                return filter.promise()
            }).done(function (response) {
                if (response != null) {
                    if (response.success == true) {
                        localStorage.setItem("ContactsGuid", response.response);
                        window.location.href = '../reportcontacts?guid=' + response.response + '&reportType=' + selfReport.ReportType() + '&reportId=' + selfReport.ReportId();
                    }
                }
            }).fail(function (error) {
                notifyError(error);
            });
        }
    }

    function onDataBound(e) {
        var colCount = $(".k-grid").find('table colgroup > col').length;
        if (e.sender.dataSource.view().length == 0) {
            e.sender.table.find('tbody').append('<tr><td colspan="' + colCount + '"><div class="notecordsfound"><div><i class="icon st-icon-browser-windows-2"></i></div><span class="bolder smaller-90">[|No records found|]</span></div></td></tr>')
        }

    }
}