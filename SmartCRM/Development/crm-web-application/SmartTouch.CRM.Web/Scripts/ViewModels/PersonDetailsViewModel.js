var personDetailsViewModel = function (PersonDetails, url, CommunitcationURL, Activities, OpportunityUrl, WEBSERVICE_URL, itemsPerPage) {
    selfDetails = this;
    var DATATYPE_DATE = 13;
    var DATATYPE_TIME = 9;
    ko.mapping.fromJS(PersonDetails, {}, selfDetails);
    ko.bindingHandlers.tooltip = {
        init: function (element, valueAccessor) {
            var local = ko.utils.unwrapObservable(valueAccessor()),
                options = {};
            ko.utils.extend(options, ko.bindingHandlers.tooltip.options);
            ko.utils.extend(options, local);
            $(element).tooltip(options);
            ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
                $(element).tooltip("destroy");
            });
        },
        options: {
            placement: "right",
            trigger: "hover"
        }
    };

    selfDetails.PreviousLifecycleStage = ko.observable(PersonDetails.LifecycleStage);

    selfDetails.DateFormat = ko.observable(PersonDetails.DateFormat);
    selfDetails.Dateformat = ko.observable(PersonDetails.DateFormat);
    selfDetails.ContactImageUrl = ko.observable();
    selfDetails.contactdetails = ko.observable();
    selfDetails.LastTouchedInformation = ko.observable(PersonDetails.LastContactedString);
    selfDetails.ContactType = ko.observable(PersonDetails.ContactType);

    if (PersonDetails.SelectedLeadSource == null)
        selfDetails.SelectedLeadSource = ko.observableArray();

    selfDetails.LastLeadSource = ko.pureComputed({
        read: function () {
            if (selfDetails.SelectedLeadSource() != null) {
                var lastLeadScore = "";
                for (var g = 0; g < selfDetails.SelectedLeadSource().length; g++) {
                    if (g > 0) {
                        if (g == selfDetails.SelectedLeadSource().length - 1) {
                            lastLeadScore += selfDetails.SelectedLeadSource()[g].DropdownValue();
                        }
                        else {
                            if (lastLeadScore.indexOf(selfDetails.SelectedLeadSource()[g].DropdownValue()))
                                lastLeadScore += selfDetails.SelectedLeadSource()[g].DropdownValue() + ",";

                        }

                    }
                }
                return lastLeadScore;
            }
            else
                return "";
        },
        write: function (newValue) {
        },
        owner: this
    });
    selfDetails.communities = "";
    if (selfDetails.Communities && selfDetails.Communities().length > 0) {
        $.each(selfDetails.Communities(), function (index, value) { selfDetails.communities = selfDetails.communities + ", " + value.DropdownValue() });
        selfDetails.communities = selfDetails.communities.slice(2, selfDetails.communities.length);
    }
    selfDetails.Persons = ko.observable();
    selfDetails.IntegratedPersons = ko.observableArray();

    selfDetails.GetPersonsCountForCompany = function () {
        $.ajax({
            url: url + 'GetPersonsCount',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'contactId': selfDetails.ContactID() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            selfDetails.Persons(data.response.PersonsCount);
            selfDetails.IntegratedPersons(data.response.Persons);
        }).fail(function (error) {
            notifyError(error);
        });
    }

    if (PersonDetails.ContactType != 1) {
        selfDetails.GetPersonsCountForCompany();
        GetOpportunitySummary();
    }

    selfDetails.WebVisits = ko.observable();
    selfDetails.showWebVisitsTab = function () {
        $('#contactwebvisitstab').click();
        $('#showWebVisits').click();
        selfDetails.webVisitsActivated(true);
    };

    function GetWebVisitsCount() {

        $.ajax({
            url: url + 'GetWebVisitsCount',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'contactId': selfDetails.ContactID(), 'period': selfDetails.SelectedPeriod() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            selfDetails.WebVisits(data.response.WebVisitsCount);
        }).fail(function (error) {
            notifyError(error);
        })
    }

    selfDetails.GetPersons = function (persons) {
        if (persons != 0 && persons > 1)
            return persons + " Persons";
        else if (persons == 1)
            return persons + " Person";
    }

    selfDetails.Periods = ko.observableArray([
        { PeriodId: 1, Period: "[|All|]" },
        { PeriodId: 2, Period: "[|Last 7 Days|]" },
        { PeriodId: 3, Period: "[|Last 30 Days|]" },
        { PeriodId: 4, Period: "[|Last 60 Days|]" },
        { PeriodId: 5, Period: "[|Last 90 Days|]" }
    ]);

    selfDetails.PeriodId = ko.observable();
    selfDetails.SelectedPeriod = ko.observable(new Date(0));

    GetEngagementDetails();
    GetContactEmailEngagementDetails();

    selfDetails.periodChange = function () {
        var period = this.value();
        if (period == "1")
            selfDetails.SelectedPeriod(moment(new Date(0)).format());
        else if (period == "2")
            selfDetails.SelectedPeriod(moment().subtract(7, 'days').format());
        else if (period == "3")
            selfDetails.SelectedPeriod(moment().subtract(30, 'days').format());
        else if (period == "4")
            selfDetails.SelectedPeriod(moment().subtract(60, 'days').format());
        else if (period == "5")
            selfDetails.SelectedPeriod(moment().subtract(90, 'days').format());

        GetEngagementDetails();
        eraseCookie("widgetperiod");
        createCookie("widgetperiod", period, 1);
        if (PersonDetails.ContactType != 1)
            GetOpportunitySummary();
    }

    function GetOpportunitySummary() {
        $.ajax({
            url: url + 'GetOpportunitySummary',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'contactId': selfDetails.ContactID() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            selfDetails.NumberOfPotentialOpportunities(data.response.OpportunitySummary.NumberOfPotentialOpportunities);
            selfDetails.NumberOfWonOpportunities(data.response.OpportunitySummary.NumberOfWonOpportunities);
            selfDetails.NumberOfWon(data.response.OpportunitySummary.NumberOfWon);
            selfDetails.ValueOfWon(data.response.OpportunitySummary.ValueOfWon);
            selfDetails.NumberOfPotential(data.response.OpportunitySummary.NumberOfPotential);
            selfDetails.ValueOfPotential(data.response.OpportunitySummary.ValueOfPotential);
        }).fail(function (error) {
            notifyError(error);
        })
    }

    function GetContactLeadScore() {
        $.ajax({
            url: url + 'GetContactLeadScore',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'contactId': selfDetails.ContactID(), 'period': selfDetails.SelectedPeriod() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            selfDetails.LeadScore(data.response.LeadScore);
        }).fail(function (error) {
            notifyError(error);
        })
    }

    function GetEmailsCount() {
        //ajax call to get the "Sent" and "Opened" emails count
        $.ajax({
            url: url + 'GetEmailsCount',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'contactId': selfDetails.ContactID(), 'period': selfDetails.SelectedPeriod() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            selfDetails.CampaignsSummary(ko.mapping.fromJS(data.response));

            $("#campaignsummary-loader").addClass("hide");
        }).fail(function (error) {
            $("#campaignsummary-loader").addClass("hide");
            notifyError(error);
        })
    }

    function GetEngagementDetails() {
        $.ajax({
            url: url + 'GetEngagementDetails',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'contactId': selfDetails.ContactID(), 'period': selfDetails.SelectedPeriod() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            selfDetails.CampaignsSummary(ko.mapping.fromJS(data.response.CampaignInfo));
            selfDetails.LeadScore(data.response.LeadScore);
            selfDetails.WebVisits(data.response.WebVisits);
            selfDetails.EmailsSummary(ko.mapping.fromJS(data.response.EmailInfo));
        }).fail(function (error) {
            notifyError(error);
        })
    }

    function GetContactEmailEngagementDetails() {
        $.ajax({
            url: url + 'GetContactEmailEngagementDetails',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'contactId': selfDetails.ContactID() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            selfDetails.WorkflowsCount(data.response.WorkflowsCount);
            selfDetails.CampaignsCount(data.response.CampaignsCount);
            selfDetails.EmailsCount(data.response.EmailsCount);
        }).fail(function (error) {
            notifyError(error);
        })
    }

    function handleRelationshipDetailsResponse(response) {
        selfDetails.RelationshipViewModel(response);
        setTimeout(function () {
            veCarousel('relationships-carousel');
        }, 500);
    }

    var GetRelationshipDetails = function () {
        $.ajax({
            url: url + 'GetRelationships',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'contactId': selfDetails.ContactID() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            handleRelationshipDetailsResponse(data.response.Relationshipentry);
        }).fail(function (error) {
            notifyError(error);
        })
    }

    function handleOpportunityDetailsResponse(response) {
        selfDetails.Opportunities(response);
        setTimeout(function () {
            veCarousel('opportunities-carousel');
        }, 500);
    }

    var GetOpportunityDetails = function () {
        $.ajax({
            url: url + 'GetOpportunities',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'contactId': selfDetails.ContactID() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            handleOpportunityDetailsResponse(data.response)
        }).fail(function (error) {
            notifyError(error);
        });
    }

    function handleActionDetailsResponse(response) {
        selfDetails.Actions(response);
        setTimeout(function () {
            veCarousel('actions-carousel');
            appendCheckbox();
            minimize();
        }, 500);
        $(':checkbox').on('change', function () {
            $(this).triggerHandler('click');
        });
    }

    var GetActionDetails = function () {
        //to get the actions for this contact
        $.ajax({
            url: url + 'GetActions',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'contactId': selfDetails.ContactID() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            handleActionDetailsResponse(data.response);
        }).fail(function (error) {
            notifyError(error);
        })
    }

    function handleTourDetailsResponse(response) {
        selfDetails.Tours(response);
        setTimeout(function () {
            veCarousel('tours-carousel');
            appendCheckbox();
            minimize();
        }, 500);
        $(':checkbox').on('change', function () {
            $(this).triggerHandler('click');
        });
    }

    var GetTourDetails = function () {
        //to get the actions for this contact
        $.ajax({
            url: url + 'GetTours',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'contactId': selfDetails.ContactID() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            } return filter.promise()
        }).done(function (data) {
            handleTourDetailsResponse(data.response);
        }).fail(function (error) {
            notifyError(error);
        });
    }

    selfDetails.BundleAjaxCalls = function (contactType, pageNumber, pageSize) {
        $.ajax({
            url: url + 'GetContactActivity',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'contactId': selfDetails.ContactID(), 'pageNumber': 1, 'pageSize': 10 })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            } return filter.promise()
        }).done(function (data) {
            //opportunities
            handleOpportunityDetailsResponse(data.response.Opportunities);
            //actions
            handleActionDetailsResponse(data.response.Actions);
            //tours
            handleTourDetailsResponse(data.response.Tours);
            //relationships
            handleRelationshipDetailsResponse(data.response.RelationshipViewModel);
        }).fail(function (error) {
            notifyError(error);
        });

    }
    selfDetails.BundleAjaxCalls(1, 1, 10);
    selfDetails.EmailStatuses = ko.observableArray([
       { Id: 50, Type: '[|Not Verified|]' },
       { Id: 51, Type: '[|Verified|]' },
       { Id: 52, Type: '[|Soft Bounce|]' },
       { Id: 53, Type: '[|Hard Bounce|]' },
       { Id: 54, Type: '[|Unsubscribed|]' },
       { Id: 56, Type: '[|Complained|]' },
       { Id: 57, Type: '[|Suppressed|]' }
    ]);


    selfDetails.InlineEditing = function (field) {

        if (field == 1) {
            $(".person_title").hide();
            $(".name-field").addClass("active");
        }
        else if (field == 2)
            $(".lifecycle-field").addClass("active");
        else if (field == 3) {
            $(".phone-field").addClass("active");
        }
        else if (field == 4) {
            if (selfDetails.Countries().length == 0)
                GetCountries();
            $(".address-field").addClass("active");
        }
        else if (field == 5) {
            $(".leadsource-field").addClass("active");
            $("#leadSourceModal").modal({ show: true, keyboard: false, backdrop: 'static' });
        }
        else if (field == 6)
            $(".email-field").addClass("active");
        else if (field == 7) {
            $(".k-upload-selected").hide();
            $(".image-field").addClass("active");
        }
        else if (field == 8)
            $(".company-field").addClass("active");
        else if (field == 9)
            $(".title-field").addClass("active");
    }

    selfDetails.InlineSaving = function (field) {

        if (field == 1)
            UpdateContactName();
        else if (field == 2)
            UpdateLifeCycleStage();
        else if (field == 3)
            UpdatePhoneNumeber();
        else if (field == 4)
            UpdateAddress()
        else if (field == 5)
            UpdateLeadSource();
        else if (field == 6)
            UpdateEmail();
        else if (field == 7) {
            UpdateImage();
        }
        else if (field == 8)
            UpdateCompanyName();
        else if (field == 9)
            UpdateTitle();
    }

    selfDetails.InlineCancel = function (field) {

        if (field == 1) {
            $(".name-field").removeClass("active");
            $(".person_title").show();
            selfDetails.FirstName(PersonDetails.FirstName == null ? "" : PersonDetails.FirstName);
            selfDetails.LastName(PersonDetails.LastName == null ? "" : PersonDetails.LastName);
            selfDetails.Name(PersonDetails.Name);
            selfDetails.FullName(PersonDetails.FullName);
        }
        else if (field == 2)
            $(".lifecycle-field").removeClass("active");
        else if (field == 3) {
            $(".phone-field").removeClass("active");

            phones_data = PersonDetails.Phones[0];

            selfDetails.Phones()[0].Number(typeof phones_data.Number == "function" ? phones_data.Number() : phones_data.Number);
            selfDetails.Phones()[0].PhoneTypeName(typeof phones_data.PhoneTypeName == "function" ? phones_data.PhoneTypeName() : phones_data.PhoneTypeName);
            selfDetails.Phones()[0].IsPrimary(typeof phones_data.IsPrimary == "function" ? phones_data.IsPrimary() : phones_data.IsPrimary);
            selfDetails.Phones()[0].CountryCode(typeof phones_data.CountryCode == "function" ? phones_data.CountryCode() : phones_data.CountryCode);
            selfDetails.Phones()[0].Extension(typeof phones_data.Extension == "function" ? phones_data.Extension() : phones_data.Extension);

        }
        else if (field == 4) {
            $(".address-field").removeClass("active");
        }
        else if (field == 5)
            $(".leadsource-field").removeClass("active");
        else if (field == 6) {
            var email_data = PersonDetails.Emails[0];
            if (typeof email_data.EmailId == "function")
                selfDetails.Emails()[0].EmailId(email_id.EmailId());
            else
                selfDetails.Emails()[0].EmailId(email_data.EmailId);

            if (typeof email_data.IsPrimary == "function")
                selfDetails.Emails()[0].IsPrimary(email_id.IsPrimary());
            else
                selfDetails.Emails()[0].IsPrimary(email_data.IsPrimary);

            if (typeof email_data.EmailStatusValue == "function")
                selfDetails.Emails()[0].EmailStatusValue(email_id.EmailStatusValue());
            else
                selfDetails.Emails()[0].EmailStatusValue(email_data.EmailStatusValue);

            $(".email-field").removeClass("active");
            $('#inlineSaving').attr('disabled', 'disabled');
        }
        else if (field == 7)
            $(".image-field").removeClass("active");
        else if (field == 8) {
            //if (selfDetails.CompanyName() == "")
            selfDetails.CompanyName(PersonDetails.CompanyName);

            $(".company-field").removeClass("active");
        }

        else if (field == 9) {
            $(".title-field").removeClass("active");
            selfDetails.Title(PersonDetails.Title);
        }
    }

    $("#images").kendoUpload({
        async: {
            saveUrl: "",
            removeUrl: "remove",
            autoUpload: false
        },
        select: onSelect
    });

    function onSelect(e) {
        $('.k-file').hide();
        $(".k-upload-selected").hide();
        var varImageType = "";
        $.each(e.files, function (index, value) {
            var ok = value.extension.toLowerCase() == ".jpg"
                     || value.extension.toLowerCase() == ".jpeg"
                     || value.extension.toLowerCase() == ".png"
                     || value.extension.toLowerCase() == ".bmp";

            if (!ok) {
                e.preventDefault();
                notifyError("[|Please upload jpg, jpeg, png, bmp files|]"); return false;
            }
            else if (bytesToSize(e.files[0].size) > 3) {
                e.preventDefault();
                notifyError("[|Image size should not be more than 3 Mb|]");
                return false;
            }
            else {
            }
            var friendlyname = value.name;

            var fileReader = new FileReader();
            fileReader.onload = function (event) {
                //  self.imagePath(event.target.result);
                var image = document.getElementById("contactimage");
                image.src = event.target.result;
                selfDetails.Image().ImageContent = event.target.result;
                selfDetails.Image().OriginalName = friendlyname;
                selfDetails.Image().ImageType = value.extension.toLowerCase();
                selfDetails.ContactImageUrl('');
            }
            fileReader.readAsDataURL(e.files[0].rawFile);
        });
    }

    function bytesToSize(bytes) {
        return (bytes / (1024 * 1024)).toFixed(2);
    };

    selfDetails.ProfileImage = ko.observable();

    selfDetails.uploadProfileImage = function () {
        var filename = selfDetails.ProfileImage();
        selfDetails.ProfileImageKey = null;
        var extension = filename.replace(/^.*\./, '');

        if (extension.toLowerCase() == "jpeg" || extension.toLowerCase() == "jpg" || extension.toLowerCase() == "png" || extension.toLowerCase() == "bmp") {
            var image = document.getElementById("contactimage");
            image.src = filename;
            selfDetails.Image.ImageContent = filename;
            selfDetails.ContactImageUrl(filename);
        }
        else {
            notifyError("[|Please upload jpg, jpeg, png, bmp files|]");
            return false;
        }
    }

    function UpdateImage() {
        $.ajax({
            url: url + 'UpdateContactImage',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'contactId': selfDetails.ContactID(), 'image': selfDetails.Image(), 'contactImageUrl': selfDetails.ContactImageUrl() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            } return filter.promise()

        }).done(function (data) {
            createCookie('log', false, 1);
            window.location.reload();
        }).fail(function (error) {
            notifyError(error);
        });

    }

    function UpdateEmail() {
		 //adding page load indicator while updating Contact Email by kiran on 18/05/2018 NEXG-3015
        pageLoader();
        var actionurl;

        var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;

        if (selfDetails.Emails()[0].EmailId() == "" || selfDetails.Emails()[0].EmailId() == null) {
            notifyError("[|Email is required.|]");
        }
        else if (re.test(selfDetails.Emails()[0].EmailId())) {
            if (selfDetails.ContactType() == "1")
                actionurl = "UpdateContactEmail";
            else
                actionurl = "UpdateCompanyEmail";


            $.ajax({
                url: url + actionurl,
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({
                    'contactId': selfDetails.ContactID(), 'emailId': selfDetails.Emails()[0].EmailId() == "" ? null : selfDetails.Emails()[0].EmailId(),
                    'emailStatusValue': selfDetails.Emails()[0].EmailStatusValue() == undefined ? null : selfDetails.Emails()[0].EmailStatusValue()
                })
            }).then(function (response) {
                var filter = $.Deferred()
                if (response.success) {
                    filter.resolve(response)
                } else {
                    filter.reject(response.error)
                } return filter.promise()

            }).done(function (data) {
                contactEmailData = data;
                var emails;
                if (data.response.person != null)
                    emails = data.response.person.Emails;
                else if (data.response.company != null)
                    emails = data.response.company.Emails;

                emails = ko.utils.arrayFirst(emails, function (eml) {
                    return eml.IsPrimary == true
                });
                PersonDetails.Emails[0] = emails;
                email_id = emails;

                selfDetails.Emails()[0].EmailId(emails.EmailId);
                selfDetails.Emails()[0].IsPrimary(true);
                selfDetails.Emails()[0].EmailStatusValue(emails.EmailStatusValue);

                $(".email-field").removeClass("active");
                $('#inlineSaving').attr('disabled', 'disabled');
                selfDetails.TimeLineViewModel(new TimeLineViewModel(url, OpportunityUrl, "contacts", PersonDetails.DateFormat, Activities, PersonDetails.ContactID, null));
				//removing page load indicator after update Contact Email by kiran on 18/05/2018 NEXG-3015
				removepageloader();      
			
            }).fail(function (error) {
				 //removing page load indicator when error occur by kiran on 18/05/2018 NEXG-3015
                removepageloader();
                notifyError(error);
            });
        }
        else {
            notifyError("[|Email is invalid.|]");
        }
        $('#inlineSaving').attr('disabled', 'disabled');
    }

    selfDetails.newLeadSourceValueId = ko.observable();
    if (selfDetails.SelectedLeadSource() && selfDetails.SelectedLeadSource().length > 0)
        selfDetails.newLeadSourceValueId(selfDetails.SelectedLeadSource()[0].DropdownValueID());

    function UpdateLeadSource() {
		
		 //adding page load indicator while updating Contact Lead Source by kiran on 18/05/2018 NEXG-3015
        pageLoader();
		
        $.ajax({
            url: url + 'UpdateContactLeadSource',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'contactId': selfDetails.ContactID(), 'contactleadSource': selfDetails.newLeadSourceValueId() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            } return filter.promise()

        }).done(function (data) {
            var newLeadsource = selfDetails.LeadSources().filter(function (e) { return e.DropdownValueID() == selfDetails.newLeadSourceValueId() })[0];
            var existingIndex = selfDetails.SelectedLeadSource() != null ? selfDetails.SelectedLeadSource().map(function (e) { return e.DropdownValueID() == selfDetails.newLeadSourceValueId() }).indexOf(true) : -1;
            if (existingIndex != -1)
                selfDetails.SelectedLeadSource.splice(existingIndex, 1);

            selfDetails.SelectedLeadSource.splice(0, 0, newLeadsource);

            $(".leadsource-field").removeClass("active");
            selfDetails.TimeLineViewModel(new TimeLineViewModel(url, OpportunityUrl, "contacts", PersonDetails.DateFormat, Activities, PersonDetails.ContactID, null));
			//removing page load indicator after update Contact Lead Source by kiran on 18/05/2018 NEXG-3015
            removepageloader();       
	   }).fail(function (error) {
		   //removing page load indicator when error occur by kiran on 18/05/2018 NEXG-3015
                removepageloader();
            notifyError(error);
        });

    }

    function UpdateAddress() {
		 //adding page load indicator while updating Contact Address by kiran on 18/05/2018 NEXG-3015
        pageLoader();
		
        selfDetails.Addresses()[0].AddressTypeID = $("#addressType").val();

        //if (selfDetails.Addresses()[0].Country.Code == "" && selfDetails.Addresses()[0].State.Code == "") {
        //    notifyError("[|An address must have Country and State selected.|]");
        //}
        //else if (selfDetails.Addresses()[0].Country.Code != "" && selfDetails.Addresses()[0].State.Code == "") {
        //    notifyError("[|An address must have State selected.|]");
        //}
        //else if (selfDetails.Addresses()[0].Country.Code == 'US' && selfDetails.Addresses()[0].ZipCode != ""
        //    && selfDetails.Addresses()[0].ZipCode != null
        //    && !(/^([0-9]{5})(?:[-\s]*([0-9]{4}))?$/.test(selfDetails.Addresses()[0].ZipCode))) {
        //        notifyError("[|Please enter valid zipcode|]");
        //}

        //else if (selfDetails.Addresses()[0].Country.Code == 'CA' && selfDetails.Addresses()[0].ZipCode != ""
        //    && selfDetails.Addresses()[0].ZipCode != null
        //    && !(/^([A-Z][0-9][A-Z])\s*([0-9][A-Z][0-9])$/.test(selfDetails.Addresses()[0].ZipCode))) {
        //        notifyError("[|Zipcode is invalid.|]");
        //}
        //else {
        var addresstring = ko.toJSON(selfDetails.Addresses()[0]);
        $.ajax({
            url: url + 'UpdateContactAddresses',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'contactId': selfDetails.ContactID(), 'address': addresstring })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            } return filter.promise()

        }).done(function (data) {

            var address;

            if (data.response.person != null)
                address = data.response.person.Addresses;
            else if (data.response.company != null)
                address = data.response.company.Addresses;
            //   PersonDetails.Addresses selfDetails.Addresses());

            var addresstype = ko.utils.arrayFirst(selfDetails.AddressTypes(), function (type) {
                return type.DropdownValueID() == address[0].AddressTypeID;
            });

            address[0].AddressType = addresstype.DropdownValue();

            selfDetails.Addresses(address);

            $(".address-field").removeClass("active");
            selfDetails.TimeLineViewModel(new TimeLineViewModel(url, OpportunityUrl, "contacts", PersonDetails.DateFormat, Activities, PersonDetails.ContactID, null));
			 	//removing page load indicator after update Contact Address by kiran on 18/05/2018 NEXG-3015
            removepageloader();
        }).fail(function (error) {
			//removing page load indicator when error occur by kiran on 18/05/2018 NEXG-3015
                removepageloader();
            notifyError(error);
        });
        //}
    }

    function UpdatePhoneNumeber() {
		
		 //adding page load indicator while updating Contact Phone Number by kiran on 18/05/2018 NEXG-3015
        pageLoader();
		
        var phones = ko.toJSON(selfDetails.Phones());
        var actionurl;
        if ((selfDetails.Phones()[0].Number() == null || selfDetails.Phones()[0].Number() == "") && ((selfDetails.Phones()[0].Extension() != null && selfDetails.Phones()[0].Extension() != "")
            || (selfDetails.Phones()[0].CountryCode() != null && selfDetails.Phones()[0].CountryCode() != ""))) {
            notifyError("[|Phone number is Invalid.|]");
            return;
        }
        if (selfDetails.Phones()[0].Number() != null && selfDetails.Phones()[0].Number().length > 0) {
            var numbers = selfDetails.Phones()[0].Number().match(/\d/g);
            if (numbers != null && numbers != 'undefined')
                if (numbers.length != 10) {
                    notifyError("[|Phone Number length is Invalid|]");
                    return;
                }
        }
        if (selfDetails.Phones()[0].CountryCode() != null && selfDetails.Phones()[0].CountryCode().length > 0) {
            var numbers = selfDetails.Phones()[0].CountryCode().match(/\d/g);
            var alpha = selfDetails.Phones()[0].CountryCode().match(/[a-z]/i);
            if ((numbers != null && numbers != 'undefined') || (alpha != null && alpha != 'undefined'))
                if ((numbers != null && numbers.length < 1) || alpha) {
                    notifyError("[|Country Code is Invalid|]");
                    isVaidPhoneNumber = false;
                    return false;
                }
        }
        if (selfDetails.Phones()[0].Number() != "" && selfDetails.Phones()[0].Number() != null) {

            if (selfDetails.ContactType() == "1")
                actionurl = "UpdateContactPhoneNumber";
            else
                actionurl = "UpdateCompanyPhoneNumber";

            $.ajax({
                url: url + actionurl,
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'contactId': selfDetails.ContactID(), 'phoneType': selfDetails.Phones()[0].PhoneType(), 'phoneNumber': selfDetails.Phones()[0].Number(), 'countryCode': selfDetails.Phones()[0].CountryCode(), 'extension': selfDetails.Phones()[0].Extension() })
            }).then(function (response) {
                var filter = $.Deferred()
                if (response.success) {
                    filter.resolve(response)
                } else {
                    filter.reject(response.error)
                } return filter.promise()

            }).done(function (data) {
                var phone;

                if (data.response.person != null)
                    phone = data.response.person.Phones;
                else if (data.response.company != null)
                    phone = data.response.company.Phones;

                phone = ko.utils.arrayFirst(phone, function (phn) {
                    return phn.IsPrimary == true
                });

                PersonDetails.Phones[0] = phone;

                phone_number = phone;
                phone_saved = true;
                selfDetails.Phones()[0].Number(phone.Number);
                selfDetails.Phones()[0].PhoneTypeName(phone.PhoneTypeName);
                selfDetails.Phones()[0].IsPrimary(true);

                $(".phone-field").removeClass("active");
                selfDetails.TimeLineViewModel(new TimeLineViewModel(url, OpportunityUrl, "contacts", PersonDetails.DateFormat, Activities, PersonDetails.ContactID, null));
           	//removing page load indicator after update Contact Phone Number by kiran on 18/05/2018 NEXG-3015
            removepageloader();
		   }).fail(function (error) {
				 //removing page load indicator when error occur by kiran on 18/05/2018 NEXG-3015
                removepageloader();
                notifyError(error);
            });
        }
        else {
            notifyError("[|Phone number is required.|]");
        }

    }

    function UpdateLifeCycleStage() {
		pageLoader(); //NEXG-3015
        $.ajax({
            url: url + 'UpdateContactLifecycleStage',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'contactId': selfDetails.ContactID(), 'lifecycleStage': selfDetails.LifecycleStage(), 'previousLifecyclestage': selfDetails.PreviousLifecycleStage() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            } return filter.promise()
        }).done(function (data) {
            var lifecycle = ko.utils.arrayFirst(selfDetails.LifecycleStages(), function (type) {
                return type.DropdownValueID() == selfDetails.LifecycleStage();
            });
            selfDetails.LifeCycle(lifecycle.DropdownValue());
            selfDetails.PreviousLifecycleStage(lifecycle.DropdownValueID());

            $(".lifecycle-field").removeClass("active");

            selfDetails.TimeLineViewModel(new TimeLineViewModel(url, OpportunityUrl, "contacts", PersonDetails.DateFormat, Activities, PersonDetails.ContactID, null));
			removepageloader(); //NEXG-3015
        }).fail(function (error) {
			removepageloader(); // NEXG-3015
            notifyError(error);
        });

    }

    function UpdateContactName() {
		
		 //adding page load indicator while updating Contact firstName and LastName by kiran on 18/05/2018 NEXG-3015
          pageLoader();
		
        $.ajax({
            url: url + 'UpdateContactName',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'contactId': selfDetails.ContactID(), 'firstName': selfDetails.FirstName(), 'lastName': selfDetails.LastName() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            } return filter.promise()
        }).done(function (data) {
            selfDetails.Name(data.response.person.Name);
            PersonDetails.Name = data.response.person.Name;
            // selfDetails.Name((data.response.person.FirstName == null ? "" : data.response.person.FirstName) + " " + (data.response.person.LastName == null ? "" : data.response.person.LastName));

            PersonDetails.FirstName = data.response.person.FirstName == null ? "" : data.response.person.FirstName;
            PersonDetails.LastName = data.response.person.LastName == null ? "" : data.response.person.LastName;

            $(".name-field").removeClass("active");
            $(".person_title").show();
            selfDetails.TimeLineViewModel(new TimeLineViewModel(url, OpportunityUrl, "contacts", PersonDetails.DateFormat, Activities, PersonDetails.ContactID, null));
			//removing page load indicator after update Contact FirstName and LastName by kiran on 18/05/2018 NEXG-3015
            removepageloader();

        }).fail(function (error) {
			 //removing page load indicator when error occur by kiran on 18/05/2018 NEXG-3015
              removepageloader();
            notifyError(error);
        });
    }

    function UpdateTitle() {
        $.ajax({
            url: url + 'UpdateContactTitle',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'contactId': selfDetails.ContactID(), 'title': selfDetails.Title() })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            } return filter.promise()
        }).done(function (data) {

            selfDetails.Title(data.response.person.Title);

            PersonDetails.Title = data.response.person.Title;

            $(".title-field").removeClass("active");

            selfDetails.TimeLineViewModel(new TimeLineViewModel(url, OpportunityUrl, "contacts", PersonDetails.DateFormat, Activities, PersonDetails.ContactID, null));

        }).fail(function (error) {
            notifyError(error);
        });

    }

    function UpdateCompanyName() {

        if (selfDetails.CompanyName() == "") {
            notifyError("Company name is required");
        }
        else {
            $.ajax({
                url: url + 'UpdateCompanyName',
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'contactId': selfDetails.ContactID(), 'companyName': selfDetails.CompanyName() })
            }).then(function (response) {
                var filter = $.Deferred()
                if (response.success) {
                    filter.resolve(response)
                } else {
                    filter.reject(response.error)
                } return filter.promise()
            }).done(function (data) {

                PersonDetails.CompanyName = data.response.company.CompanyName;

                $(".company-field").removeClass("active");

                selfDetails.TimeLineViewModel(new TimeLineViewModel(url, OpportunityUrl, "contacts", PersonDetails.DateFormat, Activities, PersonDetails.ContactID, null));

            }).fail(function (error) {
                notifyError(error);
            });
        }

    }

    selfDetails.GetDate = function (date, isdate) {

        var pattern = /Date\(([^)]+)\)/;
        var results = pattern.exec(date);
        var dt = new Date(parseFloat(results[1]));
        var tourday = kendo.toString(dt, "dd");
        if (isdate)
            return tourday;
        else
            return kendo.toString(dt, "MMM");
    }

    selfDetails.IsExpired = function (tourdate) {

        var pattern = /Date\(([^)]+)\)/;
        var results = pattern.exec(tourdate);
        var dt = new Date(parseFloat(results[1]));

        if (new Date() > new Date(parseFloat(results[1])))
            return "in-completed";
        else
            return "";
    }

    selfDetails.GetCountOfTours = function () {
        return selfDetails.Tours().length;
    }

    function veCarousel(vecarouselid) {
        jQuery('#' + vecarouselid).jcarousel({
            vertical: true,
            scroll: 2
        });
    }

    selfDetails.makeImageObservable = function () {
        selfDetails.imagePath = ko.observable();
        selfDetails.Image = ko.observable(PersonDetails.Image);
        selfDetails.imagePath(selfDetails.Image().ImageContent)
    };

    selfDetails.ContactIndex = ko.observable();
    selfDetails.TotalHits = ko.observable();

    selfDetails.CampaignsSummary = ko.observable(
{
    Sent: ko.observable(""), Delivered: ko.observable(""), Opened: ko.observable("-"), Clicked: ko.observable("-")
});

