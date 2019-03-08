
var WorkflowActionViewModel = function (data) {
    var selfWorkflow = this;

    if (!data) {
        data = {};
    }

    selfWorkflow.WorkflowID = ko.observable(data.WorkflowID);
    selfWorkflow.WorkflowActionID = ko.observable(data.WorkflowActionID);
    selfWorkflow.OrderNumber = ko.observable(data.OrderNumber);
    selfWorkflow.WorkflowActionTypeID = ko.observable(data.WorkflowActionTypeID);
    selfWorkflow.IsDeleted = ko.observable(data.IsDeleted);
    selfWorkflow.IsSubAction = ko.observable(data.IsSubAction);
    selfWorkflow.Action = ko.observable(data.Action);
    selfWorkflow.WorkflowActionTypeName = ko.observable('ActionTemplate');
    selfWorkflow.ActionTemplateName = ko.observable('');
    selfWorkflow.iconclass = ko.observable('');
    selfWorkflow.IsVisible = ko.observable(data.IsVisible);
    selfWorkflow.IsCollapsed = ko.observable(data.IsCollapsed);
    selfWorkflow.ActionTypeValidation = selfWorkflow.WorkflowActionTypeID.extend({ required: { message: '[|Action is required|]' } });
    selfWorkflow.errors = ko.validation.group([selfWorkflow.ActionTypeValidation]);
    selfWorkflow.WorkflowActionSelect = function () {
    };
    selfWorkflow.fieldSelect = function () {
    };
};

