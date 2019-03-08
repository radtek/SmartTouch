var apiViewModel = function (data) {
    selfAPI = this;
    selfAPI.Methods = ko.observableArray(data);
    selfAPI.ExposedMethods = [
        { Key: "Overview", DisplayName: "Overview", View: "Overview", SortID: 0 },
        { Key: "QuickReference", DisplayName: "Quick Reference", View: "Quick Reference", SortID: 0 },
        { Key: "POSTlogin", DisplayName: "Get API Key", View: "GetAPIKey", SortID: 0 },
        { Key: "GETapi/Accounts?accountName={accountName}", DisplayName: "Get Account", View: "GetAccountID", SortID: 1 },
        { Key: "GETDropDownValueFields?accountId={accountId}", DisplayName: "Get Dropdown Fields", View: "DropdownsByAccount", SortID: 2 },
        { Key: "GETapi/DropdownValues?dropdownId={dropdownId}", DisplayName: "Get Dropdown By ID", View: "DropdownByID", SortID: 3 },
        { Key: "POSTPerson", DisplayName: "Insert Person", View: "InsertPerson", SortID: 4 },
    ]

    selfAPI.CurrentModal = ko.observable(new dataModal());

    selfAPI.ShowMethods = ko.observableArray(ko.utils.arrayFilter(selfAPI.Methods(), function (method, index) {
        var isExposed = selfAPI.ExposedMethods.filter(function (e) { return e.Key == method.ID });
        if (isExposed && isExposed.length > 0) {
            selfAPI.Methods()[index].DisplayName = isExposed[0].DisplayName;
            selfAPI.Methods()[index].View = isExposed[0].View;
            selfAPI.Methods()[index].SortID = isExposed[0].SortID;
            return true;
        }
        else
            return false;
    })
    );

    selfAPI.ExcludedProperties = [
        {
            MethodName: "POSTPerson", Properties: [
              'SSN',
              'FullName',
              'CreatedBy',
              'PreviousLifecycleStage',
              'OwnerName',
              'ContactID',
              'LeadScore',
              'RelationshipViewModel',
              'Actions',
              'Opportunities',
              'Tours',
              'AddressList',
              'Address',
              'PartnerTypes',
              'LifecycleStages',
              'AddressTypes',
              'PhoneTypes',
              'PhoneList',
              'EmailList',
              'SocialMediaUrls',
              'LeadSources',
              'Communities',
              'SecondaryEmails',
              'LifeCycle',
              'DropdownValueTypeId',
              'LastContacted',
              'LastContactedThrough',
              'CreatedOn',
              'LastUpdatedBy',
              'LastUpdatedOn',
              'LastTouchedDate',
              'LastTouchedType',
              'Notes',
              'LastContactedString',
              'Image',
              'CustomFieldTabs',
              'ReferenceId',
              'DateFormat',
              'ContactSource',
              'SourceType',
              'FirstSourceType',
              'IsLifecycleChanged',
              'OutlookSync',
              'PopularTags',
              'RecentTags',
              'LastNote',
              'LastNoteDate',
              'ContactSummary',
               "PartnerType",
               "DoNotEmail",
               "OwnerId",
               "CompanyID",
               "PreviousOwnerId",
               "ProfileImageKey",
               "LeadScore",
               "Name",
               "FirstContactSource",
               "ContactType",
               "TagsList"
            ]
        }
   ]

}

var dataModal = function () {
    var modal = this;
    modal.MethodDescription = ko.observable();
    modal.Script = ko.observable();
    modal.Parameters = ko.observableArray([]);

    modal.getScript = function (method) {
        switch (method) {
            case "POST-login":
                modal.Script(new selfAPIKey());
                break;

                //case "DropdownByID":
                //    return DropdownByID;
                //    break;

                //case "DropdownsByAccount":
                //    return DropdownsByAccount;
                //    break;
        }

    }
}

var selfAPIKey = function () {
    var selfAPIKey = this;
    selfAPIKey.showForm = function () {
        $('#test-api').show();
        $('#testClientButton').hide();
    }

    selfAPIKey.closeForm = function () {
        $('#test-api').hide();
        $('#testClientButton').show();
        $('#api-response').hide();
    }

    selfAPIKey.getAPIKey = function () {
        $('#api-response').hide();
        $.ajax({
            url: location.origin + "/Login",
            type: 'post',
            data: {
                "UserName": $('#username').val(),
                "Password": $('#password').val(),
                "ApiKey": $('#apikey').val()
            },
            success: function (data) {
                $('#api-response').show();
                $('#api-response pre').text(JSON.stringify(data));
            },
            error: function (data) {
                alert(data.responseText);
            }
        });
    }
    return selfAPIKey;
}

function pageLoader() {
    $('body').append('<div class="pageloader-mask"><span class="pageloader-text">Loading...</span><div class="pageloader-image"></div><div class="pageloader-color"></div></div>');
}

function removepageloader() {
    $('.pageloader-mask').remove('');
}

