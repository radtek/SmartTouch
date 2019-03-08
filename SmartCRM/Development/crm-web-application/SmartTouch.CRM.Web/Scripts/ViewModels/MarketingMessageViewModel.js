var MarketingMessagesViewModel = function (data, BASE_URL, accountTypes) {
    selfMarketingMessage = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfMarketingMessage));
    selfMarketingMessage.AccountTypes = ko.observableArray(accountTypes);
    selfMarketingMessage.MarketingMessageTitle = ko.observable(data.MarketingMessageTitle).extend({ required: { message: "[|Notification  title is required|]" }, maxLength: 75 });
    selfMarketingMessage.SelectedBy = ko.observable(data.SelectedBy).extend({ required: { message: "[|Seleted by  is required|]" } });
    selfMarketingMessage.AccountIDs =  ko.observableArray(data.AccountIDs);
    selfMarketingMessage.TimeInterval = data.TimeInterval == 0 ? ko.observable(2) : ko.observable(data.TimeInterval);
    selfMarketingMessage.Messages = ko.observableArray();
    selfMarketingMessage.Status = ko.observable(data.Status);
    selfMarketingMessage.ScheduleFrom = ko.observable(data.ScheduleFrom);
    selfMarketingMessage.ScheduleTo = ko.observable(data.ScheduleTo);
    selfMarketingMessage.MessagesCount = ko.observable(0);
    var now = new Date().toUtzDate();
    selfMarketingMessage.utc_now = ko.observable(new Date(now.getTime() - 1000 * 60));

    if (data.ScheduleFrom) {
        var utzdate = ConvertToDate(selfMarketingMessage.ScheduleFrom()).toUtzDate();
        selfMarketingMessage.ScheduleFrom(utzdate);
    }

    if (data.ScheduleTo) {
        var utzdate = ConvertToDate(selfMarketingMessage.ScheduleTo()).toUtzDate();
        selfMarketingMessage.ScheduleTo(utzdate);
        if (selfMarketingMessage.ScheduleTo().getTime() < selfMarketingMessage.utc_now().getTime()) {
            selfMarketingMessage.ScheduleFrom(null);
            selfMarketingMessage.ScheduleTo(null);
        }
    }

   


    selfMarketingMessage.SelectedAccountIDs = ko.observableArray(data.AccountIDs);

    selfMarketingMessage.GetDateFormat = function () {
        return readCookie("dateformat" + " hh:mm tt");
    }
    
    // selfMarketingMessage.iconcs = ko.observable('<i class="icon st-icon-add st-ad-addicon"></i>');
    if (data.Messages != null) {
        selfMarketingMessage.MessagesCount(data.Messages.length);
        $.grep(data.Messages, function (value, index) {
            selfMarketingMessage.Messages.push(new MarketingMessageContentviewModel(value, selfMarketingMessage.MessagesCount()))
        })
    }

    var intialCount = selfMarketingMessage.MessagesCount() != 0?selfMarketingMessage.MessagesCount():0;
    selfMarketingMessage.AddMessage = function (e) {
        intialCount = intialCount + 1;
        selfMarketingMessage.MessagesCount(intialCount);
        selfMarketingMessage.Messages.push(new MarketingMessageContentviewModel(undefined, selfMarketingMessage.MessagesCount()));
    }
  
    
    if (selfMarketingMessage.Messages().length == 0) {
        selfMarketingMessage.AddMessage()
    }
    selfMarketingMessage.TimeIntervals = [{ IntervalId: 1, Name: "1 [|Sec|]" },
        { IntervalId: 2, Name: "2 [|Sec|]" },
        { IntervalId: 3, Name: "3 [|Sec|]" },
        { IntervalId: 4, Name: "4 [|Sec|]" },
        { IntervalId: 5, Name: "5 [|Sec|]" },
        { IntervalId: 6, Name: "6 [|Sec|]" },
        { IntervalId: 7, Name: "7 [|Sec|]" },
        { IntervalId: 8, Name: "8 [|Sec|]" },
        { IntervalId: 9, Name: "9 [|Sec|]" },
        { IntervalId: 10, Name: "10 [|Sec|]" },
        { IntervalId: 15, Name: "15 [|Sec|]" },
        { IntervalId: 20, Name: "20 [|Sec|]" },
        { IntervalId: 25, Name: "25 [|Sec|]" },
        { IntervalId: 30, Name: "30 [|Sec|]" }
    ];
    selfMarketingMessage.errors = ko.validation.group(selfMarketingMessage);


    selfMarketingMessage.fromDateChangeEvent = function () {
        var fromDate = this.value();
        selfMarketingMessage.ScheduleFrom(kendo.toString(fromDate, selfMarketingMessage.GetDateFormat()));
        var toDate = selfMarketingMessage.ScheduleTo();
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }

    selfMarketingMessage.toDateChangeEvent = function () {
        var fromDate = selfMarketingMessage.ScheduleFrom();
        var toDate = this.value();
        selfMarketingMessage.ScheduleTo(kendo.toString(toDate, selfMarketingMessage.GetDateFormat()));
        if (Date.parse(fromDate) > Date.parse(toDate)) {
            notifyError("[|To date should be greater than From date|]");
            return false;
        }
    }

    selfMarketingMessage.Accounts = ko.observableArray([]);


    $.ajax({
        url: 'gaa/' + selfMarketingMessage.SelectedBy(),
        type: 'get',
        dataType: 'json',
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            removeinnerLoader('loader');
            selfMarketingMessage.Accounts(data.response);
            ko.applyBindings(selfMarketingMessage);
        },
        error: function (errors) {
            console.log(errors);
        }
    });

    selfMarketingMessage.showList = function()
    {
           var id = selfMarketingMessage.SelectedBy();
            innerLoader('loader');
            $.ajax({
                url: 'gaa/' + selfMarketingMessage.SelectedBy(),
                type: 'get',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    removeinnerLoader('loader');
                    if (selfMarketingMessage.AccountIDs().length > 0)
                        selfMarketingMessage.AccountIDs([]);
                    selfMarketingMessage.Accounts(data.response);
                },
                error: function (errors) {
                    console.log(errors);
                }
            });
    }


    selfMarketingMessage.published = function () {
        var selectedAccounts = [];
        if (selfMarketingMessage.AccountIDs().length == 0) {
            for (var l = 0; l < (selfMarketingMessage.Accounts()).length; l++) {
                selectedAccounts.push((selfMarketingMessage.Accounts())[l].Id);
            }
        }
        else
            selectedAccounts = (selfMarketingMessage.AccountIDs()).length > 0 ? (selfMarketingMessage.AccountIDs()).map(Number) : [];
       
        selfMarketingMessage.SelectedAccountIDs(selectedAccounts);
        selfMarketingMessage.Status(1001);
       

        var messageError = false;
        $.each(selfMarketingMessage.Messages(), function (index, data) {
            if (data.IsDeleted() == false) {
                if (data.Icon() == undefined || data.Icon() == null || data.Icon() == "") {
                    notifyError("Icon is Required");
                    messageError = true;
                }
                else if (data.Subject() == undefined || data.Subject() == null || data.Subject() == "") {
                    notifyError("SubJect is Required");
                    messageError = true;
                }
                else if (data.Content() == undefined || data.Content() == null || data.Content() == "<br>" || data.Content() == "") {
                    notifyError("Message is Required");
                    messageError = true;
                }
            }
        });
        var messageLength = selfMarketingMessage.Messages().map(function (e) {
            return e.IsDeleted();
        }).indexOf(false);
        if (messageLength == -1) {
            notifyError("Add atleast one message");
            messageError = true;
        }


        //selfMarketingMessage.AccountIDs(selectedAccounts);

        var jsondata = ko.toJSON(selfMarketingMessage);
        var action;
        if (data.MarketingMessageID !== 0)
            action = "UpdateMarketingMessage";
        else
            action = "InsertMarketingMessage";
        if (messageError == false)
        {
            if (selfMarketingMessage.errors().length == 0) {                
              //  console.log(selfMarketingMessage.AccountIDs());
                pageLoader();
                $.ajax({
                    url: BASE_URL + action,
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ 'marketingmessage': jsondata })
                }).then(function (response) {
                    var filter = $.Deferred();
                    if (response.success) {
                        filter.resolve(response);
                    }
                    else {
                        filter.reject(response.error);
                    }
                    return filter.promise();
                }).done(function () {
                    $('.success-msg').remove();
                    notifySuccess('Successfully Published the marketing message');
                    setTimeout(function () {
                        removepageloader();
                        window.location.href = "/MarketingMessages";
                    }, setTimeOutTimer);
                }).fail(function (error) {
                    $('.success-msg').remove();
                    removepageloader();
                    notifyError(error);
                });
            }
            else {
                selfMarketingMessage.errors.showAllMessages();
            }
        }
        else
        {
            selfMarketingMessage.errors.showAllMessages();
        }
        
    }

    selfMarketingMessage.PreviewHtmlCode = function () {
        if (selfMarketingMessage.Messages().length > 0) {
            $("#messagepreview").modal('toggle');
            var value = selfMarketingMessage.TimeInterval();
            console.log(value);
            $('.carousel').carousel({
                interval: value * 1000
            });
        }
        else
        {
            notifyError("Add atleast one message to view Preview.");
        }
    }
    
    selfMarketingMessage.saveMessage = function () {
        selfMarketingMessage.Status(1000);

        var selectedAccounts = [];

        if (selfMarketingMessage.AccountIDs().length == 0) {
            for (var l = 0; l < (selfMarketingMessage.Accounts()).length; l++) {
                selectedAccounts.push((selfMarketingMessage.Accounts())[l].Id);
            }
        }
        else
            selectedAccounts = (selfMarketingMessage.AccountIDs()).length > 0 ? (selfMarketingMessage.AccountIDs()).map(Number) : [];
       
        selfMarketingMessage.SelectedAccountIDs(selectedAccounts);
        var messageError = false;
        $.each(selfMarketingMessage.Messages(), function (index, data) {
            if (data.IsDeleted() == false) {
                if (data.Icon() == undefined || data.Icon() == null || data.Icon() == "")
                {
                    notifyError("Icon is Required");
                    messageError = true;
                }
                else if (data.Subject() == undefined || data.Subject() == null || data.Subject() == "")
                {
                    notifyError("SubJect is Required");
                    messageError = true;
                }
                else if (data.Content() == undefined || data.Content() == null || data.Content() == "<br>" || data.Content() == "")
                {
                    notifyError("Message is Required");
                    messageError = true;
                }
                   
            }
        });

        var  messageLength = selfMarketingMessage.Messages().map(function (e) {
        return e.IsDeleted();
        }).indexOf(false);
        if (messageLength == -1)
        {
            notifyError("Add atleast one message");
            messageError = true;
        }
          
        var jsondata = ko.toJSON(selfMarketingMessage);
        var action;
        if (data.MarketingMessageID !== 0) {
            action = "UpdateMarketingMessage";
        }
        else
        {
            action = "InsertMarketingMessage";
        }
        
        if (messageError == false)
        {

            if (selfMarketingMessage.errors().length == 0) {
                pageLoader();
              //  selfMarketingMessage.AccountIDs(selectedAccounts);
                $.ajax({
                    url: BASE_URL + action,
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ 'marketingmessage': jsondata })
                }).then(function (response) {
                    var filter = $.Deferred();
                    if (response.success) {
                        filter.resolve(response);
                    }
                    else {
                        filter.reject(response.error);
                    }
                    return filter.promise();
                }).done(function () {
                    $('.success-msg').remove();
                    notifySuccess('Successfully Saved the marketing message');
                    setTimeout(function () {
                        removepageloader();
                        window.location.href = "/MarketingMessages";
                    }, setTimeOutTimer);
                }).fail(function (error) {
                    $('.success-msg').remove();
                    removepageloader();
                    notifyError(error);
                });

            }
            else {
                selfMarketingMessage.errors.showAllMessages();
            }
        }
        else {
            selfMarketingMessage.errors.showAllMessages();
        }
       
    }
    
}

