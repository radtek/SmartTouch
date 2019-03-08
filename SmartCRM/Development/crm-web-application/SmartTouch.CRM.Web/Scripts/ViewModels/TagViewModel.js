var TagsViewModel = function (data, url, service, accountId) {
    selfTag = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfTag));
    selfTag.TagName = ko.observable(data.TagName).extend({ required: { message: "[|Tag name is required|]" }, maxLength: 75 });
    selfTag.TagID = ko.observable(data.TagID);
    selfTag.sourceTagID = ko.observable(data.sourceTagID);
    selfTag.sourceTagName = ko.observable(data.sourceTagName);
    selfTag.Tags = ko.observableArray(data.Tags);
    selfTag.Count = ko.observable(data.Count);
    selfTag.AccountID = ko.observable(accountId);
    selfTag.errors = ko.validation.group(selfTag);

    selfTag.deleteTag = function () {
        var checkedvalues = fnGetChkvalGrid('chktag');

        if (checkedvalues != "") {
            var tid = checkedvalues;

            alertifyReset("Delete Tag", "Cancel");

            var confirmmsg = "";
            if (tid.length > 1) {
                confirmmsg = "[|Are you sure you want to delete|] " + tid.length + " [|Tags|]?";
            } else {
                confirmmsg = "[|Are you sure you want to delete this Tag|]?";
            }

            alertify.confirm(confirmmsg, function (e) {
                if (e) {
                    var jsondata = JSON.stringify({ 'TagID': tid });
                    var varDeleteURL = url + "DeleteTag";
                    jQuery.support.cors = true;
                    $.ajax({
                        url: varDeleteURL,
                        type: 'post',
                        dataType: 'json',
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify({ 'tagIds': jsondata })
                    }).then(function(response) {
                        var filter = $.Deferred();
                        if (response.success) {
                            filter.resolve(response);
                        } else {
                            filter.reject(response.error);
                        }
                        return filter.promise();
                    }).done(function() {
                        notifySuccess("[|Successfully deleted tag(s)|]");
                        setTimeout(function() { window.location.href = "tags" }, setTimeOutTimer);
                    }).fail(function(error) {
                        notifyError(error);
                    });
                }
                else {
                    notifyError("[|You've clicked Cancel|]");
                }
            });
        }
        else {
            //var validate = false;
            notifyError("[|Please select at least one tag|]");
        }
    };

    selfTag.editTag = function (data, event) {
        var menuItemId = $(event.target).attr('data-openid');
        var checkedvalues = fnGetChkvalGrid('chktag');
        if (checkedvalues.length == 0 || checkedvalues.length > 1) {
            notifyError("[|Select one tag from the grid.|]");
            return;
        }

        $("#menuPartialItemContent" + menuItemId).html('');
        OpenTopInner("menuItem" + menuItemId, "menuPartialItemContent" + menuItemId);

        $.ajax({
            url: url + "/_EditTag",
            type: 'get',
            contentType: "application/html; charset=utf-8",
            success: function (data) {
                $("#menuPartialItemContent" + menuItemId).html(data);
                var checkedvaluesname = fnGetChkvalName('chktag');
                var viewModel = new tagViewModel(checkedvaluesname[0], checkedvalues[0], service);
                ko.applyBindings(viewModel, document.getElementById('editTag'));
            },
            error: function () {
            }
        });
    };

    selfTag.mergeTag = function (data, event) {
        var menuItemId = $(event.target).attr('data-openid');
        var checkedvalues = fnGetChkvalforTagsGrid('chktag');
        if (checkedvalues.length == 0 || checkedvalues.length > 1) {
            notifyError("[|Select one tag from the grid.|]");
            return;
        }

        var strcontent = checkedvalues[0].split(';')[1];
        var count = checkedvalues[0].split(';')[2];
        if (strcontent == "true") {
            notifyError("[|The selected tag is involved in lead score. You can not merge it|]");
            return;
        }

        $("#menuPartialItemContent" + menuItemId).html('');
        OpenTopInner("menuItem" + menuItemId, "menuPartialItemContent" + menuItemId);
        //var tagslist = [];
        $.ajax({
            url: url + "/_MergeTag",
            type: 'get',
            contentType: "application/html; charset=utf-8",
            success: function (data) {
                $("#menuPartialItemContent" + menuItemId).html(data);
                //var checkedvaluesname = fnGetChkvalName('chktag');
                var viewModel = new tagMergeViewModel(selfTag.Tags(), count, url, service);
                ko.applyBindings(viewModel, document.getElementById('mergeTag'));
            },
            error: function (response) {
                console.log(response);
            }
        });
    };

    selfTag.saveTag = function () {
        selfTag.TagID(data.TagID);

        if (selfTag.TagID() != 0) {
            selfTag.errors.showAllMessages();
            if (selfTag.errors().length == 0) {
                var jsondata = ko.toJSON(selfTag);
                UpdateTag("Tag", jsondata);
            }
        }
    };

    selfTag.addTag = function () {
        selfTag.errors.showAllMessages();
         innerLoader('addTag');
        if (selfTag.errors().length == 0) {
            $.ajax({
                url: '/Contact/AddTag',
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'tagName': selfTag.TagName(), 'contactId': 0, 'tagId': 0 })
            }).then(function(response) {
                var filter = $.Deferred();
                removeinnerLoader('addTag');
                if (response.success) {
                    filter.resolve(response);
                } else {
                    filter.reject(response.error);
                }
                return filter.promise();
            }).done(function(data) {
                removeinnerLoader('addTag');
                if (data.success === false)
                    notifyError(data.response);
                else {
                    notifySuccess('[|Successfully saved the Tag|]');
                    window.location.href = "tags";
                }

            }).fail(function(error) {
                removeinnerLoader('addTag');
                notifyError(error);
            });
        }
    };


    selfTag.savemergedTag = function (count) {
        selfTag.errors.showAllMessages();
        if (selfTag.errors().length > 0)
            return;
        var jsondata = ko.toJSON(selfTag);
        innerLoader('mergeTag');
        MergeTag("MergeTag", jsondata, count);
    };

    selfTag.TagName.subscribe(function (text) {
        var result;
        var l = text.length;
        var lastChar = text.substring(l - 1, l);
        if (lastChar == "*") {
            result = text.substring(0, l - 2);
        }
        else {
            result = text;
        }
        var tag = $(selfTag.Tags()).filter(function () {
            return this.TagName.toString().toLowerCase() == result.toString().toLowerCase();
        })[0];

        if (tag != undefined && tag.TagID != selfTag.sourceTagID()) {
            selfTag.TagID = ko.observable(tag.TagID);
        }
        else {

            selfTag.TagID = ko.observable(0);

        }
    });

    selfTag.mergingTag = function () {
        selfTag.errors.showAllMessages();
        if (selfTag.errors().length > 0)
            return;
        $('#mergeconfirmationdata3').append(selfTag.Count() + ' [|items will be changed from|] <span class="bold">' + selfTag.sourceTagName() + '</span> [|to|]<span class="bold"> ' + selfTag.TagName() + '</span>');
        $('#mergeconfirmationdata3').removeClass('hide');
        $('#mergeBtn').addClass('hide');
        $('#saveBtn').removeClass('hide');
    };
}

