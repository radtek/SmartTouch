var MarketingMessageContentviewModel = function (data,msgCount) {
    if (data === undefined) {
        data = {};
    }
    var selfMessage = this;
    selfMessage.MarketingMessageContentMapID = ko.observable(data.MarketingMessageContentMapID);
    selfMessage.MarketingMessageId = ko.observable(data.MarketingMessageId);
    selfMessage.Icon = ko.observable(data.Icon).extend({ required: { message: "[|Icon is required|]" } });
    selfMessage.Subject = ko.observable(data.Subject == undefined ? '' : data.Subject).extend({ required: { message: "[|Subject is required|]" }, maxLength: 75 });
    selfMessage.Content = ko.observable(data.Content).extend({ required: { message: "[|Message is required|]" } });
    selfMessage.IsDeleted = ko.observable(false);
    selfMessage.SelectIcon = function (data, e) {
        $(e.target.parentElement).find('button').each(function (element) {
            $(this).removeClass('btn-primary');
        });
        $(e.target).addClass('btn-primary');
        selfMessage.Icon($(e.target).attr('data-cname'));
    }
    selfMessage.iconValidation = selfMessage.Icon.extend({ required: { message: "[|Icon is required|]" } });
    ko.validation.configure({
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null,
        grouping: {
            deep: true
        }
    });

    ko.validation.registerExtenders();
    selfMessage.PushingRedactorContent = function (e, index) {
        selfMarketingMessage.Messages()[index].Content(e.target.innerHTML);
    }
    loadRedactor = function (index) {
        if (!$('#' + index).parent().hasClass('redactor-box')) {
            setTimeout(
            function () {
                $("#" + index).redactor({
                    focus: true,
                    plugins: ['bufferbuttons', 'fontfamily', 'fontcolor', 'fontsize', 'imagemanager', 'table', 'fullscreen', 'textdirection', 'video', 'underline'],
                    source: true,
                    imageManagerJson: 'Contact/GetCampaignImages',
                    uploadImageField: true,
                    imageUpload: 'Contact/UploadImage',
                    imageUploadErrorCallback: function(json)
                    {
                        notifyError(json.error);
                    },
                    buttons : ['html', 'formatting', 'bold', 'italic', 'deleted', 'unorderedlist', 'orderedlist', 'outdent', 'indent', 'image', 'link', 'alignment', 'horizontalrule', 'html'],
                    paragraphize: false,
                    replaceDivs: false,
                    boldTag: 'b',
                    fixed: false,
                    //pastePlainText: true,
                    convertImageLinks: true,
                    cleanOnPaste: false,
                    linebreaks: true,
                    deniedTags: ['html', 'head', 'link', 'body', 'meta', 'script', 'applet']
                });
                $('#new-msg-'+index +' .redactor-editor').blur(function (e) {
                    selfMessage.PushingRedactorContent(e,index);
                });
            }, 1000);
        }
    }

    selfMessage.errors = ko.validation.group(selfMessage);
    selfMessage.RemoveMessage = function (e) {
        if (msgCount == 1)
        {
            notifyError("[|Message Center needs atleat one message.|]")
        }
        else {
            msgCount = msgCount - 1;
            selfMarketingMessage.MessagesCount(msgCount);
            selfMessage.IsDeleted(true);
        }
        
    }
}