var leadScoreViewModel = function (data, url, campaignsurl, webServiceUrl, allowedModules) {
    selfLeadScore = this;
    ko.validation.init();
    ko.validation.configure({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null,
        grouping: { deep: true, observable: true, live: true }
    });

    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfLeadScore));

    ko.validation.rules['conditionCannotEqual'] = {
        validator: function (condition, otherVal) {
            return (condition != otherVal);
        },
        message: '[|Select condition|]'
    };

    ko.validation.registerExtenders();
    selfLeadScore.Template = ko.observable('');
    selfLeadScore.errors = ko.validation.group(selfLeadScore, {
        observable: true,
        deep: true,
        live: true
    });
    selfLeadScore.AllTags = ko.observableArray([]);
    selfLeadScore.Campaigns = ko.observableArray([]);
    selfLeadScore.Forms = ko.observableArray([]);

    selfLeadScore.saveText = ko.observable("Save");
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfLeadScore));
    selfLeadScore.ContactTags = ko.pureComputed({
        read: function () {

            return "";
        },
        write: function (newValue) {
        },
        owner: this
    });

    selfLeadScore.Score = ko.observable(data.Score).extend({
        required: {
            message: "[|Points is required|]"
        },
        pattern: {
            params: '^0*[1-9][0-9]*$',
            message: "[|Points must be positive number|]"
        }
    });
    selfLeadScore.ConditionDescription = ko.observable(data.ConditionDescription).extend({
        required: {
            message: "[|Description is required|]"
        }
    });

    selfLeadScore.TourTypes = ko.observableArray(data.TourTypes);
    selfLeadScore.NoteCategories = ko.observableArray(data.NoteCategories);
    selfLeadScore.ActionTypes = ko.observableArray(data.ActionTypes);
    selfLeadScore.ConditionValue = ko.observable(data.ConditionValue);
    selfLeadScore.Categories = ko.observableArray(data.Categories);
    selfLeadScore.AllConditions = ko.observableArray(data.Conditions);
    selfLeadScore.Conditions = ko.observableArray([]);
    selfLeadScore.CampaignRequired = ko.observable('');
    selfLeadScore.IsNewTag = ko.observable(true);
    var categoryArray = [];
    var conditionArray = [];

    function pushingScoreCategories(moduleId) {
        selfLeadScore.Categories().filter(function (e) {
            if (e.ModuleID == moduleId) {
                categoryArray.push(e);
            }
        })
    }

    function pushingScoreConditions(moduleId) {
        selfLeadScore.AllConditions().filter(function (e) {
            if (e.ModuleID == moduleId) {
                conditionArray.push(e);
            }
        })
    }

    $.each(allowedModules, function (ind, val) {
        pushingScoreCategories(val.Module);
        pushingScoreConditions(val.Module);
    });
    pushingScoreCategories(null);
    pushingScoreConditions(null);


    selfLeadScore.Categories(categoryArray);
    selfLeadScore.AllConditions(conditionArray);

    selfLeadScore.LeadSources = ko.observableArray(data.LeadSources);
    //selfLeadScore.Conditions = ko.observableArray(data.Conditions).extend({ required: true });
    selfLeadScore.SelectedCampaignID = ko.observableArray(data.SelectedCampaignID);
    selfLeadScore.SelectedFormID = ko.observableArray(data.SelectedFormID);
    selfLeadScore.SelectedCampaignLinkID = ko.observableArray(data.SelectedCampaignLinkID);
    selfLeadScore.NoOfLinks = ko.observable("[|No links|]");
    selfLeadScore.SelectedTagID = ko.observableArray(data.SelectedTagID);
    selfLeadScore.SelectedTourTypeID = ko.observableArray(data.SelectedTourTypeID);
    selfLeadScore.SelectedLeadSourceID = ko.observableArray(data.SelectedLeadSourceID);
    selfLeadScore.SelectedNoteCategoryID = ko.observableArray(data.SelectedNoteCategoryID);
    selfLeadScore.SelectedActionTypeID = ko.observableArray(data.SelectedActionTypeID);

    selfLeadScore.DateFormat = ko.observable(data.DateFormat);

    selfLeadScore.DisplayCreatedDate = kendo.toString(kendo.parseDate(ConvertToDate(data.CreatedOn)), selfLeadScore.DateFormat());

    selfLeadScore.DisplayCreatedDate = ko.pureComputed({
        read: function () {
            if (selfLeadScore.CreatedOn() == null) {
                return new Date().toUtzDate();
            }
            else {
                var dateString = selfLeadScore.CreatedOn().toString();
                var dateFormat = readCookie("dateformat").toUpperCase();
                if (dateString.indexOf('/Date') == 0) {
                    var utzdate = ConvertToDate(selfLeadScore.CreatedOn()).toUtzDate();
                    selfLeadScore.CreatedOn(utzdate);
                    return moment(utzdate).format(dateFormat);
                }
                else {
                    var date = Date.parse(selfLeadScore.CreatedOn());
                    return ConvertToDate(date.toString());
                }
            }
        },
        write: function (newValue) {
            selfLeadScore.CreatedOn(new Date(newValue));
        }
    });

    //selfLeadScore.Template = ko.observable(data.ConditionID);

    selfLeadScore.ConditionID = ko.observable(data.ConditionID);
    selfLeadScore.CategoryID = ko.observable(data.CategoryID);
    selfLeadScore.CategoryIDValidation = selfLeadScore.CategoryID.extend({
        required: {
            message: '[|Category is required|]'
        }
    });
    selfLeadScore.ConditionIDValidation = selfLeadScore.ConditionID.extend({
        required: {
            message: 'Condition is requried'
        }
    });

    if (data.ConditionID == 2 || data.ConditionID == 1) {
        getCampaigns();
    } else if (data.ConditionID == 3) {
        getForms();
    } else {
        selfLeadScore.Template(selfLeadScore.ConditionID().toString());
    }


    selfLeadScore.CampaignRequired = ko.observable();
    selfLeadScore.CampaignLinks = ko.observableArray(data.CampaignLinks);




    //selfLeadScore.CategoryID.subscribe(function (selectedCategory) {
    //    selfLeadScore.ConditionID('');
    //});


    selfLeadScore.ConditionValueValidation = selfLeadScore.ConditionValue.extend({
        required: {
            onlyIf: function () {
                return selfLeadScore.ConditionID() == "4" || selfLeadScore.ConditionID() == "5";
            },
            message: function () {
                return selfLeadScore.ConditionID() == "4" ? "[|Website is required|]" : "[|Web page is required|]";
            }
        }
    });

    //selfLeadScore.Categories = ko.observableArray([
    //    { Id: 1, Name: "[|Campaigns|]" },
    //    { Id: 2, Name: "[|Forms|]" },
    //    { Id: 3, Name: "[|Websites|]" },
    //    { Id: 4, Name: "[|Contacts|]" },

    //]);

    //selfLeadScore.AllConditions = ko.observableArray([
    //    { Id: 1, Name: "A contact opens an email", CategoryID: 1 },
    //    { Id: 2, Name: "A contact clicks a link", CategoryID: 1 },
    //    { Id: 3, Name: "A contact submits a form", CategoryID: 2 },
    //    { Id: 4, Name: "A contact visits a web site", CategoryID: 3 },
    //    { Id: 5, Name: "A contact visits a web page", CategoryID: 3 },
    //    { Id: 6, Name: "An action includes the tag", CategoryID: 4 },
    //    { Id: 7, Name: "A note includes the tag", CategoryID: 4 },
    //    { Id: 8, Name: "The lead source is", CategoryID: 4 },
    //    { Id: 9, Name: "The tour type is", CategoryID: 4 },
    //    { Id: 23, Name: "An Email is sent", CategoryID: 4 }
    //]);

    selfLeadScore.Conditions = ko.pureComputed(function () {
        return $.grep(selfLeadScore.AllConditions(), function (e) {
            return e.ScoreCategoryID == selfLeadScore.CategoryID();
        });
    });

    selfLeadScore.LeadSources = ko.observableArray(data.LeadsourceTypes);










    selfLeadScore.SelectedCampaignID.subscribe(function (text) {
        if (selfLeadScore.ConditionID() == 2 && text.length > 0) {
            $.ajaxSettings.traditional = true
            $.ajax({
                url: campaignsurl + 'GetCampaignLinks',
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'campaignId': selfLeadScore.SelectedCampaignID() })
            }).then(function (response) {
                var filter = $.Deferred()
                if (response.success) {
                    filter.resolve(response)
                } else {
                    filter.reject(response.error)
                }
                return filter.promise()
            }).done(function (data) {
                selfLeadScore.CampaignLinks(data.CampaignLinks);
                if (selfLeadScore.CampaignLinks().length == 1) {
                    selfLeadScore.NoOfLinks("[|Only 1 Link|]");
                } else if (selfLeadScore.CampaignLinks().length > 1) {
                    selfLeadScore.NoOfLinks(selfLeadScore.CampaignLinks().length + " [|Links|]");
                } else {
                    selfLeadScore.NoOfLinks("[|No Links|]");
                }
                selfLeadScore.SelectedCampaignLinkID(selfLeadScore.SelectedCampaignLinkID());
            }).fail(function (error) {
                notifyError(error);
            })
        } else if (selfLeadScore.ConditionID() == 2 && text.length == 0)
            selfLeadScore.CampaignLinks([]);
    });



    selfLeadScore.SelectedFormText = ko.observable();



    selfLeadScore.WebAnalyticsProviders = ko.observableArray([]);
    $.ajax({
        url: webServiceUrl + "/getaccountwebanalyticsproviders",
        type: 'get',
        dataType: 'json',
        contentType: "application/json; charset=utf-8",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("Authorization", "Bearer " + readCookie("accessToken"));
        },
        data: {
            'accountId': selfLeadScore.AccountID()
        },
        success: function (data) {
            selfLeadScore.WebAnalyticsProviders(data.WebAnalyticsProviders);
        },
        error: function (response) {
            notifyError(response.responseText);
        }
    });



    selfLeadScore.ConditionID.subscribe(function (selectedvalue) {
        if (selectedvalue == 1 || selectedvalue == 2) {
            getCampaigns();
        }
        else if (selectedvalue == 3)
            getForms();
        else {
            selfLeadScore.Template(selfLeadScore.ConditionID().toString());
        }
    });



    selfLeadScore.TagsList = ko.observableArray(data.TagsList);
    selfLeadScore.showTags = ko.pureComputed(function () {
        return selfLeadScore.Template() == 6 || selfLeadScore.Template() == 7
    });



    selfLeadScore.errors = ko.validation.group(selfLeadScore, true);

    var saveRule = function (saveType, target) {
        var jsondata = ko.toJSON(selfLeadScore);
        var rule = saveType;

        selfLeadScore.errors.showAllMessages();
        if (selfLeadScore.errors().length > 0)
            return;
        innerLoader('leadscore');
        $.ajax({
            url: url + rule,
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: jsondata
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            removeinnerLoader('leadscore');
            notifySuccess("[|Successfully saved the rule|]");
            setTimeout(function () { window.location.href = document.URL }, setTimeOutTimer);
        }).fail(function (error) {
            removeinnerLoader('leadscore');
            selfLeadScore.IsNewTag(true);
            notifyError(error);
        })
    };

    selfLeadScore.insertRule = function (item, event) {
        savedSuccessfully = true;
        saveRule("InsertLeadScore", event.target);
    };

    selfLeadScore.updateRule = function (item, event) {
        saveRule("UpdateLeadScore", event.target);
    };

    selfLeadScore.cancel = function (item, event) {
        CloseTopInner(event.target);
    };

    selfLeadScore.ScoreTagElementID = ko.observable('');
    selfLeadScore.PlaceHolderName = ko.pureComputed(function () {
        return selfLeadScore.TagsList().length > 0 ? '[|Type a tag name|]' : '[|All|]'
    });

    selfLeadScore.TagsList.subscribe(function () {
        $("#" + selfLeadScore.ScoreTagElementID() + "_tag").attr('placeholder', selfLeadScore.PlaceHolderName());
    });

    selfLeadScore.ConditionChange = function (e) {
        selfLeadScore.SelectedCampaignID([]);
        selfLeadScore.SelectedCampaignLinkID([]);
        selfLeadScore.SelectedFormID([]);
        selfLeadScore.ConditionValue("");
        selfLeadScore.TagsList([]);
        selfLeadScore.SelectedTagID([]);
        selfLeadScore.SelectedLeadSourceID([]);
        selfLeadScore.SelectedTourTypeID([]);
        selfLeadScore.SelectedNoteCategoryID([]);
        selfLeadScore.PageDurationContdition.PageVisited("");
        $("#" + selfLeadScore.ScoreTagElementID() + "_tagsinput").find('span.tag').remove();
    }
    var minutesDuration = 0;
    var secondsDuration = 0;
    if (selfLeadScore.ConditionID() == "26") {
        seconds = ko.utils.arrayFirst(data.LeadScoreConditionValues, function (item) {
            return item.ValueType == 1;
        }).Value;
        minutesDuration = Math.floor(seconds / 60, 0)
        secondsDuration = seconds % 60;
    }
    selfLeadScore.PageDurationContdition = {
        Minutes: ko.observable(minutesDuration),
        Seconds: ko.observable(secondsDuration),
        AnyPage: ko.observable(selfLeadScore.ConditionValue() == "*" ? true : false),
        PageVisited: ko.observable(selfLeadScore.ConditionValue() == "*" ? '' : selfLeadScore.ConditionValue()).extend({
            required: {
                message: function () {
                    // notifyError("Web page is required");
                    return "[|Web page is required|]"
                },
                onlyIf: function () {
                    return selfLeadScore.ConditionID() == '26';
                }
            }
        })
    }

    selfLeadScore.PageDurationContdition.AnyPage.subscribe(function (value) {
        if (value == true)
            selfLeadScore.ConditionValue('*');
        else
            selfLeadScore.ConditionValue("");
    });

    selfLeadScore.PageDurationContdition.PageVisited.subscribe(function (value) {
        selfLeadScore.ConditionValue(value);
    });

    selfLeadScore.PageDurationOf = ko.utils.arrayFirst(selfLeadScore.LeadScoreConditionValues(), function (item) {
        return item.ValueType() == 1
    });

    if (!selfLeadScore.PageDurationOf) {
        selfLeadScore.PageDurationOf = {
            LeadScoreConditionValueId: ko.observable(),
            LeadScoreRuleId: ko.observable(),
            Value: ko.observable(),
            ValueType: ko.observable()

        }
    }
    selfLeadScore.PageDuration = ko.computed({
        read: function () {
            var totalDuration = selfLeadScore.PageDurationContdition.Minutes() * 60 + selfLeadScore.PageDurationContdition.Seconds();
            selfLeadScore.PageDurationOf.Value(totalDuration);
            return totalDuration;
        },
        write: function (newValue) {
        },
        owner: this
    }).extend({
        min: 1
    });
    selfLeadScore.PageDurationContdition.minutesValidation = selfLeadScore.PageDurationContdition.Minutes.extend({
        required: {
            message: "[|Minutes are required|]",
            onlyIf: function () {
                return selfLeadScore.ConditionID() == 26;
            }
        },
        max: 59,
        min: 0
    });
    selfLeadScore.PageDurationContdition.secondsValidation = selfLeadScore.PageDurationContdition.Seconds.extend({
        required: {
            message: "[|Seconds are required|]",
            onlyIf: function () {
                return selfLeadScore.ConditionID() == 26;
            }
        },
        max: 59,
        min: 0
    });

    function getTags() {
        if (selfLeadScore.AllTags().length == 0) {
            getAllTags().done(function (tagsdata) {
                selfLeadScore.AllTags(tagsdata);
                selfLeadScore.SelectedTagID(selfLeadScore.SelectedTagID());
                selfLeadScore.Template(selfLeadScore.ConditionID().toString());
            }).fail(function (err) {
                notifyError(err.responseText);
            });
        } else {
            selfLeadScore.Template(selfLeadScore.ConditionID().toString());
        }
    }

    function getCampaigns() {

        if (selfLeadScore.Campaigns() == null || selfLeadScore.Campaigns().length == 0) {
            $.ajax({
                url: url + 'GetCampaigns',
                type: 'get',
                dataType: 'json',
                contentType: "application/json; charset=utf-8"
            }).then(function (response) {
                var filter = $.Deferred()
                if (response.success) {
                    filter.resolve(response)
                } else {
                    filter.reject(response.error)
                }
                return filter.promise()
            }).done(function (data) {

                selfLeadScore.Campaigns(data.response);
                selfLeadScore.SelectedCampaignID(selfLeadScore.SelectedCampaignID());
                selfLeadScore.Template(selfLeadScore.ConditionID().toString());
            }).fail(function (error) {
                notifyError(error);
            });
        } else {
            selfLeadScore.Template(selfLeadScore.ConditionID().toString());
        }
    }

    function getForms() {

        //if (selfLeadScore.Forms().length == 0) {
        $.ajax({
            url: url + 'GetForms',
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8"
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {

            selfLeadScore.Forms(data.response);

            selfLeadScore.Template(selfLeadScore.ConditionID().toString());
            selfLeadScore.SelectedFormID(selfLeadScore.SelectedFormID());

        }).fail(function (error) {
            notifyError(error);
        });
        //} else {

        //    selfLeadScore.Template(selfLeadScore.ConditionID().toString());
        //    selfLeadScore.SelectedFormID(selfLeadScore.SelectedFormID());

        //}
    }

    function getLeadSources() {
    }
    function getTourTypes() {
    }
}







