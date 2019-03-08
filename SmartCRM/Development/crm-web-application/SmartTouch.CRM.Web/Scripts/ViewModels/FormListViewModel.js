var formListViewModel = function (data, url) {
   var selfFormList = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfFormList));

    selfFormList.saveForm = function () { }
    selfFormList.cancelForm = function () { }
    selfFormList.editForm = function () {
        checkedvalues = fnGetChkvalGrid('chkform');
        if (checkedvalues != "") {
            if (checkedvalues.length == 1) {
                console.log(fnGetChkAPIForm('chkform'));
                var checkedForms = fnGetChkAPIForm('chkform');
                if (checkedForms && checkedForms[0] == "true")
                    notifyError("[|You can not update API Form, Please select another Form|]");
                else {
                    checkedvaluesstatus = fnGetChkvalStatus('chkform')
                    window.location.href = "editform?formId=" + checkedvalues;
                }
            }
            else {
                notifyError("[|Please select only one form|]");
            }
        }
        else {
            notifyError("[|Please select only one form|]");
        }
    };

    selfFormList.saveFormAs = function () {
        checkedvalues = fnGetChkvalGrid('chkform');
        if (checkedvalues != "") {
            if (checkedvalues.length == 1) {
                var checkedForms = fnGetChkAPIForm('chkform');
                if (checkedForms && checkedForms[0] == "true")
                    notifyError("[|You can not copy API Form, Please select another Form|]");
                else 
                    window.location.replace("saveFormAs?formId=" + checkedvalues);
            }
            else {
                notifyError("[|Please select only one form|]");
            }
        }
        else {
            notifyError("[|Please select only one form|]");
        }
    }

    selfFormList.ViewSubmissions = function (fid) {
        checkedvalues = fnGetChkvalGrid('chkform');
        if (checkedvalues != "") {
            if (checkedvalues.length == 1) {
                window.location.href = "viewsubmissions?formId=" + checkedvalues;
            }
            else {
                notifyError("[|Please select only one form|]");
            }
        }
        else {
            notifyError("[|Please select only one form|]");
        }
    }




    selfFormList.deleteForm = function (fid) {
        checkedvalues = fnGetChkvalGrid('chkform');
        if (checkedvalues != "") {
            alertifyReset("Delete Form", "Cancel");
            var message = "";
            if (checkedvalues.length == 1)
                message = "[|Are you sure you want to delete this Form|]?";
            else
                message = "[|Are you sure you want to delete |] " + checkedvalues.length + " [|Forms ?|]"
            alertify.confirm(message, function (e) {
                if (e) {
                    pageLoader();
                    var cid = checkedvalues;
                    jsondata = JSON.stringify({ 'FormIDs': cid });
                    varDeleteURL = url + "DeleteForm";

                    jQuery.support.cors = true;
                    $.ajax({
                        url: varDeleteURL,
                        type: 'post',
                        dataType: 'json',
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify({ 'formIDs': jsondata })

                    }).then(function (response) {
                        console.log("response");
                        console.log(response);
                        var filter = $.Deferred();
                        if (response.success) {
                            filter.resolve(response);
                        }
                        else {
                            filter.reject(response.error);
                        }
                        return filter.promise();
                    }).done(function (data) {
                        notifySuccess("[|Successfully deleted the form|]");
                        removepageloader();
                        setTimeout(function () { window.location.href = document.URL }, setTimeOutTimer);
                    }).fail(function (error) {
                        console.log(error);
                        removepageloader();
                        notifyError(error);
                    });
                }
                else {
                    notifyError("[|You've clicked Cancel|]");
                }
            });
        }
        else {
            notifyError("[|Please select at least one form|]");
        }
    }
}