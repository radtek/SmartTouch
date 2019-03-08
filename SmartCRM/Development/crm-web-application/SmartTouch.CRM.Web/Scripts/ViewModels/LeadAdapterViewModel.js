var leadAdapterViewModel = function (data, webApp, service, LeadAdapterBaseURL, fbAppId) {

    selfLeadAdapter = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfLeadAdapter));
    selfLeadAdapter.LeadAdapterID = ko.observable(data.LeadAdapterAndAccountMapId);
    selfLeadAdapter.LeadSourceType = ko.observable(data.LeadSourceType);
    selfLeadAdapter.LeadSourceTypeValidation = selfLeadAdapter.LeadSourceType.extend({
        required: {
            message: '[|Lead Source is required|]'
        }
    });
    selfLeadAdapter.TagsList = ko.observableArray(data.TagsList);
    selfLeadAdapter.LeadAdapterTypes = ko.observableArray([
      { Id: 1, Type: "BDX" },
      { Id: 2, Type: "NHG" },
      { Id: 3, Type: "Hot On Homes" },
      { Id: 4, Type: "PROPLeads" },
      { Id: 5, Type: "Zillow" },
      { Id: 6, Type: "New Home Feed" },
      { Id: 7, Type: "Private Communities" },
      { Id: 8, Type: "IDX" },
      { Id: 9, Type: "Condo" },
      { Id: 10, Type: "Buzz Buzz Homes" },
      //{ Id: 12, Type: "Home Finder" },
      { Id: 13, Type: "Facebook" },
     // { Id: 14, Type: "Trulia" },
      { Id: 15, Type: "Builder's Update"}
    ]);

    selfLeadAdapter.LeadSourceDropdownValues = ko.observableArray(data.LeadSourceDropdownValues);
    selfLeadAdapter.LeadAdapterType = ko.observable(data.LeadAdapterType == 0 ? undefined : data.LeadAdapterType.toString());
    selfLeadAdapter.showport = ko.pureComputed(function () {
        if (selfLeadAdapter.LeadAdapterType() == 8 || selfLeadAdapter.LeadAdapterType() == 7 || selfLeadAdapter.LeadAdapterType() == 10 || selfLeadAdapter.LeadAdapterType() == 12 || selfLeadAdapter.LeadAdapterType() == 13)
            return false;
        else
            return true;
    });
    selfLeadAdapter.LeadAdapterTypeValidation = selfLeadAdapter.LeadAdapterType.extend({
        required: {
            message: "[|Leadadapter Type Is Required|]"
        }
    });
    selfLeadAdapter.PageAccessToken = ko.observable(data.PageAccessToken).extend({ required: { message: "[|Page Access Token is Required.|]", onlyIf: function () { return selfLeadAdapter.LeadAdapterType() == 13; } } });
    selfLeadAdapter.AddID = ko.observable(data.AddID).extend({ required: { message: "[|Add ID is Required.|]", onlyIf: function () { return selfLeadAdapter.LeadAdapterType() == 13; } } });
    selfLeadAdapter.PageID = ko.observable(data.PageID);//.extend({ required: { message: "[|Page ID is Required.|]", onlyIf: function () { return selfLeadAdapter.LeadAdapterType() == 13; } } });
    selfLeadAdapter.FacebookLeadAdapterID = ko.observable(data.FacebookLeadAdapterID);
    selfLeadAdapter.FacebookLeadAdapterName = ko.observable(data.FacebookLeadAdapterName).extend({ required: { message: "[|Facebook Lead Adapter Name is Required.|]", onlyIf: function () { return selfLeadAdapter.LeadAdapterType() == 13; } } });
    selfLeadAdapter.BuilderNumber = ko.observable(data.BuilderNumber).extend({ required: { message: "[|Builder Number Required.|]", onlyIf: function () { return selfLeadAdapter.LeadAdapterType() != 13; } } });

    selfLeadAdapter.CommunityNumber = ko.observable(data.CommunityNumber);

    selfLeadAdapter.Url = ko.observable(data.Url).extend({
        required: { message: "[|Host Details Required.|]", onlyIf: function () { return selfLeadAdapter.LeadAdapterType() != 13; } },
        validation: [
            {
                validator: function (val) {
                    var regex = "";
                    var regex2 = "";
                    if (selfLeadAdapter.LeadAdapterType() == 8 || selfLeadAdapter.LeadAdapterType() == 7 || selfLeadAdapter.LeadAdapterType() == 10 || selfLeadAdapter.LeadAdapterType() == 12) {
                        regex = new RegExp(/^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$/);
                        return regex.test(val);
                    }
                    else if (selfLeadAdapter.LeadAdapterType() == 13)
                        return true;
                    else {
                        regex = new RegExp(/^(ftp:\/\/)?\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\/?([a-z A-Z 0-9 /]*?)$/);
                        regex2 = new RegExp(/^(ftp:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$/);
                        var regtest2 = regex2.test(val);
                        var regtest1 = regex.test(val);
                        if (regtest2 || regtest1)
                            return true;
                        else
                            return false;
                    }
                },
                message: function () {
                    if (selfLeadAdapter.LeadAdapterType() == 8 || selfLeadAdapter.LeadAdapterType() == 7 || selfLeadAdapter.LeadAdapterType() == 10 || selfLeadAdapter.LeadAdapterType() == 12)
                        return "[|Invalid Webservice URL|]"
                    else
                        return "[|Invalid FTP URL|]"
                }
            }
        ]
    });

    selfLeadAdapter.UserName = ko.observable(data.UserName).extend({ required: { message: "[|Username Required.|]", onlyIf: function () { return selfLeadAdapter.LeadAdapterType() != 12 && selfLeadAdapter.LeadAdapterType() != 13; } } });
    selfLeadAdapter.Password = ko.observable(data.Password).extend({ required: { message: "[|Password Required.|]", onlyIf: function () { return selfLeadAdapter.LeadAdapterType() != 12 && selfLeadAdapter.LeadAdapterType() != 13; } } });
    selfLeadAdapter.Port = ko.observable(data.Port).extend({
        digit: {
            required: true,
            message: "[|Enter Valid Port Number|]"
        }, max: 99999
    });

    selfLeadAdapter.LeadAdapterCommunicationType = ko.observableArray([
      { Id: 1, Name: "FTP" },
      { Id: 2, Name: "WebService" }
    ]);
    selfLeadAdapter.AccountID = ko.observable(data.AccountID);

    selfLeadAdapter.saveText = ko.observable('[|Save|]');
    selfLeadAdapter.Status = ko.observable(true);

    selfLeadAdapter.errors = ko.validation.group(selfLeadAdapter);
    selfLeadAdapter.saveLeadAdapter = function () {
        selfLeadAdapter.errors.showAllMessages();
        if (selfLeadAdapter.errors().length > 0)
            return;
        if (selfLeadAdapter.LeadAdapterType() == 13 && !selfLeadAdapter.UserAccessToken()) {
            notifyError("Please Login and select the Page to update the Lead Adapter");
            return;
        }
        var jsondata = ko.toJSON(selfLeadAdapter);
        selfLeadAdapter.saveText('[|Saving|]..');

        var action;
        if (selfLeadAdapter.LeadAdapterAndAccountMapId() != null && selfLeadAdapter.LeadAdapterAndAccountMapId() > 0)
            action = LeadAdapterBaseURL + "UpdateLeadAdapter";
        else
            action = LeadAdapterBaseURL + "InsertLeadAdapter";
        pageLoader();
        $.ajax({
            url: action,
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'leadAdapterViewModel': jsondata })
        }).then(function (data) {
            console.log("data");
            console.log(data);
            var filter = $.Deferred();
            if (data.success)
                filter.resolve(data);
            else
                filter.reject(data.error);
            return filter.promise();
        }).done(function (data) {

            selfLeadAdapter.saveText('[|Save|]');
            notifySuccess('[|Successfully saved the LeadAdapter|]');
            setTimeout(
                function () {
                    removepageloader();
                    window.location.href = "/leadadapters";
                }, setTimeOutTimer);
        }).fail(function (err) {
            removepageloader();
            selfLeadAdapter.saveText('[|Save|]');
            notifyError(err);
        });
    }

    selfLeadAdapter.leadAdapterTypeChange = function (address, state) {

    };

    selfLeadAdapter.Pages = ko.observable([]);
    selfLeadAdapter.UserAccessToken = ko.observable();

    selfLeadAdapter.subscribeApp = function (page_id, page_access_token) {
        console.log("subscribing app to page!");
        FB.api('/' + page_id + '/subscribed_apps', 'post', { access_token: page_access_token }, function (response) {
            console.log("Successfully subscribed page", response);
            if (response.error)
                notifyError(response.error.message);
        });
    };

    selfLeadAdapter.myFacebookLogin = function () {
        if (fbAppId) {
            if (fbAppId != "" && parseInt(fbAppId) > 0) {
                FB.login(function (loginResponse) {
                    console.log('successfully logged in', loginResponse);
                    FB.api('me/accounts', function (response) {
                        console.log("successfully retrieved pages : ", response);
                        if (response.data) {
                            selfLeadAdapter.UserAccessToken(loginResponse.authResponse.accessToken);
                            $.each(response.data, function (i, d) {
                                d.IsSelected = false;
                            });
                            selfLeadAdapter.Pages(response.data);
                            $('#names').modal('show');
                        }
                        if (response.error)
                            notifyError(response.error.message);
                    })
                }, { scope: 'manage_pages' });
            }
            else {
                notifyError("Please configure Facebook APPId in AccountSettings.");
            }
        }
        else {
            notifyError("Please configure Facebook APPId in AccountSettings.");
        }
    }

    function CreateTable(data) {
        var tbl = document.createElement('table');
        //tbl.style.width  = '100px';
        //tbl.style.border = '1px solid black';
        var thead = CreateHeader();
        tbl.appendChild(thead);
        var tbdy = document.createElement('tbody');
        for (i = 0; i < data.length; i++) {
            var tr = document.createElement('tr');
            if (data[i].name) {
                var td = document.createElement('td')
                td.appendChild(document.createTextNode(data[i].name));
                tr.appendChild(td);
            }
            tbdy.appendChild(tr);
        }
        tbl.appendChild(tbdy);
        return tbl;
    };

    function CreateHeader() {
        var thead = document.createElement('thead');
        for (i = 0; i < 1; i++) {
            var th1 = document.createElement('th');
            th1.appendChild(document.createTextNode(i == 0 ? 'Page Name' : ''));
            th1.className = "k-grid-header";
            thead.appendChild(th1);
        }
        return thead;
    };

    //selfLeadAdapter.SelectedPage = ko.observable();
    //selfLeadAdapter.saveSelectedFBPage = function (data) {
    //    if (data) {
    //        selfLeadAdapter.SelectedPage(data.id);
    //    }
    //    return true;
    //}

    selfLeadAdapter.saveFBPage = function () {

        var data = selfLeadAdapter.Pages().find(function (e) {
            return e.IsSelected;
        });
        console.log(data);
        if (data) {
            selfLeadAdapter.subscribeApp(data.id, data.access_token);
            selfLeadAdapter.PageAccessToken(data.access_token);
            selfLeadAdapter.PageID(data.id);
            selfLeadAdapter.BuilderNumber("");

            $('#names').modal('toggle');
            notifySuccess("[|Successfully subscribed page|]");
        }
        else
            notifyError("Please select atleast one Page");
    }
}