var campaigStatisticsViewModel = function (data, webApp, service, requestedFrom, LitmusEmailGuid, mailTesterGuid) {
    selfCampaignStats = this;
    ko.mapping.fromJS(data, {}, selfCampaignStats);
    selfCampaignStats.saveCampaignAs = function () {
        if (selfCampaignStats.CampaignId() === 0) { return; }
        window.location.replace("/SaveCampaignAs?campaignId=" + selfCampaignStats.CampaignId());
    }
    
    selfCampaignStats.uniqueOpens = [];
    selfCampaignStats.uniqueClicks = [];
    selfCampaignStats.linkUniqueClicks = [];
    
    selfCampaignStats.SentOn = ko.computed({
        read: function () {
            if (selfCampaignStats.SentOn() == null) {
                return new Date().toUtzDate();
            }
            else {
                var dateString = selfCampaignStats.SentOn();
                var dateFormat = readCookie("dateformat").toUpperCase();
                if (dateString.indexOf('/Date') == 0) {
                    var utzdate = ConvertToDate(selfCampaignStats.SentOn()).ToUtcUtzDate();
                    selfCampaignStats.SentOn(utzdate).toString();
                    return moment(utzdate).format(dateFormat + " hh:mm A");
                }
                else {
                    var date = Date.parse(selfCampaignStats.SentOn());
                    return ConvertToDate(date.toString());
                }
            }
        },
        write: function (newValue) {
            selfCampaignStats.SentOn(new Date(newValue));
        }
    });
    selfCampaignStats.NewCampaign = new newCampaignViewModel();
    selfCampaignStats.ResetNewCampaign = function () {
        selfCampaignStats.NewCampaign = new newCampaignViewModel();
        location.reload(true);
    };

    selfCampaignStats.Calculate = ko.pureComputed(function () {
        return ko.utils.arrayFilter(selfCampaignStats.Opens(), function (item) {

            var duplicate;
            if (item.LinkIndex() == null || item.LinkIndex() == 0) {
                duplicate = selfCampaignStats.uniqueOpens.indexOf(item.RecipientId());
                if (duplicate < 0) {
                    selfCampaignStats.uniqueOpens.push(item.RecipientId());
                }
            }
            else {
                duplicate = selfCampaignStats.linkUniqueClicks.map(function (e) { return e.RecipientId }).indexOf(item.RecipientId());
                var duplicateClick = selfCampaignStats.linkUniqueClicks.map(function (e) { return e.LinkIndex }).indexOf(item.LinkIndex());

                if (duplicate < 0 || duplicateClick < 0 || duplicate != duplicateClick) {

                    selfCampaignStats.uniqueClicks.push(item.RecipientId());
                    selfCampaignStats.linkUniqueClicks.push({ RecipientId: item.RecipientId(), LinkIndex: item.LinkIndex() });

                }
            }
        });
    });

    var openRate = function () {
        return selfCampaignStats.Delivered() == 0 ? 0 : (data.Opened / selfCampaignStats.Delivered()) * 100;
    }
    var clickRate = function () {
        return selfCampaignStats.Delivered() == 0 ? 0 : (data.Clicked / selfCampaignStats.Delivered()) * 100;
    }
    selfCampaignStats.ContactTags = ko.observableArray(data.ContactTags);
    selfCampaignStats.SearchDefinitions = ko.observableArray(data.SearchDefinitions);

    selfCampaignStats.UnSubscribed = ko.observable(data.UnSubscribed);
    selfCampaignStats.Complained = ko.observable(data.Complained);
    selfCampaignStats.Bounced = ko.observable(data.Bounced);
    selfCampaignStats.Blocked = ko.observable(data.Blocked);
    selfCampaignStats.Delivered = ko.observable(data.Delivered);

    selfCampaignStats.OpenRate = openRate().toFixed(2);
    selfCampaignStats.ClickRate = clickRate().toFixed(2);
    selfCampaignStats.hasUniqueLinks = ko.observable(false);
    selfCampaignStats.LitmusGuid = ko.observable(LitmusEmailGuid);
    selfCampaignStats.MailTesterGuid = ko.observable(mailTesterGuid);
    selfCampaignStats.CampaignName = ko.observable(data.CampaignName);

    selfCampaignStats.ReRunLitmusTest = function () {

    }

    selfCampaignStats.getUniqueLinkClicks = function (linkIndex) {
        var clickCount = 0;
        $.each(selfCampaignStats.linkUniqueClicks, function (index, value) {
            if (value.LinkIndex == linkIndex) {
                clickCount = clickCount + 1;
                selfCampaignStats.hasUniqueLinks(true);
            }
        });
        return clickCount;
    }

    selfCampaignStats.deleteCampaign = function () {
        if (selfCampaignStats.CampaignId() === 0) { return; }
        alertifyReset("Delete Campaign", "Cancel");
        alertify.confirm("[|Are you sure you want to delete this Campaign?|]", function (e) {
            if (e) {
                var cid = [selfCampaignStats.CampaignId()];
                var jsondata = JSON.stringify({ 'CampaignID': cid });
                var varDeleteURL = "DeleteCampaign";

                jQuery.support.cors = true;
                $.ajax({
                    url: varDeleteURL,
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ 'campaignIDs': jsondata }),
                    success: function (data) {
                        notifySuccess("[|Successfully deleted the campaign|]");
                        if (data.success === true) {
                            window.location.href = "/campaigns";
                        }
                    },
                    error: function () {
                        notifySuccess("[|Campaign could not be deleted.|]");
                    }
                });
            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }
        });
    }

    selfCampaignStats.isURL = function (str) {
        var pattern = new RegExp('^(https?:\\/\\/)?' + // protocol
        '((([a-z\\d]([a-z\\d-]*[a-z\\d])*)\\.?)+[a-z]{2,}|' + // domain name
        '((\\d{1,3}\\.){3}\\d{1,3}))' + // OR ip (v4) address
        '(\\:\\d+)?(\\/[-a-z\\d%_.~+]*)*' + // port and path
        '(\\?[;&a-z\\d%_.~+=-]*)?' + // query string
        '(\\#[-a-z\\d_]*)?$', 'i'); // fragment locator
        return pattern.test(str);
    }

    selfCampaignStats.isImage = function (url) {
        if (url != null && url != 'undefined')
            return (url.match(/\.(jpeg|jpg|gif|png)/) != null);
        else
            return false;
    }

    function replaceAll(str, find, replace) {
        return str.replace(new RegExp(find, 'g'), replace);
    }
    selfCampaignStats.Html = ko.observable(replaceAll(data.CampaignViewModel.HTMLContent, 'src', 'lsrc'));

    selfCampaignStats.ProcessHTML = function () {
        $.each(selfCampaignStats.UniqueClicks(), function (index, item) {
            console.log(item.LinkIndex());
            var element = "a[href *= 'linkid=" + item.LinkIndex() + "']";
            var content = $(selfCampaignStats.Html()).find(element);
            if (content != null && content.length > 0) {
                var imageTag = content[0].innerHTML ? $(content[0]).find('img') : undefined;
                if (imageTag && imageTag.length > 0) {
                    item.LinkText = ko.observable(imageTag[0].getAttribute("lsrc"));
                }
                else {
                    item.LinkText = ko.observable(content[0].innerHTML);
                }
            }
        });
    }();
}