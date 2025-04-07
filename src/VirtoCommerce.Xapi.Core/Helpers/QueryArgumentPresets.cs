using System;
using GraphQL.Types;

namespace VirtoCommerce.Xapi.Core.Helpers
{
    public static class QueryArgumentPresets
    {
        public static QueryArguments ArgumentsForMoney() => new QueryArguments(
            new QueryArgument<StringGraphType> { Name = Constants.CurrencyCode },
            new QueryArgument<StringGraphType> { Name = Constants.CultureName }
        );

        [Obsolete("Use cultureName parameter on query, mutation level", DiagnosticId = "VC0010", UrlFormat = "https://docs.virtocommerce.org/platform/user-guide/versions/virto3-products-versions/")]
        public static QueryArguments GetArgumentForDynamicProperties()
        {
            return new QueryArguments(new QueryArgument<StringGraphType>
            {
                Name = "cultureName",
                Description = "Filter multilingual dynamic properties to return only values of specified language (\"en-US\")"
            });
        }

        public static QueryArguments GetArgumentsForCartValidator()
        {
            return new QueryArguments(new QueryArgument<StringGraphType> { Name = "ruleSet", Description = "CartValidator's rule sets to call. One of or comma-divided combination of \"items\",\"shipments\",\"payments\"" });
        }
    }
}
