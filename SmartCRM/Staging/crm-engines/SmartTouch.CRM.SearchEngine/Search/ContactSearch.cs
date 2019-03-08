using Antlr.Runtime.Tree;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.Logging;
using Nest;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SmartTouch.CRM.SearchEngine.Search
{
    internal class ContactSearch<T> : SearchBase<T> where T : class
    {
        public IDictionary<ContactFields, string> searchFields = new Dictionary<ContactFields, string>();

        public ContactSearch(int accountId)
            : base(accountId)
        {
            searchFields = getSearchFields();
        }

        public SearchResult<T> SearchContacts(string q, SearchParameters searchParameters)
        {
            return search<int>(q, null, searchParameters);
        }

        public SearchResult<T> SearchContacts(string q, Expression<Func<T, bool>> filter, SearchParameters searchParameters)
        {
            return search<int>(q, filter, searchParameters);
        }

        public SearchResult<T> SearchCampaigns(string q, SearchParameters searchParameters)
        {
            return searchCampaigns<int>(q, searchParameters);
        }

        //public SearchResult<T> SearchContacts(string q, int accountID)
        //{
        //    return search<int>(q, accountID);
        //}

        public override SearchResult<Suggestion> AutoCompleteField(string q, SearchParameters searchParameters)
        {
            return base.AutoCompleteField(q, searchParameters);
        }

        public SearchResult<T> DuplicateSearch(T t, SearchParameters searchParameters)
        {
            if (t.GetType().Equals(typeof(Person)))
                return isDuplicatePerson(t as Person, searchParameters);
            else if (t.GetType().Equals(typeof(Company)))
                return isDuplicateCompany(t as Company, searchParameters);

            throw new ArgumentException("Invalid argument encountered");
        }

        public async Task<SearchResult<T>> AdvancedSearchAsync(string q, SearchDefinition searchDefinition, SearchParameters searchParameters)
        {
            SearchDescriptor<T> descriptor = getDescriptor(q, searchDefinition, searchParameters);

            SearchResult<T> searchResult = new SearchResult<T>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = this.ElasticClient().Search<T>(body => descriptor);
            stopwatch.Stop();
            var elapsed_time = stopwatch.Elapsed;
            Logger.Current.Informational("Time Taken to complete Query :" + elapsed_time);
            string que = result.ConnectionStatus.ToString();
            Logger.Current.Verbose(que);

            if (searchParameters.Fields != null && searchParameters.Fields.Count() == 2 && searchParameters.Fields.Contains(ContactFields.ContactId) && searchParameters.Fields.Contains(ContactFields.IsActive))
            {
                int id = 0;
                bool isActive = false;
                IList<Contact> results = new List<Contact>();
                foreach (var hit in result.Hits)
                {
                    if (hit.Type.Equals(typeof(Company)) || hit.Type == "companies")
                    {
                        int.TryParse(hit.Id, out id);
                        results.Add(new Company() { Id = id });  //new Company() { Id = id }
                    }
                    else
                    {
                        try
                        {
                            int.TryParse(hit.Id, out id);
                            string value = hit.Fields.FieldValuesDictionary["isActive"].ToString().Replace("]", "").Replace("[", "").Replace("\r\n", "");
                            bool.TryParse(value, out isActive);
                            results.Add(new Person() { Id = id, IsActive = isActive });
                        }
                        catch (Exception ex)
                        {
                            ex.Data.Add("ContactID ", id);
                            ex.Data.Add("Type ", hit.Type);
                            Logger.Current.Error("An error occured while fetching contactids from elastic", ex);
                        }
                    }
                }
                searchResult.Results = results.Select(c => c as T);
            }
            //else if (result.Documents.IsAny() && result.Hits.IsAny() && result.HitsMetaData != null)
            //{
            //    Stopwatch hitsTimer = new Stopwatch();
            //    hitsTimer.Start();
            //    List<Contact> contacts = new List<Contact>();
            //    foreach (var hit in result.Hits)
            //    {
            //        IEnumerable<Contact> results = result.Documents.Select(c => c as T) as IEnumerable<Contact>;
            //        Contact con = results.Where(w => w.Id.ToString() == hit.Id).FirstOrDefault();
            //        var fields = hit.InnerHits.Select(s => s.Value.Hits.Hits.FirstOrDefault().Source.As<ContactCustomField>());
            //        if (fields != null && fields.Any())
            //        {
            //            List<ContactCustomField> conCustomField = new List<ContactCustomField>();
            //            foreach (var field in fields)
            //                conCustomField.Add(field as ContactCustomField);
            //            con.CustomFields = conCustomField;
            //        }
            //        contacts.Add(con);
            //    }
            //    searchResult.Results = contacts.Select(c => c as T);
            //    hitsTimer.Stop();
            //    var timeElapsed = hitsTimer.Elapsed;
            //    Logger.Current.Informational("Time taken to complete serialization : " + timeElapsed);
            //}
            else
            {
                Stopwatch hitsTimer = new Stopwatch();
                hitsTimer.Start();
                searchResult.Results = result.Documents.Select(c => c as T);
                hitsTimer.Stop();
                var timeElapsed = hitsTimer.Elapsed;
                Logger.Current.Informational("Time taken to complete serialization : " + timeElapsed);
            }

            searchResult.TotalHits = result.Total;

            return await Task.Run(() => searchResult);
        }

        public async Task<SearchResult<T>> AdvancedSearchExportAsync(string q, SearchDefinition searchDefinition, SearchParameters searchParameters)
        {
            SearchDescriptor<T> descriptor = getDescriptor(q, searchDefinition, searchParameters);

            SearchResult<T> searchResult = new SearchResult<T>();

            //var searchFields = new List<string>();
            //foreach (var field in searchParameters.Fields)
            //{
            //    searchFields.Add(field.ToString());
            //}

            var asContacts = new List<Person>();
            var scanResults = await this.ElasticClient().SearchAsync<T>(body => descriptor
                .SearchType(Elasticsearch.Net.SearchType.Scan)
                .Size(1000)
                .Scroll("6s")
                );
            string asque = scanResults.ConnectionStatus.ToString();
            Logger.Current.Verbose(asque);
            var scrolls = 0;
            var asResults = await this.ElasticClient().ScrollAsync<Person>("4s", scanResults.ScrollId);
            string asFirst = asResults.ConnectionStatus.ToString();
            Logger.Current.Verbose(asFirst);
            asContacts.AddRange(asResults.Documents);
            while (asResults.Documents.Any())
            {
                asResults = await this.ElasticClient().ScrollAsync<Person>("4s", asResults.ScrollId);
                asContacts.AddRange(asResults.Documents);
                scrolls++;
            }

            searchResult.Results = asContacts.Select(c => c as T);
            searchResult.TotalHits = asResults.Total;
            return searchResult;
        }

        private SearchDescriptor<T> getDescriptor(string q, SearchDefinition searchDefinition, SearchParameters searchParameters)
        {
            int limit = searchParameters.Limit == 0 ? 10 : searchParameters.Limit;
            int from = (searchParameters.PageNumber - 1) * limit;

            SearchDescriptor<T> descriptor = new SearchDescriptor<T>();
            descriptor = descriptor.Types(searchParameters.Types)
                            .From(from)
                            .Size(limit);

            if (searchParameters.SortField.HasValue)
            {
                if (searchParameters.SortField == Entities.ContactSortFieldType.RecentlyUpdatedContact)
                    descriptor = descriptor.Sort(s =>
                        s.OnField(searchFields[ContactFields.LastUpdateOn]).Descending().UnmappedType(Nest.FieldType.None));
                else if (searchParameters.SortField == Entities.ContactSortFieldType.FullName)
                    descriptor = descriptor.Sort(s => s.OnField(searchFields[ContactFields.FirstName_NotAnalyzed]).Ascending().UnmappedType(Nest.FieldType.None))
                .Sort(s => s.OnField(searchFields[ContactFields.LastName_NotAnalyzed]).Ascending().UnmappedType(Nest.FieldType.None));
                else if (searchParameters.SortField == Entities.ContactSortFieldType.CompanyName)
                    descriptor = descriptor.Sort(s => s.OnField(searchFields[ContactFields.CompanyName_NotAnalyzed]).Ascending().UnmappedType(Nest.FieldType.None));
                else if (searchParameters.SortField == Entities.ContactSortFieldType.LeadScore)
                    descriptor = descriptor.Sort(s => s.OnField(searchFields[ContactFields.LeadScore]).Descending().UnmappedType(Nest.FieldType.None));
                else if (searchParameters.SortField == Entities.ContactSortFieldType.FullNameOrCompanyName)
                    descriptor = descriptor.Sort(s => s.OnField(searchFields[ContactFields.CompanyName_NotAnalyzed]).Ascending().UnmappedType(Nest.FieldType.None))
                        .Sort(s => s.OnField(searchFields[ContactFields.FirstName_NotAnalyzed]).Ascending().UnmappedType(Nest.FieldType.None))
                        .Sort(s => s.OnField(searchFields[ContactFields.LastName_NotAnalyzed]).Ascending().UnmappedType(Nest.FieldType.None));
            }
            else if (searchParameters.IsResultsGrid && searchParameters.ResultsGridSortField != null)
            {
                if (searchParameters.ResultsGridSortField == "LastTouchedThrough")  //Property name in Viewmodel(ContactListEntry) and Elastic are mis-matching
                    searchParameters.ResultsGridSortField = "LastContactedThrough";
                string sortField = searchParameters.ResultsGridSortField;
                if (searchParameters.SortDirection == System.ComponentModel.ListSortDirection.Ascending)
                    descriptor = descriptor.Sort(s => s.OnField(sortField.ToCamelCase()).Ascending().UnmappedType(Nest.FieldType.None));
                else
                    descriptor = descriptor.Sort(s => s.OnField(sortField.ToCamelCase()).Descending().UnmappedType(Nest.FieldType.None));
            }
            else
                descriptor = descriptor.Sort(s => s.OnField(searchFields[ContactFields.LastUpdateOn]).Descending().UnmappedType(Nest.FieldType.None));


            var baseFilters = new List<FilterContainer>();

            setSearchFilters(searchDefinition);

            if (searchDefinition.PredicateType == Entities.SearchPredicateType.And)
            {
                if (searchDefinition.IsAggregationNeeded)
                    baseFilters.AddRange(this.GetFilterAggregator(searchDefinition));
                else
                    foreach (SearchFilter filter in searchFilters)
                        baseFilters.Add(getBaseFilter(filter));

                if (searchParameters.IsPrivateSearch || searchParameters.IsActiveContactsSearch)
                {
                    FilterContainer privateFilter = this.getActiveContactsFilters(filterDesc, searchParameters.IsPrivateSearch, searchParameters.IsActiveContactsSearch,
                        searchParameters.DocumentOwnerId.HasValue ? searchParameters.DocumentOwnerId.Value : 0);
                    FilterContainer andFilter = filterDesc.And(baseFilters.ToArray());
                    descriptor = descriptor.Filter(f => f.And(privateFilter, andFilter));
                }
                else
                    descriptor = descriptor.Filter(f => f.And(baseFilters.ToArray()));
            }
            else if (searchDefinition.PredicateType == Entities.SearchPredicateType.Or)
            {
                foreach (SearchFilter filter in searchFilters)
                    baseFilters.Add(getBaseFilter(filter));

                if (searchParameters.IsPrivateSearch || searchParameters.IsActiveContactsSearch)
                {
                    FilterContainer privateFilter = this.getActiveContactsFilters(filterDesc, searchParameters.IsPrivateSearch, searchParameters.IsActiveContactsSearch,
                        searchParameters.DocumentOwnerId.HasValue ? searchParameters.DocumentOwnerId.Value : 0);
                    FilterContainer orFilter = filterDesc.Or(baseFilters.ToArray());
                    descriptor = descriptor.Filter(f => f.Or(privateFilter, orFilter));
                }
                else
                    descriptor = descriptor.Filter(f => f.Or(baseFilters.ToArray()));
            }
            else
            {
                traverseTree(searchDefinition.CustomLogicalTree);
                FilterContainer filter = stack.Pop();

                if (searchParameters.IsPrivateSearch)
                {
                    FilterContainer privateFilter = filterDesc.Term("ownerId", searchParameters.DocumentOwnerId);
                    descriptor = descriptor.Filter(f => f.And(privateFilter, filter));
                }
                if (searchParameters.IsPrivateSearch || searchParameters.IsActiveContactsSearch)
                {
                    FilterContainer privateFilter = this.getActiveContactsFilters(filterDesc, searchParameters.IsPrivateSearch, searchParameters.IsActiveContactsSearch,
                        searchParameters.DocumentOwnerId.HasValue ? searchParameters.DocumentOwnerId.Value : 0);
                    descriptor = descriptor.Filter(f => f.And(privateFilter, filter));
                }
                else
                    descriptor = descriptor.Filter(filter);
            }

            if (!string.IsNullOrEmpty(q))
            {
                List<ContactFields> nonSearchableFields = getNonSearchableFields();

                IEnumerable<string> searchableFields = searchFields
                        .Where(f => !nonSearchableFields.Contains(f.Key))
                        .Select(f => f.Value).ToList();

                descriptor = descriptor.Query(query => query.FuzzyLikeThis(flt => flt
                .LikeText(q)
                .MinimumSimilarity(0.5)
                .IgnoreTermFrequency(true)
                .OnFields(searchableFields)
                .Boost(1.0)));
            }

            if (searchParameters.Types == null && !searchParameters.IsActiveContactsSearch)
            {
                var types = new List<Type>() { typeof(Company), typeof(Person), typeof(Contact) };
                descriptor = descriptor.Types(types.ToArray());
            }
            else if (searchParameters.Types == null && searchParameters.IsActiveContactsSearch)
            {
                var types = new List<Type>() { typeof(Person), typeof(Contact) };
                descriptor = descriptor.Types(types.ToArray());
            }

            if (searchParameters != null && searchParameters.Fields.IsAny())
            {
                var fields = searchFields.Where(s => searchParameters.Fields.Contains(s.Key)).Select(s => s.Value).ToArray();
                if (searchParameters.Fields.Count() == 2 && searchParameters.Fields.Contains(ContactFields.ContactId) && searchParameters.Fields.Contains(ContactFields.IsActive))
                    descriptor = descriptor.Fields(fields);
                else
                    descriptor = descriptor.Source(s => s.Include(fields));       //.Include("customFields"));
            }
            return descriptor;
        }

        public bool IsContactOwnedBy(int contactId, int? userId)
        {
            SearchDescriptor<T> descriptor = new SearchDescriptor<T>();
            IList<Type> types = new List<Type>() { typeof(Contact), typeof(Person), typeof(Company) };

            string[] selectedIndices = indices.Where(i => types.Contains(i.Key)).Select(i => i.Value).Distinct().ToArray();
            descriptor = descriptor.Types(types).Indices(selectedIndices);
            descriptor = descriptor.Filter(f => f.And(a => a.Term("id", contactId), a => a.Term("ownerId", userId)));

            var result = this.ElasticClient().Search<T>(body => descriptor);

            return result.Total > 0;
        }

        IEnumerable<SearchFilter> searchFilters = new List<SearchFilter>();
        FilterDescriptor<Contact> filterDesc = new FilterDescriptor<Contact>();
        Stack<FilterContainer> stack = new Stack<FilterContainer>();

        //post order traversal
        private void traverseTree(CommonTree current)
        {
            if (current == null)
                return;

            //left node
            if (current.Children != null)
                traverseTree(current.Children[0] as CommonTree);

            //right node
            if (current.ChildCount > 1)
                traverseTree(current.Children[1] as CommonTree);


            if (Regex.IsMatch(current.Text, @"^\d+$"))
            {
                SearchFilter filter = getSearchFilter(current.Text);
                FilterContainer query = getBaseFilter(filter);
                stack.Push(query);
            }
            else if (current.Text.ToLower().Equals("and"))
            {
                IList<FilterContainer> filters = new List<FilterContainer>();
                foreach (var i in Enumerable.Range(1, current.ChildCount))
                {
                    filters.Add(stack.Pop());
                }
                FilterContainer andFilter = filterDesc.And(filters.ToArray());
                stack.Push(andFilter);
            }
            else if (current.Text.ToLower().Equals("or"))
            {
                IList<FilterContainer> filters = new List<FilterContainer>();
                foreach (var i in Enumerable.Range(1, current.ChildCount))
                {
                    filters.Add(stack.Pop());
                }
                FilterContainer orFilter = filterDesc.Or(filters.ToArray());

                stack.Push(orFilter);
            }
        }

        private FilterContainer getBaseFilter(SearchFilter filter)
        {
            FilterContainer query = new FilterContainer();

            try
            {
                if (filter.Qualifier == SearchQualifier.Is)
                    query = this.isQuery(filter);

                else if (filter.Qualifier == SearchQualifier.IsNot)
                    query = this.isNotQuery(filter);

                else if (filter.Qualifier == SearchQualifier.IsEmpty)
                    query = this.isEmptyQuery(filter);

                else if (filter.Qualifier == SearchQualifier.IsNotEmpty)
                    query = this.isNotEmptyQuery(filter);

                else if (filter.Qualifier == SearchQualifier.Contains)
                    query = this.containsQuery(filter);

                else if (filter.Qualifier == SearchQualifier.DoesNotContain)
                    query = this.doesNotContainsQuery(filter);

                else if (filter.Qualifier == SearchQualifier.IsLessThan)
                    query = this.isLessThanQuery(filter);

                else if (filter.Qualifier == SearchQualifier.IsLessThanEqualTo)
                    query = this.isLessThanOrEqualToQuery(filter);

                else if (filter.Qualifier == SearchQualifier.IsGreaterThan)
                    query = this.isGreaterThanQuery(filter);

                else if (filter.Qualifier == SearchQualifier.IsGreaterThanEqualTo)
                    query = this.isGreaterThanOrEqualToQuery(filter);

                return query;
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
                ex.Data.Add("Filter", filter.ToString());
                Logger.Current.Error("An error occured while generating search query : ", ex);
                throw ex;   //If we return empty query it will return all contacts based on the account.
            }
        }

        private int isTourOrActionField(ContactFields field)
        {
            List<int> fields = new List<int>() 
            {
                (int)ContactFields.TourCreator,
                (int)ContactFields.TourDate,
                (int)ContactFields.TourType,
                (int)ContactFields.TourAssignedUsers,
                (int)ContactFields.Community,
                (int)ContactFields.ActionType,
                (int)ContactFields.ActionStatus,
                (int)ContactFields.ActionDate,
                (int)ContactFields.ActionCreatedDate,
                (int)ContactFields.ActionAssignedTo,
                (int)ContactFields.NoteCategory
            };

            return fields.FindIndex(f => f.Equals((int)field));
        }


        private FilterContainer isQuery(SearchFilter filter)
        {
            FilterContainer query = new FilterContainer();
            int tourOrActionField = -1;

            if (filter.IsDropdownField && filter.DropdownId.HasValue && filter.DropdownId.Value == (byte)DropdownFieldTypes.PhoneNumberType)
            {
                int phoneType = default(int);
                if (filter.Field == ContactFields.DropdownField)  // edit/run 
                    phoneType = filter.DropdownValueId.Value;
                else                                             //  add 
                    phoneType = (int)filter.Field;
                query = filterDesc.And(filterDesc.Term("phones.number", filter.SearchText),
                    filterDesc.Term("phones.phoneType", phoneType));
            }
            else if (filter.Field == ContactFields.WebPage || filter.Field == ContactFields.WebPageDuration)
            {
                //var value = filter.Field == ContactFields.WebPage ? "webVisits.pageVisited" : "webVisits.duration";
                query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("webVisits").Filter(f => f.Term(searchFields[filter.Field], filter.SearchText))))));

            }
            else if (filter.IsCustomField)
            {
                string customFieldValue = (filter.FieldOptionTypeId == (byte)Entities.FieldType.date || filter.FieldOptionTypeId == (byte)Entities.FieldType.datetime || filter.FieldOptionTypeId == (byte)Entities.FieldType.time)
                    ? "customFields.value_Date" : (filter.FieldOptionTypeId == (byte)Entities.FieldType.multiselectdropdown || filter.FieldOptionTypeId == (byte)Entities.FieldType.checkbox)
                    ? "customFields.value_Multiselect" : (filter.FieldOptionTypeId == (byte)Entities.FieldType.number) ? "customFields.value_Number" : "customFields.value";

                if (filter.FieldOptionTypeId.HasValue && (filter.FieldOptionTypeId == (byte)Entities.FieldType.multiselectdropdown || filter.FieldOptionTypeId == (byte)Entities.FieldType.checkbox))
                {
                    string[] sources = filter.SearchText.Split('|');

                    query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("customFields").Filter(f => f.Bool(b => b.Must(m => m.Term("customFields.customFieldId", (int)filter.Field),
                        mu => mu.Terms(customFieldValue, sources, TermsExecution.And),
                        mus => mus.Term("customFields.value_Multiselect_Count", sources.Count()))))))));
                }
                else if (filter.FieldOptionTypeId == (byte)Entities.FieldType.date || filter.FieldOptionTypeId == (byte)Entities.FieldType.datetime)
                {
                    DateTime inputDate = new DateTime();
                    DateTime.TryParse(filter.SearchText, out inputDate);
                    DateTime lowerDate = DateMath(inputDate);
                    DateTime upperDate = lowerDate.AddDays(1);
                    Logger.Current.Informational("InputText : " + filter.SearchText + ", inputDate : " + inputDate.ToString("yyyy-MM-dd'T'HH:mm:ss") + ", lowerDate : " + lowerDate.ToString("yyyy-MM-dd'T'HH:mm:ss") +
                        ", upperDate : " + upperDate.ToString("yyyy-MM-dd'T'HH:mm:ss"));

                    //var time = date.TimeOfDay;
                    //var newDate = new DateTime(time.Ticks);
                    //DateTime upperDate = date.ToUserUtcDateTimeV3().AddDays(1);
                    //string dateString = filter.FieldOptionTypeId == (byte)Entities.FieldType.time ? newDate.ToString("yyyy-MM-dd'T'HH:mm:ss") : date.ToUserUtcDateTimeV3().ToString("yyyy-MM-dd'T'HH:mm:ss");
                    query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("customFields").Filter(f => f.Bool(b => b.Must(m => m.Term("customFields.customFieldId", (int)filter.Field),
                        mus => mus.Range(r => r.GreaterOrEquals(lowerDate.ToString("yyyy-MM-dd'T'HH:mm:ss")).Lower(upperDate.ToString("yyyy-MM-dd'T'HH:mm:ss")).OnField(customFieldValue)))))))));
                }
                else if (filter.FieldOptionTypeId == (byte)Entities.FieldType.time)
                {
                    DateTime date = new DateTime();
                    DateTime.TryParse(filter.SearchText, out date);
                    date = DateMath(date);
                    var specifiedDate = date.Date;
                    var dateTicks = specifiedDate.Ticks;
                    var dateTimeTicks = date.Ticks;
                    var aggTicks = dateTimeTicks - dateTicks;
                    date = new DateTime(aggTicks);
                    query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("customFields").Filter(f => f.Bool(b => b.Must(m => m.Term("customFields.customFieldId", (int)filter.Field),
                        mus => mus.Term(customFieldValue, date))))))));
                }
                else
                    query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("customFields").Filter(f => f.Bool(b => b.Must(m => m.Term("customFields.customFieldId", (int)filter.Field),
                            mus => mus.Term(customFieldValue, filter.SearchText))))))));
            }
            else if (filter.Field == ContactFields.LastTouched || filter.Field == ContactFields.CreatedOn || filter.FieldOptionTypeId == (byte)Entities.FieldType.date || filter.Field == ContactFields.LeadSourceDate ||
                filter.Field == ContactFields.FirstLeadSourceDate || filter.Field == ContactFields.LastNoteDate)
            {
                DateTime date;
                DateTime.TryParse(filter.SearchText, out date);
                date = DateMath(date);
                DateTime upperDate = date.AddDays(1);
                if (filter.Field == ContactFields.FirstLeadSourceDate || filter.Field == ContactFields.LeadSourceDate)
                    query = filterDesc.Query(q => q.Filtered(f => f.Filter(fi => fi.Nested(ne => ne.Path("leadSources").Filter(fil =>
                        fil.Bool(b => b.Must(m =>
                        m.Range(n => n.GreaterOrEquals(date).Lower(upperDate).OnField(searchFields[filter.Field])),
                        mrt => mrt.Term("leadSources.isPrimary", filter.Field == ContactFields.FirstLeadSourceDate ? true : false))))))));
                else
                    query = filterDesc.Range(n => n.GreaterOrEquals(date).Lower(upperDate).OnField(searchFields[filter.Field]));
            }
            else if (filter.Field == ContactFields.LeadSource || filter.Field == ContactFields.FirstLeadSource || filter.Field == ContactFields.AllLeadSources)
            {
                string[] sources = filter.SearchText.Split('|');
                if (filter.Field == ContactFields.AllLeadSources)
                    query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("leadSources").Filter(nf => nf.Bool(b => b.Must(m => m.Terms("leadSources.id", sources))))))));
                else
                    query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("leadSources").Filter(nf => nf.Bool(b => b.Must(m => m.Terms(searchFields[filter.Field], sources),
                        mu => mu.Term("leadSources.isPrimary", filter.Field == ContactFields.FirstLeadSource ? true : false))))))));
            }
            else if ((tourOrActionField = isTourOrActionField(filter.Field)) > -1)
            {
                DateTime date;
                DateTime.TryParse(filter.SearchText, out date);
                date = DateMath(date);
                DateTime upperDate = date.AddDays(1);
                string pathName = tourOrActionField > 4 ? tourOrActionField > 9 ? "contactNotes" : "contactActions" : "tourCommunity";

                if (filter.Field == ContactFields.TourDate || filter.Field == ContactFields.ActionCreatedDate || filter.Field == ContactFields.ActionDate)
                    query = filterDesc.Query(q => q.Filtered(f => f.Filter(fi => fi.Nested(ne => ne.Path(pathName).Filter(fil =>
                        fil.Bool(b => b.Must(m =>
                            m.Range(n => n.GreaterOrEquals(date).Lower(upperDate).OnField(searchFields[filter.Field])))))))));
                else
                    query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path(pathName).Filter(nf => nf.Bool(b => b.Must(m => m.Term(searchFields[filter.Field], filter.SearchText))))))));
            }
            else
                query = filterDesc.Term(searchFields[filter.Field], filter.SearchText);

            return query;
        }

        private FilterContainer isNotQuery(SearchFilter filter)
        {
            FilterContainer query = new FilterContainer();
            int tourOrActionField = -1;

            if (filter.IsDropdownField && filter.DropdownId.HasValue && filter.DropdownId.Value == (byte)DropdownFieldTypes.PhoneNumberType)
            {
                int phoneType = default(int);
                if (filter.Field == ContactFields.DropdownField)  // edit/run 
                    phoneType = filter.DropdownValueId.Value;
                else                                             //  add 
                    phoneType = (int)filter.Field;
                query = filterDesc.Not(n => n.And(filterDesc.Term("phones.number", filter.SearchText),
                    filterDesc.Term("phones.phoneType", phoneType)));
            }
            else if (filter.Field == ContactFields.WebPage || filter.Field == ContactFields.WebPageDuration)
            {
                var value = filter.Field == ContactFields.WebPage ? "webVisits.pageVisited" : "webVisits.duration";
                query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("webVisits").Filter(f => f.Not(no => no.Term(value, filter.SearchText)))))));

            }
            else if (filter.IsCustomField)
            {
                string customFieldValue = (filter.FieldOptionTypeId == (byte)Entities.FieldType.date || filter.FieldOptionTypeId == (byte)Entities.FieldType.datetime || filter.FieldOptionTypeId == (byte)Entities.FieldType.time)
                    ? "customFields.value_Date" : (filter.FieldOptionTypeId == (byte)Entities.FieldType.multiselectdropdown || filter.FieldOptionTypeId == (byte)Entities.FieldType.checkbox)
                    ? "customFields.value_Multiselect" : (filter.FieldOptionTypeId == (byte)Entities.FieldType.number) ? "customFields.value_Number" : "customFields.value";

                if (filter.FieldOptionTypeId.HasValue && (filter.FieldOptionTypeId == (byte)Entities.FieldType.multiselectdropdown || filter.FieldOptionTypeId == (byte)Entities.FieldType.checkbox))
                {
                    string[] sources = filter.SearchText.Split('|');
                    query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("customFields").Filter(f => f.Bool(b => b.Must(m => m.Term("customFields.customFieldId", (int)filter.Field))
                        .MustNot(mn => mn.And(mn.Terms(customFieldValue, sources, TermsExecution.And), mn.Term("customFields.value_Multiselect_Count", sources.Count())))))))));
                }
                else if (filter.FieldOptionTypeId == (byte)Entities.FieldType.date || filter.FieldOptionTypeId == (byte)Entities.FieldType.datetime)
                {
                    DateTime date = new DateTime();
                    DateTime.TryParse(filter.SearchText, out date);
                    DateTime lowerDate = DateMath(date);
                    DateTime upperDate = lowerDate.AddDays(1);
                    query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("customFields").Filter(f => f.Bool(b => b.Must(m => m.Term("customFields.customFieldId", (int)filter.Field),
                        mu => mu.Or(o => o.Range(r => r.Greater(upperDate.ToString("yyyy-MM-dd'T'HH:mm:ss")).OnField(customFieldValue)),
                                    or => or.Range(ra => ra.Lower(lowerDate.ToString("yyyy-MM-dd'T'HH:mm:ss")).OnField(customFieldValue))))))))));
                }
                else if (filter.FieldOptionTypeId == (byte)Entities.FieldType.time)
                {
                    DateTime date = new DateTime();
                    DateTime.TryParse(filter.SearchText, out date);
                    date = DateMath(date);
                    var specifiedDate = date.Date;
                    var dateTicks = specifiedDate.Ticks;
                    var dateTimeTicks = date.Ticks;
                    var aggTicks = dateTimeTicks - dateTicks;
                    date = new DateTime(aggTicks);
                    query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("customFields").Filter(f => f.Bool(b => b.Must(m => m.Term("customFields.customFieldId", (int)filter.Field),
                        mu => mu.Not(no => no.Term(customFieldValue, date)))))))));
                }
                else
                    query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("customFields").Filter(f => f.Bool(b => b.Must(m => m.Term("customFields.customFieldId", (int)filter.Field))
                            .MustNot(mn => mn.Term(customFieldValue, filter.SearchText))))))));
            }
            else if (filter.Field == ContactFields.LastTouched || filter.Field == ContactFields.CreatedOn || filter.FieldOptionTypeId == (byte)Entities.FieldType.date || filter.Field == ContactFields.LeadSourceDate ||
                filter.Field == ContactFields.FirstLeadSourceDate || filter.Field == ContactFields.LastNoteDate)
            {
                DateTime date;
                DateTime.TryParse(filter.SearchText, out date);
                DateTime lowerDate = DateMath(date);
                DateTime upperDate = date.AddDays(1);
                if (filter.Field == ContactFields.FirstLeadSourceDate || filter.Field == ContactFields.LeadSourceDate)
                    query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(ne => ne.Path("leadSources").Filter(nf =>
                        nf.Bool(b => b.Must(m => m.Or(filterDesc.Range(n => n.Greater(upperDate).OnField(searchFields[filter.Field])),
                                                            filterDesc.Range(n => n.Lower(lowerDate).OnField(searchFields[filter.Field]))),
                                                            mrt => mrt.Term("leadSources.isPrimary", filter.Field == ContactFields.FirstLeadSourceDate ? true : false))))))));
                else
                    query = filterDesc.Or(filterDesc.Range(n => n.Greater(upperDate).OnField(searchFields[filter.Field])),
                    filterDesc.Range(n => n.Lower(lowerDate).OnField(searchFields[filter.Field])));
            }
            else if (filter.Field == ContactFields.LeadSource || filter.Field == ContactFields.FirstLeadSource)
            {
                string[] sources = filter.SearchText.Split('|');
                query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("leadSources").Filter(nf => nf.Bool(b =>
                    b.Must(mu => mu.Term("leadSources.isPrimary", filter.Field == ContactFields.FirstLeadSource ? true : false))
                    .MustNot(mn => mn.Terms(searchFields[filter.Field], sources))))))));
            }
            else if (filter.Field == ContactFields.LeadScore)
                query = filterDesc.And(f => f.Not(n => n.Term(searchFields[filter.Field], filter.SearchText)), f => f.Exists(searchFields[filter.Field]));
            else if (filter.Field == ContactFields.LeadAdapter)
                query = filterDesc.And(f => f.Not(n => n.Term(searchFields[filter.Field], filter.SearchText)), f => f.Exists(searchFields[filter.Field]),
                    f => f.Term("firstContactSource", (byte)ContactSource.LeadAdapter));
            else if ((tourOrActionField = isTourOrActionField(filter.Field)) > -1)
            {
                DateTime date;
                DateTime.TryParse(filter.SearchText, out date);
                date = DateMath(date);
                DateTime upperDate = date.AddDays(1);
                string pathName = tourOrActionField > 4 ? tourOrActionField > 9 ? "contactNotes" : "contactActions" : "tourCommunity";

                if (filter.Field == ContactFields.TourDate || filter.Field == ContactFields.ActionCreatedDate || filter.Field == ContactFields.ActionDate)
                    query = filterDesc.Query(q => q.Filtered(f => f.Filter(fi => fi.Nested(ne => ne.Path(pathName).Filter(fil =>
                        fil.Bool(b => b.Must(m =>
                            m.Range(n => n.Greater(upperDate).Lower(date).OnField(searchFields[filter.Field])))))))));
                else
                    query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path(pathName).Filter(nf => nf.Bool(b => b.MustNot(m => m.Term(searchFields[filter.Field], filter.SearchText))))))));
            }
            else
                query = filterDesc.Not(n => n.Term(searchFields[filter.Field], filter.SearchText));

            return query;
        }

        private FilterContainer isEmptyQuery(SearchFilter filter)
        {
            FilterContainer query = new FilterContainer();
            int tourOrActionField = -1;

            if (filter.IsDropdownField && filter.DropdownId.HasValue && filter.DropdownId.Value == (byte)DropdownFieldTypes.PhoneNumberType)
            {
                int phoneType = default(int);
                if (filter.Field == ContactFields.DropdownField)  // edit/run 
                    phoneType = filter.DropdownValueId.Value;
                else                                             //  add 
                    phoneType = (int)filter.Field;
                query = filterDesc.Not(n => n.And(filterDesc.Term("phones.phoneType", phoneType),
                   filterDesc.Term("phones.number", "")));
            }
            else if (filter.IsCustomField)
                query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Not(n => n.Nested(ne => ne.Path("customFields").Filter(f => f.Bool(b => b.Must(m => m.Term("customFields.customFieldId", (int)filter.Field)))))))));
            else if (filter.Field == ContactFields.WebPage)
                query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Not(no => no.Nested(n => n.Path("webVisits").Filter(f => f.Exists("webVisits.pageVisited")))))));
            else if (filter.Field == ContactFields.LeadSource || filter.Field == ContactFields.FirstLeadSource || filter.Field == ContactFields.FirstLeadSourceDate || filter.Field == ContactFields.LeadSourceDate)
            {
                bool firstSources = filter.Field == ContactFields.FirstLeadSource || filter.Field == ContactFields.FirstLeadSourceDate ? true : false;
                query = filterDesc.Query(q => q.Filtered(f => f.Filter(fi => fi.Not(n => n.Nested(ne => ne.Path("leadSources").Filter(fil => fil.Bool(b =>
                    b.Must(m => m.Term("leadSources.isPrimary", firstSources)))))))));
            }
            else if ((tourOrActionField = isTourOrActionField(filter.Field)) > -1)
            {
                string pathName = tourOrActionField > 4 ? tourOrActionField > 9 ? "contactNotes" : "contactActions" : "tourCommunity";
                query = filterDesc.Query(q => q.Filtered(f => f.Filter(fi => fi.Not(n => n.Nested(ne => ne.Path(pathName).Filter(fil => fil.Exists(searchFields[filter.Field])))))));
            }
            else
            {
                List<ContactFields> contactNumberFields = new List<ContactFields>() { ContactFields.LifecycleStageField, ContactFields.Owner, ContactFields.PartnerTypeField, ContactFields.LeadScore, ContactFields.LastTouchedThrough,
                                                                                              ContactFields.Community, ContactFields.FirstSourceType,ContactFields.LastNoteCategory };
                object term = contactNumberFields.Contains(filter.Field) ? 0.ToString() : "";
                query = filterDesc.Or(o => o.Missing(searchFields[filter.Field]), o => o.Term(searchFields[filter.Field], term));
            }

            return query;
        }

        private FilterContainer isNotEmptyQuery(SearchFilter filter)
        {
            FilterContainer query = new FilterContainer();
            int tourOrActionField = -1;

            if (filter.IsDropdownField && filter.DropdownId.HasValue && filter.DropdownId.Value == (byte)DropdownFieldTypes.PhoneNumberType)
            {
                int phoneType = default(int);
                if (filter.Field == ContactFields.DropdownField)  // edit/run modes
                    phoneType = filter.DropdownValueId.Value;
                else
                    phoneType = (int)filter.Field;
                query = filterDesc.And(filterDesc.Term("phones.phoneType", phoneType),
                        filterDesc.Exists("phones.number"));
            }
            else if (filter.IsCustomField)
            {
                query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("customFields").Filter(f => f.Bool(b => b.Must(m => m.Term("customFields.customFieldId", (int)filter.Field))))))));
                //.InnerHits(inn => inn.Name("customFields.First"))))));
            }
            else if (filter.Field == ContactFields.LeadSource || filter.Field == ContactFields.FirstLeadSource || filter.Field == ContactFields.FirstLeadSourceDate || filter.Field == ContactFields.LeadSourceDate)
            {
                bool firstSources = filter.Field == ContactFields.FirstLeadSource || filter.Field == ContactFields.FirstLeadSourceDate ? true : false;
                query = filterDesc.Query(q => q.Filtered(f => f.Filter(fi => fi.Nested(n => n.Path("leadSources").Filter(fil => fil.Bool(b => b.Must(m => m.Term("leadSources.isPrimary", firstSources))))))));
            }
            else if (filter.Field == ContactFields.WebPage)
                query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("webVisits").Filter(f => f.Exists("webVisits.pageVisited"))))));
            else if ((tourOrActionField = isTourOrActionField(filter.Field)) > -1)
            {
                string pathName = tourOrActionField > 4 ? tourOrActionField > 9 ? "contactNotes" : "contactActions" : "tourCommunity";
                query = filterDesc.Query(q => q.Filtered(f => f.Filter(n => n.Nested(ne => ne.Path(pathName).Filter(fil => fil.Exists(searchFields[filter.Field]))))));
            }
            else
            {
                List<ContactFields> contactNumberFields = new List<ContactFields>() { ContactFields.LifecycleStageField, ContactFields.Owner, ContactFields.PartnerTypeField, ContactFields.LeadScore, ContactFields.LastTouchedThrough,
                                                                                              ContactFields.Community, ContactFields.FirstSourceType,ContactFields.LastNoteCategory };
                object term = contactNumberFields.Contains(filter.Field) ? 0.ToString() : "";
                query = filterDesc.And(a => a.Exists(searchFields[filter.Field]), a => a.Not(n => n.Term(searchFields[filter.Field], term)));  // The term should not only be empty, it should not be 0.
            }

            return query;
        }

        private FilterContainer containsQuery(SearchFilter filter)
        {
            FilterContainer query = new FilterContainer();

            if (filter.IsCustomField)
            {
                string customFieldValue = (filter.FieldOptionTypeId == (byte)Entities.FieldType.date || filter.FieldOptionTypeId == (byte)Entities.FieldType.datetime || filter.FieldOptionTypeId == (byte)Entities.FieldType.time)
                    ? "customFields.value_Date" : (filter.FieldOptionTypeId == (byte)Entities.FieldType.multiselectdropdown || filter.FieldOptionTypeId == (byte)Entities.FieldType.checkbox)
                    ? "customFields.value_Multiselect" : (filter.FieldOptionTypeId == (byte)Entities.FieldType.number) ? "customFields.value_Number" : "customFields.value";

                if (filter.FieldOptionTypeId.HasValue && (filter.FieldOptionTypeId == (byte)Entities.FieldType.multiselectdropdown || filter.FieldOptionTypeId == (byte)Entities.FieldType.number
                    || filter.FieldOptionTypeId == (byte)Entities.FieldType.checkbox))
                {
                    string[] sources = filter.SearchText.Split('|');
                    //query = filterDesc.And(filterDesc.Term("customFields.customFieldId", (int)filter.Field),
                    //filterDesc.Terms(customFieldValue, sources, TermsExecution.And));
                    FilterContainer newQuery = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("customFields").Filter(f => f.Bool(b => b.Must(m => m.Term("customFields.customFieldId", (int)filter.Field),
                        mus => mus.Terms(customFieldValue, sources, TermsExecution.And))))))));
                    query = newQuery;
                }
                else
                {
                    FilterContainer newQuery = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("customFields").Filter(f => f.Bool(b => b.Must(m => m.Term("customFields.customFieldId", (int)filter.Field),
                           mus => mus.Regexp(rg => rg.OnField(customFieldValue).Value(".*" + filter.SearchText + ".*")))))))));
                    query = newQuery;
                }
                //query = filterDesc.And(filterDesc.Regexp(rg => rg.OnField(customFieldValue).Value(".*" + filter.SearchText + ".*")),
                //filterDesc.Term("customFields.customFieldId", (int)filter.Field));

            }
            else if (filter.Field == ContactFields.WebPage)
            {
                query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("webVisits").Filter(f => f.Regexp(r => r.OnField("webVisits.pageVisited").Value(".*" + filter.SearchText + ".*")))))));
            }
            else if (filter.Field == ContactFields.LeadSource)
            {
                var sources = filter.SearchText.Split('|');
                query = filterDesc.And(filterDesc.Terms(searchFields[filter.Field], sources, TermsExecution.And));
            }
            else
                query = filterDesc.Regexp(rg => rg.OnField(searchFields[filter.Field]).Value(".*" + filter.SearchText + ".*"));

            return query;
        }

        private FilterContainer doesNotContainsQuery(SearchFilter filter)
        {
            FilterContainer query = new FilterContainer();

            if (filter.IsCustomField)
            {
                string customFieldValue = (filter.FieldOptionTypeId == (byte)Entities.FieldType.date || filter.FieldOptionTypeId == (byte)Entities.FieldType.datetime || filter.FieldOptionTypeId == (byte)Entities.FieldType.time)
                      ? "customFields.value_Date" : (filter.FieldOptionTypeId == (byte)Entities.FieldType.multiselectdropdown || filter.FieldOptionTypeId == (byte)Entities.FieldType.checkbox)
                      ? "customFields.value_Multiselect" : (filter.FieldOptionTypeId == (byte)Entities.FieldType.number) ? "customFields.value_Number" : "customFields.value";

                if (filter.FieldOptionTypeId.HasValue && (filter.FieldOptionTypeId == (byte)Entities.FieldType.multiselectdropdown || filter.FieldOptionTypeId == (byte)Entities.FieldType.number
                     || filter.FieldOptionTypeId == (byte)Entities.FieldType.checkbox))
                {
                    string[] sources = filter.SearchText.Split('|');
                    //query = filterDesc.And(filterDesc.Term("customFields.customFieldId", (int)filter.Field),
                    //  filterDesc.Not(n => n.Terms(customFieldValue, sources, TermsExecution.And)));
                    FilterContainer newQuery = query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("customFields").Filter(f => f.Bool(b => b.Must(m => m.Term("customFields.customFieldId", (int)filter.Field))
                        .MustNot(mn => mn.Terms(customFieldValue, sources, TermsExecution.And))))))));
                    query = newQuery;
                }
                //query = filterDesc.Regexp(rg => rg.OnField("customFields.value").Value("^((?!" + filter.SearchText + ").)*$"));
                else
                {
                    FilterContainer newQuery = query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("customFields").Filter(f => f.Bool(b => b.Must(m => m.Term("customFields.customFieldId", (int)filter.Field))
                           .MustNot(mn => mn.Regexp(rg => rg.OnField(customFieldValue).Value(".*" + filter.SearchText + ".*")))))))));
                    query = newQuery;
                }
                //query = filterDesc.And(filterDesc.Term("customFields.customFieldId", (int)filter.Field),
                // filterDesc.Not(n => n.Regexp(rg => rg.OnField(customFieldValue).Value(".*" + filter.SearchText + ".*"))));

            }
            else if (filter.Field == ContactFields.WebPage)
            {
                query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("webVisits").Filter(f => f.Not(no => no.Regexp(r => r.OnField("webVisits.pageVisited").Value(".*" + filter.SearchText + ".*"))))))));
            }
            else if (filter.Field == ContactFields.LeadSource)
            {
                // var source = filter.SearchText.Split('|');
                query = filterDesc.Not(n => n.Term(searchFields[filter.Field], filter.SearchText));
            }
            else
                //query = filterDesc.Regexp(rg => rg.OnField(searchFields[filter.Field]).Value("^((?!" + filter.SearchText + ").)*$"));
                query = filterDesc.Not(n => n.Regexp(rg => rg.OnField(searchFields[filter.Field]).Value(".*" + filter.SearchText + ".*")));

            return query;
        }

        private FilterContainer isLessThanQuery(SearchFilter filter)
        {
            FilterContainer query = new FilterContainer();
            int tourOrActionField = -1;

            if (filter.Field == ContactFields.LastTouched || filter.Field == ContactFields.CreatedOn || filter.Field == ContactFields.LastUpdateOn || filter.Field == ContactFields.FormsubmittedOn
                        || filter.Field == ContactFields.LeadSourceDate || filter.Field == ContactFields.FirstLeadSourceDate || filter.Field == ContactFields.LastNoteDate || filter.Field == ContactFields.TourDate
                || filter.Field == ContactFields.ActionDate || filter.Field == ContactFields.ActionCreatedDate)
            {
                DateTime date;
                DateTime.TryParse(filter.SearchText, out date);
                date = DateMath(date);
                string dateString = date.ToString("MM/dd/yyyy");
                if (filter.Field == ContactFields.LeadSourceDate || filter.Field == ContactFields.FirstLeadSourceDate)
                    query = filterDesc.Query(q => q.Filtered(f => f.Filter(fi => fi.Nested(n => n.Path("leadSources").Filter(nf =>
                        nf.Bool(b => b.MustNot(mn => mn.Range(mnr => mnr.GreaterOrEquals(dateString).OnField(searchFields[filter.Field])
                            )).Must(m => m.Range(mrl => mrl.Lower(dateString).OnField(searchFields[filter.Field])), mu => mu.Term("leadSources.isPrimary", filter.Field == ContactFields.FirstLeadSourceDate ? true : false))))))));
                else if (filter.Field == ContactFields.TourDate || filter.Field == ContactFields.ActionDate || filter.Field == ContactFields.ActionCreatedDate)
                {
                    tourOrActionField = isTourOrActionField(filter.Field);
                    string pathName = tourOrActionField > 4 ? tourOrActionField > 9 ? "contactNotes" : "contactActions" : "tourCommunity";

                    query = filterDesc.Query(q => q.Filtered(f => f.Filter(fi => fi.Nested(n => n.Path(pathName).Filter(nf =>
                        nf.Bool(b => b.MustNot(mn => mn.Range(mnr => mnr.GreaterOrEquals(dateString).OnField(searchFields[filter.Field])
                            )).Must(m => m.Range(mrl => mrl.Lower(dateString).OnField(searchFields[filter.Field])))))))));
                }
                else
                    query = filterDesc.Query(q => q.Filtered(qf => qf.Filter(f => f.Bool(b => b.MustNot(mn => mn.Range(mnr => mnr.GreaterOrEquals(dateString).OnField(searchFields[filter.Field])
                            )).Must(m => m.Range(mrl => mrl.Lower(dateString).OnField(searchFields[filter.Field])))))));
            }
            else if (filter.IsCustomField)
            {
                string customFieldValue = (filter.FieldOptionTypeId == (byte)Entities.FieldType.date || filter.FieldOptionTypeId == (byte)Entities.FieldType.datetime || filter.FieldOptionTypeId == (byte)Entities.FieldType.time)
                    ? "customFields.value_Date" : (filter.FieldOptionTypeId == (byte)Entities.FieldType.multiselectdropdown || filter.FieldOptionTypeId == (byte)Entities.FieldType.checkbox)
                    ? "customFields.value_Multiselect" : (filter.FieldOptionTypeId == (byte)Entities.FieldType.number) ? "customFields.value_Number" : "customFields.value";

                int from = 0;
                int.TryParse(filter.SearchText, out from);

                if (filter.FieldOptionTypeId == (byte)Entities.FieldType.date || filter.FieldOptionTypeId == (byte)Entities.FieldType.datetime || filter.FieldOptionTypeId == (byte)Entities.FieldType.time)
                {
                    DateTime date = new DateTime();
                    DateTime.TryParse(filter.SearchText, out date);
                    date = DateMath(date);
                    var specifiedDate = date.Date;
                    var dateTicks = specifiedDate.Ticks;
                    var dateTimeTicks = date.Ticks;
                    var aggTicks = dateTimeTicks - dateTicks;
                    DateTime time = new DateTime(aggTicks);
                    string dateString = filter.FieldOptionTypeId == (byte)Entities.FieldType.time ? time.ToString("yyyy-MM-dd'T'HH:mm:ss") : date.ToString("yyyy-MM-dd'T'HH:mm:ss");
                    query = filterDesc
                        .Query(q =>
                            q.Filtered(qf =>
                                qf.Filter(f =>
                                    f.Nested(n =>
                                        n.Path("customFields")
                                        .Filter(fi =>
                                            fi.Bool(b =>
                                                b.MustNot(mn =>
                                                    mn.Range(mnr => mnr.GreaterOrEquals(dateString).OnField(customFieldValue)))
                                                 .Must(m =>
                                                    m.Range(mrl => mrl.Lower(dateString).OnField(customFieldValue)),
                                                    mrt => mrt.Term("customFields.customFieldId", (int)filter.Field)))
                                                    )))));
                }
                else
                    query = filterDesc
                        .Query(q =>
                            q.Filtered(qf =>
                                qf.Filter(f =>
                                    f.Nested(n =>
                                        n.Path("customFields")
                                        .Filter(fi =>
                                            f.Bool(b =>
                                                 b.MustNot(mn =>
                                                     mn.Range(mnr => mnr.GreaterOrEquals(from).OnField(customFieldValue)))
                                                  .Must(m =>
                                                     m.Range(mrl => mrl.Lower(from).OnField(customFieldValue)),
                                                     mrt => mrt.Term("customFields.customFieldId", (int)filter.Field))
                                                     ))))));
            }
            else if (filter.Field == ContactFields.WebPageDuration)
            {
                int from = 0;
                int.TryParse(filter.SearchText, out from);
                query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("webVisits").Filter(f => f.Range(r => r.Lower(from).OnField("webVisits.duration")))))));
            }
            else
            {
                int from = 0;
                int.TryParse(filter.SearchText, out from);
                query = filterDesc.Query(q => q.Filtered(qf => qf.Filter(f => f.Bool(b => b.MustNot(mn => mn.Range(mnr => mnr.GreaterOrEquals(from).OnField(searchFields[filter.Field])
                        )).Must(m => m.Range(mrl => mrl.Lower(from).OnField(searchFields[filter.Field])))))));
            }

            return query;
        }

        private FilterContainer isLessThanOrEqualToQuery(SearchFilter filter)
        {
            FilterContainer query = new FilterContainer();
            int tourOrActionField = -1;

            if (filter.Field == ContactFields.LastTouched || filter.Field == ContactFields.CreatedOn || filter.Field == ContactFields.LastUpdateOn || filter.Field == ContactFields.FormsubmittedOn
                        || filter.Field == ContactFields.LeadSourceDate || filter.Field == ContactFields.FirstLeadSourceDate || filter.Field == ContactFields.LastNoteDate || filter.Field == ContactFields.TourDate
                || filter.Field == ContactFields.ActionDate || filter.Field == ContactFields.ActionCreatedDate)
            {
                DateTime date;
                DateTime.TryParse(filter.SearchText, out date);
                date = DateMath(date).AddHours(23).AddMinutes(59);
                if (filter.Field == ContactFields.LeadSourceDate || filter.Field == ContactFields.FirstLeadSourceDate)
                    query = filterDesc.Query(q => q.Filtered(f => f.Filter(fi => fi.Nested(n => n.Path("leadSources").Filter(nf =>
                      nf.Bool(b => b.MustNot(mn => mn.Range(mnr => mnr.Greater(date).OnField(searchFields[filter.Field])
                          )).Must(m => m.Range(mrl => mrl.LowerOrEquals(date).OnField(searchFields[filter.Field])), mu => mu.Term("leadSources.isPrimary", filter.Field == ContactFields.FirstLeadSourceDate ? true : false))))))));
                else if (filter.Field == ContactFields.TourDate || filter.Field == ContactFields.ActionDate || filter.Field == ContactFields.ActionCreatedDate)
                {
                    tourOrActionField = isTourOrActionField(filter.Field);
                    string pathName = tourOrActionField > 4 ? tourOrActionField > 9 ? "contactNotes" : "contactActions" : "tourCommunity";

                    query = filterDesc.Query(q => q.Filtered(f => f.Filter(fi => fi.Nested(n => n.Path(pathName).Filter(nf =>
                      nf.Bool(b => b.MustNot(mn => mn.Range(mnr => mnr.Greater(date).OnField(searchFields[filter.Field])
                          )).Must(m => m.Range(mrl => mrl.LowerOrEquals(date).OnField(searchFields[filter.Field])))))))));
                }
                else
                    query = filterDesc.Query(q => q.Filtered(qf => qf.Filter(f => f.Bool(b => b.MustNot(mn => mn.Range(mnr => mnr.Greater(date).OnField(searchFields[filter.Field])
                            )).Must(m => m.Range(mrl => mrl.LowerOrEquals(date).OnField(searchFields[filter.Field])))))));
            }
            else if (filter.IsCustomField)
            {
                string customFieldValue = (filter.FieldOptionTypeId == (byte)Entities.FieldType.date || filter.FieldOptionTypeId == (byte)Entities.FieldType.datetime || filter.FieldOptionTypeId == (byte)Entities.FieldType.time)
                    ? "customFields.value_Date" : (filter.FieldOptionTypeId == (byte)Entities.FieldType.multiselectdropdown || filter.FieldOptionTypeId == (byte)Entities.FieldType.checkbox)
                    ? "customFields.value_Multiselect" : (filter.FieldOptionTypeId == (byte)Entities.FieldType.number) ? "customFields.value_Number" : "customFields.value";

                int from = 0;
                int.TryParse(filter.SearchText, out from);

                if (filter.FieldOptionTypeId == (byte)Entities.FieldType.date || filter.FieldOptionTypeId == (byte)Entities.FieldType.datetime || filter.FieldOptionTypeId == (byte)Entities.FieldType.time)
                {
                    DateTime date = new DateTime();
                    DateTime.TryParse(filter.SearchText, out date);
                    date = DateMath(date).AddHours(23).AddMinutes(59);
                    var specifiedDate = date.Date;
                    var dateTicks = specifiedDate.Ticks;
                    var dateTimeTicks = date.Ticks;
                    var aggTicks = dateTimeTicks - dateTicks;
                    DateTime time = new DateTime(aggTicks);
                    string dateString = filter.FieldOptionTypeId == (byte)Entities.FieldType.time ? time.ToString("yyyy-MM-dd'T'HH:mm:ss") : date.ToString("yyyy-MM-dd'T'HH:mm:ss");
                    query = filterDesc
                        .Query(q =>
                            q.Filtered(qf =>
                                qf.Filter(f =>
                                    f.Nested(n =>
                                        n.Path("customFields")
                                        .Filter(fi =>
                                            fi.Bool(b =>
                                                b.MustNot(mn =>
                                                    mn.Range(mnr => mnr.Greater(dateString).OnField(customFieldValue)))
                                                 .Must(m =>
                                                    m.Range(mrl => mrl.LowerOrEquals(dateString).OnField(customFieldValue)),
                                                    mrt => mrt.Term("customFields.customFieldId", (int)filter.Field)))
                                                    )))));
                }
                else
                    query = filterDesc
                            .Query(q =>
                                q.Filtered(qf =>
                                    qf.Filter(f =>
                                        f.Nested(n =>
                                            n.Path("customFields")
                                            .Filter(fi =>
                                                fi.Bool(b =>
                                                    b.MustNot(mn =>
                                                        mn.Range(mnr => mnr.Greater(from).OnField(customFieldValue)))
                                                     .Must(m =>
                                                        m.Range(mrl => mrl.LowerOrEquals(from).OnField(customFieldValue)),
                                                        mrt => mrt.Term("customFields.customFieldId", (int)filter.Field)))
                                                        )))));
            }
            else if (filter.Field == ContactFields.WebPageDuration)
            {
                int from = 0;
                int.TryParse(filter.SearchText, out from);
                query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("webVisits").Filter(f => f.Range(r => r.LowerOrEquals(from).OnField("webVisits.duration")))))));
            }
            else
            {
                int from = 0;
                int.TryParse(filter.SearchText, out from);
                query = filterDesc.Query(q => q.Filtered(qf => qf.Filter(f => f.Bool(b => b.MustNot(mn => mn.Range(mnr => mnr.Greater(from).OnField(searchFields[filter.Field])
                        )).Must(m => m.Range(mrl => mrl.LowerOrEquals(from).OnField(searchFields[filter.Field])))))));
            }

            return query;
        }

        private FilterContainer isGreaterThanQuery(SearchFilter filter)
        {
            FilterContainer query = new FilterContainer();
            int tourOrActionField = -1;

            if (filter.Field == ContactFields.LastTouched || filter.Field == ContactFields.CreatedOn || filter.Field == ContactFields.LastUpdateOn || filter.Field == ContactFields.FormsubmittedOn
                        || filter.Field == ContactFields.LeadSourceDate || filter.Field == ContactFields.FirstLeadSourceDate || filter.Field == ContactFields.LastNoteDate || filter.Field == ContactFields.TourDate
                || filter.Field == ContactFields.ActionDate || filter.Field == ContactFields.ActionCreatedDate)
            {
                DateTime date;
                DateTime.TryParse(filter.SearchText, out date);
                date = DateMath(date).AddDays(1);
                if (filter.Field == ContactFields.LeadSourceDate || filter.Field == ContactFields.FirstLeadSourceDate)
                    query = filterDesc.Query(q => q.Filtered(f => f.Filter(fi => fi.Nested(n => n.Path("leadSources").Filter(nf =>
                        nf.Bool(b => b.Must(m => m.Range(r => r.Greater(date).OnField(searchFields[filter.Field])),
                        n1 => n1.Term("leadSources.isPrimary", filter.Field == ContactFields.FirstLeadSourceDate ? true : false))))))));
                else if (filter.Field == ContactFields.TourDate || filter.Field == ContactFields.ActionDate || filter.Field == ContactFields.ActionCreatedDate)
                {
                    tourOrActionField = isTourOrActionField(filter.Field);
                    string pathName = tourOrActionField > 4 ? tourOrActionField > 9 ? "contactNotes" : "contactActions" : "tourCommunity";

                    query = filterDesc.Query(q => q.Filtered(f => f.Filter(fi => fi.Nested(n => n.Path(pathName).Filter(nf =>
                        nf.Bool(b => b.Must(m => m.Range(r => r.Greater(date).OnField(searchFields[filter.Field])))))))));
                }
                else
                    query = filterDesc.Range(n => n.Greater(date).OnField(searchFields[filter.Field]));
            }
            else if (filter.IsCustomField)
            {
                string customFieldValue = (filter.FieldOptionTypeId == (byte)Entities.FieldType.date || filter.FieldOptionTypeId == (byte)Entities.FieldType.datetime || filter.FieldOptionTypeId == (byte)Entities.FieldType.time)
                    ? "customFields.value_Date" : (filter.FieldOptionTypeId == (byte)Entities.FieldType.multiselectdropdown || filter.FieldOptionTypeId == (byte)Entities.FieldType.checkbox)
                    ? "customFields.value_Multiselect" : (filter.FieldOptionTypeId == (byte)Entities.FieldType.number) ? "customFields.value_Number" : "customFields.value";

                int from = 0;
                int.TryParse(filter.SearchText, out from);

                if (filter.FieldOptionTypeId == (byte)Entities.FieldType.date || filter.FieldOptionTypeId == (byte)Entities.FieldType.datetime || filter.FieldOptionTypeId == (byte)Entities.FieldType.time)
                {
                    DateTime date = new DateTime();
                    DateTime.TryParse(filter.SearchText, out date);
                    date = DateMath(date).AddDays(1);
                    var specifiedDate = date.Date;
                    var dateTicks = specifiedDate.Ticks;
                    var dateTimeTicks = date.Ticks;
                    var aggTicks = dateTimeTicks - dateTicks;
                    DateTime time = new DateTime(aggTicks);
                    string dateString = filter.FieldOptionTypeId == (byte)Entities.FieldType.time ? time.ToString("yyyy-MM-dd'T'HH:mm:ss") : date.ToString("yyyy-MM-dd'T'HH:mm:ss");
                    query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("customFields").Filter(f => f.Bool(b => b.Must(m => m.Term("customFields.customFieldId", (int)filter.Field),
                        mus => mus.Range(r => r.Greater(dateString).OnField(customFieldValue)))))))));
                }
                else
                    //query = filterDesc.And(filterDesc.Term("customFields.customFieldId", (int)filter.Field),
                    //filterDesc.Range(n => n.Greater(from).OnField(customFieldValue)));

                    query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("customFields").Filter(f => f.Bool(b => b.Must(m => m.Term("customFields.customFieldId", (int)filter.Field),
                            mus => mus.Range(r => r.Greater(from).OnField(customFieldValue)))))))));   //.InnerHits(inn => inn.Name("customFields.Second"))
            }
            else if (filter.Field == ContactFields.WebPageDuration)
            {
                int from = 0;
                int.TryParse(filter.SearchText, out from);
                query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("webVisits").Filter(f => f.Range(r => r.Greater(from).OnField("webVisits.duration")))))));
            }
            else
            {
                int from = 0;
                int.TryParse(filter.SearchText, out from);
                query = filterDesc.Range(n => n.Greater(from).OnField(searchFields[filter.Field]));
            }

            return query;
        }

        private FilterContainer isGreaterThanOrEqualToQuery(SearchFilter filter)
        {
            FilterContainer query = new FilterContainer();
            int tourOrActionField = -1;

            if (filter.Field == ContactFields.LastTouched || filter.Field == ContactFields.CreatedOn || filter.Field == ContactFields.LastUpdateOn || filter.Field == ContactFields.FormsubmittedOn
                        || filter.Field == ContactFields.LeadSourceDate || filter.Field == ContactFields.FirstLeadSourceDate || filter.Field == ContactFields.LastNoteDate || filter.Field == ContactFields.TourDate
                || filter.Field == ContactFields.ActionDate || filter.Field == ContactFields.ActionCreatedDate)
            {
                DateTime date;
                DateTime.TryParse(filter.SearchText, out date);
                date = DateMath(date);
                if (filter.Field == ContactFields.LeadSourceDate || filter.Field == ContactFields.FirstLeadSourceDate)
                    query = filterDesc.Query(q => q.Filtered(f => f.Filter(fi => fi.Nested(n => n.Path("leadSources").Filter(nf =>
                        nf.Bool(b => b.Must(m => m.Range(r => r.GreaterOrEquals(date).OnField(searchFields[filter.Field])),
                        n1 => n1.Term("leadSources.isPrimary", filter.Field == ContactFields.FirstLeadSourceDate ? true : false))))))));
                else if (filter.Field == ContactFields.TourDate || filter.Field == ContactFields.ActionDate || filter.Field == ContactFields.ActionCreatedDate)
                {
                    tourOrActionField = isTourOrActionField(filter.Field);
                    string pathName = tourOrActionField > 4 ? tourOrActionField > 9 ? "contactNotes" : "contactActions" : "tourCommunity";

                    query = filterDesc.Query(q => q.Filtered(f => f.Filter(fi => fi.Nested(n => n.Path(pathName).Filter(nf =>
                        nf.Bool(b => b.Must(m => m.Range(r => r.GreaterOrEquals(date).OnField(searchFields[filter.Field])))))))));
                }
                else
                    query = filterDesc.Range(n => n.GreaterOrEquals(date).OnField(searchFields[filter.Field]));
            }
            else if (filter.IsCustomField)
            {
                string customFieldValue = (filter.FieldOptionTypeId == (byte)Entities.FieldType.date || filter.FieldOptionTypeId == (byte)Entities.FieldType.datetime || filter.FieldOptionTypeId == (byte)Entities.FieldType.time)
                    ? "customFields.value_Date" : (filter.FieldOptionTypeId == (byte)Entities.FieldType.multiselectdropdown || filter.FieldOptionTypeId == (byte)Entities.FieldType.checkbox)
                    ? "customFields.value_Multiselect" : (filter.FieldOptionTypeId == (byte)Entities.FieldType.number) ? "customFields.value_Number" : "customFields.value";

                int from = 0;
                int.TryParse(filter.SearchText, out from);

                if (filter.FieldOptionTypeId == (byte)Entities.FieldType.date || filter.FieldOptionTypeId == (byte)Entities.FieldType.datetime || filter.FieldOptionTypeId == (byte)Entities.FieldType.time)
                {
                    DateTime date = new DateTime();
                    DateTime.TryParse(filter.SearchText, out date);
                    date = DateMath(date);
                    var specifiedDate = date.Date;
                    var dateTicks = specifiedDate.Ticks;
                    var dateTimeTicks = date.Ticks;
                    var aggTicks = dateTimeTicks - dateTicks;
                    DateTime time = new DateTime(aggTicks);
                    string dateString = filter.FieldOptionTypeId == (byte)Entities.FieldType.time ? time.ToString("yyyy-MM-dd'T'HH:mm:ss") : date.ToString("yyyy-MM-dd'T'HH:mm:ss");
                    //query = filterDesc.And(filterDesc.Term("customFields.customFieldId", (int)filter.Field),
                    //filterDesc.Range(n => n.GreaterOrEquals(dateString).OnField(customFieldValue)));

                    query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("customFields").Filter(f => f.Bool(b => b.Must(m => m.Term("customFields.customFieldId", (int)filter.Field),
                        mus => mus.Range(r => r.GreaterOrEquals(dateString).OnField(customFieldValue)))))))));
                }
                else
                    //query = filterDesc.And(filterDesc.Term("customFields.customFieldId", (int)filter.Field), filterDesc.Range(n => n.GreaterOrEquals(from).OnField(customFieldValue)));
                    query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("customFields").Filter(f => f.Bool(b => b.Must(m => m.Term("customFields.customFieldId", (int)filter.Field),
                                mus => mus.Range(r => r.GreaterOrEquals(from).OnField(customFieldValue)))))))));
            }
            else if (filter.Field == ContactFields.WebPageDuration)
            {
                int from = 0;
                int.TryParse(filter.SearchText, out from);
                query = filterDesc.Query(q => q.Filtered(fi => fi.Filter(fil => fil.Nested(n => n.Path("webVisits").Filter(f => f.Range(r => r.GreaterOrEquals(from).OnField("webVisits.duration")))))));
            }
            else
            {
                int from = 0;
                int.TryParse(filter.SearchText, out from);
                query = filterDesc.Range(n => n.GreaterOrEquals(from).OnField(searchFields[filter.Field]));
            }

            return query;
        }

        private SearchFilter getSearchFilter(string index)
        {
            int filterId = 0;
            int.TryParse(index, out filterId);
            SearchFilter filter = this.searchFilters.ElementAt(filterId - 1);
            return filter;
        }

        private void setSearchFilters(SearchDefinition definition)
        {
            var filters = definition.Filters.ToList();

            foreach (var filter in filters)
            {
                if (filter.Field == ContactFields.FirstNameField)
                    filter.Field = ContactFields.FirstName_NotAnalyzed;
                else if (filter.Field == ContactFields.LastNameField)
                    filter.Field = ContactFields.LastName_NotAnalyzed;
                else if (filter.Field == ContactFields.CompanyNameField)
                    filter.Field = ContactFields.CompanyName_NotAnalyzed;
                else if (filter.Field == ContactFields.StateField)
                {
                    filter.Field = ContactFields.StateCodeField;
                    continue;
                }
                else if (filter.Field == ContactFields.CountryField)
                    filter.Field = ContactFields.CountryCode;

                if (filter.SearchText != null)
                {
                    filter.SearchText = !filter.IsCustomField ? filter.SearchText.ToLower() : filter.SearchText;
                }
            }
            Action<ContactFields, string> Add = (key, value) =>
            {
                if (!searchFields.ContainsKey(key))
                    searchFields.Add(key, value);
            };
            if (searchFields.IsAny())
            {
                // more_like_this doesn't support binary/numeric fields
                Add(ContactFields.MobilePhoneField, "phones.number");
                Add(ContactFields.HomePhoneField, "phones.number");
                Add(ContactFields.LifecycleStageField, "lifecycleStage");
                Add(ContactFields.PartnerTypeField, "partnerType");
                Add(ContactFields.DonotEmail, "doNotEmail");
                Add(ContactFields.LeadSource, "leadSources.id");
                Add(ContactFields.Owner, "ownerId");
                Add(ContactFields.CreatedBy, "createdBy");
                Add(ContactFields.CreatedOn, "createdOn");
                Add(ContactFields.LastTouched, "lastContacted");
                Add(ContactFields.LastTouchedThrough, "lastContactedThrough");

                // Advanced Search
                Add(ContactFields.PrimaryEmailStatus, "emails.emailStatusValue");
                Add(ContactFields.IsPrimaryEmail, "emails.isPrimary");
                Add(ContactFields.CompanyId, "companyID");
                Add(ContactFields.LeadAdapter, "firstSourceType");
                Add(ContactFields.LeadSourceDate, "leadSources.lastUpdatedDate");
                Add(ContactFields.FirstLeadSource, "leadSources.id");
                Add(ContactFields.FirstLeadSourceDate, "leadSources.lastUpdatedDate");
                Add(ContactFields.TourCreator, "tourCommunity.createdBy");
                Add(ContactFields.TourDate, "tourCommunity.tourDate");
                Add(ContactFields.TourType, "tourCommunity.tourType");
                Add(ContactFields.Community, "tourCommunity.communityID");
                Add(ContactFields.TourAssignedUsers, "tourCommunity.associatedUsers");
                Add(ContactFields.ActionAssignedTo, "contactActions.associatedUsers");
                Add(ContactFields.ActionCreatedDate, "contactActions.createdOn");
                Add(ContactFields.ActionDate, "contactActions.actionDate");
                // Add(ContactFields.ActionStatus, "contactActions.");
                Add(ContactFields.ActionType, "contactActions.actionType");
                Add(ContactFields.NoteCategory, "contactNotes.noteCategory");
                Add(ContactFields.LastNoteCategory, "lastNoteCategory");

                this.searchFilters = filters;
            }
        }

        private SearchResult<T> search<D>(string q, Expression<Func<T, bool>> filter, SearchParameters searchParameters)
        {
            SearchResult<T> searchResult = new SearchResult<T>();
            int limit = searchParameters.Limit == 0 ? 10 : searchParameters.Limit;
            int from = (searchParameters.PageNumber - 1) * limit;
            ISearchResponse<T> result;

            SearchDescriptor<T> descriptor = new SearchDescriptor<T>();
            descriptor = descriptor.Types(searchParameters.Types)
                            .From(from)
                            .Size(limit);

            if (searchParameters.SortField.HasValue)
            {
                if (searchParameters.SortField == Entities.ContactSortFieldType.RecentlyUpdatedContact)
                    descriptor = descriptor.Sort(s =>
                        s.OnField(searchFields[ContactFields.LastUpdateOn]).Descending().UnmappedType(Nest.FieldType.None));
                else if (searchParameters.SortField == Entities.ContactSortFieldType.FullName)
                    descriptor = descriptor.Sort(s => s.OnField(searchFields[ContactFields.FirstName_NotAnalyzed]).Ascending().UnmappedType(Nest.FieldType.None))
                .Sort(s => s.OnField(searchFields[ContactFields.LastName_NotAnalyzed]).Ascending().UnmappedType(Nest.FieldType.None));
                else if (searchParameters.SortField == Entities.ContactSortFieldType.CompanyName)
                    descriptor = descriptor.Sort(s => s.OnField(searchFields[ContactFields.CompanyName_NotAnalyzed]).Ascending().UnmappedType(Nest.FieldType.None));
                else if (searchParameters.SortField == Entities.ContactSortFieldType.LeadScore)
                    descriptor = descriptor.Sort(s => s.OnField(searchFields[ContactFields.LeadScore]).Descending().UnmappedType(Nest.FieldType.None));
                else if (searchParameters.SortField == Entities.ContactSortFieldType.FullNameOrCompanyName)
                    descriptor = descriptor.Sort(s => s.OnField(searchFields[ContactFields.CompanyName_NotAnalyzed]).Ascending().UnmappedType(Nest.FieldType.None))
                        .Sort(s => s.OnField(searchFields[ContactFields.FirstName_NotAnalyzed]).Ascending().UnmappedType(Nest.FieldType.None))
                        .Sort(s => s.OnField(searchFields[ContactFields.LastName_NotAnalyzed]).Ascending().UnmappedType(Nest.FieldType.None));
            }
            else if (searchParameters.IsResultsGrid && searchParameters.ResultsGridSortField != null)
            {
                if (searchParameters.ResultsGridSortField == "LastTouchedThrough")  //Property name in Viewmodel(ContactListEntry) and Elastic are mis-matching
                    searchParameters.ResultsGridSortField = "LastContactedThrough";
                string sortField = searchParameters.ResultsGridSortField;
                if (searchParameters.SortDirection == System.ComponentModel.ListSortDirection.Ascending)
                    descriptor = descriptor.Sort(s => s.OnField(sortField.ToCamelCase()).Ascending().UnmappedType(Nest.FieldType.None));
                else
                    descriptor = descriptor.Sort(s => s.OnField(sortField.ToCamelCase()).Descending().UnmappedType(Nest.FieldType.None));
            }
            else
                descriptor = descriptor.Sort(s => s.OnField(searchFields[ContactFields.LastUpdateOn]).Descending().UnmappedType(Nest.FieldType.None));

            if (searchParameters.Fields.IsAny())
            {
                var fields = searchFields.Where(s => searchParameters.Fields.Contains(s.Key)).Select(s => s.Value).ToArray();
                descriptor = descriptor.Fields(fields);
            }

            List<ContactFields> nonSearchableFields = getNonSearchableFields();
            IEnumerable<string> searchableFields = searchFields
                        .Where(f => !nonSearchableFields.Contains(f.Key))
                    .Select(f => f.Value).ToList();

            descriptor = descriptor.Query(query => query.FuzzyLikeThis(flt => flt
            .LikeText(q)
            .MinimumSimilarity(0.5)
            .IgnoreTermFrequency(true)
            .OnFields(searchableFields)
            .Boost(1.0)));


            //if (!string.IsNullOrEmpty(q))
            //    descriptor = descriptor.Query(query => query.QueryString(qs => qs.Query(q)));

            //descriptor = descriptor.Query(query => query.Dismax(dm => dm
            //    .Queries(qr => qr.FuzzyLikeThis(flt => flt
            //        .LikeText(q)
            //        .IgnoreTermFrequency(true)
            //        .OnFields(getSearchableContactFields())
            //        .Boost(1.5)))));

            // descriptor = descriptor.Explain();

            if (filter != null)
            {
                Expression<Func<T, bool>> expression = filter;
                parseExpression(expression.Body);
                FilterContainer baseFilter = base.stack.Pop();
                descriptor = descriptor.Filter(baseFilter);
            }


            if (searchParameters != null && searchParameters.Ids.IsAny())
            {
                if (!string.IsNullOrEmpty(q))
                {
                    descriptor = descriptor.Filter(ds => ds.Terms("id", searchParameters.Ids, TermsExecution.Plain));
                }
                else
                {
                    //IEnumerable<string> ids = searchParameters.Ids.Select(i => i.ToString()).ToList();

                    //descriptor = descriptor.Query(query => query
                    //    .FunctionScore(fs => fs
                    //    .BoostMode(FunctionBoostMode.replace)
                    //    .Query(qu => qu.Ids(ids))
                    //    .ScriptScore(sc=>sc
                    //        .Params(p=>p.Add("ids", ids))
                    //        .Script("count = ids.size(); id = org.elasticsearch.index.mapper.Uid.idFromUid(doc['_uid'].value);  for (i = 0; i < count; i++) { if (id == ids[i]) { return count - i; }}"))));

                    descriptor = descriptor.Query(query => query.Ids(searchParameters.Ids.Select(i => i.ToString()).ToList()));
                }

            }

            Logger.Current.Informational("BulkOperation for fectching Bulk Contacts");

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            result = this.ElasticClient().Search<T>(body => descriptor);
            stopwatch.Stop();
            var elapsed_time = stopwatch.Elapsed;
            Logger.Current.Informational("Time Taken to complete Query for Getting Contacts Results :" + elapsed_time);
            string que = result.ConnectionStatus.ToString();
            Logger.Current.Verbose(que);

            if (searchParameters.Fields != null && searchParameters.Fields.Count() == 2 && searchParameters.Fields.Contains(ContactFields.ContactId) && searchParameters.Fields.Contains(ContactFields.IsActive))
            {
                int id = 0;
                IList<Contact> results = new List<Contact>();
                foreach (var hit in result.Hits)
                {
                    if (hit.Type.Equals(typeof(Company)))
                    {
                        int.TryParse(hit.Id, out id);
                        results.Add(new Company() { Id = id });  //new Company() { Id = id }
                    }
                    else
                    {
                        int.TryParse(hit.Id, out id);
                        results.Add(new Person() { Id = id });
                    }
                }
                searchResult.Results = results.Select(c => c as T);
            }
            else
                searchResult.Results = result.Documents.Select(c => c as T);

            searchResult.TotalHits = result.Total;
            return searchResult;
        }

        private SearchResult<T> searchCampaigns<D>(string q, SearchParameters searchParameters)
        {
            SearchResult<T> searchResult = new SearchResult<T>();
            ISearchResponse<T> result;
            SearchDescriptor<T> descriptor = new SearchDescriptor<T>();
            descriptor = descriptor.Types(searchParameters.Types).Size(int.MaxValue);

            result = this.ElasticClient().Search<T>(body => descriptor);
            string que = result.ConnectionStatus.ToString();
            Logger.Current.Verbose(que);

            searchResult.Results = result.Documents;
            searchResult.TotalHits = result.Total;

            return searchResult;
        }

        private SearchResult<T> isDuplicatePerson(Person person, SearchParameters searchParameters)
        {
            SearchDescriptor<Person> descriptor = new SearchDescriptor<Person>();

            var primaryEmail = person.Emails != null ? person.Emails.FirstOrDefault(e => e.IsPrimary) : null;
            string firstName = (person.FirstName ?? string.Empty).ToLower();
            string lastName = (person.LastName ?? string.Empty).ToLower();
            string company = (person.CompanyName ?? string.Empty).ToLower();
            string emailId = (primaryEmail != null ? primaryEmail.EmailId ?? string.Empty : string.Empty).ToLower();

            string firstNameESName = searchFields[ContactFields.FirstName_NotAnalyzed];
            string lastNameESName = searchFields[ContactFields.LastName_NotAnalyzed];
            string companyNameESName = searchFields[ContactFields.CompanyName_NotAnalyzed];

            FilterDescriptor<Person> baseFilter = new FilterDescriptor<Person>();
            var baseFilters = new List<FilterContainer>();

            if (!string.IsNullOrEmpty(emailId))
            {
                if (person.Id > 0)
                    baseFilters.Add(baseFilter
                     .And(
                        a => a.And(t => t.Term("emails.isPrimary", true), t => t.Term("emails.emailId", emailId)),
                        a => a.Not(n => n.Term(t => t.Id, person.Id))));
                else
                    baseFilters.Add(baseFilter
                     .And(
                         a => a.And(t => t.Term("emails.isPrimary", true), t => t.Term("emails.emailId", emailId))));
            }
            else if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName)
                && string.IsNullOrEmpty(company) && string.IsNullOrEmpty(emailId))
            {
                if (person.Id > 0)
                    baseFilters.Add(baseFilter
                        .And(
                            a => a.Term(firstNameESName, firstName),
                            a => a.Term(lastNameESName, lastName),
                            a => a.Missing(t => t.Emails),
                            a => a.Missing(t => t.CompanyName),
                            a => a.Not(n => n.Term(t => t.Id, person.Id))));
                else
                    baseFilters.Add(baseFilter
                        .And(
                            a => a.Term(firstNameESName, firstName),
                            a => a.Term(lastNameESName, lastName),
                            a => a.Missing(t => t.Emails),
                            a => a.Missing(t => t.CompanyName)));
            }

            else if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName)
                && !string.IsNullOrEmpty(company) && string.IsNullOrEmpty(emailId))
            {
                if (person.Id > 0)
                    baseFilters.Add(baseFilter
                         .And(
                            a => a.Term(firstNameESName, firstName),
                            a => a.Term(lastNameESName, lastName),
                            a => a.Term(companyNameESName, company),
                            a => a.Missing(t => t.Emails),
                            a => a.Not(n => n.Term(t => t.Id, person.Id))));
                else
                    baseFilters.Add(baseFilter
                        .And(
                            a => a.Term(firstNameESName, firstName),
                            a => a.Term(lastNameESName, lastName),
                            a => a.Term(companyNameESName, company),
                            a => a.Missing(t => t.Emails)));
            }

            descriptor = descriptor.Filter(f => f.Or(baseFilters.ToArray()));

            SearchResult<T> searchResult = new SearchResult<T>();
            var result = this.ElasticClient().Search<Person>(body => descriptor);

            string que = result.ConnectionStatus.ToString();
            Logger.Current.Verbose(que);

            searchResult.Results = result.Documents.Select(c => c as T);
            searchResult.TotalHits = result.Hits.Count();

            return searchResult;
        }

        private IDictionary<ContactFields, string> getSearchFields()
        {
            IDictionary<ContactFields, string> fields = new Dictionary<ContactFields, string>();
            fields.Add(ContactFields.FirstNameField, "firstName");
            fields.Add(ContactFields.LastNameField, "lastName");
            fields.Add(ContactFields.CompanyNameField, "companyName");
            fields.Add(ContactFields.PrimaryEmail, "emails.emailId");
            fields.Add(ContactFields.WorkPhoneField, "phones.number");
            fields.Add(ContactFields.AddressLine1Field, "addresses.addressLine1");
            fields.Add(ContactFields.AddressLine2Field, "addresses.addressLine2");
            fields.Add(ContactFields.CityField, "addresses.city");
            fields.Add(ContactFields.StateField, "addresses.state.name");
            fields.Add(ContactFields.StateCodeField, "addresses.state.code");
            fields.Add(ContactFields.CountryField, "addresses.country.name");
            fields.Add(ContactFields.CountryCode, "addresses.country.code");
            fields.Add(ContactFields.ZipCodeField, "addresses.zipCode");
            fields.Add(ContactFields.BlogUrl, "blogLink.Raw");
            fields.Add(ContactFields.FacebookUrl, "facebookLink.Raw");
            fields.Add(ContactFields.GooglePlusUrl, "googlePlusLink.Raw");
            fields.Add(ContactFields.LinkedInUrl, "linkedInLink.Raw");
            fields.Add(ContactFields.TwitterUrl, "twitterLink.Raw");
            fields.Add(ContactFields.WebsiteUrl, "websiteLink.Raw");
            fields.Add(ContactFields.TitleField, "titleFull");
            fields.Add(ContactFields.SecondaryEmail, "secondaryEmails.emailId");
            fields.Add(ContactFields.FirstName_NotAnalyzed, "firstNameFull");
            fields.Add(ContactFields.LastName_NotAnalyzed, "lastNameFull");
            fields.Add(ContactFields.CompanyName_NotAnalyzed, "companyNameFull");
            fields.Add(ContactFields.LastUpdateOn, "lastUpdatedOn");
            fields.Add(ContactFields.LeadScore, "leadScore");
            fields.Add(ContactFields.ContactId, "id");
            fields.Add(ContactFields.FirstSourceType, "firstContactSource");
            fields.Add(ContactFields.WebPage, "webVisits.pageVisited");
            fields.Add(ContactFields.WebPageDuration, "webVisits.duration");
            fields.Add(ContactFields.ContactTag, "tags.id");
            fields.Add(ContactFields.FormName, "formSubmissions.formId");
            fields.Add(ContactFields.FormsubmittedOn, "formSubmissions.submittedOn");

            fields.Add(ContactFields.CustomFields, "customFields");
            fields.Add(ContactFields.NoteSummary, "noteSummary");
            fields.Add(ContactFields.LastNoteDate, "lastNoteDate");
            fields.Add(ContactFields.LastNote, "lastNote");
            fields.Add(ContactFields.IncludeInReports, "includeInReports");
            fields.Add(ContactFields.EmailStatus, "emails.emailStatusValue");
            fields.Add(ContactFields.ContactEmailID, "emails.contactEmailID");
            fields.Add(ContactFields.NoteCategory, "contactNotes.noteCategory");
            fields.Add(ContactFields.LastNoteCategory, "lastNoteCategory");
            fields.Add(ContactFields.IsActive, "isActive");
            return fields;
        }

        private List<ContactFields> getNonSearchableFields()
        {
            // These fields are to be excluded in query filtering because 'more_like_this'(Fuzzy Query) doesn't support binary/numeric fields

            List<ContactFields> nonSearchableFields = new List<ContactFields>();
            nonSearchableFields.Add(ContactFields.CompanyId);
            nonSearchableFields.Add(ContactFields.PrimaryEmailStatus);
            nonSearchableFields.Add(ContactFields.LastTouched);
            nonSearchableFields.Add(ContactFields.LastTouchedThrough);
            nonSearchableFields.Add(ContactFields.CreatedOn);
            nonSearchableFields.Add(ContactFields.CreatedBy);
            nonSearchableFields.Add(ContactFields.Owner);
            nonSearchableFields.Add(ContactFields.LeadSource);
            nonSearchableFields.Add(ContactFields.PartnerTypeField);
            nonSearchableFields.Add(ContactFields.LifecycleStageField);
            nonSearchableFields.Add(ContactFields.LastUpdateOn);
            nonSearchableFields.Add(ContactFields.FirstName_NotAnalyzed);
            nonSearchableFields.Add(ContactFields.LastName_NotAnalyzed);
            nonSearchableFields.Add(ContactFields.CompanyName_NotAnalyzed);
            nonSearchableFields.Add(ContactFields.LeadScore);
            nonSearchableFields.Add(ContactFields.ContactId);
            nonSearchableFields.Add(ContactFields.LeadAdapter);
            nonSearchableFields.Add(ContactFields.FirstSourceType);
            nonSearchableFields.Add(ContactFields.WebPage);
            nonSearchableFields.Add(ContactFields.WebPageDuration);
            nonSearchableFields.Add(ContactFields.ContactTag);
            nonSearchableFields.Add(ContactFields.FormName);
            nonSearchableFields.Add(ContactFields.FormsubmittedOn);
            nonSearchableFields.Add(ContactFields.FirstLeadSourceDate);
            nonSearchableFields.Add(ContactFields.FirstLeadSource);
            nonSearchableFields.Add(ContactFields.LeadSourceDate);
            nonSearchableFields.Add(ContactFields.NoteSummary);
            nonSearchableFields.Add(ContactFields.LastNoteDate);
            nonSearchableFields.Add(ContactFields.EmailStatus);
            nonSearchableFields.Add(ContactFields.TourCreator);
            nonSearchableFields.Add(ContactFields.TourDate);
            nonSearchableFields.Add(ContactFields.TourType);
            nonSearchableFields.Add(ContactFields.Community);
            nonSearchableFields.Add(ContactFields.TourAssignedUsers);
            nonSearchableFields.Add(ContactFields.ActionAssignedTo);
            nonSearchableFields.Add(ContactFields.ActionCreatedDate);
            nonSearchableFields.Add(ContactFields.ActionDate);
            nonSearchableFields.Add(ContactFields.ActionStatus);
            nonSearchableFields.Add(ContactFields.ActionType);
            nonSearchableFields.Add(ContactFields.ContactEmailID);
            nonSearchableFields.Add(ContactFields.LastNote);
            nonSearchableFields.Add(ContactFields.IncludeInReports);
            nonSearchableFields.Add(ContactFields.NoteCategory);
            nonSearchableFields.Add(ContactFields.LastNoteCategory);
            nonSearchableFields.Add(ContactFields.IsActive);

            return nonSearchableFields;
        }

        private SearchResult<T> isDuplicateCompany(Company company, SearchParameters searchParameters)
        {
            SearchDescriptor<Company> descriptor = new SearchDescriptor<Company>();

            var primaryEmail = company.Emails != null ? company.Emails.SingleOrDefault(c => c.IsPrimary) : null;
            string companyName = (company.CompanyName ?? string.Empty).ToLower();
            string emailId = (primaryEmail != null ? primaryEmail.EmailId ?? string.Empty : string.Empty).ToLower();

            string companyNameESName = "companyNameFull";
            FilterDescriptor<Company> baseFilter = new FilterDescriptor<Company>();

            IList<FilterContainer> baseFilters = new List<FilterContainer>();


            if (!string.IsNullOrEmpty(companyName) && string.IsNullOrEmpty(emailId))
            {
                if (company.Id > 0)
                    baseFilters.Add(baseFilter
                        .And(
                            a => a.Term(companyNameESName, companyName),
                            a => a.Not(n => n.Term(t => t.Id, company.Id))));
                else
                    baseFilters.Add(baseFilter
                        .And(
                            a => a.Term(companyNameESName, companyName)));
            }
            else if (!string.IsNullOrEmpty(companyName) && !string.IsNullOrEmpty(emailId))
            {
                if (company.Id > 0)
                {
                    baseFilters.Add(baseFilter
                    .And(
                        o => o.Not(n => n.Term(t => t.Id, company.Id)),
                        o => o.Or(
                                a => a.Term(companyNameESName, companyName),
                                a => a.And(t => t.Term("emails.isPrimary", true), t => t.Term("emails.emailId", emailId)))));
                }
                else
                {
                    baseFilters.Add(baseFilter
                    .Or(
                        a => a.Term(companyNameESName, companyName),
                        a => a.And(t => t.Term("emails.isPrimary", true), t => t.Term("emails.emailId", emailId))));
                }
            }

            descriptor = descriptor.Filter(f => f.Or(baseFilters.ToArray()));

            SearchResult<T> searchResult = new SearchResult<T>();

            var result = this.ElasticClient().Search<Company>(body => descriptor);

            string que = result.ConnectionStatus.ToString();
            Logger.Current.Verbose(que);

            searchResult.Results = result.Documents.Select(c => c as T);
            searchResult.TotalHits = result.Hits.Count();

            return searchResult;
        }

        public bool SaveQuery(SearchDefinition searchDefinition, SearchParameters parameters)
        {
            return registerPercolator(searchDefinition, parameters);
        }

        public int SaveQueries(IEnumerable<SearchDefinition> searchDefinitions, SearchParameters parameters, int accountId)
        {
            List<bool> savedQueries = new List<bool>();
            string indexName = "contacts" + accountId;
            string uri = ConfigurationManager.AppSettings["ELASTICSEARCH_INSTANCE"];
            var searchBoxUri = new Uri(uri);
            var settings = new ConnectionSettings(searchBoxUri);
            settings.SetDefaultIndex(indexName);
            settings.PluralizeTypeNames();

            ElasticClient client = new ElasticClient(settings);
            foreach (var search in searchDefinitions)
            {
                try
                {
                    bool ack = registerPercolatorQueries(search, parameters, client);
                    savedQueries.Add(ack);
                    if (!ack)
                        Console.WriteLine("Failed to index the saved-search : " + search.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occured while indexing a saved-search : " + search.Id);
                    Logger.Current.Informational("An error occured with SearchDefinitionId : " + search.Id);
                    Logger.Current.Error("An error occured while indexing a saved-search query : ", ex);
                    continue;
                }
            }
            return savedQueries.Where(w => w.Equals(true)).Count();
        }

        public bool RemoveQuery(short searchDefinitionId)
        {
            var client = this.ElasticClient();
            var unRegisterResponse = client.UnregisterPercolator<T>(searchDefinitionId.ToString());
            return unRegisterResponse.Found;
        }

        private bool registerPercolator(SearchDefinition searchDefinition, SearchParameters searchParameters)
        {
            var client = this.ElasticClient();
            IRegisterPercolateResponse response = null;
            var baseFilters = new List<FilterContainer>();

            setSearchFilters(searchDefinition);

            if (searchDefinition.PredicateType == Entities.SearchPredicateType.And)
            {
                foreach (SearchFilter filter in searchFilters)
                    baseFilters.Add(getBaseFilter(filter));

                if (searchParameters.IsPrivateSearch)
                {
                    FilterContainer privateFilter = filterDesc.Term("ownerId", searchParameters.DocumentOwnerId);
                    FilterContainer andFilter = filterDesc.And(baseFilters.ToArray());
                    response = client.RegisterPercolator<T>(searchDefinition.Id.ToString(),
                        r => r.Query(q => q.Filtered(f => f.Filter(fl => fl.And(privateFilter, andFilter)))));

                }
                else
                    response = client.RegisterPercolator<T>(searchDefinition.Id.ToString(),
                         r => r.Query(q => q.Filtered(f => f.Filter(fl => fl.And(baseFilters.ToArray())))));

                var url = response.ConnectionStatus.ToString();
                Logger.Current.Informational(url);
            }
            else if (searchDefinition.PredicateType == Entities.SearchPredicateType.Or)
            {
                foreach (SearchFilter filter in searchFilters)
                    baseFilters.Add(getBaseFilter(filter));

                if (searchParameters.IsPrivateSearch)
                {
                    FilterContainer privateFilter = filterDesc.Term("ownerId", searchParameters.DocumentOwnerId);
                    FilterContainer orFilter = filterDesc.Or(baseFilters.ToArray());
                    response = client.RegisterPercolator<T>(searchDefinition.Id.ToString(),
                        r => r.Query(q => q.Filtered(f => f.Filter(fl => fl.And(privateFilter, orFilter)))));
                }
                else
                    response = client.RegisterPercolator<T>(searchDefinition.Id.ToString(),
                        r => r.Query(q => q.Filtered(f => f.Filter(fl => fl.Or(baseFilters.ToArray())))));

                var url = response.ConnectionStatus.ToString();
                Logger.Current.Informational(url);
            }
            else
            {
                traverseTree(searchDefinition.CustomLogicalTree);
                FilterContainer filter = stack.Pop();

                if (searchParameters.IsPrivateSearch)
                {
                    FilterContainer privateFilter = filterDesc.Term("ownerId", searchParameters.DocumentOwnerId);
                    response = client.RegisterPercolator<T>(searchDefinition.Id.ToString(),
                        r => r.Query(q => q.Filtered(f => f.Filter(fl => fl.And(privateFilter, filter)))));
                }
                else
                    response = client.RegisterPercolator<T>(searchDefinition.Id.ToString(),
                        r => r.Query(q => q.Filtered(f => f.Filter(fl => filter))));

                var url = response.ConnectionStatus.ToString();
                Logger.Current.Informational(url);
            }
            if (response != null)
                return response.Created;
            else
                return false;
        }

        private bool registerPercolatorQueries(SearchDefinition searchDefinition, SearchParameters searchParameters, ElasticClient elasticClient)
        {
            var client = elasticClient;
            IRegisterPercolateResponse response = null;
            var baseFilters = new List<FilterContainer>();

            setSearchFilters(searchDefinition);

            if (searchDefinition.PredicateType == Entities.SearchPredicateType.And)
            {
                foreach (SearchFilter filter in searchFilters)
                    baseFilters.Add(getBaseFilter(filter));

                if (searchParameters.IsPrivateSearch)
                {
                    FilterContainer privateFilter = filterDesc.Term("ownerId", searchParameters.DocumentOwnerId);
                    FilterContainer andFilter = filterDesc.And(baseFilters.ToArray());
                    response = client.RegisterPercolator<T>(searchDefinition.Id.ToString(),
                        r => r.Query(q => q.Filtered(f => f.Filter(fl => fl.And(privateFilter, andFilter)))));

                }
                else
                    response = client.RegisterPercolator<T>(searchDefinition.Id.ToString(),
                         r => r.Query(q => q.Filtered(f => f.Filter(fl => fl.And(baseFilters.ToArray())))));

                var url = response.ConnectionStatus.ToString();
                Logger.Current.Informational(url);
            }
            else if (searchDefinition.PredicateType == Entities.SearchPredicateType.Or)
            {
                foreach (SearchFilter filter in searchFilters)
                    baseFilters.Add(getBaseFilter(filter));

                if (searchParameters.IsPrivateSearch)
                {
                    FilterContainer privateFilter = filterDesc.Term("ownerId", searchParameters.DocumentOwnerId);
                    FilterContainer orFilter = filterDesc.Or(baseFilters.ToArray());
                    response = client.RegisterPercolator<T>(searchDefinition.Id.ToString(),
                        r => r.Query(q => q.Filtered(f => f.Filter(fl => fl.And(privateFilter, orFilter)))));
                }
                else
                    response = client.RegisterPercolator<T>(searchDefinition.Id.ToString(),
                        r => r.Query(q => q.Filtered(f => f.Filter(fl => fl.Or(baseFilters.ToArray())))));

                var url = response.ConnectionStatus.ToString();
                Logger.Current.Informational(url);
            }
            else
            {
                traverseTree(searchDefinition.CustomLogicalTree);
                FilterContainer filter = stack.Pop();

                if (searchParameters.IsPrivateSearch)
                {
                    FilterContainer privateFilter = filterDesc.Term("ownerId", searchParameters.DocumentOwnerId);
                    response = client.RegisterPercolator<T>(searchDefinition.Id.ToString(),
                        r => r.Query(q => q.Filtered(f => f.Filter(fl => fl.And(privateFilter, filter)))));
                }
                else
                    response = client.RegisterPercolator<T>(searchDefinition.Id.ToString(),
                        r => r.Query(q => q.Filtered(f => f.Filter(fl => filter))));

                var url = response.ConnectionStatus.ToString();
                Logger.Current.Informational(url);
            }
            if (response != null)
            {
                if (!response.Created)
                {
                    Logger.Current.Informational("unable to reindex the saved-search : " + searchDefinition.Id);
                    Logger.Current.Informational(response.ConnectionStatus.ToString());
                }
                return response.Created;
            }
            else
                return false;
        }

        public IEnumerable<QueryMatch> FindMatchingQueries(IEnumerable<T> documents)
        {
            IList<QueryMatch> matches = new List<QueryMatch>();
            try
            {
                var client = this.ElasticClient();
                foreach (var document in documents)
                {
                    var contact = document as Contact;
                    try
                    {
                        var result = client.Percolate<T>(p => p.Document(document));
                        if (result != null)
                        {
                            foreach (var percolateMatch in result.Matches)
                            {
                                int id = 0;
                                int.TryParse(percolateMatch.Id, out id);
                                matches.Add(new QueryMatch() { DocumentId = contact.Id, SearchDefinitionId = id });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.Data.Clear();
                        ex.Data.Add("Document", contact.Id);
                        Logger.Current.Error("An error occured while finding matching queries for given document", ex);
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while finding matching queries", ex);
            }
            return matches;
        }

        public IDictionary<DateTime, long> GetContactsAggregationByDate(SearchDefinition searchDefinition, SearchParameters parameters)
        {
            SearchDescriptor<T> descriptor = new SearchDescriptor<T>();
            var types = parameters != null && parameters.Types.IsAny() ? parameters.Types : new List<Type>() { typeof(Person), typeof(Company), typeof(Contact) };
            descriptor = descriptor.Size(0)
                            .Types(types)
                            .Aggregations(fa => fa.Filter("fa",
                                                        fd => fd.Filter(fc => fc.And(GetFilterAggregator(searchDefinition).ToArray()))
                                                                .Aggregations(a => a.DateHistogram("da",
                                                                                    d => d.Field(searchFields[ContactFields.CreatedOn])
                                                                                    .Interval(Nest.DateInterval.Day)
                                                                                    .MinimumDocumentCount(0)))));
            var result = this.ElasticClient().Search<T>(body => descriptor);

            string que = result.ConnectionStatus.ToString();
            Logger.Current.Verbose(que);

            LogErrorMessage(result);
            try
            {
                return result.Aggs.Filter("fa").DateHistogram("da").Items.Select(a => new { a.Date, a.DocCount }).ToDictionary(k => k.Date, v => v.DocCount);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error while getting contacts aggregaton by date : ", ex);
                return default(IDictionary<DateTime, long>);
            }

        }

        private static void LogErrorMessage(ISearchResponse<T> result)
        {
            try
            {
                if (result.ConnectionStatus.OriginalException != null && !string.IsNullOrEmpty(result.ConnectionStatus.OriginalException.Message))
                    Logger.Current.Error(result.ConnectionStatus.OriginalException.Message);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error while getting aggregations", ex);
            }
        }

        private IEnumerable<FilterContainer> GetFilterAggregator(SearchDefinition searchDefinition)
        {
            setSearchFilters(searchDefinition);
            var andFilters = new List<FilterContainer>();
            var orFilters = new List<FilterContainer>();
            var filterDescriptor = new FilterDescriptor<T>();

            foreach (var filter in searchDefinition.Filters.Where(sf => sf.Field != ContactFields.Owner))
                andFilters.Add(getBaseFilter(filter));

            foreach (var filter in searchDefinition.Filters.Where(sf => sf.Field == ContactFields.Owner))
                orFilters.Add(getBaseFilter(filter));
            if (orFilters.Count > 0)
            {
                FilterContainer orFC = filterDescriptor.Or(orFilters.ToArray());
                andFilters.Add(orFC);
            }

            //foreach (var filter in searchDefinition.Filters.Where(sf => sf.IsDateTime))
            //    andFilters.Add(getBaseFilter(filter));

            //foreach (var filter in searchDefinition.Filters.Where(sf => !sf.IsDateTime))
            //    orFilters.Add(getBaseFilter(filter));
            //if (orFilters.Count > 0)
            //{
            //    FilterContainer orFC = filterDescriptor.Or(orFilters.ToArray());
            //    andFilters.Add(orFC);
            //}

            return andFilters;
        }

        public IDictionary<int, long> GetTopLeadSources(SearchDefinition searchDefinition, SearchParameters parameters)
        {
            SearchDescriptor<T> descriptor = new SearchDescriptor<T>();
            var types = parameters != null && parameters.Types.IsAny() ? parameters.Types : new List<Type>() { typeof(Person), typeof(Company), typeof(Contact) };

            descriptor = descriptor.Size(0)
                            .Types(types)
                            .Aggregations(fa => fa.Filter("fa", fd => fd.Filter(fc => fc.And(GetFilterAggregator(searchDefinition).ToArray()))
                                                                       .Aggregations(a => a.Nested("nested", n => n.Path("leadSources")).Terms("ta", ta => ta.Field(searchFields[ContactFields.LeadSource])))));

            var result = this.ElasticClient().Search<T>(body => descriptor);

            string que = result.ConnectionStatus.ToString();
            Logger.Current.Verbose(que);

            LogErrorMessage(result);
            try
            {
                return result.Aggs.Filter("fa").Terms("ta").Items.Select(a => new { a.Key, a.DocCount }).ToDictionary(k => Convert.ToInt32(k.Key), v => v.DocCount);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error while getting top leads sources : ", ex);
                return default(IDictionary<int, long>);
            }

        }

        public SavedSearchActiveContacts GetAggregationBySavedSearch(SearchDefinition searchDefinition, SearchParameters parameters)
        {
            SearchDescriptor<T> descriptor = new SearchDescriptor<T>();
            var types = parameters != null && parameters.Types.IsAny() ? parameters.Types : new List<Type>() { typeof(Person), typeof(Contact) };
            SavedSearchActiveContacts ssc = new SavedSearchActiveContacts();

            descriptor = descriptor.Size(0)
                            .Types(types)
                            .Aggregations(fa => fa.Filter("fa", fd => fd.Filter(fc => fc.And(GetFilterAggregator(searchDefinition).ToArray()))
                                                                       .Aggregations(a => a.Terms("ta", ta => ta.Field(searchFields[ContactFields.IsActive])))));

            var result = this.ElasticClient().Search<T>(body => descriptor);

            string que = result.ConnectionStatus.ToString();
            Logger.Current.Verbose(que);

            LogErrorMessage(result);
            try
            {
                long count = result.Aggs.Filter("fa").DocCount;
                var dictionary = result.Aggs.Filter("fa").Terms("ta").Items.Select(a => new { a.Key, a.DocCount }).ToDictionary(k => k.Key, v => v.DocCount);
                ssc.SearchDefinitionId = searchDefinition.Id;
                ssc.TotalCount = count;
                ssc.ActiveContactsCount = dictionary.Where(w => w.Key == "T").Select(s => s.Value).FirstOrDefault();
                ssc.NonActiveContactsCount = dictionary.Where(w => w.Key == "F").Select(s => s.Value).FirstOrDefault();

                return ssc;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error while getting top leads sources : ", ex);
                return new SavedSearchActiveContacts();
            }
        }

        private FilterContainer getActiveContactsFilters(FilterDescriptor<Contact> filterDesc, bool isPrivateSearch, bool isActiveSearch, int ownerID)
        {
            FilterContainer query = new FilterContainer();
            if (isPrivateSearch && isActiveSearch)
                query = filterDesc.And(a => a.Or(o => o.Terms("emails.emailStatusValue", new List<string>() { "50", "51", "52" })), a1 => a1.And(an => an.Term("doNotEmail", 0), a2 => a2.Exists("emails.emailId")), and => and.Term("ownerId", ownerID));
            else if (isPrivateSearch)
                query = filterDesc.And(a => a.Term("ownerId", ownerID));
            else if (isActiveSearch)
                query = filterDesc.And(a => a.Or(o => o.Terms("emails.emailStatusValue", new List<string>() { "50", "51", "52" })), a1 => a1.And(an => an.Term("doNotEmail", 0), a2 => a2.Exists("emails.emailId")));
            return query;
        }

        ///DateTime
        ///
        public string TimeZone
        {
            get
            {
                string tz = ((ClaimsPrincipal)Thread.CurrentPrincipal).Claims.Where(c => c.Type == "TimeZone").Select(c => c.Value).FirstOrDefault();
                tz = string.IsNullOrEmpty(tz) ? "Central Standard Time" : tz;
                return tz;
            }
            set
            {
                TimeZoneInstance = null;
                _timeZone = value;
            }
        }
        private string _timeZone;

        public TimeZoneInfo TimeZoneInstance
        {
            get
            {
                if (_timeZoneInstance == null)
                {
                    try
                    {
                        _timeZoneInstance = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
                    }
                    catch
                    {
                        TimeZone = "Central Standard Time";
                        _timeZoneInstance = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
                    }
                }
                return _timeZoneInstance;
            }
            private set { _timeZoneInstance = value; }
        }

        private TimeZoneInfo _timeZoneInstance;

        public DateTime DateMath(DateTime start)
        {
            DateTime capturedTime = start;
            start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
            start = TimeZoneInfo.ConvertTimeFromUtc(start, this.TimeZoneInstance);
            var offset = this.TimeZoneInstance.GetUtcOffset(start).TotalHours;
            var offsetLocal = TimeZoneInfo.Local.GetUtcOffset(start).TotalHours;
            var startTime = start.AddHours(offsetLocal - offset);

            Logger.Current.Informational("TimeZone local : " + TimeZoneInfo.Local.ToString() + " UTC to UserTimeZone : " + start + " Captured time: " + capturedTime + " -> UTC:  " + start.ToUniversalTime() + "  " +
           "Adjusted time: " + startTime + " -> UTC: " + startTime.ToUniversalTime() + " offsetLocal : " + offsetLocal + " offset : " + offset);
            // this is the time you write to the db
            DateTime timeToSave = startTime.ToUniversalTime();

            return timeToSave;
        }

    }
}
