var Tagify = function (webserviceUrl, viewModel, accountId, tagCreatedBy) {
    var self = this;    
    var typewatch = (function () {
        var timer = 0;
        return function (callback, ms) {
            clearTimeout(timer);
            timer = setTimeout(callback, ms);
        };
    })();
    tagsViewModel = [];
    self.Tagify = function (inputId) {      
        var txtTagId = "#" + inputId + "_tag";
        var tagSuggestions;

        $('#' + inputId).tagsInput({
            onAddTag: function (value, uid) {
                var tagSuggestion = $(tagSuggestions).filter(function () {

                    return this.TagName.toString().replace("*", "").replace(/^\s+|\s+$/g, '').toLowerCase() == value.toString().replace("*", "").replace(/^\s+|\s+$/g, '').toLowerCase().trim();
                })[0];

                var tag = $(viewModel.TagsList()).filter(function () {
                    return this.TagName.toString().replace("*", "").replace(/^\s+|\s+$/g, '').toLowerCase() === value.toString().replace("*", "").replace(/^\s+|\s+$/g, '').toLowerCase();
                })[0];

                if (tag == null || tag == undefined) {

                    if (tagSuggestion == null || tagSuggestion == undefined) {
                        if (viewModel.TagsList() === null)
                            viewModel.TagsList([]);
                        if (inputId == "addTags")
                            viewModel.TagsList.push({ TagID: uid, TagName: value, AccountID: accountId, CreatedBy: tagCreatedBy });  //() after TagsList          
                        else
                            viewModel.TagsList.push({ TagID: uid, TagName: value, AccountID: accountId, CreatedBy: tagCreatedBy });  //() after TagsList      
                    }
                    else {
                        viewModel.TagsList.push({ TagID: tagSuggestion.TagID, TagName: tagSuggestion.TagName, AccountID: accountId, CreatedBy: tagCreatedBy });
                    }
                }
                else if (uid == 0)
                    $("#" + inputId).removeTag(value);

            },
            onRemoveTag: function (value) {
                if (viewModel.hasOwnProperty('PopularTags') && viewModel.PopularTags() != null) {
                    var popularTagfilter = ko.utils.arrayFilter(viewModel.PopularTags(), function (Tag, index) {
                        if (Tag.TagName === value) {
                            Tag.IsSelected = false;
                            $('#populartag' + (index + 1)).parent('.checked').removeClass('checked');
                        }
                        return Tag.TagName === value;
                    });
                    var recentTagfilter = ko.utils.arrayFilter(viewModel.RecentTags(), function (Tag, index) {
                        if (Tag.TagName === value) {
                            Tag.IsSelected = false;
                            $('#recenttag' + (index + 1)).parent('.checked').removeClass('checked');
                        }
                        return Tag.TagName === value;
                    });
                }
                viewModel.TagsList.remove(function (tag) {
                    return tag.TagName == value;
                });
            }
        });


        if (viewModel.TagsList() != null) {
            $.each(viewModel.TagsList(), function (index, tagItem) {
                $("#" + inputId).addTag(tagItem.TagName, { UID: tagItem.TagID });
            });
        }

        $(txtTagId).kendoAutoComplete({
            minLength: 1,
            height: 500,
            placeholder: "[|Type a tag name|]",
            dataTextField: "TagName",
            select: autocomplete_select
        });

        $(document).on('keydown keypress', txtTagId, function () {
            typewatch(function () {
                var authToken = readCookie("accessToken");
                $.ajax({
                    url: webserviceUrl + "/Tag/Names",
                    type: 'get',
                    dataType: 'json',
                    data: { 'query': $(txtTagId).val() },
                    contentType: "application/json; charset=utf-8",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader('Authorization', 'Bearer ' + authToken);
                    },
                    success: function (tagsData) {
                        if (tagsData.Tags.length > 0) {
                            tagSuggestions = tagsData.Tags;
                        }
                        var autocomplete = $(txtTagId).data("kendoAutoComplete");
                        autocomplete.dataSource.data(tagsData.Tags);
                        autocomplete.search("");
                    },
                    error: function (response) {
                        notifyError(response.responseText);
                    }
                });
            }, 500);
        });

        function readCookie(name) {
            var nameEQ = escape(name) + "=";
            var ca = document.cookie.split(';');
            for (var i = 0; i < ca.length; i++) {
                var c = ca[i];
                while (c.charAt(0) == ' ') c = c.substring(1, c.length);
                if (c.indexOf(nameEQ) == 0) return unescape(c.substring(nameEQ.length, c.length));
            }
            return null;
        }

        function autocomplete_select(e) {
            var item = e.item;

            var dataItem = this.dataItem(e.item);
            var text = item.text();
            if (!$("#" + inputId).tagExist(text)) {
                $("#" + inputId).addTag(text, { UID: dataItem.TagID });
            }
            e.preventDefault();
        }
    };

    self.TagifyOpportunities = function (inputId) {
        var txtTagId = "#" + inputId + "_tag";
        $('#' + inputId).tagsInput({
            onAddTag: function (value) {

            },
            onRemoveTag: function (value) {
                viewModel.Opportunities.remove(function (opportunity) {
                    return opportunity.OpportunityName.toLowerCase() == value.toLowerCase();
                });

            }
        });

        if (viewModel.Opportunities() != null) {
            $.each(viewModel.Opportunities(), function (index, value) {
                $("#" + inputId).addTag(value.OpportunityName, { UID: value.OpportunityID });
            });
        }
    };

    self.TagifyContacts = function (inputId) {
        var txtTagId = "#" + inputId + "_tag";
        var ContactsSuggestions;

        var IsPeople = readCookie("IsPeople");
        $('#' + inputId).tagsInput({
            onAddTag: function (value, uid) {
                $("#" + inputId).text('');
                $(txtTagId).text('');
                $(txtTagId).val('');
                var contactSuggestion = $(ContactsSuggestions).filter(function () {
                    return this.Text.toString().toLowerCase() == value.toString().toLowerCase();
                })[0];
                //This condition is to check the contacts coming from Grid selection.
                var contact = $(viewModel.Contacts()).filter(function () {
                    return this.Id == uid;
                })[0];
                if (contact == null || contact == undefined) {                  
                    if (contactSuggestion === null || contactSuggestion === undefined) {                     
                        $("#" + inputId).removeTag(value);
                        return;
                    }
                    if (viewModel.Contacts() === null)
                        viewModel.Contacts([]);

                    viewModel.Contacts.push({ Id: contactSuggestion.DocumentId, FullName: contactSuggestion.Text });
                }
            },
            onRemoveTag: function (value, uid) {
                viewModel.Contacts.remove(function (contact) {
                    return contact.FullName.toString().toLowerCase().trim() === value.toString().toLowerCase().trim() && contact.Id === uid;
                });
                if (viewModel.Relationshipentry != null) {
                    viewModel.Relationshipentry.remove(function (Relationship) {
                        return Relationship.ContactId === uid;
                    })
                }
            }
        });


        if (viewModel.Contacts() != null) {
            $.each(viewModel.Contacts(), function (index, contact) {
                if (inputId == "OpportunityContacts") {
                    if (contact.FullName == " ")
                        contact.FullName = contact.Email.EmailId;
                    $("#" + inputId).addTag(contact.FullName, { UID: contact.Id });
                    //if (contact.ContactType == parseInt(IsPeople)) {
                        
                    //}
                }
                else {
                    $("#" + inputId).addTag(contact.FullName, { UID: contact.Id });
                }
            });
        }

        $(txtTagId).kendoAutoComplete({
            minLength: 1,
            placeholder: "[|Type a contact name|]",
            height: 500,
            filter: "contains",
            serverFiltering: true,
            dataTextField: "Text",
            select: autocomplete_select,
            change: function (e) {
                $(txtTagId).data("kendoAutoComplete").value("");
            },
            open: function (e) {
            },
            close: function (e) {
            }
        });

        $(document).on('keydown keypress', txtTagId, function () {
            typewatch(function () {
                var authToken = readCookie("accessToken");
                $.ajax({
                    url: webserviceUrl + "/Contact/FullName",
                    type: 'get',
                    dataType: 'json',
                    data: { 'query': $(txtTagId).val(), 'accountId': accountId },
                    contentType: "application/json; charset=utf-8",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                    },
                    success: function (contactsData) {
                        var contactdata = [];
                        if (contactsData.Results.length > 0) {

                            if (inputId == "OpportunityContacts") {
                              //  if (parseInt(IsPeople) == 2 && viewModel.Contacts().length == 1) {
                             //       contactdata = [];
                              //  }
                              //  else {
                                    for (var d = 0; d < contactsData.Results.length; d++) {
                                        if (contactsData.Results[d].ContactType == parseInt(IsPeople))
                                            contactdata.push(contactsData.Results[d]);
                                    }
                              //  }
                                ContactsSuggestions = contactdata;
                            }
                            else {
                                ContactsSuggestions = contactsData.Results;
                            }
                        }
                        else
                            ContactsSuggestions = [];

                        var autocomplete = $(txtTagId).data("kendoAutoComplete");
                        autocomplete.dataSource.data(ContactsSuggestions);
                        autocomplete.search("");
                    },
                    error: function (response) {
                        notifyError(response.responseText);
                    }
                });
            }, 500);
        });

        function autocomplete_select(e) {
            var item = e.item;
            var dataItem = this.dataItem(e.item);
            var text = item.text();
            if (!$("#" + inputId).tagExist(text)) {
                $("#" + inputId).addTag(text, { UID: dataItem.DocumentId });
            }
        }
    };

    self.TagifyRelatedContacts = function (inputId, typeId, baseUrl) {
        var suggestions;
        var txtTagId = "#" + inputId;
        var ContactsSuggestions;
        var contacts = $(txtTagId);

        $(txtTagId).kendoAutoComplete({
            minLength: 1,
            placeholder: "[|Type a contact name|]",
            height: 500,
            filter: "contains",
            serverFiltering: true,
            dataTextField: "Text",
            select: autocomplete_select,
            change: function (e) {
                if ($(txtTagId).data("kendoAutoComplete").value() == "")
                    viewModel.RelatedContact("");
            }
        });

        $(document).on('keydown keypress', txtTagId, function () {
            typewatch(function () {
                $.ajax({
                    url: baseUrl + "GetContacts",
                    type: 'get',
                    dataType: 'json',
                    data: {
                        'name': contacts.val(),
                        'typeId': typeId()
                    },
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
                    if (data.response.Results.length > 0) {
                        var contactsdata = data.response.Results;
                        $.each(contactsdata, function (index, value) {
                            x = value.Text.split('(');
                            if (x[1] != undefined) {
                                x[1] = x[1].replace(/\s+/g, '');
                                value.Text = x[0] + "\n" + " (" + x[1];
                            }
                        });
                        ContactsSuggestions = data.response.Results;
                    }
                    var autocomplete = $(txtTagId).data("kendoAutoComplete");
                    autocomplete.dataSource.data(data.response.Results);
                    autocomplete.search("");
                }).fail(function (error) {
                    // Display error message to user            
                    notifyError(error);
                })
                //    success: function (contactsData) {
                //        if (contactsData.Results.length > 0) {
                //            ContactsSuggestions = contactsData.Results;
                //        }
                //        var autocomplete = $(txtTagId).data("kendoAutoComplete");
                //        autocomplete.dataSource.data(contactsData.Results);
                //        autocomplete.search("");
                //    }
                //});
            }, 500);
        });

        function autocomplete_select(e) {
            var validContact = false;
            var dataItem = this.dataItem(e.item);
            viewModel.RelatedContact(dataItem.Text);
            viewModel.RelatedContactID(dataItem.DocumentId);
            $.each(ContactsSuggestions, function (index, suggestion) {
                if ($.trim(suggestion.Text) == $.trim(dataItem.Text)) {
                    viewModel.RelatedContacts([]);
                    if (typeId() == 1) {
                        viewModel.RelatedContacts().push({ Id: suggestion.Id, FullName: suggestion.Text });
                    }
                    else {
                        viewModel.RelatedContacts().push({ Id: suggestion.DocumentId, FullName: suggestion.Text });
                    }
                    validContact = true;
                }
            });

            if (!validContact) {
                viewModel.RelatedContacts([]);
            }
        }
    };

    self.TagifyEmailTags = function (inputId, typeId, baseUrl) {
        {
            var txtTagId = "#" + inputId + "_tag";
            var tagSuggestions;
            $('#' + inputId).tagsInput({
                onAddTag: function (value, uid) {

                    var tagsuggestions = $(tagSuggestions).filter(function () {
                        return this.TagID == uid;
                    })[0];

                    var tagdata = $(viewModel.Tags()).filter(function () {
                        if (uid > 0)
                            return this.TagID() == uid;
                    })[0];


                    if (tagdata == null) {
                        console.log("Hello")
                        if (tagsuggestions === null || tagsuggestions === undefined) {
                            $("#" + inputId).removeTag(value, 0);
                            var tagVM = new tagViewModel(value, uid, webserviceUrl);
                            contactSummaryByTag(tagVM);
                            return false;
                        }
                    }

                },
                onRemoveTag: function (value, uid) {
                    if (uid != 0) {

                        //if (viewModel.hasOwnProperty('PopularTags') && viewModel.PopularTags() != null) {
                        //    var popularTagfilter = ko.utils.arrayFilter(viewModel.PopularTags(), function (Tag, index) {
                        //        if (Tag.TagID == uid) {
                        //            Tag.IsSelected = false;
                        //            $('#populartag' + (index + 1)).parent('.checked').removeClass('checked');
                        //        }

                        //        return Tag.TagID == uid;
                        //    });
                        //    var recentTagfilter = ko.utils.arrayFilter(viewModel.RecentTags(), function (Tag, index) {
                        //        if (Tag.TagID == uid) {
                        //            Tag.IsSelected = false;
                        //            $('#recenttag' + (index + 1)).parent('.checked').removeClass('checked');
                        //        }
                        //        return Tag.TagID == uid;
                        //    });
                        //}


                        viewModel.Tags.remove(function (tag) {
                            return tag.TagID() == uid;
                        });
                        contactSummaryByTag();
                    }
                }
            });



            $(txtTagId).kendoAutoComplete({
                minLength: 1,
                height: 500,
                placeholder: "[|Type a tag name|]",
                dataTextField: "TagName",
                select: autocomplete_select,
                change: function (e) {
                    $(txtTagId).data("kendoAutoComplete").value("");
                },
                open: function (e) {
                }
            });

            $(document).on('keydown keypress', txtTagId, function () {
                typewatch(function () {
                    var authToken = readCookie("accessToken");
                    $.ajax({
                        url: webserviceUrl + "/Tag/Names",
                        type: 'get',
                        dataType: 'json',
                        data: { 'query': $(txtTagId).val(), 'accountId': accountId },
                        contentType: "application/json; charset=utf-8",
                        beforeSend: function (xhr) {
                            xhr.setRequestHeader('Authorization', 'Bearer ' + authToken);
                        },
                        success: function (tagsData) {
                            var tagData = [];

                            if (tagsData.Tags.length > 0) {

                                for (var d = 0; d < tagsData.Tags.length; d++) {
                                    if (tagsData.Tags[d].Count != 0)
                                        tagData.push(tagsData.Tags[d]);
                                }

                                tagSuggestions = tagData;
                            }
                            else {
                                tagSuggestions = tagsData.Tags;
                            }

                            //if (tagsData.Tags.length > 0) {
                            //    tagSuggestions = tagsData.Tags;
                            //}
                            var autocomplete = $(txtTagId).data("kendoAutoComplete");
                            autocomplete.dataSource.data(tagSuggestions);
                            autocomplete.search("");
                        },
                        error: function (response) {
                            notifyError(response.responseText);
                        }
                    });
                }, 500);
            });

            function autocomplete_select(e) {

                var item = e.item;
                var text = item.text();

                var tag = $(tagSuggestions).filter(function () {
                    return this.TagName.toString().toLowerCase() == text.toString().toLowerCase();
                })[0];

                console.log("Hello");
                var tagVM = new tagViewModel(tag.TagName, tag.TagID, webserviceUrl);
                contactSummaryByTag(tagVM);
            }

            console.log(webserviceUrl);
            var contactSummaryByTag = function (tag) {
                $("#add-tag-loader").removeClass("hide");
                var jsonData;
                var allContactTags = viewModel.Tags().slice();
                var allSearchDefinitions = viewModel.SearchDefinitions().slice();
                if (typeof tag != "undefined") {
                    allContactTags.push(tag);
                    jsonData = { 'ContactTag': tag, 'AllSearchDefinitions': allSearchDefinitions, 'AccountId': accountId, 'AllContactTags': allContactTags, 'ToTagStatus': 0 };
                }
                else
                    jsonData = { 'AllSearchDefinitions': allSearchDefinitions, 'AccountId': accountId, 'AllContactTags': allContactTags, 'ToTagStatus': 0 };

                var tagExists = function (tagName) {
                    var hasTag = false;
                    $.each(viewModel.Tags(), function (index, object) {

                        if (object.TagName() == tagName)
                            hasTag = true;
                    });
                    return hasTag;
                };
                var authToken = readCookie("accessToken");
                $.ajax({
                    url: webserviceUrl + '/EmailValidatorContactsCount',
                    type: 'post',
                    data: ko.toJSON(jsonData),
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                    },
                    success: function (summaryData) {
                        if (tag != null) {
                            tag.TotalContacts(summaryData.Recipients.AllContactsByTag);
                            viewModel.TotalContactsCount(summaryData.Recipients.CampaignRecipientsCount);
                            tag.TagName(tag.TagName() + " (" + summaryData.Recipients.AllContactsByTag + ")");
                            if (!$("#" + inputId).tagExist(tag.TagName())) {
                                if (tagExists(tag.TagName()) === false)
                                    viewModel.Tags.push(tag);
                                $("#" + inputId).addTag(tag.TagName(), { UID: tag.TagID() });

                            }
                        }
                        //viewModel.TotalUniqueContacts(summaryData.CampaignRecipientsCount);
                        //viewModel.TotalActiveUniqueContacts(summaryData.Recipients.CampaignActiveRecipientsCount);
                        //viewModel.TotalAllAndActiveUniqueContacts(summaryData.Recipients.CampaignALLandACTIVERecipientsCount);
                        //viewModel.TotalActiveAndAllUniqueContacts(summaryData.Recipients.CampaignACTIVEandALLRecipientsCount);

                        //viewModel.To_All(summaryData.Recipients.AllContactsByTag);
                        //viewModel.To_Active(summaryData.Recipients.ActiveContactsByTag);

                        //if (viewModel.IsRecipientsProcessed())
                        //    $(".tagsinput-remove-link").attr("disabled", true).off('click');
                        //else
                        //    $(".tagsinput-remove-link").attr("disabled", false);


                        $("#add-tag-loader").addClass("hide");
                    },
                    error: function (response) {
                        notifyError(response.responseText);
                    }
                });
            };

            //if (viewModel.ContactTags() != null) {
            //    $.each(viewModel.ContactTags(), function (index, contactTag) {

            //        var tagVM = new tagViewModel(contactTag.TagName(), contactTag.TagID(), webserviceUrl);
            //        contactSummaryByTag(tagVM);
            //    });

            //}
        }
    }
    self.TagifyEmailSearchDefinitions = function (inputId, accountId) {
        var txtTagId = "#" + inputId + "_tag";
        var tagSuggestions;
        $('#' + inputId).tagsInput({
            onAddTag: function (value, uid) {
                var tagsuggestions = $(tagSuggestions).filter(function () {
                    return this.TagID == uid;
                })[0];

                var tagdata = $(viewModel.SearchDefinitions()).filter(function () {
                    return this.SearchDefinitionID() == uid;
                })[0];

                if (tagdata == null) {
                    if (tagsuggestions === null || tagsuggestions === undefined) {
                        $("#" + inputId).removeTag(value);
                        return;
                    }
                }
            },
            onRemoveTag: function (value, uid) {
                viewModel.SearchDefinitions.remove(function (tag) {
                    return tag.SearchDefinitionID() == uid;
                });
                contactSummaryBySearchDefinition();
            }
        });

        $(txtTagId).kendoAutoComplete({
            minLength: 1,
            height: 500,
            placeholder: "[|Type a search Saved Search name|]",
            dataTextField: "SearchDefinitionName",
            select: autocomplete_select,
            change: function (e) {
                $(txtTagId).data("kendoAutoComplete").value("");
            }
        });

        $(document).on('keydown keypress', txtTagId, function () {
            typewatch(function () {
                var authToken = readCookie("accessToken");
                $.ajax({
                    url: webserviceUrl + "/getsavedsearches",
                    type: 'get',
                    dataType: 'json',
                    data: 'query=' + $(txtTagId).val(),
                    contentType: "application/json; charset=utf-8",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader('Authorization', 'Bearer ' + authToken);
                    },
                    success: function (tagsData) {
                        if (tagsData.SearchResults.length > 0) {
                            tagSuggestions = tagsData.SearchResults;
                        }
                        var autocomplete = $(txtTagId).data("kendoAutoComplete");
                        autocomplete.dataSource.data(tagsData.SearchResults);
                        autocomplete.search("");
                    },
                    error: function (response) {
                        notifyError(response.responseText);
                    }
                });
            }, 500);
        });

        function autocomplete_select(e) {
            $("#add-definition-loader").removeClass("hide");
            var item = e.item;
            var text = item.text();
            var searchDefinition = $(tagSuggestions).filter(function () {
                return this.SearchDefinitionName.toString().toLowerCase() == text.toString().toLowerCase();
            })[0];
            var tagVM = new tagViewModel(searchDefinition.SearchDefinitionName, searchDefinition.SearchDefinitionID, webserviceUrl);
            contactSummaryBySearchDefinition(tagVM);
        }

        var contactSummaryBySearchDefinition = function (tagVM) {
            if (typeof tagVM != "undefined") {
                var searchDefinition = new searchDefinitionViewModel(tagVM.TagName(), tagVM.TagID(), tagVM.webserviceUrl);
            }
            var jsonData;
            var allSearchDefinitions = viewModel.SearchDefinitions().slice();
            var allContactTags = viewModel.Tags().slice();
            if (typeof searchDefinition != "undefined") {
                allSearchDefinitions.push(searchDefinition);
                jsonData = { 'SearchDefinition': searchDefinition, 'AllSearchDefinitions': allSearchDefinitions, 'AccountId': accountId, 'AllContactTags': allContactTags };
            }
            else
                jsonData = { 'AllSearchDefinitions': allSearchDefinitions, 'AccountId': accountId, 'AllContactTags': allContactTags };

            var tagExists = function (tagName) {
                var hasTag = false;
                $.each(viewModel.SearchDefinitions(), function (index, object) {
                    if (object.SearchDefinitionName().toLowerCase() == tagName.toLowerCase())
                        hasTag = true;
                });
                return hasTag;
            };
            var authToken = readCookie("accessToken");
            $.ajax({
                url: webserviceUrl + '/EmailValidatorContactsCount',
                type: 'post',
                data: ko.toJSON(jsonData),
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                },
                success: function (summaryData) {
                    if (searchDefinition != null) {
                        searchDefinition.TotalContacts(summaryData.Recipients.AllContactsBySS);
                        tagVM.TagName(tagVM.TagName() + " (" + summaryData.Recipients.AllContactsBySS + ")");
                        viewModel.TotalContactsCount(summaryData.Recipients.CampaignRecipientsCount);
                        //viewModel.TotalUniqueContacts(summaryData.CampaignRecipientsCount);
                        //viewModel.TotalActiveUniqueContacts(summaryData.Recipients.CampaignActiveRecipientsCount);
                        //viewModel.TotalAllAndActiveUniqueContacts(summaryData.Recipients.CampaignALLandACTIVERecipientsCount);
                        //viewModel.TotalActiveAndAllUniqueContacts(summaryData.Recipients.CampaignACTIVEandALLRecipientsCount);

                        //viewModel.SS_All(summaryData.Recipients.AllContactsBySS);
                        //viewModel.SS_Active(summaryData.Recipients.ActiveContactsBySS);

                        //viewModel.To_All(summaryData.Recipients.AllContactsByTag);
                        //viewModel.To_Active(summaryData.Recipients.ActiveContactsByTag);

                        searchDefinition.SearchDefinitionName(tagVM.TagName());
                        if (tagExists(tagVM.TagName()) === false) {
                            viewModel.SearchDefinitions.push(searchDefinition);
                            $("#" + inputId).addTag(searchDefinition.SearchDefinitionName(), { UID: tagVM.TagID() });
                            //  $("#" + inputId).addTag(searchDefinition.SearchDefinitionName());
                        }

                        //if (viewModel.IsRecipientsProcessed())
                        //    $(".tagsinput-remove-link").attr("disabled", true).off('click');
                        //else
                        //    $(".tagsinput-remove-link").attr("disabled", false);

                        $("#add-definition-loader").addClass("hide");
                    } 
                },
                error: function (response) {
                    notifyError(response.responseText);
                }
            });
        };

        //if (viewModel.SearchDefinitions() != null) {
        //    $.each(viewModel.SearchDefinitions(), function (index, searchDefinition) {
        //        // $("#" + inputId).addTag(definition.SearchDefinitionName);
        //        var tagVM = new tagViewModel(searchDefinition.SearchDefinitionName(), searchDefinition.SearchDefinitionID(), webserviceUrl);
        //        contactSummaryBySearchDefinition(tagVM);
        //    });
        //}
    }

    self.TagifyContactTags = function (inputId, accountId) {
        var txtTagId = "#" + inputId + "_tag";
        var tagSuggestions;
        $('#' + inputId).tagsInput({
            onAddTag: function (value, uid) {            
                var tagsuggestions = $(tagSuggestions).filter(function () {
                    return this.TagID == uid;
                })[0];
                var tagdata = $(viewModel.ContactTags()).filter(function () {
                    if (uid > 0)
                        return this.TagID() == uid;
                })[0];             
                if (tagdata == null) {
                    if (tagsuggestions === null || tagsuggestions === undefined) {
                       $("#" + inputId).removeTag(value,0);                                              
                        var tagVM = new tagViewModel(value, uid, webserviceUrl);                  
                       contactSummaryByTag(tagVM,"Adding");
                       return false;
                    } 
                }
               
            },
            onRemoveTag: function (value, uid) {
                if (uid != 0) {
                    if (viewModel.hasOwnProperty('PopularTags') && viewModel.PopularTags() != null) {
                        var popularTagfilter = ko.utils.arrayFilter(viewModel.PopularTags(), function (Tag, index) {
                            if (Tag.TagID == uid) {
                                Tag.IsSelected = false;
                                $('#populartag' + (index + 1)).parent('.checked').removeClass('checked');
                            }

                            return Tag.TagID == uid;
                        });
                        var recentTagfilter = ko.utils.arrayFilter(viewModel.RecentTags(), function (Tag, index) {
                            if ( Tag.TagID == uid) {
                                Tag.IsSelected = false;
                                $('#recenttag' + (index + 1)).parent('.checked').removeClass('checked');
                            }
                            return Tag.TagID == uid;
                        });
                    }

                    viewModel.ContactTags.remove(function (tag) {
                        return tag.TagID() == uid;
                    });
                    contactSummaryByTag("undefined","Removing");
                }
            }
        });



        $(txtTagId).kendoAutoComplete({
            minLength: 1,
            height: 500,
            placeholder: "[|Type a tag name|]",
            dataTextField: "TagName",
            select: autocomplete_select,
            change: function (e) {
                $(txtTagId).data("kendoAutoComplete").value("");
            },
            open: function (e) {
            }
        });

        $(document).on('keydown keypress', txtTagId, function () {
            typewatch(function () {
                var authToken = readCookie("accessToken");
                $.ajax({
                    url: webserviceUrl + "/Tag/Names",
                    type: 'get',
                    dataType: 'json',
                    data: { 'query': $(txtTagId).val(), 'accountId': accountId },
                    contentType: "application/json; charset=utf-8",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader('Authorization', 'Bearer ' + authToken);
                    },
                    success: function (tagsData) {
                        var tagData = [];
                        if (tagsData.Tags.length > 0) {

                            for (var d = 0; d < tagsData.Tags.length; d++) {
                                if (tagsData.Tags[d].Count != 0)
                                    tagData.push(tagsData.Tags[d]);
                            }

                            tagSuggestions = tagData;
                        }
                        else {
                            tagSuggestions = tagsData.Tags;
                        }

                        //if (tagsData.Tags.length > 0) {
                        //    tagSuggestions = tagsData.Tags;
                        //}
                        var autocomplete = $(txtTagId).data("kendoAutoComplete");
                        autocomplete.dataSource.data(tagSuggestions);
                        autocomplete.search("");
                    },
                    error: function (response) {
                        notifyError(response.responseText);
                    }
                });
            }, 500);
        });

        function autocomplete_select(e) {
            var item = e.item;
            var text = item.text();

            var tag = $(tagSuggestions).filter(function () {
                return this.TagName.toString().toLowerCase() == text.toString().toLowerCase();
            })[0];

            var tagVM = new tagViewModel(tag.TagName, tag.TagID, webserviceUrl);
            contactSummaryByTag(tagVM,"Adding");
        }

        var contactSummaryByTag = function (tag,tagType) {
            $("#add-tag-loader").removeClass("hide");
            var jsonData;
            var allContactTags = viewModel.ContactTags().slice();
            var allSearchDefinitions = viewModel.SearchDefinitions().slice();
            if (typeof tag != "undefined") {
                allContactTags.push(tag);
                jsonData = { 'ContactTag': tag, 'AllSearchDefinitions': allSearchDefinitions, 'AccountId': accountId, 'AllContactTags': allContactTags, 'ToTagStatus': viewModel.ToTagStatus() };
            }
            else
                jsonData = { 'AllSearchDefinitions': allSearchDefinitions, 'AccountId': accountId, 'AllContactTags': allContactTags, 'ToTagStatus': viewModel.ToTagStatus() };

            var tagExists = function (tagName) {
                var hasTag = false;
                $.each(viewModel.ContactTags(), function (index, object) {

                    if (object.TagName() == tagName)
                        hasTag = true;
                });
                return hasTag;
            };
            var authToken = readCookie("accessToken");
            $.ajax({
                url: webserviceUrl + '/CampaignRecipientsCount',
                type: 'post',
                data: ko.toJSON(jsonData),
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                },
                success: function (summaryData) {

                    if (viewModel.ContactTags() != null && (tag == null || tag == "undefined") && tagType != "Removing") {
                        $.each(summaryData.TagCounts, function (index, value) {
                            var x = parseInt(index, 10);
                            var getextag = null;
                            $.each(viewModel.ContactTags(),function(index,contactTag){
                                if (x == contactTag.TagID())
                                    getextag = contactTag;
                            })
                            getextag.TotalContacts(value);
                            getextag.TagName(getextag.TagName() + " (" + value + ")");
                            if (!$("#" + inputId).tagExist(getextag.TagName())) {
                                if (tagExists(getextag.TagName()) === false)
                                    viewModel.ContactTags.push(getextag);
                                $("#" + inputId).addTag(getextag.TagName(), { UID: getextag.TagID() });

                            }
                        })
                    }
                    else {
                        if (tag != null && tagType != "Removing") {
                            tag.TotalContacts(summaryData.CountByTag);

                            tag.TagName(tag.TagName() + " (" + summaryData.CountByTag + ")");
                            if (!$("#" + inputId).tagExist(tag.TagName())) {
                                if (tagExists(tag.TagName()) === false)
                                    viewModel.ContactTags.push(tag);
                                $("#" + inputId).addTag(tag.TagName(), { UID: tag.TagID() });

                            }
                        }
                    }

                    
                    viewModel.TotalUniqueContacts(summaryData.CampaignRecipientsCount);
                    viewModel.TotalActiveUniqueContacts(summaryData.Recipients.CampaignActiveRecipientsCount);
                    viewModel.TotalAllAndActiveUniqueContacts(summaryData.Recipients.CampaignALLandACTIVERecipientsCount);
                    viewModel.TotalActiveAndAllUniqueContacts(summaryData.Recipients.CampaignACTIVEandALLRecipientsCount);

                    viewModel.To_All(summaryData.Recipients.AllContactsByTag);
                    viewModel.To_Active(summaryData.Recipients.ActiveContactsByTag);

                    if (viewModel.IsRecipientsProcessed())
                        $(".tagsinput-remove-link").attr("disabled", true).off('click');
                    else
                        $(".tagsinput-remove-link").attr("disabled", false);
                       

                    $("#add-tag-loader").addClass("hide");
                },
                error: function (response) {
                    notifyError(response.responseText);
                }
            });
        };

        if (viewModel.ContactTags() != null) {
            contactSummaryByTag("undefined","Edit");
        }
    };

    self.TagifyTag = function (inputId, sourceTagId) {
        var txtId = inputId;
        var txtTagId = "#" + inputId;
        $(txtTagId).kendoAutoComplete([]);
        var tagNames = $(txtTagId);
        var dataSource = null;
        tagNames.bind('keydown keypress', function () {
            typewatch(function () {
                var authToken = readCookie("accessToken");
                $.ajax({
                    url: webserviceUrl + "/Tag/Names",
                    type: 'get',
                    dataType: 'json',
                    data: { 'query': $(txtTagId).val(), 'accountId': accountId },
                    contentType: "application/json; charset=utf-8",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader('Authorization', 'Bearer ' + authToken);
                    },
                    success: function (tagsData) {
                        self.Tags = tagsData.Tags;
                        var suggests = [];
                        $.each(tagsData.Tags, function (index, value) {
                            if (value.TagID != parseInt(sourceTagId))
                                suggests.push(value.TagName);
                        });
                        var autocomplete = $(txtTagId).data("kendoAutoComplete");
                        autocomplete.dataSource.data(suggests);
                        autocomplete.search("");
                    },
                    error: function (response) {
                        notifyError(response.responseText);
                    }
                });
            }, 500);
        });
    }

    self.TagifySearchDefinitions = function (inputId, accountId) {
        var txtTagId = "#" + inputId + "_tag";
        var tagSuggestions;
        $('#' + inputId).tagsInput({
            onAddTag: function (value, uid) {
                var tagsuggestions = $(tagSuggestions).filter(function () {
                    return this.TagID == uid;
                })[0];

                var tagdata = $(viewModel.SearchDefinitions()).filter(function () {
                    return this.SearchDefinitionID() == uid;
                })[0];

                if (tagdata == null) {
                    if (tagsuggestions === null || tagsuggestions === undefined) {
                        $("#" + inputId).removeTag(value);
                        return;
                    }
                }
            },
            onRemoveTag: function (value, uid) {
                viewModel.SearchDefinitions.remove(function (tag) {
                    return tag.SearchDefinitionID() == uid;
                });              
                contactSummaryBySearchDefinition(undefined, "Removing");
            }
        });

        $(txtTagId).kendoAutoComplete({
            minLength: 1,
            height: 500,
            placeholder: "[|Type a search definition name|]",
            dataTextField: "SearchDefinitionName",
            select: autocomplete_select,
            change: function (e) {
                $(txtTagId).data("kendoAutoComplete").value("");
            }
        });

        $(document).on('keydown keypress', txtTagId, function () {
            typewatch(function () {
                var authToken = readCookie("accessToken");
                $.ajax({
                    url: webserviceUrl + "/getsavedsearches",
                    type: 'get',
                    dataType: 'json',
                    data: 'query=' + $(txtTagId).val(),
                    contentType: "application/json; charset=utf-8",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader('Authorization', 'Bearer ' + authToken);
                    },
                    success: function (tagsData) {
                        if (tagsData.SearchResults.length > 0) {
                            tagSuggestions = tagsData.SearchResults;
                        }
                        var autocomplete = $(txtTagId).data("kendoAutoComplete");
                        autocomplete.dataSource.data(tagsData.SearchResults);
                        autocomplete.search("");
                    },
                    error: function (response) {
                        notifyError(response.responseText);
                    }
                });
            }, 500);
        });

        function autocomplete_select(e) {
            $("#add-definition-loader").removeClass("hide");
            var item = e.item;
            var text = item.text();
            var searchDefinition = $(tagSuggestions).filter(function () {
                return this.SearchDefinitionName.toString().toLowerCase() == text.toString().toLowerCase();
            })[0];
            var tagVM = new tagViewModel(searchDefinition.SearchDefinitionName, searchDefinition.SearchDefinitionID, webserviceUrl);
            contactSummaryBySearchDefinition(tagVM,"Adding");
        }

        var contactSummaryBySearchDefinition = function (tagVM,SDType) {
            if (typeof tagVM != "undefined") {
                var searchDefinition = new searchDefinitionViewModel(tagVM.TagName(), tagVM.TagID(), tagVM.webserviceUrl);
            }
            var jsonData;
            var allSearchDefinitions = viewModel.SearchDefinitions().slice();
            var allContactTags = viewModel.ContactTags().slice();

            if (typeof searchDefinition != "undefined") {
                allSearchDefinitions.push(searchDefinition);
                jsonData = { 'SearchDefinition': searchDefinition, 'AllSearchDefinitions': allSearchDefinitions, 'AccountId': accountId, 'AllContactTags': allContactTags };
            }
            else
                jsonData = { 'AllSearchDefinitions': allSearchDefinitions, 'AccountId': accountId, 'AllContactTags': allContactTags };

            var tagExists = function (tagName) {
                var hasTag = false;
                $.each(viewModel.SearchDefinitions(), function (index, object) {
                    if (object.SearchDefinitionName().toLowerCase() == tagName.toLowerCase())
                        hasTag = true;
                });
                return hasTag;
            };
            var authToken = readCookie("accessToken");
            $.ajax({
                url: webserviceUrl + '/CampaignRecipientsCount',
                type: 'post',
                data: ko.toJSON(jsonData),
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                },
                success: function (summaryData) {
                    if (viewModel.SearchDefinitions() != null && (searchDefinition == null || searchDefinition == "undefined") && SDType != "Removing") {
                        $.each(summaryData.SDefinitionCounts, function (index, value) {
                            var x = parseInt(index, 10);
                            var getexistSD = null;
                            $.each(viewModel.SearchDefinitions(), function (index, searchDefinition) {
                                if (x == searchDefinition.SearchDefinitionID())
                                    getexistSD = searchDefinition;
                            })
                            getexistSD.TotalContacts(value);
                            getexistSD.SearchDefinitionName(getexistSD.SearchDefinitionName() + " (" + value + ")");
                            if (tagExists(getexistSD.SearchDefinitionName()) === false) {
                                viewModel.SearchDefinitions.push(getexistSD);
                                $("#" + inputId).addTag(getexistSD.SearchDefinitionName(), { UID: getexistSD.SearchDefinitionID() });
                                //  $("#" + inputId).addTag(searchDefinition.SearchDefinitionName());
                            }
                            $("#" + inputId).addTag(getexistSD.SearchDefinitionName(), { UID: getexistSD.SearchDefinitionID() });

                           
                        })

                        viewModel.TotalUniqueContacts(summaryData.CampaignRecipientsCount);
                        viewModel.TotalActiveUniqueContacts(summaryData.Recipients.CampaignActiveRecipientsCount);
                        viewModel.TotalAllAndActiveUniqueContacts(summaryData.Recipients.CampaignALLandACTIVERecipientsCount);
                        viewModel.TotalActiveAndAllUniqueContacts(summaryData.Recipients.CampaignACTIVEandALLRecipientsCount);

                        viewModel.SS_All(summaryData.Recipients.AllContactsBySS);
                        viewModel.SS_Active(summaryData.Recipients.ActiveContactsBySS);

                        viewModel.To_All(summaryData.Recipients.AllContactsByTag);
                        viewModel.To_Active(summaryData.Recipients.ActiveContactsByTag);

                        if (viewModel.IsRecipientsProcessed())
                            $(".tagsinput-remove-link").attr("disabled", true).off('click');
                        else
                            $(".tagsinput-remove-link").attr("disabled", false);

                        $("#add-definition-loader").addClass("hide");
                    }
                    else {
                        if (searchDefinition != null && SDType != "Removing") {
                            searchDefinition.TotalContacts(summaryData.CountBySearchDefinition);
                            tagVM.TagName(tagVM.TagName() + " (" + summaryData.CountBySearchDefinition + ")");

                            viewModel.TotalUniqueContacts(summaryData.CampaignRecipientsCount);
                            viewModel.TotalActiveUniqueContacts(summaryData.Recipients.CampaignActiveRecipientsCount);
                            viewModel.TotalAllAndActiveUniqueContacts(summaryData.Recipients.CampaignALLandACTIVERecipientsCount);
                            viewModel.TotalActiveAndAllUniqueContacts(summaryData.Recipients.CampaignACTIVEandALLRecipientsCount);

                            viewModel.SS_All(summaryData.Recipients.AllContactsBySS);
                            viewModel.SS_Active(summaryData.Recipients.ActiveContactsBySS);

                            viewModel.To_All(summaryData.Recipients.AllContactsByTag);
                            viewModel.To_Active(summaryData.Recipients.ActiveContactsByTag);

                            searchDefinition.SearchDefinitionName(tagVM.TagName());
                            if (tagExists(tagVM.TagName()) === false) {
                                viewModel.SearchDefinitions.push(searchDefinition);
                                $("#" + inputId).addTag(searchDefinition.SearchDefinitionName(), { UID: tagVM.TagID() });
                                //  $("#" + inputId).addTag(searchDefinition.SearchDefinitionName());
                            }

                            if (viewModel.IsRecipientsProcessed())
                                $(".tagsinput-remove-link").attr("disabled", true).off('click');
                            else
                                $(".tagsinput-remove-link").attr("disabled", false);

                            $("#add-definition-loader").addClass("hide");
                        } else {

                            viewModel.TotalUniqueContacts(summaryData.CampaignRecipientsCount);
                            viewModel.TotalActiveUniqueContacts(summaryData.Recipients.CampaignActiveRecipientsCount);
                            viewModel.TotalAllAndActiveUniqueContacts(summaryData.Recipients.CampaignALLandACTIVERecipientsCount);
                            viewModel.TotalActiveAndAllUniqueContacts(summaryData.Recipients.CampaignACTIVEandALLRecipientsCount);

                            viewModel.SS_All(summaryData.Recipients.AllContactsBySS);
                            viewModel.SS_Active(summaryData.Recipients.ActiveContactsBySS);

                            viewModel.To_All(summaryData.Recipients.AllContactsByTag);
                            viewModel.To_Active(summaryData.Recipients.ActiveContactsByTag);
                        }
                        
                    }

                },
                error: function (response) {
                    notifyError(response.responseText);
                }
            });
        };

        if (viewModel.SearchDefinitions() != null) {
            contactSummaryBySearchDefinition(undefined,"Edit");
        }
    };

    self.TagifyEmailContacts = function (inputId, userID) {
        var txtTagId = "#" + inputId + "_tag";

        var ContactsSuggestions;
        $('#' + inputId).tagsInput({
            onAddTag: function (value, uid) {
                $("#" + inputId).text('');
                $(txtTagId).text('');
                $(txtTagId).val('');
                var contactSuggestion = $(ContactsSuggestions).filter(function () {
                    return this.Text.toString().toLowerCase() == value.toString().toLowerCase();
                })[0];
                ko.toJSON(contactSuggestion);

                //This condition is to check the contacts coming from Grid selection.
                var contact = $(viewModel.Contacts()).filter(function () {
                    return this.DocumentId === uid;
                })[0];

                if (contact == null) {
                    if (contactSuggestion === null || contactSuggestion === undefined) {
                        $("#" + inputId).removeTag(value);
                        return;
                    }

                    var EmailID = GetEmailFromString(value);

                    if (inputId == "txtTo" || inputId == "txtToModel") {

                        viewModel.Contacts.push({ DocumentId: contactSuggestion.DocumentId, FullName: contactSuggestion.Text, DocumentOwnedBy: userID, Type: "To", Email: EmailID });
                    }
                    else if (inputId == "txtBCC" || inputId == "txtBCCModel") {
                        viewModel.Contacts.push({ DocumentId: contactSuggestion.DocumentId, FullName: contactSuggestion.Text, DocumentOwnedBy: userID, Type: "BCC", Email: EmailID });
                    }
                    else if (inputId == "txtCC" || inputId == "txtCCModel") {

                        viewModel.Contacts.push({ DocumentId: contactSuggestion.DocumentId, FullName: contactSuggestion.Text, DocumentOwnedBy: userID, Type: "CC", Email: EmailID });
                    }
                }
            },
            onRemoveTag: function (value) {
                viewModel.Contacts.remove(function (contact) {
                    return contact.FullName.toLowerCase() == value.toLowerCase();
                });

            }
        });
        function GetEmailFromString(value) {
            var EmailString = value.split("<");
            EmailString = EmailString[1];
            var EmailID = EmailString.split(">");
            return EmailID[0];
        }

        if (viewModel.Contacts().length == 1) {
            $.each(viewModel.Contacts(), function (index, contact) {                
                if (inputId == "txtToModel" || inputId == "txtTo") {
                    contact.Email = GetEmailFromString(contact.Text);
                    contact.DocumentOwnedBy = userID;
                    contact.FullName = contact.Text;
                    contact.Type = "To";
                    $("#" + inputId).addTag(contact.Text, { UID: contact.DocumentId });
                }
            });
        }
        else {            
            $.each(viewModel.Contacts(), function (index, contact) {
                if (inputId == "txtBCCModel" || inputId == "txtBCC") {
                    contact.Email = GetEmailFromString(contact.Text);
                    contact.DocumentOwnedBy = userID;
                    contact.FullName = contact.Text;
                    contact.Type = "BCC";
                    $("#" + inputId).addTag(contact.Text, { UID: contact.DocumentId });
                }
            });
        }


        $(txtTagId).kendoAutoComplete({
            minLength: 1,
            height: 500,
            filter: "contains",
            serverFiltering: true,
            dataTextField: "Text",
            select: autocomplete_select,
            change: function (e) {
                $(txtTagId).data("kendoAutoComplete").value("");
            },
            open: function (e) {
            }
        });

        $(document).on('keydown keypress', txtTagId, function () {
            typewatch(function () {
                var authToken = readCookie("accessToken");
                $.ajax({
                    url: webserviceUrl + "/Contact/EmailId",
                    type: 'get',
                    dataType: 'json',
                    data: { 'query': $(txtTagId).val(), 'accountId': accountId },
                    contentType: "application/json; charset=utf-8",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                    },
                    success: function (contactsData) {
                        if (contactsData.Results.length > 0) {
                            ContactsSuggestions = contactsData.Results;
                        }
                        var autocomplete = $(txtTagId).data("kendoAutoComplete");

                        autocomplete.dataSource.data(contactsData.Results);
                        autocomplete.search("");
                    },
                    error: function (response) {
                        notifyError(response.responseText);
                    }
                });
            }, 500);
        });

        function autocomplete_select(e) {
            var item = e.item;
            var dataItem = this.dataItem(e.item);
            var text = item.text();
            if (!$("#" + inputId).tagExist(text)) {
                $("#" + inputId).addTag(text, { UID: dataItem.DocumentId });
            }
        }
    };

    self.TagifyTextContacts = function (inputId, userID) {
        var txtTagId = "#" + inputId + "_tag";
        var ContactsSuggestions;
        $('#' + inputId).tagsInput({
            onAddTag: function (value, uid) {
                $("#" + inputId).text('');
                $(txtTagId).text('');
                $(txtTagId).val('');

                var contactSuggestion = $(ContactsSuggestions).filter(function () {
                    return this.Text.toString().toLowerCase() == value.toString().toLowerCase();
                })[0];

                //This condition is to check the contacts coming from Grid selection.
                var contact = $(viewModel.Contacts()).filter(function () {
                    return this.DocumentId.toString().toLowerCase() === uid.toString().toLowerCase();
                })[0];

                var contacts = $(viewModel.Contacts()).filter(function () {
                    return this.DocumentId.toString().toLowerCase() === uid.toString().toLowerCase();
                });

                if (contact === null || contact === undefined) {
                    if (contactSuggestion === null || contactSuggestion === undefined) {
                        $("#" + inputId).removeTag(value, uid);
                        return;
                    }
                    if (viewModel.Contacts() === null)
                        viewModel.Contacts([]);
                    var phone = GetPhoneNumberFromString(value);
                    viewModel.Contacts.push({ DocumentId: contactSuggestion.DocumentId, FullName: contactSuggestion.Text, DocumentOwnedBy: userID, Phone: phone });
                }
            },
            onRemoveTag: function (value, uid) {
                viewModel.Contacts.remove(function (contact) {
                    return contact.DocumentId == uid;
                });
            }
        });

        $.each(viewModel.Contacts(), function (index, contact) {
            contact.DocumentOwnedBy = userID;
            contact.Phone = GetPhoneNumberFromString(contact.Text);
            $("#" + inputId).addTag(contact.FullName, { UID: contact.DocumentId });
        });

        function GetPhoneNumberFromString(value) {
            value = value.replace(/\s/g, '');
            var parts = value.match(/(\d{3}).*?(\d{3}).*?(\d{4})/);
            if (parts != null) {
                var phone = "";
                if (parts[1]) { phone += "(" + parts[1] + ") "; }
                phone += parts[2] + "-" + parts[3];
                return phone;
            }
            else
                return value;
        }

        $(txtTagId).kendoAutoComplete({
            minLength: 1,
            height: 500,
            filter: "contains",
            serverFiltering: true,
            dataTextField: "Text",
            select: autocomplete_select,
            change: function (e) {
                $(txtTagId).data("kendoAutoComplete").value("");
            },
            open: function (e) {
            }
        });

        $(document).on('keydown keypress', txtTagId, function () {
            typewatch(function () {
                var authToken = readCookie("accessToken");
                $.ajax({
                    url: webserviceUrl + "/Contact/Phone",
                    type: 'get',
                    dataType: 'json',
                    data: { 'query': $(txtTagId).val(), 'accountId': accountId },
                    contentType: "application/json; charset=utf-8",
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                    },
                    success: function (contactsData) {
                        if (contactsData.Results.length > 0) {
                            ContactsSuggestions = contactsData.Results;
                        }

                        var autocomplete = $(txtTagId).data("kendoAutoComplete");
                        autocomplete.dataSource.data(contactsData.Results);
                        autocomplete.search("");
                    },
                    error: function (response) {
                        notifyError(response.responseText);
                    }
                });
            }, 500);
        });

        function autocomplete_select(e) {
            var item = e.item;
            var dataItem = this.dataItem(e.item);
            var text = item.text();

            var tagviewmodel = $(tagsViewModel).filter(function () {
                return this.TagId.toString().toLowerCase() === dataItem.DocumentId.toString().toLowerCase();
            })[0];


            if (!$("#" + inputId).tagExist(text) && tagviewmodel == undefined) {
                $("#" + inputId).addTag(text, { UID: dataItem.DocumentId });
            }
            e.preventDefault();

        }
    };
};