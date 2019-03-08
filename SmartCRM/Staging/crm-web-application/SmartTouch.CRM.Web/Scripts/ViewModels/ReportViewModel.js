var opportunityPipelineViewModel = function (url, data, users, dateFormat, Type, runReport, itemsPerPage, Activities, module, filename) {
    selflist = this;
 
    selflist.ReportId = ko.observable(data.ReportId);
    selflist.ReportName = ko.observable(data.ReportName);
    selflist.ReportType = ko.observable(data.ReportType);

    if (Type == 'P') {
       selflist.Periods = ko.observableArray([
      { PeriodId: 1, Period: '[|Last 7 Days|]' },
      { PeriodId: 2, Period: '[|Last 30 Days|]' },
      { PeriodId: 3, Period: '[|Last 3 Months|]' },
      { PeriodId: 4, Period: '[|Month to Date|]' },
      { PeriodId: 5, Period: '[|Year to Date|]' },
      { PeriodId: 6, Period: '[|Last Year|]' },
      { PeriodId: 7, Period: '[|Custom|]' },
      { PeriodId: 8, Period: '[|Upcoming 30 Days|]' }
        ]);
    }
else {
        selflist.Periods = ko.observableArray([
          { PeriodId: 1, Period: '[|Last 7 Days|]' },
          { PeriodId: 2, Period: '[|Last 30 Days|]' },
          { PeriodId: 3, Period: '[|Last 3 Months|]' },
          { PeriodId: 4, Period: '[|Month to Date|]' },
          { PeriodId: 5, Period: '[|Year to Date|]' },
          { PeriodId: 6, Period: '[|Last Year|]' },
          { PeriodId: 7, Period: '[|Custom|]' }
        ]);
}

    

    if (Type == 'P') {
        selflist.Groups = ko.observableArray([
           { GroupId: 0, Group: '[|Account Executive|]' }
        ]);
    }
    else {
        selflist.Groups = ko.observableArray([
            { GroupId: 0, Group: '[|Account Executive|]' },
            { GroupId: 1, Group: '[|Community|]' }
        ]);
    }

    selflist.ComparedTo = ko.observableArray([
      { Previous: '[|Previous Period|]', PreviousId: 1 },
      { Previous: '[|Previous Year|]', PreviousId: 2 }
    ]);

    selflist.Modules = ko.observableArray(Activities);

    var funnel_colors = ["#88B764", "#92BC71", "#9CC180", "#A5C78D", "#AFCD9C", "#B9D2A9", "#C2D7B7"]; var funnel_index = 0;

    var toStartDate = new Date();
    toStartDate.setDate(toStartDate.getDate());
    selflist.CustomEndDate = ko.observable(kendo.toString(toStartDate, dateFormat)).extend({ required: { message: "" } });;
    selflist.CustomEndDatePrev = ko.observable();

    var fromdate = new Date();
    fromdate.setMonth(fromdate.getMonth() - 1);
    selflist.CustomStartDate = ko.observable(kendo.toString(fromdate, dateFormat)).extend({ required: { message: "" } });;
    selflist.CustomStartDatePrev = ko.observable();
    selflist.OwnerId = ko.observable(Type == 'A' ? 0: data.OwnerId);
    selflist.Users = ko.observableArray();
    selflist.Users(users);
    selflist.templ = kendo.template('#if (data.OwnerId == 0 )  { # <span>[|Select..|]</span># } ##if (data.IsDeleted == false )  { # <span>#= OwnerName #</span># } # #if (data.IsDeleted == true )  { # <span style="color:red;">#= OwnerName #</span># } #');

    selflist.LifecycleStages = ko.observableArray(data.LifecycleStages);
    selflist.Communities = ko.observableArray(data.Communities);
    selflist.TourTypes = ko.observableArray(data.TourTypes);
    selflist.LeadSources = ko.observableArray(data.LeadSources);
    selflist.OpportunityStages = ko.observableArray(data.OpportunityStages);

    selflist.LifecycleStage = ko.observable(0);
    selflist.Community = ko.observable(0);
    selflist.TourType = ko.observable(0);
    selflist.LeadSource = ko.observable(0);
    selflist.PreviousId = ko.observable(1);

    selflist.Module = ko.observable(module);

    selflist.RowId = ko.observable(0);
    selflist.PeriodId = ko.observable(Type == 'P' ? 8 : 2);
    selflist.OpportunityStage = ko.observable(0);

    var lifecycles = data.LifecycleStages;

    selflist.SearchResults = ko.observableArray();
    selflist.OwnerIds = ko.observableArray([]);
    selflist.LifeStageIds = ko.observableArray([]);
    selflist.CommunityIds = ko.observableArray([]);
    selflist.TourTypeIds = ko.observableArray([]);
    selflist.LeadSourceIds = ko.observableArray([]);
    selflist.ModuleIds = ko.observableArray([]);
    selflist.OpportunityStageIds = ko.observableArray([]);

    selflist.accountExecVisible = ko.observable("True");
    selflist.communityVisible = ko.observable("False");
    selflist.chartvisible = ko.observable("False");
    selflist.IsCompared = ko.observable(false);
    selflist.campareToVisible = ko.observable("True");

    selflist.CountInAreaChart = ko.observable(0);
    selflist.PreviousTotal = ko.observable(0);
    selflist.CurrentTotal = ko.observable(0);
    selflist.DropdownType = ko.observable();


    selflist.Type = ko.observable(Type);

    if (Type == 'P') {
        var toStartDate = new Date();
        selflist.CustomEndDate(moment().add(30, 'days').format());
        selflist.CustomStartDate(moment(toStartDate).format());
    }
    else {
        var toStartDate = new Date();
        selflist.CustomStartDate(moment().subtract(30, 'days').format());
        selflist.CustomEndDate(moment(toStartDate).format());
        
    }

    selflist.DateRange = ko.observable('D');

    selflist.periodChange = function () {
        var value = this.value();
        var toStartDate = new Date();
        selflist.campareToVisible('True');

        if (value != "8")
            selflist.CustomEndDate(moment(toStartDate).format());

        selflist.toMaxDate(maxdate);

        if (value == "1")  // last 7 days
            selflist.CustomStartDate(moment().subtract(7, 'days').format());

        else if (value == "2")  // last 30 days
            selflist.CustomStartDate(moment().subtract(30, 'days').format());

        else if (value == "3")  //last 3 months
            selflist.CustomStartDate(moment().subtract(3, 'months').format());


        else if (value == "4")  //month to date
            selflist.CustomStartDate(moment().startOf('month').format());


        else if (value == "5")   // month to year
            selflist.CustomStartDate(moment().startOf('year').format());


        else if (value == "6") { //last year
            selflist.campareToVisible('False');
            selflist.CustomStartDate(moment().subtract(1, 'years').format());
            selflist.IsCompared(false);
        }
         else if (value == "8") { //last year
             selflist.CustomStartDate(moment(toStartDate).format());
             selflist.CustomEndDate(moment().add(30, 'days').format());
            }
        if (value == "7") { // custom

            selflist.campareToVisible('False');
            selflist.IsCompared(false);
            var toStartDate = new Date();
            toStartDate.setDate(toStartDate.getDate());
            selflist.CustomEndDate(toStartDate);
            var fromdate = new Date();
            fromdate.setDate(fromdate.getDate() - 7);
            selflist.CustomStartDate(fromdate);
            selflist.toMaxDate(new Date());
        }
    }


    selflist.CustomDateDisplay = ko.pureComputed(function () {
        if (selflist.PeriodId() == 7) {
            return true;
        } else {
            return false;
        }
    });


    selflist.setCompareTo = function (data, event) {
        selflist.IsCompared(event.target.checked);
    }
    var maxdate = new Date();
    maxdate.setDate(maxdate.getDate() + 1);
    selflist.toMaxDate = ko.observable(maxdate);
    var fromMaxDate = new Date();
    fromMaxDate.setDate(fromMaxDate.getDate() - 1);
    selflist.fromMaxDate = ko.observable(fromMaxDate);
    selflist.DateFormat = ko.observable(dateFormat);

    if (Type == 'P') {
        selflist.fromDateChangeEvent = function () {
            var fromDate = this.value();
            selflist.CustomStartDate(kendo.toString(fromDate, dateFormat));
            var toDate = selflist.CustomEndDate();
            if (Date.parse(fromDate) > Date.parse(toDate)) {
                fromDate.setDate(fromDate.getDate() + 7);
                selflist.CustomEndDate(kendo.toString(fromDate, dateFormat));
            }
        }

        selflist.toDateChangeEvent = function () {
            var fromDate = selflist.CustomStartDate();
            var toDate = this.value();
            selflist.CustomEndDate(kendo.toString(toDate, dateFormat));
            if (Date.parse(fromDate) > Date.parse(toDate)) {
                toDate.setDate(toDate.getDate() - 7);
                selflist.CustomStartDate(kendo.toString(toDate, dateFormat));
            }
        }
        
    }
    else {
        selflist.fromDateChangeEvent = function () {
            var fromDate = this.value();
            selflist.CustomStartDate(kendo.toString(fromDate, dateFormat));
            var toDate = selflist.CustomEndDate();
            if (Date.parse(fromDate) > Date.parse(toDate)) {
                notifyError("[|To date should be greater than From date|]");
                return false;
            }
        }

        selflist.toDateChangeEvent = function () {
            var fromDate = selflist.CustomStartDate();
            var toDate = this.value();
            selflist.CustomEndDate(kendo.toString(toDate, dateFormat));
            if (Date.parse(fromDate) > Date.parse(toDate)) {
                notifyError("[|To date should be greater than From date|]");
                return false;
            }
        }
    }


    selflist.Percentage = function () {
        var num1 = selflist.CurrentTotal();
        var num2 = selflist.PreviousTotal();
        if (num1 == 0 && num2 == 0)
            return 0;
        if (num2 > num1)
            return Math.round(((num2 - num1) / num2) * 100);
        else if (num1 >= num2)
            return Math.round(((num1 - num2) / num1) * 100);
    }

    selflist.GroupId = ko.observable(0);

    selflist.groupChange = function () {
        var value = this.value();
        if (value == "0") {
            selflist.accountExecVisible("True");
            selflist.communityVisible("False");
            selflist.CommunityIds([]);
        }
        else if (value == "1") {
            selflist.communityVisible("True");
            selflist.accountExecVisible("False");
            selflist.OwnerIds([]);
        }
    }

    var ChartData = [];

    function CreateAreaChart(data) {

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

    }
    
    function CreateBarChart(data) {

        $("#BarChart").kendoChart({
            legend: {
                visible: false
            },
            dataSource: {
                data: data
            },
            seriesDefaults: {
                type: "column"
            },
            theme: 'Metro',

            series: [{
                name: "Previous",
                field: "P"
            }, {
                name: "Current",
                field: "C",
            }],
            valueAxis: {
                max: 10,
                line: {
                    visible: false
                },
                minorGridLines: {
                    visible: false
                },
                majorGridLines: {
                    visible: false
                },
                visible: false
            },
            categoryAxis: {
                visible: false,
                field: "GridValue",
                majorGridLines: {
                    visible: false
                }
            },
            tooltip: {
                visible: true,
                template: "${category} - ${ value }"
            }
        });

    }

    function CreateChart(data) {
        selflist.chartvisible("True");
        $('#PipelineChart').kendoChart({
            title: {
                text: kendo.toString(new Date(selflist.CustomStartDate()), dateFormat) + "   to   " + kendo.toString(new Date(selflist.CustomEndDate()), dateFormat),
                position: "bottom",
                font: "OpensansRegular,sans-serif"
            },
            legend: {
                visible: false
            },
            seriesColors: ["#0e5a7e", "#166f99", "#2185b4", "#319fd2", "#3eaee2"],
            seriesDefaults: {
                labels: {
                    visible: true,
                    background: "transparent",
                    color: "white",
                    format: "N0",
                    template: "#= dataItem.TotalCount # (#if (isNaN(percentage))  { # 0.00 % - $0# } # #if (!isNaN(percentage))  {# #=kendo.format('{0:P}',percentage)# - #= dispalyPotential(dataItem.Potential) # #} #)"
                },
                dynamicSlope: false,
                dynamicHeight: false
            },
            series: [{
                type: "funnel",
                field: "Potential",
                categoryField: "DropdownValue",
                data: data
            },
            {
                type: "funnel",
                field: "TotalCount",
                categoryField: "DropdownValue",
                data: data
            }],
            tooltip: {
                visible: true,
                template: "#= category #"
            }

        });
    }

    function CreatePieChart(data) {

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
    }

    function CreateGrid(data) {

        if ($('#grid').data().kendoGrid != undefined) {
            $('#grid').data().kendoGrid.destroy();
            $(".k-grid-pager").remove();
            $(".k-grid-toolbar").remove();
            $('#grid').empty();

        }
        var detailExportPromises = [];
        var rows = [];

        for (var i = 0; i < data.length; i++) {
            var entryArray = [];
            entryArray.push(data[i].Name);

            for (var j = 0; j < data[i].DropdownValues.length; j++) {
                entryArray.push(data[i].DropdownValues[j].DropdownValue);   // + "|" + data[i].DropdownValues[j].DropdownValueName + "|" + data[i].ID
            }

            entryArray.push(data[i].CurrentTotal);      // + "|" + "t" + "|" + data[i].ID
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

        if (selflist.GroupId() == 0) {
            columns.push({
                field: entryIndex,
                title: "[|Account Executive|]",
                footerTemplate: "Total"
            });
        }

        if (selflist.GroupId() == 1)
            columns.push({
                field: entryIndex,
                title: "[|Community|]",
                footerTemplate: "[|Total|]"
            });

        for (var i = 0; i < data[0].DropdownValues.length; i++) {
            var entryIndex = "entries[" + parseInt(i + 1) + "]";
            var seq = (i + 1).toString();

            if ((Type == 'A' || Type == 'S' || Type == 'P' || Type == 'L' || Type == 'T') || (Type == 'C' && data[0].DropdownValues[i].DropdownValueName != 'Total')) {
                columns.push({
                    field: entryIndex,
                    title: data[0].DropdownValues[i].DropdownValueName,
                    footerTemplate: Sum(seq, data[0].DropdownValues[i].DropdownValueName) == "0" ? "0" : "<a href='javascript:void(0)' onClick=myCustomFunction(" + "'" + data[0].DropdownValues[i].DropdownValueName.replace(/\s+/g, "") + "'" + "," + "'" + data[0].DropdownValues[i].DropdownType + "'" + ")>" + Sum(seq, data[0].DropdownValues[i].DropdownValueName) + "</a>"

                });
            }

            //columns.push({
            //    field: entryIndex,
            //    title: data[0].DropdownValues[i].DropdownValueName,
            //    footerTemplate: Sum(seq, data[0].DropdownValues[i].DropdownValueName) == "0" ? "0" : "<a href='javascript:void(0)' onClick=myCustomFunction(" + "'" + data[0].DropdownValues[i].DropdownValueName.replace(/\s+/g, "") + "'" + "," + "'" + data[0].DropdownValues[i].DropdownType + "'" + ")>" + Sum(seq, data[0].DropdownValues[i].DropdownValueName) + "</a>"

            //});

            if (i == data[0].DropdownValues.length - 1) {

                if (selflist.IsCompared() == true) {
                    var entryIndex = "entries[" + parseInt(i + 2) + "]";
                    var seq = (i + 2).toString();
                    var curruntColumnName = Type == 'C' ? "Total Count (c)" : "Total (c)";
                    var previousColumnName = Type == 'C' ? "Total Count (p)" : "Total (p)";
                    columns.push({
                        field: entryIndex,
                        title: curruntColumnName,
                        footerTemplate: Sum(seq, "CurrentTotal") == "0" ? "0" : Type != 'A' ? "<a href='javascript:void(0)' onClick=myCustomFunction(" + "'" + curruntColumnName + "'" + "," + "'" + data[0].DropdownValues[i].DropdownType + "'" + ")>" + Sum(seq, "CurrentTotal") + "</a>" : Sum(seq, "CurrentTotal")
                    });
                    entryIndex = "entries[" + parseInt(i + 3) + "]";
                    seq = (i + 3).toString();

                    columns.push({
                        field: entryIndex,
                        title: previousColumnName,
                        footerTemplate: Sum(seq, "PreviousTotal") == "0" ? "0" : Type != 'A' ? "<a href='javascript:void(0)' onClick=myCustomFunction(" + "'" + previousColumnName + "'" + "," + "'" + data[0].DropdownValues[i].DropdownType + "'" + ")>" + Sum(seq, "PreviousTotal") + "</a>" : Sum(seq, "PreviousTotal")
                    });
                }
                else if (selflist.IsCompared() == false) {
                    var entryIndex = "entries[" + parseInt(i + 2) + "]";
                    var seq = (i + 2).toString();
                    var columnName = Type == 'C' ? "Total Count" : "Total";

                    columns.push({
                        field: entryIndex,
                        title: columnName,
                        footerTemplate: Sum(seq, "Total") == "0" ? "0" : Type != 'A' ? "<a href='javascript:void(0)' onClick=myCustomFunction(" + "'" + columnName.replace(/\s+/g, "") + "'" + "," + "'" + data[0].DropdownValues[i].DropdownType + "'" + ")>" + Sum(seq, "Total") + "</a>" : Sum(seq, "Total")
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
            if (name == "Total" || name == "CurrentTotal") {
                selflist.CountInAreaChart(value);
                selflist.CurrentTotal(value);
            }
            else if (name == "PreviousTotal")
                selflist.PreviousTotal(value);
            funnel_index++;
            if (funnel_index >= funnel_colors.length) funnel_index = 0;
            //if (name != "Total")
            //    ChartData.push({ category: name, value: value });
            return value.toString();
        }

        var configuration = {
            scrollable: false,

            pageable: {
                pageSizes: ToPageSizeArray(),
                messages: {
                    display: "[|Showing|] {0}-{1} [|from|] {2} " + (selflist.GroupId() == 0 ? '[|Account Executives|]' : '[|Comminities|]')
                },
            },
            columns: columns,
            sortable: true,
            dataBound: function () {
                var colLength = $("#grid").data("kendoGrid").columns.length;

                $('td').each(function (index, value) {

                    if (index >= colLength) {
                        //console.log("Index : " + index + "   value :" + value);
                        var text = $(this).text();
                        var selectedcell = 0; var value = null;
                        var dropdowns = text.split("|");

                        var th = $(this).closest('table').find('th').eq($(this).index());
                        var firstTrText = $(this).closest('td').siblings().first().text();
                        var dropdownvalueid = $(th).text();

                        if (Type == 'T') 
                            value = ko.utils.arrayFirst(selflist.TourTypes(), function (item) {
                                if (selflist.IsCompared() == 1)
                                    dropdownvalueid = dropdownvalueid.replace("(c)", "").replace("(p)", "");
                                return item.DropdownValue.toString() === dropdownvalueid;
                            });
                        else if (Type == 'P')
                            value = ko.utils.arrayFirst(selflist.OpportunityStages(), function (item) {
                                if (selflist.IsCompared() == 1)
                                    dropdownvalueid = dropdownvalueid.replace("(c)", "").replace("(p)", "");
                                return item.DropdownValue.toString() === dropdownvalueid;
                            });
                        else if (Type == 'L' || Type == 'C')
                            value = ko.utils.arrayFirst(selflist.LifecycleStages(), function (item) {
                                if (selflist.IsCompared() == 1)
                                    dropdownvalueid = dropdownvalueid.replace("(c)", "").replace("(p)", "");
                                return item.DropdownValue.toString() === dropdownvalueid;
                            });
                        else if (Type == 'S')
                            value = ko.utils.arrayFirst(selflist.LeadSources(), function (item) {
                                if (selflist.IsCompared() == 1)
                                    dropdownvalueid = dropdownvalueid.replace("(c)", "").replace("(p)", "");
                                return item.DropdownValue.toString() === dropdownvalueid;
                            });

                        var anchor = document.createElement("a");
                        if (value != null)
                            selectedcell = value.DropdownValueID;

                        if (!isNaN(dropdowns[0]) && dropdowns[0] != undefined && parseInt(dropdowns[0]) > 0) {       
                            var groupByValue = [];
                            data.map(function (v) { groupByValue.push({ name: v.Name, Id: v.ID }) });
                            var row = ko.utils.arrayFirst(groupByValue, function (i) {
                                if (i.name == firstTrText)
                                    return i;
                            });
                            if (row != null)
                                anchor.setAttribute("rowid", row.Id);

                            if (Type == 'A' && dropdownvalueid == 'Actions') {
                                anchor.id = selectedcell;

                                anchor.style.cursor = 'pointer';
                                var userIds = [];
                                anchor.onclick = function () {
                                    var startdate = selflist.CustomStartDate();
                                    var enddate = selflist.CustomEndDate();
                                    selflist.RowId(parseInt(this.getAttribute("rowid")));
                                    userIds.push(selflist.RowId());
                                    var items = JSON.stringify(userIds);
                                    window.location.href = '../reportactions?userIds=' + items + '&StartDate=' + moment(startdate).format('MM/DD/YYYY') + '&EndDate=' + moment(enddate).format('MM/DD/YYYY');
                                };
                            }

                            if (Type == 'A' && dropdownvalueid == 'Opportunity') {
                                anchor.id = selectedcell;
                                anchor.style.cursor = 'pointer';
                                var userIds = [];
                                anchor.onclick = function () {
                                    var startdate = selflist.CustomStartDate();
                                    var enddate = selflist.CustomEndDate();
                                    selflist.RowId(parseInt(this.getAttribute("rowid")));
                                    userIds.push(selflist.RowId());
                                    var items = JSON.stringify(userIds);
                                    window.location.href = '../opportunitiesreportlist?userIds=' + items + '&StartDate=' + moment(startdate).format('MM/DD/YYYY') + '&EndDate=' + moment(enddate).add(1, 'days').format('MM/DD/YYYY');
                                };
                            }

                            if (Type == 'A' && dropdownvalueid == 'Campaigns') {
                                anchor.id = selectedcell;
                                anchor.style.cursor = 'pointer';
                                var userIds = [];
                                anchor.onclick = function () {
                                    var startdate = selflist.CustomStartDate();
                                    var enddate = selflist.CustomEndDate();
                                    selflist.RowId(parseInt(this.getAttribute("rowid")));
                                    userIds.push(selflist.RowId());
                                    var items = JSON.stringify(userIds);
                                    window.location.href = '../campaignsreportlist?userIds=' + items + '&StartDate=' + moment(startdate).format('MM/DD/YYYY') + '&EndDate=' + moment(enddate).add(1, 'days').format('MM/DD/YYYY');
                                };
                            }

                            if (Type == 'A' && dropdownvalueid == 'Forms') {
                                anchor.id = selectedcell;
                                anchor.style.cursor = 'pointer';
                                var userIds = [];
                                anchor.onclick = function () {
                                    var startdate = selflist.CustomStartDate();
                                    var enddate = selflist.CustomEndDate();
                                    selflist.RowId(parseInt(this.getAttribute("rowid")));
                                    userIds.push(selflist.RowId());
                                    var items = JSON.stringify(userIds);
                                    window.location.href = '../formsreportlist?userIds=' + items + '&StartDate=' + moment(startdate).format('MM/DD/YYYY') + '&EndDate=' + moment(enddate).add(1, 'days').format('MM/DD/YYYY');
                                };
                            }

                            if (Type == 'A' && dropdownvalueid == 'Tours') {
                                anchor.id = selectedcell;
                                selflist.ActivityModule = ko.observable(dropdownvalueid);
                                anchor.style.cursor = 'pointer';
                                anchor.setAttribute("Module", dropdownvalueid);
                                anchor.onclick = function () {
                                    var startdate = selflist.CustomStartDate();
                                    var enddate = selflist.CustomEndDate();
                                    selflist.CustomStartDate(moment(startdate).format('MM/DD/YYYY'));
                                    selflist.CustomEndDate(moment(enddate).add(1, 'days').format('MM/DD/YYYY'));
                                    selflist.RowId(parseInt(this.getAttribute("rowid")));
                                    selflist.ActivityModule(this.getAttribute("Module"));
                                    if (value != null && selectedcell != 0) {
                                        selflist.LeadSourceIds(parseInt(this.getAttribute("cellid")));
                                        if (Type == 'T')
                                            selflist.TourTypeIds(parseInt(this.getAttribute("cellid")));
                                        selflist.LifeStageIds(parseInt(this.getAttribute("cellid")));
                                    }
                                    var jsondata = ko.toJSON(selflist);
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
                                                window.location.href = '../reportcontacts?guid=' + response.response + '&reportType=' + selflist.ReportType() + '&reportId=' + selflist.ReportId();
                                            }
                                        }
                                    }).fail(function (error) {
                                        notifyError(error);
                                    });
                                };
                            }

                            if (Type == 'A' && dropdownvalueid == 'Notes') {
                                anchor.id = selectedcell;
                                selflist.ActivityModule = ko.observable(dropdownvalueid);
                                anchor.setAttribute("Module", dropdownvalueid);
                                anchor.style.cursor = 'pointer';
                                anchor.onclick = function () {
                                    var startdate = selflist.CustomStartDate();
                                    var enddate = selflist.CustomEndDate();
                                    selflist.CustomStartDate(moment(startdate).format('MM/DD/YYYY'));
                                    selflist.CustomEndDate(moment(enddate).add(1, 'days').format('MM/DD/YYYY'));

                                    selflist.RowId(parseInt(this.getAttribute("rowid")));
                                    selflist.ActivityModule(this.getAttribute("Module"));
                                    if (value != null && selectedcell != 0) {
                                        selflist.LeadSourceIds(parseInt(this.getAttribute("cellid")));
                                        if (Type == 'T')
                                            selflist.TourTypeIds(parseInt(this.getAttribute("cellid")));
                                        selflist.LifeStageIds(parseInt(this.getAttribute("cellid")));
                                    }
                                    var jsondata = ko.toJSON(selflist);
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
                                                window.location.href = '../reportcontacts?guid=' + response.response + '&reportType=' + selflist.ReportType() + '&reportId=' + selflist.ReportId();
                                            }
                                        }
                                    }).fail(function (error) {
                                        notifyError(error);
                                    });
                                };
                            }

                            if ((Type == 'A' && dropdownvalueid == 'Contacts') || Type != 'A') {
                                anchor.id = selectedcell;
                                anchor.setAttribute("cellid", selectedcell);
                                anchor.setAttribute("Module", dropdownvalueid);
                                selflist.ActivityModule = ko.observable(dropdownvalueid);
                                anchor.style.cursor = 'pointer';
                                anchor.onclick = function () {
                                    selflist.RowId(parseInt(this.getAttribute("rowid")));
                                    selflist.ActivityModule(this.getAttribute("Module"))
                                    if (value != null && selectedcell != 0) {
                                        selflist.LeadSourceIds(parseInt(this.getAttribute("cellid")));
                                        selflist.OpportunityStageIds(parseInt(this.getAttribute("cellid")));
                                        if (Type == 'T')
                                            selflist.TourTypeIds(parseInt(this.getAttribute("cellid")));
                                        selflist.LifeStageIds(parseInt(this.getAttribute("cellid")));
                                    }
                                    var jsondata = ko.toJSON(selflist);

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
                                                window.location.href = '../reportcontacts?guid=' + response.response + '&reportType=' + selflist.ReportType() + '&reportId=' + selflist.ReportId();
                                            }
                                        }
                                    }).fail(function (error) {
                                        notifyError(error);
                                    });
                                };
                            }

                            if (Type == 'A' && dropdownvalueid != 'Actions' && dropdownvalueid != 'Contacts' && dropdownvalueid != 'Opportunity' && dropdownvalueid != 'Campaigns' &&
                                dropdownvalueid != 'Forms' && dropdownvalueid != 'Tours' && dropdownvalueid != 'Notes') {
                                anchor = document.createElement("span");
                            }
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
                fileName: filename + ".xlsx",
                allPages: true,
                //for Exporting the file in IE9 and Safari Browsers
                proxyURL: "/Reports/ExportToExcel",
                forceProxy: true
            },
        }

        var timeEditGrid = $("#grid").kendoGrid(configuration).data("kendoGrid");
        var grid = $("#grid").data("kendoGrid");

        grid.dataSource.query({ page: 1, pageSize: parseInt(itemsPerPage) });
        kendo.bind($('#resultsgrid'), viewModel);

        $(".k-grid-excel").addClass("cu-grid-excel");
    }

    selflist.errors = ko.validation.group(selflist, true);

    selflist.Runlist = function () {

        selflist.errors.showAllMessages();

        if (selflist.errors().length > 0) {
            notifyError("[|Please select valid Date Range.|]");
            return;
        }
        if (runReport == 'True') {
            var tourTypefilter = [];
            var tourSourcefilter = [];
            var opportunityfilter = [];
            //  console.log(readCookie("TourByTypeDefaultFilter"));
            if (readCookie("TourByTypeDefaultFilter") != null && readCookie("TourByTypeDefaultFilter") != undefined) {
                tourTypefilter.push(readCookie("TourByTypeDefaultFilter"));
                selflist.TourType(tourTypefilter);
                eraseCookie("TourByTypeDefaultFilter");
            }
            if (readCookie("TourBySourceDefaultFilter") != null && readCookie("TourBySourceDefaultFilter") != undefined) {
                tourSourcefilter.push(readCookie("TourBySourceDefaultFilter"));
                selflist.LeadSource(tourSourcefilter);
                eraseCookie("TourBySourceDefaultFilter");
            }
            if (readCookie("OpportunityDefaultFilter") != null && readCookie("OpportunityDefaultFilter") != undefined) {
                opportunityfilter.push(readCookie("OpportunityDefaultFilter"));
                selflist.OpportunityStage(opportunityfilter);
                eraseCookie("OpportunityDefaultFilter");

            }
        }
        ChartData = [];
        funnel_index = 0;
        pageLoader();
        var selectedOwners = [];
        var selectedCommunities = [];
        var selectedlifecycles = [];
        var selectedTypes = [];
        var selectedLeadSources = [];
        var selectedModules = [];
        var selectOpportunityStages = [];

        if (selflist.LifecycleStage() == 0) {
            for (var l = 0; l < (lifecycles).length; l++) {
                selectedlifecycles.push((lifecycles)[l].DropdownValueID);
            }
        }
        else
            selectedlifecycles = (selflist.LifecycleStage()).length > 0 ? (selflist.LifecycleStage()).map(Number) : [];

        if (selflist.OpportunityStage() == 0) {
            for (var l = 0; l < (selflist.OpportunityStages()).length; l++) {
                selectOpportunityStages.push((selflist.OpportunityStages())[l].DropdownValueID);
            }
        }
        else
            selectOpportunityStages = (selflist.OpportunityStage()).length > 0 ? (selflist.OpportunityStage()).map(Number) : [];

        if (selflist.OwnerId() == 0) {
                for (var l = 0; l < (users).length; l++) {
                    selectedOwners.push((users)[l].OwnerId);
                }
           
        }
        else if (selflist.OwnerId() == data.OwnerId && Type != 'A') {
            if (Array.isArray(selflist.OwnerId()))
                selectedOwners = (selflist.OwnerId()).length > 0 ? (selflist.OwnerId()).map(Number) : [];
            else
                selectedOwners.push(selflist.OwnerId());
        }

        else {
            selectedOwners = (selflist.OwnerId()).length > 0 ? (selflist.OwnerId()).map(Number) : [];
        }

        if (selflist.Community() == 0) {
            for (var l = 0; l < (selflist.Communities()).length; l++) {
                selectedCommunities.push((selflist.Communities())[l].DropdownValueID);
            }
        }
        else
            selectedCommunities = (selflist.Community()).length > 0 ? (selflist.Community()).map(Number) : [];

        if (selflist.TourType() == 0) {
            for (var l = 0; l < (selflist.TourTypes()).length; l++) {
                selectedTypes.push((selflist.TourTypes())[l].DropdownValueID);
            }
        }
        else
            selectedTypes = (selflist.TourType()).length > 0 ? (selflist.TourType()).map(Number) : [];

        if (selflist.LeadSource() == 0) {
            for (var l = 0; l < (selflist.LeadSources()).length; l++) {
                selectedLeadSources.push((selflist.LeadSources())[l].DropdownValueID);
            }
        }
        else
            selectedLeadSources = (selflist.LeadSource()).length > 0 ? (selflist.LeadSource()).map(Number) : [];


        if (selflist.Module().length == 0) {
            selectedModules.push(0);
        }
        else {
            selectedModules = (selflist.Module()).length > 0 ? (selflist.Module()).map(Number) : [];
        }


        selflist.LifeStageIds(selectedlifecycles);
        selflist.OwnerIds(selectedOwners);
        selflist.CommunityIds(selectedCommunities);
        selflist.TourTypeIds(selectedTypes);
        selflist.LeadSourceIds(selectedLeadSources);
        selflist.ModuleIds(selectedModules);
        selflist.OpportunityStageIds(selectOpportunityStages);

        // This code is used to get the previous perid

        var oneDay = 24 * 60 * 60 * 1000;
        var firstDate = new Date(selflist.CustomStartDate());
        var secondDate = new Date(selflist.CustomEndDate());
        var diffDays = Math.round(Math.abs((firstDate.getTime() - secondDate.getTime()) / (oneDay)));


        if (selflist.PreviousId() == 1) {
            selflist.CustomStartDatePrev(moment().subtract(diffDays * 2, 'days').format());
            selflist.CustomEndDatePrev(moment().subtract(diffDays, 'days').format());
        }
        else if (selflist.PreviousId() == 2) {
            selflist.CustomStartDatePrev(moment(firstDate).subtract(1, 'years').format());
            selflist.CustomEndDatePrev(moment(secondDate).subtract(1, 'years').format());
        }

        if (diffDays < 60)
            selflist.DateRange('D');
        else if (diffDays > 60 && diffDays < 180)
            selflist.DateRange('W');
        else if (diffDays > 180)
            selflist.DateRange('M');

        var jsondata = ko.toJSON(selflist);
        $.ajax({
            url: url + '/GetReportData',
            type: 'post',
            data: jsondata,
            dataType: 'json',
            contentType: "application/json; charset=utf-8"
            //success: function (response) {           
            //    CreateGrid(response.GridData);
            //    var grid = $("#grid").data("kendoGrid");
            //    CreateChart(ChartData);
            //    CreatePieChart(response.PieChartData);
            //    CreateAreaChart(response.AreaChartData);
            //    CreateBarChart(response.PieChartData);
            //    removepageloader();
            //},
            //error: function (response) {
            //}
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            } return filter.promise()
        }).done(function (data) {
            var resultData = data.response;
            if (resultData.GridData.length > 0) {
                CreateGrid(resultData.GridData);
                $("#resultsgrid").show();
                $("#noRecordsFound").hide();
            }
            else {
                $("#resultsgrid").hide();
                $("#noRecordsFound").show();
            }
            //  var grid = $("#grid").data("kendoGrid");
            console.log(resultData.DashboardPieCharData)
            CreateChart(resultData.DashboardPieCharData);
            CreatePieChart(resultData.PieChartData);
            CreateAreaChart(resultData.AreaChartData);
            CreateBarChart(resultData.PieChartData);
            removepageloader();
        }).fail(function (error) {
            notifyError(error);
        });
    }
    if (runReport == 'True')
        selflist.Runlist();
}