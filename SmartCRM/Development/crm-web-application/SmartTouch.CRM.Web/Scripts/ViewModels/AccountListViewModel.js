var accountlistViewModel = function (data, url) {
    var selfAccountList = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfAccountList));

    selfAccountList.editAccount = function () {
       var checkedvalues = fnGetChkvalGrid('chkaccount');
        if (checkedvalues != "") {
            if (checkedvalues.length == 1) {
                window.location.href = "../editaccount?accountId=" + checkedvalues;
            }
            else {
                notifyError("[|Please select only one account|]");
            }
        }
        else {
            notifyError("[|Please select only one account|]");
        }
    };

    selfAccountList.copyAccount = function () {
       var checkedvalues = fnGetChkvalGrid('chkaccount');
        if (checkedvalues != "") {
            if (checkedvalues.length == 1) {
                window.location.href = "../copyaccount?accountId=" + checkedvalues;
            }
            else {
                notifyError("[|Please select only one account|]");
            }
        }
        else {
            notifyError("[|Please select only one account|]");
        }
    };

    selfAccountList.deleteAccount = function () {
       var checkedvalueIds = fnGetChkvalGrid('chkaccount');
        var checkedAccountName = fnGetChkvalAccountName();
        if (checkedvalueIds != "" && checkedAccountName.Name.length != "") {
            alertifyReset("Pause Account", "Cancel");
            //alertify.confirm("Are you sure you want to delete this Account(s)?", function (e) {
            alertify.confirm("[|Are you sure you want to permanently DELETE the selected Account(s)|]?", function (e) {
                if (e) {
                    pageLoader();
                    var aid = checkedvalueIds;
                    var names = checkedAccountName;
                    var jsondata = JSON.stringify({ 'AccountID': aid, 'StatusID': 6, 'AccountNames': names.Name });
                   var varDeleteURL = url + "DeleteAccount";

                    jQuery.support.cors = true;
                    $.ajax({
                        url: varDeleteURL,
                        type: 'post',
                        dataType: 'json',
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify({ 'accountData': jsondata })
                    }).then(function (response) {
                        var filter = $.Deferred();
                        if (response.success) {
                            filter.resolve(response);
                        }
                        else {
                            filter.reject(response.error);
                        }
                        return filter.promise();
                    }).done(function (data) {
                     
                        notifySuccess("[|Successfully deleted the account(s)|]");
                        if (data.success === true) {
                            setTimeout(
                                function () {
                                    removepageloader();
                                    window.location.href = "/accounts";
                                }, setTimeOutTimer);

                        }
                    }).fail(function (error) {
                        removepageloader();
                        notifyError(error);
                    });

                }
                else {
                    //notifyError("You've clicked Cancel");
                    notifyError("[|Delete Account request canceled|]");
                }
            });
        }
        else {
            notifyError("[|Please select at least one account|]");
        }
    };

    selfAccountList.terminateAccount = function () {
       var checkedvalues = fnGetChkvalGrid('chkaccount');
        var checkedAccountName = fnGetChkvalAccountName();
        if (checkedvalues != "" && checkedAccountName.Name.length != "") {
            alertifyReset("Close Account", "Cancel");
            //alertify.confirm("Are you sure you want to close this Account(s)?", function (e) {
            alertify.confirm("[| Are you sure you want to close the selected Account(s)|]?", function (e) {
                if (e) {
                    pageLoader();
                    var aid = checkedvalues;
                    var names = checkedAccountName;
                    var jsondata = JSON.stringify({ 'AccountID': aid, 'StatusID': 4, 'AccountNames': names.Name });
                    var varDeleteURL = url + "DeleteAccount";

                    jQuery.support.cors = true;
                    $.ajax({
                        url: varDeleteURL,
                        type: 'post',
                        dataType: 'json',
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify({ 'accountData': jsondata })
                    }).then(function (response) {
                        var filter = $.Deferred();
                        if (response.success) {
                            filter.resolve(response);
                        }
                        else {
                            filter.reject(response.error);
                        }
                        return filter.promise();
                    }).done(function (data) {
                        notifySuccess("[|Successfully closed the account(s)|]");
                        if (data.success === true) {
                            //$("#grid").data("kendoGrid").dataSource.read();                      
                            setTimeout(
                                function () {
                                    removepageloader();
                                    window.location.href = "/accounts";
                                }, setTimeOutTimer);


                        }
                    }).fail(function (error) {
                        removepageloader();
                        notifyError(error);
                    });

                }
                else {
                    //notifyError("You've clicked Cancel");
                    notifyError("[|Close Account request canceled|]");
                }
            });
        }
        else {
            notifyError("[|Please select at least one account|]");
        }
    };

    selfAccountList.excelExport = function () {
        $('#gridAccount').data("kendoGrid").saveAsExcel();
    };
}

