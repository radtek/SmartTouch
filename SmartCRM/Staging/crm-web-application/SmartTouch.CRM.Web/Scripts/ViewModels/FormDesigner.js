var formViewModel = function (data, webApp, webService, accountId, userId) {

    selfForm = this;
    //CONSTANTS
    CHECKBOX = "checkbox";
    ACTIVE = '201'; INACTIVE = '202'; 
    dropdownValues=[];
    convertedDropdownValues = ko.observableArray([]);

    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfForm));

    //Custom Validations
    ko.validation.rules['minimumLength'] = {
        validator: function (controls, minimumLength) {
            return (controls.length >= minimumLength);
        },
        message: '[|Please add at least one field to the form|]'
    };
    ko.validation.registerExtenders();
    selfForm.CurrentView = ko.observable('formdesigner');
    selfForm.DateFormat = ko.observable(data.DateFormat);
    selfForm.Name = ko.observable(data.Name).extend({ required: { message: "[|Form name is required|]" } });
    selfForm.FormFields = ko.observableArray([]);
    selfForm.CustomFields = ko.observableArray(data.CustomFields);
    selfForm.Status = ko.observable(data.Status.toString());
    selfForm.AllSubmissions = ko.observable(data.AllSubmissions);  
    //The following method will make all the properties of the object as observable.
    var makeChildrenObservable = {
        child: {
            create: function (options) {
                if (!options.data) {
                    return null;
                }
                return ko.mapping.fromJS(options.data);
            }
        }
    }
    ko.mapping.fromJS(data.FormFields, makeChildrenObservable, selfForm.FormFields);

    selfForm.Designer = ko.observable(new formDesigner(selfForm.FormFields(), data.FormId, data.AccountId, webService, userId));

    var getAccountDropdownValues = function () {
        pageLoader();
        $.ajax({
            url: webApp + 'GetAccountDropdownValues',
            type: 'get',
            dataType: 'json',
            contentType: 'application/json; charset = utf-8',
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + authToken);
            },
        }).then(function (response) {
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response)
            }
            else {
                filter.reject(response.error)
            }
            
            return filter.promise()
        }).done(function (response) {
            dropdownValues = response.response
            $.each(dropdownValues, function (dropDownIndex, dropdownValue) {
                $.each(dropdownValue.DropdownValuesList, function (index, value) {
                    if (value.IsActive == true)
                        convertedDropdownValues.push(ko.observable({ CustomFieldId: value.DropdownID, CustomFieldValueOptionId: value.DropdownValueID, Value: value.DropdownValue }));
                })
            });
            selfForm.Fields()[selfForm.Fields().map(function (e) { return e.FieldId }).indexOf(contactFields.LeadSource)].ValueOptions
                = convertedDropdownValues().filter(function (e) { return e().CustomFieldId == dropdownTypes.LeadSources });
            selfForm.Fields()[selfForm.Fields().map(function (e) { return e.FieldId }).indexOf(contactFields.Community)].ValueOptions
                = convertedDropdownValues().filter(function (e) { return e().CustomFieldId == dropdownTypes.Community });

            var leadSourceField = selfForm.FormFields().map(function (e) { return e.FieldId(); }).indexOf(contactFields.LeadSource);
            var communityField = selfForm.FormFields().map(function (e) { return e.FieldId(); }).indexOf(contactFields.Community);
            if (leadSourceField >= 0)
                selfForm.FormFields()[leadSourceField].ValueOptions(convertedDropdownValues().filter(function (e) { return e().CustomFieldId == dropdownTypes.LeadSources }));
            if (communityField >= 0)
                selfForm.FormFields()[communityField].ValueOptions(convertedDropdownValues().filter(function (e) { return e().CustomFieldId == dropdownTypes.Community }));
            removepageloader();
            selfForm.Designer().Controls.valueHasMutated();
            selfForm.Designer().CaptureAllEdits();
        }).fail(function () {
            dropdownValues = [];
            removepageloader();
            notifyError(error);
        });
    }

    getAccountDropdownValues();
    ko.bindingHandlers.sortableList = {
        init: function (element, valueAccessor) {
            var list = valueAccessor();
            $(element).sortable({
                update: function (event, ui) {
                    //retrieve our actual data item
                    var item = ui.item.tmplItem().data;
                    //figure out its new position
                    var position = ko.utils.arrayIndexOf(ui.item.parent().children(), ui.item[0]);
                    //remove the item and add it back in the right spot
                    if (position >= 0) {
                        list.remove(item);
                        list.splice(position, 0, item);
                    }
                }
            });
        }
    };
    selfForm.temporaryStorage = ko.observable('');
    selfForm.temporaryDisplayName = ko.observable('');
    var varvalue = ""; var Index = 0;


    selfForm.editFieldDisplayName = function (index) {
        Index = index;
        selfForm.fieldEditMode(true);
        var fieldId = selfForm.Designer().Controls()[index].FieldId();
        var displayName = selfForm.Designer().Controls()[index].DisplayName();
        selfForm.temporaryDisplayName(displayName);
        $("#fieldlabel-" + fieldId).addClass("hide");
        $("#displayname-" + fieldId).removeClass("hide");
        $(".st-icon-edit").addClass('hide');
    }

    selfForm.saveDisplayNameChange = function (index) {
        if (selfForm.temporaryDisplayName().trim() == '') {
            notifyError("[|Display name cannot be empty|]");
            return;
        }
        selfForm.fieldEditMode(false);
        var fieldId = selfForm.Designer().Controls()[index].FieldId();
        selfForm.Designer().Controls()[index].DisplayName(selfForm.temporaryDisplayName());
        $("#fieldlabel-" + fieldId).removeClass("hide");
        $("#displayname-" + fieldId).addClass("hide");
        $(".st-icon-edit").removeClass('hide');

        selfForm.Designer().CaptureAllEdits();
    };

   
    selfForm.formfieldkeyup = function (tabIndex, event) {
        if (event.keyCode == 13) {
            varvalue = $("#formfieldname").val();
            if (varvalue != "") {
                selfForm.temporaryStorage(varvalue);

                var fieldId = selfForm.Designer().Controls()[Index].FieldId();
                selfForm.Designer().Controls()[Index].DisplayName(selfForm.temporaryDisplayName());

                $("#fieldlabel-" + fieldId).removeClass("hide");
                $("#displayname-" + fieldId).addClass("hide");

                $(".st-icon-edit").removeClass('hide');
            }
            else {
                notifyError("[|Display name cannot be empty|]");
                return;
            }

        }
    }



    selfForm.cancelDisplayNameChange = function (index) {
        selfForm.fieldEditMode(false);
        var fieldId = selfForm.Designer().Controls()[index].FieldId();
        selfForm.temporaryDisplayName('');
        $("#fieldlabel-" + fieldId).removeClass("hide");
        $("#displayname-" + fieldId).addClass("hide");
        $(".st-icon-edit").removeClass('hide');

    };

    selfForm.EmailStatuses = ko.observableArray([
        { Id: 50, Type: '[|NotVerified|]' },
        { Id: 51, Type: '[|Verified|]' },
        { Id: 52, Type: '[|Soft Bounce|]' },
        { Id: 53, Type: '[|Hard Bounce|]' },
        { Id: 54, Type: '[|UnSubscribed|]' }]);

    selfForm.websiteType = ko.observableArray([
        { Id: 1, Type: 'https://' },
        { Id: 2, Type: 'http://' }]);

    selfForm.TestHTML = ko.observable();

    selfForm.TestHTML = ko.observable();
    selfForm.HTMLContent = ko.observable(data.HTMLContent);
    selfForm.HTMLContent.subscribe(function () {
        testHTML = document.createElement('div');
        testHTML.innerHTML = selfForm.HTMLContent();
        $(testHTML).children().each(function () {
            if ($(this).attr("src")) {
                $(this).remove();
            }
        });
        selfForm.TestHTML(testHTML.innerHTML);
    });
    
    

    selfForm.Designer().ViewableHTML.subscribe(function () {
        selfForm.HTMLContent(selfForm.Designer().ViewableHTML());
    });
    selfForm.CreatedDate = ko.observable(ConvertToDate(data.CreatedDate));
    selfForm.DisplayCreatedDate = kendo.toString(kendo.parseDate(selfForm.CreatedDate()), selfForm.DateFormat());

    var convertToCommaSeparatedString = function (tagsArray) {
        if (tagsArray == null)
            return "";

        var tags = [];
        $.each(tagsArray, function (index, value) {
            if (ko.isObservable(value.TagName))
                tags.push(value.TagName());
            else
                tags.push(value.TagName);
        });
        return tags.join(",");
    }
    selfForm.TagsList = ko.observableArray(data.TagsList);
    selfForm.FormTags = ko.pureComputed(function () {
        return convertToCommaSeparatedString(selfForm.TagsList());
    });

    selfForm.Fields = ko.observableArray(data.Fields);
    selfForm.searchQuery = ko.observable('');

    selfForm.computedFieldSearch = ko.pureComputed(function () {
        return ko.utils.arrayFilter(selfForm.Fields(), function (item) {
            var isFieldSelected = selfForm.FormFields().map(function (e) { return e.FieldId() }).indexOf(item.FieldId) >=0;
            var filteredFields = item.Title.toLowerCase().indexOf(selfForm.searchQuery().toLowerCase()) >= 0;
            return filteredFields && !isFieldSelected;
        });
    });

    selfForm.computedCustomFieldSearch = ko.pureComputed(function () {
        return ko.utils.arrayFilter(selfForm.CustomFields(), function (item) {
            var isFieldSelected = selfForm.FormFields().map(function (e) { return e.FieldId() }).indexOf(item.FieldId) >= 0;

            var filteredFields = item.Title.toLowerCase().indexOf(selfForm.searchQuery().toLowerCase()) >= 0;
            return filteredFields && !isFieldSelected;

        });
    });
    selfForm.AcknowledgementType = ko.observable(data.AcknowledgementType + '');
    selfForm.Acknowledgement = ko.observable(data.Acknowledgement).extend({
        required: { message: "[|Acknowledgement is required|]" },
        pattern: {
            params: /(http(s)?:\\)?([\w-]+\.)+[\w-]+[.com|.in|.org]+(\[\?%&=]*)?/, message: "[|Invalid Web Site URL|]",
            onlyIf: function () { return selfForm.AcknowledgementType() == '1'; }
        }
    });

    var getSelectedField = function (fieldId) {
        var fieldCodeIndex = selfForm.computedFieldSearch().map(function (e) { return e.FieldId }).indexOf(parseInt(fieldId.replace('control-', '').replace('toolbar', '')));

        if (fieldCodeIndex != -1) {
            return selfForm.computedFieldSearch()[fieldCodeIndex];
        }
        else {

            fieldCodeIndex = selfForm.computedCustomFieldSearch().map(function (e) { return e.FieldId }).indexOf(parseInt(fieldId.replace('control-', '').replace('toolbar', '')));

            return selfForm.computedCustomFieldSearch()[fieldCodeIndex];
        }
    };
    var applyDroppables = function () {    
        droppableControl(function (event, ui) {            
            $Container = $(this);
            var selectedField = getSelectedField($(ui.draggable)[0].id);
            selectedField.Id = $(ui.draggable)[0].id;
            var disableSelectedField;
            disableSelectedField = document.getElementById($(ui.draggable)[0].id);
            disableSelectedField.className = disableSelectedField.className.replace('formitem', 'hide');
            if (selfForm.FormFields().map(function (e) { return e.FieldId() }).indexOf(parseInt(selectedField.FieldId)) == -1)
                selfForm.Designer().AddControl(selectedField);

            selfForm.Designer().CaptureAllEdits();
        }, true);
    }


    var droppableControl = function (dropCallback, isControl) {
        $(".st-form-droparea").droppable({
            greedy: true,
            hoverClass: "st-drophover",
            accept: ".formitem",
            heightStyle: "content",
            drop: dropCallback
        });
    }
    applyDroppables();
    selfForm.LoadControls = function () {
        applyDroppables();
        //editor();
    }
    selfForm.LoadControls();
    //Extended validation
    selfForm.controlsValidation = selfForm.Designer().Controls.extend({ minimumLength: 1 });

    selfForm.errors = ko.validation.group(selfForm);
    var authToken = readCookie("accessToken");
    selfForm.saveForm = function (redirectPath) {
        selfForm.FormFields(selfForm.Designer().Controls());
        selfForm.HTMLContent(selfForm.Designer().ViewableHTML());
        selfForm.errors.showAllMessages();
        if (selfForm.errors().length > 0) {
            validationScroll();
            return;
        }
      
        if (selfForm.FormFields().length > 0) {
            for (var i = 0; i < selfForm.FormFields().length; i++) {
                if (selfForm.FormFields()[i].DisplayName().length > 75) {
                    notifyError(selfForm.FormFields()[i].Title() + " exceeds maximum length of 75 characters");
                    return;
                }
            }
        }
        var type = 'post';
        if (selfForm.FormId() > 0)
            type = 'put';
        var jsondata = ko.toJSON(selfForm);
        pageLoader();
        $.ajax({
            url: webService + '/Forms',
            type: type,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + authToken);
            },
            data: jsondata,
            success: function (data) {               
                notifySuccess('[|Successfully saved the form|]');
                 setTimeout(
                        function () {
                            removepageloader();
                            if (redirectPath == "testform")
                                selfForm.CurrentView('testform');
                            else
                            window.location.href = "/forms";
                        }, setTimeOutTimer);
            },
            error: function (response) {              
                removepageloader();              
                notifyError(response.responseText);
            }

        });

    }
    selfForm.saveFormAs = function () {
        if (selfForm.FormId() === 0) {
            return;
        }
        window.location.replace("/SaveFormAs?formId=" + selfForm.FormId());
    }

    selfForm.testForm = function () {
        if (selfForm.FormId() > 0) {
            alertifyReset("OK", "Cancel");
            alertify.confirm("[|Any changes made to the form will be saved. Are you sure?|]", function (e) {
                if (e) {
                    selfForm.saveForm("testform");
                }
            });
        }
        else
            notifyError("[|Form should be saved before testing.|]");
    }

    selfForm.exitTestForm = function () {
        selfForm.CurrentView('formdesigner');
    }

    selfForm.cancelForm = function () {
        alertifyReset("Delete Form", "Cancel");
        alertify.confirm("[|Are you sure you want to exit this form without saving?|]", function (e) {
            if (e) {
                window.location.href = "/forms";
            }
        });
    }

    //legacy code to delete forms
    selfForm.deleteForm = function () {
        alertifyReset("Delete Form", "Cancel");
        alertify.confirm("[|Are you sure you want to delete this Form?|]", function (e) {
            if (e) {
                var formIds = [];
                formIds.push(selfForm.FormId());
                varDeleteURL = "Form/DeleteForm";
                jsondata = JSON.stringify({ 'FormIDs': formIds });

                $.ajax({
                    url: varDeleteURL,
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ 'formIDs': jsondata })
                }).then(function (response) {
                    var filter = $.Deferred();
                    if (response.success) {
                        filter.resolve(response)
                    }
                    else {
                        filter.reject(response.error)
                    }
                    return filter.promise()
                }).done(function (data) {
                    notifySuccess("[|Successfully deleted the form|]");
                    setTimeout(function () { window.location.href = "/forms" }, setTimeOutTimer);
                }).fail(function (error) {
                    notifyError(error);
                });
                
            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }
        });
    };

    selfForm.showFieldControls = function () {
        $("#fieldcontrols-" + this.FieldId()).removeClass('hide');
    };

    selfForm.hideFieldControls = function () {
        $("#fieldcontrols-" + this.FieldId()).addClass('hide');
    };

    selfForm.fieldEditMode = ko.observable(false);
    $.ajax({
        url: webApp + 'GetCountriesAndStates',
        type: 'get',
        dataType: 'json',
        contentType: "application/json; charset=utf-8"
    }).then(function (response) {
        var filter = $.Deferred();
        if (response.success) {
            filter.resolve(response)
        }
        else {
            filter.reject(response.error)
        }
        return filter.promise()
    }).done(function (data) {
        var countryselect = { CustomFieldId: 0, CustomFieldValueOptionId: null, Value: "Select" };
        $.each(data.response.Countries, function (index, value) {
            var country = { CustomFieldId: 20, CustomFieldValueOptionId: value.Code, Value: value.Name };
            selfForm.Fields()[19].ValueOptions.push(ko.observable(country));
        });
        selfForm.Fields()[19].ValueOptions.unshift(ko.observable(countryselect));
        var stateselect = { CustomFieldId: 0, CustomFieldValueOptionId: null, Value: "Select" };
        $.each(data.response.States, function (index, value) {
            var state = { CustomFieldId: 18, CustomFieldValueOptionId: value.Code, Value: value.Name };
            selfForm.Fields()[17].ValueOptions.push(ko.observable(state));
        })
        selfForm.Fields()[17].ValueOptions.unshift(ko.observable(stateselect));
        var stateFormFieldIndex = selfForm.FormFields().map(function (e) { return e.FieldId(); }).indexOf(18);
        var countryFormFieldIndex = selfForm.FormFields().map(function (e) { return e.FieldId(); }).indexOf(20);
        if (stateFormFieldIndex >= 0)
            selfForm.FormFields()[stateFormFieldIndex].ValueOptions(selfForm.Fields()[17].ValueOptions);
        if (countryFormFieldIndex >= 0)
            selfForm.FormFields()[countryFormFieldIndex].ValueOptions(selfForm.Fields()[19].ValueOptions);

        selfForm.Designer().CaptureAllEdits();
    }).fail(function (error) {
        notifyError(error);
    });
    ;

    selfForm.selectedLeadSourceValue = ko.observable();
    var initialLoad = true;
    setTimeout(function () {
        if (initialLoad) {
            selfForm.FormFields().filter(function (e) { return e.FieldId() == 24 })[0].Value($(data.HTMLContent).find("#24").val())
        }
    }, 3000);
}

