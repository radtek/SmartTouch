var AddTagViewModel = function (data, url, isContactTag, notselectall) {
    var selfTag = this;
    ko.mapping.fromJS(data, {}, selfTag);

    ko.validation.rules['minimumLength'] = {
        validator: function (contacts, minimumLength) {
            return (contacts.length > minimumLength);
        },
        message: '[|Select at least one contact|]'
    };
    ko.validation.registerExtenders();

    if (notselectall == false)
        data.SelectAll = true;
    selfTag.notselectall = ko.observable(notselectall);
    selfTag.SelectAll = ko.observable(data.SelectAll);


    selfTag.PopularOrRecent = ko.observableArray([{ Popular: true, Recent: true }]);
    selfTag.PopularTags = ko.observableArray(data.PopularTags);
    selfTag.RecentTags = ko.observableArray(data.RecentTags);
    appendCheckbox();

    //selfTag.RecentClicked = function () {
    //    alert("Recent Clicked");
    //    selfTag.PopularOrRecent.replace(selfTag.PopularOrRecent()[0], {
    //        Popular: false,
    //        Recent : true
    //    });
    //    appendCheckbox();
    //}
    //selfTag.PopularClicked = function () {
    //    alert("Popular Clicked");
    //    selfTag.PopularOrRecent.replace(selfTag.PopularOrRecent()[0], {
    //        Popular: true,
    //        Recent: false
    //    });
    //    appendCheckbox();
    //}

    selfTag.TagsList = ko.observableArray(data.TagsList);

    selfTag.Contacts = ko.observableArray(data.Contacts);

    selfTag.TagSelected = function (value) {
     
        if (value.TagName != null) {
            if (selfTag.TagsList().length == 0) {
                $("#addTags").addTag(value.TagName(), { UID: value.TagID() });
            }
            else {
                var tagIndex;
                var filter = ko.utils.arrayFilter(selfTag.TagsList(), function (Tag, index) {                 
                    if (Tag.TagName === value.TagName()) {
                        tagIndex = index;
                    }
                    return Tag.TagName === value.TagName();
                });
                if (filter.length == 0) {
                    $("#addTags").addTag(value.TagName(), { UID: value.TagID() });
                }
                else {
                    $("#addTags").removeTag(value.TagName());
                }
            }
        }
        return true;
    }

  

    selfTag.TagsComputed = ko.computed({
        read: function () {
            var TagNames = "";
            if (selfTag.TagsList() != null) {
                $.each(selfTag.TagsList(), function (index, value) {
                    if (TagNames != null && TagNames != "" && value.TagName != null)
                        TagNames = TagNames + "," + value.TagName;
                    else
                        TagNames = TagNames + value.TagName;
                });
            }
            return TagNames;
        },
        write: function () { }

    });

    var selectedOpportunities = GetSelectedOpportunities('chkopportunity');
    if (selectedOpportunities.length != 0) {
        var arrayOpportunities = [];
        $.each(selectedOpportunities, function (index, value) {
            arrayOpportunities.push({ OpportunityID: value.OpportunityID, OpportunityName: value.OpportunityName, Contacts: null });
        });
        selfTag.Opportunities = ko.observableArray(arrayOpportunities);
    }


    if (checkedContactValues.length > 0 || localStorage.getItem("contactdetails") != null) {
        if (selfTag.SelectAll() == false)
            selfTag.Contacts(selectedContacts(data.AddTagID, checkedContactValues, data.Contacts));
        else if (selfTag.SelectAll() == true)
            selfTag.Contacts(selectedContacts(0, checkedContactValues, data.Contacts));
    }

    selfTag.ContactFullNames = ko.computed({
        read: function () {
            var contactFullNames = "";
            if (selfTag.Contacts() != null) {
                $.each(selfTag.Contacts(), function (index, value) {
                    if (contactFullNames != null && contactFullNames != "" && value.FullName != null)
                        contactFullNames = contactFullNames + "," + value.FullName;
                    else
                        contactFullNames = contactFullNames + value.FullName;
                });
            }
            return contactFullNames;
        },
        write: function () { },
        owner: this
    });

    selfTag.OpportunityNames = ko.computed({
        read: function () {
            var opportunityNames = "";
            if (selfTag.Opportunities() != null) {
                $.each(selfTag.Opportunities(), function (index, value) {
                    if (opportunityNames != null && opportunityNames != "" && value.OpportunityName != null)
                        opportunityNames = opportunityNames + "," + value.OpportunityName;
                    else
                        opportunityNames = opportunityNames + value.OpportunityName;
                });
            }
            return opportunityNames;
        },
        write: function () { },
        owner: this
    });
   
    if (isContactTag == "True" && selfTag.SelectAll() == false)
        selfTag.contactsValidation = selfTag.ContactFullNames.extend({ minimumLength: { params: 1, message: "[|Select at least one contact|]" } });
    //else
    //    notifyError("Select at least one opportunity");
    //selfTag.opportunitiesValidation = selfTag.OpportunityNames.extend({ minimumLength: { params: 1, message: "Select at least one opportunity" } });

    selfTag.TagsValidation = selfTag.TagsComputed.extend({ required: { message: "[|Select at least one tag|]" } });

    selfTag.errors = ko.validation.group(selfTag, true);

    selfTag.SaveAddTag = function () {
        if (checkedContactValues.length == 0 && selfTag.SelectAll() == true && isContactTag == "True")
        {
            notifyError("Select at least one Contact");
        }
        else
        {
            if (selfTag.notselectall() == false) {
                selfTag.SelectAll = ko.observable(true);
            }

            if (isContactTag != "True" && selfTag.Opportunities() == null)
                notifyError("[|Select at least one Opportunity|]");
            else {
                var jsondata = ko.toJSON(selfTag);

                selfTag.errors.showAllMessages();
                if (selfTag.errors().length > 0)
                    return;
                innerLoader('addTag');

                $.ajax({
                    url: url + "SaveContactTags",
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ 'addTagViewModel': jsondata })
                }).then(function (response) {
                    var filter = $.Deferred();
                    if (response.success) {
                        filter.resolve(response);
                    } else {
                        filter.reject(response.error);
                    }
                    return filter.promise();
                }).done(function () {
                    removeinnerLoader('addTag');
                    if (isContactTag == "True")
                        notifySuccess('[|Successfully tagged contacts|]');
                    else
                        notifySuccess('[|Successfully tagged opportunities|]');
                    setTimeout(
                        function () {
                            createCookie('log', false, 1);
                            window.location.href = document.URL;
                        }, setTimeOutTimer);
                }).fail(function (error) {
                    removeinnerLoader('addTag');
                    notifyError(error);
                });
            }
        }
        }
}
checkedContactValues = fnGetCheckedValues();