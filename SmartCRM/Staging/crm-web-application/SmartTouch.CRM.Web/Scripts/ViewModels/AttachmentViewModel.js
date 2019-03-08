var AttachmentViewModel = function (url, pageName, dateFormat, contactid, opportunityid) {

    var selfAttachment = this;



    var options = {
        // Required. Called when a user selects an item in the Chooser.
        success: function (files) {
            var contactid = null, opportunityid = null;
            if (pageName == "contacts")
                contactid = viewModel1.ContactID();
            else
                opportunityid = viewModel1.OpportunityID();

            $.ajax({
                url: url + "SaveAttachment",
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'filesViewModel': files, 'page': pageName, 'StorageSource': 'D', 'ContactID': contactid, 'OpportunityID': opportunityid })
            }).then(function (response) {
                var filter = $.Deferred();
                if (response.success) {
                    filter.resolve(response);
                } else {
                    filter.reject(response.error);
                }
                return filter.promise();
            }).done(function () {
                $('.success-msg').remove();
                selfAttachment.GetNewData();
            }).fail(function (error) {
                notifyError(error);
            });
        },
        cancel: function () {
        },
        linkType: "preview", // or "direct"
        multiselect: true, // or true
        extensions: ['.pdf', '.doc', '.docx', '.jpg', '.jpeg', '.png', '.xls', '.txt', '.csv', '.rtf']
    };



    try {
        var button = Dropbox.createChooseButton(options);
        document.getElementById("container").appendChild(button);
    }
    catch (ex) {
        console.log("Dropbox exception: " + ex.message);
    }



    selfAttachment.DateFormat = ko.observable(dateFormat);
    selfAttachment.PageNumber = ko.observable(1);
    selfAttachment.AttachmentData = ko.observableArray([]);


    selfAttachment.TotalHits = ko.observable(0);

    selfAttachment.DisplayEndMessage = ko.pureComputed(function () {
        if (selfAttachment.AttachmentData().length == selfAttachment.TotalHits()) {
            return true;
        } else {
            return false;
        }
    });

    selfAttachment.deleteDocument = function (document) {
        alertifyReset("Delete Attachment", "Cancel");
        alertify.confirm("Are you sure you want to delete this attachement?", function (e) {
            if (e) {
                pageLoader();
                var varDeleteURL = url + "DeleteAttachment?DocID=" + document.DocumentID;
                jQuery.support.cors = true;
                $.ajax({
                    url: varDeleteURL,
                    type: 'post',
                    dataType: 'json',
                    contentType: 'application/json; charset=utf-8'
                }).then(function (response) {
                    var filter = $.Deferred();
                    if (response.success) {
                        filter.resolve(response);
                    } else {
                        filter.reject(response.error);
                    }
                    return filter.promise();
                }).done(function () {
                    notifySuccess('Attachment deleted successfully');
                    selfAttachment.AttachmentData.remove(document);
                    removepageloader();

                }).fail(function (error) {
                    notifyError(error);
                })
            }
            else {
                notifyError("You've clicked Cancel");
            }
        });
    }

    selfAttachment.GetDataFromNextSection = function () {
        innerLoader("Attachment");
        pageLoader();
        var AttachmentViewModel = {
            PageNumber: selfAttachment.PageNumber(),
            PageName: pageName,
            ContactID: contactid,
            OpportunityID: opportunityid
        };

        $.ajax({
            url: url + "GetAllAttachments",
            type: 'post',
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: ko.toJSON(AttachmentViewModel),
            success: function (data) {
                if (selfAttachment.PageNumber() == 1) {
                    selfAttachment.AttachmentData(data.Data);
                }
                else {
                    ko.utils.arrayPushAll(selfAttachment.AttachmentData, data.Data);
                }
                removepageloader();
                selfAttachment.TotalHits(data.Total);
                removeinnerLoader("attachment");
            }
        });
    }

    selfAttachment.GetNewData = function () {
        selfAttachment.PageNumber(1);
        selfAttachment.GetDataFromNextSection();
    }

    var hasAttachmentsFetched = false;
    selfAttachment.GetAttachments = function () {
        if (!hasAttachmentsFetched) {
            selfAttachment.GetDataFromNextSection();
            hasAttachmentsFetched = true
        }
    }
    $(document).scroll(function () {
        if ($(window).scrollTop() == ($(document).height() - $(window).height())) {
            if (selfAttachment.AttachmentData().length == selfAttachment.TotalHits()) {
                return;
            } else {
                selfAttachment.PageNumber(selfAttachment.PageNumber() + 1);
                GetDataFromNextSection();
            }
        }
    });



}