var EmailValidate = function (index,publicKey) {
    selfEmailValidate = this;
    contactEmailStatus = ko.observable();

    // attach jquery plugin to validate address
    selfEmailValidate.validate = function (id) {
        $('#Email_' + id).mailgun_validator({
            api_key: publicKey, 
            in_progress: validation_in_progress,
            success: validation_success,
            error: validation_error,
        });    
        return contactEmailStatus();
    }

    // while the lookup is performing
    function validation_in_progress() {
        $('#emailStatus_' + index).html("<img src='/img/loading.gif' height='16'/>");
    }

    // if email successfull validated
    function validation_success(data) {
        $('#emailStatus_' + index).html(get_suggestion_str(data['is_valid'], data['did_you_mean']));
        $('#inlineSaving').removeAttr("disabled");
        return contactEmailStatus();
       
    }

    // if email is invalid
    function validation_error(error_message) {
        contactEmailStatus(50);
        $('#emailStatus_' + index).html(error_message);
    }

    // suggest a valid email
    function get_suggestion_str(is_valid, alternate) {
        if (is_valid) {
            contactEmailStatus(51);
            var result = '<span class="mailgun-success">Address is valid.</span>';
            if (alternate) {
                contactEmailStatus(51);
                result += '<span class="mailgun-warning"> (Though did you mean <em>' + alternate + '</em>?)</span>';
            }
            return result
        } else if (alternate) {
            contactEmailStatus(53);
            return '<span class="mailgun-warning">Did you mean <em>' + alternate + '</em>?</span>';
        } else {
            contactEmailStatus(53);
            return '<span class="mailgun-emailError">Address is invalid.</span>';
        }
    }
}