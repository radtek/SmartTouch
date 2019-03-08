var opportunityViewModel1 = function (data, url, WEBSERVICE_URL, imagePath) {
    selfOpportunity = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfOpportunity));

    ko.validation.rules.ownerCannotEqual = {
        validator: function (ownerID, otherVal) {
            return selfOpportunity.OwnerId() !== otherVal;
        },
        message: '[|Select owner|]'
    };
   


    selfOpportunity.minDate = ko.pureComputed({
        read: function () {
            return new Date().toUtzDate();
        }
    });

    selfOpportunity.DisplaywithDateTimeFormat = function (date) {
        var dateFormat = readCookie("dateformat").toUpperCase();
        if (date == null) {
            return "";
        }
        var utzDate = new Date(moment(date).toDate()).ToUtcUtzDate();
        return moment(utzDate).format(dateFormat + " hh:mm A");
    }

    selfOpportunity.dateFormat = ko.observable(data.DateFormat);
    ko.validation.registerExtenders();


    selfOpportunity.OpportunityName = ko.observable(data.OpportunityName).extend({ required: { message: "[|Opportunity is required|]" }, maxLength: 75 });
    selfOpportunity.Potential = ko.observable(data.Potential === 0 ? "" : data.Potential).extend({
        required: { message: "[|Potential is required|]" },
        pattern: { params: '^[0-9]*$', message: "[|Must be a number|]" },
        min: 1,
        max: 922337203685477
    });
    selfOpportunity.PotentialwithCurrency = ko.observable(dispalyPotential(data.Potential));
    selfOpportunity.Statuses = ko.observableArray(data.Stages);
    selfOpportunity.Description = ko.observable(data.Description).extend({ maxLength: 1000 });
    selfOpportunity.Users = ko.observableArray(data.Users);
    selfOpportunity.OpportunityID = ko.observable(data.OpportunityID);
    selfOpportunity.Module = ko.observable("");
    selfOpportunity.UserName = ko.observable(data.UserName);
    selfOpportunity.OpportunityType = ko.observable(data.OpportunityType);
    selfOpportunity.ProductType = ko.observable(data.ProductType);
    selfOpportunity.Address = ko.observable(data.Address);

    selfOpportunity.PreviousOwnerId = ko.observable(data.OwnerId);


    var user = $(data.Users).filter(function () {
        return this.UserID == data.OwnerId;
    })[0];

    if (user != undefined)
        selfOpportunity.SelectedOwnerText = ko.observable(user.Name);
    else
        selfOpportunity.SelectedOwnerText = ko.observable(data.UserName);

    selfOpportunity.SelectedOwnerText.subscribe(function (text) {
        var user = $(data.Users).filter(function () {
            return this.Name.toString().toLowerCase() == text.toString().toLowerCase();
        })[0];

        if (user != null) {
            selfOpportunity.OwnerId = ko.observable(user.UserID);
        }
        else
            selfOpportunity.OwnerId = ko.observable(0);
    });
    selfOpportunity.ownerValidation = selfOpportunity.SelectedOwnerText.extend({ ownerCannotEqual: 0 });
    selfOpportunity.OpportunityID() === 0 ? selfOpportunity.Module("[|Add Opportunity|]") : selfOpportunity.Module("[|Edit Opportunity|]");

    selfOpportunity.DeleteAction = function (Action) {
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

    selfOpportunity.Completed = function (Action) {
        var action = "ActionCompleted";
        $.ajax({
            url: url + action,
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'actionID': Action.ActionId(), 'isActionCompleted': Action.IsCompleted(), 'contactId': 0, 'opportunityId': data.OpportunityID })
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
            if (Action.IsCompleted())
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
        return true;
    };


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

    selfOpportunity.errors = ko.validation.group([selfOpportunity.OpportunityName, selfOpportunity.OpportunityType, selfOpportunity.Potential, selfOpportunity.stageValidation, selfOpportunity.SelectedOwnerText, selfOpportunity.DateValidation, selfOpportunity.ProductType]);
    selfOpportunity.saveOpportunity = function () {

        selfOpportunity.errors.showAllMessages();
        if (selfOpportunity.errors().length > 0) {
            validationScroll();
            return;
        }
        var jsondata = ko.toJSON(selfOpportunity);
        var operation = "", type = "";
        if (selfOpportunity.OpportunityID() > 0) {
            operation = "UpdateOpportunity";
            type = "put";
        }
        else {
            operation = "InsertOpportunity";
            type = "post";
        }

        pageLoader();
        var authToken = readCookie("accessToken");
        $.ajax({
            url: WEBSERVICE_URL + '/Opportunity',
            type: type,
            data: jsondata,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + authToken);
            },
            success: function (data) {
                $('.success-msg').remove();
                notifySuccess('[|Successfully saved the opportunity|]');
                removepageloader();
                if (localStorage.getItem("ContactOpportunity") == "person")
                    window.location.href = "/person/" + readCookie("contactid") + "/" + readCookie("contactindex");
                else if (localStorage.getItem("ContactOpportunity") == "company")
                    window.location.href = "/company/" + readCookie("contactid") + "/" + readCookie("contactindex")
                else if (localStorage.getItem("ContactOpportunity") == "grid")
                    window.location.href = "/contacts";
                else
                    window.location.href = "/opportunities";
            },
            error: function (response) {
                removepageloader();
                notifyError(response.responseText);
            }
        });
    };

    selfOpportunity.cancelopportunity = function () {
        if (localStorage.getItem("ContactOpportunity") == "person")
            window.location.href = "/person/" + readCookie("contactid") + "/" + readCookie("contactindex");
        else if (localStorage.getItem("ContactOpportunity") == "company")
            window.location.href = "/company/" + readCookie("contactid") + "/" + readCookie("contactindex");
        else if (localStorage.getItem("ContactOpportunity") == "grid")
            window.location.href = "/contacts";
        else
            window.location.href = "/opportunities";
    };

    selfOpportunity.editOpportunity = function () {
        window.location.href = "/editopportunity?opportunityID=" + selfOpportunity.OpportunityID();
    };


    selfOpportunity.deleteOpportunity = function () {
        alertifyReset("Delete Opportunity", "Cancel");
        var array = [];
        array.push(selfOpportunity.OpportunityID());
        var confirmMesaage = "[|Are you sure you want to delete this Opportunity|]?";
        commondelete(confirmMesaage, array);
    };

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
                    setTimeout(
                            function () {
                                window.location.href = "/opportunities";
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

    function dispalyPotential(potential) {
        var currencyFormat = data.Currency;

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

    selfOpportunity.imagePath = ko.observable();
    selfOpportunity.ProfileImage = ko.observable();
    selfOpportunity.ImageAssign = function () {
        selfOpportunity.Image = ko.observable(data.Image);
        if (selfOpportunity.Image().ImageContent != null && selfOpportunity.Image().ImageContent != "undefined") {
             //selfOpportunity.imagePath(selfOpportunity.Image().ImageContent);
            // selfOpportunity.Image.ImageContent = null;
        }
        else {
            selfOpportunity.Image().ImageContent = imagePath;
        }
    }

    selfOpportunity.ImageAssign();

};