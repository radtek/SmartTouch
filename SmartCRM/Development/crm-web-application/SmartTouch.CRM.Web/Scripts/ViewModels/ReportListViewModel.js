var reportListViewModel = function (data, BASE_URL)
{
    var selfReport = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfReport));
   

}