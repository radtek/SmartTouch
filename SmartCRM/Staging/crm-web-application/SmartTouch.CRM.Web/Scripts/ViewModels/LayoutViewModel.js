var conn = undefined;

//ViewModel
var LayoutViewModel = function (data, url, userId, ImagePath, isFirstLogin, isTourCompleted, userId) {
    self = this;

    self.Notifications = ko.observableArray([]);
    self.WebVisitNotifications = ko.observableArray([]);

    self.NotificationModules = ko.observableArray([{ ModuleId: 1, Name: "Accounts", selected: ko.observable(false) }, { ModuleId: 7, Name: "Tours", selected: ko.observable(false) },
                    { ModuleId: 5, Name: "Actions", selected: ko.observable(false) }, { ModuleId: 16, Name: "Opportunities", selected: ko.observable(false) }, { ModuleId: 3, Name: "Contacts", selected: ko.observable(false) },
                    { ModuleId: 23, Name: "Imports", selected: ko.observable(false) },
                    { ModuleId: 4, Name: "Campaign", selected: ko.observable(false) }, { ModuleId: 19, Name: "Lead Adapters", selected: ko.observable(false) }, { ModuleId: 72, Name: "Downloads", selected: ko.observable(false) },
                    { ModuleId: 77, Name: "Litmus&#x00AE; Results", selected: ko.observable(false) },
                    { ModuleId: 79, Name: "Mail Tester", selected: ko.observable(false) }
    ]);           

    self.EmailClients = ko.observableArray([{ Client: "OL2000", Value: "Outlook 2000", LitmusId: ko.observable(false) },
                                            { Client: "OL2002", Value: "Outlook 2002", LitmusId: ko.observable(false) },
                                            { Client: "OL2003", Value: "Outlook 2003", LitmusId: ko.observable(false) },
                                            { Client: "OL2007", Value: "Outlook 2007", LitmusId: ko.observable(false) },
                                            { Client: "OL2010", Value: "Outlook 2010", LitmusId: ko.observable(false) },
                                            { Client: "OL2011", Value: "Outlook 2011", LitmusId: ko.observable(false) },
                                            { Client: "OL2013", Value: "Outlook 2013", LitmusId: ko.observable(false) },
                                            { Client: "OL2016", Value: "Outlook 2016", LitmusId: ko.observable(false) },
                                            { Client: "OL2013DPI120", Value: "Outlook 2013 120 DPI", LitmusId: ko.observable(false) },
                                            { Client: "IPHONE5S", Value: "iPhone 5s (IOS 7)", LitmusId: ko.observable(false) },
                                            { Client: "IPHONE6", Value: "iPhone 6", LitmusId: ko.observable(false) },
                                            { Client: "IPHONE6S", Value: "iPhone 6s", LitmusId: ko.observable(false) },
                                            { Client: "IPHONE6SPLUS", Value: "iPhone 6s Plus", LitmusId: ko.observable(false) },
                                            { Client: "IPAD", Value: "iPad (Retina)", LitmusId: ko.observable(false) },
                                            { Client: "CHROMEGMAILNEW", Value: "Gmail (Chrome)", LitmusId: ko.observable(false) },
                                            { Client: "CHROMEGOOGLEINBOX", Value: "Inbox by Gmail (Chrome)", LitmusId: ko.observable(false) },
                                            { Client: "OUTLOOKCOM", Value: "Outlook.com (Explorer)", LitmusId: ko.observable(false) },
                                            { Client: "FFOUTLOOKCOM", Value: "Outlook.com (Firefox)", LitmusId: ko.observable(false) }]);


    self.NotificationPermissionModules = ko.observableArray([]);

    self.NotificationsCount = ko.observableArray([0, 0]);
    self.AccountNotificationsCount = ko.observableArray([0, 0]);
    self.ToursNotificationsCount = ko.observableArray([0, 0]);
    self.ActionsNotificationsCount = ko.observableArray([0, 0]);
    self.OpportunitiesNotificationsCount = ko.observableArray([0, 0]);
    self.ContactsNotificationsCount = ko.observableArray([0, 0]);
    self.ImportsNotificationsCount = ko.observableArray([0, 0]);
    self.CampaignNotificationsCount = ko.observableArray([0, 0]);
    self.LeadAdapterNotificationsCount = ko.observableArray([0, 0]);
    self.DownloadNotificationsCount = ko.observableArray([0, 0]);
    self.CampaignLitmusNotificationsCount = ko.observableArray([0, 0]);
    self.MailTesterCount = ko.observableArray([0, 0]);


    self.AccountNotifications = ko.observableArray([]);
    self.TourNotifications = ko.observableArray([]);
    self.ActionNotifications = ko.observableArray([]);
    self.OpportunityNotifications = ko.observableArray([]);
    self.ContactNotifications = ko.observableArray([]);
    self.ImportNotifications = ko.observableArray([]);
    self.CampaignNotifications = ko.observableArray([]);
    self.LeadAdapterNotifications = ko.observableArray([]);
    self.DownloadNotifications = ko.observableArray([]);
    self.CampaignLitmusNotifications = ko.observableArray([]);
    self.MailTesterNotifications = ko.observableArray([]);

    self.LitmusId = ko.observable();
    self.HelpURL = readCookie('helpURL');
    self.NotificationSelectedModules = ko.pureComputed(function () {
        return ko.utils.arrayFilter(self.NotificationPermissionModules(), function (module) {
            return module.selected() == true;
        });
    });

    self.NotificationUnSelectedModules = ko.pureComputed(function () {
        return ko.utils.arrayFilter(self.NotificationPermissionModules(), function (module) {
            return module.selected() == false;
        });
    });

    self.GetActiveAccordion = function () {
        var activeAccordion = $('.accordion-container .accordion-content').filter(function () {
            return $(this).css('display') == "block";
        });
        var moduleId = "";
        if (activeAccordion != null && activeAccordion.length > 0) {
            var accordionId = $(activeAccordion[0]).closest('.accordion-container').attr('id');
            if (accordionId != null || accordionId != 'undefined') {
                var numberPattern = /\d+/g;
                moduleId = accordionId.match(numberPattern);
            }
        }

        var moduleIds = [];
        moduleIds.push(parseInt(moduleId));

        return moduleIds;
    };

    self.ActualImagePreview = function (client, value, litmusGuid) {
        $("#litmus-preview").modal('toggle');
        $("#lmt-img-prw img").remove();
        var fullSrc = 'https://instant-api.litmus.com/v1/emails/' + litmusGuid + '/previews/' + client + '/full';
        var x = document.createElement("img");
        x.setAttribute("src", fullSrc);
        x.setAttribute("height", "auto");
        x.setAttribute("width", "auto");
        document.getElementById('lmt-img-prw').appendChild(x);
        $('#mdl-hdr').text(value);
    }

    self.CheckForDuplicate = function (moduleNotifications, moduleId, readNotifications) {
        var validUnreadNotifications = ko.observableArray();
        if (readNotifications != null && readNotifications.length > 0) {
            var entityIds = ko.utils.arrayMap(moduleNotifications, function(moduleNotification){ 
                if(moduleId != 5 && moduleId != 7)
                    return moduleNotification.NotificationID;
                else
                    return moduleNotification.EntityId;
            });
            ko.utils.arrayForEach(readNotifications, function (notification) {
                if (moduleId != 5 && moduleId != 7) {
                    if (entityIds.indexOf(notification.NotificationID) < 0)
                        validUnreadNotifications.push(notification);
                }
                else {
                    if (entityIds.indexOf(notification.EntityId) < 0)
                        validUnreadNotifications.push(notification);
                }
            });
        }
        return validUnreadNotifications();
    };

    self.ProcessReadNotifications = function (moduleIds, response) {
        var moduleId = moduleIds[0];
        var readNotifications = [];
        if (moduleId == 1) {
            var validUnreadNotifications = self.CheckForDuplicate(self.AccountNotifications(), 1, response.Notifications);
            if (validUnreadNotifications.length > 0) {
                ko.utils.arrayPushAll(self.AccountNotifications, validUnreadNotifications);
                sorted = self.sortNotifications(self.AccountNotifications());
                self.AccountNotifications([]);
                ko.utils.arrayPushAll(self.AccountNotifications, sorted);
            }
            else if (validUnreadNotifications.length == 0)
                notifyError("[|All previously read notifications are loaded (OR) There are no previously read notifications|]");
            else { }
        }
        else if (moduleId == 3) {
            var validUnreadNotifications = self.CheckForDuplicate(self.ContactNotifications(), 3, response.Notifications);
            if (validUnreadNotifications.length > 0) {
                ko.utils.arrayPushAll(self.ContactNotifications, validUnreadNotifications);
                sorted = self.sortNotifications(self.ContactNotifications());
                self.ContactNotifications([]);
                ko.utils.arrayPushAll(self.ContactNotifications, sorted);
            }
            else if (validUnreadNotifications.length == 0)
                notifyError("[|All previously read notifications are loaded (OR) There are no previously read notifications|]");
            else { }
        }
        else if (moduleId == 72) {
            var validUnreadNotifications = self.CheckForDuplicate(self.DownloadNotifications(), 72, response.Notifications);
            if (validUnreadNotifications.length > 0) {
                ko.utils.arrayPushAll(self.DownloadNotifications, validUnreadNotifications);
                sorted = self.sortNotifications(self.DownloadNotifications());
                self.DownloadNotifications([]);
                ko.utils.arrayPushAll(self.DownloadNotifications, sorted);
            }
            else if (validUnreadNotifications.length == 0)
                notifyError("[|All previously read notifications are loaded (OR) There are no previously read notifications|]");
            else { }
        }
        else if (moduleId == 4) {
            var validUnreadNotifications = self.CheckForDuplicate(self.CampaignNotifications(), 4, response.Notifications);
            if (validUnreadNotifications.length > 0) {
                ko.utils.arrayPushAll(self.CampaignNotifications, validUnreadNotifications);
                sorted = self.sortNotifications(self.CampaignNotifications());
                self.CampaignNotifications([]);
                ko.utils.arrayPushAll(self.CampaignNotifications, sorted);
            }
            else if (validUnreadNotifications.length == 0)
                notifyError("[|All previously read notifications are loaded (OR) There are no previously read notifications|]");
            else { }
        }
        else if (moduleId == 5) {
            var validUnreadNotifications = self.CheckForDuplicate(self.ActionNotifications(), 5, response.Notifications);
            if (validUnreadNotifications.length > 0) {
                console.log(validUnreadNotifications);
                ko.utils.arrayPushAll(self.ActionNotifications, validUnreadNotifications);
                sorted = self.sortNotifications(self.ActionNotifications());
                self.ActionNotifications([]);
                ko.utils.arrayPushAll(self.ActionNotifications, sorted);
            }
            else if (validUnreadNotifications.length == 0)
                notifyError("[|All previously read notifications are loaded (OR) There are no previously read notifications|]");
            else { }
        }
        else if (moduleId == 7) {
            var validUnreadNotifications = self.CheckForDuplicate(self.TourNotifications(), 7, response.Notifications);
            if (validUnreadNotifications.length > 0) {
                console.log(validUnreadNotifications);
                ko.utils.arrayPushAll(self.TourNotifications, validUnreadNotifications);
                sorted = self.sortNotifications(self.TourNotifications());
                self.TourNotifications([]);
                ko.utils.arrayPushAll(self.TourNotifications, sorted);
            }
            else if (validUnreadNotifications.length == 0)
                notifyError("[|All previously read notifications are loaded (OR) There are no previously read notifications|]");
            else { }
        }
        else if (moduleId == 16) {
            var validUnreadNotifications = self.CheckForDuplicate(self.OpportunityNotifications(), 16, response.Notifications);
            if (validUnreadNotifications.length > 0) {
                ko.utils.arrayPushAll(self.OpportunityNotifications, validUnreadNotifications);
                sorted = self.sortNotifications(self.OpportunityNotifications());
                self.OpportunityNotifications([]);
                ko.utils.arrayPushAll(self.OpportunityNotifications, sorted);
            }
            else if (validUnreadNotifications.length == 0)
                notifyError("[|All previously read notifications are loaded (OR) There are no previously read notifications|]");
            else { }
        }
        else if (moduleId == 19) {
            var validUnreadNotifications = self.CheckForDuplicate(self.LeadAdapterNotifications(), 19, response.Notifications);
            if (validUnreadNotifications.length > 0) {
                ko.utils.arrayPushAll(self.LeadAdapterNotifications, validUnreadNotifications);
                sorted = self.sortNotifications(self.LeadAdapterNotifications());
                self.LeadAdapterNotifications([]);
                ko.utils.arrayPushAll(self.LeadAdapterNotifications, sorted);
            }
            else if (validUnreadNotifications.length == 0)
                notifyError("[|All previously read notifications are loaded (OR) There are no previously read notifications|]");
            else { }
        }
        else if (moduleId == 23) {
            var validUnreadNotifications = self.CheckForDuplicate(self.ImportNotifications(), 23, response.Notifications);
            if (validUnreadNotifications.length > 0) {
                ko.utils.arrayPushAll(self.ImportNotifications, validUnreadNotifications);
                sorted = self.sortNotifications(self.ImportNotifications());
                self.ImportNotifications([]);
                ko.utils.arrayPushAll(self.ImportNotifications, sorted);
            }
            else if (validUnreadNotifications.length == 0)
                notifyError("[|All previously read notifications are loaded (OR) There are no previously read notifications|]");
            else { }
        }
        else if (moduleId == 77) {
            var validUnreadNotifications = self.CheckForDuplicate(self.CampaignLitmusNotifications(), 77, response.Notifications);
            if (validUnreadNotifications.length > 0) {
                ko.utils.arrayPushAll(self.CampaignLitmusNotifications, validUnreadNotifications);
                sorted = self.sortNotifications(self.CampaignLitmusNotifications());
                self.CampaignLitmusNotifications([]);
                ko.utils.arrayPushAll(self.CampaignLitmusNotifications, sorted);
            }
            else if (validUnreadNotifications.length == 0)
                notifyError("[|All previously read notifications are loaded (OR) There are no previously read notifications|]");
            else { }
        }
        else if (moduleId == 79) {
            var validUnreadNotifications = self.CheckForDuplicate(self.MailTesterNotifications(), 77, response.Notifications);
            if (validUnreadNotifications.length > 0) {
                ko.utils.arrayPushAll(self.MailTesterNotifications, validUnreadNotifications);
                sorted = self.sortNotifications(self.MailTesterNotifications());
                self.MailTesterNotifications([]);
                ko.utils.arrayPushAll(self.MailTesterNotifications, sorted);
            }
            else if (validUnreadNotifications.length == 0)
                notifyError("[|All previously read notifications are loaded (OR) There are no previously read notifications|]");
            else { }
        }
        else { }
    };

    //load previously read notifications
    self.LoadNotifications = function () {
        //try get which accordion is active
        var moduleIds = self.GetActiveAccordion();
        var activeModule = moduleIds[0];

        if (!isNaN(activeModule)) {
            $('.accordion-content').append('<div class="k-loading-mask"><div class="k-loading-image"></div></div>');
            var authToken = readCookie("accessToken");
            $.ajax({
                url: WEBSERVICE_URL + "/Notifications",
                type: 'get',
                dataType: 'json',
                data: { 'userId': userId, 'moduleIds': JSON.stringify(moduleIds), 'todayNotifications': self.TodayNotifications() },
                contentType: "application/json; charset=utf-8",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                },
                success: function (response) {
                    $('.accordion-content .k-loading-mask').fadeOut().remove('');
                    if (moduleIds.length == 1) {
                        self.ProcessReadNotifications(moduleIds, response);
                    }
                },
                error: function (error) {
                    $('.accordion-content .k-loading-mask').fadeOut().remove('');
                    console.log(error);
                }
            });
            $('.accordion-content .k-loading-mask').fadeOut().remove('');
        }
        else
            notifyError("Please select at least one module");
    }

    self.sortNotifications = function (totalUnread) {
        var notifications = totalUnread;
        var sortedNotifications = [];
        if (totalUnread.length > 0) {
            sortedNotifications = notifications.sort(function (notification1, notification2) {
                if (new Date(Date.parse(notification1.Time)) > new Date(Date.parse(notification2.Time))) return -1;
                if (new Date(Date.parse(notification1.Time)) < new Date(Date.parse(notification2.Time))) return 1;
                return 0;
            });
        }
        return sortedNotifications;
    }

    self.LastNotificationsAccessed = ko.observable();
    self.TodayNotifications = ko.observable(true);

    self.TabSelection = function (data, e) {
        if (e != null) {
            if (e.target.id == "tab1") {
                self.TodayNotifications(true);
            }
            else if (e.target.id == "tab2") {
                self.TodayNotifications(false);
            }
            else { }
        }
    }

    self.ShowNotifications = function () {
        // if (self.NotificationsLoaded() == true)
        //  return true;
        var moduleIds = self.GetActiveAccordion();
        var activeModule = moduleIds[0];

        if (!isNaN(activeModule)) {
            $('.accordion-content').append('<div class="k-loading-mask"><div class="k-loading-image"></div></div>');
            var authToken = readCookie("accessToken");
            $.ajax({
                url: WEBSERVICE_URL + "/Reminders",
                type: 'get',
                dataType: 'json',
                data: { 'userId': userId, 'moduleIds': JSON.stringify(moduleIds), 'todayNotifications': self.TodayNotifications() },
                contentType: "application/json; charset=utf-8",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                },
                success: function (response) {

                    $('.accordion-content .k-loading-mask').fadeOut().remove('');

                    if (response.Notifications) {
                        if (moduleIds[0] == 1) {
                            self.AccountNotifications([]);
                            ko.utils.arrayPushAll(self.AccountNotifications, response.Notifications);
                        }
                        else if (moduleIds[0] == 3) {
                            self.ContactNotifications([]);
                            ko.utils.arrayPushAll(self.ContactNotifications, response.Notifications);
                        }
                        else if (moduleIds[0] == 72) {
                            self.DownloadNotifications([]);
                            ko.utils.arrayPushAll(self.DownloadNotifications, response.Notifications);
                        }
                        else if (moduleIds[0] == 4)  {
                            self.CampaignNotifications([]);
                            ko.utils.arrayPushAll(self.CampaignNotifications, response.Notifications);
                        } 
                        else if (moduleIds[0] == 5) {
                            self.ActionNotifications([]);
                            ko.utils.arrayPushAll(self.ActionNotifications, response.Notifications);
                        }
                        else if (moduleIds[0] == 7) {
                            self.TourNotifications([]);
                            ko.utils.arrayPushAll(self.TourNotifications, response.Notifications);
                        }
                        else if (moduleIds[0] == 16) {
                            self.OpportunityNotifications([]);
                            ko.utils.arrayPushAll(self.OpportunityNotifications, response.Notifications);
                        }
                        else if (moduleIds[0] == 19) {
                            self.LeadAdapterNotifications([]);
                            ko.utils.arrayPushAll(self.LeadAdapterNotifications, response.Notifications);
                        }
                        else if (moduleIds[0] == 23) {
                            self.ImportNotifications([]);
                            ko.utils.arrayPushAll(self.ImportNotifications, response.Notifications);
                        }
                        else if (moduleIds[0] == 77) {
                            self.CampaignLitmusNotifications([]);
                            ko.utils.arrayPushAll(self.CampaignLitmusNotifications, response.Notifications);
                        }
                        else if (moduleIds[0] == 79) {
                            self.MailTesterNotifications([]);
                            ko.utils.arrayPushAll(self.MailTesterNotifications, response.Notifications);
                        }
                        else { }
                    }
                    self.NotificationsLoaded(true);
                    //return self.Notifications.sort(function (notification1, notification2) {
                    //    if (new Date(Date.parse(notification1.Time())) > new Date(Date.parse(notification2.Time()))) return -1;
                    //    if (new Date(Date.parse(notification1.Time())) < new Date(Date.parse(notification2.Time()))) return 1;
                    //    return 0;
                    //});
                    //return self.Notifications();
                    //}, timeout);

                }
            });
        }
        else
            notifyError("Please select at least one module");
    }

    self.LoadNotificationsByModule = function (data) {
        if (data != null || data != 'undefined') {
            var selection = data.selected();
            if (selection) {
                self.ShowNotifications();
            }
            else {
                var notifications = ko.utils.arrayFilter(self.Notifications(), function (activity) {
                    return activity.ModuleID != data.ModuleId;
                });
                if (notifications.length == 0)
                    self.ShowNotifications();
                else
                    self.Notifications(notifications);
            }
        }
        return true;
    }

    // Notification no filtes
    function notifinofilter() {
        var hh = "";
        $('.accordion-container').each(function () {
            hh = $('.accordion-container:visible').length;
        });
        if (hh == "0") {
            if ($(".st-no-filters").length == "0") {
                $('.tab-pane').append('<div class="st-no-filters"><div class="st-no-icon icon st-icon-bell-2"></div><div class="st-no-text"> No Items Selected</div></div>');
            }
        } else {
            $('.st-no-filters').remove();
        }

    }

    self.notificationfilterclickevent = function () {
        notifinofilter();
    }

    self.AccordionSelection = function (data, event) {
        setTimeout(function () {
            e = event;
            elementSibling = $(event.target).siblings('.accordion-content');
            if (elementSibling != null && elementSibling.css('display') == "block") {
                var id = event.target.parentElement.id;
                var numberPattern = /\d+/g;
                var moduleId = id.match(numberPattern);
                console.log(moduleId[0]);
                $('.contactsnotifications').removeClass('active');
                $('.opportunitynotifications').removeClass('active');
                $('.leadadapternotifications').removeClass('active');
                $('.campaignnotifications').removeClass('active');
                $('.importnotifications').removeClass('active');
                $('.accountnotifications').removeClass('active');
                $('.actionnotifications').removeClass('active');
                $('.tournotifications').removeClass('active');
                $('.litmusnotifications').removeClass('active');
                
                if (moduleId[0] == 3)
                    $('.contactsnotifications').addClass('active');
                else if (moduleId[0] == 16)
                    $('.opportunitynotifications').addClass('active');
                else if (moduleId[0] == 19)
                    $('.leadadapternotifications').addClass('active');
                else if (moduleId[0] == 4)
                    $('.campaignnotifications').addClass('active');
                else if (moduleId[0] == 23)
                    $('.importnotifications').addClass('active');
                else if (moduleId[0] == 1)
                    $('.accountnotifications').addClass('active');
                else if (moduleId[0] == 5)
                    $('.actionnotifications').addClass('active');
                else if (moduleId[0] == 7)
                    $('.tournotifications').addClass('active');
                else if (moduleId[0] == 77)
                    $('.litmusnotifications').addClass('active');

                ko.utils.arrayForEach(self.NotificationPermissionModules(), function (noti) {
                    if (noti.ModuleId == moduleId[0])
                        noti.selected(true);
                });
                self.ShowNotifications();
            }
            else { }
            return true;
        }, 600)
    }

    self.FilterSelection = function (data) {
        ko.utils.arrayForEach(self.NotificationSelectedModules(), function (module) {
            $('#' + module.ModuleId + '-today').css('display', '');
            $('#' + module.ModuleId + '-previous').css('display', '');
           // notifinofilter();
        });
        ko.utils.arrayForEach(self.NotificationUnSelectedModules(), function (module) {
            $('#' + module.ModuleId + '-today').css('display', 'none');
            $('#' + module.ModuleId + '-previous').css('display', 'none');
            notifinofilter();
        });
        return true;
    }

    self.DeleteNotification = function (bulkRemove, notifications, data, moduleId, arePreviousNotifications, count) {
        var authToken = readCookie("accessToken");
        $('.accordion-content').append('<div class="k-loading-mask"><div class="k-loading-image"></div></div>');
        notificationIds = [];
        if ((moduleId == 5 || moduleId == 7) && data.EntityId != null) {
            notificationIds.push(data.EntityId);
        }
        else if (data.NotificationID != null) {
            notificationIds.push(data.NotificationID);
        }

        var tabId = "";
        var activeTab = $('#tabsByDate .active .tabIdentify');
        tabId = $(activeTab[0]).attr('id');
        console.log(tabId);
        if (tabId == "tab2")
            arePreviousNotifications = true;
        else if (tabId == "tab1")
            arePreviousNotifications = false;
        else { }

        var deleteViewModel = {};
        deleteViewModel.NotificationIds = notificationIds;
        deleteViewModel.IsBulkDelete = bulkRemove;
        deleteViewModel.ModuleId = moduleId;
        deleteViewModel.ArePreviousNotifications = arePreviousNotifications;
        var stringified = JSON.stringify(deleteViewModel);

        $.ajax({
            url: WEBSERVICE_URL + "/DeleteNotification",
            type: 'get',
            dataType: 'json',
            data: { 'deleteViewModel': stringified },
            contentType: "application/json; charset=utf-8",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + authToken);
            },
            success: function (response) {
                //We need to change the notifications count if user delete unread notifications only(not viewed notifications).
                if (response != null) {
                    self.GetCountByDate();
                    var presentCount = self.NewNotifications();
                    if (bulkRemove) {
                        notifications.removeAll();
                        var newCount = presentCount - response.DeletedCount;
                        if (newCount != 0 && newCount > -1)
                            self.NewNotifications(parseInt(newCount));
                    }
                    else {
                        if (data.Status == 1) {
                            self.NewNotifications(parseInt(self.NewNotifications()) - 1);
                        }
                    }
                    if (notificationIds != null || typeof (notificationIds) != 'undefined') {
                        notifications.remove(data);
                    }
                    $('.accordion-content .k-loading-mask').fadeOut().remove('');
                }
            },
            error: function (error) {
                console.log(error);
                $('.accordion-content .k-loading-mask').fadeOut().remove('');
            }
        });
        $('.accordion-content .k-loading-mask').fadeOut().remove('');
    }

    self.Deletenotemessage = ko.observable(false);

    self.Deletemessage = ko.observable(false);

    self.NewNotifications = ko.observable(0);
    self.NewWebVisitNotifications = ko.observable(0);
    self.NewNotificationsCount = ko.pureComputed({
        read: function () {
            var NEW_NOTIFICATION_STATUS = 1;
            var newNotifications = ko.utils.arrayFilter(self.Notifications(), function (item) {
                if (item.Status === NEW_NOTIFICATION_STATUS)
                    return true;
                else
                    return false;
            });
            var count = parseInt(self.NewNotifications());

            self.NewNotifications(count);

            return newNotifications.length;
        },
        write: function (value) {
            self.NewNotifications(value);
        },
        owner: this
    });

    self.NewWebVisitNotificationIds = ko.observableArray([]);
    self.NotificationsLoaded = ko.observable(false);

    self.CurrentNotification = ko.observable();

    self.CurrentNotificationSubject = ko.observable();

    self.ViewNotification = function (data, event) {
        self.Deletemessage = ko.observable(false);
        var IscontactDeleted = $(data.ContactEntries).filter(function () {
            return this.IsDeleted === true;
        })[0];
        if (IscontactDeleted != undefined)
            self.Deletemessage = ko.observable(true);
        if (data.Source === 1)
            self.CurrentNotificationSubject("Action Reminder");
        else if (data.Source === 2)
            self.CurrentNotificationSubject("Tour Reminder");
        else
            self.CurrentNotificationSubject(data.Subject);

        if (data.Source != 77 && data.Source != 79)
            self.CurrentNotification(data);
        
        if (data.Status === 2 && data.Source != 77 && data.Source != 79)
            return;

        if (data.Source == 79) {
            console.log(data);
        }
        var authToken = readCookie("accessToken");
        if (data.Source === 77) {
            $.ajax({
                url: WEBSERVICE_URL + "/litmusresults",
                type: 'get',
                contentType: "application/json; charset=utf-8",
                beforeSend: function (xhr) {
                    xhr.setRequestHeader("Authorization", "Bearer " + authToken);
                },
                data: {
                    'campaignId': data.EntityId
                },
                success: function (response) {
                    $.each(self.EmailClients(), function (index, value) {
                        value.LitmusId = response.LitmusId;
                    });

                    data.Details = "";
                    data.EmailClients = self.EmailClients();
                    self.CurrentNotification(data);
                }
            });
        }

        
        $.ajax({
            url: WEBSERVICE_URL + "/MarkNotificationAsRead",
            type: 'post',
            contentType: "application/json; charset=utf-8",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Bearer " + authToken);
            },
            data: ko.toJSON(data),
            success: function (response) {
                data.Status = 2;
                element = $(event.target).closest('li');
                if (element != null)
                    element.removeClass("unread");

                var NEW_NOTIFICATION_STATUS = 1;
                var newNotifications = ko.utils.arrayFilter(self.Notifications(), function (item) {
                    if (item.Status === NEW_NOTIFICATION_STATUS)
                        return true;
                    else
                        return false;
                });
                self.NewNotifications(parseInt(self.NewNotifications()) - 1);
            }
        });
    }

   

    self.RecentActivities = ko.observableArray([]);

    self.CurrentUserActivity = ko.observable();
    self.RecentActivitiesModules = ko.observableArray([{ ModuleId: 1, Name: "Accounts", selected: ko.observable(false) }, { ModuleId: 2, Name: "Users", selected: ko.observable(false) },
        { ModuleId: 3, Name: "Contacts", selected: ko.observable(false) }, { ModuleId: 7, Name: "Tours", selected: ko.observable(false) }, { ModuleId: 9, Name: "Tags", selected: ko.observable(false) },
        { ModuleId: 5, Name: "Actions", selected: ko.observable(false) }, { ModuleId: 6, Name: "Notes", selected: ko.observable(false) }, { ModuleId: 4, Name: "Campaigns", selected: ko.observable(false) },
        { ModuleId: 10, Name: "Forms", selected: ko.observable(false) }, { ModuleId: 16, Name: "Opportunities", selected: ko.observable(false) },
        { ModuleId: 24, Name: "Reports", selected: ko.observable(false) },
        { ModuleId: 31, Name: "Advanced Search", selected: ko.observable(false) }, { ModuleId: 33, Name: "Automation", selected: ko.observable(false) }]);

    self.UserPermissionModules = ko.observableArray([]);

    self.SelectedModules = ko.pureComputed(function () {
        return ko.utils.arrayFilter(self.RecentActivitiesModules(), function (module) {
            return module.selected() == true;
        });
    });
    self.RecentActivityPage = ko.observable(1);
    self.LoadRecentActivities = function () {
        var moduleIds = ko.utils.arrayMap(self.SelectedModules(), function (module) {
            return module.ModuleId;
        });
        $('#UserRecentActivities').append('<div class="k-loading-mask"><div class="k-loading-image"></div></div>');
        var url = "/Home/RecentActivities";
        $.ajax({
            url: url,
            type: 'get',
            dataType: 'json',
            data: { 'moduleIds': JSON.stringify(moduleIds), 'pageNo': self.RecentActivityPage() },
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                $('#UserRecentActivities .k-loading-mask').fadeOut().remove('');
                if (self.RecentActivityPage() == 1)
                    self.RecentActivities([]);
                ko.utils.arrayPushAll(self.RecentActivities, data.UserActivities.UserActivities);   //If we don't push activities scroll will come down and will load activities again
                //$("#useractscroll").animate({ scrollTop: 0 }, "slow");
                var array = [];
                var distinctModules = function () {
                    ko.utils.arrayForEach(self.RecentActivitiesModules(), function (activityModule) {
                        if (data.UserModules.indexOf(activityModule.ModuleId) > -1)
                            array.push(activityModule);
                    });
                }();
                self.UserPermissionModules(array);
                if (self.SelectedModules().length > 0) {
                    var validActivities = [];
                    var selectedModuleIds = self.SelectedModules().map(function (f) { return f.ModuleId; });
                    ko.utils.arrayForEach(self.RecentActivities(), function (activity) {
                        if (selectedModuleIds.indexOf(activity.ModuleID) > -1)
                            validActivities.push(activity);
                    });
                    self.RecentActivities(validActivities);
                };
            }
        });
        return true;
    };

    $("#useractscroll").scroll(function () {
        if ($(this).scrollTop() + $(this).innerHeight() >= $(this)[0].scrollHeight) {
            var pageNo = self.RecentActivityPage();
            self.RecentActivityPage(pageNo + 1);
            self.LoadRecentActivities();
        }
    });

    self.sortRecentActivities = function (activities) {
        var userActivities = activities;
        var sortedActivities = [];
        if (activities.length > 0) {
            sortedActivities = userActivities.sort(function (activity1, activity2) {
                if (new Date(Date.parse(activity1.LogDate)) > new Date(Date.parse(activity2.LogDate))) return -1;
                if (new Date(Date.parse(activity1.LogDate)) < new Date(Date.parse(activity2.LogDate))) return 1;
                return 0;
            });
        }
        return sortedActivities;
    }

    //.RecentWebVisits = ko.observableArray([]);
    var webVisitPageNumber = 0;
    var pageLimit = 5;
    self.IncludePreviouslyReadWebVisits = ko.observable(false);
    self.IncludePreviouslyReadWebVisits.subscribe(function (e) {
        webVisitPageNumber = 0;

        self.LoadRecentWebVisits(false,true);

    });
    self.LoadRecentWebVisits = function (loadMore, viewToggle) {
        if (loadMore && !viewToggle) {
            webVisitPageNumber = webVisitPageNumber + 1;
        }
        else if (!viewToggle) {
            webVisitPageNumber = 0;
            $("#nowebvists").addClass("hide").removeClass("show");
            $("#loadmorewebvisits").addClass("show").removeClass("hide");
        }
        if (self.WebVisitNotifications().length == 0 || loadMore || viewToggle || webVisitPageNumber==0)
            loadWebVists(webVisitPageNumber, viewToggle, loadMore);
    }

    var loadWebVists = function (webVisitPageNumber, viewToggle, loadMore) {
        var url = window.location.origin + "/Home/RecentWebVisits";
        $('#RecentWebVisits').append('<div class="k-loading-mask"><div class="k-loading-image"></div></div>');
        $.ajax({
            url: url,
            type: 'get',
            dataType: 'json',
            data: { 'parameters': JSON.stringify({ 'PageNumber': webVisitPageNumber, 'Limit': pageLimit, 'IncludePreviouslyRead': self.IncludePreviouslyReadWebVisits() }) },
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                $('#RecentWebVisits .k-loading-mask').fadeOut().remove('');
                temp = data;
                if (webVisitPageNumber>0 || loadMore == true)
                    ko.utils.arrayPushAll(self.WebVisitNotifications, data.RecentWebVisits);
                else
                    self.WebVisitNotifications(data.RecentWebVisits);
                if (data.RecentWebVisits.length < pageLimit) {

                    $("#nowebvists").addClass("show").removeClass("hide");
                    $("#loadmorewebvisits").addClass("hide").removeClass("show");

                }
                else {
                    $("#nowebvists").addClass("hide").removeClass("show");
                    $("#loadmorewebvisits").addClass("show").removeClass("hide");
                }

                if (webVisitPageNumber > 0 || viewToggle)
                    $("#webvisitsnotifications").animate({ scrollTop: $("#webvisitsnotifications").height() * webVisitPageNumber }, 'slow');
            },
            error: function (error) {
                console.log(error)
            }
        });
    }
    self.LoadModule = function (data) {
        if (data != null || data != 'undefined') {
            var selection = data.selected();
            if (selection) {
                self.RecentActivityPage(1);
                //console.log("Selected and page is " + self.RecentActivityPage());
                self.LoadRecentActivities();
            }
            else {
                var recentActivities = ko.utils.arrayFilter(self.RecentActivities(), function (activity) {
                    return activity.ModuleID != data.ModuleId;
                });
                if (recentActivities.length == 0) {
                    self.RecentActivityPage(1);
                    //console.log("Un Selected and page is " + self.RecentActivityPage());
                    self.LoadRecentActivities();
                }
                else
                    self.RecentActivities(recentActivities);
            }
        }
        return true;
    };

    self.ViewUserActivity = function (activity, event) {

        self.Deletenotemessage = ko.observable(false);

        var moduleId = activity.ModuleID;
        var entityIds = activity.EntityIds;
        var jsondata = JSON.stringify({ 'moduleId': moduleId, 'entityIds': entityIds });
        if (moduleId != null && entityIds != null) {
            var authToken = readCookie("accessToken");
            $.ajax({
                url: "/Home/ActivityDetail",
                type: 'post',
                dataType: 'json',
                data: JSON.stringify({
                    'moduleId': moduleId, 'entityIds': entityIds
                }),
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    activity.EntityDetails = data;

                    var Isdeleted = $(activity.EntityDetails).filter(function () {
                        return this.EntityId === 0;
                    })[0];
                    if (Isdeleted != undefined)
                        self.Deletenotemessage = ko.observable(true);
                    self.CurrentUserActivity(activity);
                }
            });
        }
    };

    self.markAsViewed = function (notification) {
        noti = notification;
        var indexOfViewed = self.NewWebVisitNotificationIds().indexOf(notification.NotificationID);
        console.log("Index of viewed : " + indexOfViewed);
        if (indexOfViewed != -1)
            self.NewWebVisitNotificationIds.splice(indexOfViewed, 1);
        webVisitPageNumber = 0;
    };

}

