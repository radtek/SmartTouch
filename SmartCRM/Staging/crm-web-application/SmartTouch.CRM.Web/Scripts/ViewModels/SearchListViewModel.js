var searchListViewModel = function (data, url, WEBSERVICE_URL) {
    var selfListSearch = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfListSearch));
    selfListSearch.editSearch = function () {
        var selectedSearches = fnGetSelectedSearches('chkadvancedsearch');
        var favorite_SelectedSearches = fnGetSelectedSearches('chkadvancedsearch2');
        var total_Checkedsearches = (selectedSearches.length + favorite_SelectedSearches.length);
        if (selectedSearches.length == 0 || selectedSearches.length > 1) {
            notifyError("[|Please select only one Search.|]");
        } else {

            window.location.href = selectedSearches.length != 0 ? "/editsearch?SearchDefinitionID=" + selectedSearches[0].searchDefinitionID : "/editsearch?SearchDefinitionID=" + favorite_SelectedSearches[0].searchDefinitionID
        }
    };

    selfListSearch.runSearch = function () {
        var favorite_SelectedSearches = fnGetSelectedSearches('chkadvancedsearch2');
        var selectedSearches = fnGetSelectedSearches('chkadvancedsearch');

        var total_Checkedsearches = (selectedSearches + favorite_SelectedSearches.length);
        if (selectedSearches.length == 0 || selectedSearches.length > 1)
            notifyError("[|Please select only one Search.|]");
        else
            window.location.href = favorite_SelectedSearches.length != 0 ? "../advancedview/searchdefinitionId=" + favorite_SelectedSearches[0].searchDefinitionID :
                "../advancedview/searchdefinitionId=" + selectedSearches[0].searchDefinitionID;
    };

    selfListSearch.viewSearch = function () {
        var selectedSearches = fnGetSelectedSearches('chkadvancedsearch');

        if (selectedSearches.length == 0 || selectedSearches.length > 1)
            notifyError("[|Please select only one Search.|]");
        else
            window.location.href = "/viewsearch?SearchDefinitionID=" + selectedSearches[0].searchDefinitionID
    };

    selfListSearch.copySearch = function () {
        var selectedSearches = fnGetSelectedSearches('chkadvancedsearch');
        var favorite_SelectedSearches = fnGetSelectedSearches('chkadvancedsearch2');
        var total_Checkedsearches = (selectedSearches.length + favorite_SelectedSearches.length);
        if (selectedSearches.length > 1 || selectedSearches.length == 0) {
            notifyError("[|Please select only one Search.|]");
        } else {
            window.location.href = selectedSearches.length != 0 ? "/copysearch?SearchDefinitionID=" + selectedSearches[0].searchDefinitionID : "/copysearch?SearchDefinitionID=" + favorite_SelectedSearches[0].searchDefinitionID;
        }
    };

    var selectedSearches = [];
    function fnGetSelectedSearches_Delete_Function(checkboxClass) {
        selectedSearches = $('.' + checkboxClass + ':checked').map(function () {
            return $(this).attr('SearchDefinitionID')
        }).get();
        return selectedSearches;
    };

    selfListSearch.deleteSearch = function () {
        var selectedSearches = fnGetSelectedSearches_Delete_Function('chkadvancedsearch');
        var favorite_SelectedSearches = fnGetSelectedSearches_Delete_Function('chkadvancedsearch2');

        $.each(favorite_SelectedSearches, function (index, value) {
            selectedSearches.push(value);
        });

        if (selectedSearches.length == 0) {
            notifyError("[|Please select at least one Search.|]");
        } else {
            alertifyReset("Delete Search(s)", "Cancel");
            var message = "";
            if (selectedSearches.length == 1) {
                message = "[|You’re about to delete 1 Search. Are you sure you want to delete 1 search?|]";
            } else {
                message = "[|You’re about to delete|] " + selectedSearches.length + " [|Search(es)|] . [|Are you sure you want to delete|] " + selectedSearches.length + " [|Search(es)?|]";
            }
            alertify.confirm(message, function (e) {
                if (e) {
                    var searchIDs = [];
                    searchIDs = selectedSearches;
                    console.log(searchIDs);
                    var varDeleteURL = url + "DeleteSearches";

                    var array = [];
                    $.each(selectedSearches, function (index, value) {
                        array.push(value);
                    });
                    jQuery.ajaxSettings.traditional = true
                    $.ajax({
                        url: varDeleteURL,
                        type: 'post',
                        dataType: 'json',
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify({ SearchID: array })
                    }).then(function (response) {
                        var filter = $.Deferred();
                        if (response.success) {
                            filter.resolve(response);
                        }
                        else {
                            filter.reject(response.error);
                        }
                        return filter.promise();
                    }).done(function (data) {

                        notifySuccess("[|Successfully deleted the  Search(es).|]");
                        setTimeout(
                            function () {
                                window.location.href = "/advancedsearchlist";
                            }, setTimeOutTimer);
                    }).fail(function (error) {
                        notifyError(error);
                    });
                }
                else {
                    notifyError("[|You've clicked Cancel.|]");
                }
            });
        }
    };
}

