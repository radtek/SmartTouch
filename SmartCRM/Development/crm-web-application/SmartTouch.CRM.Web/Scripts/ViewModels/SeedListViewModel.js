var seedListViewModel = function (data, url, accountID) {

    var selfSeedList = this;
    var SeedListArray = [];
    var DuplicateEmails = [];
    var emailsList = ko.utils.arrayMap(data, function (email) {
        return email.Email;
    });

    var x = [emailsList.join("\n")];

    selfSeedList.Email = ko.observableArray(x);

    selfSeedList.errors = ko.validation.group(selfSeedList);

    selfSeedList.saveSeedList = function () {

        alertifyReset();
        alertify.confirm("[|Are you sure you want to replace the old list with new list?|]", function (e) {
            if (e) {
                var Emails = [selfSeedList.Email()];
                if (Emails != "") {
                    var EmailList = Emails.join("\n").split("\n");
                    for (var i = 0; i < EmailList.length ; i++) {
                        var a = EmailList[i];
                        var ValidEmail = validateEmail(a);
                        if (DuplicateEmails.indexOf(a) == -1 && ValidEmail) {
                            DuplicateEmails.push(a);
                            var SeedListObj = { Email: a }
                            SeedListArray.push(SeedListObj);
                        }
                    }

                    DuplicateEmails = null;

                    if (SeedListArray.length > 0) {
                        pageLoader();
                        $.ajax({
                            url: url + "/InsertSeedList",
                            type: 'post',
                            dataType: 'json',
                            data: JSON.stringify({ "SeedEmail": ko.toJSON(SeedListArray) }),
                            contentType: "application/json; charset=utf-8",
                            success: function (data) {
                                notifySuccess("[| Successfully Inserted |]");
                                setTimeout(
                                          function () {
                                              removepageloader();
                                              window.location.href = "/seedlist";
                                          }, setTimeOutTimer);
                            },
                            error: function (data) {
                                notifyError("[| Failed to insert List |]");
                            }
                        })
                    }
                    else {
                        notifyError("[| Please Enter Valid Emails |]");
                    }
                }
                else {
                    notifyError("[| Textarea should not be Empty |]");
                }
            }
        })
    }

    function validateEmail(email) {
        var re = /^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$/i;
        return re.test(email);
    }

    selfSeedList.Cancel = function () {
        alertifyReset();
        alertify.confirm("[|Are you sure you want to exit without inserting the list?|]", function (e) {
            if (e) {
                window.location.href = "/accounts";
            }
        });    
    };
}