///Form Designer;
var formDesigner = function (formFields, formId, accountId, webService, userId) {
    // The following method will add EditedHTML property to each FormField object. 
    // This property will be used to capture the html of the FormField. Basically, this method will be applicable in form edit mode.
    var addEditedHTMLProperty = function () {
        $.each(formFields, function (index, value) {
            value.EditedHTML = ko.observable("");
        })
    };
    addEditedHTMLProperty();

    selfDesigner = this;
    selfDesigner.Controls = ko.observableArray(formFields);
    HideSelectedFields = function () {
        $.each(selfDesigner.Controls(), function (index, value) {
            test = value.FieldId;
            disableSelectedField = document.getElementById("toolbarcontrol-" + value.FieldId());
            if (typeof disableSelectedField != "undefined" && disableSelectedField != null) {
                disableSelectedField.className = disableSelectedField.className.replace('formitem', 'hide');
            }
        })
    };
    selfDesigner.AddControl = function (selectedField) {
        var controlId = selfDesigner.Controls().length + 1;        
        var control = new controlTemplate(controlId, selectedField);
        control.EditedHTML = ko.observable("");
        selfDesigner.Controls.push(control);
    }

    
    var currentHostName = window.location.origin;
    
    selfDesigner.ViewableHTML = ko.computed({
        read: function () {
            
            var viewableHtml = "<style>"
                +".required{color: #ea2323;} .submitform{position:relative;}"
                +".loader{position:absolute;left:0;right:0;top:0;bottom:0;background-color:rgba(0,0,0,0);}"
                +".loader img{position:absolute;top:50%;left:50%;margin-left:-10px;margin-top:-10px;}"
                +".control-group{display:block;margin-bottom:10px;}.control-group > div {position: relative; margin-left: 210px;}"
                +".control-label{float: left;width: 200px;}.control-group select{border: 1px solid #dbdbdb;background-color: #fff;padding: 5px;width: 200px;}"
                + ".control-group textarea,.control-group input{border: 1px solid #dbdbdb;background-color: #fff;padding: 5px;width: 200px;}"
                +'.radio-inline > input[type="radio"], .checkbox-inline > input[type="checkbox"] {margin-right: 5px;float: left;width: auto;}'
                +'.radio-inline, .checkbox-inline {cursor: pointer;display: inline-block;font-weight: normal;margin-bottom: 0;vertical-align: middle;margin-right: 8px;}'
                +'#submit'+ formId +'{margin-left: 210px;background-color: #88B764;color: #fff;padding:10px 15px;border-radius: 5px;border: none;margin-top: 10px;}'
                + "</style>"
                
                
            var submitformid = "submitform" + formId;
            var failuremessage = "failuremessage" + formId;
            var successresponse = "successrespone" + formId;
            var successmessage = "successmessage" + formId;
            var databeingsubmitted = "databeingsubmitted" + formId;       
            
            var htmlFormTag = "<form action='" + webService + "/submitformpost'" + " method='post' id='" + submitformid + "' name='stform'>"
                + "<div id = '" + failuremessage + "' style='color: #ea2323; border: 1px solid #d6e9c6; border-radius: 4px; margin-bottom: 20px; padding: 15px;display:none;font-family:Calibri;'></div>"
                + "<div id='"+databeingsubmitted+"'>"
                + "<div style='max-width:990px; margin:20px auto;font-family:Calibri;'>"
                + "<input type='hidden' name='formid' value=" + formId + ">"
                + "<input type='hidden' name='accountid' value=" + accountId + ">"
                + "<input type='hidden' name='userid' value=" + userId + ">"
                + "<input type='hidden' id='domainname' name='domainname' value=''>"
                + "<input type='hidden' id='STITrackingID' name='STITrackingID' value=''>"
                + "<input type='hidden' id='postBackUrl' name='postBackUrl' value=''>"
                + "<input type='hidden' id='redirect-override' name='redirect-override' value=''>"

                + "<input type='hidden' id='ajaxurl' name='ajaxurl' value='" + webService + "/submitform'>"
                + "<input type='submit' style='display:none;'/>";
                
            ko.utils.arrayForEach(selfDesigner.Controls(), function (item) {
                if (item.EditedHTML() !== "") {
                    var displayProperty = item.IsHidden() ? "display : none" : ""
                    var temp = "<div><div class='control-group' style='" + displayProperty + "'>" + item.EditedHTML() + "</div></div>";
                    cloned = $(temp).clone();
                    cloned.find("*").removeAttr("data-bind");
                    cloned.find(".designer").remove();
                    cloned.find("#fieldtitle").remove();
                    htmlFormTag = htmlFormTag + cloned.html();

                }

            });
            htmlFormTag = htmlFormTag + "<div class='hr-border'></div><div><button type='button' id ='submit" + formId + "' onClick='if(this.type==\"submit\"){this.form.submit(); this.disabled=true;}'>[|Submit|]</button></div>"
                + '<div class="loader" style="display:none;"><img src="' + currentHostName + '/img/loader.gif" alt="Loading" /></div>  '
                + "</div></div>"
                + "<div style='display:none' id='"+successresponse+"'> <div style='font-family: Open Sans, sans-serif; padding: 50px 0;'>"
                + "<div style='width:620px;margin:0 auto;'><div style='text-align:center;'></div>"
                + "<div style='background-color:#ffffff;box-shadow:0 0 5px rgba(0,0,0,0.5);padding:50px 25px;border-radius:5px;min-height:300px;'>"
                + "<div id = '"+successmessage+"' style='background-color: #dff0d8; color: #3c763d; border: 1px solid #d6e9c6; border-radius: 4px; margin-bottom: 20px; padding: 15px;font-family:Calibri;'></div>"
                + "<div class='mll mbl' style='margin: auto '><button type='button'  id ='ok" + formId + "'>OK</button></div>"
                + "</div></div></div></div>"
                + "</form>";
            viewableHtml = viewableHtml + htmlFormTag

                + "<script>"

                + "if (!window.jQuery) {"
                    + "var script = document.createElement('script');" 
                    + "script.type = 'text/javascript';"
                    + "script.src = 'http://code.jquery.com/jquery-1.11.2.min.js';" 
                    + "document.getElementsByTagName('head')[0].appendChild(script);"
                + "}"
                + "setTimeout(function(){"
                + "function readCookie(cname) {"
                        + 'var name = cname + "=";'
                        + "var ca = document.cookie.split(';');"
                        + "for(var i=0; i<ca.length; i++) {"
                            + "var c = ca[i];"
                            + "while (c.charAt(0)==' ') c = c.substring(1);"
                                + "if (c.indexOf(name) == 0) document.getElementById('STITrackingID').value=c.substring(name.length, c.length);"
                     + "}"
                    + "};"

                + "jQuery.support.cors = true;"
                + "$('#submit" + formId + "').off('click');"
                + "$('#submit" + formId + "').click(function() {"
                    + "readCookie('STITrackingID');"
                    + "if (!$('#" + submitformid + "')[0].checkValidity()) {$('#" + submitformid + "').find('input[type=" + '"submit"' + "]').click();return false;};"
                    + "$('#" + successresponse + "').hide();"
                    + "$('#"+failuremessage+"').hide();"
                    + "$('.loader').show();"
                    + '$.ajax({data: $("#' + submitformid + '").serialize(),type: $("#' + submitformid + '").attr("method"),url: $("#ajaxurl").val(),'
                    + "success: function(data) {$('#"+databeingsubmitted+"').hide();"
                    + "if(data.AcknowledgementType == 1){window.location.href = data.Acknowledgement;}else {$('#" + successresponse + "').show();$('#"+successmessage+"').html(data.Acknowledgement)};$('.loader').hide();},"
                    + "error:function(error){$('#"+failuremessage+"').html(error.responseText);$('#"+failuremessage+"').show();$('.loader').hide();}});});"
                    + "$('#ok" + formId + "').click(function(){$('#"+databeingsubmitted+"').show();$('#" + successresponse + "').hide();});"
                    + 'var options = $("#18").html();'
                    + '$("#20").change(function(e) {text = $("#20 :selected").val();$("#18").html(options);if(text == "") return;'
                    + "$('#18 :not([value^='+text.trim()+'])').remove();})"
                    + "},2000)"


                + "</script>";   
            return viewableHtml.toString();
        },
        write: function (newValue) {
        }
    });

    selfDesigner.CaptureAllEdits = function () {
        ko.utils.arrayForEach(selfDesigner.Controls(), function (item) {            
            if ($("#control-content-" + item.FieldId()).html() != undefined) {
                newInnerHtml = document.createElement('div');
                newInnerHtml.innerHTML = $("#control-content-" + item.FieldId()).html();
                $(newInnerHtml).children("option").removeAttr("data-bind");
                newInnerHtml.innerHTML.replace('contenteditable=""', '')
                    .replace('contenteditable', '').replace().replace('form-control', '').replace(/"/g, "'");
                item.EditedHTML(newInnerHtml.innerHTML);                
            }
        });
        $("#htmlPrettifier").removeClass("prettyprinted");
        prettyPrint();
    }

    selfDesigner.DeleteControl = function (data, event) {
        selfDesigner.Controls.remove(this);
        $("#toolbarcontrol-" + this.FieldId()).addClass("formitem").removeClass("hide");
        selfDesigner.CaptureAllEdits();
        selfForm.searchQuery(selfForm.searchQuery() + " ");
        selfForm.searchQuery(selfForm.searchQuery().substring(0, selfForm.searchQuery().length - 1));
    }
};

var controlTemplate = function (id, selectedField) {
    var selfTemplate = this;
    selfTemplate.Title = ko.observable(selectedField.Title);
    selfTemplate.FieldCode = ko.observable(selectedField.FieldCode);
    selfTemplate.Value = ko.observable(selectedField.Value);
    selfTemplate.FieldId = ko.observable(selectedField.FieldId);
    selfTemplate.FieldInputTypeId = ko.observable(selectedField.FieldInputTypeId);
    selfTemplate.DisplayName = ko.observable(selectedField.Title || selectedField.DisplayName);
    selfTemplate.IsMandatory = ko.observable(selectedField.IsMandatory);
    selfTemplate.IsHidden = ko.observable(false );
    selfTemplate.ValidationMessage = ko.observable(selectedField.ValidationMessage);
    selfTemplate.SubFields = ko.observable(selectedField.SubFields);
    selfTemplate.ValueOptions = ko.observableArray(selectedField.ValueOptions);
    selfTemplate.SortId = ko.observable(id);
    selfTemplate.TemplateId = ko.observable('controltemplate-' + selectedField.FieldCode);
};
