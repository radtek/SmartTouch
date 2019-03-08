var dashBoardViewModel = function (BASE_URL, Contact_URL, reportPermissions, URL, Report_URL, settings, isStAdmin) {

    selfDashBoard = this;

    selfDashBoard.Settings = ko.observableArray(settings);

   
    selfDashBoard.ActionContactId = ko.observable();
    selfDashBoard.campaginList = ko.observableArray([]);
    selfDashBoard.NewLeadsTotalCount = ko.observable();
    selfDashBoard.NewLeadsPreviousCount = ko.observable();

    selfDashBoard.UntouchedLeadsTotalCount = ko.observable();
    selfDashBoard.UntouchedLeadsPreviousCount = ko.observable();

    selfDashBoard.NewLeadsPreviousCountPercentage = ko.observable();
    selfDashBoard.UntouchedLeadsPreviousCountPercentage = ko.observable();

    selfDashBoard.hasReportPermission = ko.observable(reportPermissions);
    selfDashBoard.IsSTAdmin = ko.observable(isStAdmin);
    selfDashBoard.TrafficSourceTotalCount = ko.observable();
    selfDashBoard.TrafficSourcePreviousCount = ko.observable();
    selfDashBoard.TrafficSourcePreviousCountPercentage = ko.observable();
    selfDashBoard.ContactsCount = ko.observable(0);
    selfDashBoard.CampaignsCount = ko.observable(0);
    selfDashBoard.ToursByLeadSourceLink = ko.observable();
    selfDashBoard.NewContactsLink = ko.observable();
    selfDashBoard.unTouchedContactsLink = ko.observable();
    selfDashBoard.ToursByTypeLink = ko.observable();
    selfDashBoard.HotListLink = ko.observable();
    selfDashBoard.CampaignListlink = ko.observable();
    selfDashBoard.Opportunitylink=ko.observable();
    selfDashBoard.GetAllActionsList = ko.observable();
    selfDashBoard.TrafficBySourceLink = ko.observable();
    selfDashBoard.NewLeadsLink = ko.observable();


    selfDashBoard.NewLeadsAreaChartDetails = ko.observableArray([]);
    selfDashBoard.NewLeadsPreviousDateAreaChartDetails = ko.observableArray([]);

    selfDashBoard.UntouchedLeadsAreaChartDetails = ko.observableArray([]);
    selfDashBoard.UntouchedLeadsPreviousDateAreaChartDetails = ko.observableArray([]);  

    selfDashBoard.TourByTypeBarChartDetails = ko.observableArray([]);
    selfDashBoard.NewLeadsPieChartDetails = ko.observableArray([]);
    selfDashBoard.TourByTypeFunnelChartDetails = ko.observableArray([]);
    selfDashBoard.TourBySourcePieChartDetails = ko.observableArray([]);
    selfDashBoard.TourBySourceAreaChartdetails = ko.observableArray([]);

    selfDashBoard.NewLeadsAreaChartVisible = ko.observable();
    selfDashBoard.UnTouchedLeadsAreaChartVisible = ko.observable();

    selfDashBoard.NewLeadsPieChartVisible = ko.observable();
    selfDashBoard.TourbySourceAreaChartVisible = ko.observable();
    selfDashBoard.TourbySourcePieChartVisible = ko.observable();
    selfDashBoard.TourbyTypeBarChartVisible = ko.observable();
    selfDashBoard.TourbyTypeFunnelChartVisible = ko.observable();

    selfDashBoard.NewLeadsPieChartVisible = ko.observable();
    selfDashBoard.contactLeadScoreList = ko.observableArray([]);
    selfDashBoard.Messages = ko.observableArray([]);
    selfDashBoard.TimeInterval = ko.observable();
    selfDashBoard.MyCommunication = ko.observableArray([]);
    selfDashBoard.MyCommunicationGridVisible = ko.observable();
    selfDashBoard.MyCommunicationChartVisible = ko.observable();
    selfDashBoard.Period = ko.observable('1');
   
    var value = readCookie("MessagesCookie");
    if (value == null)
    {
        $.ajax({
            url: '/Dashboard/GetAllMarketingMessages',
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data.response.length > 0) {          
                    $('.stm-notification-wrapper').removeClass('hide');
                    selfDashBoard.Messages(data.response);
                    var value = selfDashBoard.Messages()[0].TimeInterval;
                    $('.carousel').carousel({
                        interval: value * 1000
                     })
                }
              
            },
            error: function () {
                
            }
        });
    }
   
 




    

    $('.stm-notification').on("click", "#msg-rm", function () {
        $('.stm-notification-wrapper').slideUp('slow', function () {
            $('.stm-notification-wrapper').remove();
            createCookie("MessagesCookie", "1", 1);
        });

    });

    if ((selfDashBoard.Settings()[0].Value == true) || (selfDashBoard.Settings()[3].Value == true)) {
        $.ajax({
            url: '/Dashboard/GetNewLeadsChartDetails',
            type: 'get',          
            dataType: 'json',
            contentType: "application/json; charset=utf-8"
        })
            .then(function (response) {
                var filter = $.Deferred();
                if (response.success) {
                    filter.resolve(response);
                } else {
                    filter.reject(response.error);
                }
                return filter.promise();
            }).done(function (data) {
                var newLeads = data.response;
                selfDashBoard.NewLeadsPieChartDetails(newLeads.TopFive);
                if (newLeads.TopLeads != null && newLeads.TopLeads.length > 0) {
                    selfDashBoard.NewLeadsAreaChartVisible('True');
                    newLeadsTotals(newLeads.TopLeads, newLeads.TopPreviousLeads);
                    selfDashBoard.NewLeadsAreaChartDetails(newLeads.TopLeads);
                    selfDashBoard.NewLeadsPreviousDateAreaChartDetails(newLeads.TopPreviousLeads);
                    newLeadsAreaChart(newLeads.TopLeads, newLeads.TopPreviousLeads);
                }
                else
                    selfDashBoard.NewLeadsAreaChartVisible('False');
                if (newLeads.TopFive != null && newLeads.TopFive.length > 0) {
                    selfDashBoard.NewLeadsPieChartVisible('True');
                    newLeadsPieChart(newLeads.TopFive);
                }
                else
                    selfDashBoard.NewLeadsPieChartVisible('False');

                if (newLeads.ReportId != null)
                    selfDashBoard.NewContactsLink("/editreport?reportType=3&ReportID=" + newLeads.ReportId + "&runReportResults=True");
                selfDashBoard.NewLeadsLink("/editreport?reportType=3&ReportID=" + newLeads.ReportId + "&runReportResults=True");

            }).fail(function (error) {
                // Display error message to user            
                selfDashBoard.NewLeadsAreaChartVisible('False'); selfDashBoard.NewLeadsPieChartVisible('False');
                notifyError(error);
            });
    }


    if ((selfDashBoard.Settings()[1].Value == true)) {
        $.ajax({
            url: '/Dashboard/GetUnTouchedLeadsChartDetails',
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8"
        })
           .then(function (response) {
               var filter = $.Deferred();
               if (response.success) {
                   filter.resolve(response);
               } else {
                   filter.reject(response.error);
               }
               return filter.promise();
           }).done(function (data) {
               var untouchedLeads = data.response;
              
               if (untouchedLeads.TopLeads != null && untouchedLeads.TopLeads.length > 0) {
                   selfDashBoard.UnTouchedLeadsAreaChartVisible('True');
                   untouchedLeadsTotals(untouchedLeads.TopLeads, untouchedLeads.TopPreviousLeads);

                   selfDashBoard.UntouchedLeadsAreaChartDetails(untouchedLeads.TopLeads);
                   selfDashBoard.UntouchedLeadsPreviousDateAreaChartDetails(untouchedLeads.TopPreviousLeads);
                   UntouchedLeadsAreaChart(untouchedLeads.TopLeads, untouchedLeads.TopPreviousLeads);
               }
               else
                   selfDashBoard.UnTouchedLeadsAreaChartVisible('False');

               if (untouchedLeads != null) {
                   if (data.success == true) {
                       localStorage.setItem("ContactsGuid", untouchedLeads.ContactsGuid);
                       var url = '../ncrcontacts?guid=' + null + '&reportType=' + 3 + '&reportId=' + null;
                       selfDashBoard.unTouchedContactsLink(url);
                     //  window.location.href = '../contactresults?guid=' + untouchedLeads.ContactsGuid + '&reportType='+null+'&reportId='+null;
                   }
               }
          
           }).fail(function (error) {
               // Display error message to user            
               selfDashBoard.UnTouchedLeadsAreaChartVisible('False'); 
               notifyError(error);
           });
    }

    //calling on pageload

    if (selfDashBoard.Settings()[2].Value == true) {
        $.ajax({
            url: '/Dashboard/GetTrafficBySourceAreaChartDetails',
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8"
        })
            .then(function (response) {
                var filter = $.Deferred();
                if (response.success) {
                    filter.resolve(response);
                } else {
                    filter.reject(response.error);
                }
                return filter.promise();
            }).done(function (data) {
                var toursByLeadSource = data.response;
                if (toursByLeadSource.Chart1Details != null && toursByLeadSource.Chart1Details.length > 0) {
                    selfDashBoard.TourbySourceAreaChartVisible('True');
                    selfDashBoard.TourBySourceAreaChartdetails(toursByLeadSource.Chart1Details);
                    trafficBySourceAreaChart(toursByLeadSource.Chart1Details);
                    selfDashBoard.TrafficSourceTotalCount(toursByLeadSource.PresentCount);
                    selfDashBoard.TrafficSourcePreviousCount(toursByLeadSource.PreviousCount);
                    selfDashBoard.TrafficSourcePreviousCountPercentage(trafficSourcePercentage(toursByLeadSource.PresentCount, toursByLeadSource.PreviousCount));
                }
                else
                    selfDashBoard.TourbySourceAreaChartVisible('False');
                if (toursByLeadSource.ReportId != null)
                    selfDashBoard.ToursByLeadSourceLink("/editreport?reportType=8&ReportID=" + toursByLeadSource.ReportId + "&runReportResults=True");

                selfDashBoard.TrafficBySourceLink("/editreport?reportType=8&ReportID=" + toursByLeadSource.ReportId + "&runReportResults=True");

            }).fail(function (error) {
                notifyError(error);
                selfDashBoard.TourbySourceAreaChartVisible('False');
            });
    }

    if (selfDashBoard.Settings()[4].Value == true) { 
        $.ajax({
            url: '/Dashboard/GetTrafficBySourcePieChartDetails',
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8"
        })
          .then(function (response) {
              var filter = $.Deferred();
              if (response.success) {
                  filter.resolve(response);
              } else {
                  filter.reject(response.error);
              }
                return filter.promise();
            }).done(function (data) {
              var toursByLeadSource = data.response;

              if (toursByLeadSource.Chart2Details != null && toursByLeadSource.Chart2Details.length > 0) {
                  selfDashBoard.TourbySourcePieChartVisible('True');
                  selfDashBoard.TourBySourcePieChartDetails(toursByLeadSource.Chart2Details);
                  trafficBySourcePieChart(toursByLeadSource.Chart2Details);
              }
              else
                  selfDashBoard.TourbySourcePieChartVisible('False');

              if (toursByLeadSource.ReportId != null)
                  selfDashBoard.ToursByLeadSourceLink("/editreport?reportType=8&ReportID=" + toursByLeadSource.ReportId + "&runReportResults=True");


          }).fail(function (error) {
              selfDashBoard.TourbySourcePieChartVisible('False');
              notifyError(error);
          });
    }

    var TourByTypeMaxValue = 0;
    var monthvalue;
    //calling on pageload

    if ((selfDashBoard.Settings()[2].Value == true)) {
        $.ajax({
            url: '/Dashboard/GetTrafficByTypeBarChartDetails',
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8"
        })
            .then(function (response) {
                var filter = $.Deferred();
                if (response.success) {
                    filter.resolve(response);
                } else {
                    filter.reject(response.error);
                }
                return filter.promise();
            }).done(function (data) {
                var toursByType = data.response;
                //  selfDashBoard.TourByTypeFunnelChartDetails(toursByType.Chart2Details);
                monthvalue = moment.months()[parseInt(new Date().getMonth())];
                if (toursByType.Chart1Details != null && toursByType.Chart1Details.length > 0) {
                    selfDashBoard.TourbyTypeBarChartVisible('True');
                    selfDashBoard.TourByTypeBarChartDetails(toursByType.Chart1Details);
                    TourByTypeMaxValue = toursByType.MaxValue;
                    trafficByType(toursByType.Chart1Details, toursByType.MaxValue);

                    if (toursByType.ReportId != null)
                        selfDashBoard.ToursByTypeLink("/editreport?reportType=7&ReportID=" + toursByType.ReportId + "&runReportResults=True");
                }
                else
                    selfDashBoard.TourbyTypeBarChartVisible('False');
                //if (toursByType.Chart2Details != null && toursByType.Chart2Details.length > 0) {
                //    selfDashBoard.TourbyTypeFunnelChartVisible('True');
                //    OppertunityChart(toursByType.Chart2Details, monthvalue);
                //}
                //else
                //    selfDashBoard.TourbyTypeFunnelChartVisible('False');


            }).fail(function (error) {

                selfDashBoard.TourbyTypeBarChartVisible('False');
                notifyError(error);
            });
    }
    //calling on pageload

    if (selfDashBoard.Settings()[5].Value == true) {
        $.ajax({
            url: '/Dashboard/GetTrafficByTypeFunnelChartDetails',
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8"
    })
          .then(function (response) {
              var filter = $.Deferred();
              if (response.success) {
                  filter.resolve(response);
              } else {
                  filter.reject(response.error);
              }
                return filter.promise();
            }).done(function (data) {
              var toursByType = data.response;
              selfDashBoard.TourByTypeFunnelChartDetails(toursByType.Chart2Details);
                monthvalue = moment.months()[parseInt(new Date().getMonth())];

              if (toursByType.Chart2Details != null && toursByType.Chart2Details.length > 0) {
                  selfDashBoard.TourbyTypeFunnelChartVisible('True');
                  OppertunityChart(toursByType.Chart2Details, monthvalue);
                  if (toursByType.ReportId != null) {
                      selfDashBoard.Opportunitylink("/editreport?reportType=4&ReportID=" + toursByType.ReportId + "&runReportResults=True");
                  }
              }
              else
                  selfDashBoard.TourbyTypeFunnelChartVisible('False');


          }).fail(function (error) {
              selfDashBoard.TourbyTypeBarChartVisible('False'); selfDashBoard.TourbyTypeFunnelChartVisible('False');
              notifyError(error);

          });
    }

    if (selfDashBoard.Settings()[6].Value == true) {
        $.ajax({
            url: '/Dashboard/GetHotListContacts',
            type: 'get',
            dataType: 'json',
            contentType: "application/json; charset=utf-8"
        }).then(function(response) {
            removepageloader();
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response);
            } else {
                filter.reject(response.error);
            }
            return filter.promise();
        }).done(function(data) {
            selfDashBoard.contactLeadScoreList(data.response.HotlistGridData.HotlistData);
            if (data.response.HotlistGridData.HotlistData != null)
                selfDashBoard.ContactsCount(data.response.HotlistGridData.HotlistData.length);
            selfDashBoard.HotListLink("/editreport?reportType=1&ReportID=" + data.response.ReportId + "&runReportResults=True");
        }).fail(function(error) {
            notifyError(error);
        });
    }

    if (selfDashBoard.Settings()[7].Value == true) {
        //calling on pageload
        $.ajax({
            url: '/Dashboard/GetActiveCampaignList',
            type: 'get',
            dataType: 'json',

            contentType: "application/json; charset=utf-8"
        }).then(function(response) {
            removepageloader();
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response);
            } else {
                filter.reject(response.error);
            }
            return filter.promise();
        }).done(function(data) {
            selfDashBoard.campaginList(data.response.ReportList);
            selfDashBoard.CampaignListlink("/editreport?reportType=2&ReportID=" + data.response.ReportId + "&runReportResults=True");
            if (data.response.ReportList != null);
            selfDashBoard.CampaignsCount(data.response.ReportList.length);
        }).fail(function(error) {
            notifyError(error);
        });
    }

    // For My Communications
    if (selfDashBoard.Settings()[10].Value == true) {
        //calling on pageload
        $.ajax({
            url: '/Dashboard/GetMyCommunicationData',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'period': 1 })
        }).then(function (response) {
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response);
            } else {
                filter.reject(response.error);
            }
            return filter.promise();
        }).done(function (data) {
            selfDashBoard.MyCommunication(data.response);
            selfDashBoard.MyCommunicationGridVisible('True');
        }).fail(function (error) {
            selfDashBoard.MyCommunicationGridVisible('False');
            notifyError(error);
        });
    }

    selfDashBoard.saveUserSettings = function () {
        var data = ko.toJSON(selfDashBoard);
        pageLoader();
        $.ajax({
            url: '/Reports/InsertUserSettings',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'viewModel': data })
        }).then(function(response) {
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response);
            } else {
                filter.reject(response.error);
            }
            return filter.promise();
        }).done(function() {
            notifySuccess('[|Successfully saved dashboard settings|]');
            setTimeout(function() { location.reload(); }, setTimeOutTimer);
        }).fail(function() {
            removepageloader();
            notifyError(data.response);
        });
    };


    var trafficSourcePercentage = function (present, previous) {

        var percentage = 0;
        if (parseInt(previous) > 0 && parseInt(present) > 0) {
            if (present < previous)
                percentage = Math.round(((parseInt(previous) - parseInt(present)) / parseInt(previous)) * 100);
            else
                percentage = Math.round(((parseInt(present) - parseInt(previous)) / parseInt(present)) * 100);
        }

        else if (parseInt(previous) == 0)
            percentage = 100;
        return percentage;
    }

    var newLeadsTotals = function (newLeadsList, previousLeadsList) {
        var newLeadsSum = 0;
        var previousLeadsSum = 0;
        var totalNewLeads = ko.utils.arrayForEach(newLeadsList, function (type) {
            newLeadsSum += type.Value;
        });
        var previousSumOfLeads = ko.utils.arrayForEach(previousLeadsList, function (type) {
            previousLeadsSum += type.Value;
        });
        selfDashBoard.NewLeadsTotalCount(newLeadsSum);

        var percentage = 0;
        if (parseInt(previousLeadsSum) > 0 && parseInt(newLeadsSum) > 0) {
            if (newLeadsSum < previousLeadsSum)
                percentage = Math.round(((parseInt(previousLeadsSum) - parseInt(newLeadsSum)) / parseInt(previousLeadsSum)) * 100);
            else
                percentage = Math.round(((parseInt(newLeadsSum) - parseInt(previousLeadsSum)) / parseInt(newLeadsSum)) * 100);
        }
        else if (parseInt(previousLeadsSum) == 0)
            percentage = 100;
        selfDashBoard.NewLeadsPreviousCount(previousLeadsSum);
        selfDashBoard.NewLeadsPreviousCountPercentage(Math.round(percentage));

    }


    var untouchedLeadsTotals = function (untouchedLeadsList, previousUntouchedLeadsList) {
        var untouchedLeadsSum = 0;
        var previousUntouchedLeadsSum = 0;
        var totalNewLeads = ko.utils.arrayForEach(untouchedLeadsList, function (type) {
            untouchedLeadsSum += type.Value;
        });
        var previousSumOfLeads = ko.utils.arrayForEach(previousUntouchedLeadsList, function (type) {
            previousUntouchedLeadsSum += type.Value;
        });
        selfDashBoard.UntouchedLeadsTotalCount(untouchedLeadsSum);

        var percentage = 0;
        if (parseInt(previousUntouchedLeadsSum) > 0 && parseInt(untouchedLeadsSum) > 0) {
            if (untouchedLeadsSum < previousUntouchedLeadsSum)
                percentage = Math.round(((parseInt(previousUntouchedLeadsSum) - parseInt(untouchedLeadsSum)) / parseInt(previousUntouchedLeadsSum)) * 100);
            else
                percentage = Math.round(((parseInt(untouchedLeadsSum) - parseInt(previousUntouchedLeadsSum)) / parseInt(untouchedLeadsSum)) * 100);
        }
        else if (parseInt(previousUntouchedLeadsSum) == 0)
            percentage = 100;
        selfDashBoard.UntouchedLeadsPreviousCount(previousUntouchedLeadsSum);
        selfDashBoard.UntouchedLeadsPreviousCountPercentage(Math.round(percentage));

    }


    selfDashBoard.Actions = ko.observableArray([]);

    selfDashBoard.ActionCompletedMessage = ko.observable();
    selfDashBoard.CompletedActionOption = ko.observable();
    selfDashBoard.MarkAsCompleteActionId = ko.observable();
    selfDashBoard.MarkAsCompleteStatus = ko.observable();
    selfDashBoard.ActionContactId = ko.observable(null);
    selfDashBoard.ActionOpportunityId = ko.observable();

    selfDashBoard.DisplaywithDateTimeFormat = function (date) {
        var dateFormat = readCookie("dateformat").toUpperCase();
        if (date == null) {
            return "";
        }
        var utzDate = new Date(moment(date).toDate()).ToUtcUtzDate();
        return moment(utzDate).format(dateFormat + " hh:mm A");
    };

    //selfDashBoard.GetActions();


    selfDashBoard.CompletedOperation = function (Action) {
        //var action = "ActionCompletedOperation";
        var arrowicon = $("#Arrow" + Action.ActionId);
        arrowicon.toggleClass('hide');
        selfDashBoard.MarkAsCompleteActionId(Action.ActionId);
        pageLoader();
        if (Action.IsCompleted) {
            selfDashBoard.ActionCompletedMessage("[|Are you sure you want to mark this Action as Completed|]?");
            selfDashBoard.CompletedActionOption("[|OK|]");
        }
        else {
            selfDashBoard.ActionCompletedMessage("[|Are you sure you want to mark this Action as not Completed|]?");
            selfDashBoard.CompletedActionOption("[|OK|]");
        }
        selfDashBoard.ActionOpportunityId(Action.OppurtunityId);
        selfDashBoard.MarkAsCompleteStatus(Action.IsCompleted);
        $('#myModal').modal('show');
        removepageloader();
       
        return false;
    };

    selfDashBoard.Completed = function () {
        var forAll = true;
        var action = "ActionCompleted";
        pageLoader();
        $.ajax({
            url: Contact_URL + action,
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                'actionID': selfDashBoard.MarkAsCompleteActionId(),
                'isActionCompleted': selfDashBoard.MarkAsCompleteStatus(),
                'contactId': selfDashBoard.ActionContactId(),
                'completedForAll': forAll,
                'opportunityId': selfDashBoard.ActionOpportunityId()
            })
        }).then(function (response) {
            removepageloader();
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response);
            } else {
                filter.reject(response.error);
            }
            return filter.promise();
        }).done(function () {
            $('.success-msg').remove();           
              
                if (selfDashBoard.MarkAsCompleteStatus()) {
                    notifySuccess('[|Action marked as completed.|]');
                    $('#myModal').modal('hide');
                }
                else {
                    notifySuccess('[|Action marked as not completed.|]');
                    $('#myModal').modal('hide');
                }
            

        }).fail(function (error) {         
            notifyError(data.response);
        });
        return false;
    };


    selfDashBoard.GetActions = function () {
        if (selfDashBoard.Settings()[9].Value == true) {
            $.ajax({
                url: 'Dashboard/GetUserCreatedActions',
                type: 'post',
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'pageNumber': 1, 'limit': 10 })

            }).then(function (response) {
                var filter = $.Deferred();
                if (response.success) {
                    filter.resolve(response);
                } else {
                    filter.reject(response.error);
                }
                return filter.promise();
            }).done(function (data) {
                selfDashBoard.Actions(data.response);
              
            }).fail(function (error) {
                notifyError(error);
            });
            //to get the actions for this user
        }
    }

    selfDashBoard.GetRemindOn = function (date) {

        var pattern = /Date\(([^)]+)\)/;
        if (date != null) {
            var results = pattern.exec(date);
            var dateFormat = readCookie("dateformat").toUpperCase();
         
            var utzDate = new Date(moment(results).toDate()).ToUtcUtzDate();
            return moment(utzDate).format(dateFormat + " hh:mm A");
        }
        return "";
    }


    selfDashBoard.PageNumber = ko.observable(1);
    selfDashBoard.Limit = ko.observable(10);

    //function veCarousel(vecarouselid) {
    //    jQuery('#' + vecarouselid).jcarousel({
    //        vertical: true,
    //        scroll: 2
    //    });
    //}
    //calling on pageload
    //selfDashBoard.GetActions();

    selfDashBoard.Calender = ko.observable(new calenderViewModel());

    //selfDashBoard.Settings = ko.observableArray([
    //    { Id: 0, Report: "NewLeads", Value: ko.observable() },
    //    { Id: 1, Report: "Traffic", Value: ko.observable() },
    //    { Id: 2, Report: "TrafficTypes", Value: ko.observable() },
    //    { Id: 3, Report: "TopFiveLeadSources", Value: ko.observable() },
    //    { Id: 4, Report: "TopFiveTraffficSources", Value: ko.observable() },
    //    { Id: 5, Report: "PipeLine", Value: ko.observable() },
    //    { Id: 6, Report: "HotList", Value: ko.observable() },
    //    { Id: 7, Report: "CampaignList", Value: ko.observable() },
    //    { Id: 8, Report: "MyCalender", Value: ko.observable() },
    //    { Id: 9, Report: "MyActions", Value: ko.observable() }
    //]);






    //selfDashBoard.Settings = ko.observableArray([]);
    //$.ajax({
    //    url: url + 'GetSettings',
    //    type: 'get',
    //    dataType: 'json',
    //    contentType: "application/json; charset=utf-8"
    //}).then(function (response) {
    //    var filter = $.Deferred()
    //    if (response.success) {
    //        filter.resolve(response)
    //    } else {
    //        filter.reject(response.error)
    //    }
    //    return filter.promise()
    //}).done(function (data) {
    //    selfDashBoard.Settings(data.response);
    //}).fail(function (error) {
    //    notifyError(error);
    //})




    selfDashBoard.Settings.subscribe(function () {
        selfDashBoard.setUnsubscribe = function (data, event) {
            selfDashBoard.Settings(event.target.checked);
        }
    });



    selfDashBoard.WidgetsCount = ko.pureComputed(function () {
        return selfDashBoard.Settings().filter(function (e) { return e.Value === true }).length;
        //selfDashBoard.Settings().length;
    });

    selfDashBoard.hideSettings = function () {

        $(".setting-st.da-header-specific.ptl").hide();
    }
    var trafficBySourcePieChart = function (toursByLeadSource) {
        $("#datoptrafficsource").kendoChart({
            legend: {
                position: "bottom"
            },
            chartArea: {
                background: ""
            },
            theme: 'Metro',
            seriesDefaults: {
                labels: {
                    visible: false,
                    background: "transparent",
                    template: "#= category #: #= value#"
                }
            },
            dataSource: {
                data: toursByLeadSource
            },
            series: [{
                type: "pie",
                field: "TotalCount",
                startAngle: 150,
                categoryField: "DropdownValue",
                data: selfDashBoard.TourBySourcePieChartDetails()
            }],
            seriesClick: function (e) {
                //   eraseCookie("TourByTypeDefaultFilter");
                if (selfDashBoard.IsSTAdmin() == "True") {
                    console.log(e.dataItem.DropdownValueID);
                    createCookie("TourBySourceDefaultFilter", e.dataItem.DropdownValueID, 1);
                    window.location.href = selfDashBoard.ToursByLeadSourceLink();
                }
               else if (selfDashBoard.hasReportPermission() == "True") {
                   console.log(e.dataItem.DropdownValueID);
                   createCookie("TourBySourceDefaultFilter", e.dataItem.DropdownValueID, 1);
                   window.location.href = selfDashBoard.ToursByLeadSourceLink();
                }   
            },
            seriesColors: ["#ef7373", "#e6e473", "#79d082", "#efa76d", "#5cc0f4"],
            tooltip: {
                visible: true,
                template: "${category}- ${ value }"
            }

        });

    }
    var newLeadsPieChart = function (newLeads) {



        $("#datopleadsource").kendoChart({
            legend: {
                position: "bottom"
            },
            chartArea: {
                background: ""
            },
            theme: 'Metro',
            seriesDefaults: {
                labels: {
                    visible: false,
                    background: "transparent",
                    template: "#= category #: #= value#"
                }
            },
            dataSource: {
                data: newLeads
            },
            series: [{
                type: "pie",
                field: "Value",
                startAngle: 150,
                categoryField: "Name",
                data: selfDashBoard.NewLeadsPieChartDetails()
            }],
            seriesClick: function (e) {
                //   eraseCookie("TourByTypeDefaultFilter");
                if (selfDashBoard.IsSTAdmin() == "True") {
                    console.log(e.dataItem.DropdownValueID);
                    createCookie("NeWContactseDefaultFilter", e.dataItem.DropdownValueID, 1);
                    window.location.href = selfDashBoard.NewContactsLink();
                }
               else if (selfDashBoard.hasReportPermission() == "True")
                 {
                   console.log(e.dataItem.DropdownValueID);
                   createCookie("NeWContactseDefaultFilter", e.dataItem.DropdownValueID, 1);
                   window.location.href = selfDashBoard.NewContactsLink();
                }
            },
            seriesColors: ["#ef7373", "#e6e473", "#79d082", "#efa76d", "#5cc0f4"],
            tooltip: {
                visible: true,
                template: "${category}- ${ value }"
            }

        });

    }

    var myCommunicationsPieChart = function (communicationsData) {
        $("#mycommunicationda").kendoChart({
            legend: {
                position: "bottom"
            },
            chartArea: {
                background: ""
            },
            theme: 'Metro',
            seriesDefaults: {
                labels: {
                    visible: false,
                    background: "transparent",
                    template: "#= category #: #= value#"
                }
            },
            dataSource: {
                data: communicationsData
            },
            series: [{
                type: "pie",
                field: "TotalCount",
                startAngle: 150,
                categoryField: "Name",
                data: selfDashBoard.MyCommunication()
            }],
            seriesClick: function (e) {
                var activityType = "";
                if (e.dataItem.DropdownType == "E")
                    activityType = e.dataItem.Name == "Emails Delivered" ? "D" : "O";
                else if (e.dataItem.DropdownType == "C")
                    activityType = e.dataItem.Name == "Campaigns Delivered" ? "D" : "O";
                else
                    activityType = e.dataItem.ID;

                window.location.href = "../mycommunicationcontacts?activity=" + e.dataItem.ActivityType + '&period=' + selfDashBoard.Period() + '&entity=' + activityType
            },
            seriesColors: ["#ef7373", "#e6e473", "#79d082", "#efa76d", "#5cc0f4", "#ff9e00", "#00b8ff", "#7494a0", "#dfebf0", "#427b8a", "#6b9b98", "#549ed6", "#639bb4", "#00b8ff", "#7dbf94", "#e7ca52", "#ffb300", "#f6dccc", "#4a899a"],
            tooltip: {
                visible: true,
                template: "${category}- ${ value }"
            }

        });

    }

    var newLeadsAreaChart = function (results, previousResults) {
       
        if ((results.length > previousResults.length) || (results.length = previousResults.length)) {
            $.each(previousResults, function (index, previous_Value) {
                results[index].PreviousValue = previous_Value.Value;
            });
          
            applyDataToAreaChart_1(results);
        }
        else if ((results.length < previousResults.length)) {
            $.each(results, function (index, previous_Value) {
                previousResults[index].PreviousValue = previous_Value.Value;
            });
           
            applyDataToAreaChart_2(previousResults);
        }   
    }


    var UntouchedLeadsAreaChart = function (results, previousResults) {
        if ((results.length > previousResults.length) || (results.length = previousResults.length)) {
            $.each(previousResults, function (index, previous_Value) {
                if (previous_Value) {
                    if (results[index])
                        results[index].PreviousValue = previous_Value.Value;
                    else
                        results = results.slice(index, -1);
                }

            });

            applyDataToUnTouchedAreaChart_1(results);
        }
        else if ((results.length < previousResults.length)) {
            $.each(results, function (index, previous_Value) {
                if (previous_Value) {
                    if(previousResults[index])
                        previousResults[index].PreviousValue = previous_Value.Value;
                    else
                        previousResults = previousResults.slice(index, -1);
                }
                
            });

            applyDataToUnTouchedAreaChart_2(previousResults);
        }
    }




    var applyDataToAreaChart_1 = function (results) {
        $("#danewleads").kendoChart({
            legend: {
                position: "bottom",
                visible: false
            },
            seriesDefaults: {
                type: "area",
                area: {
                    line: {
                        style: "smooth"
                    }
                }
            },
            theme: 'Metro',
            series: [{
                name: "Current",
                field: "Value",
                categoryField: "Name",
                data: results
            }, {
                name: "Previous",
                field: "PreviousValue",
                categoryField: "Name",
                data: results
            }],

            valueAxis: {
                labels: {
                    visible: false,
                    format: "{0}%"
                },
                line: {
                    visible: true
                },
                majorGridLines: {
                    visible: false
                },
                visible: false
            },
            categoryAxis: {
                // categories: [2002, 2003, 2004, 2005, 2006, 2007, 2008, 2009, 2010, 2011],
                visible: false,
                majorGridLines: {
                    visible: false
                }
            },
            tooltip: {
                visible: true,
                format: "{0}%",
                template: "#= series.name #: #= value #"
            }
        });
    }


    var applyDataToAreaChart_2 = function (results) {
        $("#danewleads").kendoChart({
            legend: {
                position: "bottom",
                visible: false
            },
            seriesDefaults: {
                type: "area",
                area: {
                    line: {
                        style: "smooth"
                    }
                }
            },
            theme: 'Metro',
            series: [{
                name: "Current",
                field: "PreviousValue",
                categoryField: "Name",
                data: results
            }, {
                name: "Previous",
                field: "Value",
                categoryField: "Name",
                data: results
            }],

            valueAxis: {
                labels: {
                    visible: false,
                    format: "{0}%"
                },
                line: {
                    visible: true
                },
                majorGridLines: {
                    visible: false
                },
                visible: false
            },
            categoryAxis: {
                // categories: [2002, 2003, 2004, 2005, 2006, 2007, 2008, 2009, 2010, 2011],
                visible: false,
                majorGridLines: {
                    visible: false
                }
            },
            tooltip: {
                visible: true,
                format: "{0}%",
                template: "#= series.name #: #= value #"
            }
        });
    }



    var applyDataToUnTouchedAreaChart_1 = function (results) {
        $("#dauntouchedleads").kendoChart({
            legend: {
                position: "bottom",
                visible: false
            },
            seriesDefaults: {
                type: "area",
                area: {
                    line: {
                        style: "smooth"
                    }
                }
            },
            theme: 'Metro',
            series: [{
                name: "Current",
                field: "Value",
                categoryField: "Name",
                data: results
            }, {
                name: "Previous",
                field: "PreviousValue",
                categoryField: "Name",
                data: results
            }],

            valueAxis: {
                labels: {
                    visible: false,
                    format: "{0}%"
                },
                line: {
                    visible: true
                },
                majorGridLines: {
                    visible: false
                },
                visible: false
            },
            categoryAxis: {
                // categories: [2002, 2003, 2004, 2005, 2006, 2007, 2008, 2009, 2010, 2011],
                visible: false,
                majorGridLines: {
                    visible: false
                }
            },
            tooltip: {
                visible: true,
                format: "{0}%",
                template: "#= series.name #: #= value #"
            }
        });
    }


    var applyDataToUnTouchedAreaChart_2 = function (results) {
        $("#dauntouchedleads").kendoChart({
            legend: {
                position: "bottom",
                visible: false
            },
            seriesDefaults: {
                type: "area",
                area: {
                    line: {
                        style: "smooth"
                    }
                }
            },
            theme: 'Metro',
            series: [{
                name: "Current",
                field: "PreviousValue",
                categoryField: "Name",
                data: results
            }, {
                name: "Previous",
                field: "Value",
                categoryField: "Name",
                data: results
            }],

            valueAxis: {
                labels: {
                    visible: false,
                    format: "{0}%"
                },
                line: {
                    visible: true
                },
                majorGridLines: {
                    visible: false
                },
                visible: false
            },
            categoryAxis: {
                // categories: [2002, 2003, 2004, 2005, 2006, 2007, 2008, 2009, 2010, 2011],
                visible: false,
                majorGridLines: {
                    visible: false
                }
            },
            tooltip: {
                visible: true,
                format: "{0}%",
                template: "#= series.name #: #= value #"
            }
        });
    }

    var trafficBySourceAreaChart = function (presentResults) {
        // selfDashBoard.TrafficBySourceAreaChartData = ko.observableArray(presentResults);
        if (selfDashBoard.TourBySourceAreaChartdetails().length == 1) {
            var dateValue = selfDashBoard.TourBySourceAreaChartdetails()[0].DateNumber;
            var presentValue = selfDashBoard.TourBySourceAreaChartdetails()[0].Present;
            var previousValue = selfDashBoard.TourBySourceAreaChartdetails()[0].Previous;
            selfDashBoard.TourBySourceAreaChartdetails.removeAll();
            selfDashBoard.TourBySourceAreaChartdetails.push({ Present: 0, Previous: 0, DateNumber: parseInt(dateValue) + 1 });
            selfDashBoard.TourBySourceAreaChartdetails.push({ Present: presentValue, Previous: previousValue, DateNumber: parseInt(dateValue) });
        }

        $("#datrafficbySource").kendoChart({

            legend: {
                position: "bottom",
                visible: false
            },
            seriesDefaults: {
                type: "area",
                area: {
                    line: {
                        style: "smooth"

                    }
                }
            },
            theme: 'Metro',

            series: [{
                name: "Current",
                field: "Present",
                categoryField: "DateNumber",
                data: selfDashBoard.TourBySourceAreaChartdetails()
            }, {
                name: "Previous",
                field: "Previous",
                categoryField: "DateNumber",
                data: selfDashBoard.TourBySourceAreaChartdetails()
            }],
            valueAxis: {
                labels: {
                    visible: false,
                    format: "{0}%"
                },
                line: {
                    visible: false
                },
                majorGridLines: {
                    visible: false
                },
                visible: false
            },
            categoryAxis: {
                visible: false,

                majorGridLines: {
                    visible: false
                }
            },
            tooltip: {
                visible: true,
                format: "{0}%",
                template: "#= series.name #: #= value #"
            }
        });

    }
    var OppertunityChart = function CreateChart(data, monthValue) {


        $('#dalifecyclepipeline').kendoChart({
            title: {
                text: monthValue,
                position: "bottom"
            },
            legend: {
                visible: false
            },
            seriesColors: ["#0e5a7e", "#166f99", "#2185b4", "#319fd2", "#3eaee2"],
            seriesDefaults: {
                labels: {
                    visible: true,
                    background: "transparent",
                    color: "white",
                    format: "N0",
                    template: "#= value # (#if (isNaN(percentage))  { # 0.00 % # } # #if (!isNaN(percentage))  {# #=kendo.format('{0:P}',percentage)# #} #)"
                },
                dynamicSlope: false,
                dynamicHeight: false
            },
            series: [{
                type: "funnel",
                field: "TotalCount",
                categoryField: "DropdownValue",
                data: data
            }],
            seriesClick: function (e) {
                //   eraseCookie("TourByTypeDefaultFilter");
                if (selfDashBoard.IsSTAdmin() == "True") {
                    console.log(e.dataItem.DropdownValueID);
                    createCookie("OpportunityDefaultFilter", e.dataItem.DropdownValueID, 1);
                    window.location.href = selfDashBoard.Opportunitylink();
                }
                else if (selfDashBoard.hasReportPermission() == "True") {
                    console.log(e.dataItem.DropdownValueID);
                    createCookie("OpportunityDefaultFilter", e.dataItem.DropdownValueID, 1);
                    window.location.href = selfDashBoard.Opportunitylink();
                }  
            },

            tooltip: {
                visible: true,
                template: "#= category #"
            }
        });
        $('#potentialfunnel').kendoChart({
            title: {
                text: monthValue,
                position: "bottom"
            },
            legend: {
                visible: false
            },
            seriesColors: ["#0e5a7e", "#166f99", "#2185b4", "#319fd2", "#3eaee2"],
            seriesDefaults: {
                labels: {
                    visible: true,
                    background: "transparent",
                    color: "white",
                    format: "N0",
                    template: "#= dataItem.TotalCount # (#= dispalyPotential(dataItem.Potential) #)"
                },
                dynamicSlope: false,
                dynamicHeight: false
            },
            series: [{
                type: "funnel",
                field: "Potential",
                categoryField: "DropdownValue",
                data: data
            },
            {
                type: "funnel",
                field: "TotalCount",
                categoryField: "DropdownValue",
                data: data
            }],
            seriesClick: function (e) {
                //   eraseCookie("TourByTypeDefaultFilter");
                if (selfDashBoard.IsSTAdmin() == "True") {
                    console.log(e.dataItem.DropdownValueID);
                    createCookie("OpportunityDefaultFilter", e.dataItem.DropdownValueID, 1);
                    window.location.href = selfDashBoard.Opportunitylink();
                }
                else if (selfDashBoard.hasReportPermission() == "True") {
                    console.log(e.dataItem.DropdownValueID);
                    createCookie("OpportunityDefaultFilter", e.dataItem.DropdownValueID, 1);
                    window.location.href = selfDashBoard.Opportunitylink();
                }
            },

            tooltip: {
                visible: true,
                template: "#= category #"
            }
        });
    }


    var trafficByType = function (trafficByTypeResults, maxValue) {

        $("#datrafficatypes").kendoChart({
            legend: {
                visible: false
            },
            dataSource: {
                data: trafficByTypeResults
            },
            seriesDefaults: {
                type: "column"
            },
            theme: 'Metro',

            series: [{
                name: "Total Visits",
                field: "TotalVisits",              
                data: selfDashBoard.TourByTypeBarChartDetails()
            }, {
                name: "Unique visitors",
                field: "UniqueVisitors",            
                data: selfDashBoard.TourByTypeBarChartDetails()
            }],
             seriesClick: function(e) {
                 //   eraseCookie("TourByTypeDefaultFilter");
                 if (selfDashBoard.IsSTAdmin() == "True") {                   
                     console.log(e.dataItem.DropdownValueID);
                     createCookie("TourByTypeDefaultFilter", e.dataItem.DropdownValueID, 1);
                     window.location.href = selfDashBoard.ToursByTypeLink();
                 }
                 else if (selfDashBoard.hasReportPermission() == "True") {
                     console.log(e.dataItem.DropdownValueID);
                     createCookie("TourByTypeDefaultFilter", e.dataItem.DropdownValueID, 1);
                     window.location.href = selfDashBoard.ToursByTypeLink();
                 }
                   },
            valueAxis: {
                max: maxValue + 20,
                line: {
                    visible: false
                },
                minorGridLines: {
                    visible: false
                },
                majorGridLines: {
                    visible: false
                },
                visible: false
            },
            categoryAxis: {
                visible: false,
                field: "DropdownValue",
                majorGridLines: {
                    visible: false
                }
            },
            tooltip: {
                visible: true,
                template: "#= category # - #= series.name #: #= value #"
            }
        });
    }
    $("#view-tourtype").click(function () {
        $(".da-traffic-types").show();
        trafficByType(selfDashBoard.TourByTypeBarChartDetails(), TourByTypeMaxValue);
        $(".da-traffic").hide();
    });

    $("#view-tours").click(function () {
        $(".da-traffic-types").hide();
        $(".da-traffic").show();
    });

    $("#vwbytype").click(function () {
        if (selfDashBoard.MyCommunicationGridVisible() == 'True')
        {
            $("#vwbytype").text('View Grid');
            $(".my-comunication-chart").show();
            selfDashBoard.MyCommunicationGridVisible('False');
            selfDashBoard.MyCommunicationChartVisible('True');
            myCommunicationsPieChart(selfDashBoard.MyCommunication());
        }
        else if (selfDashBoard.MyCommunicationChartVisible() == 'True') {
            $("#vwbytype").text('View Chart');
            $(".my-comunication-chart").hide();
            selfDashBoard.MyCommunicationGridVisible('True');
            selfDashBoard.MyCommunicationChartVisible('False');
        }
    });

    function GetMyCommunicationData(period) {
        $.ajax({
            url: '/Dashboard/GetMyCommunicationData',
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'period': period })
        }).then(function (response) {
            var filter = $.Deferred();
            if (response.success) {
                filter.resolve(response);
            } else {
                filter.reject(response.error);
            }
            return filter.promise();
        }).done(function (data) {
            selfDashBoard.MyCommunication(data.response);
            if (selfDashBoard.MyCommunicationChartVisible() == 'True')
                myCommunicationsPieChart(selfDashBoard.MyCommunication());
        }).fail(function (error) {
            //selfDashBoard.MyCommunicationGridVisible('False');
            notifyError(error);
        });
    }

    $("#vwbyperiod").click(function () {
        if ($("#vwbyperiod").text() == "Last Week") {
            $("#vwbyperiod").text("Last Month")
            GetMyCommunicationData(0)
            selfDashBoard.Period('0')
        }
        else if ($("#vwbyperiod").text() == "Last Month") {
            $("#vwbyperiod").text("Last Week")
            GetMyCommunicationData(1)
            selfDashBoard.Period('1')
        }
    });

    $("#view-potential").click(function () {
        var text = $("#view-potential").text();
        if (text.includes("POTENTIAL"))
            $("#view-potential").text("VIEW PERCENTAGE IN EACH STAGE")
        else
            $("#view-potential").text("VIEW POTENTIAL IN EACH STAGE")
        $("#pipeline").toggle();
    });

    selfDashBoard.isDisplayEnabled = function (id) {
        var result = selfDashBoard.Settings()[selfDashBoard.Settings().map(function (e) { return e.Id }).indexOf(id)].Value();
        return result;
    };

    selfDashBoard.selectedReportsCount = function () {
        var result = selfDashBoard.Settings().map(function (e) { return e.Value() == true }).length;

        return result;
    };

    selfDashBoard.CancelAction = function () {
        var action = $.grep(selfDashBoard.Actions(), function (e) { return e.ActionId.toString() == selfDashBoard.MarkAsCompleteActionId().toString(); })[0];

        if (action != null) {
            var checkbox = $("#Action" + selfDashBoard.MarkAsCompleteActionId());
            var checkboxlabel = $("#" + selfDashBoard.MarkAsCompleteActionId());
            checkboxlabel.toggleClass("checked");
            checkbox.prop("checked", !checkbox.prop("checked"));
            var arrowicon = $("#Arrow" + selfDashBoard.MarkAsCompleteActionId());
            arrowicon.toggleClass('hide');
        }
    }

    $(window).resize(function () {
        newLeadsAreaChart(selfDashBoard.NewLeadsAreaChartDetails(), selfDashBoard.NewLeadsPreviousDateAreaChartDetails());
        newLeadsPieChart(selfDashBoard.NewLeadsPieChartDetails());
        trafficBySourcePieChart(selfDashBoard.TourBySourcePieChartDetails());
        trafficBySourceAreaChart(selfDashBoard.TourBySourceAreaChartdetails());
        trafficByType(selfDashBoard.TourByTypeBarChartDetails(), TourByTypeMaxValue);
        OppertunityChart(selfDashBoard.TourByTypeFunnelChartDetails(), monthvalue);

    });

    $(document).on('click', function (event) {
        if (event.target.id == 'myModal')
            selfDashBoard.CancelAction();
    });
};