var checkedContactValues = "";
function fnGetCheckedValues() {
    checkedContactValues = $('.chkcontacts:checked').map(function () {
        return ($(this).attr('id') + "|" + $(this).attr('data-name') + "|" + $(this).attr('data-company') + "|" + $(this).attr('data-Email')) + "|" + $(this).attr('data-contacttype');
    }).get();
    return checkedContactValues;
}

function getgridpagesize(gridid) {
    if ($("#" + gridid).length > 0) {
        var pagesize = $("#" + gridid).data("kendoGrid").dataSource.pageSize();
        return pagesize;
    } else {
        if ($('#resultsGrid').length > 0) {
            var table = $('#resultsGrid').DataTable();
            var info = table.page.info();
            return info.length;
        }
        return 10;
    }
}

var checkedContactEmailValues = "";
function fnGetCheckedContactEmails() {

    checkedContactEmailValues = $('.chkcontacts:checked').map(function () {
        return ($(this).attr('data-email-id') + "|" + $(this).attr('data-name') + "|" + $(this).attr('data-company') + "|" + $(this).attr('data-Email') + "|" + $(this).attr('data-contacttype') + "|" + $(this).attr('data-DonotEmail') + "|" + $(this).attr('data-EmailStatus'));
    }).get();
    return checkedContactEmailValues;
}



