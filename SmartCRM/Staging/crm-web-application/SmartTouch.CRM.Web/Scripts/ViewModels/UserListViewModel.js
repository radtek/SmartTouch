var userlistViewModel = function (data) {
    var selfUserList = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfUserList));
    
   
    
}