var calenderViewModel = function () {

    var selfCalender = this;
    selfCalender.dataSource = ko.observableArray([]);
    selfCalender.currentView = ko.observable("Week");
    selfCalender.mainDataSource = ko.pureComputed(function () {
        return selfCalender.dataSource();
    });
    selfCalender.date = ko.observable(new Date().toUtzDate());

    selfCalender.dataSourceUrl = "/GetUserCalender?userId=0&startDate=" + Date.parse(new Date()) + "&endDate=" + Date.parse(selfCalender.date());
    selfCalender.Data = ko.observableArray([]);
    selfCalender.config = {
        date: selfCalender.date,
        height: 600,
        views: ["day", { type: "week", selected: true }, "month"],
        editable: {
            create: false,
            destroy: false,
            move: false,
            resize: false
        },
        autoBind: false,
        change: scheduler_change,
        navigate: scheduler_navigate,
        edit: scheduler_edit,
        dataSource: {
            //batch: true,
            transport: {
                read: function (options) {
                    if (selfDashBoard.Settings()[8].Value == true) {
                        $.ajax({
                            url: '/GetUserCalender',
                            type: 'get',
                            dataType: 'json',
                            data: { 'startDate': Date.parse(new Date()), 'endDate': Date.parse(selfCalender.date()) },
                            contentType: "application/json; charset=utf-8"
                        }).then(function (response) {
                            var filter = $.Deferred();
                            if (response.success) {
                                filter.resolve(response);
                            } else {
                                filter.reject(response.error);
                            }
                            return filter.promise();
                        }).done(function (data) {
                            selfCalender.Data(data.response);
                            for (var i = 0; i < data.response.length; i++) {
                                data.response[i].id = i;
                                data.response[i].start = ConvertToDate(data.response[i].start).toUtzDate();
                                data.response[i].end = ConvertToDate(data.response[i].end).toUtzDate();
                            }
                            options.success(data.response);
                            removepageloader();
                        }).fail(function (error) {
                            notifyError(error);
                        });
                    }
                }
            }
        }
    };

    //Kendo Provided function
    function scheduler_navigate(e) {
        selfCalender.date(e.date);
        kendoSchedular = $("#scheduler").data("kendoScheduler");
        //kendoSchedular.dataSource.transport.options.read.url = window.location.origin + "/GetUserCalender?userId=0&startDate=" + Date.parse(selfCalender.date())
        //    + "&endDate=" + Date.parse(selfCalender.date());
        kendoSchedular.dataSource.read();
        kendoSchedular.refresh();
        selfCalender.currentView("kendoSchedular.view().title");
    }

    //Kendo Provided function
    function scheduler_change(e) {
        //var start = e.start; //selection start date
        //var end = e.end; //selection end date
        //var slots = e.slots; //list of selected slots
        var events = e.events; //list of selected Scheduler events
        var message = "change:: selection from {0:g} till {1:g}";

        if (events.length) {
            message += ". The selected event is '" + events[events.length - 1].title + "'";
        }
    }





    function scheduler_edit(e) {
        e.event.start = kendo.toString(e.event.start, readCookie("dateformat") + " h:mm:ss tt");
        e.event.end = kendo.toString(e.event.end, readCookie("dateformat") + " h:mm:ss tt");
        $('input[name="start"]').val(e.event.start).attr("disabled", true);
        $('input[name="end"]').val(e.event.end).attr("disabled", true);

        $(".k-input.k-textbox").attr("disabled", true);
        $(".k-icon.k-i-calendar").attr("unselectable", "off");
        $(".k-picker-wrap .k-select").addClass("hide");
        $(".k-picker-wrap").attr("disabled", true);

        $("label[for='isAllDay']").parent().addClass("hide");
        $("div[data-container-for='isAllDay']").addClass("hide");

        $("label[for='recurrenceRule']").parent().addClass("hide");
        $("div[data-container-for='recurrenceRule']").addClass("hide");

        $("label[for='description']").parent().addClass("hide");
        $("div[data-container-for='description']").addClass("hide");

        $(".k-button.k-scheduler-cancel").text("Close");
        $(".k-button.k-primary.k-scheduler-update ").addClass("hide");

    }


};

