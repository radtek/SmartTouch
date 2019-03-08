var opportunityViewModel = function (buyerdata, url, WEBSERVICE_URL, contacturl, OpportunityPage, oppName, modal, Activities) {
    selfBuyer = this;
    ko.validatedObservable(ko.mapping.fromJS(buyerdata, {}, selfBuyer));
    ko.validation.rules['minimumLength'] = {

        validator: function (contacts, minimumLength) {
            return (contacts.length > minimumLength);
        },
        message: '[|Select at least one Contact|]'
    };

    ko.validation.rules.stageCannotEqual = {
        validator: function (stageID, otherVal) {
            return (stageID !== otherVal || stageID !== "");
        },
        message: '[|Select Stage|]'
    };

    ko.validation.rules.ownerCannotEqual = {
        validator: function (ownerID, otherVal) {
            return (ownerID != otherVal && ownerID != "");
        },
        message: '[|Select Owner|]'
    };
    ko.validation.rules.dateShouldBeInFuture = {
        validator: function (expectedCloseDate, otherVal) {
            if (expectedCloseDate === null)
                return false
            else {
                console.log(expectedCloseDate);
                var dateString = expectedCloseDate.toString();
                if (dateString.indexOf('/Date') == 0) {
                    var utzdate = ConvertToDate(selfBuyer.ExpectedCloseDate()).toUtzDate();
                    if (utzdate.getTime() > selfBuyer.utc_now().getTime())
                       return true;
                    }
                else {
                   if (expectedCloseDate.getTime() > selfBuyer.utc_now().getTime())
                       return true
                     }
            }
        },
        message: '[|Expected Close is required|]'
    };


    selfBuyer.minDate = ko.pureComputed({
        read: function () {
            return new Date().toUtzDate();
        }
    });


    ko.validation.rules.relationtypeCannotEqual = {
        validator: function (relationtypeID, otherVal) {
            return (relationtypeID != otherVal || relationtypeID != "");
        },
        message: '[|Select relationship|]'
    };

    ko.validation.rules.contacttypeCannotEqual = {
        validator: function (contactID, otherVal) {
            return (contactID != otherVal && contactID != "");
        },
        message: '[|Select Contact|]'
    };

    selfBuyer.DisplaywithDateTimeFormat = function (date) {
        var dateFormat = readCookie("dateformat").toUpperCase();
        if (date == null) {
            return "";
        }
        var utzDate = new Date(moment(date).toDate()).ToUtcUtzDate();
        return moment(utzDate).format(dateFormat + " hh:mm A");
    }

    selfBuyer.dateFormat = ko.observable(buyerdata.DateFormat);
    ko.validation.registerExtenders();

    selfBuyer.ExpectedCloseDate = ko.observable(buyerdata.ExpectedCloseDate);
    selfBuyer.DisplayCreatedDate = kendo.toString(kendo.parseDate(ConvertToDate(buyerdata.CreatedOn)), selfBuyer.DateFormat());

    selfBuyer.DisplayCreatedDate = ko.pureComputed({
        read: function () {
            if (selfBuyer.CreatedOn() == null) {
                return new Date().toUtzDate();
            }
            else {
                var dateString = selfBuyer.CreatedOn().toString();
                var dateFormat = readCookie("dateformat").toUpperCase();
                if (dateString.indexOf('/Date') == 0) {
                    var utzdate = ConvertToDate(selfBuyer.CreatedOn()).toUtzDate();
                    selfBuyer.CreatedOn(utzdate);
                    return moment(utzdate).format(dateFormat);
                }
                else {
                    var date = Date.parse(selfBuyer.CreatedOn());
                    return ConvertToDate(date.toString());
                }
            }
        },
        write: function (newValue) {
            selfBuyer.CreatedOn(new Date(newValue));
        }
    });



    if (oppName == null || oppName == '') {
        $("#txtOpportunity").prop("readonly", false);
        selfBuyer.OpportunityName = ko.observable(buyerdata.OpportunityName).extend({ required: { message: "[|Opportunity is required|]" }, maxLength: 75 });
    }
    else {
        $("#txtOpportunity").prop("readonly", true);
        selfBuyer.OpportunityName = ko.observable(oppName);
    }

    selfBuyer.Potential = ko.observable(buyerdata.Potential === 0 ? "" : buyerdata.Potential).extend({
        required: { message: "[|Potential is required|]" },
        pattern: { params: '^[0-9]*$', message: "[|Must be a Number|]" },
        min: 1,
        max: 922337203685477
    });
    selfBuyer.PotentialwithCurrency = ko.observable(dispalyPotential1(buyerdata.Potential));
    selfBuyer.StageID = ko.observable(buyerdata.StageID).extend({ required: { message: "[|Stage is required|]" } });
    selfBuyer.PreviousStageID = ko.observable(buyerdata.StageID);
    selfBuyer.stageValidation = selfBuyer.StageID.extend({ stageCannotEqual: 0 });
    selfBuyer.Statuses = ko.observableArray(buyerdata.Stages);
    selfBuyer.Contact = ko.observableArray([]);
   
    selfBuyer.Description = ko.observable(buyerdata.Description).extend({ maxLength: 1000 });
    selfBuyer.PeopleInvolved = ko.observableArray(buyerdata.PeopleInvolved);
    selfBuyer.Users = ko.observableArray(buyerdata.Users);
    selfBuyer.OpportunityID = ko.observable(buyerdata.OpportunityID);
    selfBuyer.Module = ko.observable("");
    selfBuyer.UserName = ko.observable(buyerdata.UserName);
    selfBuyer.OpportunityTags = ko.observableArray(buyerdata.OpportunityTags);
    selfBuyer.RelationshipTypeID = ko.observable("");
    selfBuyer.relationtypeValidation = selfBuyer.RelationshipTypeID.extend({ relationtypeCannotEqual: 0 });
    selfBuyer.OpportunityIndex = ko.observable();
    selfBuyer.OpportunityTotalHits = ko.observable();
    selfBuyer.opportunityValidation = ko.observable();
    selfBuyer.UserID = ko.observable(buyerdata.UserID === undefined ? 0 : buyerdata.UserID);
    selfBuyer.opportunityData = ko.observableArray();
    selfBuyer.Comments = ko.observable(buyerdata.Comments);
    selfBuyer.OpportunityContactMapID = ko.observable(0);
    selfBuyer.OpportunityType = ko.observable(buyerdata.OpportunityType);
    selfBuyer.ProductType = ko.observable(buyerdata.ProductType);
    selfBuyer.Address = ko.observable(buyerdata.Address);
    selfBuyer.OppId = ko.observable(buyerdata.OpportunityID);
    selfBuyer.AccountID = ko.observable(buyerdata.AccountID);
    selfBuyer.ContactCount = ko.observable(buyerdata.ContactCount);

    selfBuyer.OpportunityName.subscribe(function (text) {
        var opportunity = $(selfBuyer.opportunityData()).filter(function () {
            return this.OpportunityName.toString().toLowerCase() == text.toString().toLowerCase();
        })[0];
        if (opportunity != null) {
            selfBuyer.OpportunityID = ko.observable(opportunity.OpportunityID);
        }
        else {
            selfBuyer.OpportunityID = ko.observable();
        }
    });

    selfBuyer.PreviousOwnerId = ko.observable(buyerdata.OwnerId);


    var user = $(buyerdata.Users).filter(function () {
        return this.UserID == buyerdata.OwnerId;
    })[0];

    if (user != undefined)
        selfBuyer.SelectedOwnerText = ko.observable(user.Name);
    else
        selfBuyer.SelectedOwnerText = ko.observable(buyerdata.UserName);

    selfBuyer.SelectedOwnerText.subscribe(function (text) {
        var user = $(buyerdata.Users).filter(function () {
            return this.Name.toString().toLowerCase() == text.toString().toLowerCase();
        })[0];

        if (user != null) {
            selfBuyer.OwnerId = ko.observable(user.UserID);
        }
        else
            selfBuyer.OwnerId = ko.observable(0);
    });
    selfBuyer.ownerValidation = selfBuyer.UserID.extend({ ownerCannotEqual: 0 });
    selfBuyer.OpportunityID() === 0 ? selfBuyer.Module("[|Add Opportunity|]") : selfBuyer.Module("[|Edit Opportunity|]");

    selfBuyer.EnablePrevious = ko.pureComputed(function () {
        if (selfBuyer.OpportunityIndex() <= 1)
            return false;
        else
            return true;
    });

    selfBuyer.EnableNext = ko.pureComputed(function () {
        if (selfBuyer.OpportunityIndex() >= selfBuyer.OpportunityTotalHits())
            return false;
        else
            return true;
    });

    selfBuyer.HideNavigation = ko.pureComputed(function () {
        if (selfBuyer.OpportunityIndex() == 0 || selfBuyer.OpportunityTotalHits() == 1)
            return true;
        else
            return false;
    });


    selfBuyer.Contacts = ko.observableArray(buyerdata.Contacts);

    if ((checkedContactValues.length > 0 || localStorage.getItem("contactdetails") != null) && selfBuyer.Contacts().length == 0) {
        if (localStorage.getItem("contactdetails") != null && selfBuyer.Contacts().length > 0) {
            var selectedContactss = selectedContacts(0, checkedContactValues, buyerdata.Contacts);
            $.each(selectedContactss, function (i, v) {
                if (selfDetails.ContactType() == '1')
                    v.ContactType = 1;
                else
                    v.ContactType = 2;
            });
           
            selfBuyer.Contacts(selectedContactss);
        }
        else {

            selfBuyer.Contacts(selectedContacts(0, checkedContactValues, buyerdata.Contacts));
        }
    }

    var selectedOpportunities = GetSelectedOpportunities('chkopportunity');

    if (localStorage.getItem("OpportunityName") != null) {
        selfBuyer.OpportunityName(localStorage.getItem("OpportunityName"));
        selfBuyer.OpportunityID(localStorage.getItem("OpportunityID"));
        selfBuyer.OppId(localStorage.getItem("OpportunityID"));
        $("#txtOpportunity").prop("readonly", true);
    }

    var array = [];
    $.each(selectedOpportunities, function (index, value) {
        var viewOppViewModel = {
            id: value.OpportunityID,
            name: value.OpportunityName
        }
        array.push(viewOppViewModel);
    });
    $("#txtOpportunity").prop('disabled', false);
    if (array.length == 0) {
        // notifyError("[|Please select at least one opportunity|]");

    } else if (array.length > 1) {
        notifyError("[|Please select only one opportunity|]");
    } else {
        selfBuyer.OpportunityName(array[0].name);
        selfBuyer.OpportunityID(array[0].id);
        $("#txtOpportunity").prop('disabled', true);
    }

    selfBuyer.ContactFullNames = ko.computed({
        read: function () {
            var contactFullNames = "";
            if (selfBuyer.Contacts() != null) {
                $.each(selfBuyer.Contacts(), function (index, value) {
                    if (contactFullNames != null && contactFullNames != "" && value.FullName != null)
                        contactFullNames = contactFullNames + "," + value.FullName;
                    else
                        contactFullNames = contactFullNames + value.FullName;
                });
            }
            return contactFullNames;
        },
        write: function () { },
        owner: this
    });

    selfBuyer.contactsValidation = selfBuyer.ContactFullNames.extend({ minimumLength: 1 });


    localStorage.removeItem("contactsData");
    var ArrayUsers = "";
    if (selfBuyer.Contacts()) {
        $.each(selfBuyer.Contacts(), function (index, value) {
            ArrayUsers += (value.FullName + "|" + value.Id) + "||";
        });
    }
  


    localStorage.setItem("contactsData", ArrayUsers);

    selfBuyer.DeleteAction = function (Action) {
        var action = "GetActionContactsCount";
        $.ajax({
            url: url + action,
            type: 'post',
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({ 'actionId': Action.ActionId() }),
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            if (data.response == 1) {
                DeleteConfirmation("Delete Action", "Cancel", "[|Are you sure you want to delete this Action|]?", Action);
                DeleteConfirmation("Delete Action", "Cancel", "[|You’re about to delete this Action. Are you sure you want to delete|]?", Action);
            }
            else {
                DeleteConfirmation("Delete Action", "Cancel", "[|More than one contact tagged for this Action, are you sure you want to delete this Action?|]", Action);
                DeleteConfirmation("Delete Action", "Cancel", "[|More than one contact tagged for this Action|]" + "</br>" + "[|You’re about to delete this Action. Are you sure you want to delete|]?", Action);
            }
        }).fail(function (error) {
            if (error == undefined)
                $('#opportunitydettrigger').modal('toggle');
            else
                notifyError(error);
        })
    };

    selfBuyer.ActionCompletedMessage = ko.observable();
    selfBuyer.CompletedActionOption = ko.observable();
    selfBuyer.ActionTypeValue = ko.observable();
    selfBuyer.ActionDateOn = ko.observable();
    selfBuyer.ToSend = ko.observable(false);
    selfBuyer.ActionCompleted = ko.observable();
    selfBuyer.MailBulkId = ko.observable();
    selfBuyer.IsCompleted = ko.observable();
    selfBuyer.ActionId = ko.observable();
    selfBuyer.AddNoteSummary = ko.observable(false);
    var now = new Date().toUtzDate();
    selfBuyer.utc_now = ko.observable(new Date(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate(), now.getUTCHours(), now.getUTCMinutes(), now.getUTCSeconds(), now.getUTCMilliseconds()));

    selfBuyer.Completed = function (Action) {
        if (Action.IsCompleted()) {
           
            selfBuyer.ActionCompletedMessage("[|Are you sure you want to mark this action as not completed|]?");
            selfBuyer.CompletedActionOption("[|Mark as Not Complete|]");
        }
        else {
            selfBuyer.ActionCompletedMessage("[|Are you sure you want to mark this action as completed|]?");
            selfBuyer.CompletedActionOption("[|Mark as Completed|]");
        }

        var dateString = Action.ActionDate().toString();
        if (dateString.indexOf('/Date') == 0) {
            var utzdate = ConvertToDate(Action.ActionDate()).toUtzDate();
            selfBuyer.ActionDateOn(utzdate);
        }
        else {
            var date = Date.parse(Action.ActionDate());
            selfBuyer.ActionDateOn(ConvertToDate(date.toString()));
        }
        selfBuyer.ActionTypeValue(Action.ActionTypeValue());
        selfBuyer.MailBulkId(Action.MailBulkId());
        selfBuyer.ActionCompleted(Action.IsCompleted());
        selfBuyer.ActionId = ko.observable(Action.ActionId());
        $('#oppModal').modal('show');
        
        var action = "ActionCompleted";
        //return true;
    };

    selfBuyer.OpportunityActionCompleted = function (message) {
        var status;
        if (selfBuyer.ActionCompleted() === false)
            status = true;
        else
            status = false;

        console.log(status);
        var action = "ActionCompleted";
        $.ajax({
            url: url + action,
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                'actionID': selfBuyer.ActionId(), 'isActionCompleted': status, 'opportunityId': buyerdata.OpportunityID, 'isSchedule': selfBuyer.ToSend(), 'mailBulkId': selfBuyer.MailBulkId(),'AddToNoteSummary':selfBuyer.AddNoteSummary()
            })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            $('.success-msg').remove();
            if (status)
                notifySuccess('[|Action marked as completed.|]');
            else
                notifySuccess('[|Action marked as not completed.|]');

            window.location.href = document.URL;
        }).fail(function (error) {
            if (error == undefined)
                $('#opportunitycomptrigger').modal('toggle');
            else
                notifyError(error);
        })
    }

    selfBuyer.CloseWindowFunction = function (act) {
        $('#oppModal').modal('hide');
    }


    function DeleteConfirmation(okText, cancelText, confirmMessage, Action) {
        alertifyReset(okText, cancelText);
        alertify.confirm(confirmMessage, function (e) {
            if (e) {
                var action = "OpportunityDeleteAction";
                $.ajax({
                    url: url + action,
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ 'actionId': Action.ActionId() })
                }).then(function (response) {
                    var filter = $.Deferred()
                    if (response.success) {
                        filter.resolve(response)
                    } else {
                        filter.reject(response.error)
                    }
                    return filter.promise()
                }).done(function (data) {
                    $('.success-msg').remove();
                    notifySuccess('[|Action deleted successfully|]');
                    setTimeout(
                        function () {
                            window.location.reload(true);
                        }, setTimeOutTimer);
                }).fail(function (error) {
                    notifyError(error);
                })
            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }
        });
    }

    selfBuyer.makeImageObservable = ko.observable();
    selfBuyer.makeImageObservable = function () {
        selfBuyer.imagePath = ko.observable();
        selfBuyer.Image = ko.observable(PersonDetails.Image);
        selfBuyer.imagePath(selfBuyer.Image().ImageContent)
    };

    selfBuyer.AllContacts = ko.observableArray();
    selfBuyer.ContactID = ko.observable("");
    selfBuyer.ContactText = ko.observable("");
    selfBuyer.ContactTextID = ko.observable("");
    selfBuyer.contactValidation = selfBuyer.ContactID.extend({ contacttypeCannotEqual: 0 });
    selfBuyer.DateValidation = selfBuyer.ExpectedCloseDate.extend({ dateShouldBeInFuture: new Date() });

    selfBuyer.errors = ko.validation.group([selfBuyer.OpportunityName, selfBuyer.contactsValidation, selfBuyer.Potential, selfBuyer.stageValidation, selfBuyer.SelectedOwnerText, selfBuyer.DateValidation, selfBuyer.UserID, selfBuyer.Comments]);
    selfBuyer.saveBuyer = function () {
        selfBuyer.OwnerId(selfBuyer.UserID());



        selfBuyer.errors.showAllMessages();
        if (selfBuyer.errors().length > 0) {
            validationScroll();
            return;
        }

        if (selfBuyer.OpportunityID() == 0 || selfBuyer.OpportunityID() == undefined) {
            notifyError("[|Opportunity name '" + selfBuyer.OpportunityName() +"' does not exits|]");
            return;
        }
        var jsondata = ko.toJSON(selfBuyer);

        operation = "InsertBuyer";
        type = "post";

        pageLoader();
        var authToken = readCookie("accessToken");
        $.ajax({
            url: WEBSERVICE_URL + '/Buyer',
            type: type,
            data: jsondata,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + authToken);
            },
            success: function (data) {
                $('.success-msg').remove();
                notifySuccess('[|Successfully saved the Buyer(s)|]');
                removepageloader();
                window.location.reload();
                
            },
            error: function (response) {
                removepageloader();
                notifyError(response.responseText);
            }
        });
    };


    selfBuyer.showPeopleInvolved = ko.observable(true);
    selfBuyer.AddPeopleDisplay = function () {
        selfBuyer.showPeopleInvolved(false);
    };

    selfBuyer.editOpportunity = function () {
        window.location.href = "/editopportunity?opportunityID=" + selfBuyer.OpportunityID();
    };


    selfBuyer.deleteOpportunity = function () {
        alertifyReset("Delete Opportunity", "Cancel");
        var array = [];
        array.push(selfBuyer.OpportunityID());
        var confirmMesaage = "[|Are you sure you want to delete this Opportunity|]?";
        commondelete(confirmMesaage, array);
    };

    selfBuyer.InlineEditing = function (fieldId) {
        if (fieldId == 1)
            $(".name-field").addClass("active");
        else if (fieldId == 2)
            $(".decription-field").addClass("active");
        else if (fieldId == 4) {
            $(".owner-field").addClass("active");
            selfBuyer.UserID(selfBuyer.OwnerId());
        }
        else if (fieldId == 5)
            $(".potential-field").addClass("active");
        else if (fieldId == 7)
            $(".type-field").addClass("active");
        else if (fieldId == 8)
            $(".product-field").addClass("active");
        else if (fieldId == 9)
            $(".address-field").addClass("active");
    }

    selfBuyer.InlineSaving = function (fieldId) {
        if (fieldId == 1)
            updateOpportunityName();
        else if (fieldId == 2)
            updateOpportunityDescription();
        else if (fieldId == 4)
            updateOpportunityOwner();
        else if (fieldId == 5)
            updateOpportunityPotential();
        else if (fieldId == 7)
            updateOpportunityType();
        else if (fieldId == 8)
            updateProductType();
        else if (fieldId == 9)
            updateOpportunityAddress();
        else if (fieldId == 10)
            updateOpportunityImage();

    }

    selfBuyer.InlineCancel = function (fieldId) {
        if (fieldId == 1) {
            $(".name-field").removeClass("active");
            selfBuyer.OpportunityName((buyerdata.OpportunityName == null || buyerdata.OpportunityName == undefined) ? "" : buyerdata.OpportunityName);
        }
        else if (fieldId == 2) {
            $(".decription-field").removeClass("active");
            selfBuyer.Description((buyerdata.Description == null || buyerdata.Description == undefined) ? "" : buyerdata.Description);
        }
        else if (fieldId == 5) {
            $(".potential-field").removeClass("active");
            selfBuyer.Potential((buyerdata.Potential == null || buyerdata.Potential == undefined) ? "": buyerdata.Potential);;
        }
        else if (fieldId == 7) {
            $(".type-field").removeClass("active");
            selfBuyer.OpportunityType((buyerdata.OpportunityType == null || buyerdata.OpportunityType == undefined) ? "" : buyerdata.OpportunityType);
        }
        else if (fieldId == 8) {
            $(".product-field").removeClass("active");
            selfBuyer.ProductType((buyerdata.ProductType == null || buyerdata.ProductType == undefined) ? "" : buyerdata.ProductType);

        }
        else if (fieldId == 9) {
            $(".address-field").removeClass("active");
            selfBuyer.Address((buyerdata.Address == null || buyerdata.Address == undefined) ? "" : buyerdata.Address);
        }

    }

    selfBuyer.buyerActivated = ko.observable(false);
    selfBuyer.getBuyers = function () {
        $("#opp-time-line").removeClass("active");
        $("#opp-attachments").removeClass("active");
        $('.nav-tabs a[href="#buyerssummary"]').tab('show');
        if (!selfBuyer.buyerActivated()) {
            CreateOpportunitBuyersGrid();
            selfBuyer.buyerActivated(true);
        }
    }

    function CreateOpportunitBuyersGrid() {
        var newDataSource = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    var pageNumber = typeof ($("#opportunity-buyers-grid").data("kendoGrid").dataSource.page()) != "undefined" ? $("#opportunity-buyers-grid").data("kendoGrid").dataSource.page() : 1;
                    var pagesize = typeof ($("#opportunity-buyers-grid").data("kendoGrid").dataSource.pageSize()) != "undefined" ? $("#opportunity-buyers-grid").data("kendoGrid").dataSource.pageSize() : parseInt(parseInt(readCookie('pagesize')));
                    $.ajax({
                        url: '/Opportunities/GetOpportunityBuyers',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            'opportunityId': selfBuyer.OppId(),
                            'accountId': selfBuyer.AccountID(),
                            'pageNumber': pageNumber,
                            'pageSize': pagesize
                        }),
                        type: 'post'
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
                        options.success(data.response);
                        selfBuyer.ContactCount(data.response.Total);
                    }).fail(function (error) {
                        notifyError(error);
                    });
                },
            },
            serverPaging: true,
            schema: {
                data: "Data",
                total: "Total"
            }
        });

        $("#opportunity-buyers-grid").kendoGrid({
            dataSource: newDataSource,
            scrollable: false,
            sortable: true,
            columns: [{

                field: "Name",
                title: "Contact Name",
                template: "#if(ContactType===1){# <a href='/person/#:ContactID#'>#:Name#</a>#}else{#<a href='/company/#:ContactID#'>#:Name#</a>#}#"
            }, {

                field: "Stage",
                title: "Stage",
            }
            , {
                field: "ExpectedToClose",
                title: "Expected Close",
                template: "#:displayCloseDate(ExpectedToClose)#"
            },
            {
                field: "Potential",
                title: "Potential",
                template: "#:dispalyPotential(Potential)#"
            }, {
                field: "Comments",
                title: "Comments",
                template: "<span>#:dispalycomments(Comments)#</span>"
            }, {
                template: "#if(PreviousComments=== null){#  #}else{#<a data-target='\\#buyercommentsmodal' data-toggle='modal' href='javascript:void(0)' data-content='#:PreviousComments#' onclick='PrveComments(this)' class='buyercomments' title='Previous Comments'><i class='icon st-icon-notes'></a>#}#"
            },
            {
                field: "OwnerName",
                title: "Owner"
            }
            , {
                template: "<div><a data-target='\\#modal' data-toggle='modal'  href='/Opportunities/EditBuyer?buyerId=#:OpportunityContactMapID#'><i class='icon st-icon-edit'></i></a><a href='javascript:void(0)' onclick='DeleteOpportunityBuyer(#:OpportunityContactMapID#)'   data-id='#:OpportunityContactMapID#'  class='buy-dlt' title='Delete Buyer'><i class='icon st-icon-bin-3'></i></a></div>"
            }
            ],
            dataBound: function (e) {
                onDataBound(e);
            },
            pageable: {
                pageSizes: ToPageSizeArray(),
                messages: {
                    display: "[|Showing|] {0}-{1} [|from|] {2:n0} [|Buyer(s)|]"
                },
            }
        })
        var contactSummaryGrid = $("#opportunity-buyers-grid").data("kendoGrid");
        var pagesize = typeof ($("#opportunity-buyers-grid").data("kendoGrid").dataSource.pageSize()) != "undefined" ? $("#opportunity-buyers-grid").data("kendoGrid").dataSource.pageSize() : parseInt(parseInt(readCookie('pagesize')));
        contactSummaryGrid.dataSource.query({ page: 1, pageSize: pagesize });
        $("#opportunity-buyers-grid").wrap("<div class='cu-table-responsive bdx-report-grid'></div>");
    }

  

    function onDataBound(e) {

        var colCount = $(".k-grid").find('table colgroup > col').length;
        if (e.sender.dataSource.view().length == 0) {
            e.sender.table.find('tbody').append('<tr><td colspan="' + colCount + '"><div class="notecordsfound"><div><i class="icon st-icon-browser-windows-2"></i></div><span class="bolder smaller-90">[|No records found|]</span></div></td></tr>')
        }

    }




    function updateOpportunityName() {
        $.ajax({
            url: url + 'UpdateOpportunityName',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'opportunityId': selfBuyer.OppId(), 'oppName': selfBuyer.OpportunityName() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            } return filter.promise()
        }).done(function (data) {
            selfBuyer.OpportunityName(data.response.OpportunityName)
            buyerdata.OpportunityName = data.response.OpportunityName;
            localStorage.removeItem("OpportunityName")
            $(".name-field").removeClass("active");
            localStorage.setItem("OpportunityName", selfBuyer.OpportunityName());
            selfBuyer.TimeLineViewModel(new TimeLineViewModel(contacturl, url, "opportunities", selfBuyer.dateFormat(), Activities, null, selfBuyer.OppId()));
        }).fail(function (error) {
            notifyError(error);
        });
    }

    function updateOpportunityDescription() {
        $.ajax({
            url: url + 'UpdateOpportunityDescription',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'opportunityId': selfBuyer.OppId(), 'description': selfBuyer.Description() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            } return filter.promise()
        }).done(function (data) {
            $(".decription-field").removeClass("active");
            selfBuyer.Description(data.response.Description);
            buyerdata.Description = data.response.Description;
            selfBuyer.TimeLineViewModel(new TimeLineViewModel(contacturl, url, "opportunities", selfBuyer.dateFormat(), Activities, null, selfBuyer.OppId()));
        }).fail(function (error) {
            notifyError(error);
        });
    }

    function updateOpportunityOwner() {
        $.ajax({
            url: url + 'UpdateOpportunityOwner',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'opportunityId': selfBuyer.OppId(), 'ownerId': selfBuyer.UserID() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            } return filter.promise()
        }).done(function (data) {
            $(".owner-field").removeClass("active");
            selfBuyer.UserID(data.response.OwnerId);
            selfBuyer.TimeLineViewModel(new TimeLineViewModel(contacturl, url, "opportunities", selfBuyer.dateFormat(), Activities, null, selfBuyer.OppId()));
        }).fail(function (error) {
            notifyError(error);
        });
    }

    function updateOpportunityPotential() {
        var reg = new RegExp('^\\d*$');
        if (reg.test(selfBuyer.Potential())) {
            $.ajax({
                url: url + 'UpdateOpportunityPotential',
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'opportunityId': selfBuyer.OppId(), 'potential': selfBuyer.Potential() })
            }).then(function (response) {
                var filter = $.Deferred()
                if (response.success) {
                    filter.resolve(response)
                } else {
                    filter.reject(response.error)
                } return filter.promise()
            }).done(function (data) {
                selfBuyer.PotentialwithCurrency(dispalyPotential1(data.response.Potential));
                buyerdata.Potential = data.response.Potential;
                selfBuyer.TimeLineViewModel(new TimeLineViewModel(contacturl, url, "opportunities", selfBuyer.dateFormat(), Activities, null, selfBuyer.OppId()));
                $(".potential-field").removeClass("active");
            }).fail(function (error) {
                notifyError(error);
            });
        }

    }



    function updateOpportunityType() {
        $.ajax({
            url: url + 'UpdateOpportunityType',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'opportunityId': selfBuyer.OppId(), 'type': selfBuyer.OpportunityType() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            } return filter.promise()
        }).done(function (data) {
            $(".type-field").removeClass("active");
            selfBuyer.OpportunityType(data.response.OpportunityType);
            buyerdata.OpportunityType = data.response.OpportunityType;
            selfBuyer.TimeLineViewModel(new TimeLineViewModel(contacturl, url, "opportunities", selfBuyer.dateFormat(), Activities, null, selfBuyer.OppId()));
        }).fail(function (error) {
            notifyError(error);
        });
    }

    function updateProductType() {
        $.ajax({
            url: url + 'UpdateOpportunityProductType',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'opportunityId': selfBuyer.OppId(), 'productType': selfBuyer.ProductType() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            } return filter.promise()
        }).done(function (data) {
            $(".product-field").removeClass("active");
            selfBuyer.ProductType(data.response.ProductType);
            buyerdata.ProductType = data.response.ProductType;
            selfBuyer.TimeLineViewModel(new TimeLineViewModel(contacturl, url, "opportunities", selfBuyer.dateFormat(), Activities, null, selfBuyer.OppId()));
        }).fail(function (error) {
            notifyError(error);
        });
    }

    function updateOpportunityAddress() {
        $.ajax({
            url: url + 'UpdateOpportunityAddress',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'opportunityId': selfBuyer.OppId(), 'address': selfBuyer.Address() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            } return filter.promise()
        }).done(function (data) {
            $(".address-field").removeClass("active");
            selfBuyer.Address(data.response.Address);
            buyerdata.Address = data.response.Address;
            selfBuyer.TimeLineViewModel(new TimeLineViewModel(contacturl, url, "opportunities", selfBuyer.dateFormat(), Activities, null, selfBuyer.OppId()));
        }).fail(function (error) {
            notifyError(error);
        });
    }

    function updateOpportunityImage() {
        $.ajax({
            url: url + 'UpdateOpportunityImage',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'opportunityId': selfBuyer.OppId(), 'image': selfBuyer.Image })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            } return filter.promise()
        }).done(function (data) {
            //$(".address-field").removeClass("active");
            createCookie('log', false, 1);
            window.location.reload();

        }).fail(function (error) {
            notifyError(error);
        });
    }

    $("#images").kendoUpload({
        async: {
            saveUrl: "",
            removeUrl: "remove",
            autoUpload: false
        },
        select: onSelect
    });

    function onSelect(e) {
        $('.k-file').hide();
        $(".k-upload-selected").hide();
        var varImageType = "";
        $.each(e.files, function (index, value) {
            var ok = value.extension.toLowerCase() == ".jpg"
                     || value.extension.toLowerCase() == ".jpeg"
                     || value.extension.toLowerCase() == ".png"
                     || value.extension.toLowerCase() == ".bmp";

            if (!ok) {
                e.preventDefault();
                notifyError("[|Please upload jpg, jpeg, png, bmp files|]"); return false;
            }
            else if (bytesToSize(e.files[0].size) > 3) {
                e.preventDefault();
                notifyError("[|Image size should not be more than 3 Mb|]");
                return false;
            }
            else {
            }
            var friendlyname = value.name;

            var fileReader = new FileReader();
            fileReader.onload = function (event) {
                //  self.imagePath(event.target.result);
                var image = document.getElementById("opportunityimage");
                image.src = event.target.result;
                selfBuyer.Image.ImageContent = event.target.result;
                selfBuyer.Image.OriginalName = friendlyname;
                selfBuyer.Image.ImageType = value.extension.toLowerCase();
                //  selfBuyer.ContactImageUrl('');
            }
            fileReader.readAsDataURL(e.files[0].rawFile);
        });
    }

    function bytesToSize(bytes) {
        return (bytes / (1024 * 1024)).toFixed(2);
    };

    selfBuyer.ProfileImage = ko.observable();

    selfBuyer.uploadProfileImage = function () {
        var filename = selfBuyer.ProfileImage();
        selfBuyer.ProfileImageKey = null;
        var extension = filename.replace(/^.*\./, '');

        if (extension.toLowerCase() == "jpeg" || extension.toLowerCase() == "jpg" || extension.toLowerCase() == "png" || extension.toLowerCase() == "bmp") {
            var image = document.getElementById("opportunityimage");
            image.src = filename;
            selfBuyer.Image.ImageContent = filename;
            //selfBuyer.ContactImageUrl(filename);
        }
        else {
            notifyError("[|Please upload jpg, jpeg, png, bmp files|]");
            return false;
        }
    }

    function commondelete(confirmationMessage, opportunityIds) {
        alertify.confirm(confirmationMessage, function (e) {
            if (e) {
                var varDeleteURL = url + "DeleteOpportunities";
                jQuery.ajaxSettings.traditional = true;
                $.ajax({
                    url: varDeleteURL,
                    type: 'POST',
                    dataType: 'json',
                    data: JSON.stringify({ Id: opportunityIds }),
                    contentType: 'application/json; charset=utf-8'
                }).then(function (response) {
                    var filter = $.Deferred()
                    if (response.success) {
                        filter.resolve(response)
                    }
                    else {
                        filter.reject(response.error)
                    }
                    return filter.promise()
                }).done(function (data) {
                    notifySuccess("[|Successfully deleted the opportunities|]");
                    window.location.href = "/opportunities";
                }).fail(function (error) {
                    notifyError(error);
                })
            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }
        });
    }

    function dispalyPotential1(potential) {
        var currencyFormat = buyerdata.Currency;

        if (currencyFormat == "$X,XXX.XX") {
            kendo.culture("en-US");
            return "$" + kendo.toString(potential, 'n');
        } else if (currencyFormat == "X XXX,XX $") {
            kendo.culture("fr-FR");
            return kendo.toString(potential, 'n') + " $";
        } else if (currencyFormat == "B/.X,XXX.XX") {
            kendo.culture("en-US");
            return "B/." + kendo.toString(potential, 'n');
        } else {
            kendo.culture("en-US");
            return "$" + kendo.toString(potential, 'n');
        }
    }


}

checkedContactValues = fnGetCheckedValues();