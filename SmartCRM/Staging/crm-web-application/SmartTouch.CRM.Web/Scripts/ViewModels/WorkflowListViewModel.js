var WorkflowListViewModel = function (data, webserviceurl) {
    var selfWorkflowList = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfWorkflowList));

    selfWorkflowList.editWorkflow = function () {
        var checkedvalues = fnGetCheckedWorkflows('chkworkflow');
        if (checkedvalues.length == 0) {
            notifyError("[|Please select at least one Worklow|]");
        } else if (checkedvalues.length == 1 && checkedvalues[0].WorkflowStatus == WorkflowTypes.Draft) {
            window.location.href = "editworkflow?WorkflowID=" + checkedvalues[0].WorkflowID;
        } else if (checkedvalues.length == 1 && checkedvalues[0].WorkflowStatus == WorkflowTypes.Paused || checkedvalues[0].WorkflowStatus == WorkflowTypes.Active) {
            window.location.href = "/fulleditworkflow?WorkflowID=" + checkedvalues[0].WorkflowID;
            //notifyError("[|Please select Draft workflows for editing|]");
        } else if (checkedvalues.length == 1 && checkedvalues[0].WorkflowStatus == WorkflowTypes.InActive) {
            notifyError("[|Please select only Draft, Paused, Active  workflows for editing|]");
        } else if (checkedvalues.length > 1) {
            notifyError("[|Please select only one Workflow|]");
        }
    };


    selfWorkflowList.viewWorkflow = function () {
        var checkedvalues = fnGetCheckedWorkflows('chkworkflow');
        if (checkedvalues.length == 0) {
            notifyError("[|Please select at least one Worklow|]");
        } else if (checkedvalues.length == 1 && checkedvalues[0].WorkflowStatus != WorkflowTypes.Draft) {
            //window.location.href = "workflowreport?WorkflowID=" + checkedvalues[0].WorkflowID;
            notifyError("[|Please select Draft workflows for viewing|]");
        } else if (checkedvalues.length == 1 && checkedvalues[0].WorkflowStatus == WorkflowTypes.Draft) {
            window.location.href = "editworkflow?WorkflowID=" + checkedvalues[0].WorkflowID;
        } else if (checkedvalues.length > 1) {
            notifyError("[|Please select only one Worklow|]");
        }
    };


    selfWorkflowList.saveWorkflowAs = function () {
        var checkedvalues = fnGetChkvalGrid('chkworkflow');
        if (checkedvalues != "") {
            if (checkedvalues.length == 1)
                window.location.replace("copyworkflow?WorkflowID=" + checkedvalues);
            else
                notifyError("[|Please select only one workflow|]");
        }
        else
            notifyError("[|Please select at least one workflow|]");
    }

    selfWorkflowList.deleteWorkflow = function () {
        var checkedvalues = fnGetCheckedWorkflows('chkworkflow');
        console.log(checkedvalues);
        if (checkedvalues.length > 0) {
            var nondeletableworkflows = ko.utils.arrayFilter(checkedvalues, function (item) {
                return item.WorkflowStatus != WorkflowTypes.Draft;
            });

            if (nondeletableworkflows.length > 0) {
                notifyError("[|Please select only draft workflows to delete.|]");
                return;
            }


            alertifyReset("Delete Workflow", "Cancel");
            var message = "";
            if (checkedvalues.length > 1)
                message = "[|Are you sure you want to delete |]" + checkedvalues.length + "[| Workflows?|]";
            else
                message = "[|Are you sure you want to delete this Workflow?|]";

            alertify.confirm(message, function (e) {
                if (e) {
                    var cid = [];
                    ko.utils.arrayForEach(checkedvalues, function (item) {
                        cid.push(item.WorkflowID);
                    });

                    console.log(typeof(cid),cid);

                    var authToken = readCookie("accessToken");
                    var jsondata = JSON.stringify({ 'WorkflowIDs': cid });
                    pageLoader();
                    $.ajax({
                        url: webserviceurl + '/Workflows',
                        type: 'delete',
                        dataType: 'json',
                        contentType: "application/json; charset=utf-8",
                        beforeSend: function (xhr) {
                            xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                        },
                        data: jsondata,
                        success: function () {                        
                            if (checkedvalues.length > 1)
                                notifySuccess('[|Successfully deleted the workflows|]');
                            else
                                notifySuccess('[|Successfully deleted the workflow|]');
                            removepageloader();
                            setTimeout(
                                   function () {
                                       window.location.href = "/workflows";
                                   }, setTimeOutTimer);
                        },
                        error: function (response) {
                            removepageloader();
                            notifyError(response.responseText);
                        }
                    });

                }
                else {
                    notifyError("[|You've clicked Cancel|]");
                }
            });
        }
        else {
            notifyError("[|Please select at least one Workflow|]");
        }
    }
}