var checkedWorkflowStatus = "";
function fnGetCheckedWorkflows(checkboxId) {
    checkedWorkflowStatus = $('.' + checkboxId + ':checked').map(function () {
        var workflow = { WorkflowID: $(this).attr('id'), WorkflowStatus: $(this).attr('status') };
        return workflow;
    }).get();

    return checkedWorkflowStatus;
}



var checkedContactPhoneNumbers = "";
function fnGetCheckedContactPhones() {
    checkedContactPhoneNumbers = $('.chkcontacts:checked').map(function () {
        return ($(this).attr('data-phone-id') + "|" + $(this).attr('data-name') + "|" + $(this).attr('data-company') + "|" + $(this).attr('data-phone') + "|" + $(this).attr('data-contacttype')+ "|" + $(this).attr('id'));
    }).get();
    return checkedContactPhoneNumbers;
}
var chkCopyContacttype = "";
function fnGetCheckedContactWithType() {
    chkCopyContacttype = $('.chkcontacts:checked').map(function () {
        return ($(this).attr('data-contacttype'));
    }).get();

    return chkCopyContacttype;

}

var checkedContactIds = "";
function fnGetCheckedContactIDs() {
    checkedContactIds = $('.chkcontacts:checked').map(function () {
        return $(this).attr('id');
    }).get();

    return checkedContactIds;
}

