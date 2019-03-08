var ApplicationTourViewModel = function (data, url, accountId) {
    selfAppTour = this;
    ko.mapping.fromJS(data, {}, selfAppTour);

    ko.validation.registerExtenders();

    selfAppTour.ApplicationTourDetailsID = ko.observable(data.ApplicationTourDetailsID);
    selfAppTour.disableSelection = ko.observable(true);
    selfAppTour.ApplicationTourDetailsID.subscribe(function (id) {
        if(id)
            selfAppTour.disableSelection(false);
    });
    selfAppTour.Sections = ko.pureComputed({
        read: function () {
            if (data != null && data.length > 0) {
                ko.utils.arrayForEach(data, function (d) {
                    return d.Name = d.DivisionName + " - " + d.SectionName;
                });
            }
            return data;
        },
    });

    selfAppTour.Title = ko.observable(data.Title).extend({ required: { message: "Title is required" } });
    selfAppTour.Content = ko.observable(data.Content).extend({ required: { message: "Content is required" } });
    selfAppTour.ChangeSection = function (e) {

        console.log("in selection");
        var applicationDetailsId = selfAppTour.ApplicationTourDetailsID();
        console.log(selfAppTour.ApplicationTourDetailsID());
        var dataItem;
        if (e.sender.dataItem(e.item).ApplicationTourDetailsID != -1) {
             dataItem = this.dataItem(e.item.index() + 1);
        }
        else {
            dataItem = null;          
        }


        if (selfAppTour.ApplicationTourDetailsID() != undefined) {

            alertifyReset("Ok", "Cancel");            
            alertify.confirm("[|Any changes made to content for the selected section will be lost if not saved , Are you sure?|]", function (e) {
                if (e) {                  
                   if (dataItem != null) {
                        $("#editor").redactor('code.set', dataItem.Content);
                        selfAppTour.Title(dataItem.Title);
                        selfAppTour.ApplicationTourDetailsID(dataItem.ApplicationTourDetailsID);
                        selfAppTour.Content(dataItem.Content);
                   }
                   else {
                       $("#editor").redactor('code.set', '');
                       selfAppTour.Title("");
                       selfAppTour.Content("");
                       selfAppTour.ApplicationTourDetailsID(0);
                   }
                }
                else {
                    notifyError("[|You've clicked Cancel|].");
                    console.log(applicationDetailsId);
                    $("#appTour").data("kendoDropDownList").value(applicationDetailsId);
                    selfAppTour.ApplicationTourDetailsID(applicationDetailsId);
                }
            });
        }
        else {
            if (dataItem != null) {
                $("#editor").redactor('code.set', dataItem.Content);
                selfAppTour.Title(dataItem.Title);
                selfAppTour.ApplicationTourDetailsID(dataItem.ApplicationTourDetailsID);
                selfAppTour.Content(dataItem.Content);
            }
            else {
                $("#editor").redactor('code.set', '');
                selfAppTour.Title("");
                selfAppTour.Content("");
                selfAppTour.ApplicationTourDetailsID(0);
            }
        }
    };

    selfAppTour.saveTourCMS = function () {

        var content = $("#editor").redactor('code.get');
        
        if (content != null && content.replace(/<br>/g, "").trim() != "") {
            section = ko.utils.arrayFirst(selfAppTour.Sections(), function (item) {
                return item.ApplicationTourDetailsID == selfAppTour.ApplicationTourDetailsID();
            });
            if (section != null) {
                if (content == "<br>")
                    content = "";
                selfAppTour.Content(content);

                if (selfAppTour.errors().length > 0) {
                    notifyError(selfAppTour.errors().toString().replace(",", ", \r\n"));
                    return;
                }
                var updateCMSViewModel = {};
                updateCMSViewModel.ApplicationTourDetailsID = selfAppTour.ApplicationTourDetailsID();
                updateCMSViewModel.Title = selfAppTour.Title();
                updateCMSViewModel.Content = content
                var jsondata = ko.toJSON(updateCMSViewModel);
                //console.log(stringified);
               
                pageLoader();
                $.ajax({
                    url: url + "UpdateTour",
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ 'updateCMSViewModel': jsondata })
                }).then(function (response) {
                    var filter = $.Deferred();
                    if (response.success)
                        filter.resolve(response)
                    else
                        filter.reject(response.error)
                    return filter.promise()
                }).done(function (data) {
                    notifySuccess('Successfully saved the content');
                    setTimeout(function () {
                        removepageloader();
                        location.reload();
                    }, setTimeOutTimer);
                }).fail(function (error, status, error1) {
                    removepageloader();
                    myError = error;
                    myE = status;
                    my = error1;
                    notifyError(error);
                });
            }
            else
                notifyError("Please select section");
        }
        else
            notifyError("Please enter content for the selected section");
    };

    selfAppTour.cancel = function () {
        window.location.reload();
    };
    selfAppTour.errors = ko.validation.group(selfAppTour);
}