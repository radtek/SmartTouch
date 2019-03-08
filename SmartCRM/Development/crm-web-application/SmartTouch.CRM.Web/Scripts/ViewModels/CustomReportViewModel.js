var customReportViewModel = function ( data, url, dateFormat, itemsPerPage) {
    var selflist = this;

    selflist.ReportId = ko.observable(data.ReportId);
    selflist.ReportName = ko.observable(data.ReportName);
    selflist.ReportType = ko.observable(data.ReportType);

    selflist.Periods = ko.observableArray([
    { PeriodId: 1, Period: '[|Last 7 Days|]' },
    { PeriodId: 2, Period: '[|Last 30 Days|]' },
    { PeriodId: 3, Period: '[|Last 3 Months|]' },
    { PeriodId: 4, Period: '[|Month to Date|]' },
    { PeriodId: 5, Period: '[|Year to Date|]' },
    { PeriodId: 6, Period: '[|Last Year|]' },
    { PeriodId: 7, Period: '[|Custom|]' }

    ]);


    var toStartDate = new Date();
    toStartDate.setDate(toStartDate.getDate());  
    selflist.CustomEndDate = ko.observable(moment(new Date()).format()).extend({ required: { message: "" } });;
    selflist.CustomEndDatePrev = ko.observable();

    var fromdate = new Date();
    fromdate.setMonth(fromdate.getMonth() - 1);
    selflist.CustomStartDate = ko.observable(kendo.toString(fromdate, dateFormat)).extend({ required: { message: "" } });;
    selflist.CustomStartDatePrev = ko.observable();

    selflist.PeriodId = ko.observable(2);

    toStartDate = new Date();
    selflist.CustomEndDate(moment(toStartDate).format());
    selflist.CustomStartDate(moment().subtract(30, 'days').format());


    selflist.periodChange = function () {
        var value = this.value();
       
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
         
            selflist.CustomStartDate(moment().subtract(1, 'years').format());
          
        }
        if (value == "7") { // custom

          
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

    var maxdate = new Date();
    maxdate.setDate(maxdate.getDate() + 1);
    selflist.toMaxDate = ko.observable(maxdate);
    var fromMaxDate = new Date();
    fromMaxDate.setDate(fromMaxDate.getDate() - 1);
    selflist.fromMaxDate = ko.observable(fromMaxDate);
    selflist.DateFormat = ko.observable(dateFormat);


    selflist.fromDateChangeEvent = function () {
        var fromDate = this.value();
        selflist.CustomStartDate(moment(this.value()).format());
        var toDate = selflist.CustomEndDate();
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }

    selflist.toDateChangeEvent = function () {
        var fromDate = selflist.CustomStartDate();
        var toDate = this.value();
        selflist.CustomEndDate(moment(this.value()).format());
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }
    selflist.errors = ko.validation.group(selflist, true);

    function CreateGrid(data) {
        if ($('#customgrid').data().kendoGrid != undefined) {
            $('#customgrid').data().kendoGrid.destroy();
            $(".k-grid-pager").remove();
            $(".k-grid-toolbar").remove();
            $('#customgrid').empty();
        }

        console.log("in create grid");
        console.log(data.length);

        var rows = [];
        for (var i = 0; i < data.length; i++) {
            var entryArray = [];

            for (var j = 0; j < data[i].CustomData.length; j++) {
                //console.log("in inner for each");
                //console.log(data[i].CustomData[j].ColumnValue);

                entryArray.push(data[i].CustomData[j].ColumnValue);
            }
            rows.push(kendo.observable({
                entries: entryArray
            }));
            //console.log("rows");
            //console.log(rows);
        }

        //console.log("in create grid");
        //console.log(rows);

        var gridviewModel = kendo.observable({
            gridRows: rows
        });

        //console.log("grid view model");
        //console.log(gridviewModel);

        var columns = [];
       // var entryIndex = "entries[" + 0 + "]";
        for (var i = 0; i < data[0].CustomData.length; i++) {
            var entryIndex = "entries[" + parseInt(i) + "]";
            columns.push({
                field: entryIndex,
                title: data[0].CustomData[i].ColumnName               
            });
        }

        var configuration = {
            scrollable: false,
            toolbar: [{ name: "excel", text: "Export to Excel" }],
            excel: {
                fileName: "CustomReport.xlsx",
                allPages: true
            },
            pageable: {
                pageSizes: ToPageSizeArray(),
                messages: {
                    display: "[|Showing|] {0}-{1} [|from|] {2} " + '[|Contacts|]'
                }
            },
            columns: columns,
            sortable: true          
        }

        //var timeEditGrid = $("#customgrid").kendoGrid(configuration).data("kendoGrid");
        var grid = $("#customgrid").data("kendoGrid");

        grid.dataSource.query({ page: 1, pageSize: parseInt(itemsPerPage) });
        kendo.bind($('#resultsgrid'), gridviewModel);
    
        $("#customgrid").wrap("<div class='st-responsive-grid'></div>");

    }


    selflist.Runlist = function () {
        selflist.errors.showAllMessages();

        if (selflist.errors().length > 0) {
            notifyError("[|Please select valid Date Range.|]");
            return;
        }
        pageLoader();
        var jsondata = ko.toJSON(selflist);

        console.log("in run report");

       // selflist.CustomStartDate(kendo.toString()

        console.log(selflist.CustomStartDate());
        console.log(selflist.CustomEndDate());

        //console.log(kendo.toString(selflist.CustomStartDate(), 'd'));
        //var dt = selflist.CustomEndDate().addHours(23).addMinutes(59);
        //console.log(dt);

       // console.log("in runreport");
        $.ajax({
            url: url + '/GetCustomReportData',
            type: 'post',
            data: jsondata,
            dataType: 'json',
            contentType: "application/json; charset=utf-8"
           
        }).then(function (response) {
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response);
            } else {
                filter.reject(response.error);
            }
            return filter.promise();
        }).done(function (data) {
            //var arr_from_json = JSON.parse(data);
            //console.log("in sucesss");
            console.log(data.response);

            if (data.response.length > 0) {
                CreateGrid(data.response);
                $("#resultsgrid").show();
                $("#noRecordsFound").hide();
            }
            else {
                $("#resultsgrid").hide();
                $("#noRecordsFound").show();
            }
          

            removepageloader();
        }).fail(function (error) {
            notifyError(error);
        });


    }

}