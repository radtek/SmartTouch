var SuppressionListViewModel = function (data, BASE_URL, accountId) {
    SuppresssionList = this;
    var SuppressedEmailListArray = [];
    var SuppressedDomainListArray = [];
    var InvalidEmails = [];
    var InvalidDomains = [];
    SuppresssionList.Email = ko.observableArray(data.Email);
    SuppresssionList.Domain = ko.observableArray(data.Domain);
    SuppresssionList.AccountID = ko.observable(accountId);


    SuppresssionList.saveSuppressionEmailList = function () {
        var Emails = [SuppresssionList.Email()];
        if (Emails != "") {
            var EmailList = Emails.join("\n").split("\n");
            InvalidEmails = [];
            if (EmailList.length <= 100) {
                for (var i = 0; i < EmailList.length ; i++) {
                    var a = EmailList[i];
                    var ValidEmail = validateEmail(a);
                    if (ValidEmail) {
                        var suppressedEmailListObj = { SuppressedEmailID: 0, Email: a, AccountID: SuppresssionList.AccountID() }
                        SuppressedEmailListArray.push(suppressedEmailListObj);
                    }
                    else {
                        if(a)
                            InvalidEmails.push(a);
                    }

                }
                if (SuppressedEmailListArray.length > 0 && InvalidEmails.length == 0) {
                    pageLoader();
                    $.ajax({
                        url: BASE_URL + "InsertSuppressedEmails",
                        type: 'post',
                        dataType: 'json',
                        data: JSON.stringify({ "suppressedEmails": ko.toJSON(SuppressedEmailListArray) }),
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            notifySuccess("[| Successfully Inserted Suppressed Emails |]");
                            setTimeout(
                                      function () {
                                          removepageloader();
                                          window.location.href = "/suppressionlist";
                                      }, setTimeOutTimer);
                        },
                        error: function (data) {
                            notifyError("[| Failed to insert email List |]");
                        }
                    })
                }
                else {
                    notifyError("[| Please Enter Valid Emails |]");
                }
            }
            else {
                notifyError("[|Do not add more than 100 emails |]");
            }

        }
        else {
            notifyError("[| Textarea should not be Empty |]");
        }
    }

    SuppresssionList.emailCancel = function () {
        window.location.href = "/suppressionlist";
    }

    SuppresssionList.saveSuppressionDomainList = function () {
        var domains = [SuppresssionList.Domain()];
        if (domains != "") {
            var domainList = domains.join("\n").split("\n");
            InvalidDomains = [];
            if (domainList.length <= 100) {
                for (var i = 0; i < domainList.length ; i++) {
                    var d = domainList[i];
                    var ValidDomain = validateDomain(d);
                    if (ValidDomain) {
                        var suppressedDomainListObj = { SuppressedEmailID: 0, Domain: d, AccountID: SuppresssionList.AccountID() }
                        SuppressedDomainListArray.push(suppressedDomainListObj);
                    }
                    else {
                        if(d)
                            InvalidDomains.push(d);
                    }

                }
                if (SuppressedDomainListArray.length > 0 && InvalidDomains.length == 0) {
                    pageLoader();
                    $.ajax({
                        url: BASE_URL + "InsertSuppressedDomains",
                        type: 'post',
                        dataType: 'json',
                        data: JSON.stringify({ "suppressedDomains": ko.toJSON(SuppressedDomainListArray) }),
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            notifySuccess("[| Successfully Inserted Suppressed Domains |]");
                            setTimeout(
                                      function () {
                                          removepageloader();
                                          window.location.href = "/suppressionlist";
                                      }, 500);
                        },
                        error: function (data) {
                            notifyError("[| Failed to insert email List |]");
                        }
                    })
                }
                else {
                    notifyError("[| Please Enter Valid Domains |]");
                }

            }
            else {
                notifyError("[| Do not add more than 100 domains |]");
            }
        }
        else {
            notifyError("[| Textarea should not be Empty |]");
        }
    }

    SuppresssionList.domainCancel = function () {
        window.location.href = "/suppressionlist";
    }

    function validateEmail(email) {
        var re = /^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$/i;
        return re.test(email);
    }

    function validateDomain(domain) {
        var regex = /^[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,6}$/i;
        return regex.test(domain);
    }
}
