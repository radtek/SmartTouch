var neverBounceReport = function (url, data) {
    neverBounce = this;

    neverBounce.exportList = function() {
        pageLoader();
        $('#neverBounce').data("kendoGrid").saveAsExcel();
        removepageloader();
    }
}