var WorkflowCampaignActionViewModel = function (data) {
    selfCampaign = this;

    if (!data) {
        data = {};
    }
    var selectedlinks = [];
    selfCampaign.WorkflowActionID = ko.observable(data.WorkflowActionID);
    selfCampaign.WorkflowActionTypeID = ko.observable(1);
    selfCampaign.WorkflowCampaignActionID = ko.observable(data.WorkflowCampaignActionID);
    selfCampaign.CampaignID = ko.observable(data.CampaignID);
    selfCampaign.campaignValidation = selfCampaign.CampaignID.extend({ required: { message: "[|Campaign is required|]" } });
    selfCampaign.CampaignLinks = ko.observableArray([]);
    selfCampaign.CampaignName = ko.observable(data.CampaignName);
    selfCampaign.TotalLinks = ko.observable('');
    function isEmpty(obj) {
        return Object.keys(obj).length === 0;
    }

    function emptyActions(linkModels) {
        var emptyActions = [];
        $.each(data.urls, function (i, url) {
            var exist = ko.utils.arrayFirst(linkModels, function (model) {
                return model.CampaignLinkId() == url.CampaignLinkId;
            });
            if (exist == null) {
                emptyActions.push(url);
            }
        });
        return emptyActions;
    }

    //selfCampaign.SortLinks = function () {
    //    return selfCampaign.CampaignLinks.sort(function (left, right) {
    //        return left.Order() == right.Order() ?
    //             0 :
    //             (left.Order() < right.Order() ? -1 : 1);
    //    });
    //}

    //selfCampaign.SortedCampaignLinks = ko.pureComputed({
    //    read: function () {
    //        return selfCampaign.CampaignLinks.sort(function (left, right) {
    //            return left.Order() == right.Order() ?
    //                 0 :
    //                 (left.Order() < right.Order() ? -1 : 1);
    //        });
    //    }
    //}).extend({ rateLimit : 500 });

    selfCampaign.linkModel = [];
    if (!isEmpty(data) && (data.CampaignLinks != null || data.CampaignLinks != undefined)) {
        if (data.urls != null && data.urls.length > 0) {
            var campaignLinksLength = data.urls.length;
            selfCampaign.TotalLinks(campaignLinksLength == 0 ? '[|No Campaign Links |]' : (campaignLinksLength == 1 ? "[|1 Campaign Link|]" : campaignLinksLength + " " + "[|Campaign Links|]"));
            ko.utils.arrayForEach(data.urls, function (url, index) {
                var campaignLink = ko.utils.arrayFirst(data.CampaignLinks, function (link) {
                    return link.CampaignLinkId == url.CampaignLinkId;
                });

                if (campaignLink != null) {
                    var linkModel = new CampaignLinkAction(campaignLink, data.urls);
                    selfCampaign.linkModel.push(linkModel);
                }
                else {
                    url.Order = index + 1;
                    var emptyLinkModel = new CampaignLinkAction(url, data.urls);
                    selfCampaign.linkModel.push(emptyLinkModel);
                }
            });
            linkModels = ko.mapping.fromJS(selfCampaign.linkModel);
            selfCampaign.CampaignLinks = ko.observableArray(linkModels());
        }
    }
    selfCampaign.CampaignURLs = [];
    selfCampaign.CampaignSelect = function (e) {
        setTimeout(function () { selfCampaign.CampaignChangeInActions(e); }, 1000);
    };

    selfCampaign.CampaignChangeInActions = function (dataItem) {
        var currentCampaignAction = dataItem.Action();
        currentCampaignAction.CampaignLinks([]);
        getCampaignUrls(currentCampaignAction.CampaignID()).done(function (campaignlinks) {
            currentCampaignAction.CampaignName(dataItem.CampaignName)
            currentCampaignAction.linkModel = [];
            currentCampaignAction.CampaignURLs = campaignlinks;
            if (campaignlinks.length > 0) {
                var campaignLinksLength = campaignlinks.length;
                currentCampaignAction.TotalLinks(campaignLinksLength == 0 ? '[|No Campaign Links |]' : (campaignLinksLength == 1 ? "[|1 Campaign Link|]" : campaignLinksLength + " " + "[|Campaign Links|]"));
            }
            ko.utils.arrayForEach(campaignlinks, function (link, index) {
                link.Order = index + 1;
                var model = new CampaignLinkAction(link, campaignlinks);
                currentCampaignAction.linkModel.push(model);
            });
            var linkModels = ko.mapping.fromJS(currentCampaignAction.linkModel);
            currentCampaignAction.CampaignLinks(linkModels());
        }).fail(function (err) {
            removepageloader();
            notifyError(err.responseText);
        });
    }

    selfCampaign.CampaignChange = function (dataItem) {
        getCampaignUrls(dataItem.CampaignID).done(function (campaignlinks) {
            selfCampaign.CampaignName(dataItem.CampaignName)
            selfCampaign.linkModel = [];
            selfCampaign.CampaignURLs = campaignlinks;
            if (campaignlinks.length > 0) {
                var campaignLinksLength = campaignlinks.length;
                selfCampaign.TotalLinks(campaignLinksLength == 0 ? '[|No Campaign Links |]' : (campaignLinksLength == 1 ? "[|1 Campaign Link|]" : campaignLinksLength + " " + "[|Campaign Links|]"));
            }
            ko.utils.arrayForEach(campaignlinks, function (link, index) {
                link.Order = index + 1;
                var model = new CampaignLinkAction(link, campaignlinks);
                selfCampaign.linkModel.push(model);
            });
            var linkModels = ko.mapping.fromJS(selfCampaign.linkModel);
            selfCampaign.CampaignLinks(linkModels());
        }).fail(function (err) {
            removepageloader();
            notifyError(err.responseText);
        });
    }

    selfCampaign.addLinkAction = function (link) {
        if (!link) {
            link = {};
        }
        var link_nomap = ko.mapping.toJS(link);
        var new_link = new CampaignLinkAction(link_nomap, selfCampaign.CampaignURLs);
        selfCampaign.CampaignLinks.push(new_link);
    }
    //selfCampaign.TotalLinks = ko.pureComputed({
    //    read: function () {
    //        var campaignLinksLength = selfCampaign.CampaignLinks().length;
    //        return campaignLinksLength == 0 ? '[|No Campaign Links |]' : (campaignLinksLength == 1 ? "[|1 Campaign Link|]" : campaignLinksLength + " " + "[|Campaign Links|]");
    //    },
    //    write: function(value) {
    //        //var campaignLinksLength = value;
    //        //console.log(campaignLinksLength);
    //        //return campaignLinksLength == 0 ? '[|No Campaign Links |]' : (campaignLinksLength == 1 ? "[|1 Campaign Link|]" : campaignLinksLength + " " + "[|Campaign Links|]");
    // }
    //});

    //if (data.CampaignID != 0 && data.CampaignID != undefined) {
    //    var campaignLinks = [];
    //    getCampaignUrls(data.CampaignID).done(function (campaignlinks) {
    //        campaignLinks = campaignlinks;
    //        if (selfCampaign.CampaignLinks().length > 0) {
    //            $.each(selfCampaign.CampaignLinks(), function (index, campaignLink) {
    //                campaignLink = ko.observable(campaignLink)
    //                campaignLink.URL = ko.observable()

    //                campaignLink.SubActionTemplateName = ko.observable();
    //                campaignLink.Action = ko.observable();
    //                campaignLink.CampaignLinkId = ko.observable(campaignLink().CampaignLinkId)
    //                //campaignLink.Action != null
    //                if (1==1) {
    //                    var link = $.grep(campaignLinks, function (e) { return e.CampaignLinkId == campaignLink.CampaignLinkId() })[0];

    //                    campaignLink.URL = ko.mapping.fromJS(link.URL);;

    //                    var template = $.grep(viewModel.LinkWorkflowActionTypes(), function (e) { return e.WorkflowActionTypeID == campaignLink().Action.WorkflowActionTypeID; })[0];

    //                    if (campaignLink().Action.WorkflowActionTypeID == actionTypes.AddTag || campaignLink().Action.WorkflowActionTypeID == actionTypes.RemoveTag) {
    //                        getAllTags().done(function (tagsdata) {
    //                            var model = new WorkflowTagActionViewModel(campaignLink.Action);
    //                            viewModel.Tags(tagsdata);
    //                            model.ActionType((campaignLink.Action.WorkflowActionTypeID == actionTypes.AddTag) ? 1 : 0);
    //                            model.Order = campaignLink.Order;
    //                            campaignLink.Action(model);
    //                            model.WorkflowActionTypeID = campaignLink.Action.WorkflowActionTypeID;
    //                            campaignLink.SubActionTemplateName(template.ActionTempalteName);
    //                        }).fail(function (err) {
    //                            notifyError(err.responseText);
    //                        });
    //                    }
    //                    else if (campaignLink.Action.WorkflowActionTypeID == actionTypes.AdjustLeadScore) {
    //                        var model = new WorkflowLeadScoreActionViewModel(campaignLink.Action);
    //                        campaignLink.Action(model);
    //                        campaignLink.SubActionTemplateName(template.ActionTempalteName);
    //                    }
    //                    else if (campaignLink.Action.WorkflowActionTypeID == actionTypes.UpdateField) {
    //                        if (viewModel.Fields().length > 0) {
    //                            var model = new WorkflowContactFieldActionViewModel(campaignLink.Action);
    //                            viewModel.Fields(viewModel.Fields());
    //                            campaignLink.Action(model);
    //                            campaignLink.SubActionTemplateName(template.ActionTempalteName);
    //                        }
    //                        else
    //                            getFields().done(function (fieldsdata) {
    //                                var model = new WorkflowContactFieldActionViewModel(campaignLink.Action);
    //                                viewModel.Fields(fieldsdata);
    //                                campaignLink.Action(model);
    //                                campaignLink.SubActionTemplateName(template.ActionTempalteName);
    //                            }).fail(function (err) {
    //                                notifyError(err.responseText);
    //                            });
    //                    }
    //                    else if (campaignLink.Action.WorkflowActionTypeID == actionTypes.NotifyUser) {
    //                        if (viewModel.Users().length > 0) {
    //                            var model = new WorkflowNotifyUserActionViewModel(campaignLink.Action);
    //                            campaignLink.Action(model);
    //                            campaignLink.SubActionTemplateName(template.ActionTempalteName);
    //                        } else {
    //                            getUsers().done(function (usersdata) {
    //                                var model = new WorkflowNotifyUserActionViewModel(campaignLink.Action);
    //                                viewModel.Users(usersdata);
    //                                campaignLink.Action(model);
    //                                campaignLink.SubActionTemplateName(template.ActionTempalteName);
    //                            }).fail(function (err) {
    //                                notifyError(err.responseText);
    //                            });
    //                        }
    //                    }
    //                }
    //            });
    //        }
    //        selfCampaign.CampaignLinks.valueHasMutated()
    //    });
    //}

    selfCampaign.FromUserMailID = ko.observable(data.FromUserMailID);

    selfCampaign.ActionTemplateName = ko.observable(data.ActionTemplateName);

    selfCampaign.IsLinkActionEnabled = ko.pureComputed({
        read: function () {
            if (selfCampaign.CampaignLinks().length > 0) {
                return true;
            } else
                return false;
        },
        write: function () { }
    });

    selfCampaign.ActionSelect = function (campaignLink, selectedIndex) {
        var index = selectedIndex();
        setTimeout(function () {
            if (index != null && index >= 0) {
                var campaignLink = selfCampaign.CampaignLinks()[index];
                var actionType = $.grep(viewModel.LinkWorkflowActionTypes(), function (e) { return e.WorkflowActionTypeID.toString() == campaignLink.WorkflowActionTypeID(); })[0];
                if (campaignLink != null) {
                    campaignLink.SubActionTemplateName('');
                    campaignLink.Action('');
                    if (campaignLink.WorkflowActionTypeID() == actionTypes.AddTag || campaignLink.WorkflowActionTypeID() == actionTypes.RemoveTag) {
                        getAllTags().done(function (tagsdata) {
                            var model = new WorkflowTagActionViewModel();
                            viewModel.Tags(tagsdata);
                            model.ActionType((campaignLink.WorkflowActionTypeID() == actionTypes.AddTag) ? 1 : 0);
                            model.Order = selfWorkflow.WorkflowActions().length;
                            model.WorkflowActionTypeID = campaignLink.WorkflowActionTypeID();
                            campaignLink.Action(model);
                            campaignLink.SubActionTemplateName(actionType.ActionTempalteName);
                        }).fail(function (err) {
                            notifyError(err.responseText);
                        });
                    }
                    else if (campaignLink.WorkflowActionTypeID() == actionTypes.AdjustLeadScore) {
                        var model = new WorkflowLeadScoreActionViewModel();
                        campaignLink.Action(model);
                        campaignLink.SubActionTemplateName(actionType.ActionTempalteName);
                    }
                    else if (campaignLink.WorkflowActionTypeID() == actionTypes.NotifyUser) {
                        if (viewModel.Users().length > 0 && viewModel.NotificationFields().length > 0) {
                            var model = new WorkflowNotifyUserActionViewModel();
                            model.Order = Math.floor(Math.random() * 999) + 1;
                            campaignLink.Action(model);
                            campaignLink.SubActionTemplateName(actionType.ActionTempalteName);
                        } else {
                            getUsers().done(function (usersdata) {
                                getNotificationFields().done(function (notificationFieldsData) {
                                    viewModel.NotificationFields(notificationFieldsData);
                                    var model = new WorkflowNotifyUserActionViewModel();
                                    model.Order = Math.floor(Math.random() * 999) + 1;
                                    viewModel.Users(usersdata);
                                    campaignLink.Action(model);
                                    campaignLink.SubActionTemplateName(actionType.ActionTempalteName);
                                }).fail(function (err) {
                                    notifyError(err.responseText);
                                });

                            }).fail(function (err) {
                                notifyError(err.responseText);
                            });



                        }
                    }
                    else if (campaignLink.WorkflowActionTypeID() == actionTypes.EnterWorkflow) {
                        var model = new EnterWorkflowActionViewModel();
                        campaignLink.Action(model);
                        campaignLink.SubActionTemplateName(actionType.ActionTempalteName);
                    }
                    else if (campaignLink.WorkflowActionTypeID() == actionTypes.ChangeLifeCycle) {
                        if (viewModel.LifeCycleDropdownValues().length > 0) {
                            var model = new WorkflowLifeCycleActionViewModel();
                            campaignLink.Action(model);
                            campaignLink.SubActionTemplateName(actionType.ActionTempalteName);
                        } else {
                            getDropdownValues(dropDownTypes.LifeCycle).done(function (dropdowndata) {
                                viewModel.LifeCycleDropdownValues(dropdowndata.response);
                                var model = new WorkflowLifeCycleActionViewModel();
                                campaignLink.Action(model);
                                campaignLink.SubActionTemplateName(actionType.ActionTempalteName);
                            });
                        }
                    }
                    else if (campaignLink.WorkflowActionTypeID() == actionTypes.UpdateField) {
                        if (viewModel.Fields().length > 0) {
                            var model = new WorkflowContactFieldActionViewModel();
                            campaignLink.Action(model);
                            campaignLink.SubActionTemplateName(actionType.ActionTempalteName);
                        }
                        else {
                            getFields().done(function (fieldsdata) {
                                ko.utils.arrayForEach(fieldsdata, function (item) {
                                    item.ComputedFieldId = item.FieldId + "" + item.IsDropdownField;
                                });
                                viewModel.Fields(fieldsdata);
                                var model = new WorkflowContactFieldActionViewModel();
                                campaignLink.Action(model);
                                campaignLink.SubActionTemplateName(actionType.ActionTempalteName);
                            }).fail(function (err) {
                                notifyError(err.responseText);
                            });
                        }
                    }
                    else if (campaignLink.WorkflowActionTypeID() == actionTypes.AssigntoUser) {
                        if (viewModel.Users().length > 0) {
                            var model = new WorkflowUserAssignmentActionViewModel();
                            model.Order = Math.floor(Math.random() * 999) + 1;
                            campaignLink.Action(model);
                            campaignLink.SubActionTemplateName(actionType.ActionTempalteName);
                        } else {
                            getUsers().done(function (usersdata) {
                                var model = new WorkflowUserAssignmentActionViewModel();
                                model.Order = Math.floor(Math.random() * 999) + 1;
                                viewModel.Users(usersdata);
                                campaignLink.Action(model);
                                campaignLink.SubActionTemplateName(actionType.ActionTempalteName);
                            }).fail(function (err) {
                                notifyError(err.responseText);
                            });
                        }
                    }
                    else if (campaignLink.WorkflowActionTypeID() == actionTypes.SendEmail) {
                        if (viewModel.FromUserEmails().length > 0) {
                            var model = new WorkflowEmailNotificationActionViewModel();
                            campaignLink.Action(model);
                            campaignLink.SubActionTemplateName(actionType.ActionTempalteName);
                        } else {
                            var SenderName = readCookie("SenderName");
                            getFromUserEmails().done(function (useremails) {
                                viewModel.FromUserEmails(useremails.response);
                                $.each(viewModel.FromUserEmails(), function (index, value) {
                                    value.DisplayEmail = SenderName + " <" + value.EmailId + ">";
                                });
                                var model = new WorkflowEmailNotificationActionViewModel();
                                campaignLink.Action(model);
                                campaignLink.SubActionTemplateName(actionType.ActionTempalteName);
                            }).fail(function (err) {
                                notifyError(err.responseText);
                            });
                        }
                    }
                    else if (campaignLink.WorkflowActionTypeID() == actionTypes.SendCampaign) {
                        if (viewModel.AutomationCampaigns().length > 0) {
                            var model = new WorkflowCampaignSubActionViewModel();
                            model.Order = index + 1;
                            campaignLink.Action(model);
                            campaignLink.SubActionTemplateName(actionType.ActionTempalteName);
                        } else {
                            getAllCampaigns(true).done(function (campaignsdata) {
                                var model = new WorkflowCampaignSubActionViewModel();
                                model.Order = index + 1;
                                viewModel.AutomationCampaigns(campaignsdata);
                                campaignLink.Action(model);
                                campaignLink.SubActionTemplateName(actionType.ActionTempalteName);
                            }).fail(function (err) {
                                notifyError(err.responseText);
                            });
                        }
                    }
                    else {
                        campaignLink.Action(undefined);
                    }
                }
            }
            else {
                campaignLink.Action(undefined);
            }

        }, 500);
    };

    selfCampaign.errors = ko.validation.group(selfCampaign);
};

