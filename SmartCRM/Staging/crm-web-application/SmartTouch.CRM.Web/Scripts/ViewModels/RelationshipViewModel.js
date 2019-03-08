var relationshipViewModel = function (data, url, mode, DropDownURL) {
   
    selfRelation = this;
    ko.mapping.fromJS(data, {}, selfRelation);
    selfRelation.saveText = ko.observable('[|Save|]');
    selfRelation.addText = ko.observable('Add');
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfRelation));

    ko.validation.rules['minimumLength'] = {
        validator: function (val, contactsField) {
            if (val == null || val.length === 0)
                return false;
            return true;
        },
        message: '[|Select at least one contact|]'
    };

    var fromContactDetails = typeof selfDetails != "undefined" ? true : false;
    var fromOpportunityDetails = typeof selfOpportunity != "undefined" ? true : false;

    var notselectall;
    if (fromContactDetails == true)
        notselectall = true;
    else if (fromOpportunityDetails == true)
        notselectall = true;
    else
        notselectall = readCookie("selectallsearchstring") == "" ? true : false;

    selectedAll = notselectall == false ? true : false;

    selfRelation.RelatedContactTitle = ko.observable(data.RelatedContactTitle);
    ko.validation.registerExtenders();
    selfRelation.RelationshipType = ko.observable(data.RelationshipType);
    selfRelation.RelationshipTypes = ko.observableArray(data.RelationshipTypes);
   
    selfRelation.Relationshipentry = ko.observableArray(data.Relationshipentry);
    $.each(selfRelation.RelationshipTypes(), function (index, Relationtype) {

        if (Relationtype.IsDefault == true) {
            selfRelation.RelationshipType(Relationtype.DropdownValueID);
            selfRelation.RelatedContactTitle(Relationtype.DropdownValue);
        }
    })
    selfRelation.Contacts = ko.observableArray(data.Contacts);
    selfRelation.SelectAll = selectedAll;
    selfRelation.RelatedContacts = ko.observableArray([]);

    selfRelation.RelatedContact = ko.observable(data.RelatedContact);
    selfRelation.RelatedContactID = ko.observable(data.RelatedContactID);

    if (checkedContactValues.length > 0 || localStorage.getItem("contactdetails") != null)
        selfRelation.Contacts(selectedContacts(data.Id, checkedContactValues, data.Contacts));

    selfRelation.ContactFullNames = ko.computed({
        read: function () {
            var contactFullNames = "";
            if (selfRelation.Contacts() != null) {
                $.each(selfRelation.Contacts(), function (index, value) {
                    if (contactFullNames != null && contactFullNames != "" && value.FullName != null)
                        contactFullNames = contactFullNames + "," + value.FullName;
                    else
                        contactFullNames = contactFullNames + value.FullName;
                });
            }
            return contactFullNames;
        },
        write: function (newValue) { },
        owner: this
    });
    selfRelation.contactsValidation = selfRelation.ContactFullNames.extend({ minimumLength: 1 });

    if (mode == "EditView") {

        selfRelation.RelationshipType(data.RelationshipType);
        selfRelation.RelatedContactTitle(data.RelationshipTypeName);



        selfRelation.contactsValidation.rules.remove(function (item) {
            return item.rule = "required";
        });
    }

    selfRelation.RelationContactFullNames = ko.pureComputed({
        read: function () {
           
            var relatedcontactFullNames = "";

            if (selfRelation.RelatedContact() != null) {
                if (selfRelation.Id() == 0)
                    selfRelation.RelatedContacts.push({ Address: null, CompanyName: null, ContactType: 0, FullName: selfRelation.RelatedContact(), Id: selfRelation.RelatedContactID() });
                else
                    selfRelation.RelatedContacts([{ Address: null, CompanyName: null, ContactType: 0, FullName: selfRelation.RelatedContact(), Id: selfRelation.RelatedContactID() }]);

                relatedcontactFullNames = selfRelation.RelatedContact();
            }
            if (mode == "EditView" && selfRelation.Relationshipentry().length > 0) {
                selfRelation.Relationshipentry()[0].RelatedContactID = parseInt(selfRelation.RelatedContacts()[0].Id);
                selfRelation.Relationshipentry()[0].RelatedContact = selfRelation.RelatedContacts()[0].FullName;
            }
            return relatedcontactFullNames;
        },
        write: function (newValue1) {
            
            //selfRelation.RelationContactFullNames(newValue1);
            
            return newValue1;
        },
        owner: this
    }).extend({ minimumLength: 1 });

    //selfRelation.RelationContactFullNames = ko.observable(data.RelatedContact).extend({
    //    required: { message: '[|Add at least one person|]' }
    //});
    selfRelation.RelationshipType.subscribe(function (selectedValue) {

        var relationdata = ko.utils.arrayFirst(selfRelation.RelationshipTypes(), function (choice) {
            return choice.DropdownValueID == selectedValue;
        });
        selfRelation.RelatedContactTitle(relationdata.DropdownValue);
        if (mode == "EditView" && selfRelation.Relationshipentry().length > 0) {
            selfRelation.Relationshipentry()[0].RelationshipType = selectedValue;
        }
    });

    //selfRelation.RelationContactFullNames.subscribe(function (val) {

    //});
    selfRelation.uniqueRelationTypes = ko.computed(function () {
        return ko.utils.arrayGetDistinctValues(selfRelation.Relationshipentry().map(function (e) { return e.RelationshipType }));
    });

    selfRelation.errors = ko.validation.group(selfRelation);

    selfRelation.saveRelationship = function () {


        //if (mode == "EditView") {
        //    console.log('Edit View');
        //    console.log(data.RelationshipViewModel);
        //    return;
        //  
        //}
        if (selfRelation.Relationshipentry().length > 0 && selfRelation.SelectAll == true) {

            uniqueRelations = ko.observableArray([]);
            ko.utils.arrayForEach(selfRelation.uniqueRelationTypes(), function (type) {
                uniqueRelations.push(ko.utils.arrayFirst(selfRelation.Relationshipentry(), function (entry) { return entry.RelationshipType == type }));
            });
            selfRelation.Relationshipentry = uniqueRelations;

        }

        var jsondata = ko.toJSON(selfRelation);
        selfRelation.errors.showAllMessages();

        if (selfRelation.errors().length > 0)
            return;
        if (selfRelation.Relationshipentry().length > 0) {
           
            var action = "SaveRelation";
            selfRelation.saveText('[|Saving|]');
            innerLoader('Relationship');
            $.ajax({
                url: url + action,
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'relationViewModel': jsondata })

            }).then(function (response) {
               // removepageloader();
              //  removeinnerLoader('Relationship');
                var filter = $.Deferred()
                if (response.success) {
                    filter.resolve(response)
                }
                else {
                    filter.reject(response.error)
                }
                return filter.promise()
            }).done(function (data) {
                removeinnerLoader('Relationship');
                notifySuccess('[|Successfully saved Relationship(s)|]');
                setTimeout(
                     function () {
                         window.location.href = document.URL
                     }, setTimeOutTimer);
            }).fail(function (error) {
                removeinnerLoader('Relationship');
                selfRelation.Relationshipentry.removeAll();
                notifyError(error);
            })
            selfRelation.saveText('[|Save|]');
            //removepageloader();
            
        }
        else
            notifyError('[|Please add at least one relation|]')
    }
    var index = 0;
    selfRelation.addItem = function () {
        var defaultindex = ko.utils.arrayFirst(selfRelation.RelationshipTypes(), function (type) {
            return type.DropdownValueID == selfRelation.RelationshipType();
        });
        var varContactData = selfRelation.Contacts();
        var varRelContactData = selfRelation.RelatedContacts();
        index = selfRelation.RelationshipTypes().indexOf(defaultindex);
        if (varRelContactData.length > 0) {
            ++index;
        }
        if (index >= selfRelation.RelationshipTypes().length)
            index = 0;
        var varRelationTypeId = selfRelation.RelationshipType();
        
        if (varRelContactData.length > 0)
            var relContact = selfRelation.RelatedContacts()[0].Id;
        console.log(varContactData.length)
        console.log(varRelContactData.length)
        //console.log(selfRelation.RelatedContacts()[0].FullName)
        console.log(selfRelation.RelationContactFullNames())
        if (varContactData.length > 0 && varRelContactData.length > 0 && selfRelation.RelatedContacts()[0].FullName == selfRelation.RelationContactFullNames()) {
            circularRelations = [];
            for (var i = 0; i < varContactData.length; i++) {
                var varContact = parseInt(selfRelation.Contacts()[i].Id);
                var checkDuplicate = false;
                var checkDuplicaterelation = false;
                if (relContact == "") {
                    notifyError('[|Specify the relation contact Name|]');
                    return;
                }
                if (varContact == relContact) {
                    checkDuplicaterelation = true;
                    circularRelations.push(selfRelation.Contacts()[i].FullName);
                    continue;
                }
                
                if (selfRelation.Relationshipentry().length > 0) {
                    ko.utils.arrayForEach(selfRelation.Relationshipentry(), function (Relationship) {

                        if (Relationship.RelatedContactID == relContact && Relationship.ContactId == varContact
                            && Relationship.RelationshipType == varRelationTypeId) {
                            // alert('Duplicate');
                            checkDuplicate = true;
                        }
                    })
                }
                if (checkDuplicate == true) {

                   
                    notifyError('[|Duplicate Records are not allowed|]')

                }
                else {
                    console.log('Enter the loop' + i);
                    if (checkDuplicate == false && checkDuplicaterelation == false)

                        selfRelation.Relationshipentry.push({
                            ContactId: varContactData[i].Id,
                            RelationshipType: varRelationTypeId,
                            RelatedContactID: varRelContactData[0].Id,
                            DisplayContact: varContactData[i].FullName,
                            DisplayRelationShipTypeValues: selfRelation.RelatedContactTitle(),
                            RelatedContact: varRelContactData[0].FullName
                        });
                    selfRelation.RelatedContacts([]);
                    $("#addContactsModel,#addContacts").val("");
                }
            }
        }
        else {
            notifyError('Add at least one person')
            return;
        }
        if (circularRelations.length > 0) {
            var contactsExcluded = circularRelations.toString().replace(",", ", ");
            notifyError("<strong>" + contactsExcluded + "</strong> have been removed from the list since self relations are not accepted.");
        }
        selfRelation.RelationshipType(selfRelation.RelationshipTypes()[index].DropdownValueID);

    }.bind(this);
    selfRelation.removeRelationshipentry = function (obj) {
        alertifyReset("Delete record", "Cancel");
        alertify.confirm("[|Are you sure you want to delete this record |]?", function (e) {
            if (e) {
               
                selfRelation.Relationshipentry.remove(obj);
            }
            else
                alertify.error("[|You've clicked Cancel|]");
        });

    };
}

checkedContactValues = fnGetCheckedValues();