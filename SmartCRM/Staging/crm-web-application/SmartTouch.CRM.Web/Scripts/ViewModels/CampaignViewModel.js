var CampaignViewModel = function (data, url) {
    var selfCampaign = this;

    var USER_LOGGED_IN = 1;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfCampaign));
  
    selfCampaign.CreatedBy = ko.observable(USER_LOGGED_IN);
  
}


