var SendCampaign = function (data, url) {
    var selfSendCampaign = this;
    ko.mapping.fromJS(data, {}, selfSendCampaign);

    selfSendCampaign.templates = ko.observableArray();
    var templates = data.CampaignTemplates;
    $.each(templates, function (i, v) {
        var template = new Object();
        $.each(v, function (n, value) {
            template.Name = value;
        });
        selfSendCampaign.templates.push(template);
    });
};