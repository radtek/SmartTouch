var customFieldsManagerViewModel = function (viewModel, customFieldTabs, contactId, url) {
    var dataTypes = {
        "checkbox": 1,
        "datetime": 2,
        "email": 3,
        "number": 5,
        "radio": 6,
        "text": 8,
        "time": 9,
        "url": 10,
        "dropdown": 11,
        "multiselectdropdown": 12,
        "date": 13,
        "textarea": 14
    }
    var selfCustomFields = this;
    var dropdownFieldTypes = [1, 12];
    selfCustomFields.NavigationBarWidth = ko.observable(0);
    selfCustomFields.TabWidths = ko.observableArray([]);

    selfCustomFields.CurrentFirstTabWidth = ko.observable(0);
    selfCustomFields.CurrentFirstTabIndex = ko.observable(0);
    selfCustomFields.CalculateDimensions = function () {
        selfCustomFields.TabWidths([]);
        selfCustomFields.NavigationBarWidth(0);
        setTimeout(function () {
            $('.nav.nav-tabs.nav-stacked li').each(function (index, value) {
                var tabWidth = $(value).outerWidth(true);
                selfCustomFields.NavigationBarWidth(selfCustomFields.NavigationBarWidth() + tabWidth);
                selfCustomFields.TabWidths.push(tabWidth);
                $('#customfields-nav-bar').width(selfCustomFields.NavigationBarWidth() + 100);
            });
            selfCustomFields.CurrentFirstTabWidth(0);
            selfCustomFields.tabsMaxIndex = selfCustomFields.TabWidths().length - 1;
            setTimeout(function () {
                if (selfCustomFields.isLastTabReached()) {
                    $('.flslarrow.flslright').hide();
                    $(".flslarrow.flslleft").hide();
                }
            }, 2000);
        }, 1000);
        $('#customfields-nav-bar-wrapper').width($('#contact-details-tabs').width());

    };
    selfCustomFields.fetchedEditableCustomFields = ko.observable(false);
    selfCustomFields.isAnyFieldInEditMode = ko.observable(false);
    selfCustomFields.fieldBeingEdited = ko.observable();
    selfCustomFields.EditCustomField = function (tabIndex, sectionIndex, fieldIndex) {
        selfCustomFields.fetchedEditableCustomFields(true);
        selfCustomFields.fieldBeingEdited(customFieldTabs()[tabIndex].Sections()[sectionIndex].CustomFields()[fieldIndex]);
        selfCustomFields.fieldBeingEdited().OriginalValue = selfCustomFields.fieldBeingEdited().Value();
        if (dropdownFieldTypes.indexOf(selfCustomFields.fieldBeingEdited().FieldInputTypeId()) != -1)
            selfCustomFields.fieldBeingEdited().Value(selfCustomFields.fieldBeingEdited().Value().split("|").filter(function (e) { return e }));  //filter removes any empty records

        selfCustomFields.fieldBeingEdited().EditMode(true);
        selfCustomFields.isAnyFieldInEditMode(true);
        $('.edit-contact-custom-fields').addClass('hideElement');
        $('.edit-contact-custom-fields').removeClass('edit-contact-custom-fields');
    }

    selfCustomFields.isLastTabReached = function () {
        return $('#customfieldtab-' + selfCustomFields.tabsMaxIndex).offset().left + $('#customfieldtab-' + selfCustomFields.tabsMaxIndex).width() < $('.flslarrow.flslright').offset().left;
    }
    selfCustomFields.NextTab = function () {

        if (!selfCustomFields.isLastTabReached()) {
            var leftToBeAdded = selfCustomFields.TabWidths()[selfCustomFields.CurrentFirstTabIndex()]
            selfCustomFields.CurrentFirstTabIndex(selfCustomFields.CurrentFirstTabIndex() + 1);
            selfCustomFields.CurrentFirstTabWidth(selfCustomFields.CurrentFirstTabWidth() - leftToBeAdded);
            $('#customfields-nav-bar').css({ 'left': selfCustomFields.CurrentFirstTabWidth() });
            $('.flslarrow.flslright').removeClass("slarrow-disable");
        }

        if (selfCustomFields.isLastTabReached())
            $('.flslarrow.flslright').addClass("slarrow-disable");
        else
            $('.flslarrow.flslright').removeClass("slarrow-disable");

        if (selfCustomFields.CurrentFirstTabIndex() < 1)
            $(".flslarrow.flslleft").addClass("slarrow-disable");
        else
            $(".flslarrow.flslleft").removeClass("slarrow-disable");
    }
    selfCustomFields.PreviousTab = function () {
        if (selfCustomFields.CurrentFirstTabIndex() > 0) {
            selfCustomFields.CurrentFirstTabIndex(selfCustomFields.CurrentFirstTabIndex() - 1);
            var leftToBeAdded = selfCustomFields.TabWidths()[selfCustomFields.CurrentFirstTabIndex()];
            selfCustomFields.CurrentFirstTabWidth(selfCustomFields.CurrentFirstTabWidth() + leftToBeAdded);
            $('#customfields-nav-bar').css({ 'left': selfCustomFields.CurrentFirstTabWidth() });
            $(".flslarrow.flslleft").removeClass("slarrow-disable");
        }
        if (selfCustomFields.isLastTabReached())
            $('.flslarrow.flslright').addClass("slarrow-disable");
        else
            $('.flslarrow.flslright').removeClass("slarrow-disable");

        if (selfCustomFields.CurrentFirstTabIndex() < 1)
            $(".flslarrow.flslleft").addClass("slarrow-disable");
        else
            $(".flslarrow.flslleft").removeClass("slarrow-disable");
    }
    var BASE_URL = '@Url.Content("~/Contact/")';
    selfCustomFields.isAnyFieldInEditMode = ko.observable(false);

    var validate = function (field) {
        if (field.Value() != "") {
            if (field.FieldInputTypeId() == 10) {
                var reg = /^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/|www\.)[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$/;
                return reg.test(field.Value())
            }
            else if (field.FieldInputTypeId() == 3) {
                var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
                return re.test(field.Value());
            }
            else
                return true;
        }
        else
            return true;
    }
    selfCustomFields.SaveCustomField = function (fieldId) {
        var newValue;
        if (validate(selfCustomFields.fieldBeingEdited()) == true) {
            if (selfCustomFields.fieldBeingEdited().FieldInputTypeId() == 1) {
                var selectedValues = [];
                $('#' + fieldId + ' input[type=checkbox]').map(function () {
                    if (this.checked == true)
                        selectedValues.push(($(this).val()));
                });
                newValue = selectedValues.join('|');
            }
            else if (dropdownFieldTypes.indexOf(selfCustomFields.fieldBeingEdited().FieldInputTypeId()) != -1) {
                newValue = selfCustomFields.fieldBeingEdited().Value().join("|");
            }
            else if (selfCustomFields.fieldBeingEdited().FieldInputTypeId() == dataTypes.time)
                newValue = $('#' + fieldId).val();
            else if (selfCustomFields.fieldBeingEdited().FieldInputTypeId() == dataTypes.date) {
                var dateFormat = readCookie("dateformat").toUpperCase();
                if (selfCustomFields.fieldBeingEdited().Value()) {
                    selfCustomFields.fieldBeingEdited().Value(kendo.toString(new Date(selfCustomFields.fieldBeingEdited().Value()), readCookie("dateformat")));
                    newValue = selfCustomFields.fieldBeingEdited().Value();
                }
            }
            else
                newValue = selfCustomFields.fieldBeingEdited().Value();
            selfCustomFields.fieldBeingEdited().Value(newValue);
            if (selfCustomFields.fieldBeingEdited().OriginalValue != newValue) {
                $.ajax({
                    url: url + 'UpdateCustomFieldValue',
                    type: 'post',
                    dataType: 'json',
                    data: JSON.stringify({ 'contactId': contactId, 'fieldId': fieldId, 'newValue': newValue, 'inputType': selfCustomFields.fieldBeingEdited().FieldInputTypeId() }),
                    contentType: "application/json; charset=utf-8"
                }).then(function (response) {
                    var filter = $.Deferred()
                    if (response.success) {
                        filter.resolve(response)
                    } else {
                        filter.reject(response.error)
                    }
                    removeinnerLoader();
                    return filter.promise()
                }).done(function (data) {
                    viewModel.getTimeLine();
                }).fail(function (error) {
                    notifyError(error);
                });
            }
            selfCustomFields.isAnyFieldInEditMode(false);
            selfCustomFields.fieldBeingEdited().EditMode(false);
            $('.hideElement').addClass('edit-contact-custom-fields');
            $('.hideElement').removeClass('hideElement');
        }
        else
            notifyError("Invalid Format");
    }
    selfCustomFields.CancelEditing = function () {
        selfCustomFields.isAnyFieldInEditMode(false);
        selfCustomFields.fieldBeingEdited().Value(selfCustomFields.fieldBeingEdited().OriginalValue);
        $('.hideElement').addClass('edit-contact-custom-fields');
        $('.hideElement').removeClass('hide-custom-field-edit-icon');
    }
    $(".flslarrow.flslleft").addClass("slarrow-disable");

   
}