var CampaignLinkAction = function (data, urls) {
    var selfLink = this;

    ko.mapping.fromJS(data, {}, selfLink);
    selfLink.WorkflowCampaignLinkID = ko.observable(data.WorkflowCampaignLinkID);
    selfLink.Actions = ko.observableArray(data.Actions);
    selfLink.CampaignLinkId = ko.observable(data.CampaignLinkId);
    selfLink.LinkActionID = ko.observable(data.CampaignLinkId);
    selfLink.Order = ko.observable(data.Order);
    selfLink.ParentWorkflowActionID = ko.observable(data.ParentWorkflowActionID);
    selfLink.SubActionTemplateName = ko.observable(data.SubActionTemplateName || '');
    selfLink.WorkflowActionTypeID = ko.observable(data.WorkflowActionTypeID);
    var validLink = $.grep(urls, function (e) { return e.CampaignLinkId == data.CampaignLinkId; })[0];
    selfLink.URL = ko.observable();
    if (validLink != null) {
        selfLink.URL.URL = ko.observable(validLink.URL.URL);
    }

    AddLinkAction = function (sl, link_action) {
        var linkAction = {
            WorkflowActionID: ko.observable(link_action.WorkflowActionID),
            WorkflowActionTypeID: ko.observable(0),
            IsDeleted: ko.observable(false),
            LinkActionOrder: ko.observable(0),
            WorkflowActionTypeName: ko.observable('ActionTemplate'),
            ActionTemplateName: ko.observable('').extend({ rateLimit: 100 }),
            Action: ko.observable(),
            LinkWorkflowActionSelect: function (e, option) {
                if (option == 'change') {
                    link_action.Action = {};
                }
                var dataItem = e;
                var setNewAction = function (item, action) {
                    if (link_action.WorkflowActionTypeID != item.WorkflowActionTypeID) {
                        linkAction.WorkflowActionID(0)
                    }

                    linkAction.WorkflowActionTypeName(item.WorkflowActionTypeName);
                    linkAction.WorkflowActionTypeID(item.WorkflowActionTypeID);
                    linkAction.LinkActionOrder(1)
                    linkAction.ActionTemplateName(item.ActionTempalteName);
                    linkAction.Action(action);
                }
                if (dataItem.WorkflowActionTypeID == actionTypes.SendCampaign) {
                    if (viewModel.AutomationCampaigns().length > 0) {
                        var model = new WorkflowCampaignSubActionViewModel(link_action.Action);
                        selfLink.WorkflowActionTypeID(actionTypes.SendCampaign);
                        setNewAction(dataItem, model);
                    } else {
                        getAllCampaigns(true).done(function (campaignsdata) {
                            var model = new WorkflowCampaignSubActionViewModel(link_action.Action);
                            viewModel.AutomationCampaigns(campaignsdata);
                            selfLink.WorkflowActionTypeID(actionTypes.SendCampaign);
                            setNewAction(dataItem, model);
                        }).fail(function (err) {
                            notifyError(err.responseText);
                        });
                    }
                }
                else if (dataItem.WorkflowActionTypeID == actionTypes.AddTag || dataItem.WorkflowActionTypeID == actionTypes.RemoveTag) {
                    var model = new WorkflowTagActionViewModel(link_action.Action);
                    if (viewModel.Tags().length == 0) {
                        getAllTags().done(function (tags) {
                            viewModel.Tags(tags)
                            model.ActionType = (dataItem.WorkflowActionTypeID == actionTypes.AddTag) ? 1 : 0;
                            model.WorkflowActionTypeID = dataItem.WorkflowActionTypeID;
                            model.Order = Math.floor(Math.random() * 999) + 1;
                            setNewAction(dataItem, model);
                        }).fail(function (err) {
                            notifyError(err.responseText);
                        });
                    } else {
                        model.ActionType = (dataItem.WorkflowActionTypeID == actionTypes.AddTag) ? 1 : 0;
                        model.WorkflowActionTypeID = dataItem.WorkflowActionTypeID;
                        model.Order = Math.floor(Math.random() * 999) + 1
                        setNewAction(dataItem, model);
                    }

                }
                else if (dataItem.WorkflowActionTypeID == actionTypes.AdjustLeadScore) {
                    var model = new WorkflowLeadScoreActionViewModel(link_action.Action);
                    model.WorkflowActionTypeID = 6
                    model.Order = linkAction.LinkActionOrder();
                    setNewAction(dataItem, model);
                }
                else if (dataItem.WorkflowActionTypeID == actionTypes.ChangeLifeCycle) {
                    if (viewModel.LifeCycleDropdownValues().length > 0) {
                        var model = new WorkflowLifeCycleActionViewModel(link_action.Action);
                        selfLink.WorkflowActionTypeID(actionTypes.ChangeLifeCycle);
                        setNewAction(dataItem, model);
                    } else {
                        getDropdownValues(dropDownTypes.LifeCycle).done(function (dropdowndata) {
                            viewModel.LifeCycleDropdownValues(dropdowndata.response);
                            var model = new WorkflowLifeCycleActionViewModel(link_action.Action);
                            selfLink.WorkflowActionTypeID(actionTypes.ChangeLifeCycle);
                            setNewAction(dataItem, model);
                        });
                    }
                }
                else if (dataItem.WorkflowActionTypeID == actionTypes.UpdateField) {
                    if (option == 'change') {
                        if (viewModel.Fields().length > 0) {
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
                    else {
                        var setValueOptions = function () {
                            var field = $.grep(viewModel.Fields(), function (e) { return e.FieldId == link_action.Action.FieldID })[0];
                            var contactDropdown = null;
                            link_action.Action.FieldInputTypeId = field.FieldInputTypeId;
                            var valueOptions = [];
                            if (link_action.Action.FieldID == DO_NOT_EMAIL_FIELDID) {
                                valueOptions = doNotEmailValueOptions;
                            }

                            if (field.FieldId == PARTNER_TYPE_FIELD)
                                contactDropdown = dropDownTypes.PartnerType;
                            else if (field.FieldId == LIFECYCLE_STAGE_FIELD)
                                contactDropdown = dropDownTypes.LifeCycle;
                            else if (field.FieldId == LEAD_SOURCE_FIELD)
                                contactDropdown = dropDownTypes.LeadSource;

                            if (field.FieldInputTypeId == fieldInputTypes.Checkbox || field.FieldInputTypeId == fieldInputTypes.DropDown
                                                            || field.FieldInputTypeId == fieldInputTypes.Radio || field.FieldInputTypeId == fieldInputTypes.MultiSelect) {
                                var authToken = readCookie("accessToken");
                                $.ajax({
                                    url: AdvancedSearch_BASE_URL + 'GetSearchQualifiers',
                                    type: 'get',
                                    dataType: 'json',
                                    data: { 'fieldId': field.FieldId, 'contactDropdown': contactDropdown },
                                    contentType: "application/json; charset=utf-8",
                                    beforeSend: function (xhr) {
                                        xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                                    }
                                }).then(function (response) {
                                    var filter = $.Deferred();
                                    if (response.success)
                                        filter.resolve(response);
                                    else
                                        filter.reject(response.error);
                                    return filter.promise();
                                }).done(function (dataResponse) {
                                    valueOptions = dataResponse.response;
                                    link_action.Action.ValueOptions = valueOptions;
                                    var contactFieldmodel = new WorkflowContactFieldActionViewModel(link_action.Action);
                                    if (field.FieldInputTypeId == fieldInputTypes.MultiSelect)
                                        contactFieldmodel.FieldValue(contactFieldmodel.FieldValue());
                                    selfLink.WorkflowActionTypeID(actionTypes.UpdateField);
                                    setNewAction(dataItem, contactFieldmodel);
                                }).fail(function (err) {
                                    notifyError(err);
                                });
                            }
                            else {
                                var contactFieldmodel = new WorkflowContactFieldActionViewModel(link_action.Action);
                                selfLink.WorkflowActionTypeID(actionTypes.UpdateField);
                                setNewAction(dataItem, contactFieldmodel);
                            }
                        }
                        if (viewModel.Fields().length > 0) {
                            setValueOptions();
                        }
                        else {
                            getFields().done(function (fieldsdata) {
                                var contactFieldmodel = new WorkflowContactFieldActionViewModel(link_action.Action);
                                ko.utils.arrayForEach(fieldsdata, function (item) {
                                    item.ComputedFieldId = item.FieldId + "" + item.IsDropdownField;
                                });
                                viewModel.Fields(fieldsdata);
                                setValueOptions();
                                setNewAction(dataItem, contactFieldmodel);
                            }).fail(function (err) {
                                notifyError(err.responseText);
                            });
                        }
                    }

                }
                else if (dataItem.WorkflowActionTypeID == actionTypes.NotifyUser) {
                    if (viewModel.Users().length > 0 && viewModel.NotificationFields().length > 0) {
                        var model = new WorkflowNotifyUserActionViewModel(link_action.Action);
                        model.Order = Math.floor(Math.random() * 999) + 1;
                        selfLink.WorkflowActionTypeID(actionTypes.NotifyUser);
                        setNewAction(dataItem, model);
                    } else {
                        getUsers().done(function (users) {
                            viewModel.Users(users);
                            getNotificationFields().done(function (notificationFieldsData) {
                                viewModel.NotificationFields(notificationFieldsData);
                                var model = new WorkflowNotifyUserActionViewModel(link_action.Action);
                                var ids = [];
                                $.each(users, function (ind, val) {
                                    ids.push(val.UserID);
                                });
                                if (link_action.Action.UserIds) {
                                    $.each(link_action.Action.UserIds, function (index, value) {
                                        if (jQuery.inArray(value, ids) == -1) {
                                            var ind = link_action.Action.UserIds.indexOf(value)
                                            link_action.Action.UserIds.splice(ind, 1);
                                        }
                                    });
                                }
                                model.Order = Math.floor(Math.random() * 999) + 1;
                                selfLink.WorkflowActionTypeID(actionTypes.NotifyUser);
                                setNewAction(dataItem, model);
                            });

                        });

                    }
                }
                else if (dataItem.WorkflowActionTypeID == actionTypes.AssigntoUser) {
                    if (viewModel.Users().length > 0) {
                        var model = new WorkflowUserAssignmentActionViewModel(link_action.Action);
                        model.Order = Math.floor(Math.random() * 999) + 1;
                        selfLink.WorkflowActionTypeID(actionTypes.AssigntoUser);
                        setNewAction(dataItem, model);
                    } else {
                        getUsers().done(function (users) {
                            viewModel.Users(users);
                            var model = new WorkflowUserAssignmentActionViewModel(link_action.Action);
                            model.Order = Math.floor(Math.random() * 999) + 1;
                            selfLink.WorkflowActionTypeID(actionTypes.AssigntoUser);
                            setNewAction(dataItem, model);
                        });
                    }
                }
                else if (dataItem.WorkflowActionTypeID == actionTypes.SendEmail) {
                    if (viewModel.FromUserEmails().length > 0) {
                        var model = new WorkflowEmailNotificationActionViewModel(link_action.Action);
                        selfLink.WorkflowActionTypeID(actionTypes.SendEmail);
                        setNewAction(dataItem, model);
                    } else {
                        var SenderName = readCookie("SenderName");
                        getFromUserEmails().done(function (useremails) {
                            viewModel.FromUserEmails(useremails.response);
                            $.each(viewModel.FromUserEmails(), function (index, value) {
                                value.DisplayEmail = SenderName + " <" + value.EmailId + ">";
                            });
                            var model = new WorkflowEmailNotificationActionViewModel(link_action.Action);
                            selfLink.WorkflowActionTypeID(actionTypes.SendEmail);
                            setNewAction(dataItem, model);
                        }).fail(function (err) {
                            notifyError(err.responseText);
                        });
                    }
                }
                else if (dataItem.WorkflowActionTypeID == actionTypes.EnterWorkflow) {
                    var model = new EnterWorkflowActionViewModel(link_action.Action);
                    selfLink.WorkflowActionTypeID(actionTypes.EnterWorkflow);
                    setNewAction(dataItem, model);
                }
            },
            RemoveLinkAction: function (parent, current) {
                current.IsDeleted(true)
                //find if any item with out deleted
                var hasItems = false;
                $.grep(parent.Actions(), function (v, i) {
                    if (!v.IsDeleted()) {
                        hasItems = true;
                    }
                });
                if (!hasItems) {
                    AddLinkAction(parent, {});
                }
            }
        }
        var actionType = {};
        $.grep(viewModel.LinkWorkflowActionTypes(), function (v, i) {
            if (v.WorkflowActionTypeID == link_action.WorkflowActionTypeID) {
                actionType = v;
            }
        })
        linkAction.LinkWorkflowActionSelect(actionType);
        sl.Actions.push(linkAction);
    }

    if (selfLink.Actions().length == 0) {
        AddLinkAction(selfLink, {});
    }
    else {
        var actions = selfLink.Actions();
        selfLink.Actions([]);
        $.grep(actions, function (action, index) {
            AddLinkAction(selfLink, action);
        });
    }

    selfLink.errors = ko.validation.group(selfLink);
};

var WorkflowCampaignSubActionViewModel = function (data) {
    var selfCampSubAction = this;

    if (!data) {
        data = {};
    }

    selfCampSubAction.WorkflowActionID = ko.observable(data.WorkflowActionID);
    selfCampSubAction.WorkflowCampaignActionID = ko.observable(data.WorkflowCampaignActionID);
    selfCampSubAction.CampaignID = ko.observable(data.CampaignID);
    selfCampSubAction.campaignValidation = selfCampSubAction.CampaignID.extend({ required: { message: "[|Campaign is required|]" } });
    selfCampSubAction.ActionTemplateName = ko.observable(data.ActionTemplateName);
    selfCampSubAction.CampaignLinks = ko.observableArray([]);
    selfCampSubAction.TotalLinks = ko.observable();
    selfCampSubAction.CampaignSelect = function (e) {
        // var dataItem = this.dataItem(e.item);
        var subAction = e.Action();
        if (subAction.CampaignID != '') {
        }
    };

    selfCampSubAction.WorkflowActionTypeID = actionTypes.SendCampaign;
    selfCampSubAction.errors = ko.validation.group(selfCampSubAction);
};

var WorkflowCampaignActionLinkViewModel = function (data) {
    var self = this;

    if (!data) {
        data = {};
    }

    self.WorkflowActionID = ko.observable(data.WorkflowActionID);
    self.WorkflowCampaignLinkID = ko.observable(data.WorkflowCampaignLinkID);
    self.LinkID = ko.observable(data.LinkID);
    self.ParentWorkflowActionID = ko.observable(data.ParentWorkflowActionID);
    self.LinkActionID = ko.observable(data.LinkActionID);
    self.WorkflowActionTypeID = ko.observable(data.WorkflowActionTypeID);
    self.errors = ko.validation.group(self);
};

var WorkflowLeadScoreActionViewModel = function (data) {
    var selfLeadScore = this;

    if (!data) {
        data = {};
    }
    selfLeadScore.WorkflowActionID = ko.observable(data.WorkflowActionID);
    selfLeadScore.WorkflowLeadScoreActionID = ko.observable(data.WorkflowLeadScoreActionID);
    selfLeadScore.LeadScoreValue = ko.observable(data.LeadScoreValue);
    selfLeadScore.LeadScoreValueValidation = selfLeadScore.LeadScoreValue.extend({
        required: { message: "[|Lead Score value is required|]" }
    });
    selfLeadScore.WorkflowActionTypeID = actionTypes.AdjustLeadScore;
    selfLeadScore.errors = ko.validation.group(selfLeadScore);
};

var WorkflowEmailNotificationActionViewModel = function (data) {
    var selfEmailNotification = this;
    if (!data) {
        data = {};
    }

    ko.validation.rules['maxLength'] = {
        validator: function (emailsubject) {
            if (emailsubject.length > 100) {
                return false;
            } else {
                return true;
            }
        },
        message: ''
    };
    ko.validation.registerExtenders();

    selfEmailNotification.WorkflowActionID = ko.observable(data.WorkflowActionID);
    selfEmailNotification.WorkFlowEmailNotificationActionID = ko.observable(data.WorkFlowEmailNotificationActionID);
    selfEmailNotification.WorkflowActionTypeID = actionTypes.SendEmail;
    selfEmailNotification.FromEmailID = ko.observable(data.FromEmailID == undefined ? '' : data.FromEmailID);
    selfEmailNotification.fromemailValidation = selfEmailNotification.FromEmailID.extend({
        required: {
            message: '[|From User Email is required|]'
        }
    });
    selfEmailNotification.Subject = ko.observable(data.Subject == undefined ? '' : data.Subject).extend({
        required: {
            message: "[|Subject is required|]"
        }
    });
    selfEmailNotification.Body = ko.observable(data.Body).extend({
        required: {
            message: "[|Body is required|]"
        }
    });

    selfEmailNotification.charactersremaining = ko.pureComputed({
        read: function () {
            return (100 - selfEmailNotification.Subject().length) + ((100 - selfEmailNotification.Subject().length) > 0 ? ' [|characters remaining|]' : ' [|Too many characters|]');
        }
    });
    selfEmailNotification.classname = ko.pureComputed({
        read: function () {
            if (selfEmailNotification.Subject().length > 100)
                return 'red num-characters';
            else
                return 'green num-characters';
        }
    });

    selfEmailNotification.MaxLengthValidation = selfEmailNotification.Subject.extend({
        maxLength: 100,
        message: "Subject can not be more than 100 characters"
    });

    selfEmailNotification.errors = ko.validation.group(selfEmailNotification);
};

var WorkflowLifeCycleActionViewModel = function (data) {
    var selfLifeCycle = this;

    if (!data) {
        data = {};
    }

    selfLifeCycle.WorkflowActionID = ko.observable(data.WorkflowActionID);
    selfLifeCycle.WorkflowLifeCycleActionID = ko.observable(data.WorkflowLifeCycleActionID);
    selfLifeCycle.LifeCycleDropdownValueID = ko.observable(data.LifeCycleDropdownValueID);
    selfLifeCycle.LifeCycleValidation = selfLifeCycle.LifeCycleDropdownValueID.extend({
        required:
            {
                message: "[|Life Cycle value is required|]"
            }
    });
    selfLifeCycle.WorkflowActionTypeID = actionTypes.ChangeLifeCycle;
    selfLifeCycle.errors = ko.validation.group(selfLifeCycle);
};

var TriggerCategoryTypeViewModel = function (data) {
    var self = this;

    if (!data) {
        data = {};
    }
    self.WorkflowActionID = ko.observable(data.WorkflowActionID);
    self.TriggerTypeID = ko.observable(data.TriggerTypeID);
    self.TriggerName = ko.observable(data.TriggerName);
    self.TriggerCategory = ko.observable(data.TriggerCategory);
    self.WorkflowActionType = ko.observable(data.WorkflowActionTypeID);

};

var WorkflowTagActionViewModel = function (data) {
    var selfTag = this;
    if (!data) {
        data = {};
    }

    selfTag.WorkflowActionID = ko.observable(data.WorkflowActionID);
    selfTag.WorkflowTagActionID = ko.observable(data.WorkflowTagActionID);
    selfTag.TagID = ko.observable(data.TagID);

    selfTag.tagValidation = selfTag.TagID.extend({ required: { message: "[|Tag is required|]" } });
    selfTag.ActionType = ko.observable(data.ActionType);
    selfTag.ActionTemplateName = ko.observable(data.ActionTemplateName);
    selfTag.WorkflowActionTypeID = selfTag.ActionType() == 1 ? actionTypes.AddTag : actionTypes.RemoveTag;
    selfTag.errors = ko.validation.group(selfTag);
    selfTag.Tags = ko.observable(data.Tags);
};

var WorkflowNotifyUserActionViewModel = function (data) {
    var self = this;

    if (!data) {
        data = {};
    }

    ko.validation.rules['maxLengthDataValidation'] = {
        validator: function (messagebody) {
            if ((self.NotifyType().toString() == '3' || self.NotifyType().toString() == '2') && messagebody.length > 160) {
                return false;
            } else if (self.NotifyType().toString() == '1' && messagebody.length > 512) {
                return false;
            } else {
                return true;
            }
        },
        message: ''
    };
    ko.validation.registerExtenders();

    var sfields = [1, 2, 7, 3, 24, 25, 22, 26];

    self.WorkflowActionID = ko.observable(data.WorkflowActionID);
    self.WorkflowNotifyUserActionID = ko.observable(data.WorkflowNotifyUserActionID || 0);
    self.WorkflowActionTypeID = actionTypes.NotifyUser;
    self.UserID = ko.observable(data.UserID);
    self.UserIds = ko.observableArray(data.UserIds);
    self.userValidation = self.UserIds.extend({ required: { message: "[|User is required|]" } });
    self.NotifyType = ko.observable(data.NotifyType == undefined ? '3' : data.NotifyType.toString());
    self.NotifyTypeValidation = self.NotifyType.extend({ required: { message: "[|Notify type is required|]" } });
    self.NotificationFieldIds = ko.observableArray(data.NotificationFieldIds == undefined ? sfields : data.NotificationFieldIds);
    self.fieldValidation = self.NotificationFieldIds.extend({ required: { message: "[|Select atleast one field|]" } });
    self.MessageBody = ko.observable(data.MessageBody == undefined ? '' : data.MessageBody).extend({
        required: {
            message: "[|Message is required|]"
        }
    });
    self.charactersremaining = ko.pureComputed({
        read: function () {
            if (self.NotifyType() == '3' || self.NotifyType() == '2') {
                return (160 - self.MessageBody().length) + ((160 - self.MessageBody().length) > 0 ? ' [|characters remaining|]' : ' [|Too many characters|]');
            } else {
                return (512 - self.MessageBody().length) + ((512 - self.MessageBody().length) > 0 ? ' [|characters remaining|]' : ' [|Too many characters|]');
            }
        }
    });
    self.classname = ko.pureComputed({
        read: function () {

            if (self.NotifyType().toString() == '3' || self.NotifyType().toString() == '2') {
                if (self.MessageBody().length > 160)
                    return 'red num-characters';
                else
                    return 'green num-characters';
            } else {
                if (self.MessageBody().length > 512)
                    return 'red num-characters';
                else
                    return 'green num-characters';
            }
        }
    });
    self.MaxLengthValidation = self.MessageBody.extend({
        maxLengthDataValidation: self.NotifyType()
    });

    self.errors = ko.validation.group(self);

};

var WorkflowUserAssignmentActionViewModel = function (data) {
    var self = this;
    if (!data) {
        data = {};
    }
    self.WorkflowActionID = ko.observable(data.WorkflowActionID);
    self.WorkflowUserAssignmentActionID = ko.observable(data.WorkflowUserAssignmentActionID);
    self.WorkflowActionTypeID = actionTypes.AssigntoUser;
    self.Weekdays = ko.observableArray([{ Id: 1, Day: "Monday" }, { Id: 2, Day: "Tuesday" }, { Id: 3, Day: "Wednesday" }, { Id: 4, Day: "Thursday" }, { Id: 5, Day: "Friday" },
        { Id: 6, Day: "Saturday" }, { Id: 7, Day: "Sunday" }, { Id: 8, Day: "Week Day" }, { Id: 9, Day: "Weekend" }, { Id : 10, Day: "Daily"}]);

    var assignment_action = [];
    if (data.RoundRobinContactAssignments) {
        $.each(data.RoundRobinContactAssignments, function (index, action) {
            var assigment = new RoundRobinContactAssignment(action);
            assignment_action.push(assigment);
        });
    }
    if (assignment_action.length == 0)
        assignment_action.push(new RoundRobinContactAssignment());
    self.RoundRobinContactAssignments = ko.observableArray(assignment_action);

    self.ScheduledID = ko.observable(data.ScheduledID ? data.ScheduledID.toString() : "1");
    
    self.scheduleValidation = self.ScheduledID.extend({});
    self.ScheduledID.subscribe(function (id) {
        id = parseInt(id);
        if (id == 1) {
            self.RoundRobinContactAssignments([]);
            $.each(self.Weekdays(), function (i, d) {
                if (d.Id == 10) {
                    var data = {};
                    data.DayOfWeek = d.Id;
                    var viewModel = new RoundRobinContactAssignment(data);
                    self.RoundRobinContactAssignments.push(viewModel);
                }
            });
        }
        else if (id == 2) {
            self.RoundRobinContactAssignments([]);
            $.each(self.Weekdays(), function (i, d) {
                if (d.Id > 7 && d.Id < 10) {
                    var data = {};
                    data.DayOfWeek = d.Id;
                    var viewModel = new RoundRobinContactAssignment(data);
                    self.RoundRobinContactAssignments.push(viewModel);
                }
            });
        }
        else if (id == 3) {
            self.RoundRobinContactAssignments([]);
            $.each(self.Weekdays(), function (i, d) {
                if (d.Id < 8) {
                    var data = {};
                    data.DayOfWeek = d.Id;
                    var viewModel = new RoundRobinContactAssignment(data);
                    self.RoundRobinContactAssignments.push(viewModel);
                }
            });
        }
    });
    
    self.errors = ko.validation.group(self);
};

var RoundRobinContactAssignment = function (data) {
    var self = this;

    if (!data) {
        data = {};
    }

    self.WorkFlowUserAssignmentActionID = ko.observable(data.WorkFlowUserAssignmentActionID);
    self.RoundRobinContactAssignmentID = ko.observable(data.RoundRobinContactAssignmentID);
    self.DayOfWeek = ko.observable(data.DayOfWeek ? data.DayOfWeek : "10");
    self.IsRoundRobinAssignment = ko.observable(data.IsRoundRobinAssignment ? data.IsRoundRobinAssignment : "0");
    self.CheckboxOrder = Math.floor(Math.random() * 999) + 1;
    self.UserID = ko.observable(data.UserID);
    //.extend({
    //    required: {
    //        message: "[|Please select user|]",
    //        onlyIf: function () {
    //            return self.IsRoundRobinAssignment() == "0";
    //        }
    //    }
    //});
    self.UserIds = ko.observableArray(data.UserIds);
    //.extend({
    //    required: {
    //        message: "[|Please select users|]",
    //        onlyIf: function () {
    //            return self.IsRoundRobinAssignment() == "1";
    //        }
    //    }
    //});
    //self.userIdValidation = self.UserID.extend({ UserRequired: "" });
    //self.userIdsValidation = self.UserIds.extend({ UserRequired: "" });
    self.errors = ko.validation.group(self);
};

var WorkflowContactFieldActionViewModel = function (data) {
    var self = this;

    if (!data) {
        data = {};
    }
    ko.validation.rules['url'] = {
        validator: function (val, required) {
            if (!val) {
                return !required;
            }
            val = val.replace(/^\s+|\s+$/, ''); //Strip whitespace
            //Regex by Diego Perini from: http://mathiasbynens.be/demo/url-regex
            return val.match(/^(?:(?:https?|ftp):\/\/)(?:\S+(?::\S*)?@)?(?:(?!10(?:\.\d{1,3}){3})(?!127(?:\.‌​\d{1,3}){3})(?!169\.254(?:\.\d{1,3}){2})(?!192\.168(?:\.\d{1,3}){2})(?!172\.(?:1[‌​6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1‌​,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00‌​a1-\uffff0-9]+-?)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]+-?)*[a-z\u‌​00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})))(?::\d{2,5})?(?:\/[^\s]*)?$/i);
        },
        message: '[|This field has to be a valid URL|]'
    };
    ko.validation.registerExtenders();
    self.WorkflowActionID = ko.observable(data.WorkflowActionID);
    self.WorkflowContactFieldActionID = ko.observable(data.WorkflowContactFieldActionID);
    self.FieldID = ko.observable(data.FieldID);
    self.DropdownValueId = ko.observable(data.DropdownValueId);
    self.IsDropdownField = ko.observable(data.IsDropdownField);
    self.ComputedFieldId = ko.observable(data.FieldID + "" + data.IsDropdownField);
    self.FieldInputTypeId = ko.observable(data.FieldInputTypeId);
    self.fieldValidation = self.FieldID.extend({ required: { message: '[|Field is required|]' } });
    self.FieldValue = ko.observable(data.FieldValue);
    self.ValueOptions = ko.observableArray(data.ValueOptions);
    var doNotEmailValueOptions = [{ Id: 1, Value: 'Yes', IsDeleted: false }, { Id: 0, Value: 'No', IsDeleted: false }];

    //If field input type is 1 or 12 (checkbox or multi-select)
    if (self.FieldInputTypeId() == 1 || self.FieldInputTypeId() == 12) {
        if (self.FieldValue().indexOf('|') > -1) {
            var text = self.FieldValue().split('|');
            self.FieldValue(text);
        }
    }
    else if (self.FieldInputTypeId() == 2 || self.FieldInputTypeId() == 13 || self.FieldInputTypeId() == 9) {
        //var dateFormat = readCookie("dateformat");
        var dateString = self.FieldValue();
        if (self.FieldInputTypeId() == 9)
            self.FieldValue(kendo.toString(new Date(dateString)), 't');
        else
            self.FieldValue(kendo.toString(new Date(dateString)));
    }

    if (self.FieldID() == 23)
        self.ValueOptions(doNotEmailValueOptions);

    self.ComputedFieldId.subscribe(function (computedFieldId) {
        var fieldId = computedFieldId.replace('true', '').replace('false', '');
        self.FieldID(fieldId);       // For newely added filter by default FieldId = 1; if changed to another field we need to update for refelection in the search
    });
    self.fieldValueValidation = self.FieldValue.extend({
        required:
            {
                onlyIf: function () {
                    return self.FieldID() != '' || self.FieldID() == '0';
                },
                message: '[|Field Value is required|]'
            },
        email: {
            onlyIf: function () {
                return self.FieldInputTypeId() == fieldInputTypes.Email;
            },
            message: "[|Please enter a valid email|]"
        },
        number: {
            onlyIf: function () {
                return self.FieldInputTypeId() == fieldInputTypes.Number;
            },
            message: "[|Please enter a valid number|]"
        },
        url: {
            onlyIf: function () {
                return self.FieldInputTypeId() == fieldInputTypes.Url;
            }
        }

    });
    self.WorkflowActionTypeID = actionTypes.UpdateField;

    self.errors = ko.validation.group(self);


    self.valueOptionsNeeded = ko.pureComputed({
        read: function () {
            var fields = ko.utils.arrayFilter(viewModel.Fields(), function (item) {
                if (item.FieldInputTypeId == '1' || item.FieldInputTypeId == '6' || item.FieldInputTypeId == '11' || item.FieldInputTypeId == '12')
                    return item;
            });
            return fields;
        },
        write: function () { }
    });

    var PARTNER_TYPE_FIELD = 21;
    var LIFECYCLE_STAGE_FIELD = 22;
    var LEAD_SOURCE_FIELD = 24;
    var LEAD_SCORE_FIELD = 26;
    var DO_NOT_EMAIL_FIELDID = 23;


    self.fieldSelect = function (e) {
        var dataItem = this.dataItem(e.item);
        var contactDropdown = null;

        var fieldId = dataItem.FieldId;
        if (fieldId == "") {
            self.FieldInputTypeId('');
            self.FieldID('');
            self.FieldValue('');
            return;
        }
        if (fieldId == DO_NOT_EMAIL_FIELDID) {
            self.FieldInputTypeId(11);
            self.ValueOptions(doNotEmailValueOptions);
            return;
        }
        var IsAjaxCallNeeded = ko.utils.arrayFilter(self.valueOptionsNeeded(), function (item) {
            if (item.FieldId == fieldId && dataItem.IsCustomField == item.IsCustomField && dataItem.IsDropdownField == item.IsDropdownField)
                return item;
            else
                return false;
        });
        if (fieldId != null) {
            self.FieldValue('');
            var field = ko.utils.arrayFirst(viewModel.Fields(), function (item) {
                if (item.FieldId == fieldId)
                    return item;
            });

            self.FieldInputTypeId(field.FieldInputTypeId);
            if (field.FieldId == PARTNER_TYPE_FIELD)
                contactDropdown = dropDownTypes.PartnerType;
            else if (field.FieldId == LIFECYCLE_STAGE_FIELD)
                contactDropdown = dropDownTypes.LifeCycle;
            else if (field.FieldId == LEAD_SOURCE_FIELD)
                contactDropdown = dropDownTypes.LeadSource;
            if (fieldId == DO_NOT_EMAIL_FIELDID) {
                dataItem.InputTypeId(11);
                dataItem.ValueOptions(doNotEmailValueOptions);
                IsAjaxCallNeeded = [];
            }

            if (IsAjaxCallNeeded.length > 0) {
                var authToken = readCookie("accessToken");
                $.ajax({
                    url: AdvancedSearch_BASE_URL + 'GetSearchQualifiers',
                    type: 'get',
                    dataType: 'json',
                    data: { 'fieldId': field.FieldId, 'contactDropdown': contactDropdown },
                    contentType: "application/json; charset=utf-8",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                    }
                }).then(function (response) {
                    var filter = $.Deferred();
                    if (response.success) {
                        filter.resolve(response);
                    } else {
                        filter.reject(response.error);
                    }
                    return filter.promise();
                }).done(function (data) {
                    if (data.success)
                        self.ValueOptions(data.response);
                }).fail(function (err) {
                    notifyError(err);
                });
            }
            if (field == null || field == 'undefined') {
                self.InputTypeId(8);
            }

            self.IsDropdownField(dataItem.IsDropdownField);
        }
    }
};