selfDetails.EmailsSummary = ko.observable(
{
   Delivered: ko.observable(""), Opened: ko.observable("-"), Clicked: ko.observable("-")
});
    selfDetails.PhoneTypes = ko.observableArray(PersonDetails.PhoneTypes);
    var defaultPhoneTypeIndex = ko.utils.arrayFirst(selfDetails.PhoneTypes(), function (type) {
        return type.IsDefault === true;
    });
    //selfDetails.Phones = ko.observableArray(PersonDetails.Phones);
    //$.each(selfDetails.Phones(), function (i, phone) {
    //    var type = ko.utils.arrayFirst(selfDetails.PhoneTypes(), function (type) {
    //        return type.DropdownValueID == phone.PhoneType;
    //    });
    //    if (type)
    //        phone.PhoneType = ko.observable(phone.PhoneType);
    //    else
    //        phone.PhoneType = ko.observable(defaultPhoneTypeIndex.DropdownValueID);

    //    phone.Number = ko.observable(phone.Number);
    //    phone.PhoneTypeName = ko.observable(phone.PhoneTypeName);
    //    phone.CountryCode = ko.observable(phone.CountryCode);
    //    phone.Extension = ko.observable(phone.Extension);
    //    phone.IsPrimary = ko.observable(phone.IsPrimary);
    //    phone.ContactPhoneNumberID = ko.observable(phone.ContactPhoneNumberID);
    //});
    selfDetails.NumberOfWon = ko.observable();
    selfDetails.ValueOfWon = ko.observable();
    selfDetails.NumberOfPotential = ko.observable();
    selfDetails.ValueOfPotential = ko.observable();
    selfDetails.NumberOfPotentialOpportunities = ko.observableArray();
    selfDetails.NumberOfWonOpportunities = ko.observableArray();
    selfDetails.Actions = ko.observableArray();
    selfDetails.Tours = ko.observableArray();
    selfDetails.Opportunities = ko.observableArray();
    selfDetails.RelationshipViewModel = ko.observableArray();
    selfDetails.LeadScore = ko.observable(0);
    selfDetails.Countries = ko.observableArray();
    selfDetails.States = ko.observableArray();
    selfDetails.WorkflowsCount = ko.observable();
    selfDetails.CampaignsCount = ko.observable();
    selfDetails.EmailsCount = ko.observable();

    selfDetails.WonSelect = function (value) {
        localStorage.setItem("WonOpportunities", value);
    }
    selfDetails.PotentialSelect = function (value) {
    }

    selfDetails.CurrentTabView = ko.observable(0);
    selfDetails.showCurrentTab = function (tabIndex) {
        selfDetails.CurrentTabView(tabIndex);
    }

    selfDetails.CustomFields = ko.observableArray(PersonDetails.CustomFields);

    selfDetails.RenderCustomFields = ko.observable(false);
    selfDetails.RenderCustomFields.subscribe(function () {
        $.each(selfDetails.CustomFieldTabs(), function (index, tab) {
            $.each(tab.Sections(), function (sectionIndex, section) {
                $.each(section.CustomFields(), function (fieldIndex, field) {
                    var customFieldIndex = selfDetails.CustomFields().map(function (e) { return e.CustomFieldId; }).indexOf(parseInt(field.FieldId()));
                    field.EditMode = ko.observable(false);
                    field.TabIndex = index;
                    field.SectionIndex = sectionIndex;
                    field.FieldIndex = fieldIndex;
                    var data;
                    if (field.Value()) {
                        data = field.Value().replace(/\\n/g, "<br\>");
                    }
                    field.Value = ko.observable(data);
                    field.Title(field.Title())
                    if (customFieldIndex >= 0) {
                        if (field.FieldInputTypeId() == DATATYPE_DATE) {
                            var dateFormat = readCookie("dateformat").toUpperCase();
                            if (selfDetails.CustomFields()[customFieldIndex].Value != "")
                                field.Value(kendo.toString(new Date(moment(selfDetails.CustomFields()[customFieldIndex].Value_Date).toDate()).ToUtcUtzDate(), readCookie("dateformat")));
                        }
                        else if (field.FieldInputTypeId() == DATATYPE_TIME) {
                            field.Value(kendo.toString(new Date(moment(selfDetails.CustomFields()[customFieldIndex].Value_Date).toDate()).ToUtcUtzDate(), "t"));
                        }
                        else
                        {
                            var data1;
                            if (selfDetails.CustomFields()[customFieldIndex].Value) {
                                data1 = selfDetails.CustomFields()[customFieldIndex].Value.replace(/\\n/g, "<br />");
                            }
                            field.Value(data1);
                        }
                    }
                    else {
                        field.Value("");
                    }
                })
            })
        })
    });

    var hasCustomFieldsFetched = false;
    selfDetails.GetCustomFieldTabs = function () {
        if (!hasCustomFieldsFetched) {
            selfDetails.getCustomFieldTabs();
            hasCustomFieldsFetched = true
        }
    }

    selfDetails.getCustomFieldTabs = function () {
        innerLoader('customfields-body');
        $.ajax({
            url: url + 'GetCustomFieldTabs',
            type: 'get',
            dataType: 'json',
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
            selfDetails.CustomFieldTabs = ko.mapping.fromJS(data.response.CustomFieldTabs);
            selfDetails.RenderCustomFields(true);
            selfDetails.CustomFieldsManager = new customFieldsManagerViewModel(selfDetails, selfDetails.CustomFieldTabs, selfDetails.ContactID(), url);
            selfDetails.CustomFieldsManager.CalculateDimensions();

        }).fail(function (error) {
            notifyError(error);
        });

    }


    selfDetails.customValueOptions = function (valueoptions, value) {
        var valueType = Object.prototype.toString.call(value);
        var selectedvalues;
        if (valueType == "[object Array]")
            selectedvalues = value;
        else
            selectedvalues = value.split('|');
        var multiplevalues = "";
        for (var i = 0; i < valueoptions.length; i++) {
            for (var g = 0; g < selectedvalues.length; g++) {
                if (selectedvalues[g] == valueoptions[i].CustomFieldValueOptionId()) {
                    multiplevalues += valueoptions[i].Value() + ", ";
                }
            }
        }
        multiplevalues = multiplevalues.substring(0, multiplevalues.length - 2);
        return multiplevalues;
    }

    selfDetails.Addresses = ko.observableArray(PersonDetails.Addresses);
    selfDetails.DefaultAddress = ko.computed({
        read: function () {
            var defaultAddress;
            $.each(selfDetails.Addresses(), function (index, value) {
                if (value.IsDefault == true) {
                    defaultAddress = new Object();
                    defaultAddress.Stringified = AddressToString(value);
                    defaultAddress.AddressType = value.AddressType;
                    defaultAddress.AddressLine1 = value.AddressLine1;
                    defaultAddress.City = value.City;
                    defaultAddress.State = value.State.Name;
                    defaultAddress.Zipcode = value.ZipCode;
                }
            });

            return defaultAddress;
        }
    });

    function GetCountries() {
        $.ajax({
            url: url + 'GetCountries',
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8"
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {

            selfDetails.Countries(data.response);
            var code = selfDetails.Addresses()[0].Country.Code();
            selfDetails.Addresses()[0].Country.Code("");
            selfDetails.Addresses()[0].Country.Code(code);

        }).fail(function (error) {
            notifyError(error);
        });
    }

    //Gets states of the selected country.
    selfDetails.countryChanged = function (address) {
        var countryCode = "";
        if (ko.isObservable(selfDetails.Addresses()[0].Country.Code))
            countryCode = selfDetails.Addresses()[0].Country.Code();
        else
            countryCode = selfDetails.Addresses()[0].Country.Code;
        if (countryCode != "") {
            $.ajax({
                url: url + 'GetStates',
                type: 'get',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: { 'countryCode': countryCode }
            }).then(function (response) {
                var filter = $.Deferred()
                if (response.success) {
                    filter.resolve(response)
                } else {
                    filter.reject(response.error)
                }
                return filter.promise()
            }).done(function (data) {
                selfDetails.States(data.response);
                var code = address.State.Code();
                address.State.Code("");
                address.State.Code(code);

            }).fail(function (error) {
                notifyError(error);
            });
        } else {
            selfDetails.States([]);
        }
    };

    $.each(selfDetails.Addresses(), function (index, address) {
        address.AddressTypeID = ko.observable(address.AddressTypeID);
        address.States = ko.observableArray(address.States);
        address.Country.Code = ko.observable(address.Country.Code);
        address.State.Code = ko.observable(address.State.Code);
        address.IsDefault = ko.observable(address.IsDefault);
    });

    selfDetails.countryChanges = function () {
        selfDetails.Addresses()[0].Country.Code = $("#country").val();
        selfDetails.Addresses()[0].State.Code = "";
        selfDetails.countryChanged(selfDetails.Addresses()[0]);
    }
    selfDetails.stateChange = function () {
        selfDetails.Addresses()[0].State.Code = $("#state").val();
    }
    selfDetails.countryChanged(selfDetails.Addresses()[0]);

    ko.bindingHandlers.KoAfterRender = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
            // This will be called when the binding is first applied to an element
            // Set up any initial state, event handlers, etc. here
            $(element).popover();
        }
    };

    selfDetails.DisplayAddresses = ko.computed({
        read: function () {
            var addresses = [];
            $.each(selfDetails.Addresses(), function (index, address) {
                var addresstoString = AddressToString(address);
                address.ToString = (addresstoString.trim() == "-" ? "" : addresstoString);
            })
        },
        write: function () {
        }
    });

    selfDetails.EnablePrevious = ko.pureComputed(function () {
        if (selfDetails.ContactIndex() <= 1)
            return false;
        else
            return true;
    });

    selfDetails.EnableNext = ko.pureComputed(function () {
        if (selfDetails.ContactIndex() >= selfDetails.TotalHits())
            return false;
        else
            return true;
    });

    selfDetails.HideNavigation = ko.pureComputed(function () {
        if (selfDetails.ContactIndex() == 0 || selfDetails.TotalHits() == 1)
            return true;
        else
            return false;
    });

    selfDetails.makeImageObservable();
    selfDetails.ContactTags = ko.pureComputed({
        read: function () {
            if (selfDetails.TagsList() != null)
                return selfDetails.TagsList().join(",");
            else
                return "";
        },
        write: function (newValue) {
        },
        owner: this
    });

    ko.bindingHandlers.formatNumberText = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            var computed = ko.pureComputed(function () {
                var number = ko.utils.unwrapObservable(valueAccessor());
                if (typeof number != "undefined") {
                    return "(" + number.substr(0, 3) + ")" + " " + number.substr(3, 3) + "-" + number.substr(6);
                }
                return number;
            });
            ko.applyBindingsToNode(element, {
                text: computed()
            });
        }
    };

    selfDetails.AttachmentViewModel = ko.observable(new AttachmentViewModel(CommunitcationURL, "contacts", PersonDetails.DateFormat, PersonDetails.ContactID, null));

    selfDetails.FormatPhone = function (s) {
        return formatPhone(s);
    }

    //var st = '01985623145278956';
    //var numb = st.startsWith('01');// || string.startsWith('1') || string.startsWith('0'));

    function formatPhone(phonenum) {
        var regexObj = /^(?:\+?1[-. ]?)?(?:\(?([0-9]{3})\)?[-. ]?)?([0-9]{3})[-. ]?([0-9]{4})$/;
        if (regexObj.test(phonenum)) {
            var parts = phonenum.match(regexObj);
            var phone = "";
            if (parts[1]) {
                phone += "(" + parts[1] + ") ";
            }
            phone += parts[2] + "-" + parts[3];
            return phone;
        }
        else {
            return phonenum;
        }
    }

    function formatPostalcode(pcode) {
        var regexObj = /^\s*([a-ceghj-npr-tvxy]\d[a-ceghj-npr-tv-z])(\s)?(\d[a-ceghj-npr-tv-z]\d)\s*$/i
        if (regexObj.test(pcode)) {
            var parts = pcode.match(regexObj);
            var pc = parts[1] + " " + parts[3];
            return pc.toUpperCase();
        }
        else {
            return pcode;
        }
    }

    selfDetails.PrimaryPhone = ko.pureComputed(function () {
        var primaryPhone = ko.utils.arrayFirst(selfDetails.Phones(), function (phone) {
            return phone.IsPrimary() == true
        });
        return primaryPhone;
    });

    var phone_number = ko.utils.arrayFirst(selfDetails.Phones(), function (phone) {
        return phone.IsPrimary() == true
    });

    var phone_saved = false;

    var email_id = ko.utils.arrayFirst(selfDetails.Emails(), function (eml) {
        return eml.IsPrimary() == true
    });

    selfDetails.getEmail = function (EmailId, contactID) {

        var name = "";
        //  var Emailformate = "";
        if (selfDetails.Name == undefined) {
            //  var Text = selfDetails.CompanyName() + "(" + EmailId + ")*";
            name = encodeURIComponent(selfDetails.CompanyName());
            // Emailformate = encodeURIComponent(EmailId);
        }
        else {
            //  var Text = selfDetails.Name() + "(" + EmailId + ")*";
            name = encodeURIComponent(selfDetails.Name());
            // Emailformate = encodeURIComponent(EmailId);
        }
        return url + "_ContactSendMailModel?contactName=" + name + "&emailID=" + EmailId + "&contactID=" + contactID;
    };

    selfDetails.Actions = ko.observableArray(PersonDetails.Actions);
    selfDetails.ActionCompletedMessage = ko.observable();
    selfDetails.CompletedActionOption = ko.observable();
    selfDetails.ActionContactsCount = ko.observable();
    selfDetails.TourContactsCount = ko.observable();

    selfDetails.MarkAsCompleteActionId = ko.observable();
    selfDetails.MarkAsCompleteStatus = ko.observable();
    selfDetails.ActionOpportunityId = ko.observable();
    selfDetails.ActionTypeValue = ko.observable();
    selfDetails.ActionDateOn = ko.observable();
    selfDetails.ToSend = ko.observable(false);
    selfDetails.ActionCompleted = ko.observable();
    selfDetails.IsTourCompleted = ko.observable();
    selfDetails.MailBulkId = ko.observable();
    selfDetails.GROUPID = ko.observable();
    selfDetails.AddNoteSummary = ko.observable(false);

    var now = new Date().toUtzDate();
    selfDetails.utc_now = ko.observable(new Date(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate(), now.getUTCHours(), now.getUTCMinutes(), now.getUTCSeconds(), now.getUTCMilliseconds()));

    selfDetails.CompletedOperation = function (Action) {
        var dateString = Action.ActionDate.toString();
        if (dateString.indexOf('/Date') == 0) {
            var utzdate = ConvertToDate(Action.ActionDate).toUtzDate();
            selfDetails.ActionDateOn(utzdate);
        }
        else {
            var date = Date.parse(Action.ActionDate);
            selfDetails.ActionDateOn(ConvertToDate(date.toString()));
        }
        selfDetails.ActionTypeValue(Action.ActionTypeValue);
        selfDetails.MailBulkId(Action.MailBulkId);
        selfDetails.ActionCompleted(Action.IsCompleted);
        selfDetails.GROUPID(Action.GROUPID);
        var action = "ActionCompletedOperation";
        var url = "/Contact/";
        $.ajax({
            url: url + action,
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                'actionID': Action.ActionId
            })
        }).then(function (response) {
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response)
            }
            else {
                filter.reject(response.error);
            }
            return filter.promise()
        }).done(function (data) {
            if (data.response.Count == 1 || data.response.SelectAll == true) {
                if (Action.IsCompleted) {
                    selfDetails.ActionCompletedMessage("[|Are you sure you want to mark this Action as Completed|]?");
                    selfDetails.CompletedActionOption("[|Mark as Completed|]");
                }
                else {
                    selfDetails.ActionCompletedMessage("[|Are you sure you want to mark this Action as Not Completed|]?");
                    selfDetails.CompletedActionOption("[|Mark as Not Complete|]");
                }
                selfDetails.ActionContactsCount(1);
            }
            else if (data.response.Count > 1) {
                if (Action.IsCompleted)
                    selfDetails.ActionCompletedMessage("[|More than one Contact is included for this Action, How do you want to Complete this Action|]?");
                else
                    selfDetails.ActionCompletedMessage("[|More than one Contact tagged for this Action, are you sure you want to mark this as Not Complete|]?");
                selfDetails.ActionContactsCount(2);
            }
            selfDetails.ActionOpportunityId(Action.OppurtunityId);
            selfDetails.MarkAsCompleteActionId(Action.ActionId);
            selfDetails.MarkAsCompleteStatus(Action.IsCompleted);
            $('#myModal').modal('show');
        }).fail(function (error) {
            if (error == undefined)
                $('#actioncomptrigger').modal('toggle');
            else
                notifyError(error);
        });
        return false;
    };

    selfDetails.TourCompletedOperation = function (Tour) {
        var action = "TourCompletedOperation";
        var url = "/Contact/";
        selfDetails.IsTourCompleted(Tour.IsCompleted);
        $.ajax({
            url: url + action,
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                'tourID': Tour.TourID
            })
        }).then(function (response) {
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response)
            }
            else {
                filter.reject(response.error);
            }
            return filter.promise()
        }).done(function (data) {
            if (data.response.Count == 1 || data.response.SelectAll == true) {
                if (Tour.IsCompleted) {
                    selfDetails.ActionCompletedMessage("[|Are you sure you want to mark this tour as Completed|]?");
                    selfDetails.CompletedActionOption("[|Mark as Completed|]");
                }
                else {
                    selfDetails.ActionCompletedMessage("[|Are you sure you want to mark this tour as Not Completed|]?");
                    selfDetails.CompletedActionOption("[|Mark as Not Complete|]");
                }
                selfDetails.TourContactsCount(1);
            }
            else if (data.response.Count > 1) {
                if (Tour.IsCompleted)
                    selfDetails.ActionCompletedMessage("[|More than one Contact tagged for this Tour, are you sure you want to mark this as Complete|]?");
                else
                    selfDetails.ActionCompletedMessage("[|More than one Contact tagged for this Tour, are you sure you want to mark this as Not Complete|]?");
                selfDetails.TourContactsCount(2);
            }
            selfDetails.ActionOpportunityId(Tour.OppurtunityId);
            selfDetails.MarkAsCompleteActionId(Tour.TourID);
            selfDetails.MarkAsCompleteStatus(Tour.IsCompleted);
            $('#myModal').modal('show');
        }).fail(function (error) {
            if (error == undefined)
                $('#tourcomptrigger').modal('toggle');
            else
                notifyError(error);
        });
        return false;
    };

    selfDetails.TourCompletedOperation_TourEdit = function (Tour) {
        var action = "TourCompletedOperation";
        var url = "/Contact/";
        selfDetails.IsTourCompleted(Tour.IsCompleted());
        $.ajax({
            url: url + action,
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                'tourID': Tour.TourID()
            })
        }).then(function (response) {
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response)
            }
            else {
                filter.reject(response.error);
            }
            return filter.promise()
        }).done(function (data) {
            if (data.response.Count == 1) {
                if (Tour.IsCompleted()) {
                    selfDetails.ActionCompletedMessage("[|Are you sure you want to mark this tour as Completed|]?");
                    selfDetails.CompletedActionOption("[|Mark as Completed|]");
                }
                else {
                    selfDetails.ActionCompletedMessage("[|Are you sure you want to mark this tour as Not Completed|]?");
                    selfDetails.CompletedActionOption("[|Mark as Not Complete|]");
                }
                selfDetails.TourContactsCount(1);
            }
            else if (data.response.Count > 1) {
                if (Tour.IsCompleted())
                    selfDetails.ActionCompletedMessage("[|More than one Contact tagged for this Tour, are you sure you want to mark this as Complete|]?");
                else
                    selfDetails.ActionCompletedMessage("[|More than one Contact tagged for this Tour, are you sure you want to mark this as Not Complete|]?");
                selfDetails.TourContactsCount(2);
            }
            //  selfDetails.ActionOpportunityId(Tour.OppurtunityId());
            selfDetails.MarkAsCompleteActionId(Tour.TourID());
            selfDetails.MarkAsCompleteStatus(Tour.IsCompleted());
            $('#myModal').modal('show');
        }).fail(function (error) {
            notifyError(error);
        });
        return false;
    };

    selfDetails.CompletedOperation_ActionEdit = function (Action) {
        var dateString = Action.ActionDate().toString();
        if (dateString.indexOf('/Date') == 0) {
            var utzdate = ConvertToDate(Action.ActionDate()).toUtzDate();
            selfDetails.ActionDateOn(utzdate);
        }
        else {
            var date = Date.parse(Action.ActionDate());
            selfDetails.ActionDateOn(ConvertToDate(date.toString()));
        }
        selfDetails.ActionTypeValue(Action.ActionTypeValue());
        selfDetails.MailBulkId(Action.MailBulkId());
        selfDetails.ActionCompleted(Action.IsCompleted());

        var action = "ActionCompletedOperation";
        var url = "/Contact/";
        $.ajax({
            url: url + action,
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                'actionID': Action.ActionId()
            })
        }).then(function (response) {
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response)
            }
            else {
                filter.reject(response.error);
            }
            return filter.promise()
        }).done(function (data) {
            if (data.response.Count == 1) {
                if (Action.IsCompleted()) {
                    selfDetails.ActionCompletedMessage("[|Are you sure you want to mark this Action as Completed|]?");
                    selfDetails.CompletedActionOption("[|Mark as Completed|]");
                }
                else {
                    selfDetails.ActionCompletedMessage("[|Are you sure you want to mark this Action as Not Completed|]?");
                    selfDetails.CompletedActionOption("[|Mark as Not Complete|]");
                }
                selfDetails.ActionContactsCount(1);
            }
            else if (data.response.Count > 1) {
                if (Action.IsCompleted())
                    selfDetails.ActionCompletedMessage("[|More than one Contact is included for this Action, How do you want to Complete this Action|]?");
                else
                    selfDetails.ActionCompletedMessage("[|More than one Contact tagged for this Action, are you sure you want to mark this as Not Complete|]?");
                selfDetails.ActionContactsCount(2);
            }
            //  selfDetails.ActionOpportunityId(Action.OppurtunityId);
            selfDetails.MarkAsCompleteActionId(Action.ActionId());
            selfDetails.MarkAsCompleteStatus(Action.IsCompleted());
            $('#myModal').modal('show');
        }).fail(function (error) {
            notifyError(error);
        });
        return false;
    };

    selfDetails.Completed = function (message) {

        var forAll = true;
        if (message == 'one')
            forAll = false;
        var action = "ActionCompleted";
        var url = "/Contact/";
        pageLoader();
        $.ajax({
            url: url + action,
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                'actionID': selfDetails.MarkAsCompleteActionId(),
                'isActionCompleted': selfDetails.MarkAsCompleteStatus(),
                'contactId': PersonDetails.ContactID,
                'completedForAll': forAll,
                'opportunityId': 0,
                'isSchedule': selfDetails.ToSend(),
                'mailBulkId': selfDetails.MailBulkId(),
                'AddToNoteSummary': selfDetails.AddNoteSummary()
            })
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
            removepageloader();
            if (selfDetails.MarkAsCompleteStatus()) {
                notifySuccess('[|Action marked as Completed.|]');
                $('#myModal').modal('hide');
            }
            else {
                notifySuccess('[|Action marked as Not Completed.|]');
                $('#myModal').modal('hide');
            }
            if (typeof selfAction != 'undefined')
                selfAction.updatingAction();
            GetActionDetails();
            selfDetails.TimeLineViewModel(new TimeLineViewModel(url, OpportunityUrl, "contacts", PersonDetails.DateFormat, Activities, PersonDetails.ContactID, null));
        }).fail(function (error) {
            removepageloader();
            notifyError(error);
            GetActionDetails();
        });
        return true;
    };

    selfDetails.getTimeLine = function () {
        selfDetails.TimeLineViewModel(new TimeLineViewModel(url, OpportunityUrl, "contacts", PersonDetails.DateFormat, Activities, PersonDetails.ContactID, null));
    }
    selfDetails.TourCompleted = function (message) {

        var forAll = true;
        if (message == 'one')
            forAll = false;
        var action = "TourCompleted";
        pageLoader();
        $.ajax({
            url: url + action,
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                'tourID': selfDetails.MarkAsCompleteActionId(),
                'isTourCompleted': selfDetails.MarkAsCompleteStatus(),
                'contactId': PersonDetails.ContactID,
                'completedForAll': forAll,
                'addToContactSummary': selfDetails.AddNoteSummary()
            })
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
            removepageloader();
            if (selfDetails.MarkAsCompleteStatus()) {
                notifySuccess('[|Tour marked as Completed.|]');
                $('#myModal').modal('hide');
            }
            else {
                notifySuccess('[|Tour marked as Not Completed.|]');
                $('#myModal').modal('hide');
            }

            if (typeof selfTour != 'undefined')
                selfTour.updatingtour();
            GetTourDetails();
            selfDetails.TimeLineViewModel(new TimeLineViewModel(url, OpportunityUrl, "contacts", PersonDetails.DateFormat, Activities, PersonDetails.ContactID, null));
        }).fail(function (error) {
            removepageloader();
            notifyError(error);
            GetTourDetails();
        });
        return true;
    };

    selfDetails.CloseWindowFunction = function (value) {
        if (value == 'A')
            GetActionDetails();
        else if (value == 'T')
            GetTourDetails();
        selfDetails.ActionContactsCount("");
        selfDetails.TourContactsCount("");
        $('#myModal').modal('hide');
    }

    selfDetails.ActionCancel = function () {
        var selectedAction = ko.utils.arrayFirst(selfDetails.Actions(), function (action) {
            return action.ActionId == selfDetails.MarkAsCompleteActionId();
        });
        if (selfDetails.MarkAsCompleteStatus()) {
            selectedAction.IsCompleted = false;
            $('#' + selfDetails.MarkAsCompleteActionId()).removeClass('checked');
        }
        else if (!selfDetails.MarkAsCompleteStatus()) {
            selectedAction.IsCompleted = true;
            $('#' + selfDetails.MarkAsCompleteActionId()).addClass('checked');
        }
    };

    selfDetails.DisplayWithCurrency = function (potential) {
        var currencyFormat = readCookie("currencyformat");
        if (currencyFormat == "$X,XXX.XX") {
            kendo.culture("en-US");
            return "$" + kendo.toString(potential, 'n');
        } else if (currencyFormat == "X XXX,XX $") {
            kendo.culture("fr-FR");
            return kendo.toString(potential, 'n') + " $";
        } else if (currencyFormat == "B/.X,XXX.XX") {
            kendo.culture("en-US");
            return "B/." + kendo.toString(potential, 'n');
        }
    }

    selfDetails.DisplaywithDateFormat = function (date) {
        var dateFormat = readCookie("dateformat").toUpperCase();
        if (date == null) {
            return "";
        }
        return moment(date).format(dateFormat);
    };

    selfDetails.DisplaywithDateTimeFormat = function (date) {
        var dateFormat = readCookie("dateformat").toUpperCase();
        if (date == null) {
            return "";
        }
        var utzDate = new Date(moment(date).toDate()).ToUtcUtzDate();
        return moment(utzDate).format(dateFormat + " hh:mm A");
    };

    selfDetails.DeleteOpportunityContactMap = function (opportunity) {
        console.log(opportunity.OpportunityContactMapID);
        alertifyReset("Delete Buyer", "Cancel");
        var confirmMesaage = "[|Are you sure you want to delete this buyer|]?";
        commonbuyerdelete(confirmMesaage, opportunity.OpportunityContactMapID);

    };

    function commonbuyerdelete(message, buyerId) {
        alertify.confirm(message, function (e) {
            if (e) {
                jQuery.ajaxSettings.traditional = true;
                $.ajax({
                    url: '/Opportunities/DeleteOpportunityBuyer',
                    type: 'POST',
                    dataType: 'json',
                    data: JSON.stringify({ 'buyerId': buyerId }),
                    contentType: 'application/json; charset=utf-8'
                }).then(function (response) {
                    var filter = $.Deferred()
                    if (response.success) {
                        filter.resolve(response)
                    }
                    else {
                        filter.reject(response.error)
                    }
                    return filter.promise()
                }).done(function (data) {
                    notifySuccess("[|Successfully deleted the buyer|]");
                    createCookie('log', false, 1);
                    window.location.reload(true);
                }).fail(function (error) {
                    notifyError(error);
                })
            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }
        });
    }

    selfDetails.DeleteAction = function (Action) {
        var action = "GetActionContactsCount";
        $.ajax({
            url: url + action,
            type: 'post',
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({
                'actionId': Action.ActionId
            })
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
            if (data.response.Count == 1 || data.response.SelectAll) {
                DeleteConfirmation('OK', "Cancel", "[|You’re about to delete this action. Are you sure you want to delete|]?", Action)
            }
            else {
                DeleteConfirmation("OK", "Cancel", "[|More than one Contact is included for this Action,|]" + "</br>" + "[|How do you want to delete this Action?|]?", Action);
            }
        }).fail(function (error) {
            notifyError(error);
        });
    };

    selfDetails.DeleteTour = function (tourId) {
        var tour = "GetTourContactsCount";
        $.ajax({
            url: url + tour,
            type: 'post',
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({ 'tourId': tourId })
        }).then(function (response) {
            var filter = $.Deferred()
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            if (data.response.Count == 1 || data.response.SelectAll == true) {
                DeleteTourConfirmation("Delete Tour", "Cancel", "[|Are you sure you want to delete this Tour|]?", tourId, data.response.Count == 1 ? selfDetails.ContactID() : 0);
            }
            else {
                DeleteTourConfirmation("Delete Tour", "Cancel", "[|More than one contact tagged for this Tour, are you sure you want to delete this Tour|]?", tourId, 0);
            }
        }).fail(function (error) {
            if (error == undefined)
                $('#tourmodeltrigger').modal('toggle');
            else
                notifyError(error);

        })
    }
    var DeleteTourConfirmation = function (okText, cancelText, confirmMessage, TourId, cid) {
        alertifyReset(okText, cancelText);
        alertify.confirm(confirmMessage, function (e) {
            if (e) {
                var tour = "DeleteTour";
                $.ajax({
                    url: url + tour,
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({
                        'tourId': TourId, 'contactId': cid
                    })
                }).then(function (response) {
                    var filter = $.Deferred()
                    if (response.success) {
                        filter.resolve(response)
                    } else {
                        filter.reject(response.error)
                    }
                    return filter.promise()
                }).done(function (data) {
                    $('.success-msg').remove();
                    notifySuccess('[|Tour deleted successfully|]');
                    createCookie('log', false, 1);
                    window.location.href = document.URL;
                }).fail(function (error) {
                    notifyError(error);
                })
            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }
        });
    }

    selfDetails.DeleteRelation = function (Relation) {
        alertifyReset("Delete Relation", "Cancel");
        alertify.confirm("[|Are you sure you want to delete this Relationship(s)|]?", function (e) {
            if (e) {
                var action = "DeleteRelation";
                $.ajax({
                    url: url + action,
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({
                        'relationId': Relation.ContactRelationshipMapID
                    })
                }).then(function (response) {
                    var filter = $.Deferred()
                    if (response.success) {
                        filter.resolve(response)
                    } else {
                        filter.reject(response.error)
                    }
                    return filter.promise()
                }).done(function (data) {
                    $('.success-msg').remove();
                    notifySuccess('[|Relationship(s) deleted successfully|]');
                    setTimeout(function () {
                        createCookie('log', false, 1);
                        window.location.reload(true);
                    }, setTimeOutTimer)
                }).fail(function (error) {
                    notifyError(error);
                })
            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }
        });
    }

    function DeleteConfirmation(okText, cancelText, confirmMessage, Action) {
        alertifyReset(okText, cancelText);
        alertify.confirm(confirmMessage, function (e) {
            if (e) {
                var action = "DeleteAction";
                $.ajax({
                    url: url + action,
                    type: 'post',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({
                        'actionId': Action.ActionId
                    })
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
                    $('.success-msg').remove();
                    notifySuccess('[|Action deleted successfully|]');
                    setTimeout(
                            function () {
                                createCookie('log', false, 1);
                                window.location.reload(true);
                            }, setTimeOutTimer);
                }).fail(function (error) {
                    $('.success-msg').remove();
                    notifyError(error);
                });
            }
            else {
                notifyError("[|You've clicked Cancel|]");
            }
        });
    }

    //selfDetails.imagePath = ko.observable();
    //selfDetails.ProfileImage = ko.observable();
    //selfDetails.ImageAssign = function () {

    //    selfDetails.Image = ko.observable(PersonDetails.Image);
    //    if (selfDetails.Image().ImageContent != null && selfDetails.Image().ImageContent != "undefined") {
    //        // selfContact.imagePath(selfContact.Image().ImageContent);
    //        // selfContact.Image.ImageContent = null;
    //    }
    //    else {
    //        // selfContact.imagePath(ImagePath);
    //        selfDetails.Image().ImageContent = ImagePath;
    //    }
    //}
    //selfDetails.ImageAssign();
    //selfDetails.uploadProfileImage = function () {
    //    var filename = selfDetails.ProfileImage();
    //    selfDetails.ProfileImageKey = null;
    //    var extension = filename.replace(/^.*\./, '');

    //    if (extension.toLowerCase() == "jpeg" || extension.toLowerCase() == "jpg" || extension.toLowerCase() == "png" || extension.toLowerCase() == "bmp") {
    //        var image = document.getElementById("contactimage");
    //        image.src = filename;
    //        selfDetails.Image.ImageContent = filename;
    //        selfDetails.ContactImageUrl = filename;
    //    }
    //    else {
    //        notifyError("[|Please upload jpg, jpeg, png, bmp files|]");
    //        return false;
    //    }
    //}

    selfDetails.TimeLineViewModel = ko.observable(new TimeLineViewModel(url, OpportunityUrl, "contacts", PersonDetails.DateFormat, Activities, PersonDetails.ContactID, null));
    selfDetails.webVisitsActivated = ko.observable(false);
    selfDetails.getWebVisits = function () {
        if (!selfDetails.webVisitsActivated()) {
            CreateContactWebVisitsGrid();
            selfDetails.webVisitsActivated(true);
        }
    }

    function CreateContactWebVisitsGrid() {

        var newDataSource = new kendo.data.DataSource({
            transport: {
                read: function (options) {
                    var pageNumber = typeof ($("#contact-webvisits-grid").data("kendoGrid").dataSource.page()) != "undefined" ? $("#contact-webvisits-grid").data("kendoGrid").dataSource.page() : 1;
                    var pagesize = typeof ($("#contact-webvisits-grid").data("kendoGrid").dataSource.pageSize()) != "undefined" ? $("#contact-webvisits-grid").data("kendoGrid").dataSource.pageSize() : parseInt(parseInt(readCookie('pagesize')));
                    $.ajax({
                        url: '/Contact/GetContactWebVisits',
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        data: JSON.stringify({
                            'ContactId': selfDetails.ContactID(),
                            'PageNumber': pageNumber,
                            'PageSize': pagesize
                        }),
                        type: 'post'
                    }).then(function (response) {
                        var filter = $.Deferred()
                        if (response.success == true) {
                            filter.resolve(response)
                        }
                        else {
                            filter.reject(response.error)
                        }
                        return filter.promise()
                    }).done(function (data) {
                        options.success(data.response);
                    }).fail(function (error) {
                        notifyError(error);
                    });
                },
            },
            serverPaging: true,
            schema: {
                data: "Data",
                total: "Total"
            }
        });

        $("#contact-webvisits-grid").kendoGrid({
            dataSource: newDataSource,
            pageable: {
                pageSizes: true,
                buttonCount: 5
            },
            scrollable: false,
            sortable: true,
            columns: [{

                field: "VisitedOn",
                title: "Visited On",
                template: "#:displayDate(VisitedOn)#"
            }, {
                field: "PageViews",
                title: "Page Views",
                template: "#:PageViews#"
            }, {
                field: "Duration",
                title: "Duration"
            }, {
                field: "Page1",
                title: "Top Pages",
                template: "<div class='tm-minimize'>#:Page1# <br> #:Page2#  <br> #:Page3#</div>"
            }, {
                field: "Source",
                title: "Source"
            },
             {
                 field: "Location",
                 title: "Location"
             },
            {
                field: "VisitReference",
                title: "Details",
                template: "<div class='tm-minimize'> <span><a data-toggle='modal' data-target='\\#bigmodal' href='/webnotificationdetails?webVisitReference=#:VisitReference#'><i class='icon st-icon-eye'></i></a></span></div>"
            }
            ],

            dataBound: function (e) {
                onDataBound(e);
            },
            pageable: {
                pageSizes: ToPageSizeArray(),
                messages: {
                    display: "[|Showing|] {0}-{1} [|from|] {2:n0} [|Web visits|]"
                },
            },
            serverPaging: true,
        })
        var contactSummaryGrid = $("#contact-webvisits-grid").data("kendoGrid");
        var pagesize = typeof ($("#contact-webvisits-grid").data("kendoGrid").dataSource.pageSize()) != "undefined" ? $("#contact-webvisits-grid").data("kendoGrid").dataSource.pageSize() : parseInt(parseInt(readCookie('pagesize')));
        contactSummaryGrid.dataSource.query({ page: 1, pageSize: pagesize });
        $("#contact-webvisits-grid").wrap("<div class='cu-table-responsive bdx-report-grid'></div>");
    }

    function onDataBound(e) {
        var colCount = $(".k-grid").find('table colgroup > col').length;
        if (e.sender.dataSource.view().length == 0) {
            e.sender.table.find('tbody').append('<tr><td colspan="' + colCount + '"><div class="notecordsfound"><div><i class="icon st-icon-browser-windows-2"></i></div><span class="bolder smaller-90">[|No records found|]</span></div></td></tr>')
        }

    }

    function createCookie(name, value, days) {
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            var expires = "; expires=" + date.toGMTString();
        } else var expires = "";
        document.cookie = escape(name) + "=" + escape(value) + expires + "; path=/";
    }
    createCookie("pagesize", itemsPerPage, 1);
    selfDetails.summaryViewModel = new contactDetailsViewModel(WEBSERVICE_URL, selfDetails, itemsPerPage);

    function webVisitSearchParameters() {
        if (viewModel != null && typeof (viewModel) != typeof (undefined))
            $.each(viewModel.SearchFilters(), function (index, searchFilter) {
                if (searchFilter.InputTypeId() == 1 || searchFilter.InputTypeId() == 12) {  // 1 - Checkbox , 2 - Multiselect
                    if (searchFilter.SearchText() != null && Array.isArray(searchFilter.SearchText())) {
                        var searchText = searchFilter.SearchText().join('|');
                        searchFilter.SearchText(searchText);
                    }
                }
            });
        var parameters = {
            aviewModel: ko.toJSON(viewModel)
        }
        return parameters;
    };

    selfDetails.fetchedEditableCustomFields = ko.observable(false);
    selfDetails.CustomFieldsManager = new customFieldsManagerViewModel(selfDetails.CustomFieldTabs);

    selfDetails.Lookups = ko.observableArray([]);
    selfDetails.zillowLookUp = function (address) {
        if (selfDetails.Addresses()) {
            var primaryAddress = [];
            if (address == 1)
                primaryAddress = ko.utils.arrayFirst(selfDetails.Addresses(), function (add) {
                    var IsDefault = ko.isObservable(add.IsDefault) ? add.IsDefault() : add.IsDefault;
                    return IsDefault == true;
                });
            else
                primaryAddress = address;
            if (primaryAddress) {
                var addressLine1 = primaryAddress.AddressLine1 ? primaryAddress.AddressLine1.replace(/ /g, "+") : "";
                var city = primaryAddress.City ? primaryAddress.City.replace(/ /g, "+") : "";
                var state = primaryAddress.State.Name ? primaryAddress.State.Name.replace(/ /g, "+") : "";
                var zipcode = primaryAddress.ZipCode;

                var zillowUrl = "GetSearchResults.htm?zws-id=ZKEY&address=" + addressLine1;
                if (zipcode)
                    zillowUrl = zillowUrl + "&citystatezip=" + zipcode;
                else if (city && state)
                    zillowUrl = zillowUrl + "&citystatezip=" + city + "%2C+" + state;

                $.ajax({
                    url: url + "AddressLookup",
                    type: 'GET',
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8",
                    data: { 'zillowUrl': zillowUrl}
                }).then(function (response) {
                    var filter = $.Deferred();
                    if (response.success) {
                        filter.resolve(response)
                    }
                    else {
                        filter.reject(response.error);
                    }
                    return filter.promise()
                }).done(function (data) {
                    if (data.success && data.response.length > 0) {
                        //$.each(data.response, function (i, l) { l["Whitepages"] = window.location.origin + '/Contact/WhitePages?street_line_1=' + addressLine1 + '&city=' + city + '&postal_code=' + zipcode + '&api_key=WKEY' });
                        selfDetails.Lookups(data.response);
                        $('#zillowModal').modal('show');
                    }
                    else if (data.success && data.response.length == 0) {
                        notifyError("No exact match found for input address");
                    }
                    else
                        notifyError("An error occured while requesting for address lookup. Please contact Administrator");
                }).fail(function (error) {
                    notifyError(error);
                });
            }
        }
    };
}
