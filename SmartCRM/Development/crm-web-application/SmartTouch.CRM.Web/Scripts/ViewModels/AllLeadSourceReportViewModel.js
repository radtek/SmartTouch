var allLeadSourceReport = function (url, data, dateFormat, runReport, itemsPerPage) {
    allLSReport = this;
    allLSReport.ReportType = ko.observable(17);
    allLSReport.ReportId = ko.observable(data.ReportId);
    allLSReport.Periods = ko.observableArray([
      { PeriodId: 1, Period: '[|Last 7 Days|]' },
      { PeriodId: 2, Period: '[|Last 30 Days|]' },
      { PeriodId: 3, Period: '[|Last 3 Months|]' },
      { PeriodId: 4, Period: '[|Month to Date|]' },
      { PeriodId: 5, Period: '[|Year to Date|]' },
      { PeriodId: 6, Period: '[|Last Year|]' },
      { PeriodId: 7, Period: '[|Custom|]' }
    ]);
    allLSReport.PeriodId = ko.observable(2);
    var toStartDate = new Date();
    toStartDate.setDate(toStartDate.getDate());
    allLSReport.CustomEndDate = ko.observable(kendo.toString(toStartDate, dateFormat)).extend({ required: { message: "" } });
    allLSReport.CustomEndDatePrev = ko.observable();

    var fromdate = new Date();
    fromdate.setMonth(fromdate.getMonth() - 1);
    allLSReport.CustomStartDate = ko.observable(kendo.toString(fromdate, dateFormat)).extend({ required: { message: "" } });
    allLSReport.CustomStartDatePrev = ko.observable();

    allLSReport.CustomEndDate(moment(new Date()).format());
    allLSReport.CustomStartDate(moment().subtract(30, 'days').format());
    var maxdate = new Date();
    maxdate.setDate(maxdate.getDate() + 1);
    allLSReport.toMaxDate = ko.observable(maxdate);
    var fromMaxDate = new Date();
    fromMaxDate.setDate(fromMaxDate.getDate() - 1);
    allLSReport.fromMaxDate = ko.observable(fromMaxDate);
    allLSReport.DateFormat = ko.observable(dateFormat);
    allLSReport.DateFormat = ko.observable(dateFormat);
    allLSReport.DateRange = ko.observable('D');

    allLSReport.periodChange = function () {
        var value = this.value();
        allLSReport.toMaxDate(maxdate);
        if (value == "1")  // last 7 days
            allLSReport.CustomStartDate(moment().subtract(7, 'days').format());
        else if (value == "2")  // last 30 days
            allLSReport.CustomStartDate(moment().subtract(30, 'days').format());
        else if (value == "3")  //last 3 months
            allLSReport.CustomStartDate(moment().subtract(3, 'months').format());
        else if (value == "4")  //month to date
            allLSReport.CustomStartDate(moment().startOf('month').format());
        else if (value == "5")   // month to year
            allLSReport.CustomStartDate(moment().startOf('year').format());
        else if (value == "6") { //last year
            allLSReport.CustomStartDate(moment().subtract(1, 'years').format());
        }
        if (value == "7") { // custom
            var toStartDate = new Date();
            toStartDate.setDate(toStartDate.getDate());
            allLSReport.CustomEndDate(toStartDate);
            var fromdate = new Date();
            fromdate.setDate(fromdate.getDate() - 7);
            allLSReport.CustomStartDate(fromdate);
            allLSReport.toMaxDate(new Date());
        }
    }

    allLSReport.CustomDateDisplay = ko.pureComputed(function () {
        if (allLSReport.PeriodId() == 7)
            return true;
        else
            return false;
    });

    allLSReport.fromDateChangeEvent = function () {
        var fromDate = this.value();
        allLSReport.CustomStartDate(kendo.toString(fromDate, dateFormat));
        var toDate = allLSReport.CustomEndDate();
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }

    allLSReport.toDateChangeEvent = function () {
        var fromDate = allLSReport.CustomStartDate();
        var toDate = this.value();
        allLSReport.CustomEndDate(kendo.toString(toDate, dateFormat));
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }

    allLSReport.CommunityIds = ko.observableArray([]);
    allLSReport.LeadSourceIds = ko.observableArray([]);
    allLSReport.LifeStageIds = ko.observableArray([]);

    allLSReport.Communities = ko.observableArray(data.Communities);
    allLSReport.LeadSources = ko.observableArray(data.LeadSources);
    allLSReport.LifecycleStages = ko.observableArray(data.LifecycleStages);

    allLSReport.lifecyclesVisible = ko.observable("False");
    allLSReport.communityVisible = ko.observable("False");
    allLSReport.LeadSourceVisible = ko.observable("True");

    allLSReport.LifecycleStage = ko.observable(0);
    allLSReport.Community = ko.observable(0);
    allLSReport.LeadSource = ko.observable(0);
    allLSReport.CountInAreaChart = ko.observable(0);

    allLSReport.chartVisible = ko.observable("False");
    allLSReport.Groups = ko.observableArray([
           { GroupId: 1, Group: '[|Lead Sources|]' },
           { GroupId: 2, Group: '[|Lifecycle Stages|]' },
           { GroupId: 3, Group: '[|Community|]' }

    ]);
    allLSReport.GroupId = ko.observable(1);
    allLSReport.groupChange = function () {
        var value = this.value();
        if (value == "1") {
            allLSReport.LeadSourceVisible("True");
            allLSReport.lifecyclesVisible("False");
            allLSReport.communityVisible("False");
            allLSReport.CommunityIds([]);
            allLSReport.LifeStageIds([]);
        }
        else if (value == "2") {
            allLSReport.lifecyclesVisible("True");
            allLSReport.LeadSourceVisible("False");
            allLSReport.communityVisible("False");
            allLSReport.CommunityIds([]);
            allLSReport.LeadSourceIds([]);
        }
        else if (value == "3") {
            allLSReport.communityVisible("True");
            allLSReport.LeadSourceVisible("False");
            allLSReport.lifecyclesVisible("False");
            allLSReport.LifeStageIds([]);
            allLSReport.LeadSourceIds([]);
        }
    }
    allLSReport.errors = ko.validation.group(allLSReport, true);

    allLSReport.createGrid = function (data) {
        if ($('#grid').data().kendoGrid != undefined) {
            $('#grid').data().kendoGrid.destroy();
            $(".k-grid-pager").remove();
            $(".k-grid-toolbar").remove();
            $('#grid').empty();
        }

        var count = 0;
        $.each(data, function (index, value) {
            count += parseInt(value.SecondaryLSCount);
        });
        allLSReport.CountInAreaChart(count);
        var columns = [];
        columns.push({
            field: "Value",
            title: "[|Lead Sources|]",
        });
        columns.push({
            field: "SecondaryLSCount",
            title: "[|Total Contacts|]",
            attributes: {
                rowid: "#:data.LeadSourceID#"
            }
        });
        columns.push({
            field: "SecondaryLSPercent",
            title: "[|% of Contacts|]",
        });
        columns.push({
            field: "PrimaryLSCount",
            title: "[|Unique Contacts|]",
            attributes: {
                rowid: "#:data.LeadSourceID#"
            }
        });
        columns.push({
            field: "PrimaryLSPercent",
            title: "[|% of Unique Contacts|]",
        });

        var configuration = {
            scrollable: false,
            dataSource: data,
            pageable: {
                pageSizes: ToPageSizeArray(),
                messages: {
                    display: "[|Showing|] {0}-{1} [|from|] {2} Lead Sources"
                },
            },
            columns: columns,
            sortable: true,
            resizable: true,
            reorderable: true,
            dataBound: function () {
                $('td').each(function (index, value) {
                    if ((index % 5 == 1 || index % 5 == 3) && $(this).text() != 0) {
                        var anchor;
                        anchor = document.createElement("a");
                        anchor.setAttribute("cellid", index);
                        anchor.style.cursor = 'pointer';
                        anchor.onclick = function () {
                            allLSReport.LeadSourceIds(parseInt(this.parentNode.getAttribute("rowid")));
                            allLSReport.ColumnIndex = ko.observable(index % 5 == 1 ? 1 : 2);
                            var jsondata = ko.toJSON(allLSReport);
                            pageLoader();
                            $.ajax({
                                url: url + '/GetContacts',
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
                                return filter.promise()
                            }).done(function (response) {
                                removepageloader();
                                if (response != null) {
                                    if (response.success == true) {
                                        localStorage.setItem("ContactsGuid", response.response);
                                        window.location.href = '../reportcontacts?guid=' + response.response + '&reportType=' + allLSReport.ReportType() + '&reportId=' + allLSReport.ReportId();
                                    }
                                }
                            }).fail(function (error) {
                                notifyError(error);
                            });
                        };
                        anchor.innerHTML = $(this).text();
                        $(this).text("");
                        $(this).append(anchor);
                    }
                });
                
            },
            toolbar: [{ name: "excel", text: "Export to Excel" }],
            excelExport: function (e) {
                var sheet = e.workbook.sheets[0];
                for (var i = 1; i < sheet.rows.length; i++) {
                    for (var ci = 1; ci < sheet.rows[i].cells.length; ci++) {
                        var cellvalue = sheet.rows[i].cells[ci].value;
                        sheet.rows[i].cells[ci].value = parseInt(cellvalue);
                    }
                }
            },
            excel: {
                fileName: "AllLeadSourceReport.xlsx",
                allPages: true,
                //for Exporting the file in IE9 and Safari Browsers
                proxyURL: "/Reports/ExportToExcel",
                forceProxy: true
            }
        }
        var timeEditGrid = $("#grid").kendoGrid(configuration).data("kendoGrid");
        var grid = $("#grid").data("kendoGrid");
        grid.dataSource.query({ page: 1, pageSize: parseInt(itemsPerPage) });

        $(".k-grid-excel").addClass("cu-grid-excel");
    };
    allLSReport.createPieChart = function (data) {

        allLSReport.chartVisible("True");

        $("#PieChart").kendoChart({
            legend: {
                position: "bottom"
            },
            chartArea: {
                background: ""
            },
            theme: 'Metro',
            seriesDefaults: {
                labels: {
                    visible: false,
                    background: "transparent",
                    template: "#= category #: #= value#"
                }
            },
            dataSource: {
                data: data
            },
            series: [{
                type: "pie",
                field: "C",
                startAngle: 150,
                categoryField: "GridValue",
            }],
            seriesColors: ["#ef7373", "#e6e473", "#79d082", "#efa76d", "#5cc0f4"],
            tooltip: {
                visible: true,
                template: "${category}- ${ value }"
            },
        });
    };
    allLSReport.createAreaChart = function (data) {

        $("#AreaChart").kendoChart({
            legend: {
                position: "bottom",
                visible: false
            },
            seriesDefaults: {
                type: "area",
                area: {
                    line: {
                        style: "smooth"
                    }
                }
            },
            theme: 'Metro',
            series: [{
                name: "Current",
                field: "C",
                categoryField: "ID",
                data: data
            }, {
                name: "Previous",
                field: "P",
                categoryField: "ID",
                data: data
            }],
            valueAxis: {
                labels: {
                    visible: false,
                    format: "{0}%"
                },
                line: {
                    visible: false
                },
                majorGridLines: {
                    visible: false
                },
                visible: false
            },
            categoryAxis: {
                visible: false,

                majorGridLines: {
                    visible: false
                }
            },
            tooltip: {
                visible: true,
                format: "{0}%",
                template: "#= series.name #: #= value #"
            }
        });
    };

    allLSReport.Runlist = function () {
        allLSReport.errors.showAllMessages();
        if (allLSReport.errors().length > 0) {
            notifyError("[|Please select valid Date Range.|]");
            return;
        }
        var oneDay = 24 * 60 * 60 * 1000;
        var firstDate = new Date(allLSReport.CustomStartDate());
        var secondDate = new Date(allLSReport.CustomEndDate());
        var diffDays = Math.round(Math.abs((firstDate.getTime() - secondDate.getTime()) / (oneDay)));
        if (diffDays < 60)
            allLSReport.DateRange('D');
        else if (diffDays > 60 && diffDays < 180)
            allLSReport.DateRange('W');
        else if (diffDays > 180)
            allLSReport.DateRange('M');

        var selectedCommunities = [];
        var selectedlifecycles = [];
        var selectedLeadSources = [];

        if (allLSReport.LifecycleStage() == 0) {
            for (var l = 0; l < (data.LifecycleStages).length; l++) {
                selectedlifecycles.push((data.LifecycleStages)[l].DropdownValueID);
            }
        }
        else
            selectedlifecycles = (allLSReport.LifecycleStage()).length > 0 ? (allLSReport.LifecycleStage()).map(Number) : [];

        if (allLSReport.LeadSource() == 0) {
            for (var l = 0; l < (allLSReport.LeadSources()).length; l++) {
                selectedLeadSources.push((allLSReport.LeadSources())[l].DropdownValueID);
            }
        }
        else
            selectedLeadSources = (allLSReport.LeadSource()).length > 0 ? (allLSReport.LeadSource()).map(Number) : [];

        if (allLSReport.Community() == 0) {
            for (var l = 0; l < (allLSReport.Communities()).length; l++) {
                selectedCommunities.push((allLSReport.Communities())[l].DropdownValueID);
            }
        }
        else
            selectedCommunities = (allLSReport.Community()).length > 0 ? (allLSReport.Community()).map(Number) : [];

        allLSReport.CommunityIds(selectedCommunities);
        allLSReport.LifeStageIds(selectedlifecycles);
        allLSReport.LeadSourceIds(selectedLeadSources);
        pageLoader();
        var jsondata = ko.toJSON(allLSReport);
        $.ajax({
            url: url + '/GetFirstLeadSourceReport',
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
            } return filter.promise()
        }).done(function (data) {
            allLSReport.createGrid(data.response.AllLeadSourceData);
            allLSReport.createPieChart(data.response.PieChartData);
            allLSReport.createAreaChart(data.response.AreaChartData);
            removepageloader();
        }).fail(function (error) {
            notifyError(error);
        });
    };

    allLSReport.RunReport = function () {
        allLSReport.Runlist();
    }

    if (runReport == 'True')
        allLSReport.Runlist();
};