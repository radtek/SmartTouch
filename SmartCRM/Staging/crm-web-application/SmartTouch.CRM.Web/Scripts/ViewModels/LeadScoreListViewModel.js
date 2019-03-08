var leadScoreListViewModel = function (data, url) {    

    selfLeadScoreList = this;

    selfLeadScoreList.ChangeScore = function (data, event) {
        var menuItemId = $(event.target).attr('data-openid');
        checkedvalues = fnGetChkvalGrid('chkleadscore');
        if (checkedvalues.length == 0 || checkedvalues.length > 1) {
            notifyError("[|Please select only one condition from the grid.|]");
            return;
        }

        $("#menuPartialItemContent" + menuItemId).html('');
        OpenTopInner("menuItem" + menuItemId, "menuPartialItemContent" + menuItemId);
        
        $.ajax({
            url: "changescore",
            type: 'get',
            contentType: "application/html; charset=utf-8",
            success: function (editScoreView) {
                console.log("editScoreView");
                $("#menuPartialItemContent" + menuItemId).html(editScoreView);
                checkedvaluesname = fnGetChkvalName('chkleadscore');
                loadRule(checkedvalues);
            },
            error: function (data) {
              
            }
        });
    };
    var loadRule = function (ruleId) {
        
        $.ajax({
            url: "getleadscore?leadScoreId=" + ruleId,
            type: 'post',          
            contentType: "application/json; charset=utf-8",
            success: function (rule) {
              
                console.log(rule);
                var viewModel = new changeScoreViewModel(rule, url);
                console.log(viewModel);
                ko.applyBindings(viewModel, document.getElementById('changescore1'));
            },
            error: function (data) {
               
            }
        });
    }
}

var changeScoreViewModel = function (data,url) {
    var selfChangeScore = this;

    ko.mapping.fromJS(data, {}, selfChangeScore);

    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfChangeScore));

    selfChangeScore.SelectedRule = ko.observable();
    selfChangeScore.AppliedToPreviousActions = ko.observable(data.AppliedToPreviousActions);

    selfChangeScore.setAppliesToPrevious = function (data, event) {
        selfChangeScore.AppliedToPreviousActions(event.target.checked);
    }

    selfChangeScore.Score = ko.observable(data.Score).extend({ required: { message: "[|Points is required|]" }, pattern: { params: '^0*[1-9][0-9]*$', message: "[|Points must be positive number|]" } });

    selfLeadScoreList.errors = ko.validation.group(selfLeadScoreList, true);

    selfChangeScore.errors = ko.validation.group(selfChangeScore);

    selfChangeScore.changeLeadScore = function () {
        if (selfChangeScore.LeadScoreID != 0) {
            selfChangeScore.errors.showAllMessages();
            if (selfChangeScore.errors().length > 0)
                return;
            var jsondata = ko.toJSON(selfChangeScore);
            $('body').append('<div class="overlay"><div class="loadmessage">[|Please wait while saving|]...<div class="mtl text-center"><figure><img src="../../img/loader.gif"  alt"Loading" /></figure></div></div><div class="modal-backdrop fade in"></div></div> ');
            $.ajax({
                url: url + "ChangeLeadScore",
                type: "post",
                data: JSON.stringify({ "leadScoreRoleViewModel": jsondata }),
                dataType: 'json',                
                contentType: "application/json; charset=utf-8"
            }).then(function (response) {            
                var filter = $.Deferred()            
                if (response.success) {                
                filter.resolve(response)            
                } else {                
                filter.reject(response.error)            
                }            
                return filter.promise()        
            }).done(function (data) {
                $('.success-msg').remove();
                $('.overlay').remove();
                notifySuccess('Successfully changed the score');
                setTimeout(function () { window.location.href = "leadscorerules" }, setTimeOutTimer);
            }).fail(function (error) {                           
                notifyError(error);
            })            
        }
    };
}