var WorkflowTextNotificationActionViewModel = function (data) {
    var self = this;

    if (!data) {
        data = {};
    }

    self.WorkflowActionID = ko.observable(data.WorkflowActionID);
    self.WorkflowTextNotificationActionID = ko.observable(data.WorkflowTextNotificationActionID);
    self.Message = ko.observable(data.Message == null ? '' : data.Message).extend({
        required: {
            message: '[|Message is required|]'
        },
        maxLength: {
            message: '[|Only 160 chacters are allowed|]',
            params: 160
        }
    });
    self.FromMobileID = ko.observable(data.FromMobileID);
    self.WorkflowActionTypeID = ko.observable(data.WorkflowActionTypeID);
    self.phoneNumberValidation = self.FromMobileID.extend({ required: { message: "[|Phone number is required|]" } });
    self.errors = ko.validation.group(self);
};

var WorkflowSetTimerActionViewModel = function (data) {
    var self = this;

    if (!data) {
        data = {};
    }

    self.WorkflowActionID = ko.observable(data.WorkflowActionID);
    self.WorkflowActionTypeID = actionTypes.SetTimer;
    self.TimerType = ko.observable(data.TimerType == undefined ? "1" : data.TimerType.toString());

    self.TimerTypeValidation = self.TimerType.extend({
        required:
            {
                message: '[|Set Timer Based on is required|]'
            }
    });
    self.RunType = ko.observable(data.RunType == undefined ? '' : data.RunType.toString());
    self.RunTypeValidation = self.RunType.extend({
        required:
            {
                onlyIf: function () {
                    return self.TimerType() == TimerTypes.date.toString();
                },
                message: '[|Timer is required|]'
            }
        //logChange:
    });

    self.WorkflowTimerActionID = ko.observable(data.WorkflowTimerActionID);
    self.DelayUnit = ko.observable(data.DelayUnit);
    self.DelayUnit = ko.observable(data.DelayUnit);
    self.DelayPeriod = ko.observable(data.DelayPeriod);
    self.DelayPeriod.extend({
        required: {
            onlyIf: function () {
                return self.TimerType() == TimerTypes.timeDelay;
            },
            message: '[|Wait at least is required|]'
        },
        number: {
            onlyIf: function () {
                return self.TimerType() == TimerTypes.timeDelay;
            },
            message: '[|Please enter a number|]'
        },
        validation: [{
            validator: function () {
                if (self.DelayUnit() == "6" && (parseInt(self.DelayPeriod()) < 15 || parseInt(self.DelayPeriod()) == NaN)) {
                    return false;
                }
                else {
                    return true;
                }
            },
            message: '[|Minimum wait period should be 15 minutes|]',
        }, {
            validator: function () {
                if ((self.DelayUnit() == "5" || self.DelayUnit() == "2" || self.DelayUnit() == "3" || self.DelayUnit() == "4") && (parseInt(self.DelayPeriod()) < 1 || parseInt(self.DelayPeriod()) == NaN)) {
                    return false;
                }
                else {
                    return true;
                }
            },
            message: '[|Minimum wait period should be 15 minutes|]',
        }
        ]
    });
    

    self.PeriodValidation = self.DelayUnit.extend({
        required: {
            onlyIf: function () {
                return self.TimerType() == TimerTypes.timeDelay.toString();
            },
            message: '[|Period is required|]'
        }
    });

    self.RunOn = ko.observable(data.RunOn == undefined ? '' : data.RunOn.toString());
    self.RunOnValidation = self.RunOn.extend({
        required: {
            onlyIf: function () {
                return self.TimerType() == TimerTypes.timeDelay.toString();
            },
            message: '[|Run on validation is required|]'
        }
    });
    self.RunAt = ko.observable(data.RunAt);
    self.RunAtDateTime = ko.observable(self.RunAt());
    self.RunAt.subscribe(function (value) {
        self.RunAtDateTime(value);
    }, this);
    //self.RunAtValidation = self.RunAt.extend({
    //    required: {
    //        onlyIf: function () {
    //            return self.TimerType() == TimerTypes.timeDelay;
    //        },
    //        message: "[|Run at is required|]"
    //    }
    //});

    
    self.CustomRunAt = ko.observable(self.DelayPeriod());
    
    self.RunAtValidation = self.CustomRunAt.extend({
        validation: [{
            validator: function () {
                if (self.DelayUnit() == "6" && parseInt(self.DelayPeriod()) > 1445) {
                    return false;
                }
                else {
                    return true;
                }
            },
            message: '[|Run at is required|]',
        }, {
            validator: function () {
                if (self.DelayUnit() == "5" && parseInt(self.DelayPeriod()) > 24 && (self.RunAt() == null || self.RunAt() == undefined)) {
                    return false;
                }
                else {
                    return true;
                }
            },
            message: '[|Run at is required|]',
        },
        {
            validator: function () {
                if ((self.DelayUnit() == "2" && (self.RunAt() == null || self.RunAt() == undefined)) || (self.DelayUnit() == "3" && (self.RunAt() == null || self.RunAt() == undefined)) || (self.DelayUnit() == "4" && (self.RunAt() == null || self.RunAt() == undefined))) {
                    return false;
                }
                else {
                    return true;
                }
            },
            message: '[|Run at is required|]',
        }]
    });

    self.RunOnDate = ko.observable(data.RunOnDate);
    self.RunOnDateValidation = self.RunOnDate.extend({
        required: {
            onlyIf: function () {
                return self.RunType() == RunTypes.OnADate.toString();
            },
            message: "[|On date is required|]"
        }
    });
    self.StartDate = ko.observable(data.StartDate);
    self.StartDateValidation = self.StartDate.extend({
        required: {
            onlyIf: function () {
                return (self.TimerType() == TimerTypes.date) && (self.RunType() == RunTypes.BetweenDates);
            },
            message: "[|Start date is required|]"
        }
    });
    self.EndDate = ko.observable(data.EndDate);
    self.EndDateValidation = self.EndDate.extend({
        required: {
            onlyIf: function () {
                return (self.TimerType() == TimerTypes.date) && (self.RunType() == RunTypes.BetweenDates);
            },
            message: "[|End date is required|]"
        }
    });
    self.RunAtTime = ko.observable(data.RunAtTime);
    self.RunAtTimeDateTime = ko.observable(self.RunAtTime());
    self.RunAtTime.subscribe(function (value) {
        self.RunAtTimeDateTime(value);
    }, this);

    self.RunAtTimeValidation = self.RunAtTime.extend({
        required: {
            onlyIf: function () {
                return (self.TimerType() == TimerTypes.date) && (self.RunType() == RunTypes.OnADate);
            },
            message: "[|Run at is required|]"
        }
    });

    var days = data.DaysOfWeek;
    var daysofweek = [];
    if (days != undefined && days.length > 0) {
        for (var g = 0; g < days.length; g++)
            daysofweek.push(days[g].toString());
    }

    self.DaysOfWeek = ko.observableArray(daysofweek);
    self.DayofWeekValidation = self.DaysOfWeek.extend({
        required:
            {
                onlyIf: function () {
                    return self.TimerType() == TimerTypes.week.toString();
                },
                message: '[|Day of week is required|]'
            }
    });

    self.errors = ko.validation.group(self);
}

var EnterWorkflowActionViewModel = function (data) {
    var selfWorkflowAction = this;

    if (!data) {
        data = {};
    }

    selfWorkflowAction.WorkflowActionID = ko.observable(data.WorkflowActionID);
    selfWorkflowAction.TriggerWorkflowActionID = ko.observable(data.TriggerWorkflowActionID);
    selfWorkflowAction.SiblingWorkflowID = ko.observable(data.SiblingWorkflowID);
    selfWorkflowAction.SiblingWorkflowIDValidation = selfWorkflowAction.SiblingWorkflowID.extend({
        required: {
            message: "[|Workflow is required|]"
        }
    });
    selfWorkflowAction.ActionType = ko.observable(data.ActionType);
    selfWorkflowAction.ActionTemplateName = ko.observable(data.ActionTemplateName);
    selfWorkflowAction.WorkflowActionTypeID = actionTypes.EnterWorkflow;
    selfWorkflowAction.errors = ko.validation.group(selfWorkflowAction);
};
