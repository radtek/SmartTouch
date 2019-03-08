var newCampaignViewModel = function () {
    var selfNewCampaign = this;
    selfNewCampaign.Show = ko.observable(false);
    selfNewCampaign.Name = ko.observable()
        .extend({
            required: {
                message: "Campaign name is required"
            }
        });
    selfNewCampaign.CampaignTypeId = ko.observable("131");
    selfNewCampaign.TagsList = ko.observableArray([]);
    var authToken = readCookie("accessToken");

    selfNewCampaign.Proceed = function () {
        if (selfNewCampaign.errors().length > 0) {
            selfNewCampaign.errors.showAllMessages();
            return;
        }
        var jsonData = ko.toJSON(selfNewCampaign);
        pageLoader();
        console.log(jsonData);

        $.ajax({
            url: WEBSERVICE_URL + '/Campaign',
            type: 'post',
            data: jsonData,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + authToken);
            },
            success: function (response) {
                //   removepageloader();
                //notifySuccess('[|Successfully created the Campaign. Redirecting to edit page...|]');
                setTimeout(function () {
                    removepageloader();
                    window.location.replace("/editcampaign?campaignId=" + response.CampaignViewModel.CampaignID);
                }, 500);
            },
            error: function (response) {
                notifyError(response.responseText);
                setTimeout(function () {
                    removepageloader();
                    if (response.responseText == "The campaign cannot be saved   the deleted links are associated with automation workflow") {
                        alertifyReset();
                        alertify.confirm("[|Do you want to reload the page to get the original links?|]", function (e) {
                            if (e) {
                                location.reload();
                            }
                        });
                    }
                }, setTimeOutTimer);
            }
        });
    }

    selfNewCampaign.changeType = function (e) {
        ee = this;
        console.log(ee);
        //selfNewCampaign.CampaignTypeId
    }
    $('.campaign-type-select').click(function (e) {
        selfNewCampaign.CampaignTypeId(e.currentTarget.getAttribute('id'));
    })
    selfNewCampaign.errors = ko.validation.group(selfNewCampaign);
}
