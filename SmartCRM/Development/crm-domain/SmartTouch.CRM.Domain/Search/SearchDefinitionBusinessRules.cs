using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Search
{
    public static class SearchDefinitionBusinessRules
    {
        public static readonly BusinessRule InValidLogicalExpression = new BusinessRule("[|Invalid custom logic expression|].");

        public static readonly BusinessRule SearchIsRequired = new BusinessRule("[|Search name is required|].");

        public static readonly BusinessRule MinimumOneFilterRequired = new BusinessRule("[|At least one filter is required|].");

        public static readonly BusinessRule InvalidSearchFieldValue = new BusinessRule("[|Invalid search filter value|].");

        public static readonly BusinessRule InvalidLeadScoreValue = new BusinessRule("[|Invalid Lead score value|].");

        public static readonly BusinessRule SearchTextMaxLength = new BusinessRule("[|Enter characters less than 256|]");
    }
}
