using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    public class StoreResponseType : ExtendableGraphType<StoreResponse>
    {
        public StoreResponseType()
        {
            Field(x => x.StoreId, nullable: false).Description("Store ID");
            Field(x => x.StoreName, nullable: false).Description("Store name");
            Field(x => x.CatalogId, nullable: false).Description("Store catalog ID");
            Field(x => x.StoreUrl, nullable: true).Description("Store URL");

            Field<NonNullGraphType<LanguageType>>(nameof(StoreResponse.DefaultLanguage)).Description("Language").Resolve(context => context.Source.DefaultLanguage);
            Field<NonNullGraphType<ListGraphType<NonNullGraphType<LanguageType>>>>(nameof(StoreResponse.AvailableLanguages)).Description("Available languages").Resolve(context => context.Source.AvailableLanguages);

            Field<NonNullGraphType<CurrencyType>>(nameof(StoreResponse.DefaultCurrency)).Description("Currency").Resolve(context => context.Source.DefaultCurrency);
            Field<NonNullGraphType<ListGraphType<NonNullGraphType<CurrencyType>>>>(nameof(StoreResponse.AvailableCurrencies)).Description("Available currencies").Resolve(context => context.Source.AvailableCurrencies);

            Field<NonNullGraphType<StoreSettingsType>>(nameof(StoreResponse.Settings)).Description("Store settings").Resolve(context => context.Source.Settings);
            Field<NonNullGraphType<GraphQLSettingsType>>(nameof(StoreResponse.GraphQLSettings)).Description("GraphQL settings").Resolve(context => context.Source.GraphQLSettings);
        }
    }
}
