var sendTextViewModel = function (data, url) {
    var self = this;




    ko.mapping.fromJS(data, {}, self);
    self.sendTextValue = ko.observable('Send');
    self.from = ko.observable((data.From == null?"":data.From).replace( /\s/g, "").replace(/[^\w\s]/gi, ''));
    //self.fromValidation = self.from.extend({ required: { message: "[|From number required|]" }, digit: true, minLength: 10 });
    //  self.to = ko.observable(data.to).extend({ required: { message: "To number required" }, digit: true, minLength: 10 })
    self.To = ko.observable(data.To);
    //self.body = ko.observable(data.body).extend({ required: { message: "Please enter message" }, maxLength: 160 })
    self.body = ko.observable(data.Body);
    self.bodyValidation = self.body.extend({ maxLength: 137 });
    self.Contacts = ko.observableArray(data.Contacts);
    //self.contactsValidation = self.To.extend({ minimumLength: 1 });
    
    if (checkedContactPhoneNumbers.length > 0 || localStorage.getItem("contactdetails") != null)
        self.Contacts(selectedContactPhoneNumbers(checkedContactPhoneNumbers, data.Contacts));


    self.contactsValidation = self.To.extend({ required: { message: "[|Please select To|]" } });

    var fromPhones = [];
    $.each(data.FromPhones, function (i, obj) {
        fromPhones.push(obj == null?"":obj.replace( /\s/g, "").replace(/[^\w\s]/gi, ''));
    });

    self.fromPhoneNumbers = ko.observableArray(fromPhones);
  
    //self.getServiceproviders = function () {

    //}
    //$.ajax({
    //    url: url + 'GetCategories',
    //    type: 'get',
    //    dataType: 'json',
    //    contentType: "application/json; charset=utf-8",
    //    success: function (categories) {
    //        selfLeadScore.Categories(categories);
    //    }
    //});



    self.errors = ko.validation.group([self.fromValidation, self.contactsValidation, self.bodyValidation]);
    self.sendText = function () {
        self.errors.showAllMessages();
        var Tofield = "";
        $.each(self.Contacts(), function (index, contact) {            
            if (contact.Phone != "") {
                if (parseInt(index) == 0)
                    Tofield = contact.Phone;
                if (parseInt(index) > 0)
                    Tofield = Tofield + ";" + contact.Phone;                
            }

        });
        self.To(Tofield);

        if (self.body() == null || self.body() == "") {
            notifyError("Please enter message");
            return;
        }        
        if (self.errors().length > 0)
            return;
        var jsondata = ko.toJSON(self);

        var action = "SaveRelation";
        //self.saveText('Saving..');
        innerLoader('divText');

        $.ajax({
            url: "/Communication/SendText",
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'sendTextViewModel': jsondata })
        }).then(function (response) {
            $('.success-msg').remove();
            removeinnerLoader('divText');
            var filter = $.Deferred()            
            if (response.success) {                
                filter.resolve(response)            
            } else {                
                filter.reject(response.error)            
            }            
            return filter.promise()        
        }).done(function (data) {
            notifySuccess(data.response);
            createCookie('log', false, 1);
            window.location.href = document.URL;
        }).fail(function (error) {            
            notifyError(error);
        });
    }
}
checkedContactPhoneNumbers = fnGetCheckedContactPhones();
function CheckValidation() {
    ko.applyBindings("", document.getElementById("divText"));
}

//ko.bindingHandlers.numeric = {
//    init: function (element, valueAccessor) {
//        $(element).on("keydown", function (event) {
//            if (event.keyCode == 46 || event.keyCode == 8 || event.keyCode == 9 || event.keyCode == 27 || event.keyCode == 13 ||
//                (event.keyCode == 65 && event.ctrlKey === true) ||
//                (event.keyCode == 188 || event.keyCode == 190 || event.keyCode == 110) ||
//                (event.keyCode >= 35 && event.keyCode <= 39)) {
//                return;
//            }
//            else {
//                if (event.shiftKey || (event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105)) {
//                    event.preventDefault();
//                }
//            }
//        });
//    }
//};


