using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.Logging;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Domain.SuppressedEmails;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;


namespace SmartTouch.CRM.SearchEngine.Search
{
    internal class SearchBase<T> where T : class
    {
        int accountId;
        string uri;
        protected IDictionary<Type, string> indices = new Dictionary<Type, string>();

        public string IndexName { get; set; }

        public SearchBase(int accountId)
        {
            this.accountId = accountId;
            this.uri = ConfigurationManager.AppSettings["ELASTICSEARCH_INSTANCE"];

            indices.Add(typeof(Contact), "contacts" + this.accountId);
            indices.Add(typeof(Company), "contacts" + this.accountId);
            indices.Add(typeof(Person), "contacts" + this.accountId);
            indices.Add(typeof(Tag), "tags" + this.accountId);
            indices.Add(typeof(Opportunity), "opportunities");
            indices.Add(typeof(Campaign), "campaigns" + this.accountId);
            indices.Add(typeof(Form), "forms");
            indices.Add(typeof(SuppressedEmail), "suppressedemails" + this.accountId);
            indices.Add(typeof(SuppressedDomain), "suppresseddomains" + this.accountId);

            if (indices.ContainsKey(typeof(T)))
                this.IndexName = indices[typeof(T)];
        }

        protected ElasticClient ElasticClient()
        {
            try
            {
                var searchBoxUri = new Uri(uri);
                var settings = new ConnectionSettings(searchBoxUri);
                settings.SetDefaultIndex(this.IndexName);
                settings.PluralizeTypeNames();

                return new ElasticClient(settings);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public SearchResult<T> Search(string q, SearchParameters searchParameters)
        {
            return search<int>(q, null, searchParameters);
        }

        public SearchResult<T> Search(string q, Expression<Func<T, bool>> filter, SearchParameters searchParameters)
        {
            return search<int>(q, filter, searchParameters);
        }

        public virtual SearchResult<Suggestion> AutoCompleteField(string q, SearchParameters searchParameters)
        {
            if (string.IsNullOrEmpty(q))
                return new SearchResult<Suggestion>() { Results = new List<Suggestion>(), TotalHits = 0 };

            SearchDescriptor<T> descriptor = new SearchDescriptor<T>();
            descriptor = descriptor.Size(0).Types(searchParameters.Types).SuggestCompletion("suggest",
                    c => c.OnField(searchParameters.AutoCompleteFieldName).Text(q).Size(15));

            var result = this.ElasticClient().Search<T>(s => descriptor);

            string qe = result.ConnectionStatus.ToString();
            Logger.Current.Verbose(qe);

            return convertSuggestionsToList(result.Suggest);
        }

        protected SearchResult<Suggestion> convertSuggestionsToList(IDictionary<string, Suggest[]> suggestions)
        {
            IList<Suggestion> suggestResults = new List<Suggestion>();
            if (suggestions != null && suggestions.Count > 0)
            {
                Suggest[] suggests = suggestions["suggest"];
                foreach (Suggest suggest in suggests)
                {
                    Logger.Current.Informational("Auto Complete requested text : " + suggest.Text + " and options count : " + suggest.Options.Count());
                    foreach (SuggestOption option in suggest.Options)
                    {
                        var payload = JsonConvert.DeserializeObject<SuggesterPayload>(option.Payload.ToString());
                        Logger.Current.Informational("Option text : " + option.Text + " DocumentId : " + payload.DocumentId + " Type : " + payload.ContactType);
                        suggestResults.Add(new Suggestion()
                        {
                            DocumentId = payload.DocumentId,
                            Text = option.Text,
                            AccountId = payload.AccountId,
                            DocumentOwnedBy = payload.DocumentOwnedBy,
                            ContactType = payload.ContactType
                        });
                    }
                }
            }
            SearchResult<Suggestion> searchResult = new SearchResult<Suggestion>();
            searchResult.Results = suggestResults;

            return searchResult;
        }

        public SearchResult<Suggestion> QuickSearch(string q, SearchParameters parameters)
        {
            int limit = parameters.Limit == 0 ? 10 : parameters.Limit;
            int from = (parameters.PageNumber - 1) * limit;

            SearchDescriptor<object> descriptor = new SearchDescriptor<object>();
            descriptor = descriptor.From(from).Size(limit);

            IList<FilterContainer> filters = new List<FilterContainer>();

            FilterDescriptor<object> baseFilter = new FilterDescriptor<object>();
            filters.Add(baseFilter.Term("accountID", parameters.AccountId));
            if (parameters != null && parameters.IsPrivateSearch && parameters.PrivateModules.IsAny() &&
               (parameters.PrivateModules.Contains(Entities.AppModules.Campaigns) || parameters.PrivateModules.Contains(Entities.AppModules.Forms)))
            {
                filters.Add(baseFilter.Term("ownerId", parameters.DocumentOwnerId.ToString()));
                //if(parameters.PrivateModules.Count() > 0 &&
                //parameters.PrivateModules.Contains(Entities.AppModules.Campaigns) )
                //    filters.Add(baseFilter.Term("status", 109));
            }

            if (filters.Count > 1)
            {
                descriptor = descriptor.Filter(f => f.And(filters.ToArray()));
            }
            else
                descriptor = descriptor.Filter(f => f.Term("accountID", parameters.AccountId));
            //descriptor = descriptor.Filter(f => f.Term("accountID", parameters.AccountId));

            string[] selectedIndices = null;
            Type[] types = null;

            if (parameters != null && parameters.Types.IsAny())
            {
                selectedIndices = parameters.Types.Where(t => indices.ContainsKey(t)).Select(t => indices[t]).Distinct().ToArray();
                types = parameters.Types.Distinct().ToArray();
            }
            else
            {
                selectedIndices = indices.Select(c => c.Value).Distinct().ToArray();
                types = indices.Select(c => c.Key).Distinct().ToArray();
            }

            descriptor = descriptor.Indices(selectedIndices).IndicesBoost(b => b
                .Add("contacts" + this.accountId, 3.5).Add("campaigns" + this.accountId, 3.0).Add("opportunities", 2.5));

            descriptor = descriptor.Types(types);
            if (!string.IsNullOrEmpty(q))
            {
                string queryString = q + "*" + " " + q;
                if (parameters != null && parameters.PrivateModules.IsAny())
                {
                    string createdByTerm = string.Empty;
                    string createdByTermValue = string.Empty;
                    string ownerIdTerm = string.Empty;
                    string ownerIdTermValue = string.Empty;
                    if (parameters.PrivateModules.Contains(Entities.AppModules.Campaigns) || parameters.PrivateModules.Contains(Entities.AppModules.Forms))
                    {
                        createdByTerm = "createdBy";
                        createdByTermValue = parameters.DocumentOwnerId.ToString();
                    }


                    if (parameters.PrivateModules.Contains(Entities.AppModules.Contacts) || parameters.PrivateModules.Contains(Entities.AppModules.Opportunity))
                    {
                        ownerIdTerm = "ownerId";
                        ownerIdTermValue = parameters.DocumentOwnerId.ToString();
                    }

                    IList<string> contactIndices = new List<string>();
                    if (parameters.PrivateModules.Contains(Entities.AppModules.Contacts))
                        contactIndices.Add("contacts" + this.accountId);
                    if (parameters.PrivateModules.Contains(Entities.AppModules.Opportunity))
                        contactIndices.Add("opportunities");
                    if (parameters.PrivateModules.Contains(Entities.AppModules.Campaigns))
                        contactIndices.Add("campaigns" + this.accountId);
                    if (parameters.PrivateModules.Contains(Entities.AppModules.Forms))
                        contactIndices.Add("forms");

                    //descriptor = descriptor.Query(qr=>qr.Filtered(flt=>flt.Query(qs=>qs.in)))
                    // descriptor = descriptor.Query(qu => qu.Indices(i => i.Indices(contactIndices)
                    //.NoMatchQuery(nm => nm.Term(createdByTerm, createdByTermValue)).NoMatchQuery(nm => nm.Term(ownerIdTerm, ownerIdTermValue)).Query(qr => qr.QueryString(qs => qs.Query(q)))));//Term(ownerIdTerm, ownerIdTermValue)

                    //modified

                    descriptor = descriptor.Query(qu =>
                        qu.Indices(i => i.Indices(contactIndices).
                        NoMatchQuery(nm => nm.QueryString(qs => qs.Query(queryString))).
                        Query(que => que.Bool(b => b.Must(
                            s => s.Bool(bo => bo.Should(sh => sh.Term(createdByTerm, createdByTermValue), sh => sh.Term(ownerIdTerm, ownerIdTermValue))),
                            s => s.QueryString(qs => qs.Query(queryString).OnFieldsWithBoost(o => o.Add("firstName", 5).Add("lastName", 5).Add("emails.emailId", 5).Add("companyName", 3.5).Add("_all", 2))))))));

                }
                else
                    descriptor = descriptor.Query(query => query.QueryString(qs => qs.Query(queryString).OnFieldsWithBoost(o => o.Add("firstName", 5).Add("lastName", 5).Add("emails.emailId", 5).Add("companyName", 3.5).Add("_all", 2))));
            }
            /*
             * Don't show archived campaigns, search by account id, don't show unsubscribed email
             * */
            descriptor = descriptor.Filter(f => f.And(query => !query.Term("campaignStatus", CampaignStatus.Archive),
                query => query.Term("accountID", parameters.AccountId)));
            var result = this.ElasticClient().Search<object>(body => descriptor);

            string qe = result.ConnectionStatus.ToString();
            Logger.Current.Verbose(qe);

            var searchResult = convertToSuggestions(result.Documents, parameters);
            searchResult.TotalHits = result.Total;

            return searchResult;
        }

        public bool IsDocumentCreatedBy(int contactId, int? userId)
        {
            SearchDescriptor<T> descriptor = new SearchDescriptor<T>();
            IList<Type> types = new List<Type>() { typeof(T) };

            string[] selectedIndices = indices.Where(i => types.Contains(i.Key)).Select(i => i.Value).Distinct().ToArray();
            descriptor = descriptor.Types(types).Indices(selectedIndices);
            descriptor = descriptor.Filter(f => f.And(a => a.Term("id", contactId), a => a.Term("createdBy", userId)));

            var result = this.ElasticClient().Search<T>(body => descriptor);

            return result.Total > 0;
        }

        SearchResult<Suggestion> convertToSuggestions(IEnumerable<object> quickSearchResults, SearchParameters parameters)
        {
            SearchResult<Suggestion> searchResult = new SearchResult<Suggestion>();
            IList<Suggestion> suggestions = new List<Suggestion>();
            foreach (object result in quickSearchResults)
            {
                Suggestion suggestion = new Suggestion();
                Type type = result.GetType();
                if (type.Equals(typeof(Person)))
                {
                    Person person = result as Person;
                    int ownerId = person.OwnerId.HasValue ? person.OwnerId.Value : 0;
                    if (parameters.IsPrivateSearch && parameters.PrivateModules != null && parameters.PrivateModules.Contains(AppModules.Contacts) && parameters.DocumentOwnerId != ownerId)
                        continue;

                    suggestion.AccountId = person.AccountID;
                    suggestion.DocumentId = person.Id;
                    string text = (person.FirstName + " " + person.LastName).Trim();

                    if (!string.IsNullOrEmpty(person.CompanyName))
                        text += " (" + person.CompanyName + ")";

                    if (person != null && person.Emails.IsAny() && person.Emails.Any(p => p.IsPrimary))
                        text += "|" + person.Emails.Where(p => p.IsPrimary).First().EmailId;
                    else
                        text += "|";

                    text += "|" + person.LeadScore;

                    suggestion.Text = text;
                    suggestion.Entity = Entities.SearchableEntity.People;
                }
                else if (type.Equals(typeof(Company)))
                {
                    Company company = result as Company;
                    int ownerId = company.OwnerId.HasValue ? company.OwnerId.Value : 0;
                    if (parameters.IsPrivateSearch && parameters.PrivateModules != null && parameters.PrivateModules.Contains(AppModules.Contacts) && parameters.DocumentOwnerId != ownerId)
                        continue;

                    suggestion.AccountId = company.AccountID;
                    suggestion.DocumentId = company.Id;

                    string text = company.CompanyName;

                    if (company != null && company.Emails.IsAny() && company.Emails.Any(p => p.IsPrimary))
                        text += "|" + company.Emails.Where(p => p.IsPrimary).First().EmailId;

                    //text += "      " + company.LeadScore;

                    suggestion.Text = text;
                    suggestion.Entity = Entities.SearchableEntity.Companies;
                }
                else if (type.Equals(typeof(Tag)))
                {
                    Tag tag = result as Tag;
                    suggestion.AccountId = tag.AccountID;
                    suggestion.DocumentId = tag.Id;
                    suggestion.Text = tag.TagName;
                    suggestion.Entity = Entities.SearchableEntity.Tags;
                }
                else if (type.Equals(typeof(Campaign)))
                {
                    Campaign campaign = result as Campaign;
                    int ownerId = campaign.CreatedBy;
                    if (parameters.IsPrivateSearch && parameters.PrivateModules != null && parameters.PrivateModules.Contains(AppModules.Campaigns) && parameters.DocumentOwnerId != ownerId)
                        continue;

                    suggestion.AccountId = campaign.AccountID;
                    suggestion.DocumentId = campaign.Id;
                    suggestion.Text = campaign.Name;
                    suggestion.Entity = Entities.SearchableEntity.Campaigns;
                }
                else if (type.Equals(typeof(Opportunity)))
                {
                    Opportunity opportunity = result as Opportunity;
                    int ownerId = opportunity.CreatedBy;
                    if (parameters.IsPrivateSearch && parameters.PrivateModules != null && parameters.PrivateModules.Contains(AppModules.Opportunity) && parameters.DocumentOwnerId != ownerId)
                        continue;

                    suggestion.AccountId = opportunity.AccountID;
                    suggestion.DocumentId = opportunity.Id;
                    suggestion.Text = opportunity.OpportunityName;
                    suggestion.Entity = Entities.SearchableEntity.Opportunities;
                }
                else if (type.Equals(typeof(Form)))
                {
                    Form form = result as Form;
                    int ownerId = form.CreatedBy;
                    if (parameters.IsPrivateSearch && parameters.PrivateModules != null && parameters.PrivateModules.Contains(AppModules.Forms) && parameters.DocumentOwnerId != ownerId)
                        continue;

                    suggestion.AccountId = form.AccountID;
                    suggestion.DocumentId = form.Id;
                    suggestion.Text = form.Name;
                    suggestion.Entity = Entities.SearchableEntity.Forms;
                }
                suggestions.Add(suggestion);
            }
            searchResult.Results = suggestions;
            return searchResult;
        }

        SearchResult<T> search<D>(string q, Expression<Func<T, bool>> filter, SearchParameters searchParameters)
        {
            SearchResult<T> searchResult = new SearchResult<T>();
            int limit = searchParameters.Limit == 0 ? 10 : searchParameters.Limit;
            int from = (searchParameters.PageNumber - 1) * limit;
            ISearchResponse<T> result;
            //string[] types = searchParameters.Types.Select(t => t.FullName).ToArray();

            SearchDescriptor<T> descriptor = new SearchDescriptor<T>();
            descriptor = descriptor.Types(searchParameters.Types)
                            .From(from)
                            .Size(limit);
            descriptor = descriptor.Filter(f => f.Term("accountID", searchParameters.AccountId));

            if (typeof(T).Equals(typeof(Campaign)))
            {
                if (searchParameters.SortField == ContactSortFieldType.CampaignClickrate)
                    descriptor = descriptor.Sort(s => s.OnField("uniqueClicks").Descending().UnmappedType(Nest.FieldType.None));
                else if (!searchParameters.SortFields.IsAny())
                    descriptor = descriptor.Sort(s => s.OnField("lastUpdatedOn").Descending().UnmappedType(Nest.FieldType.None));
            }
            else if (typeof(T).Equals(typeof(Form)) && !searchParameters.SortFields.IsAny())
                descriptor = descriptor.Sort(s => s.OnField("lastModifiedOn").Descending().UnmappedType(Nest.FieldType.None));
            else if (typeof(T).Equals(typeof(Opportunity)) && !searchParameters.SortFields.IsAny())
                descriptor = descriptor.Sort(s => s.OnField("createdOn").Descending().UnmappedType(Nest.FieldType.None));

            if (searchParameters != null && searchParameters.SortFields.IsAny())
            {
                //todo multi-level sorting.
                string sortField = searchParameters.SortFields.First();
                if (searchParameters.SortDirection == System.ComponentModel.ListSortDirection.Ascending)
                    descriptor = descriptor.Sort(s => s.OnField(sortField.ToCamelCase()).Ascending().UnmappedType(Nest.FieldType.None));
                else
                    descriptor = descriptor.Sort(s => s.OnField(sortField.ToCamelCase()).Descending().UnmappedType(Nest.FieldType.None));
            }

            if (!string.IsNullOrEmpty(q))
                descriptor = descriptor.Query(query => query.QueryString(qs => qs.Query(q)));

            if (filter != null)
            {
                string createddate = typeof(T).Equals(typeof(Form)) ? "createdDate" : (typeof(T).Equals(typeof(Campaign)) ? "createdDate" : "createdOn");
                Expression<Func<T, bool>> expression = filter;
                parseExpression(expression.Body);
                FilterContainer baseFilter = stack.Pop();
                FilterDescriptor<T> filterD = new FilterDescriptor<T>();
                FilterContainer accountIDFilter = filterD.Term("accountID", searchParameters.AccountId);
                FilterContainer dateRangeContainer = new FilterContainer();
                if (searchParameters.StartDate.HasValue && searchParameters.EndDate.HasValue)
                {
                    FilterDescriptor<T> dateRangeFilter = new FilterDescriptor<T>();
                    dateRangeContainer = dateRangeFilter.Range(r =>
                        r.GreaterOrEquals(searchParameters.StartDate.Value.ToJSDate()).
                          LowerOrEquals(searchParameters.EndDate.Value.ToJSDate()).OnField(createddate));
                }
                descriptor = descriptor.Filter(f => f.And(baseFilter, accountIDFilter, dateRangeContainer));
            }

            if (searchParameters != null && searchParameters.Ids.IsAny())
            {
                if (!string.IsNullOrEmpty(q))
                    descriptor = descriptor.Filter(ds => ds.Terms("id", searchParameters.Ids, TermsExecution.Plain));
                else
                    descriptor = descriptor.Query(query => query.Ids(searchParameters.Ids.Select(i => i.ToString()).ToList()));
            }



            result = ElasticClient().Search<T>(body => descriptor);
            string que = result.ConnectionStatus.ToString();
            Logger.Current.Verbose(que);

            searchResult.Results = result.Documents;
            searchResult.TotalHits = result.Total;

            return searchResult;
        }

        SearchResult<T> searchCampaign<D>(Expression<Func<T, bool>> filter, SearchParameters searchParameters)
        {
            SearchResult<T> searchResult = new SearchResult<T>();
            ISearchResponse<T> result;
            int? limit = searchParameters.Limit;
            if (limit == 0)
                searchParameters.Limit = limit.Value;
            SearchDescriptor<T> descriptor = new SearchDescriptor<T>();
            descriptor = descriptor.Types(searchParameters.Types).Size(2000);

            result = ElasticClient().Search<T>(body => descriptor);
            string que = result.ConnectionStatus.ToString();
            Logger.Current.Verbose(que);

            searchResult.Results = result.Documents;
            searchResult.TotalHits = result.Total;
            return searchResult;
        }

        //Expression Tree is parsed only till fist level, recusion for n-layers need to be implemented.
        //protected BaseFilter parseExpression(Expression<Func<T, bool>> expression)
        //{
        //    BinaryExpression binaryExpression = expression.Body as BinaryExpression;
        //    MemberExpression left = (MemberExpression)binaryExpression.Left;
        //    string parameterName = left.Member.Name;

        //    ConstantExpression right = binaryExpression.Right as ConstantExpression;
        //    string parameterValue = right.Value as string;

        //    FilterDescriptor<T> filterD = new FilterDescriptor<T>();
        //    return filterD.Term(parameterName.ToCamelCase(), parameterValue);
        //}

        protected Stack<FilterContainer> stack = new Stack<FilterContainer>();


        protected void parseExpression(Expression expressionTree)
        {
            if (expressionTree == null)
                return;

            if (expressionTree is BinaryExpression && expressionTree.NodeType == ExpressionType.AndAlso
                && ((BinaryExpression)expressionTree).Left != null)
                parseExpression(((BinaryExpression)expressionTree).Left);

            if (expressionTree is BinaryExpression && expressionTree.NodeType == ExpressionType.AndAlso
                && ((BinaryExpression)expressionTree).Right != null)
                parseExpression(((BinaryExpression)expressionTree).Right);

            if (expressionTree.NodeType == ExpressionType.Equal || expressionTree.NodeType == ExpressionType.NotEqual)
            {
                BinaryExpression binaryExpression = expressionTree as BinaryExpression;

                string parameterName = "";
                if (binaryExpression.Left.GetType().Equals(typeof(UnaryExpression)))
                {
                    UnaryExpression unaryExpression = binaryExpression.Left as UnaryExpression;
                    MemberExpression left = unaryExpression.Operand as MemberExpression;
                    parameterName = left.Member.Name;
                }
                else
                {
                    MemberExpression left = binaryExpression.Left as MemberExpression;
                    parameterName = left.Member.Name;
                }

                string parameterValue = "";
                if (binaryExpression.Right.NodeType == ExpressionType.Convert)
                {
                    UnaryExpression right = binaryExpression.Right as UnaryExpression;
                    MemberExpression memberExpression = right.Operand as MemberExpression;
                    ConstantExpression constantExpression = memberExpression.Expression as ConstantExpression;
                    object container = constantExpression.Value;
                    var member = memberExpression.Member;
                    if (member is FieldInfo)
                    {
                        FieldInfo fieldInfo = member as FieldInfo;
                        if (fieldInfo.FieldType.IsEnum)
                        {
                            object value = ((FieldInfo)member).GetValue(container);
                            //var en = Enum.ToObject(fieldInfo.FieldType, value);
                            var enumValue = (short)value;
                            parameterValue = enumValue.ToString();

                        }
                        else
                        {
                            object value = ((FieldInfo)member).GetValue(container);
                            parameterValue = value.ToString();
                        }

                    }
                    if (member is PropertyInfo)
                    {
                        //object value = ((PropertyInfo)member).GetValue(container, null);
                    }

                }
                else if (binaryExpression.Right.NodeType != ExpressionType.Constant)
                {
                    MemberExpression memberExpression = binaryExpression.Right as MemberExpression;
                    // MemberExpression fieldExpression = memberExpression.Expression as MemberExpression;
                    ConstantExpression right = memberExpression.Expression as ConstantExpression;
                    //parameterValue = right.Value.ToString();

                    object container = right.Value;
                    var member = memberExpression.Member;
                    if (member is FieldInfo)
                    {
                        object value = ((FieldInfo)member).GetValue(container);
                        parameterValue = value.ToString();

                    }
                    if (member is PropertyInfo)
                    {
                        //object value = ((PropertyInfo)member).GetValue(container, null);
                    }
                }
                else
                {
                    ConstantExpression right = binaryExpression.Right as ConstantExpression;
                    parameterValue = right.Value.ToString();
                }
                if (expressionTree.NodeType == ExpressionType.Equal)
                {
                    FilterDescriptor<T> filterD = new FilterDescriptor<T>();
                    FilterContainer filter = filterD.Term(parameterName.ToCamelCase(), parameterValue);
                    stack.Push(filter);
                }
                else
                {
                    FilterDescriptor<T> filterD = new FilterDescriptor<T>();
                    FilterContainer filter = filterD.Not(n => n.Term(parameterName.ToCamelCase(), parameterValue));
                    stack.Push(filter);
                }

            }
            else if (expressionTree.NodeType == ExpressionType.Lambda)
            {
                LambdaExpression lambdaExpression = expressionTree as LambdaExpression;
                MethodCallExpression methodCallExpression = lambdaExpression.Body as MethodCallExpression;
                MemberExpression memberExpression = methodCallExpression.Object as MemberExpression;
                if (memberExpression.NodeType == ExpressionType.MemberAccess)
                    memberExpression = memberExpression.Expression as MemberExpression;

                ConstantExpression constantExpression = methodCallExpression.Arguments[0] as ConstantExpression;

                string parameterName = memberExpression.Member.Name;
                string parameterValue = constantExpression.Value.ToString();

                FilterDescriptor<T> filterD = new FilterDescriptor<T>();
                FilterContainer baseFilter = filterD.Term(parameterName.ToCamelCase(), parameterValue);

                stack.Push(baseFilter);
            }
            else if (expressionTree.NodeType == ExpressionType.Call)
            {
                MethodCallExpression methodCallExpression = expressionTree as MethodCallExpression;
                Expression collectionExpression = null;
                MemberExpression memberExpression = null;
                if (methodCallExpression != null && methodCallExpression.Method.Name == "Contains")
                {
                    if (methodCallExpression.Method.DeclaringType == typeof(Enumerable))
                    {
                        collectionExpression = methodCallExpression.Arguments[0];
                        memberExpression = methodCallExpression.Arguments[1] as MemberExpression;
                    }
                    else
                    {
                        collectionExpression = methodCallExpression.Object;
                        memberExpression = methodCallExpression.Arguments[0] as MemberExpression;
                    }

                    string parameterValue = string.Empty;
                    var paramvalue = new List<int>();
                    if (collectionExpression != null && memberExpression != null)
                    {
                        var lambda = Expression.Lambda<Func<List<int>>>(collectionExpression, new ParameterExpression[0]);
                        var value = lambda.Compile()();
                        parameterValue = memberExpression.Member.Name.ToCamelCase();
                        paramvalue = value;
                    }
                    FilterDescriptor<T> filterD = new FilterDescriptor<T>();
                    //FilterContainer baseFilter = filterD.Term(memberExpression.Member.Name.ToCamelCase(), paramvalue);
                    FilterContainer baseFilter = filterD.Terms(parameterValue, paramvalue.Select(i => i.ToString()).ToList());

                    stack.Push(baseFilter);
                }
                else
                {
                     memberExpression = methodCallExpression.Object as MemberExpression;
                    if (memberExpression.NodeType == ExpressionType.MemberAccess)
                        memberExpression = memberExpression.Expression as MemberExpression;

                    ConstantExpression constantExpression = methodCallExpression.Arguments[0] as ConstantExpression;

                    string parameterName = memberExpression.Member.Name;
                    string parameterValue = constantExpression.Value.ToString();

                    FilterDescriptor<T> filterD = new FilterDescriptor<T>();
                    FilterContainer baseFilter = filterD.Term(parameterName.ToCamelCase(), parameterValue);
                    stack.Push(baseFilter);
                }
                
   
            }
            else if (expressionTree.NodeType == ExpressionType.AndAlso)
            {
                //BinaryExpression binaryExpression = expressionTree as BinaryExpression;
                IList<FilterContainer> filters = new List<FilterContainer>();

                foreach (var i in Enumerable.Range(1, stack.Count))
                {
                    FilterContainer filter = stack.Pop();
                    filters.Add(filter);
                }

                FilterDescriptor<T> filterD = new FilterDescriptor<T>();
                FilterContainer baseFilter = filterD.And(filters.ToArray());
                stack.Push(baseFilter);
            }
        }

        public IEnumerable<string> CheckSuppressionList(SearchParameters parameters)
        {
            SearchResult<T> searchResult = new SearchResult<T>();
            ISearchResponse<object> result;
            SearchDescriptor<object> descriptor = new SearchDescriptor<object>();
            String[] types = new string[] { "suppressedemails", "suppresseddomains" };

            if (parameters.Ids.IsAny())
                parameters.Ids = parameters.Ids.ToList().ConvertAll(c => c.ToLower());
            List<string> domains = getDomains(parameters.Ids.ToList());
            descriptor = descriptor.Query(q => q.Filtered(f => f.Filter(fi => fi.Or(o => o.Terms("email", parameters.Ids), r => r.Terms("domain", domains)))))
                .Indices(getIndices(parameters.AccountId)).Types(types);

            result = ElasticClient().Search<object>(body => descriptor);
            string qu = result.ConnectionStatus.ToString();
            Logger.Current.Informational(qu);

            return convertDocs(parameters.Ids, result.Hits);
        }


        private IEnumerable<string> convertDocs(IEnumerable<string> ids, IEnumerable<IHit<object>> docs)
        {
            List<string> results = new List<string>();
            if (docs != null && docs.IsAny())
            {
                foreach (var doc in docs)
                {
                    JObject obj = (JObject)doc.Source;
                    if (doc.Type == "suppresseddomains")
                    {
                        string domain = obj["domain"].ToObject<string>();
                        var suppEmails = ids.Where(w => w.Contains(domain));
                        results.AddRange(suppEmails);
                    }
                    else if (doc.Type == "suppressedemails")
                    {
                        string email = obj["email"].ToObject<string>();
                        results.Add(email);
                    }
                }
            }
            return results.Distinct();
        }

        private List<string> getDomains(List<string> emails)
        {
            List<string> domains = new List<string>();
            if (emails != null && emails.IsAny())
            {
                emails.Each(e =>
                {
                    string[] strArray = e.Split('@');
                    if (strArray != null && strArray.Any() && strArray.Length > 1)
                        domains.Add(strArray[1]);
                });
                domains = domains.Distinct().ToList();
            }
            return domains;
        }

        private IEnumerable<string> getIndices(int id)
        {
            List<string> indices = new List<string>() { "suppressedemails1", "suppresseddomains1" };
            if (id != 1)
            {
                indices.Add("suppressedemails" + id);
                indices.Add("suppresseddomains" + id);
            }
            return indices;
        }

        public SearchResult<T> SearchSuppressionEmails(SearchParameters parameters)
        {
            SearchResult<T> searchResult = new SearchResult<T>();
            ISearchResponse<T> result;

            SearchDescriptor<T> descriptor = new SearchDescriptor<T>();
            descriptor.Query(q => q.Filtered(f => f.Filter(fi => fi.Terms("email", parameters.Ids))));

            result = ElasticClient().Search<T>(body => descriptor);
            string qu = result.ConnectionStatus.ToString();
            Logger.Current.Informational(qu);

            searchResult.Results = result.Documents;
            searchResult.TotalHits = result.Total;
            return searchResult;
        }

        public IEnumerable<T> SearchSuppressionList(string q, SearchParameters p)
        {
            ISearchResponse<T> result;

            SearchDescriptor<T> descriptor = new SearchDescriptor<T>();
            string Field = string.Empty;
            List<string> Indices = new List<string>();
            Type type = p.Types.FirstOrDefault();
            if (p != null && p.Types.FirstOrDefault() != null && type.Equals(typeof(SuppressedEmail)))
            {
                Field = "email";
                Indices.Add("suppressedemails1");
                if (p.AccountId != 1)
                    Indices.Add("suppressedemails" + p.AccountId);
            }
            else if (p != null && p.Types.FirstOrDefault() != null && type.Equals(typeof(SuppressedDomain)))
            {
                Field = "domain";
                Indices.Add("suppresseddomains1");
                if (p.AccountId != 1)
                    Indices.Add("suppresseddomains" + p.AccountId);
            }
            string regexValue = ".*" + q + ".*";
            if (!string.IsNullOrEmpty(q))
                regexValue = ".*" + q.ToLower() + ".*";
            FilterContainer query = new FilterContainer();
            FilterDescriptor<SuppressionList> filterDesc = new FilterDescriptor<SuppressionList>();

            query = filterDesc.Regexp(rg => rg.OnField(Field).Value(regexValue));
            descriptor = descriptor.Filter(fil => fil.Or(query)).Indices(Indices).Type(type.Name.ToLower() + "s");


            result = ElasticClient().Search<T>(b => descriptor);
            string qu = result.ConnectionStatus.ToString();
            Logger.Current.Informational(qu);

            return result.Documents;
        }

        private IEnumerable<Suggestion> convertSuppressionList(IEnumerable<object> list, Type type)
        {
            List<Suggestion> results = new List<Suggestion>();
            if (list.IsAny())
            {
                foreach (var res in list)
                {
                    if (type.Equals(typeof(SuppressedEmail)))
                    {
                        SuppressedEmail email = res as SuppressedEmail;
                        results.Add(new Suggestion() { AccountId = email.AccountID, DocumentId = email.Id, Text = email.Email });
                    }
                    else
                    {
                        SuppressedDomain domain = res as SuppressedDomain;
                        results.Add(new Suggestion() { AccountId = domain.AccountID, DocumentId = domain.Id, Text = domain.Domain });
                    }
                }
            }
            return results;
        }

    }
}
