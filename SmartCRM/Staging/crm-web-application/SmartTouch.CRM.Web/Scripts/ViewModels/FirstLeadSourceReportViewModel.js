var firstLeadSourceReportVM = function (url, data, dateFormat, users, runReport, itemsPerPage) {
    fls = this;
    fls.ReportType = ko.observable(16);
    fls.ReportId = ko.observable(data.ReportId);
    fls.Periods = ko.observableArray([
      { PeriodId: 1, Period: '[|Last 7 Days|]' },
      { PeriodId: 2, Period: '[|Last 30 Days|]' },
      { PeriodId: 3, Period: '[|Last 3 Months|]' },
      { PeriodId: 4, Period: '[|Month to Date|]' },
      { PeriodId: 5, Period: '[|Year to Date|]' },
      { PeriodId: 6, Period: '[|Last Year|]' },
      { PeriodId: 7, Period: '[|Custom|]' }
    ]);
    fls.ComparedTo = ko.observableArray([
      { Previous: '[|Previous Period|]', PreviousId: 1 },
      { Previous: '[|Previous Year|]', PreviousId: 2 }
    ]);
    fls.PeriodId = ko.observable(2);
    fls.PreviousId = ko.observable(1);

    var toStartDate = new Date();
    toStartDate.setDate(toStartDate.getDate());
    fls.CustomEndDate = ko.observable(kendo.toString(toStartDate, dateFormat)).extend({ required: { message: "" } });;
    fls.CustomEndDatePrev = ko.observable();

    var fromdate = new Date();
    fromdate.setMonth(fromdate.getMonth() - 1);
    fls.CustomStartDate = ko.observable(kendo.toString(fromdate, dateFormat)).extend({ required: { message: "" } });;
    fls.CustomStartDatePrev = ko.observable();

    fls.IsCompared = ko.observable(false);
    fls.compareToVisible = ko.observable("True");
    fls.CustomEndDate(moment(new Date()).format());
    fls.CustomStartDate(moment().subtract(30, 'days').format());
    fls.campareToVisible = ko.observable("True");
    var maxdate = new Date();
    maxdate.setDate(maxdate.getDate() + 1);
    fls.toMaxDate = ko.observable(maxdate);
    var fromMaxDate = new Date();
    fromMaxDate.setDate(fromMaxDate.getDate() - 1);
    fls.fromMaxDate = ko.observable(fromMaxDate);
    fls.DateFormat = ko.observable(dateFormat);
    fls.DateFormat = ko.observable(dateFormat);
    fls.DateRange = ko.observable('D');

    fls.periodChange = function () {
        var value = this.value();
        fls.campareToVisible('True');
        fls.toMaxDate(maxdate);
        if (value == "1")  // last 7 days
            fls.CustomStartDate(moment().subtract(7, 'days').format());
        else if (value == "2")  // last 30 days
            fls.CustomStartDate(moment().subtract(30, 'days').format());
        else if (value == "3")  //last 3 months
            fls.CustomStartDate(moment().subtract(3, 'months').format());
        else if (value == "4")  //month to date
            fls.CustomStartDate(moment().startOf('month').format());
        else if (value == "5")   // month to year
            fls.CustomStartDate(moment().startOf('year').format());
        else if (value == "6") { //last year
            fls.campareToVisible('False');
            fls.CustomStartDate(moment().subtract(1, 'years').format());
            fls.IsCompared(false);
        }
        if (value == "7") { // custom
            fls.campareToVisible('False');
            fls.IsCompared(false);
            var toStartDate = new Date();
            toStartDate.setDate(toStartDate.getDate());
            fls.CustomEndDate(toStartDate);
            var fromdate = new Date();
            fromdate.setDate(fromdate.getDate() - 7);
            fls.CustomStartDate(fromdate);
            fls.toMaxDate(new Date());
        }
    }

    fls.CustomDateDisplay = ko.pureComputed(function () {
        if (fls.PeriodId() == 7)
            return true;
        else
            return false;
    });

    fls.fromDateChangeEvent = function () {
        var fromDate = this.value();
        fls.CustomStartDate(kendo.toString(fromDate, dateFormat));
        var toDate = fls.CustomEndDate();
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }

    fls.toDateChangeEvent = function () {
        var fromDate = fls.CustomStartDate();
        var toDate = this.value();
        fls.CustomEndDate(kendo.toString(toDate, dateFormat));
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }

    fls.setCompareTo = function (data, event) {
        fls.IsCompared(event.target.checked);
    }

    fls.OwnerIds = ko.observableArray([]);
    fls.CommunityIds = ko.observableArray([]);
    fls.LeadSourceIds = ko.observableArray([]);
    fls.LifeStageIds = ko.observableArray([]);
    fls.accountExecVisible = ko.observable("True");
    fls.lifecyclesVisible = ko.observable("False");
    fls.communityVisible = ko.observable("False");
    fls.chartVisible = ko.observable("False");
    fls.Users = ko.observableArray(users);
    fls.Communities = ko.observableArray(data.Communities);
    fls.LeadSources = ko.observableArray(data.LeadSources);
    fls.LifecycleStages = ko.observableArray(data.LifecycleStages);
    fls.OwnerId = ko.observable(data.OwnerId);
    fls.LifecycleStage = ko.observable(0);
    fls.Community = ko.observable(0);
    fls.CountInAreaChart = ko.observable(0);
    fls.LeadSource = ko.observable(0);

    fls.templ = kendo.template('#if (data.OwnerId == 0 )  { # <span>[|Select..|]</span># } ##if (data.IsDeleted == false )  { # <span>#= OwnerName #</span># } # #if (data.IsDeleted == true )  { # <span style="color:red;">#= OwnerName #</span># } #');
    fls.Groups = ko.observableArray([
           { GroupId: 1, Group: '[|Account Executive|]' },
           { GroupId: 2, Group: '[|Lifecycle Stage|]' },
           { GroupId: 3, Group: '[|Community|]' }
    ]);
    fls.GroupId = ko.observable(1);

    fls.groupChange = function () {
        var value = this.value();
        if (value == "1") {
            fls.accountExecVisible("True");
            fls.lifecyclesVisible("False");
            fls.communityVisible("False");
            fls.CommunityIds([]);
            fls.LifeStageIds([]);
        }
        else if (value == "2") {
            fls.lifecyclesVisible("True");
            fls.accountExecVisible("False");
            fls.communityVisible("False");
            fls.OwnerIds([]);
            fls.CommunityIds([]);
        }
        else if (value == "3") {
            fls.communityVisible("True");
            fls.accountExecVisible("False");
            fls.lifecyclesVisible("False");
            fls.LifeStageIds([]);
            fls.OwnerIds([]);
        }
    }
    fls.errors = ko.validation.group(fls, true);

    fls.Runlist = function () {
        fls.errors.showAllMessages();
        if (fls.errors().length > 0) {
            notifyError("[|Please select valid Date Range.|]");
            return;
        }
        var oneDay = 24 * 60 * 60 * 1000;
        var firstDate = new Date(fls.CustomStartDate());
        var secondDate = new Date(fls.CustomEndDate());
        var diffDays = Math.round(Math.abs((firstDate.getTime() - secondDate.getTime()) / (oneDay)));
        if (diffDays < 60)
            fls.DateRange('D');
        else if (diffDays > 60 && diffDays < 180)
            fls.DateRange('W');
        else if (diffDays > 180)
            fls.DateRange('M');

        var selectedOwners = [];
        var selectedlifecycles = [];
        var selectedLeadSources = [];
        var selectedCommunities = [];

        if (fls.OwnerId() == 0) {

            if (users.length > 0) {
                for (var l = 0; l < (users).length; l++) {
                    selectedOwners.push((users)[l].OwnerId);
                }
            }
            else
                selectedOwners.push(0);
        }
        else if (fls.OwnerId() == data.OwnerId) {

            if (Array.isArray(fls.OwnerId()))
                selectedOwners = (fls.OwnerId()).length > 0 ? (fls.OwnerId()).map(Number) : [];
            else
                selectedOwners.push(fls.OwnerId());
        }

        else {
            selectedOwners = (fls.OwnerId()).length > 0 ? (fls.OwnerId()).map(Number) : [];
        }

        if (fls.LifecycleStage() == 0) {
            for (var l = 0; l < (data.LifecycleStages).length; l++) {
                selectedlifecycles.push((data.LifecycleStages)[l].DropdownValueID);
            }
        }
        else
            selectedlifecycles = (fls.LifecycleStage()).length > 0 ? (fls.LifecycleStage()).map(Number) : [];

        if (fls.LeadSource() == 0) {
            for (var l = 0; l < (fls.LeadSources()).length; l++) {
                selectedLeadSources.push((fls.LeadSources())[l].DropdownValueID);
            }
        }
        else
            selectedLeadSources = (fls.LeadSource()).length > 0 ? (fls.LeadSource()).map(Number) : [];

        if (fls.Community() == 0) {
            for (var i = 0; i < (data.Communities).length; i++) {
                selectedCommunities.push((data.Communities)[i].DropdownValueID);
            }
        }
        else
            selectedCommunities = (fls.Community()).length > 0 ? (fls.Community()).map(Number) : [];

        fls.OwnerIds(selectedOwners);
        fls.LifeStageIds(selectedlifecycles);
        fls.LeadSourceIds(selectedLeadSources);
        fls.CommunityIds(selectedCommunities);
        pageLoader();
        var jsondata = ko.toJSON(fls);
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
            fls.createGrid(data.response.GridData);
            fls.createPieChart(data.response.PieChartData);
            fls.createAreaChart(data.response.AreaChartData);
            removepageloader();
        }).fail(function (error) {
            notifyError(error);
        });
    };

    fls.createGrid = function (data) {
        if ($('#grid').data().kendoGrid != undefined) {
            $('#grid').data().kendoGrid.destroy();
            $(".k-grid-pager").remove();
            $(".k-grid-toolbar").remove();
            $('#grid').empty();
        }

        var rows = [];
        for (var i = 0; i < data.length; i++) {
            var entryArray = [];
            entryArray.push(data[i].Name || "");  

            var total = 0;
            for (var j = 0; j < data[i].DropdownValues.length; j++) {
                total += data[i].DropdownValues[j].DropdownValue;
                entryArray.push(data[i].DropdownValues[j].DropdownValue);  // + "|" + data[i].DropdownValues[j].DropdownValueName + "|" + data[i].ID      parseInt(data[i].DropdownValues[j].DropdownValue)
            }
            entryArray.push(total);  // + "|" + "t" + "|" + data[i].ID   parseInt(

            entryArray.push(data[i].PreviousTotal);
            rows.push(kendo.observable({
                entries: entryArray
            }));
        }

        var viewModel = kendo.observable({
            gridRows: rows
        });

        var columns = [];
        var group = [];
        var entryIndex = "entries[" + 0 + "]";

        columns.push({
                    field: entryIndex,
                    title: "[|Lead Sources|]",
                    footerTemplate: "Total"
                });

        for (var i = 0; i < data[0].DropdownValues.length; i++) {
            var entryIndex = "entries[" + parseInt(i + 1) + "]";
            var seq = (i + 1).toString();
            columns.push({
                field: entryIndex,
                title: data[0].DropdownValues[i].DropdownValueName,
                attributes: { cellid: data[0].DropdownValues[i].DropdownValueId },
                footerTemplate: Sum(seq, data[0].DropdownValues[i].DropdownValueName) == "0" ? "0" : "<a href='javascript:void(0)' onClick=myCustomFunction(" + "'" + data[0].DropdownValues[i].DropdownValueId + "'" + ")>" + Sum(seq, data[0].DropdownValues[i].DropdownValueName) + "</a>"
                });

            if (i == data[0].DropdownValues.length - 1) {

                if (fls.IsCompared() == true) {
                    var entryIndex = "entries[" + parseInt(i + 2) + "]";
                    var seq = (i + 2).toString();
                    columns.push({
                        field: entryIndex,
                        title: "Total (c)",
                        footerTemplate: Sum(seq, "CurrentTotal")
                    });
                    entryIndex = "entries[" + parseInt(i + 3) + "]";
                    seq = (i + 3).toString();

                    columns.push({
                        field: entryIndex,
                        title: "Total (p)",
                        footerTemplate: Sum(seq, "PreviousTotal")
                    });
                }
                else if (fls.IsCompared() == false) {
                    var entryIndex = "entries[" + parseInt(i + 2) + "]";
                    var seq = (i + 2).toString();
                    columns.push({
                        field: entryIndex,
                        title: "Total",
                        footerTemplate: Sum(seq, "Total") == "0" ? "0" : "<a href='javascript:void(0)' onClick=myCustomFunction(" + "'" + 0 + "'" + ")>" + Sum(seq, data[0].DropdownValues[i].DropdownValueName) + "</a>"
                    });
                }
            }
        }

        function Sum(seq, name) {
            var value = 0;
            for (var i = 0; i < rows.length; i++) {

                var rowentry = rows[i].entries[parseInt(seq)];
                var rowentrylist = rowentry.toString().split("|");
                value += parseInt(rowentrylist[0]);

            }
            if (name == "Total")
                fls.CountInAreaChart(value);
            return value.toString();
        }
        
        var configuration = {
            scrollable: false,

            pageable: {
                pageSizes: ToPageSizeArray(),
                messages: {
                    display: "[|Showing|] {0}-{1} [|from|] {2} Lead Sources"
                },
            },
            columns: columns,
            sortable: true,
            dataBound: function () {
                var colLength = $("#grid").data("kendoGrid").columns.length;
                $('td').each(function (index, value) {

                    if (index >= colLength) {
                        var text = $(this).text();
                        var selectedcell = 0; var value = null;
                        var dropdowns = text.split("|");
                        var th = $(this).closest('table').find('th').eq($(this).index());
                        var firstTrText = $(this).closest('td').siblings().first().text();
                        var dropdownvalueid = $(th).text();
                        
                        var anchor = document.createElement("a");
                     

                        if (!isNaN(dropdowns[0]) && dropdowns[0] != undefined && parseInt(dropdowns[0]) > 0) {
                            var groupByValue =[];
                            data.map(function (v) { groupByValue.push({ name: v.Name, Id: v.ID })
                            });
                            var row = ko.utils.arrayFirst(groupByValue, function (i) {
                                if (i.name == firstTrText)
                                    return i;
                            });
                            if (row != null)
                                anchor.setAttribute("rowid", row.Id);

                            anchor.id = selectedcell;
                            anchor.setAttribute("cellid", selectedcell);
                            anchor.style.cursor = 'pointer';
                            anchor.onclick = function () {
                                var cellid = $(this).closest('td')[0].getAttribute("cellid");
                                if (cellid == null) {
                                    var tds = $(this).closest('tr').find('td');
                                    if (tds) {
                                        var ids = [];
                                        $.each(tds, function (i, d) {
                                            var cellid = $(d).attr("cellid");
                                            if (cellid)
                                                ids.push(cellid);
                                        });
                                        fls.OwnerIds(ids);
                                        fls.LifeStageIds(ids);
                                        fls.CommunityIds(ids);
                                    }
                                }
                                else {
                                    fls.OwnerIds(parseInt(cellid));
                                    fls.LifeStageIds(parseInt(cellid));
                                    fls.CommunityIds(parseInt(cellid));
                                }
                                fls.LeadSourceIds(parseInt(this.getAttribute("rowid")));
                                var jsondata = ko.toJSON(fls);
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
                                            window.location.href = '../reportcontacts?guid=' + response.response + '&reportType=' + fls.ReportType() + '&reportId=' + fls.ReportId();
                                    }
                                }
                                }).fail(function (error) {
                                    notifyError(error);
                            });
                        };
                        }
                        else {
                            anchor = document.createElement("span");
                            }
                        anchor.innerHTML = dropdowns[0];
                        $(this).text("");
                        $(this).append(anchor);
                    }
                });
            },
            toolbar: [{ name: "excel", text: "Export to Excel" }],
            excelExport: function (e) {
                var sheet = e.workbook.sheets[0];
                var sheetValue = 0;
                for (var i = 1; i < sheet.rows.length; i++) {
                    for (var ci = 1; ci < sheet.rows[i].cells.length; ci++) {
                        var cellvalue = sheet.rows[i].cells[ci].value.toString().split("|");
                        if (parseInt(cellvalue[0]) == 0 || parseInt(cellvalue[0]) > 0)
                            sheetValue = parseInt(cellvalue[0])
                        else
                            sheetValue = $(cellvalue[0]).text();

                        sheet.rows[i].cells[ci].value = sheetValue;
                    }
                }
            },
            excel: {
                fileName: "FirstLeadSourceReport.xlsx",
                allPages: true,
                //for Exporting the file in IE9 and Safari Browsers
                proxyURL: "/Reports/ExportToExcel",
                forceProxy: true
            }
        }
        var timeEditGrid = $("#grid").kendoGrid(configuration).data("kendoGrid");
        var grid = $("#grid").data("kendoGrid");
        grid.dataSource.query({ page: 1, pageSize: parseInt(itemsPerPage) });
        kendo.bind($('#resultsgrid'), viewModel);

        $(".k-grid-excel").addClass("cu-grid-excel");
    };

    fls.createPieChart = function (data) {
        fls.chartVisible("True");
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

    fls.createAreaChart = function (data) {
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

    fls.RunReport = function () {
        fls.Runlist();
    }

    if (runReport == 'True')
        fls.Runlist();
};