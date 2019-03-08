var newLeadreportViewModel = function (url, data, users, dateFormat, runReport, itemsPerPage) {
    selfNewLeads = this;

    ko.validation.configure({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null
    });

    selfNewLeads.OwnerId = ko.observable(0);
    selfNewLeads.OwnerIds = ko.observableArray([]);
    selfNewLeads.gridvisible = ko.observable("False");

    selfNewLeads.Users = ko.observableArray(users);

    selfNewLeads.templ = kendo.template('#if (data.OwnerId == 0 )  { # <span>[|Select..|]</span># } ##if (data.IsDeleted == false )  { # <span>#= OwnerName #</span># } # #if (data.IsDeleted == true )  { # <span style="color:red;">#= OwnerName #</span># } #');

    selfNewLeads.SearchResults = ko.observableArray();
    
    selfNewLeads.TotalHits = ko.observable(0);
    selfNewLeads.NoOfHits = ko.observable(0);
    selfNewLeads.PageNumber = ko.observable();
    selfNewLeads.ShowTop = ko.observable(data.ShowTop);
    selfNewLeads.ReportId = ko.observable(data.ReportId);
    selfNewLeads.ReportName = ko.observable(data.ReportName);
    selfNewLeads.CustomPredicateScript = ko.observable("");
    selfNewLeads.Filters = ko.observableArray([]);
    selfNewLeads.PeriodId = ko.observable(2);
    selfNewLeads.SearchResults = ko.observableArray([]);
    selfNewLeads.PreviousDates = ko.observableArray([]);
    selfNewLeads.TotalNewLeads = ko.observable(0);
    selfNewLeads.TotalPreviousNewLeads = ko.observable(0);
    selfNewLeads.PreviousPeriodId = ko.observable(1); 
    selfNewLeads.IsCompared = ko.observable(false);
    selfNewLeads.ComparedToPreviousDate = ko.observable(moment().subtract(60, 'days').format());
    selfNewLeads.ComparedToPreviousText = ko.observable("Previous 30 Days");
    selfNewLeads.DateRange = ko.observable('D');
    selfNewLeads.CustomStartDatePrev = ko.observable();
    selfNewLeads.CustomEndDatePrev = ko.observable();
    selfNewLeads.PreviousLeads = ko.observable();
    selfNewLeads.PreviousDisplayText = ko.observable();
    selfNewLeads.IsDashboard = ko.observable(false);
    selfNewLeads.LeadSources = ko.observableArray(data.LeadSources);

    selfNewLeads.LeadSourceIds = ko.observableArray([]);
    selfNewLeads.LeadSource = ko.observable(0);
    // var LeadSources = data.LeadSources;
    var today = new Date();

    var leadSources = selfNewLeads.LeadSources();

    selfNewLeads.Periods = ko.observableArray([
    { PeriodText: '[|Last 7 Days|]', PeriodId: 1, Period: moment().subtract(7, 'days').format(), PreviousPeriod: moment().subtract(14, 'days').format(), PreviousText: '[|Previous 7 Days|]' },
    { PeriodText: '[|Last 30 Days|]', PeriodId: 2, Period: moment().subtract(30, 'days').format(), PreviousPeriod: moment().subtract(60, 'days').format(), PreviousText: '[|Previous 30 Days|]' },
    { PeriodText: '[|Last 3 Months|]', PeriodId: 3, Period: moment().subtract(3, 'months').format(), PreviousPeriod: moment().subtract(6, 'months').format(), PreviousText: '[|Previous 3 Months|]' },
    { PeriodText: '[|Month to Date|]', PeriodId: 4, Period: moment().startOf('month').format(), PreviousPeriod: moment().startOf('month').subtract(1, 'monhts').format(), PreviousText: '[|Previous Month to Date|]' },
    { PeriodText: '[|Year to Date|]', PeriodId: 5, Period: moment().startOf('year').format(), PreviousPeriod: moment().startOf('year').subtract(1, 'years').format(), PreviousText: '[|Previous Year to Date|]' },
    { PeriodText: '[|Last Year|]', PeriodId: 6, Period: moment().subtract(1, 'years').format(), PreviousPeriod: moment().subtract(2, 'years').format(), PreviousText: '[|Previous Year|]' },
    { PeriodText: '[|Custom|]', PeriodId: 7, Period: moment(new Date()).format(), PreviousPeriod: moment(new Date()).format(), PreviousText: '[|Previous Date|]' }
    ]);
    selfNewLeads.ComparedTo = ko.observableArray([
        { Text: "[|Previous Period|]", PreviousPeriodId: 1 },
        { Text: "[|Previous Year|]", PreviousPeriodId: 2 }
    ]);

    selfNewLeads.setCompareTo = function (data, event) {
        selfNewLeads.IsCompared(event.target.checked);

    }

    selfNewLeads.PeriodId.subscribe(function (newValue) {
        if (newValue != 7) {
            selfNewLeads.CustomEndDate(moment(new Date()).format());
            var tempPreviousPeriod = ko.utils.arrayFirst(selfNewLeads.Periods(), function (type) {
                return type.PeriodId == newValue;
            });
            selfNewLeads.CustomStartDate(tempPreviousPeriod.Period);
            selfNewLeads.ComparedToPreviousDate(tempPreviousPeriod.PreviousPeriod);
            selfNewLeads.ComparedToPreviousText(tempPreviousPeriod.PreviousText);
        }
        else
        {
            selfNewLeads.ComparedToPreviousText("[|Previous Period|]");
        }
    });

    selfNewLeads.GetPhoneNumber = function (phones) {
        var number = "";
        if (phones == null)
            return number;
        else {
            var phoneNumbers = "";
            if (phones.length > 0)
                phoneNumbers = ko.utils.arrayMap(phones, function (phone) {
                    if (phone.IsPrimary == true)
                        return phone.Number + " *";
                    else
                        return phone.Number;
                }).join();
            number = phoneNumbers;
        }
        return number;
    }
    selfNewLeads.getEmail = function (fullname, primaryemail) {

        var name = "";
        var email = "";
        name = encodeURIComponent(fullname);
        email = encodeURIComponent(primaryemail);
        return "contact/_ContactSendMailModel?contactName=" + name + "&emailID=" + email;
    };
    var toStartDate = new Date();
    toStartDate.setDate(toStartDate.getDate());
    selfNewLeads.CustomEndDate = ko.observable(moment(new Date()).format()).extend({ required: { message: "" } });

    selfNewLeads.CustomStartDate = ko.observable(moment().subtract(30, 'days').format()).extend({ required: { message: "" } });
    var maxdate = new Date();
    maxdate.setDate(maxdate.getDate() + 1);
    selfNewLeads.toMaxDate = ko.observable(maxdate);
    var fromMaxDate = new Date();
    fromMaxDate.setDate(fromMaxDate.getDate() - 1);
    selfNewLeads.fromMaxDate = ko.observable(fromMaxDate);

    selfNewLeads.DateFormat = ko.observable(dateFormat);

    selfNewLeads.fromDateChangeEvent = function () {
        var fromDate = moment(moment(this.value()).format("YYYY-MM-DD"));
        var toDate = moment(moment(selfNewLeads.CustomEndDate()).format("YYYY-MM-DD"));
        var differenceOfDays = fromDate.diff(toDate, 'days');
        selfNewLeads.ComparedToPreviousDate(fromDate.subtract(differenceOfDays, 'days').format("DD/MM/YYYY"));
        selfNewLeads.CustomStartDate(moment(this.value()).format());

        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }

    selfNewLeads.toDateChangeEvent = function () {
        var fromDate = moment(moment(selfNewLeads.CustomStartDate()).format("YYYY-MM-DD"));
        var toDate = moment(moment(this.value()).format("YYYY-MM-DD"));
        var differenceOfDays = fromDate.diff(toDate, 'days');
        selfNewLeads.ComparedToPreviousDate(fromDate.subtract(differenceOfDays, 'days').format("DD/MM/YYYY"));
        selfNewLeads.CustomEndDate(moment(this.value()).format());
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }

    selfNewLeads.periodChange = function () {
        selfNewLeads.toMaxDate(maxdate);
        var value = this.value();
        if (parseInt(value) == 7) {
            var toStartDate = new Date();
            toStartDate.setDate(toStartDate.getDate());
            selfNewLeads.CustomEndDate(toStartDate);
            var fromdate = new Date();
            fromdate.setDate(fromdate.getDate() - 7);
            selfNewLeads.CustomStartDate(fromdate);
            selfNewLeads.toMaxDate(new Date());
        }
    }
    selfNewLeads.CustomDateDisplay = ko.pureComputed(function () {

        if (selfNewLeads.PeriodId() == 7) {
            return true;
        } else {
            return false;
        }
    });
    selfNewLeads.errors = ko.validation.group(selfNewLeads, true);
    var selectedLeadSources = [];
    if (readCookie("NeWContactseDefaultFilter") != null && readCookie("NeWContactseDefaultFilter") != undefined) {
        selectedLeadSources.push(readCookie("NeWContactseDefaultFilter"));
        selfNewLeads.LeadSource(selectedLeadSources);
        eraseCookie("NeWContactseDefaultFilter");
       // selfNewLeads.IsDashboard(false);
    }
    //else {
    //    selfNewLeads.IsDashboard(true);
    //}

    selfNewLeads.Runlist = function () {

        var selectedleadsouces = [];
        var selectedOwnerIds = [];

      
        selfNewLeads.EnableVisuvalizations = ko.observable(true);
        selfNewLeads.reportCriteria();
        selfNewLeads.errors.showAllMessages();

        if (selfNewLeads.errors().length > 0) {
            notifyError("[|Please select valid Date Range.|]");
            return;
        }
        pageLoader();


        var oneDay = 24 * 60 * 60 * 1000;
        var firstDate = new Date(selfNewLeads.CustomStartDate());
        var secondDate = new Date(selfNewLeads.CustomEndDate());
        var diffDays = Math.round(Math.abs((firstDate.getTime() - secondDate.getTime()) / (oneDay)));


        if (selfNewLeads.PreviousPeriodId() == 1) {
            selfNewLeads.CustomStartDatePrev(moment().subtract(diffDays * 2, 'days').format());
            selfNewLeads.CustomEndDatePrev(moment().subtract(diffDays, 'days').format());
        }
        else if (selfNewLeads.PreviousPeriodId() == 2) {
            selfNewLeads.CustomStartDatePrev(moment(firstDate).subtract(1, 'years').format());
            selfNewLeads.CustomEndDatePrev(moment(secondDate).subtract(1, 'years').format());
        }
        selfNewLeads.PreviousDisplayText(selfNewLeads.ComparedToPreviousText());
        if (diffDays < 60)
            selfNewLeads.DateRange('D');
        else if (diffDays > 60 && diffDays < 180)
            selfNewLeads.DateRange('W');
        else if (diffDays > 180)
            selfNewLeads.DateRange('M');
        //  selfNewLeads.gridvisible('True');
    
        

        if (selfNewLeads.LeadSource() == 0 || selfNewLeads.LeadSource().length == 0) {
            //   for (var l = 0; l < (selfNewLeads.LeadSources()).length; l++) {
            //    selectedleadsouces.push((selfNewLeads.LeadSources())[l].DropdownValueID);
            //}
        }
        else
            selectedleadsouces = (selfNewLeads.LeadSource()).length > 0 ? (selfNewLeads.LeadSource()).map(Number) : [];

        selfNewLeads.LeadSourceIds(selectedleadsouces);

        var grid = $('#NewLeadsGrid').data("kendoGrid");
        var newdataSource = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    $.ajax({
                        url: '/Reports/NewLeadsList',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            ShowTop: $('#NewLeadsGrid').data("kendoGrid").dataSource.pageSize(), Filters: selfNewLeads.Filters(), CustomStartDate: selfNewLeads.CustomStartDate(),
                            CustomEndDate: selfNewLeads.CustomEndDate(), PageNumber: $('#NewLeadsGrid').data("kendoGrid").dataSource.page(), CustomPredicateScript: selfNewLeads.CustomPredicateScript(),
                            PreviousPeriodId: selfNewLeads.PreviousPeriodId(), ReportId: selfNewLeads.ReportId(), PeriodId: selfNewLeads.PeriodId(), LeadSourceIds: selfNewLeads.LeadSourceIds(), IsDashboard: selfNewLeads.IsDashboard(),
                            ReportName: selfNewLeads.ReportName(),LeadSource:selfNewLeads.LeadSource(), DateRange: selfNewLeads.DateRange(), CustomStartDatePrev: selfNewLeads.CustomStartDatePrev(), CustomEndDatePrev: selfNewLeads.CustomEndDatePrev()
                        }),
                        type: 'post'
                    }).then(function (response) {
                        var filter = $.Deferred()
                        if (response.success) {
                            filter.resolve(response)
                        } else {
                            filter.reject(response.error)
                        } return filter.promise()
                    }).done(function (data) {

                        var result = data.response;
                       
                        selfNewLeads.TotalHits(data.response.Total);
                        options.success(result);
                        selfNewLeads.Visuvalizations();
                        //selfNewLeads.EnableVisuvalizations(false)
                    }).fail(function (error) {

                        notifyError(error);
                    });
                },
            },
            serverPaging: true,
            schema: {
                data: "Data",
                total: "Total"
            },
            pageable: {
                pageSizes: ToPageSizeArray(),
                messages: {
                    display: "[|Showing|] {0}-{1} [|from|] {2} [|Contact(s)|]"
                },
            }           
           // serverPaging: true,
           // pageSize: 10
        });
     
    
     
        grid.setDataSource(newdataSource);
        grid.dataSource.query({ page: 1, pageSize: itemsPerPage });

    

    }



    selfNewLeads.ExportNewLeads = function () {       
       
        var ds = new kendo.data.DataSource({           
            transport: {
                read: function (options) {
                    $.ajax({
                        url: '/Reports/NewLeadsList',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            ShowTop: 10000, Filters: selfNewLeads.Filters(), CustomStartDate: selfNewLeads.CustomStartDate(),
                            CustomEndDate: selfNewLeads.CustomEndDate(), PageNumber: 1, CustomPredicateScript: selfNewLeads.CustomPredicateScript(),
                            PreviousPeriodId: selfNewLeads.PreviousPeriodId(), ReportId: selfNewLeads.ReportId(), PeriodId: selfNewLeads.PeriodId(), LeadSourceIds: selfNewLeads.LeadSourceIds(), IsDashboard: selfNewLeads.IsDashboard(),
                            ReportName: selfNewLeads.ReportName(), LeadSource: selfNewLeads.LeadSource(), DateRange: selfNewLeads.DateRange(), CustomStartDatePrev: selfNewLeads.CustomStartDatePrev(), CustomEndDatePrev: selfNewLeads.CustomEndDatePrev()
                        }),
                        type: 'post'
                    }).then(function (response) {
                        var filter = $.Deferred()
                        if (response.success) {
                            filter.resolve(response)
                        } else {
                            filter.reject(response.error)
                        } return filter.promise()
                    }).done(function (data) {

                        options.success(data.response.Data);

                    });
                }
            },
            schema: {
                model: {
                    fields: {
                        Name: { type: "string" },
                        Phone: { type: "string" },
                        PrimaryEmail: { type: "string" },
                        LeadSources: { type: "string" },
                        LifecycleName: { type: "string" },
                        CreatedOn: { type: "string"},
                        OwnerName: { type: "string" }
                    }
                }
            }
        });

        var rows = [{
            cells: [            
              { value: "Contact Name" },
              { value: "Phone" },
              { value: "Primary Email" },
              { value: "Lead Source" },
              { value: "Life Cycle" },
              { value: "Created" },
              { value: "Account Executive" }
            ]
        }];
        function displayDate(date) {
            if (date == null) {
                return "";
            }
            return kendo.toString(kendo.parseDate(date, 'yyyy/MM/dd'), dateFormat);
        }
        //using fetch, so we can process the data when the request is successfully completed
        ds.fetch(function () {
            var data = this.data();
           
            for (var i = 0; i < data.length; i++) {
                //push single row for every record
                rows.push({
                    cells: [
                      { value: data[i].Name },
                      { value: data[i].Phone },
                      { value: data[i].PrimaryEmail },
                      { value: data[i].LeadSources },
                      { value: data[i].LifecycleName },
                      { value: displayDate(data[i].CreatedOn) },
                      { value: data[i].OwnerName }
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
                fileName: "NewContacts.xlsx",
                //for Exporting the file in IE9 and Safari Browsers
                proxyURL: "/Reports/ExportToExcel",
                forceProxy: true
            });
        });

    }



    selfNewLeads.viewContacts = function () {
        var contacts_Guid = null;
        var reportType=3
        window.location.href = '../ncrcontacts?guid=' + contacts_Guid + '&reportType=' + reportType + '&reportId=' + selfNewLeads.ReportId() + '&reportName=' + selfNewLeads.ReportName();
    }

   

    selfNewLeads.reportCriteria = function () {

        var filters = [];
        var script = "";
        var ownerids = selfNewLeads.OwnerId();
        var owners = ownerids;

    


        if (selfNewLeads.OwnerId() != 0 && selfNewLeads.OwnerId() != undefined && selfNewLeads.OwnerId() != []) {
            script += "(";
            for (var p = 0; p < owners.length; p++) {
                filters.push({
                    Field: 'Owner', Qualifier: "Is", SearchText: owners[p],
                })
                if (p == owners.length - 1)
                    script += " " + filters.length + ")";
                else
                    script += " " + filters.length + ' OR';
            }
            selfNewLeads.Filters(filters);
            selfNewLeads.CustomPredicateScript(script);
        }
        else
        {
            selfNewLeads.Filters(filters);
            selfNewLeads.CustomPredicateScript(script);
        }
        //else {
        //    owners = selfNewLeads.Users();
        //    script += "(";
        //    filters.push({
        //        Field: 'Owner', Qualifier: "IsNotEmpty", SearchText: ""
        //    })
        //    script += "  " + filters.length + " )";

        //}
        //script += "AND (";
        //filters.push({
        //    Field: 'LifecycleStageField', Qualifier: "Is", SearchText: lyfecyclestageids
        //})
        // script += "  " + filters.length + " )";

        var results = selfNewLeads.SearchResults();
        selfNewLeads.SearchResults([]);
        var users = selfNewLeads.Users();
        selfNewLeads.Users([]);
        var periods = selfNewLeads.Periods();
        selfNewLeads.Periods([]);


        var jsondata = ko.toJSON(selfNewLeads);

        selfNewLeads.SearchResults(results);
        selfNewLeads.Users(users);
        selfNewLeads.Periods(periods);

        return jsondata;
    }

    var newLeadsCount = function (newLeads, previousLeads) {
        var newLeadsSum = 0;
        var totalNewLeads = ko.utils.arrayFirst(newLeads, function (type) {
            newLeadsSum += type.value;

        });
        selfNewLeads.totalNewLeads(newLeadsSum);
        var percentage = 0;
        if (parseInt(previousLeads) > 0 && parseInt(newLeadsSum) > 0) {
            if (parseInt(previousLeads) > parseInt(newLeadsSum))
                percentage = Math.round((parseInt(previousLeads) - parseInt(newLeadsSum) / parseInt(newLeadsSum)) * 100);
            if (parseInt(newLeadsSum) > parseInt(previousLeads))
                percentage = Math.round((parseInt(newLeadsSum) - parseInt(previousLeads) / parseInt(newLeadsSum)) * 100);
        }
        else if (parseInt(previousLeads) == 0)
            percentage = 100;
        selfNewLeads.PreviousLeads(Math.round(percentage));
    }
    var applyVisualizationToAreaChart = function (results,previousResults) {
          
        var newLeadsSum = 0;
        var previousNewLeadsSum = 0;
       
        var totalNewLeads = ko.utils.arrayFirst(results, function (type) {
            newLeadsSum += type.Value;
        });
      
       
        var previousNewLeads = ko.utils.arrayFirst(previousResults, function (type) {
            previousNewLeadsSum += type.Value;
        });
      
        var percentage = 0;
        if (parseInt(previousNewLeadsSum) > 0 && parseInt(newLeadsSum) > 0) {
            if (newLeadsSum < previousNewLeadsSum)
                percentage = Math.round(((parseInt(previousNewLeadsSum) - parseInt(newLeadsSum)) / parseInt(previousNewLeadsSum)) * 100);
            else
                percentage = Math.round(((parseInt(newLeadsSum) - parseInt(previousNewLeadsSum)) / parseInt(newLeadsSum)) * 100);
        }
        else if (parseInt(previousNewLeadsSum) == 0)
            percentage = 100;
        selfNewLeads.TotalPreviousNewLeads(previousNewLeadsSum);
        // selfDashBoard.NewLeadsPreviousCount(previousCount);
        selfNewLeads.PreviousLeads(Math.round(percentage));
        selfNewLeads.TotalNewLeads(newLeadsSum);
        if ((results.length > previousResults.length) || (results.length = previousResults.length)) {
            $.each(previousResults, function (index, previous_Value) {
                results[index].PreviousValue = previous_Value.Value;
            });
            selfNewLeads.AreaChartDetails = ko.observableArray(results);
            applyDataToAreaChart_1(results);
        }
        else if ((results.length < previousResults.length)) {
            $.each(results, function (index, previous_Value) {

                previousResults[index].PreviousValue = previous_Value.Value;
            });
            selfNewLeads.AreaChartDetails = ko.observableArray(previousResults);
            applyDataToAreaChart_2(previousResults);
        }
       
    }
    var applyDataToAreaChart_1 = function (results)
    {

        $("#danewleads").kendoChart({
            legend: {
                position: "bottom",
                visible: false
            },
            dataSource: {
                data: results
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
                field: "Value",
                categoryField: "Name"

            }, {
                name: "Previous",
                field: "PreviousValue",
                categoryField: "Name"

            }],
            valueAxis: {
                labels: {
                    visible: false,
                    format: "{0}%"
                },
                line: {
                    visible: true
                },
                majorGridLines: {
                    visible: false
                },
                visible: false
            },
            categoryAxis: {
                // categories: [2002, 2003, 2004, 2005, 2006, 2007, 2008, 2009, 2010, 2011],
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
    var applyDataToAreaChart_2 = function (results) {

        $("#danewleads").kendoChart({
            legend: {
                position: "bottom",
                visible: false
            },
            dataSource: {
                data: results
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
                field: "PreviousValue",
                categoryField: "Name"

            }, {
                name: "Previous",
                field: "Value",
                categoryField: "Name"

            }],
            valueAxis: {
                labels: {
                    visible: false,
                    format: "{0}%"
                },
                line: {
                    visible: true
                },
                majorGridLines: {
                    visible: false
                },
                visible: false
            },
            categoryAxis: {
                // categories: [2002, 2003, 2004, 2005, 2006, 2007, 2008, 2009, 2010, 2011],
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
    var applyVisualizations = function (results) {
     
        $("#datopleadsource").kendoChart({
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
                data: results
            },
            series: [{
                type: "pie",
                field: "Value",
                startAngle: 150,
                categoryField: "Name",
            }],
            seriesColors: ["#ef7373", "#e6e473", "#79d082", "#efa76d", "#5cc0f4"],
            tooltip: {
                visible: true,
                template: "${category}- ${ value }"
            },

        });
    }
    selfNewLeads.Visuvalizations = function () {
        
        if (parseInt(selfNewLeads.TotalHits()) > 0)
            selfNewLeads.ShowTop(selfNewLeads.TotalHits());
        var selectedOwners = [];
        if (selfNewLeads.OwnerId() == 0 || selfNewLeads.OwnerId() == []) {
            for (var i = 0; i < (users).length; i++) {
                selectedOwners.push((users)[i].OwnerId);
            }
        }
        else
            selectedOwners = (selfNewLeads.OwnerId()).length > 0 ? (selfNewLeads.OwnerId()).map(Number) : [];
        selfNewLeads.OwnerIds(selectedOwners);


        var jsondata = ko.toJSON(selfNewLeads);
       
        $.ajax({
            url: '/Reports/NewContactsVisualization',
            type: 'post',
            data: jsondata,
            dataType: 'json',
            contentType: "application/json; charset=utf-8"

        }).then(function (response) {
            removepageloader();
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            }
            else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {

            var resultData = data.response;
            console.log(resultData);
            applyVisualizations(resultData.TopFive);
            applyVisualizationToAreaChart(resultData.TopLeads, resultData.TopPreviousLeads);
            selfNewLeads.gridvisible('True');
             //selfNewLeads.IsDashboard(false);
            removepageloader();


        }).fail(function (error) {
            removepageloader();
            notifyError(error);

        })
    }
    if (runReport == 'True')
        selfNewLeads.Runlist();


}