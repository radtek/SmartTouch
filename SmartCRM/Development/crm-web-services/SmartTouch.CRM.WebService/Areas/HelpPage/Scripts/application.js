
function alertifyReset() {
    $("#toggleCSS").attr("href", "../themes/alertify.default.css");
    alertify.set({
        labels: {
            //ok: okText,
            //cancel: cancelText
            ok: "OK",
            cancel: "Cancel"
        },
        delay: 20000,
        buttonReverse: true,
        buttonFocus: "ok"
    });
}

function notifySuccess(message) {
    console.log(message);
    alertifyReset();
    alertify.success(message)
    return false;
}
function notifyError(message) {
    if (message.hasOwnProperty('statusText')) {
        if (message.statusText == 'abort')
            return;
    }
    alertifyReset();
    if (message == "[object Object]") {
        message = "An error occured, please contact administrator"
    }
    alertify.error(message);
}
function notifyInfo(message) {
    alertifyReset();
    alertify.log(message);
}

function notifyConfirm(message, okFn, delrecore, canFn, cancelMsg) {
    //function notifyConfirm(message) {
    alertifyReset();
    alertify.confirm(message, function (e) {
        if (e) {
            okFn(delrecore);
            return "ok";
        } else {
            canFn(cancelMsg);
            return "cancel";
        }
    });
}