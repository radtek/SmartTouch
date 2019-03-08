var EmailValidatorViewModel = function (data,url) {
    selfEmailValidator = this;

    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfEmailValidator));

    selfEmailValidator.IsTags = ko.observable('true');
    selfEmailValidator.Tags = ko.observableArray([]);
    selfEmailValidator.SearchDefinitions = ko.observableArray([]);
    selfEmailValidator.TotalContactsCount = ko.observable(0);

    selfEmailValidator.IsTags.subscribe(function (value) {
        if (value == 'false') {
            selfEmailValidator.Tags([]);
            $("#email-tag-selection").addClass('hide');
            $("#email-search-selection").removeClass('hide');
        }
        else {
            selfEmailValidator.SearchDefinitions([]);
            $("#email-search-selection").addClass('hide');
            $("#email-tag-selection").removeClass('hide');
        }
    })

    selfEmailValidator.validateEmails = function () {
        if(selfEmailValidator.Tags().length == 0 && selfEmailValidator.SearchDefinitions().length == 0)
            return notifyError("Please select atleast one Tag/Saved Search to validate emails.");
        else {
            var confirmMessage = "List(s) queued for cleaning and validation. Fee for List Cleaning is based on number of Email Addresses on the list plus a USD$125.00 processing fee. List Cleaning will be processed upon receipt of payment and once complete, you will receive a Notification Email to check back to the Email Validator screen for results. Clean Emails will have a status of Verified and Bad Emails will have a status of Hard Bounce. Click OK to proceed with List Clean or click Cancel to exit without cleaning.";
            alertifyReset();
            alertify.confirm(confirmMessage, function (e) {
                if (e) {
                    var mappedTagsData = [];
                    var mappedSearchDefinitionsData = [];
                    if (selfEmailValidator.Tags().length > 0) {
                        mappedTagsData = ko.utils.arrayMap(selfEmailValidator.Tags(), function (tag) {
                            return tag.TagID();
                        });
                    }

                    if (selfEmailValidator.SearchDefinitions().length > 0) {
                        mappedSearchDefinitionsData = ko.utils.arrayMap(selfEmailValidator.SearchDefinitions(), function (searchDefinition) {
                            return searchDefinition.SearchDefinitionID();
                        });
                    }

                    pageLoader();
                    var action = "EmailsValidation";
                    $.ajax({
                        url: url + action,
                        type: 'post',
                        dataType: 'json',
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify({
                            'tagIds': mappedTagsData, 'searchDefinitionIds': mappedSearchDefinitionsData, 'totalCount': selfEmailValidator.TotalContactsCount()
                        })
                    }).then(function (response) {
                        var filter = $.Deferred()
                        if (response.success) {
                            filter.resolve(response)
                        } else {
                            filter.reject(response.error)
                        }
                        return filter.promise()
                    }).done(function (data) {
                        //$('.success-msg').remove();
                        notifySuccess("The Email validation list successfully queued.");
                        removepageloader();
                        setTimeout(function () {
                            window.location.href = "/scrubqueue";
                        }, 1000);
                    }).fail(function (error) {
                        notifyError(error);
                    });
                }
                else {
                    notifyError("[|You've clicked Cancel|]");
                }
            });



        }
    }

    selfEmailValidator.cancel = function () {
        window.location.href = "/scrubqueue";
    }

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

    selfEmailValidator.errors = ko.validation.group(selfEmailValidator);
}