var checkedvalues = "";
function fnGetChkvalGrid(checkboxId) {
    checkedvalues = $('.' + checkboxId + ':checked').map(function () {
        return $(this).attr('id');
    }).get();
    return checkedvalues;
}


var checkedvaluesforcontacts = "";
function fnGetChkvalforContactsGrid(checkboxId) {
    checkedvaluesforcontacts = $('.' + checkboxId + ':checked').map(function () {
        if ($(this).attr('data-IsDelete') == 'true' || $(this).attr('data-IsAccountAdmin') == 'True' || $(this).attr('data-IsAccountAdmin') == 'true') {
            return $(this).attr('id');
        }
    }).get();
    return checkedvaluesforcontacts;
}

var checkedContactStatus = false;
function fnGetChkStsforContact(checkboxId) {
    $('.' + checkboxId + ':checked').each(function () {
        if ($(this).attr('data-IsDelete') == 'true' || $(this).attr('data-IsAccountAdmin') == 'True') {
            checkedContactStatus = true;
        }
        else {
            checkedContactStatus = false;
            return checkedContactStatus;
        }
    });
    return checkedContactStatus;
}

var checkedvaluesfortagsgrid = "";
function fnGetChkvalforTagsGrid(checkboxId) {
    checkedvalues = $('.' + checkboxId + ':checked').map(function () {
        return $(this).attr('id') + ";" + $(this).attr('data-leadscoretag') + ";" + $(this).attr('data-status');
    }).get();
    return checkedvalues;
}