var tagViewModel = function (tagName, tagId) {
    var selfEditTag = this;

    selfEditTag.TagName = ko.observable(tagName).extend({
        required: {
            message: '[|Tag name is requried|]'
        },
        maxLength: 75
    });;

    selfEditTag.TagID = ko.observable(tagId);
    selfEditTag.DisplayName = ko.observable();
    selfEditTag.TotalContacts = ko.observable();
    selfEditTag.errors = ko.validation.group(selfEditTag);
    selfEditTag.saveTag = function () {
        if (selfEditTag.TagID() != 0) {
            selfEditTag.errors.showAllMessages();
            if (selfEditTag.errors().length > 0)
                return;
            var jsondata = ko.toJSON(selfEditTag);
            innerLoader('editTag');
            UpdateTag("Tag", jsondata);
        }
    };
}


var tagMergeViewModel = function (data, itemcount) {
    selfMergeTag = this;

    var checkedvalues = fnGetChkvalGrid('chktag');
    var checkedvaluesname = fnGetChkvalName('chktag');
    var checkedvaluesstatus = fnGetChkvalStatus('chktag');
    var tagIdValue = checkedvaluesstatus[0];
    var tagDetails = tagIdValue.split('|');
    selfMergeTag.Tags = ko.observableArray(data);
    selfMergeTag.sourceTagID = ko.observable(checkedvalues[0]);
    selfMergeTag.sourceTagName = ko.observable(checkedvaluesname[0]);
    selfMergeTag.tagCount = ko.observable(tagDetails[1]);
    selfMergeTag.TagName = ko.observable().extend({ required: { message: "[|Tag name is required|]" }, maxLength: 75 });
    selfMergeTag.TagID = ko.observable();

    selfMergeTag.savemergedTag = function () {
        selfMergeTag.errors.showAllMessages();
        if (selfMergeTag.errors().length > 0)
            return;
        var jsondata = ko.toJSON(selfMergeTag);
        innerLoader('mergeTag');
        MergeTag("MergeTag", jsondata, itemcount);
    };

    selfMergeTag.TagName.subscribe(function (text) {
        var result;
        var l = text.length;
        var lastChar = text.substring(l - 1, l);
        var lastbutLeastChar = text.substring(l - 2, l);
        if (lastChar === "*" && lastbutLeastChar === " ") {
            result = text.substring(0, l - 2);
        }

        else {
            result = text;
        }

        var tag = $(selfMergeTag.Tags()).filter(function () {
            return this.TagName.toString().toLowerCase() == result.toString().toLowerCase();
        })[0];

        if (tag != undefined && tag.TagID != selfMergeTag.sourceTagID()) {
            selfMergeTag.TagID = tag.TagID;
        }
        else {
            selfMergeTag.TagID = ko.observable(0);
        }
    });

    selfMergeTag.errors = ko.validation.group(selfMergeTag, { deep: false });

    selfMergeTag.mergingTag = function () {       
        selfMergeTag.errors.showAllMessages();
        if (selfMergeTag.errors().length > 0)
            return;
        $('#mergeconfirmationdata3').append(selfMergeTag.tagCount() + ' [|items will be changed from|] <span class="bold">' + selfMergeTag.sourceTagName() + '</span> [|to|] <span class="bold"> ' + selfMergeTag.TagName() + '</span>');

        $('#mergeconfirmationdata3').removeClass('hide');
        $('#mergeBtn').addClass('hide');
        $('#saveBtn').removeClass('hide');
    };
    

}
