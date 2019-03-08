var ChangeOwnerViewModel = function (data, url, Users, notselectall) {
    var self = this;
    self.Contacts = ko.observableArray();

    var checkedContactValues = fnGetCheckedValues();
    self.notselectall = ko.observable(notselectall);
    self.SelectAll = ko.observable(notselectall == false?true:false);

    ko.validation.configure({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null
    });


    if (checkedContactValues.length > 0 || localStorage.getItem("contactdetails") != null) {
        self.Contacts(selectedContacts(0, checkedContactValues));
    }

    self.OwnerId = ko.observable();

    self.ownerValidation = self.OwnerId.extend({
        validation: {
            validator: function (val) {
                if (self.OwnerId() == undefined) {
                    return false;
                }
                else { return true; }
            },
            message: '[|Please select owner|]'
        }
    });
    self.errors = ko.validation.group(self, true);
    //  self.ownerValidation = self.Contacts.extend({ minimumLength: 1 });
    self.changeowner = function () {
        if (checkedContactValues.length == 0 && self.SelectAll() == true)
        {
            notifyError("Select at least one Contact");
        }
        else
        {
            if (self.notselectall() == false) {
                self.SelectAll = ko.observable(true);
            }
            self.errors.showAllMessages();
            if (self.errors().length > 0)
                return;
            if (self.Contacts().length == 0 && self.OwnerId != null && self.SelectAll() == false) {
                notifyError("[|Please select at least one contact|]");
                return;
            }
            innerLoader('changeowner');
            var jsondata = ko.toJSON(self);
            $.ajax({
                url: url + "ChangeOwner",
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'changeOwnerViewModel': jsondata })
            }).then(function (response) {
                var filter = $.Deferred();
                if (response.success) {
                    filter.resolve(response);
                } else {
                    filter.reject(response.error);
                }
                return filter.promise();
            }).done(function (data) {
                $('.success-msg').remove();
                removeinnerLoader('changeowner');
                notifySuccess('[|Successfully changed the owner|]');
                setTimeout(
                   function () {
                       createCookie('log', false, 1);
                       window.location.href = document.URL;
                   }, setTimeOutTimer);
            }).fail(function (error) {
                $('.overlay').remove();
                removeinnerLoader('changeowner');
                notifyError(error);
            })
        }
             
    }
    self.Users = ko.observableArray();

    Users = ko.utils.arrayFilter(Users, function (usr) {
        return usr.IsDeleted == false;
    });

    self.Users(Users);

}