var checkedvaluesforaddopportunity = "";
function fnGetChkvalGridForOpportunities(checkboxId) {
    checkedvaluesforaddopportunity = $('.' + checkboxId + ':checked').map(function () {
        return { Id: $(this).attr('id'), type: $(this).attr('data-contacttype') };
    }).get();
    return checkedvaluesforaddopportunity;
}


var checkedvaluesforactionsgrid = "";
function fnGetChkvalforActionGrid(checkboxId) {
    
    checkedvaluesforactionsgrid = $('.' + checkboxId + ':checked').map(function () {
        var action = {
            ActionID: $(this).attr('data-id'), ActionType: $(this).attr('data-actiontype'), ActionDate: $(this).attr('data-actiondate'), IsCompleted: $(this).attr('data-Iscompleted')
     };
        return action;
    }).get();
    return checkedvaluesforactionsgrid;
}


var checkedAccountName = [];
function fnGetChkvalAccountName() {
    checkedAccountName.Name = $('.chkaccount:checked').map(function () {
        return $(this).attr('data-name');
    }).get();
    checkedAccountName.Id = $('.chkaccount:checked').map(function () {
        return $(this).attr('id');
    }).get();
    return checkedAccountName;
}

var selectedUsers = [];
function fnGetSelectedUsers(checkboxId) {
    selectedUsers = $('.' + checkboxId + ':checked').map(function () {
        return { Id: $(this).attr('id'), Status: $(this).attr('data-status'), Role: $(this).attr('data-roleid') };
    }).get();
    return selectedUsers;
}

