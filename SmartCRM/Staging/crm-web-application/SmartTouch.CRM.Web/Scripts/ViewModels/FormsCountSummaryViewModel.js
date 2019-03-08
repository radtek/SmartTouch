var formsCountSummaryViewModel = function (BASE_URL, data, dateFormat, itemsPerPage, runReport) {

    selfReport = this;
    ko.validation.configure({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null
    });
    var maxdate = new Date();
    maxdate.setDate(maxdate.getDate() + 1);
    selfReport.ReportId = ko.observable(data.ReportId);
    selfReport.ReportName = ko.observable(data.ReportName);
    selfReport.ReportType = ko.observable(data.ReportType);
    selfReport.FormsVisible = ko.observable("True");
    selfReport.LeadSourceVisible = ko.observable("False");
    selfReport.PeriodId = ko.observable(2);
    selfReport.LeadSource = ko.observableArray(data.LeadSources);
    selfReport.CustomEndDate = ko.observable(moment(new Date()).format()).extend({ required: { message: "" } });
    selfReport.CustomStartDate = ko.observable(moment().subtract(30, 'days').format()).extend({ required: { message: "" } });
    selfReport.GroupId = ko.observable(0);
    selfReport.FormIds = ko.observableArray([]);
    selfReport.FormId = ko.observableArray(0);
    selfReport.LeadAdapterIds = ko.observableArray([]);
    selfReport.LeadAdapterId = ko.observable(0);
    selfReport.gridvisible = ko.observable(false);
    selfReport.results = ko.observableArray();

    selfReport.RowId = ko.observable();

    selfReport.Periods = ko.observableArray([
     { PeriodId: 1, Period: '[|Last 7 Days|]' },
     { PeriodId: 2, Period: '[|Last 30 Days|]' },
     { PeriodId: 3, Period: '[|Last 3 Months|]' },
     { PeriodId: 4, Period: '[|Month to Date|]' },
     { PeriodId: 5, Period: '[|Year to Date|]' },
     { PeriodId: 6, Period: '[|Last Year|]' },
     { PeriodId: 7, Period: '[|Custom|]' }
    ]);
    selfReport.Groups = ko.observableArray([
           { GroupId: 0, Group: '[|Forms|]' },
           { GroupId: 1, Group: '[|Lead Adapter|]' }
    ]);
    selfReport.LeadAdapters = ko.observableArray(
        [
            { Id: 1, Name: '[|BDX|]' },
            { Id: 2, Name: '[|NHG|]' },
            { Id: 3, Name: '[|HotonHomes|]' },
            { Id: 4, Name: '[|PROPLeads|]' },
            { Id: 5, Name: '[|Zillow|]' },
            { Id: 6, Name: '[|NewHomeFeed|]' },
            { Id: 7, Name: '[|PrivateCommunities|]' },
            { Id: 8, Name: '[|IDX|]' },
            { Id: 9, Name: '[|Condo|]' },
            { Id: 10, Name: '[|BuzzBuzzHomes|]' },
            { Id: 12, Name: '[|HomeFinder|]' },
            { Id: 13, Name: '[|Facebook|]' },
            { Id: 14, Name: '[|Trulia|]' },
            { Id: 15, Name: '[|Builders Update|]' }

        ]);
    var forms;
    selfReport.Forms = ko.observableArray();

   
    selfReport.groupChange = function () {
        var value = this.value();
        if (value == "0") {
            selfReport.FormsVisible("True");
            selfReport.LeadSourceVisible("False");
        }
        else if (value == "1") {
            selfReport.LeadSourceVisible("True");
            selfReport.FormsVisible("False");
        }
    }

    selfReport.periodChange = function () {
        selfReport.CustomEndDate(moment(new Date()).format());
        selfReport.toMaxDate(maxdate);
        var value = this.value();
        if (value == "1")
            selfReport.CustomStartDate(moment().subtract(7, 'days').format());

        else if (value == "2")
            selfReport.CustomStartDate(moment().subtract(30, 'days').format());

        else if (value == "3")
            selfReport.CustomStartDate(moment().subtract(3, 'months').format());

        else if (value == "4")
            selfReport.CustomStartDate(moment().subtract(1, 'months').format());

        else if (value == "5")
            selfReport.CustomStartDate(moment().startOf('year').format());

        else if (value == "6") {
            selfReport.CustomStartDate(moment().subtract(1, 'years').format());
        }
        else if (value == "7") {
            var toStartDate = new Date();
            toStartDate.setDate(toStartDate.getDate());
            selfReport.CustomEndDate(toStartDate);
            var fromdate = new Date();
            fromdate.setDate(fromdate.getDate() - 7);
            selfReport.CustomStartDate(fromdate);
            selfReport.toMaxDate(new Date());
        }
    }
    selfReport.CustomDateDisplay = ko.pureComputed(function () {
        if (selfReport.PeriodId() == 7) {
            return true;
        } else {
            return false;
        }
    });

    selfReport.toMaxDate = ko.observable(maxdate);
    var fromMaxDate = new Date();
    fromMaxDate.setDate(fromMaxDate.getDate() - 1);
    selfReport.fromMaxDate = ko.observable(fromMaxDate);
    selfReport.DateFormat = ko.observable(dateFormat);

    selfReport.fromDateChangeEvent = function () {
        var fromDate = this.value();
        selfReport.CustomStartDate(moment(fromDate).format());
        var toDate = selfReport.CustomEndDate();
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }

    selfReport.toDateChangeEvent = function () {
        var fromDate = selfReport.CustomStartDate();
        var toDate = this.value();
        selfReport.CustomEndDate(moment(toDate).format());
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }

    function CreateGrid(data) {
        
        if ($('#grid').data().kendoGrid != undefined) {          
            $('#grid').data().kendoGrid.destroy();
            $(".k-grid-pager").remove();
            $(".k-grid-toolbar").remove();
            $('#grid').empty();
        }
        var rows = [];
        for (var i = 0; i < data.length; i++) {
            var entryArray = [];
            entryArray.push(data[i].Name);
            entryArray.push(data[i].UniqueSubmissions);     // + "|" + data[i].FormID + "|" + data[i].DropdownValueID
            entryArray.push(data[i].Total);         // + "|" + data[i].FormID + "|" + data[i].DropdownValueID
            rows.push(kendo.observable({
                entries: entryArray
            }));
        }

        var viewModel = kendo.observable({
            gridRows: rows
        });

        var columns = [];
        if (selfReport.GroupId() == 0) {
            columns.push({
                field: "entries[0]",
                title: "[|Form Name|]",
            });
        }
        if (selfReport.GroupId() == 1) {
            columns.push({
                field: "entries[0]",
                title: "[|Lead Adapter Name|]",
            });
        }
        columns.push({
            field: "entries[1]",
            title: "[|Unique Contacts|]",
            headerAttributes: {
                "class": "text-center unique-submissions"
            },
            attributes: {
                "class": "text-center"
            }
        });
        columns.push({
            field: "entries[2]",
            title: "[|Overall Submissions|]",
            headerAttributes: {
                "class": "text-center overall-submissions"
            },
            attributes: {
                "class": "text-center"
            }
        });

        var convertDateToString = function (date) {
            var day = date.getDay();        // yields day
            var month = date.getMonth();    // yields month
            var year = date.getFullYear();  // yields year
            var hour = date.getHours();     // yields hours 
            var minute = date.getMinutes(); // yields minutes
            var second = date.getSeconds(); // yields seconds
            var time = day + "/" + month + "/" + year + " " + hour + ':' + minute + ':' + second;
            return time;
        }

        var configuration = {
            scrollable: false,
            pageable: {
                pageSizes: ToPageSizeArray(),
                messages: {
                    display: "[|Showing|] {0}-{1} [|from|] {2} " + (selfReport.GroupId() == 0 ? '[|Forms|]' : '[|Lead Adapters|]')
                },
            },
            columns: columns,
            selectable: true,
            sortable: true,
            dataBound: function (e) {
                var colCount = $(".k-grid").find('table colgroup > col').length;
                $('#formsGrid table tr').each(function () {
                    $(this).find('td').each(function (index, value) {
                        if (index > 0) {
                            var text = $(this).text();
                            var selectedcell = 0; var value;
                            var dropdowns = text.split("|");

                            var firstTrText = $(this).closest('td').siblings().first().text();
                            var groupByValue = [];
                            data.map(function (v) { groupByValue.push({ name: v.Name, Id: v.FormID, mapID: v.LeadAdapterAndAccountMapID, isAPIForm: v.IsAPIForm }) });
                            var row = ko.utils.arrayFirst(groupByValue, function (i) {
                                if (i.name == firstTrText)
                                    return i;
                            });
                            var anchor = (row != null && row.isAPIForm && index == 2) ? document.createElement("span") : document.createElement("a");
                            if (row != null) {
                                anchor.setAttribute("cellid", row.Id);
                                anchor.setAttribute("mapid", row.mapID);
                            }
                            anchor.id = "id";

                            anchor.innerHTML = dropdowns[0];

                            if (dropdowns[0] != undefined && row != null && !(row.isAPIForm && index == 2)) {
                                anchor.style.cursor = 'pointer';
                                anchor.onclick = function () {
                                    var formId = $(this).attr('cellid');
                                    selfReport.RowId(parseInt(this.getAttribute("mapid")));

                                    if (selfReport.GroupId() == 0)
                                        selfReport.FormIds(parseInt(this.getAttribute("cellid")))
                                    if (selfReport.GroupId() == 1)
                                        selfReport.LeadAdapterIds(parseInt(this.getAttribute("cellid")))


                                    var jsondata = ko.toJSON(selfReport);
                                    if (index == 1 || selfReport.GroupId()== 1) {
                                        $.ajax({
                                            url: BASE_URL + '/GetOnlineRegistertedContacts',
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
                                                    window.location.href = '../formcontacts?guid=' + response.response + '&reportType=' + selfReport.ReportType() + '&reportId=' + selfReport.ReportId() + '&formId=' + formId;
                                                }
                                            }
                                        }).fail(function (error) {
                                            notifyError(error);
                                        });
                                    }
                                    if (index == 2 && selfReport.GroupId() == 0) {
                                        var customStartDate = Date.parse(selfReport.CustomStartDate());
                                        var customEndDate = Date.parse(selfReport.CustomEndDate());

                                        window.location.href = '../Form/FormSubmissions?formId=' + formId + '&periodId=' + selfReport.PeriodId() + '&customStartDateTicks=' + customStartDate.toString() + '&customEndDateTicks=' + customEndDate.toString();
                                    }
                                };

                            }
                            $(this).text("");
                            $(this).append(anchor);
                        }
                        else
                            anchor = document.createElement("span");

                    });
                });
                

                if (e.sender.dataSource.view().length == 0) {
                    e.sender.table.find('tbody').append('<tr><td colspan="' + colCount + '"><div class="notecordsfound"><div><i class="icon st-icon-browser-windows-2"></i></div><span class="bolder smaller-90">[|No records found|]</span></div></td></tr>')
                }
            },
            toolbar: [{ name: "excel", text: "Export to Excel" }],
            excelExport: function (e) {
                var sheet = e.workbook.sheets[0];
                for (var i = 1; i < sheet.rows.length; i++) {
                    for (var ci = 1; ci < sheet.rows[i].cells.length; ci++) {
                        var cellvalue = sheet.rows[i].cells[ci].value.toString().split("|");
                        sheet.rows[i].cells[ci].value = parseInt(cellvalue[0]);
                    }
                }
            },
            excel: {
                fileName:"FormCountSummary.xlsx",
                allPages: true,
                //for Exporting the file in IE9 and Safari Browsers
                proxyURL: "/Reports/ExportToExcel",
                forceProxy: true
            },
        }

        function detailInit(e) {
            var detailRow = e.detailRow;
            var grid = $("#grid").data("kendoGrid");
            var dataRows = grid.items();
            var uid = e.data.uid;
            var row = grid
              .tbody
              .find("tr[data-uid='" + uid + "']");
            var dataRows = grid.items();
            var rowIndex = dataRows.index(row);
            var currentPage = grid.dataSource.page();
            var pageIndex = (parseInt(currentPage - 1) * 5) + rowIndex;

            if (selfReport.GroupId() == 0)
                selfReport.FormIds(parseInt(data[pageIndex].FormID))
            if (selfReport.GroupId() == 1)
                selfReport.LeadAdapterIds(parseInt(data[pageIndex].FormID))

            selfReport.RowId(parseInt(data[pageIndex].DropdownValueID));

            var jsondata = ko.toJSON(selfReport);
            $("<div/>").appendTo(e.detailCell).kendoGrid({
                dataSource: {
                    type: "json",
                    transport: {
                        read: function (options) {
                            var griddata;
                            $.ajax({
                                url: BASE_URL + '/GetOnlineRegistertedContacts',
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
                                options.success(data.response);

                            }).fail(function (error) {
                                notifyError(error);
                            })
                        }
                    }
                },
                scrollable: false,
                columns: [
                    { field: "firstname", title: "[|First Name|]", width: "25%" },
                    { field: "lastname", title: "[|Last Name|]", width: "20%" },
                    { field: "email", title: "[|Primary Email|]", width: "30%" },
                    { field: "phonenumber", title: "[|Phone|]", width: "25%" }
                ]
            });
        }

        var timeEditGrid = $("#grid").kendoGrid(configuration).data("kendoGrid");
        var grid = $("#grid").data("kendoGrid");
        grid.dataSource.query({ page: 1, pageSize: parseInt(itemsPerPage) });
        kendo.bind($('#formsGrid'), viewModel);
        $(".k-grid-excel").addClass("cu-grid-excel");
        $("#grid").wrap("<div class='cu-table-responsive bdx-report-grid'></div>");

    }
    selfReport.errors = ko.validation.group(selfReport, true);
    selfReport.Runlist = function () {
        var selectedForms = [];
        var selectedLeadAdapters = [];

        selfReport.errors.showAllMessages();
        if (selfReport.errors().length > 0) {
            notifyError("[|Please select valid Date Range.|]");
            return;
        }

        //console.log(selfReport.FormId().length);

        if (selfReport.FormId() == 0 || selfReport.FormId().length == 0) {
            for (var l = 0; l < (selfReport.Forms()).length; l++) {
                selectedForms.push((selfReport.Forms())[l].Id);
            }
        }
        else
            selectedForms = (selfReport.FormId()).length > 0 ? (selfReport.FormId()).map(Number) : [];
        if (selfReport.LeadAdapterId() == 0) {
            for (var l = 0; l < (selfReport.LeadAdapters()).length; l++) {
                selectedLeadAdapters.push((selfReport.LeadAdapters())[l].Id);
            }
        }
        else
            selectedLeadAdapters = (selfReport.LeadAdapterId()).length > 0 ? (selfReport.LeadAdapterId()).map(Number) : [];

        //console.log("selected forms");
        //console.log(selectedForms);

        selfReport.FormIds(selectedForms);

        //if (selfReport.FormIds() == [] || selfReport.FormIds().length <= 0) {
        //    notifyError("[|There are no forms to run the report.|]");
        //    return;
        //}

        selfReport.LeadAdapterIds(selectedLeadAdapters);
        var jsondata = ko.toJSON(selfReport);
        $.ajax({
            url: '/Reports/FormsCountSummary',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: jsondata,
            type: 'post'

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
            selfReport.gridvisible('True');
            CreateGrid(data.response.Data);

        }).fail(function (error) {
            notifyError(error);
        })
    }    

    $.ajax({
        url: '/LeadScore/GetForms',
        type: 'get',
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
        forms = data.response;        
        selfReport.Forms(data.response);
        if (data.response.length <= 0) {
            notifyError("[|There are no forms to run the report.|]");
            return;
        }
        if (runReport == 'True' && selfReport.Forms().length > 0)
            selfReport.Runlist();


    }).fail(function (error) {
        notifyError(error);
    })

}