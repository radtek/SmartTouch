var formApproveLead = function (url, data) {
    formApproveModel = this;
    ko.validatedObservable(ko.mapping.fromJS(data, {}, formApproveModel));

    formApproveModel.FirstName = ko.observable();
    formApproveModel.LastName = ko.observable();
    formApproveModel.Email = ko.observable();
    formApproveModel.MobilePhone = ko.observable();
    formApproveModel.SubmittedFormDataID = ko.observable();
    formApproveModel.LeadSourceType = ko.observable();

    formApproveModel.NewFirstName = ko.observable();
    formApproveModel.NewLastName = ko.observable();
    formApproveModel.NewEmail = ko.observable();
    formApproveModel.NewMobilePhone = ko.observable();

    formApproveModel.updateFormData = function () {
        console.log("This is a function");
        pageLoader();
        formApproveModel.FirstName(formApproveModel.NewFirstName());
        formApproveModel.LastName(formApproveModel.NewLastName());
        formApproveModel.Email(formApproveModel.NewEmail());
        formApproveModel.MobilePhone(formApproveModel.NewMobilePhone());
        $.ajax({
            url: 'Form/UpdateFormData',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'queue': ko.toJSON(formApproveModel) }) //{ 'FirstName': formApproveModel.FirstName, 'LastName': formApproveModel.LastName, 'Email': formApproveModel.Email, 'MobilePhone': formApproveModel.MobilePhone, 'SubmittedFormDataID': formApproveModel.SubmittedFormDataID }
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            removepageloader();
            console.log("Successfully updated the form details");
            notifySuccess("Form data updated successfully");
            window.location.href = "/approveleads";
        }).fail(function (error) {
            notifyError(error);
            removepageloader();
        })
    };
}