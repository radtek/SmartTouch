var QuickSearchViewModel = function (webserviceUrl) {
    var self = this;
    var ALL = 0, PEOPLE = 1, COMPANIES = 2, CAMPAIGNS = 4, TAGS = 3, REPORTS = 6, OPPORTUNITIES = 5, SAVEDSEARCHES = 7;
    self.SearchText = ko.observable();
    self.SearchableEntities = ko.observableArray([]);

    self.IncludeCampaigns = ko.observable(false);
    self.IncludePeople = ko.observable(false);
    self.IncludeCompanies = ko.observable(false);
    self.IncludeTags = ko.observable(false);
    self.IncludeReports = ko.observable(false);
    self.IncludeOpportunities = ko.observable(false);
    self.IncludeSavedSearches = ko.observable(false);
    self.IncludeAll = ko.observable(true);
    self.PageNumber = ko.observable(1);
    self.Limit = ko.observable(10);

    self.ToggleFilter = function (entityId) {
        self.SearchResults([]);
        self.PageNumber(1);

        if (entityId == ALL) {
            self.SearchableEntities([]);
            self.IncludeAll(true);
            self.IncludePeople(false);
            self.IncludeCompanies(false);
            self.IncludeTags(false);
            self.IncludeCampaigns(false);
            self.IncludeOpportunities(false);
            self.Search();
            return;
        }
        else {
            self.IncludeAll(false);
        }

        if (entityId == PEOPLE) {
            if (self.IncludePeople()) {
                self.IncludePeople(false);
                self.SearchableEntities.remove(PEOPLE);
            }
            else {
                self.IncludePeople(true);
                self.SearchableEntities.push(PEOPLE);
            }
        }
        else if (entityId == COMPANIES) {
            if (self.IncludeCompanies()) {
                self.IncludeCompanies(false);
                self.SearchableEntities.remove(COMPANIES);
            }
            else {
                self.IncludeCompanies(true);
                self.SearchableEntities.push(COMPANIES);
            }
        }
        else if (entityId == TAGS) {
            if (self.IncludeTags()) {
                self.IncludeTags(false);
                self.SearchableEntities.remove(TAGS);
            }
            else {
                self.IncludeTags(true);
                self.SearchableEntities.push(TAGS);
            }
        }
        else if (entityId == CAMPAIGNS) {
            if (self.IncludeCampaigns()) {
                self.IncludeCampaigns(false);
                self.SearchableEntities.remove(CAMPAIGNS);
            }
            else {
                self.IncludeCampaigns(true);
                self.SearchableEntities.push(CAMPAIGNS);
            }
        }
        else if (entityId == OPPORTUNITIES) {
            if (self.IncludeOpportunities()) {
                self.IncludeOpportunities(false);
                self.SearchableEntities.remove(OPPORTUNITIES);
            }
            else {
                self.IncludeOpportunities(true);
                self.SearchableEntities.push(OPPORTUNITIES);
            }
        }
        self.Search();
    };

    self.SearchResults = ko.observableArray();
    self.TotalHits = ko.observable(0);
    self.SearchedText = ko.observable('');

    

    self.MoreResults = function () {
        var pageNumber = self.PageNumber() + 1;
        self.PageNumber(pageNumber);
        self.Search();
        var $t = $('.unified-result');
        $t.animate({ "scrollTop": $('.unified-result')[0].scrollHeight }, "slow");
    }

    self.ShowMoreResults = ko.pureComputed(function () {
        var resultsDisplayed = self.SearchResults().length;
        if (resultsDisplayed < self.TotalHits())
            return true;
        else
            return false;
    });

    var typewatch = (function () {
        var timer = 0;
        return function (callback, ms) {
            clearTimeout(timer);
            timer = setTimeout(callback, ms);
        };
    })();

    $(document).on('keyup', "#quickSearchInput", function () {        
        typewatch(function () {
            if ($("#quickSearchInput").val() == '') {
                self.SearchResults([]);
                self.TotalHits(0);
                return;
            }
            
            if ($("#quickSearchInput").val() === self.SearchedText()) {                
                return;
            }
            self.SearchResults([]);
            self.PageNumber(1);
            self.SearchText($("#quickSearchInput").val());
            self.Search();
            self.TotalHits(0);
        }, 800);
    });
};