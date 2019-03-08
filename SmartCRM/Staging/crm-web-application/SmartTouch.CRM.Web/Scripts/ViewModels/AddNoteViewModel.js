var noteViewModel = function (data, url, notselectall) {
   var selfNote = this;
    var savedSuccessfully = false;

    ko.mapping.fromJS(data, {}, selfNote);

    ko.validation.rules['minimumLength'] = {
        validator: function (contacts, minimumLength) {
            return (contacts.length > minimumLength);
        },
        message: '[|Select at least one Contact|]'
    };

    ko.validation.rules['notecategoryCannotEqual'] = {
        validator: function (notecategory, otherVal) {
            return (notecategory != otherVal || notecategory != "");
        },
        message: '[|Select Note Category|]'
    };

    ko.validation.registerExtenders();

    ko.validation.init();
    ko.validation.configure({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null,
        grouping: { deep: true, observable: true, live: true }
    });
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfNote));

    if (notselectall == false)
        data.SelectAll = true;

    //alert(readCookie("opportunityid"));
    selfNote.NoteId = ko.observable(data.NoteId);
    selfNote.SelectAll = ko.observable(data.SelectAll);
    selfNote.Contacts = ko.observableArray(data.Contacts);
    selfNote.AddToContactSummary = ko.observable(data.AddToContactSummary);
    selfNote.NoteCategories = ko.observable(data.NoteCategories);
    selfNote.NoteCategory = ko.observable(data.NoteCategory).extend({ notecategoryCannotEqual: 0 });
    selfNote.notecategoryValidation = ko.observable();

    var selectedOpportunity = GetSelectedOpportunities('chkopportunity');
    selfNote.OppurtunityId = selectedOpportunity.length == 1 ? selectedOpportunity[0].OpportunityID : (readCookie("opportunityid") == null ? 0 : readCookie("opportunityid"));

    selfNote.NoteDetails = ko.observable(data.NoteDetails).extend({ required: { message: "[|Note details is required|]" } });

    if (checkedContactValues.length > 0 || localStorage.getItem("contactdetails") != null) {
        if (selfNote.SelectAll() == false)
            selfNote.Contacts(selectedContacts(data.NoteId, checkedContactValues, data.Contacts));

        else if (selfNote.SelectAll() == true)
            selfNote.Contacts(selectedContacts(0, checkedContactValues, data.Contacts));
    }

    var GetContacts;
    var arrayContacts;
    var contact;
    if (selectedOpportunity.length == 1) {
        GetContacts = (selectedOpportunity[0].PeopleInvolved).split(",");
        arrayContacts = [];
        if (typeof selfBuyer != "undefined") {
            for (contact = 0; contact < GetContacts.length; contact++) {
                if (GetContacts[contact].split("|")[0] != "") {
                    arrayContacts.push({ Address: null, CompanyName: null, ContactType: 0, FullName: GetContacts[contact].split("|")[0], Id: GetContacts[contact].split("|")[1] });
                }
            }
        }
        selfNote.Contacts(arrayContacts);
    }

    if (localStorage.getItem("contactsData") != null && (selfNote.Contacts() == undefined || selfNote.Contacts().length == 0)) {
        GetContacts = [];
        GetContacts = localStorage.getItem("contactsData").split("||");
        arrayContacts = [];
        if (typeof selfBuyer != "undefined") {
            for (contact = 0; contact < GetContacts.length; contact++) {
                if (GetContacts[contact].split("|")[0] != "") {
                    arrayContacts.push({ Address: null, CompanyName: null, ContactType: 0, FullName: GetContacts[contact].split("|")[0], Id: GetContacts[contact].split("|")[1] });
                }
            }
        }
        selfNote.Contacts(arrayContacts);
    }

    selfNote.ContactFullNames = ko.computed({
        read: function () {
            var contactFullNames = "";
            if (selfNote.Contacts() != null) {
                $.each(selfNote.Contacts(), function (index, value) {
                    if (contactFullNames != null && contactFullNames != "" && value.FullName != null)
                        contactFullNames = contactFullNames + "," + value.FullName;
                    else
                        contactFullNames = contactFullNames + value.FullName;
                });
            }
            return contactFullNames;
        },
        write: function () {
        },
        owner: this
    });
    if (selfNote.SelectAll() == false) {
        selfNote.contactsValidation = selfNote.ContactFullNames.extend({ minimumLength: 1 });
    }
    //selfNote.Contacts.subscribe(function () {
    //    contactsValidator();
    //});

    //var contactsValidator = function(){
    //    if (selfNote.Contacts() != null && selfNote.Contacts().length > 0)
    //        contactsValidation.clearError();
    //    else
    //        contactsValidation.setError('[|Select at least one Contact|]');
    //}

    selfNote.TagsList = ko.observableArray(data.TagsList);

    selfNote.NoteTags = ko.computed({
        read: function () {
            var tags = "";
            if (selfNote.TagsList() != null) {
                $.each(selfNote.TagsList(), function (index, value) {
                    if (tags != null && tags != "")
                        tags = tags + "," + value.TagName;
                    else
                        tags = tags + value.TagName;

                });
            }
            return tags;
        },
        write: function () { },
        owner: this
    });
    function DeleteConfirmation(okText, cancelText, confirmMessage, Note) {
        alertifyReset(okText, cancelText);
        alertify.confirm(confirmMessage, function (e) {
            if (e) {
                pageLoader();
                var action = "DeleteNote";
                $.ajax({
                    url: url + action,
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ 'noteId': Note.NoteId() })
                }).then(function (response) {
                    removepageloader();
                    var filter = $.Deferred();
                    if (response.success) {
                        filter.resolve(response);
                    } else {
                        filter.reject(response.error);
                    }
                    return filter.promise();
                }).done(function () {
                    $('.success-msg').remove();
                    notifySuccess('[|Successfully deleted Note|]');
                    createCookie('log', false, 1);
                    window.location.href = document.URL;
                }).fail(function (error) {
                    notifyError(error);
                });
            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }
        });
    }
    selfNote.DeleteNote = function (Note) {
        var note = "GetNoteContactsCount";
        $.ajax({
            url: url + note,
            type: 'post',
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({ 'noteId': Note.NoteId() })
        }).then(function (response) {
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response);
            } else {
                filter.reject(response.error);
            }
            return filter.promise();
        }).done(function (data) {
            if (data.response == 1) {
                DeleteConfirmation("Delete Note", "Cancel", "[|Are you sure you want to delete this Note?|]", Note);
            } else {
                DeleteConfirmation("Delete Note", "Cancel", "[|Note added to more than one Contact. Deleting this Note will remove it from all associated Contacts. Are you sure you want to delete?|]", Note);
            }
        }).fail(function (error) {
            notifyError(error);
        });
    };



    ko.validation.rules['minimumLength'] = {
        validator: function () {
            if (selfNote.Contacts() == null)
                return false;
            return selfNote.Contacts().length > 0;
        },
        message: '[|Select at least one Contact|]'
    };

    ko.validation.rules['notecategoryCannotEqual'] = {
        validator: function (notecategory, otherVal) {
            return (notecategory != otherVal || notecategory != "");
        },
        message: '[|Select Note Category|]'
    };

    ko.validation.registerExtenders();

    selfNote.errors = ko.validation.group(selfNote, {
        observable: true,
        deep: true,
        live: true
    });

    var saveNote = function (saveType) {
        selfNote.notecategoryValidation = selfNote.NoteCategory.extend({ notecategoryCannotEqual: 0 });
        if (selfNote.NoteId() == 0 && checkedContactValues.length == 0 && selfNote.SelectAll() == true) {
            notifyError("Select at least one Contact");
        }
        else {
            var jsondata = ko.toJSON(selfNote);
            var note = saveType;

            selfNote.errors.showAllMessages();
            //contactsValidator();

            if (selfNote.errors().length > 0)
                return;

            if (notselectall == false) {
                selfNote.SelectAll = ko.observable(true);
            }

            if (selectedOpportunity.length > 1) {
                notifyError("[|Please select only one opportunity|]");
                return;
            }


            innerLoader('Note');
            $.ajax({
                url: url + note,
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'noteViewModel': jsondata })
            }).then(function (response) {
                var filter = $.Deferred();
                if (response.success) {
                    filter.resolve(response);
                } else {
                    filter.reject(response.error);
                }
                return filter.promise();
            }).done(function () {
                notifySuccess("[|Successfully saved the Note|]");
                setTimeout(function () {
                    createCookie('log', false, 1);
                    window.location.href = document.URL;
                    removeinnerLoader('Note');
                }, setTimeOutTimer);
            }).fail(function (error) {
                removeinnerLoader('Note');
                notifyError(error);
            });
        }

    };

    selfNote.insertNote = function (item, event) {
        savedSuccessfully = true;
        saveNote("InsertNote", event.target);
    };

    selfNote.updateNote = function (item, event) {
        saveNote("UpdateNote", event.target);
    };

    selfNote.cancel = function (item, event) {
        CloseTopInner(event.target);
    };
}

checkedContactValues = fnGetCheckedValues();
