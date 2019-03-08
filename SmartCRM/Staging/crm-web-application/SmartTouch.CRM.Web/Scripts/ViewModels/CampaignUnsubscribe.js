var campaignUnsubscribeViewModel = function (BASE_URL, contactId, emailID, campaignId, workflowId, privacyPolicy, websiteURL) {
    console.log(emailID);
    var selfCampaignUnsubscribe = this;
    selfCampaignUnsubscribe.CampaignEmailStatus = ko.observable(false);
    selfCampaignUnsubscribe.cID = ko.observable(contactId);
    selfCampaignUnsubscribe.emailId = ko.observable(emailID);
    selfCampaignUnsubscribe.campaignId = ko.observable(campaignId);
    selfCampaignUnsubscribe.workflowId = ko.observable(workflowId);
    // selfCampaignUnsubscribe.snoozeperiod=ko.observablearray
    selfCampaignUnsubscribe.snoozeDays = ko.observable();
    

    var currentyear = '© ' + new Date().getFullYear();

    selfCampaignUnsubscribe.CurrentYear = ko.observable(currentyear);

    selfCampaignUnsubscribe.snoozeperiod = ko.observableArray([
     { SnoozeID: 30, SnoozeDays: '30 [|Days|]' },
     { SnoozeID: 60, SnoozeDays: '60 [|Days|]' },
     { SnoozeID: 90, SnoozeDays: '90 [|Days|]' }
    ]);

    selfCampaignUnsubscribe.UpdateEmailStatus = function ()
    {
        if (selfCampaignUnsubscribe.CampaignEmailStatus() == false) {
            notifyError("[|Please select your option for Unsubscribe and save your preferences|]");
            return;
        }
        pageLoader();
        //  var data_param = cId=' + contactId + '&emailId=' + emailID;
        console.log('ContactID' + contactId);
        console.log('emailID' + emailID);
        
        $.ajax({
            url: "campaignEmailUpdate",
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: { 'contactID': selfCampaignUnsubscribe.cID(), 'emailID': selfCampaignUnsubscribe.emailId(), 'campaignId': selfCampaignUnsubscribe.campaignId(), 'snoozeperiod': selfCampaignUnsubscribe.snoozeDays(), 'workflowId': selfCampaignUnsubscribe.workflowId() },
            success: function (data) {
                removepageloader();
                if (data.success === true) {
                    notifySuccess(data.response);
                    if (websiteURL)
                        window.location.href = websiteURL;
                    else if (privacyPolicy)
                        window.localStorage.href = privacyPolicy;
                }
                if (data.success === false) {
                    notifyError(data.response);
                }
            },
            error: function () {
                removepageloader();
            }
        });

    };

};