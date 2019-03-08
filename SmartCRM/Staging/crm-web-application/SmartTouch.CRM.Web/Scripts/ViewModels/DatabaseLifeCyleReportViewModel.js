var databaseLifeCycleViewModel = function (url, data, Users, dateFormat, runReport, itemsPerPage) {
    selfLifeCycle = this;


    ko.validation.configure({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null
    });

    selfLifeCycle.OwnerId = ko.observable(0);
    selfLifeCycle.Users = ko.observableArray(Users);
    selfLifeCycle.OwnerIds = ko.observableArray([]);

    selfLifeCycle.templ = kendo.template('#if (data.OwnerId == 0 )  { # <span>[|Select..|]</span># } ##if (data.IsDeleted == false )  { # <span>#= OwnerName #</span># } # #if (data.IsDeleted == true )  { # <span style="color:red;">#= OwnerName #</span># } #');

    selfLifeCycle.ReportId = ko.observable(data.ReportId);
    selfLifeCycle.ReportName = ko.observable(data.ReportName);
    selfLifeCycle.PeriodId = ko.observable(2);
    selfLifeCycle.DateRange = ko.observable('D');
    selfLifeCycle.CustomStartDate = ko.observable();
    selfLifeCycle.CustomEndDate = ko.observable();
    selfLifeCycle.LifecycleStages = ko.observableArray(data.LifecycleStages);
    selfLifeCycle.LifecycleStage = ko.observable(0);
    selfLifeCycle.LifeStageIds = ko.observableArray([]);
    selfLifeCycle.chartVisible = ko.observable("False");
    selfLifeCycle.CountInAreaChart = ko.observable(0);
    selfLifeCycle.ReportType = ko.observable(19);

    var LifecycleStages = data.LifecycleStages;
    var users = Users;
    var today = new Date();

    var leadSources = selfLifeCycle.LifecycleStages();

    var toStartDate = new Date();
    toStartDate.setDate(toStartDate.getDate());
    selfLifeCycle.CustomEndDate = ko.observable(kendo.toString(toStartDate, dateFormat)).extend({ required: { message: "" } });;


    var fromdate = new Date();
    fromdate.setMonth(fromdate.getMonth() - 1);
    selfLifeCycle.CustomStartDate = ko.observable(kendo.toString(fromdate, dateFormat)).extend({ required: { message: "" } });;

    selfLifeCycle.Periods = ko.observableArray([
      { PeriodId: 1, Period: '[|Last 7 Days|]' },
      { PeriodId: 2, Period: '[|Last 30 Days|]' },
      { PeriodId: 3, Period: '[|Last 3 Months|]' },
      { PeriodId: 4, Period: '[|Month to Date|]' },
      { PeriodId: 5, Period: '[|Year to Date|]' },
      { PeriodId: 6, Period: '[|Last Year|]' },
      { PeriodId: 7, Period: '[|Custom|]' }
    ]);

    var toStartDate = new Date();
    selfLifeCycle.CustomEndDate(moment(toStartDate).format());
    selfLifeCycle.CustomStartDate(moment().subtract(30, 'days').format());

    selfLifeCycle.periodChange = function () {
        var value = this.value();
        selfLifeCycle.toMaxDate(maxdate);
        var toStartDate = new Date();
        toStartDate.setDate(toStartDate.getDate());
        selfLifeCycle.CustomStartDate(toStartDate);
        selfLifeCycle.CustomEndDate(toStartDate);
        if (value == "1")  // last 7 days
            selfLifeCycle.CustomStartDate(moment().subtract(7, 'days').format());

        else if (value == "2")  // last 30 days
            selfLifeCycle.CustomStartDate(moment().subtract(30, 'days').format());

        else if (value == "3")  //last 3 months
            selfLifeCycle.CustomStartDate(moment().subtract(3, 'months').format());


        else if (value == "4")  //month to date
            selfLifeCycle.CustomStartDate(moment().startOf('month').format());


        else if (value == "5")   // month to year
            selfLifeCycle.CustomStartDate(moment().startOf('year').format());


        else if (value == "6") { //last year
            selfLifeCycle.CustomStartDate(moment().subtract(1, 'years').format());
        }
        if (value == "7") { // custom

            var toStartDate = new Date();
            toStartDate.setDate(toStartDate.getDate());
            selfLifeCycle.CustomEndDate(toStartDate);
            var fromdate = new Date();
            fromdate.setDate(fromdate.getDate() - 7);
            selfLifeCycle.CustomStartDate(fromdate);
            selfLifeCycle.toMaxDate(new Date());
        }
    }

    selfLifeCycle.CustomDateDisplay = ko.pureComputed(function () {
        if (selfLifeCycle.PeriodId() == 7) {
            return true;
        } else {
            return false;
        }
    });

    var maxdate = new Date();
    maxdate.setDate(maxdate.getDate() + 1);
    selfLifeCycle.toMaxDate = ko.observable(maxdate);
    var fromMaxDate = new Date();
    fromMaxDate.setDate(fromMaxDate.getDate() - 1);
    selfLifeCycle.fromMaxDate = ko.observable(fromMaxDate);
    selfLifeCycle.DateFormat = ko.observable(dateFormat);

    selfLifeCycle.fromDateChangeEvent = function () {
        var fromDate = this.value();
        selfLifeCycle.CustomStartDate(kendo.toString(fromDate, dateFormat));
        var toDate = selfLifeCycle.CustomEndDate();
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }

    selfLifeCycle.toDateChangeEvent = function () {
        var fromDate = selfLifeCycle.CustomStartDate();
        var toDate = this.value();
        selfLifeCycle.CustomEndDate(kendo.toString(toDate, dateFormat));
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }
   
    selfLifeCycle.errors = ko.validation.group(selfLifeCycle);

    selfLifeCycle.createGrid = function (data) {
        if ($('#grid').data().kendoGrid != undefined) {
            $('#grid').data().kendoGrid.destroy();
            $(".k-grid-pager").remove();
            $(".k-grid-toolbar").remove();
            $('#grid').empty();
        }

        var count = 0;
       
        if (data)
            $.each(data, function (i, v) {
                count += v.ContactsCount
            });

        selfLifeCycle.CountInAreaChart(count);
        if (data)
            $.each(data, function (i, v) {
                var lifeCycleStage = ko.utils.arrayFirst(selfLifeCycle.LifecycleStages(), function (item) {
                    return item.DropdownValueID == v.LifecycleStageId;
                });
                v.LifeCycle = lifeCycleStage.DropdownValue;

                if (count == 0)
                    count = 1
                v.PersontageOfContactsCount = Math.round((v.ContactsCount.toFixed(2) / count.toFixed(2)) * 100);
            });
       
        var columns = [];
        columns.push({
            field: "LifeCycle",
            title: "[|Life Cycle|]",
        });
        columns.push({
            field: "ContactsCount",
            title: "[|Overall Contacts|]",
            attributes: {
                rowid: "#:data.LifecycleStageId#"
            },
        });
        columns.push({
            field: "PersontageOfContactsCount",
            title: "[|% of Contacts|]",
            template: '#: data.PersontageOfContactsCount # %'
        });

        var configuration = {
            scrollable: false,
            dataSource: data,
            pageable: {
                pageSizes: ToPageSizeArray(),
                messages: {
                    display: "[|Showing|] {0}-{1} [|from|] {2} Life Cycles"
                },
            },
            columns: columns,
            sortable: true,
            resizable: true,
            reorderable: true,
            dataBound: function () {
                
                $('td').each(function (index, value) {
                    var selectedcel = $(this).text();
                    if (index != 0 && !isNaN(selectedcel) && selectedcel != 0) {
                        var anchor;
                        anchor = document.createElement("a");
                        anchor.setAttribute("cellid", selectedcel);
                        anchor.style.cursor = 'pointer';
                        anchor.onclick = function () {
                            selfLifeCycle.LifeStageIds = ko.observable(parseInt(this.parentNode.getAttribute("rowid")));
                            var jsondata = ko.toJSON(selfLifeCycle);
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
                                        window.location.href = '../reportcontacts?guid=' + response.response + '&reportType=' + selfLifeCycle.ReportType() + '&reportId=' + selfLifeCycle.ReportId();
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
                fileName: "DatabaseLifeCycleReport.xlsx",
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

    selfLifeCycle.createPieChart = function (data) {
        selfLifeCycle.chartVisible("True");
        $.each(data, function (i, v) {
            var lifeCycleStage = ko.utils.arrayFirst(selfLifeCycle.LifecycleStages(), function (item) {
                return item.DropdownValueID == v.LifecycleStageId;
            });
            if (v.ContactsCount != 0)
                v.GridValue = lifeCycleStage.DropdownValue;

        });
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
                field: "ContactsCount",
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
    selfLifeCycle.createAreaChart = function (data) {
        
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

    selfLifeCycle.Runlist = function () {

        var oneDay = 24 * 60 * 60 * 1000;
        var firstDate = new Date(selfLifeCycle.CustomStartDate());
        var secondDate = new Date(selfLifeCycle.CustomEndDate());
        var diffDays = Math.round(Math.abs((firstDate.getTime() - secondDate.getTime()) / (oneDay)));
        if (diffDays < 60)
            selfLifeCycle.DateRange('D');
        else if (diffDays > 60 && diffDays < 180)
            selfLifeCycle.DateRange('W');
        else if (diffDays > 180)
            selfLifeCycle.DateRange('M');

        var selectedlifecycles = [];
        var selectedOwners = [];
        var GridData = [];

        if (selfLifeCycle.LifecycleStage() == 0) {
            for (var l = 0; l < (LifecycleStages).length; l++) {
                selectedlifecycles.push((LifecycleStages)[l].DropdownValueID);
            }
        }
        else
            selectedlifecycles = (selfLifeCycle.LifecycleStage()).length > 0 ? (selfLifeCycle.LifecycleStage()).map(Number) : [];


        if (selfLifeCycle.OwnerId() == 0) {
            for (var l = 0; l < (users).length; l++) {
                selectedOwners.push((users)[l].OwnerId);
            }
        }
        else
            selectedOwners = (selfLifeCycle.OwnerId()).length > 0 ? (selfLifeCycle.OwnerId()).map(Number) : [];

        selfLifeCycle.OwnerIds(selectedOwners);
        selfLifeCycle.LifeStageIds(selectedlifecycles);
       
        if (selfLifeCycle.errors().length > 0) {
            notifyError("[|Please select valid Date Range.|]");
            return;
        }
       
        var jsondata = ko.toJSON(selfLifeCycle);

        if (selfLifeCycle.errors().length == 0) {
            pageLoader();
            $.ajax(
            {
                url: url + "GetDatabaseLifeCycleData",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                type: 'post',
                traditional: true,
                data: jsondata
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
                selfLifeCycle.createGrid(data.response.AllDatabaseLifeCycleData);
                selfLifeCycle.createPieChart(data.response.DatabasePiesChartData);
                selfLifeCycle.createAreaChart(data.response.AreaChartData);
                removepageloader();
            }).fail(function (error) {
                notifyError(error);
                removepageloader();
            });
        }
        else {
            selfLifeCycle.errors.showAllMessages();
        }
    };

    

    selfLifeCycle.RunReport = function () {
        selfLifeCycle.Runlist();
    }

    if (runReport == 'True')
        selfLifeCycle.Runlist();
}