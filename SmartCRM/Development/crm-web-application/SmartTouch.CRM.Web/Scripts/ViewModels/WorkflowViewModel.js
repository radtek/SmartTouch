var WorkflowViewModel = function (data, url, webserviceurl, advancedsrhurl, ParendWfId, SenderName) {
    selfWorkflow = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfWorkflow));

    ko.validation.rules['simpleDate'] = {
        validator: function (val) {
            return ko.validation.utils.isEmptyVal(val) || moment(val, 'MM/DD/YYYY').isValid();
        },
        message: '[|Invalid date|]'
    };

    ko.validation.rules.triggerCannotEqual = {
        validator: function (triggerTypeID, otherVal) {
            return (triggerTypeID !== otherVal);
        },
        message: '[|Select trigger|]'
    };

    /* storing default settings  */
    var StatusID = data.StatusID, AllowParallelWorkflows = data.AllowParallelWorkflows,
        DeactivatedOn = data.DeactivatedOn, IsWorkflowAllowedMoreThanOnce = data.IsWorkflowAllowedMoreThanOnce,
        RemoveFromWorkflows = data.RemoveFromWorkflows, TimeSensitive = data.DeactivatedOn == null ? "0" : "1";

    //this is the code for multiselect binding with datatextfield and value field
    ko.bindingHandlers.multiselect = {
        init: function (element, valueAccessor) {
            var value = valueAccessor();
            var data = "", placeholder = "", dataTextField = "", content = value.value;

            if (value.type == 'Form') {
                data = selfWorkflow.Forms();
                placeholder = '[|Select Forms|]';
                dataTextField = 'Name';
            }
            else if (value.type == 'Search') {
                data = selfWorkflow.SmartSearches();
                placeholder = '[|Select Searches|]';
                dataTextField = 'SearchDefinitionName';
            }
            // Create the mutliselect
            ko.applyBindingsToNode(element, {
                kendoMultiSelect: {
                    value: value.value,
                    dataTextField: value.dataTextField,
                    data: value.data,
                    placeholder: value.placeholder,
                    enabled: value.enabled
                }
            });
            var src = $(element).data('kendoMultiSelect').dataSource.data();
        }
    }

    $(':radio').on('change', function () {
        $(this).triggerHandler('click');
    });

    ko.validation.registerExtenders();

    selfWorkflow.WorkflowName = ko.observable(data.WorkflowName).extend({ required: { message: "[|Workflow Name is required|]" }, maxLength: 75 });
    selfWorkflow.WorkflowID = ko.observable(data.WorkflowID);

    selfWorkflow.StartTriggerID = ko.observable("");
    selfWorkflow.StartTriggerName = ko.observable("");
    selfWorkflow.StartTriggerValidation = ko.observable("");
    selfWorkflow.StartTriggerTemplateName = ko.observable("");
    selfWorkflow.ActiveWorkflows = ko.observableArray([]);
    selfWorkflow.RemoveFromWorkflows = ko.observable();
    selfWorkflow.NoWorkflows = ko.observable(false);
    selfWorkflow.PopularTags = ko.observableArray(data.PopularTags);
    selfWorkflow.RecentTags = ko.observableArray(data.RecentTags);
    selfWorkflow.ParentWorkflowID = ko.observable(ParendWfId);
    // Code for Recent and Popular Tags

    selfWorkflow.TagIDs = ko.observableArray(data.TagIDs);

    selfWorkflow.ChangeTimeSensitive = function (val) {
        if (val.IsTimeSensitive() == "0") {
            selfWorkflow.DeactivatedOn('');
        }
        return true;
    }

    selfWorkflow.RemoveFromWorkflows.subscribe(function (data) {
        if (data) {
            $.each(data, function (i, d) {
                if (selfWorkflow.ActiveWorkflows().map(function (e) { return e.WorkflowID; }).indexOf(d) < 0)
                    selfWorkflow.NoWorkflows(true);
                else
                    selfWorkflow.NoWorkflows(false);
            });
        }
    });
    getWorkflows(selfWorkflow.WorkflowID).done(function (workflows) {
        selfWorkflow.ActiveWorkflows(workflows);
        selfWorkflow.RemoveFromWorkflows(data.RemoveFromWorkflows);

    }).fail(function (err) {
        notifyError(err.responseText);
    });

    selfWorkflow.Triggers = ko.observableArray(data.Triggers);

    selfWorkflow.StopTriggerID = ko.observable("");
    selfWorkflow.StopTriggerName = ko.observable("");
    selfWorkflow.StopTriggerValidation = ko.observable("");

    selfWorkflow.WorkflowTriggers = ko.observableArray([
         { TriggerTypeID: 1, FilterTypeID: 3, iconclass: 'st-icon-search-2', TriggerName: '[|A Contact is in a Smart Search|]', TriggerTemplateName: 'SmartSearch' },
         { TriggerTypeID: 2, FilterTypeID: 3, iconclass: 'st-icon-form-submitted', TriggerName: '[|A Form is Submitted|]', TriggerTemplateName: 'FormSubmitted' },
         { TriggerTypeID: 3, FilterTypeID: 3, iconclass: 'st-icon-lifecycle-changes', TriggerName: '[|Life Cycle changes|]', TriggerTemplateName: 'Lifecyclechanges' },
         { TriggerTypeID: 4, FilterTypeID: 3, iconclass: 'st-icon-tag', TriggerName: '[|Tags are applied|]', TriggerTemplateName: 'TagsApplied' },
         { TriggerTypeID: 5, FilterTypeID: 3, iconclass: 'st-icon-bullhorn-2', TriggerName: '[|A Campaign is sent|]', TriggerTemplateName: 'CampaignSent' },
         { TriggerTypeID: 6, FilterTypeID: 3, iconclass: 'st-icon-opputunities', TriggerName: '[|Opportunity status change|]', TriggerTemplateName: 'OpportunityStage' },
         { TriggerTypeID: 9, FilterTypeID: 3, iconclass: 'st-icon-polaroid-2', TriggerName: '[|A Lead Adapter is submitted|]', TriggerTemplateName: 'LeadAdapterSubmitted' },
         { TriggerTypeID: 7, FilterTypeID: 3, iconclass: 'st-icon-link', TriggerName: '[|A Link is clicked|]', TriggerTemplateName: 'LinkClicked' },
         { TriggerTypeID: 10, FilterTypeID: 3, iconclass: 'st-icon-database-add', TriggerName: '[|Lead Score is reached|]', TriggerTemplateName: 'LeadscoreReached' },
         { TriggerTypeID: 11, FilterTypeID: 3, iconclass: 'st-icon-browser-download', TriggerName: '[|Web Page visited|]', TriggerTemplateName: 'WebPageVisited' },
         { TriggerTypeID: 12, FilterTypeID: 3, iconclass: 'st-icon-tick', TriggerName: '[|Action Completed|]', TriggerTemplateName: 'ActionCompleted' },
         { TriggerTypeID: 13, FilterTypeID: 3, iconclass: 'st-icon-pin-2', TriggerName: '[|Tour Completed|]', TriggerTemplateName: 'TourCompleted' }
    ]);

    selfWorkflow.WorkflowActionTypes = ko.observableArray([
        {
            WorkflowActionTypeID: 1, iconclass: 'st-icon-campaign-sent', IsLinkAction: true, WorkflowActionTypeName: '[|Send a Campaign|]', ActionTempalteName: 'SendCampaign', IsVisible: true
        },
        //{ WorkflowActionTypeID: 2,iconclass:'st-icon-speech-bubble-center-2', IsLinkAction: false, WorkflowActionTypeName: '[|Send a Text|]', ActionTempalteName: 'SendText' },
        {
            WorkflowActionTypeID: 3, iconclass: 'st-icon-stopwatch', IsLinkAction: false, WorkflowActionTypeName: '[|Set a Timer|]', ActionTempalteName: 'SetTimer', IsVisible: true
        },
        {
            WorkflowActionTypeID: 4, iconclass: 'st-icon-tags-applied', IsLinkAction: true, WorkflowActionTypeName: '[|Add a Tag|]', ActionTempalteName: 'Tags', IsVisible: true
        },
        {
            WorkflowActionTypeID: 5, iconclass: 'st-icon-tags-remove', IsLinkAction: true, WorkflowActionTypeName: '[|Remove a Tag|]', ActionTempalteName: 'Tags', IsVisible: true
        },
        {
            WorkflowActionTypeID: 6, iconclass: 'st-icon-database-add', IsLinkAction: true, WorkflowActionTypeName: '[|Adjust Lead Score|]', ActionTempalteName: 'LeadScore', IsVisible: true
        },
        {
            WorkflowActionTypeID: 7, iconclass: 'st-icon-lifecycle-changes', IsLinkAction: true, WorkflowActionTypeName: '[|Change Life Cycle|]', ActionTempalteName: 'Lifecycle', IsVisible: true
        },
        {
            WorkflowActionTypeID: 8, iconclass: 'st-icon-marquee-plus', IsLinkAction: true, WorkflowActionTypeName: '[|Update a Field|]', ActionTempalteName: 'UpdateField', IsVisible: true
        },
        {
            WorkflowActionTypeID: 9, iconclass: 'st-icon-user-2-add', IsLinkAction: true, WorkflowActionTypeName: '[|Assign to User(s)|]', ActionTempalteName: 'AssignUser', IsVisible: true
        },
        {
            WorkflowActionTypeID: 10, iconclass: 'st-icon-notify-team', IsLinkAction: true, WorkflowActionTypeName: '[|Notify User|]', ActionTempalteName: 'NotifyUser', IsVisible: true
        },
        {
            WorkflowActionTypeID: 12, iconclass: 'st-icon-envelope', IsLinkAction: true, WorkflowActionTypeName: '[|Send Email|]', ActionTempalteName: 'SendEmail', IsVisible: true
        },
        {
            WorkflowActionTypeID: 13, iconclass: 'st-icon-split', IsLinkAction: true, WorkflowActionTypeName: '[|Trigger workflow|]', ActionTempalteName: 'EnterWorkflow', IsVisible: true
        },
        { WorkflowActionTypeID: 14, iconclass: 'st-icon-split', IsLinkAction: false, WorkflowActionTypeName: '[|Link Actions|]', ActionTempalteName: 'LinkActions', IsVisible: false }
    ]);

    selfWorkflow.Qualifiers = [{ Id: 1, Value: '[|Is Less Than|]' }, { Id: 2, Value: '[|Is Equal To|]' }, { Id: 3, Value: '[|Is Greater Than|]' }];

    var tags_trigger1 = data.Triggers[0].TagIDs == null ? [] : data.Triggers[0].TagIDs; var tags_trigger2 = data.Triggers[1].TagIDs == null ? [] : data.Triggers[1].TagIDs;

    selfWorkflow.PopularClicked = function (trigger) {
        var poptag;
        var rectag;
        $.each(selfWorkflow.PopularTags(), function (index) {
            document.getElementById('populartag' + (index + 1)).checked = false;
        });

        var popularclicked = document.getElementsByClassName("PopularClicked" + trigger);

        if (!$(popularclicked).hasClass('selected')) {

            $(popularclicked).parent('.filters').find('a.selected').removeClass('selected');
            $(popularclicked).addClass('selected');

            poptag = document.getElementById("poptags" + trigger);
            rectag = document.getElementById("recenttags" + trigger);

            $(poptag).removeClass('hide').addClass('show');
            $(rectag).removeClass('show').addClass('hide');
        }
        else {

            $(popularclicked).removeClass('selected');
            poptag = document.getElementById("poptags" + trigger);
            $(poptag).removeClass('show').addClass('hide');
        }
    }
    selfWorkflow.RecentClicked = function (trigger) {

        $.each(selfWorkflow.RecentTags(), function (index) {
            document.getElementById('recenttag' + (index + 1)).checked = false;
        });
        var poptag;
        var rectag;
        var recentclicked = document.getElementsByClassName("RecentClicked" + trigger);
        if (!$(recentclicked).hasClass('selected')) {
            $(recentclicked).parent('.filters').find('a.selected').removeClass('selected');
            $(recentclicked).addClass('selected');
            poptag = document.getElementById("poptags" + trigger);
            rectag = document.getElementById("recenttags" + trigger);

            $(poptag).removeClass('show').addClass('hide');
            $(rectag).removeClass('hide').addClass('show');
        } else {
            $(recentclicked).removeClass('selected');
            rectag = document.getElementById("recenttags" + trigger);
            $(rectag).removeClass('show').addClass('hide');
        }
    }

    selfWorkflow.TagSelected = function (id, name, ischecked, field, trigger, type) {
        //var tagname = name;
        var tagid = id;
        tags_trigger1 = selfWorkflow.Triggers()[0].TagIDs() == null ? [] : selfWorkflow.Triggers()[0].TagIDs();
        tags_trigger2 = selfWorkflow.Triggers()[1].TagIDs() == null ? [] : selfWorkflow.Triggers()[1].TagIDs();
        var istag_exist = $(tags_trigger1).filter(function () {
            return this == tagid;
        })[0];

        var istag_exist1 = $(tags_trigger2).filter(function () {
            return this == tagid;
        })[0];

        if (type == 'T') {
            if (ischecked == true) {
                if (trigger == "true") {
                    if (istag_exist == undefined)
                        tags_trigger1.push(tagid);
                }
                else if (trigger == "false") {
                    if (istag_exist1 == undefined)
                        tags_trigger2.push(tagid);
                }
                selfWorkflow.Triggers()[0].TagIDs(tags_trigger1);
                selfWorkflow.Triggers()[1].TagIDs(tags_trigger2);
            }
            else if (ischecked == false) {
                var index;
                if (trigger == "true" && tags_trigger1.length > 0) {
                    index = tags_trigger1.indexOf(tagid);
                    tags_trigger1.splice(index, 1);
                    selfWorkflow.Triggers()[0].TagIDs(tags_trigger1);
                }
                else if (trigger == "false" && tags_trigger2.length > 0) {
                    index = tags_trigger2.indexOf(tagid);
                    tags_trigger2.splice(index, 1);
                    selfWorkflow.Triggers()[1].TagIDs(tags_trigger2);
                }
            }
        }
        else if (type == 'A') {
            $.grep(viewModel.WorkflowActions(), function (action, index) {
                if (action.WorkflowActionTypeID() == actionTypes.LinkActions || action.WorkflowActionTypeID() == actionTypes.SendCampaign) {
                    $.grep(action.Action().CampaignLinks(), function (link, i) {
                        $.grep(link.Actions(), function (a, k) {
                            if ((a.WorkflowActionTypeID() == actionTypes.AddTag || a.WorkflowActionTypeID() == actionTypes.RemoveTag)
                                && a.Action().Order == trigger) {
                                a.Action().TagID(tagid);
                            }
                        })
                    })
                }
            });

            var workflowactions = selfWorkflow.WorkflowActions();
            $.each(workflowactions, function (index, workflowaction) {
                if (workflowaction.Action().Order == trigger) {
                    if (typeof workflowaction.Action().Action == 'function') {
                        workflowaction.Action().Action().TagID(tagid);
                    }
                    else
                        workflowaction.Action().TagID(tagid);
                }
            });
        }
        return true;
    }
    // end point of the code recent and popular tags

    function isEmpty(obj) {
        if (obj != null || obj == undefined)
            true;
        else
            return Object.keys(obj).length === 0;
    }

    function checkValidations() {
        //var triggers = selfWorkflow.Triggers();
        var starttrigger = selfWorkflow.Triggers()[0];
        var stoptrigger = selfWorkflow.Triggers()[1];
        var stopsaving = false;
        if (starttrigger.TriggerTypeID() == stoptrigger.TriggerTypeID()) {

            var commonelements = [];

            if (starttrigger.TriggerTypeID() == triggerTypes.FormSubmitted && starttrigger.FormIDs() != null && stoptrigger.FormIDs() != null) {

                commonelements = starttrigger.FormIDs().filter(function (n) {
                    return stoptrigger.FormIDs().indexOf(n) != -1;
                });
            } else if (starttrigger.TriggerTypeID() == triggerTypes.SmartSearch && starttrigger.SearchDefinitionIDs() != null && stoptrigger.SearchDefinitionIDs() != null) {
                commonelements = starttrigger.SearchDefinitionIDs().filter(function (n) {
                    return stoptrigger.SearchDefinitionIDs().indexOf(n) != -1;
                });
            } else if (starttrigger.TriggerTypeID() == triggerTypes.TagsApplied && starttrigger.TagIDs() != null && stoptrigger.TagIDs() != null) {
                commonelements = starttrigger.TagIDs().filter(function (n) {
                    return stoptrigger.TagIDs().indexOf(n) != -1;
                });
            } else if (starttrigger.TriggerTypeID() == triggerTypes.LifeCycleChange && starttrigger.LifecycleDropdownValueID() == stoptrigger.LifecycleDropdownValueID()) {
                commonelements.push(starttrigger.LifecycleDropdownValueID());
            } else if (starttrigger.TriggerTypeID() == triggerTypes.CampaignSent && starttrigger.CampaignID() == stoptrigger.CampaignID()) {
                commonelements.push(starttrigger.CampaignID());
            } else if (starttrigger.TriggerTypeID() == triggerTypes.LeadAdapterSubmitted && starttrigger.LeadAdapterIDs() != null && stoptrigger.LeadAdapterIDs() != null) {
                commonelements = starttrigger.LeadAdapterIDs().filter(function (n) {
                    return stoptrigger.LeadAdapterIDs().indexOf(n) != -1;
                });
            }

            if (commonelements.length > 0) {
                notifyError("[|Start Trigger and Goal can not be same|]");
                stopsaving = true;
            }

        }

        selfWorkflow.errors.showAllMessages();

        ko.utils.arrayForEach(selfWorkflow.Triggers(), function (trigger) {
            trigger.errors.showAllMessages();
            if (trigger.errors().length > 0)
                stopsaving = true;
        });
        ko.utils.arrayForEach(selfWorkflow.WorkflowActions(), function (action) {
            if (action.Action() != undefined && action.IsDeleted() == false) {
                if (action.Action().errors().length > 0) {
                    action.Action().errors.showAllMessages();
                    stopsaving = true;
                }
            }

            if (action.Action() != undefined && action.IsDeleted() == true) {
                if (action.WorkflowActionTypeID() == actionTypes.NotifyUser) {
                    action.Action().UserID(0);
                } else if (action.WorkflowActionTypeID() == actionTypes.AssigntoUser) {
                    var assignments = action.Action().RoundRobinContactAssignments();
                    ko.utils.arrayForEach(assignments, function (assg) {
                        assg.UserID("");
                    });
                } else if (action.WorkflowActionTypeID() == actionTypes.AddTag || action.WorkflowActionTypeID() == actionTypes.RemoveTag) {
                    action.Action().TagID(0);
                } else if (action.WorkflowActionTypeID() == actionTypes.ChangeLifeCycle) {
                    action.Action().LifeCycleDropdownValueID(0);
                } else if (action.WorkflowActionTypeID() == actionTypes.SendEmail) {
                    action.Action().FromEmailID(0);
                } else if (action.WorkflowActionTypeID() == actionTypes.AdjustLeadScore) {
                    action.Action().LeadScoreValue(0);
                } else if (action.WorkflowActionTypeID() == actionTypes.SendCampaign) {
                    action.Action().CampaignID(0);
                }
            }
            if (action.WorkflowActionTypeID() == actionTypes.SendCampaign) {
                if (action.Action().CampaignLinks() != undefined) {
                    ko.utils.arrayForEach(action.Action().CampaignLinks(), function (link) {
                        if (link.Actions() != undefined && link.Actions() != '') {
                            ko.utils.arrayForEach(link.Actions(), function (linkAction) {
                                var action = linkAction.Action();
                                if (linkAction.Action() != undefined && linkAction.IsDeleted() == false && linkAction.Action().errors().length > 0) {
                                    linkAction.Action().errors.showAllMessages();
                                    stopsaving = true;
                                }
                                if (action != undefined && linkAction.IsDeleted() == false && action.WorkflowActionTypeID == actionTypes.NotifyUser) {
                                    if (action.UserIds != null) {
                                        if (action.UserIds().length < 1) {
                                            action.UserIds([]);
                                            stopsaving = true;
                                        }
                                        if (action.MessageBody() != null && action.MessageBody().length < 1) {
                                            action.errors.showAllMessages();
                                            stopsaving = true;
                                        }
                                    }
                                }
                                if (action != undefined && linkAction.IsDeleted() == false && action.WorkflowActionTypeID == actionTypes.AssigntoUser) {
                                    ko.utils.arrayForEach(action.RoundRobinContactAssignments(), function (assignment) {
                                        if (assignment.errors().length > 0) {
                                            assignment.errors.showAllMessages();
                                            stopsaving = true;
                                        }
                                    });
                                }
                            })
                        }
                    });
                }
                if (action.Action() != undefined && action.Action().errors().length > 0) {
                    action.Action().errors.showAllMessages();
                    stopsaving = true;
                }
            }
            if (action.WorkflowActionTypeID() == actionTypes.LinkActions) {
                if (action.Action().CampaignLinks() != undefined) {
                    ko.utils.arrayForEach(action.Action().CampaignLinks(), function (link) {
                        if (link.Actions() != undefined && link.Actions() != '') {
                            ko.utils.arrayForEach(link.Actions(), function (linkAction) {
                                console.log(linkAction.IsDeleted())
                                var action = linkAction.Action();
                                
                                if (linkAction.Action() != undefined && linkAction.IsDeleted() == false && linkAction.Action().errors().length > 0) {
                                    linkAction.Action().errors.showAllMessages();
                                    stopsaving = true;
                                }
                                if (action != undefined && linkAction.IsDeleted() == false && action.WorkflowActionTypeID == actionTypes.NotifyUser) {
                                    if (action.UserIds != null) {
                                        if (action.UserIds().length < 1) {
                                            action.UserIds([]);
                                            stopsaving = true;
                                        }
                                        if (action.MessageBody() != null && action.MessageBody().length < 1) {
                                            action.errors.showAllMessages();
                                            stopsaving = true;
                                        }
                                    }
                                }
                                if (action != undefined && linkAction.IsDeleted() == false && action.WorkflowActionTypeID == actionTypes.AssigntoUser) {
                                    ko.utils.arrayForEach(action.RoundRobinContactAssignments(), function (assignment) {
                                        if (assignment.errors().length > 0) {
                                            assignment.errors.showAllMessages();
                                            stopsaving = true;
                                        }
                                    });
                                }
                            })
                        }
                    });
                }
                if (action.Action() != undefined && action.Action().errors().length > 0) {
                    action.Action().errors.showAllMessages();
                    stopsaving = true;
                }
            }

            if (action.WorkflowActionTypeID() == actionTypes.NotifyUser && !action.IsDeleted()) {
                if (action.Action().UserIds != null) {
                    if (action.Action().UserIds().length < 1) {
                        action.Action().UserIds([]);
                        stopsaving = true;
                    }
                }
            }

            if (action.WorkflowActionTypeID() == actionTypes.AssigntoUser && !action.IsDeleted()) {
                if (action.Action().RoundRobinContactAssignments) {
                    ko.utils.arrayForEach(action.Action().RoundRobinContactAssignments(), function (assignment) {
                        if (assignment.errors().length > 0) {
                            assignment.errors.showAllMessages();
                            stopsaving = true;
                        }
                    });
                }
            }

            action.errors.showAllMessages();
            if (action.IsDeleted() != true && action.errors().length > 0)
                stopsaving = true;
            //var fieldInputTypes;
            if (action.WorkflowActionTypeID() == actionTypes.UpdateField
                && (action.Action().FieldInputTypeId() == fieldInputTypes.MultiSelect
                || action.Action().FieldInputTypeId() == fieldInputTypes.Checkbox)) {
                var multiselectvalue = action.Action().FieldValue();
                if (typeof multiselectvalue === 'string') {
                    multiselectvalue = [multiselectvalue];
                }
                var splitvalue = multiselectvalue.join('|');
                action.Action().FieldValue(splitvalue);
            }
        });

        if (selfWorkflow.errors().length > 0) {
            stopsaving = true;
        }
        return stopsaving;
    }

    selfWorkflow.LinkWorkflowActionTypes = ko.pureComputed(function () {
        return ko.utils.arrayFilter(selfWorkflow.WorkflowActionTypes(), function (actiontype) {
            return actiontype.IsLinkAction == true;
        });
    });
    selfWorkflow.VisibleWorkflowActionTypes = ko.pureComputed(function () {
        return ko.utils.arrayFilter(selfWorkflow.WorkflowActionTypes(), function (actiontype) {
            return actiontype.IsVisible == true;
        });
    });
    selfWorkflow.PeriodValues = ko.observableArray([
        {
            PeriodID: 6, PeriodValue: '[|Minutes|]'
        },
        {
            PeriodID: 5, PeriodValue: '[|Hours|]'
        },
        {
            PeriodID: 4, PeriodValue: '[|Days|]'
        },
        {
            PeriodID: 3, PeriodValue: '[|Weeks|]'
        },
        { PeriodID: 2, PeriodValue: '[|Months|]' }
    ]);

    selfWorkflow.MeridianValues = ko.observableArray([
        {
            MeridianID: 1, MeridianValue: '[|AM|]'
        },
        { MeridianID: 2, MeridianValue: '[|PM|]' }
    ]);

    selfWorkflow.Statuses = ko.observableArray([
        {
            StatusID: 401, StatusName: '[|Active|]'
        },
        { StatusID: 402, StatusName: '[|Draft|]' }
    ]);

    selfWorkflow.StatusID = ko.observable(data.StatusID);
    selfWorkflow.IsTimeSensitive = ko.observable(data.DeactivatedOn == null ? "0" : "1");
    selfWorkflow.IsDateEnabled = ko.pureComputed({
        read: function () {
            if (selfWorkflow.IsTimeSensitive() == "0")
                return false;
            else if (selfWorkflow.IsTimeSensitive() == "1")
                return true;
            else
                return false;
        }
    });
    selfWorkflow.DeactivatedOn = ko.observable(data.DeactivatedOn);
    selfWorkflow.DeactivatedOnValidation = selfWorkflow.DeactivatedOn.extend({
        required: {
            message: '[|Deactivated on is required|]',
            onlyIf: function () {
                return (selfWorkflow.IsTimeSensitive() === "1");
            }
        }
    });
    selfWorkflow.IntiateLabel = ko.observable("[| Initiate|]");
    function saveWorkflow() {
        pageLoader();
        selfWorkflow.RecentTags = ko.observable("");
        selfWorkflow.PopularTags = ko.observable("");
        var jsonDateString;
        var type = 'post';
        var updateUrl = 'InsertWorkflow';
        if (selfWorkflow.WorkflowID() > 0)
            updateUrl = 'UpdateWorkflow';
        $.each(selfWorkflow.WorkflowActions(), function (index, value) {
            if (value.WorkflowActionTypeID() == 3) {
                if (value.Action().RunAtDateTime() != null) {
                    jsonDateString = value.Action().RunAtDateTime().toString();
                    if (jsonDateString.indexOf("/Date") > -1) {

                        value.Action().RunAtDateTime(kendo.toString(new Date(parseInt(jsonDateString.replace('/Date(', ''))), 'g'));
                    }
                    else
                        value.Action().RunAtDateTime(kendo.toString(new Date(value.Action().RunAtDateTime()), 'g'));
                }
                if (value.Action().RunAtTimeDateTime() != null) {
                    jsonDateString = value.Action().RunAtTimeDateTime().toString();
                    if (jsonDateString.indexOf("/Date") > -1) {

                        value.Action().RunAtTimeDateTime(kendo.toString(new Date(parseInt(jsonDateString.replace('/Date(', ''))), 'g'));
                    }
                    else
                        value.Action().RunAtTimeDateTime(kendo.toString(new Date(value.Action().RunAtTimeDateTime()), 'g'));
                }
            }
            else if (value.WorkflowActionTypeID() == 8) {       //Update field 
                if (value.Action() != null) {
                    if (value.Action().FieldInputTypeId() == 12 || value.Action().FieldInputTypeId() == 1) {
                        if (value.Action().FieldValue() != null && Array.isArray(value.Action().FieldValue())) {
                            var searchText = value.Action().FieldValue().join('|');
                            value.Action().FieldValue(searchText);
                        }
                    }
                }
            }
            else if (value.WorkflowActionTypeID() == 1 || value.WorkflowActionTypeID() == 14) {
                if (value.Action() != null && value.Action().CampaignLinks() != null && value.Action() != undefined && value.Action().CampaignLinks() != undefined) {
                    $.each(value.Action().CampaignLinks(), function (index, link) {
                        $.grep(link.Actions(), function (v, i) {
                            var linkAction = null;
                            if (typeof v.Action == "function")
                                linkAction = v.Action();
                            else
                                linkAction = link.Action;
                            if (linkAction != null && linkAction != undefined) {
                                if (linkAction.WorkflowActionTypeID == 8) {    //Update field
                                    if (linkAction.FieldInputTypeId() == 12 || linkAction.FieldInputTypeId() == 1) {
                                        if (linkAction.FieldValue() != null && Array.isArray(linkAction.FieldValue())) {
                                            var searchText = linkAction.FieldValue().join('|');
                                            linkAction.FieldValue(searchText);
                                        }
                                    }
                                }
                                if (linkAction.WorkflowActionTypeID == 12) {
                                    if (linkAction.FromEmailID() == "")
                                        linkAction.FromEmailID(0);
                                }
                                if (linkAction.WorkflowActionTypeID == 1) {
                                    if (linkAction.CampaignID() == "")
                                        linkAction.CampaignID(0);
                                }
                            }
                        })

                    });
                }
            }
        });

        $.each(selfWorkflow.Triggers(), function (index, value) {
            if (value.TriggerTypeID() == 11) {  // Web page visited trigger
                var duration = (value.Minutes() * 60) + value.Seconds();
                value.Duration(duration);
            }
        });
        var jsondata = ko.toJSON(selfWorkflow);
        $.ajax({
            url: updateUrl,
            type: type,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'workflowViewModel': jsondata })
        }).then(function (response) {
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response);
            } else {
                filter.reject(response.error);
            }
            return filter.promise();
        }).done(function (data) {
            // Process success result      

            if (data.success) {
                notifySuccess('[|Successfully saved the workflow|]');
                setTimeout(
                       function () {
                           removepageloader();
                           window.location.href = "/workflows";
                       }, setTimeOutTimer);
            }
        }).fail(function (error) {
            // Display error message to user            
            removepageloader();
            selfWorkflow.IntiateLabel(" [|Initiate|]")
            notifyError(error);
        });
    }

    selfWorkflow.InitiateWorkflow = function () {
        var stopsaving = checkValidations();
        if (stopsaving == true) {
            validationScroll();
            return;
        } else {
            selfWorkflow.IntiateLabel(" [|Initiating..|]")
            selfWorkflow.StatusID(WorkflowStatus.Active);
            saveWorkflow();
        }
    }
    selfWorkflow.IsWorkflowActive = ko.pureComputed(function () {
        if (selfWorkflow.StatusID() == WorkflowStatus.Active) {
            return true;
        } else {
            return false;
        }
    });
    selfWorkflow.LifeCycleDropdownValues = ko.observableArray([]);
    selfWorkflow.Campaigns = ko.observableArray([]);
    selfWorkflow.AutomationCampaigns = ko.observableArray([]);
    selfWorkflow.FromUserEmails = ko.observableArray([]);
    selfWorkflow.FromUserEmails.subscribe(function () {
        selfWorkflow.FromUserEmails().map(function (e) { delete e.EmailSignature });
    })
    selfWorkflow.Forms = ko.observableArray([]);
    selfWorkflow.LeadAdapters = ko.observableArray([]);
    selfWorkflow.SmartSearches = ko.observableArray([]);
    selfWorkflow.OpportunityStageDropdownValues = ko.observableArray([]);
    selfWorkflow.Tags = ko.observableArray([]);
    selfWorkflow.ActionTypes = ko.observableArray([]);
    selfWorkflow.TourTypes = ko.observableArray([]);
    selfWorkflow.CampaignLinks = ko.observableArray([]);
    selfWorkflow.Users = ko.observableArray([]);
    selfWorkflow.Fields = ko.observableArray([]);
    selfWorkflow.PhoneNumbers = ko.observableArray([]);
    selfWorkflow.NotificationFields = ko.observableArray([]);
    selfWorkflow.CurrentDate = ko.observable((new Date()).toUtzDate());
    selfWorkflow.AllActiveWorkflowIDs = ko.computed(function () {
        var results = [];
        ko.utils.arrayForEach(selfWorkflow.ActiveWorkflows(), function (value) {
            results.push(value.WorkflowID);
        });
        return results;
    });

    selfWorkflow.DispalyCharactersRemaining = function (NotifyBy, Message) {
    }

    selfWorkflow.cancelSettings = function () {

        selfWorkflow.DeactivatedOn(DeactivatedOn);
        console.log(selfWorkflow.AllowParallelWorkflows());
        if (selfWorkflow.AllowParallelWorkflows() != null)
            selfWorkflow.AllowParallelWorkflows(data.AllowParallelWorkflows == null ? "" : data.AllowParallelWorkflows.toString());
        //selfWorkflow.IsWorkflowAllowedMoreThanOnce(IsWorkflowAllowedMoreThanOnce.toString());
        selfWorkflow.StatusID(StatusID);
        selfWorkflow.RemoveFromWorkflows(RemoveFromWorkflows);
        selfWorkflow.IsTimeSensitive(TimeSensitive);
    };

    selfWorkflow.saveSettings = function () {
        if (selfWorkflow.WorkflowID() > 0) {
            selfWorkflow.saveWorkflow();
        } else {
            alertifyReset("OK", "Cancel");
            alertify.confirm("[|Are you sure you want to save the entire workflow?|]", function (e) {
                if (e) {
                    selfWorkflow.saveWorkflow();
                }
                else {
                    notifyError("[|You have clicked cancel button|]");
                }
            });
        }
    }

    selfWorkflow.AllowParallelWorkflows = ko.observable(data.AllowParallelWorkflows == null ? "" : data.AllowParallelWorkflows.toString());
    selfWorkflow.IsWorkflowAllowedMoreThanOnce = ko.observable(data.IsWorkflowAllowedMoreThanOnce == null ? "" : data.IsWorkflowAllowedMoreThanOnce.toString());
    selfWorkflow.RemoveFromWorkflowValidation = selfWorkflow.RemoveFromWorkflows.extend({
        required: {
            message: "[|Please select Workflows|]",
            onlyIf: function () {
                return selfWorkflow.AllowParallelWorkflows() == "3";
            }
        }
    });

    selfWorkflow.IsRemvoefromWorkflowsEnabled = ko.pureComputed({
        read: function () {
            if (selfWorkflow.AllowParallelWorkflows() == "1") {
                //selfWorkflow.RemoveFromWorkflows('');
                return false;
            }
            else if (selfWorkflow.AllowParallelWorkflows() == "2") {
                //selfWorkflow.RemoveFromWorkflows(selfWorkflow.AllActiveWorkflowIDs());
                return false;
            }
            else if (selfWorkflow.AllowParallelWorkflows() == "3") {
                //selfWorkflow.RemoveFromWorkflows('');
                return true;
            } else {
                return false;
            }
        }
    });

    selfWorkflow.WorkflowStartTriggers = ko.utils.arrayFilter(selfWorkflow.WorkflowTriggers(), function (item) {
        return item.FilterTypeID == 1 || item.FilterTypeID == 3;
    });

    selfWorkflow.WorkflowStopTriggers = ko.utils.arrayFilter(selfWorkflow.WorkflowTriggers(), function (item) {
        return item.FilterTypeID == 2 || item.FilterTypeID == 3;
    });

    selfWorkflow.errors = ko.validation.group(selfWorkflow);

    selfWorkflow.saveWorkflow = function () {
        var stopsaving = checkValidations();
        if (stopsaving == true) {
            validationScroll();
            return;
        } else {
            saveWorkflow();
        }
    }

    selfWorkflow.saveWorkflowAs = function () {
        window.location.href = "/copyworkflow?WorkflowID=" + selfWorkflow.WorkflowID();
    }

    selfWorkflow.deleteWorkflow = function () {

        alertifyReset("[|Delete Workflow|]", "[|Cancel|]");
        var message = "[|Are you sure you want to delete this Workflow ?|]";

        alertify.confirm(message, function (e) {
            if (e) {
                var cid = [selfWorkflow.WorkflowID()];
                var authToken = readCookie("accessToken");
                var jsondata = JSON.stringify({ 'WorkflowIDs': cid });
                pageLoader();
                $.ajax({
                    url: webserviceurl + '/Workflows',
                    type: 'delete',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                    },
                    data: jsondata,
                    success: function (data) {

                        notifySuccess('[|Successfully deleted the workflow|]');
                        setTimeout(
                               function () {
                                   removepageloader();
                                   window.location.href = "/workflows";
                               }, setTimeOutTimer);
                    }, error: function (response) {
                        removepageloader();
                        notifyError(response.responseText);
                    }
                });

            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }
        });
    }

    selfWorkflow.addAction = function (data) {
        if (!data) {
            data = {
            };
        }
        var newAction = {
            WorkflowID: ko.observable(selfWorkflow.WorkflowID()),
            WorkflowActionID: ko.observable(data.WorkflowActionID),
            WorkflowActionTypeID: ko.observable(data.WorkflowActionTypeID),
            IsVisible: ko.observable(true),
            IsCollapsed: ko.observable(false),
            IsDeleted: ko.observable(false),
            iconclass: ko.observable(''),
            OrderNumber: ko.observable(selfWorkflow.WorkflowActions().length + 1),
            WorkflowActionTypeName: ko.observable(data.WorkflowActionTypeName),
            ActionTemplateName: ko.observable(data.ActionTemplateName),
            Action: ko.observable(data.Action),

            WorkflowActionSelect: function (e) {

                var dataItem;
                if (!(e == undefined)) {
                    dataItem = this.dataItem(e.item);
                } else {
                    $.grep(viewModel.WorkflowActionTypes(), function (v, i) {
                        if (v.WorkflowActionTypeID == actionTypes.LinkActions) {
                            dataItem = v;
                        }
                    });
                }
                var setNewAction = function (dataItem, action) {
                    action.Order = selfWorkflow.WorkflowActions().length;
                    newAction.WorkflowActionTypeName(dataItem.WorkflowActionTypeName);
                    newAction.WorkflowActionTypeID(dataItem.WorkflowActionTypeID);
                    newAction.ActionTemplateName(dataItem.ActionTempalteName);
                    newAction.iconclass(dataItem.iconclass);
                    newAction.Action(action);
                    newAction.IsVisible(false);
                }
                if (dataItem.WorkflowActionTypeID == actionTypes.AddTag || dataItem.WorkflowActionTypeID == actionTypes.RemoveTag) {
                    if (selfWorkflow.Tags().length > 0) {
                        var model = new WorkflowTagActionViewModel();
                        model.ActionType = (dataItem.WorkflowActionTypeID == actionTypes.AddTag) ? 1 : 0;
                        model.WorkflowActionTypeID = dataItem.WorkflowActionTypeID;
                        setNewAction(dataItem, model);
                    } else {
                        getAllTags().done(function (tagsdata) {
                            var model = new WorkflowTagActionViewModel();
                            selfWorkflow.Tags(tagsdata);
                            model.ActionType = (dataItem.WorkflowActionTypeID == actionTypes.AddTag) ? 1 : 0;
                            model.WorkflowActionTypeID = dataItem.WorkflowActionTypeID;
                            setNewAction(dataItem, model);
                        }).fail(function (err) {
                            notifyError(err.responseText);
                        });
                    }
                }
                else if (dataItem.WorkflowActionTypeID == actionTypes.EnterWorkflow) {
                    if (selfWorkflow.ActiveWorkflows().length > 0) {
                        var model = new EnterWorkflowActionViewModel();
                        setNewAction(dataItem, model);
                    } else {
                        getWorkflows(selfWorkflow.WorkflowID).done(function (workflowData) {
                            selfWorkflow.ActiveWorkflows(workflowData);
                            var model = new EnterWorkflowActionViewModel();
                            setNewAction(dataItem, model);
                        }).fail(function (err) {
                            notifyError(err.responseText);
                        });
                    }
                }
                else if (dataItem.WorkflowActionTypeID == actionTypes.ChangeLifeCycle) {
                    if (selfWorkflow.LifeCycleDropdownValues().length > 0) {
                        var action = new WorkflowLifeCycleActionViewModel();
                        setNewAction(dataItem, action);
                    } else {
                        getDropdownValues(dropDownTypes.LifeCycle).done(function (dropdowndata) {
                            var action = new WorkflowLifeCycleActionViewModel();
                            selfWorkflow.LifeCycleDropdownValues(dropdowndata.response);
                            setNewAction(dataItem, action);
                        });
                    }
                }
                else if (dataItem.WorkflowActionTypeID == actionTypes.AdjustLeadScore) {
                    setNewAction(dataItem, new WorkflowLeadScoreActionViewModel());
                }
                else if (dataItem.WorkflowActionTypeID == actionTypes.AssigntoUser) {
                    if (selfWorkflow.Users().length > 0) {
                        var model = new WorkflowUserAssignmentActionViewModel();
                        setNewAction(dataItem, model);
                    } else {
                        getUsers().done(function (usersdata) {
                            var model = new WorkflowUserAssignmentActionViewModel();
                            selfWorkflow.Users(usersdata);
                            setNewAction(dataItem, model);
                        }).fail(function (err) {
                            notifyError(err.responseText);
                        });
                    }
                }
                else if (dataItem.WorkflowActionTypeID == actionTypes.NotifyUser) {
                    if (selfWorkflow.Users().length > 0 && selfWorkflow.NotificationFields().length > 0) {
                        setNewAction(dataItem, new WorkflowNotifyUserActionViewModel());
                    } else {
                        getUsers().done(function (usersdata) {
                            selfWorkflow.Users(usersdata);
                            getNotificationFields().done(function (notificationFieldsData) {
                                selfWorkflow.NotificationFields(notificationFieldsData)
                                setNewAction(dataItem, new WorkflowNotifyUserActionViewModel());
                            }).fail(function (err) {
                                notifyError(err.responseText);
                            });

                        }).fail(function (err) {
                            notifyError(err.responseText);
                        });
                    }
                }
                else if (dataItem.WorkflowActionTypeID == actionTypes.UpdateField) {
                    if (selfWorkflow.Fields().length > 0) {
                        var model = new WorkflowContactFieldActionViewModel();
                        setNewAction(dataItem, model);
                    } else {
                        getFields().done(function (fieldsdata) {
                            ko.utils.arrayForEach(fieldsdata, function (item) {
                                item.ComputedFieldId = item.FieldId + "" + item.IsDropdownField;
                            });
                            var model = new WorkflowContactFieldActionViewModel();
                            selfWorkflow.Fields(fieldsdata);
                            setNewAction(dataItem, model);
                        }).fail(function (err) {
                            notifyError(err.responseText);
                        });
                    }
                }
                else if (dataItem.WorkflowActionTypeID == actionTypes.SendText) {
                    getPhoneNumbers().done(function (phonesdata) {
                        var model = new WorkflowTextNotificationActionViewModel();

                        selfWorkflow.PhoneNumbers(phonesdata);
                        //model.PhoneNumbers(phonesdata);
                        setNewAction(dataItem, model);
                    }).fail(function (err) {
                        notifyError(err.responseText);
                    });
                }
                else if (dataItem.WorkflowActionTypeID == actionTypes.SetTimer) {
                    var model = new WorkflowSetTimerActionViewModel();
                    setNewAction(dataItem, model);
                }
                else if (dataItem.WorkflowActionTypeID == actionTypes.SendCampaign) {
                    if (selfWorkflow.AutomationCampaigns().length > 0) {
                        var model = new WorkflowCampaignActionViewModel();
                        setNewAction(dataItem, model);
                    } else {
                        getAllCampaigns(true).done(function (campaignsdata) {
                            selfWorkflow.AutomationCampaigns(campaignsdata);
                            var model = new WorkflowCampaignActionViewModel();
                            setNewAction(dataItem, model);
                        }).fail(function (err) {

                            notifyError(err.responseText);
                        });
                    }

                }
                else if (dataItem.WorkflowActionTypeID == actionTypes.SendEmail) {
                    if (selfWorkflow.FromUserEmails().length > 0) {
                        var model = new WorkflowEmailNotificationActionViewModel();
                        setNewAction(dataItem, model);
                    } else {
                        getFromUserEmails().then(function (response) {
                            var filter = $.Deferred();
                            if (response.success) {
                                filter.resolve(response);
                            } else {
                                filter.reject(response.error);
                            }
                            return filter.promise();
                        }).done(function (useremails) {
                            selfWorkflow.FromUserEmails(useremails.response);
                            $.each(selfWorkflow.FromUserEmails(), function (index, value) {
                                value.DisplayEmail = SenderName + " <" + value.EmailId + ">";
                            });
                            var model = new WorkflowEmailNotificationActionViewModel();
                            setNewAction(dataItem, model);
                        }).fail(function (err) {
                            notifyError(err.responseText);
                        });
                    }
                } else if (dataItem.WorkflowActionTypeID == actionTypes.LinkActions) {
                    var innerAction = ko.mapping.toJS(newAction.Action())
                    var model = new WorkflowCampaignActionViewModel();
                    model.CampaignID(innerAction.CampaignID);
                    model.CampaignName(innerAction.CampaignName)
                    model.CampaignChange(innerAction);
                    setNewAction(dataItem, model)
                }
            }

        };
        newAction.ActionTypeValidation = newAction.WorkflowActionTypeID.extend({ required: { message: '[|Action is required|]' } });
        newAction.errors = ko.validation.group([newAction.ActionTypeValidation]);
        if (data.WorkflowActionTypeID == actionTypes.LinkActions) {
            newAction.WorkflowActionSelect();
            newAction.OrderNumber(1);
            selfWorkflow.WorkflowActions.splice(0, 0, newAction);
        } else {
            selfWorkflow.WorkflowActions.push(newAction);
        }
    }
    selfWorkflow.WorkflowActions = ko.observableArray(data.WorkflowActions);
    selfWorkflow.removeAction = function (action) {
        action.IsDeleted(true);
        action.IsVisible(false);
        var index = selfWorkflow.WorkflowActions().indexOf(action);
        if (action.Action() == undefined)
            selfWorkflow.WorkflowActions.remove(action);
        else
            selfWorkflow.WorkflowActions()[index] = action;
    };

    selfWorkflow.copyAction = function (action) {
        if (action.Action() == undefined) {
            selfWorkflow.addAction();
            return;
        }
        var template = $.grep(selfWorkflow.WorkflowActionTypes(), function (e) { return e.WorkflowActionTypeID == action.WorkflowActionTypeID(); })[0];
        var copyAction = ko.toJS(action);
        copyAction.WorkflowActionID = 0;
        copyAction.OrderNumber = (action.OrderNumber + 1);
        var newAction = new WorkflowActionViewModel(copyAction);
        newAction.WorkflowActionTypeName(template.WorkflowActionTypeName);
        newAction.ActionTemplateName(template.ActionTempalteName);
        newAction.iconclass(template.iconclass);
        var setNewAction = function (model) {
            newAction.Action(model);
            selfWorkflow.WorkflowActions.push(newAction);
        }
        var convertedAction = ko.toJS(action.Action);
        convertedAction.WorkflowActionID = 0;
        if (template.WorkflowActionTypeID == actionTypes.AddTag || template.WorkflowActionTypeID == actionTypes.RemoveTag) {
            convertedAction.WorkflowTagActionID = 0;
            var model = new WorkflowTagActionViewModel(convertedAction);
            model.Order = Math.floor(Math.random() * 999) + 1;
            if (selfWorkflow.Tags().length > 0) {
                setNewAction(model);
            } else {
                getAllTags().done(function (tagsdata) {
                    selfWorkflow.Tags(tagsdata);
                    setNewAction(model);
                }).fail(function (err) {
                    notifyError(err.responseText);
                });
            }
        }
        else if (template.WorkflowActionTypeID == actionTypes.ChangeLifeCycle) {
            convertedAction.WorkflowLifeCycleActionID = 0;
            var model = new WorkflowLifeCycleActionViewModel(convertedAction);
            if (selfWorkflow.LifeCycleDropdownValues().length > 0) {
                setNewAction(model);
            } else {
                getDropdownValues(dropDownTypes.LifeCycle).done(function (dropdowndata) {
                    selfWorkflow.LifeCycleDropdownValues(dropdowndata.response);
                    setNewAction(model);
                });
            }
        }
        else if (template.WorkflowActionTypeID == actionTypes.EnterWorkflow) {
            convertedAction.TriggerWorkflowActionID = 0;
            var model = new EnterWorkflowActionViewModel(convertedAction);
            if (selfWorkflow.ActiveWorkflows().length > 0) {
                setNewAction(model);
            } else {
                getWorkflows(selfWorkflow.WorkflowID).done(function (workflowdata) {
                    selfWorkflow.ActiveWorkflows(workflowdata.response);
                    setNewAction(model);
                });
            }
        }
        else if (template.WorkflowActionTypeID == actionTypes.AdjustLeadScore) {
            convertedAction.WorkflowLeadScoreActionID = 0;
            setNewAction(new WorkflowLeadScoreActionViewModel(convertedAction));
        }
        else if (template.WorkflowActionTypeID == actionTypes.AssigntoUser) {
            convertedAction.WorkflowUserAssignmentActionID = 0;
            var model = new WorkflowUserAssignmentActionViewModel(convertedAction);
            model.Order = Math.floor(Math.random() * 999) + 1;
            if (selfWorkflow.Users().length > 0) {
                setNewAction(model);
            } else {
                getUsers().done(function (usersdata) {
                    selfWorkflow.Users(usersdata);
                    setNewAction(model);
                }).fail(function (err) {
                    notifyError(err.responseText);
                });
            }
        }
        else if (template.WorkflowActionTypeID == actionTypes.NotifyUser) {
            convertedAction.WorkflowNotifyUserActionID = 0;
            var model = new WorkflowNotifyUserActionViewModel(convertedAction);
            model.Order = Math.floor(Math.random() * 999) + 1;
            if (selfWorkflow.Users().length > 0 && selfWorkflow.NotificationFields().length > 0) {
                setNewAction(model);
            } else {
                getUsers().done(function (responseText, statusText, xhr, $form) {
                    selfWorkflow.Users(responseText);
                    getNotificationFields().done(function (responseText, statusText, xhr, $form) {
                        selfWorkflow.NotificationFields(responseText);
                        setNewAction(model);
                    }).fail(function (err) {
                        notifyError(err.responseText);
                    });
                }).fail(function (err) {
                    notifyError(err.responseText);
                });
            }
        }
        else if (template.WorkflowActionTypeID == actionTypes.UpdateField) {
            convertedAction.WorkflowContactFieldActionID = 0;
            var model = new WorkflowContactFieldActionViewModel(convertedAction);
            if (selfWorkflow.Fields().length > 0) {
                setNewAction(model);
            } else {
                getFields().done(function (fieldsdata) {
                    ko.utils.arrayForEach(fieldsdata, function (item) {
                        item.ComputedFieldId = item.FieldId + "" + item.IsDropdownField;
                    });
                    selfWorkflow.Fields(fieldsdata);
                    setNewAction(model);
                }).fail(function (err) {
                    notifyError(err.responseText);
                });
            }
        }
        else if (template.WorkflowActionTypeID == actionTypes.SendText) {
            getPhoneNumbers().done(function (phonesdata) {
                var model = new WorkflowTextNotificationActionViewModel(ko.toJS(action.Action));
                model.PhoneNumbers(phonesdata);
                setNewAction(model);
            }).fail(function (err) {
                notifyError(err.responseText);
            });
        }
        else if (template.WorkflowActionTypeID == actionTypes.SetTimer) {
            convertedAction.WorkflowTimerActionID = 0;
            var model = new WorkflowSetTimerActionViewModel(convertedAction);
            setNewAction(model);
        }
        else if (template.WorkflowActionTypeID == actionTypes.SendCampaign) {
            convertedAction.WorkflowCampaignActionID = 0;

            var model = new WorkflowCampaignActionViewModel(convertedAction);

            setNewAction(model);
        }
        else if (template.WorkflowActionTypeID == actionTypes.SendEmail) {
            convertedAction.WorkFlowEmailNotificationActionID = 0;
            var model = new WorkflowEmailNotificationActionViewModel(convertedAction);
            setNewAction(model);
        }
    };

    selfWorkflow.displayAction = function (action) {
        if (action.IsCollapsed() == true)
            action.IsCollapsed(false);
        else
            action.IsCollapsed(true);
    };
}


