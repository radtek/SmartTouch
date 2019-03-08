var contactDetailsViewModel = function (WEBSERVICE_URL, viewModel, itemsPerPage) {
    var self = this;
    self.summaryActivated = ko.observable(false);
    self.ContactSummary = ko.observableArray([]);

    self.getContactSummary = function () {
        if (!self.summaryActivated()) {
            var authToken = readCookie("accessToken");
            $.ajax({
                url: WEBSERVICE_URL + '/contactsummary',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: { 'contactId': viewModel.ContactID() },
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                },
                type: 'get',
                success: function (response) {
                    bi = response;
                    if (response.ContactSummaryDetails)
                    {
                        self.ContactSummary(response.ContactSummaryDetails.sort(function (a, b) {
                            return a.CreatedOn < b.CreatedOn;
                        }));
                    }

                    else
                        self.ContactSummary("");

                    self.summaryActivated(true);
                    createContactSummaryTable();
                },
                error: function (response) {
                    notifyError(response);
                }
            });
        };
    }

    function createCookie(name, value, days) {
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            var expires = "; expires=" + date.toGMTString();
        } else var expires = "";
        document.cookie = escape(name) + "=" + escape(value) + expires + "; path=/";
    }
    createCookie("pagesize", itemsPerPage, 1);

 

    function createContactSummaryTable() {
        $("#contact-summary-grid").kendoGrid({
            dataSource: self.ContactSummary(),
            pageable: {
                pageSizes: true,
                buttonCount: 5
            },
            scrollable: false,
            sortable: true,
            columns: [{
          
                field: "NoteDetails",
                title: "Summary Details",
                filterable: {
                    multi: true,
                    search: true
                },
                template: "<div class='tm-minimize notesummary'>#:NoteDetails#</div>"
            }, {
                field: "CreatedOn",
                title: "Created On",
                template: "#:displayDate(CreatedOn)#"
            }, {
                field: "CreatedBy",
                title: "Created By"
            },{
                 field: "NoteCategory",
                 title: "Summary Category"
            },
            {
                template: "<div><a data-target='\\#modal' data-toggle='modal'  href='/Contact/EditNote?noteId=#:NoteID#'><i class='icon st-icon-edit'></i></a>" +
                            "<a href='javascript:void(0)' onclick='DeleteContactNote(#:NoteID#)' data-id='#:NoteID#'  class='nt-dlt' title='Delete Note'><i class='icon st-icon-bin-3'></i></a>" +
                             "<a href='javascript:void(0)' data-target='\\#contactsummarymodal' data-toggle='modal' data-content='#:SummaryDetails#' onclick='ViewContactSummary(this)'  data-id='#:NoteID #' class='nt-view' title='Contact Summary' ><i class='icon st-icon-eye'></i>" +
                            "</div>"
            }
             ],
            
            dataBound: function (e) {
                onDataBound(e);
            },
            pageable: {
                pageSizes: ToPageSizeArray(),
                messages: {
                    display: "[|Showing|] {0}-{1} [|from|] {2} " + '[|Notes|]'
                },
            },
            serverPaging: true,
        })
        var contactSummaryGrid = $("#contact-summary-grid").data("kendoGrid");

        contactSummaryGrid.dataSource.query({ page: 1, pageSize: parseInt(parseInt(readCookie('pagesize'))) });
        $("#contact-summary-grid").wrap("<div class='cu-table-responsive bdx-report-grid'></div>");
    }

    function onDataBound(e) {
        var colCount = $(".k-grid").find('table colgroup > col').length;
        if (e.sender.dataSource.view().length == 0) {
            e.sender.table.find('tbody').append('<tr><td colspan="' + colCount + '"><div class="notecordsfound"><div><i class="icon st-icon-browser-windows-2"></i></div><span class="bolder smaller-90">[|No records found|]</span></div></td></tr>')
        }
        
    }

}