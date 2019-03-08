var campaignlistViewModel = function (data, url) {
    selfCampaignList = this;
    //ko.validatedObservable(ko.mapping.fromJS(data, {}, selfCampaignList));

    selfCampaignList.editCampaign = function () {
        var checkedvalues = fnGetChkvalGrid('chkcampaign');

        if (checkedvalues != "") {
            if (checkedvalues.length == 1) {
                var checkedvaluesstatus = fnGetChkvalStatus('chkcampaign');
                var checkedvaluearray = checkedvaluesstatus[0].split('|');
                if (checkedvaluearray[1] != 105 && checkedvaluearray[1] != 106 && checkedvaluearray[1] != 107 && checkedvaluearray[1] != 108 && checkedvaluearray[1] != 109)
                    window.location.href = "editcampaign?campaignId=" + checkedvalues;
                else
                    notifyError("[|Campaigns with status SENT,ARCHIVE, QUEUED, AUTOMATION or ANALYZING cannot be edited|]");
            }
            else {
                notifyError("[|Please select only one campaign|]");
            }
        }
        else {
            notifyError("[|Please select only one campaign|]");
        }
    };
    selfCampaignList.NewCampaign = new newCampaignViewModel();
    selfCampaignList.ResetNewCampaign = function () {
        selfCampaignList.NewCampaign = new newCampaignViewModel();
        location.reload(true);
    };
    selfCampaignList.addNewCampaign = function () {
        selfCampaignList.NewCampaign.Show(true);
    };
    selfCampaignList.viewCampaign = function () {
        var checkedvalues = fnGetChkvalGrid('chkcampaign');

        if (checkedvalues != "") {
            if (checkedvalues.length == 1) {
                var checkedvaluesstatus = fnGetChkvalStatus('chkcampaign')

                var campaignStatus = checkedvaluesstatus[0].split('|');

                if (campaignStatus[1] == 101 || campaignStatus[1] == 102 || campaignStatus[1] == 103 || campaignStatus[1] == 110)
                    window.location.href = "editcampaign?campaignId=" + checkedvalues;
                else if (campaignStatus[1] == 105 || campaignStatus[1] == 107 || campaignStatus[1] == 109)
                    window.location.href = "CampaignStatistics?campaignId=" + checkedvalues;
                else
                    notifyError("This campaign is being processed. Try again after some time.");
            }
            else {
                notifyError("[|Please select only one campaign|]");
            }
        }
        else {
            notifyError("[|Please select only one campaign|]");
        }
    };


    selfCampaignList.saveCampaignAs = function () {
        var checkedvalues = fnGetChkvalGrid('chkcampaign');
        if (checkedvalues != "") {
            if (checkedvalues.length == 1) {
                window.location.replace("savecampaignas?campaignId=" + checkedvalues);
            }
            else {
                notifyError("[|Please select only one campaign|]");
            }
        }
        else {
            notifyError("[|Please select only one campaign|]");
        }
    }
    selfCampaignList.archiveCampaign = function () {
        var checkedvaluesstatus = fnGetChkvalStatus('chkcampaign')
        var checkedcampaignvalues = [];
        $.each(checkedvaluesstatus, function (index, value) {
            var campaignDetails = value.split('|');
            if (campaignDetails[1] == 105) {
                checkedcampaignvalues.push(campaignDetails[0]);
            }
        });

        if (checkedcampaignvalues.length > 0) {
            alertifyReset("Archive Campaign", "Cancel");
            var message = "";
            if (checkedcampaignvalues.length == 1)
                message = "[|Are you sure you want to Archive this Campaign|]?";
            else
                message = "[|Are you sure you want to archive|] " + checkedcampaignvalues.length + " [|Campaigns|]?"
            alertify.confirm(message, function (e) {
                if (e) {
                    var cid = checkedcampaignvalues;
                    var jsondata = JSON.stringify({ 'CampaignID': cid });
                    var varDeleteURL = url + "ArchiveCampaign";
                    jQuery.support.cors = true;
                    $.ajax({
                        url: varDeleteURL,
                        type: 'post',
                        dataType: 'json',
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify({ 'campaignIDs': jsondata }),
                        success: function (data) {
                            if (data.success === true) {
                                notifySuccess(data.response);
                                setTimeout(function () {
                                    window.location.href = document.URL;
                                }, setTimeOutTimer);
                            }
                            else {
                                notifyError(data.response);
                            }
                        },
                        error: function (x, y, z) {
                        }
                    });

                }
                else {
                    notifyError("[|You've clicked Cancel|]");
                }
            });
        }
        else {
            notifyError('[|Please select at least one Sent Campaign|]');
        }
    }
    selfCampaignList.deleteCampaign = function () {
        var checkedvaluesstatus = fnGetChkvalStatus('chkcampaign')
        var checkedcampaignvalues = [];
        $.each(checkedvaluesstatus, function (index, value) {
            var campaignDetails = value.split('|');

            if (campaignDetails[1] != 105 && campaignDetails[1] != 106 && campaignDetails[1] != 115 && campaignDetails[1] != 109) {
                checkedcampaignvalues.push(campaignDetails[0]);
            }
        });

        if (checkedcampaignvalues.length > 0) {
            alertifyReset("Delete Campaign", "Cancel");
            var message = "";
            if (checkedcampaignvalues.length == 1)
                message = "[|Are you sure you want to delete this Campaign|]?";
            else
                message = "[|Are you sure you want to delete|] " + checkedcampaignvalues.length + " [|Campaigns|]?"
            alertify.confirm(message, function (e) {
                if (e) {
                    pageLoader();
                    var cid = checkedcampaignvalues;
                    var jsondata = JSON.stringify({ 'CampaignID': cid });
                    var varDeleteURL = url + "DeleteCampaign";
                    jQuery.support.cors = true;
                    $.ajax({
                        url: varDeleteURL,
                        type: 'post',
                        dataType: 'json',
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify({ 'campaignIDs': jsondata }),
                        success: function (data) {
                            removepageloader();
                            if (data.success === true) {
                                notifySuccess(data.response);
                                setTimeout(function () {
                                    window.location.href = document.URL;
                                }, setTimeOutTimer);
                            }
                            else {
                                notifyError(data.error);
                            }
                        },
                        error: function (data) {
                            removepageloader();
                        }
                    });

                }
                else {
                    notifyError("[|You've clicked Cancel|]");
                }
            });
        }
        else {
            notifyError('[|Please select at least one Draft or Failed Campaign|]');
        }
    }

}


