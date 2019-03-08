using System;
using System.Collections.Generic;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Entities;
using System.Linq;
using Antlr.Runtime.Tree;
using Antlr;
using Antlr.Runtime;
using System.Text.RegularExpressions;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Fields;

namespace SmartTouch.CRM.Domain.Search
{
    public class SearchDefinition : EntityBase<int>, IAggregateRoot
    {
        public string Name { get; set; }
        public IEnumerable<SearchFilter> Filters { get; set; }
        public SearchPredicateType PredicateType { get; set; }
        public string CustomPredicateScript { get; set; }
        public DateTime? LastRunDate { get; set; }
        public IEnumerable<Tag> TagsList { get; set; }
        public string ElasticQuery { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public int? AccountID { get; set; }
        public short PageNumber { get; set; }
        public IList<Field> SelectedColumns { get; set; }        
        public IEnumerable<Field> Fields { get; set; }
        public bool IsFavoriteSearch { get; set; }
        public bool IsPreConfiguredSearch { get; set; }
        public bool? SelectAllSearch { get; set; }
        public string TagName { get; set; }
        public int TotalSearchsCount { get; set; }
        public bool IsAggregationNeeded { get; set; }
        public string ReportName { get; set; }

        public CommonTree CustomLogicalTree
        {
            get
            {
                if (string.IsNullOrEmpty(this.CustomPredicateScript)) return new CommonTree();
                else return getTree(this.CustomPredicateScript);
            }
        }

        private CommonTree getTree(string customExpression)
        {
            ANTLRStringStream expression = new ANTLRStringStream(customExpression);
            var tokens = new CommonTokenStream(new SimpleBooleanLexer(expression));
            var parser = new SimpleBooleanParser(tokens);
            SimpleBooleanParser.expr_return ret = parser.expr();
            return (CommonTree)ret.Tree;
        }

        protected override void Validate()
        {
            if (string.IsNullOrEmpty(Name)) AddBrokenRule(SearchDefinitionBusinessRules.SearchIsRequired);
            if (Filters == null || !Filters.Any()) AddBrokenRule(SearchDefinitionBusinessRules.MinimumOneFilterRequired);
            this.validateSearchFilters();
            if (PredicateType == SearchPredicateType.Custom && string.IsNullOrEmpty(this.CustomPredicateScript)) AddBrokenRule(SearchDefinitionBusinessRules.InValidLogicalExpression);
            if (PredicateType == SearchPredicateType.Custom && !isValidLogicalExpression()) AddBrokenRule(SearchDefinitionBusinessRules.InValidLogicalExpression);
        }

        private void validateSearchFilters()
        {
            foreach (SearchFilter filter in this.Filters)
            {
                if (filter.Field != ContactFields.LeadScore)
                {
                    if ((filter.Qualifier == SearchQualifier.Is ||
                        filter.Qualifier == SearchQualifier.IsNot ||
                        filter.Qualifier == SearchQualifier.Contains ||
                        filter.Qualifier == SearchQualifier.DoesNotContain ||
                        filter.Qualifier == SearchQualifier.IsLessThan ||
                        filter.Qualifier == SearchQualifier.IsLessThanEqualTo ||
                        filter.Qualifier == SearchQualifier.IsGreaterThan ||
                        filter.Qualifier == SearchQualifier.IsGreaterThanEqualTo) && string.IsNullOrEmpty(filter.SearchText))
                        AddBrokenRule(SearchDefinitionBusinessRules.InvalidSearchFieldValue);
                }
                else
                {
                    if (string.IsNullOrEmpty(filter.SearchText)) AddBrokenRule(SearchDefinitionBusinessRules.InvalidLeadScoreValue);

                    int leadScore = 0;
                    bool result = int.TryParse(filter.SearchText, out leadScore);
                    if (result == false) AddBrokenRule(SearchDefinitionBusinessRules.InvalidLeadScoreValue);
                    if ((filter.Qualifier == SearchQualifier.Is ||
                        filter.Qualifier == SearchQualifier.IsNot ||
                        filter.Qualifier == SearchQualifier.IsGreaterThan ||
                        filter.Qualifier == SearchQualifier.IsGreaterThanEqualTo ||
                        filter.Qualifier == SearchQualifier.IsLessThan ||
                        filter.Qualifier == SearchQualifier.IsLessThanEqualTo) && result == false)
                        AddBrokenRule(SearchDefinitionBusinessRules.InvalidLeadScoreValue);
                }
            }
        }
        private bool isValidLogicalExpression()
        {
            string re = @"(?x)^ 
			\s* (?: (?<open> \( ) \s* )*
			\d+
			\s* (?: (?<-open> \) ) \s* )*
			(?:
				\s+(?:AND|OR)\s+
				\s* (?: (?<open> \( ) \s* )*
				\d+
				\s* (?: (?<-open> \) ) \s* )*
			)*
			(?(open)(?!))
			\z";
            Match match = Regex.Match(this.CustomPredicateScript, re);
            if (!match.Success) return false;
            try
            {
                parseExpression(this.CustomLogicalTree, 0);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private void parseExpression(CommonTree tree, int level)
        {
            if (tree is CommonErrorNode) throw new ArgumentException();
            if (!(tree.Text.ToLower().Equals("or") || tree.Text.ToLower().Equals("and") || Regex.IsMatch(tree.Text, @"^\d+$"))) throw new ArgumentException();
            if (Regex.IsMatch(tree.Text, @"^\d+$"))
            {
                int filterId = 0;
                int.TryParse(tree.Text, out filterId);
                if (this.Filters.ElementAt(filterId - 1) == null)
                    throw new ArgumentException();
            }
            if (tree.Children != null)
                foreach (CommonTree child in tree.Children)
                    parseExpression(child, level + 1);
        }
    }
}