var selectedSearches = [];
function fnGetSelectedSearches(checkboxClass) {
    selectedSearches = $('.' + checkboxClass + ':checked').map(function () {
        return { searchDefinitionID: $(this).attr('SearchDefinitionID'), isPreconfiguredSearch: $(this).attr('IsPreConfigSearch'), isFavoritSearch: $(this).attr('IsFavoriteSearch') }
    }).get();
    return selectedSearches;
}


var selectedOpportunities = [];
function GetSelectedOpportunities(checkboxclass) {
    selectedOpportunities = $('.' + checkboxclass + ':checked').map(function () {
        return { OpportunityID: $(this).attr('OpportunityID'), PeopleInvolved: $(this).attr('PeopleInvolved'), OpportunityName: $(this).attr('OpportunityName') };
    }).get();
    return selectedOpportunities;
}

var checkedvaluesname = "";
function fnGetChkvalName(checkboxId) {
    checkedvaluesname = $('.' + checkboxId + ':checked').map(function () {
        return $(this).attr('data-name');
    }).get();

    return checkedvaluesname;
}

//to get the status in the grid used in campaigns to hide the edit buttons
//used same method in tags to know the no of items tagged
var checkedvaluesstatus = "";
function fnGetChkvalStatus(checkboxId) {
    checkedvaluesstatus = $('.' + checkboxId + ':checked').map(function () {
        return $(this).attr('id') + "|" + $(this).attr('data-status');
    }).get();

    return checkedvaluesstatus;
}

var checkedAPIForm = "";
function fnGetChkAPIForm(checkboxId) {
    checkedAPIForm = $('.' + checkboxId + ':checked').map(function () {
        return $(this).attr('data-isapiform');
    }).get();

    return checkedAPIForm;
}

var checkedvalues = "";
function fnGetChkMrkMsgvalGrid(checkboxId) {
    checkedvalues = $('.' + checkboxId + ':checked').map(function () {
        return $(this).attr('data-id');
    }).get();
    return checkedvalues;
}

var checkedmsgstvalues = "";
function fnGetChkMrkMsgStValGrid(checkboxId) {
    checkedmsgstvalues = $('.' + checkboxId + ':checked').map(function () {
        return $(this).attr('data-id') + "|" + $(this).attr('data-status');
    }).get();
    return checkedmsgstvalues;
}

function DeleteBuyer(oppcontactmapId) {
    alertifyReset("Delete Buyer", "Cancel");
    var confirmMesaage = "[|Are you sure you want to delete this buyer|]?";
    //commonbuyerdelete(confirmMesaage, oppcontactmapId);
};


