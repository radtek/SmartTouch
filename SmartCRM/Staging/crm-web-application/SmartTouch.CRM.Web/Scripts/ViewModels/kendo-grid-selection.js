var checkedContactValues = "";
function fnGetCheckedValues() {
    checkedContactValues = $('.chkcontacts:checked').map(function () {
        return ($(this).attr('id') + "|" + $(this).attr('data-name') + "|" + $(this).attr('data-company'));
    }).get();

    return checkedContactValues;
}


var checkedvalues = "";
function fnGetChkvalGrid(checkboxId) {
    checkedvalues = $('.' + checkboxId + ':checked').map(function () {
        return $(this).attr('id');
    }).get();

    return checkedvalues;
}


var checkedvaluesname = "";
function fnGetChkvalName(checkboxId) {
    checkedvaluesname = $('.' + checkboxId + ':checked').map(function () {
        return $(this).attr('data-name');
    }).get();

    return checkedvaluesname;
}

var checkedvaluesstatus = "";
function fnGetChkvalStatus(checkboxId) {
    checkedvaluesstatus = $('.' + checkboxId + ':checked').map(function () {
        return $(this).attr('data-status');
    }).get();

    return checkedvaluesstatus;
}