using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Schemas;

public class StoreSettingsType : ExtendableGraphType<StoreSettings>
{
    public StoreSettingsType()
    {
#pragma warning disable VC0009
        Field(x => x.QuotesEnabled).Description("Quotes enabled").DeprecationReason("Use Quotes.EnableQuotes public property instead.");
        Field(x => x.SubscriptionEnabled).Description("Subscription enabled").DeprecationReason("Use Subscription.EnableSubscriptions public property instead.");
        Field(x => x.IsSpa).Description("SPA").DeprecationReason("Client application should use own business logic for SPA detection.");
#pragma warning restore VC0009
#pragma warning disable VC0010
        Field(x => x.CreateAnonymousOrderEnabled).Description("Allow anonymous users to create orders (XAPI)")
            .DeprecationReason("Use XOrder.CreateAnonymousOrderEnabled public property instead.");
        Field(x => x.DefaultSelectedForCheckout).Description("Default \"Selected for checkout\" state for new line items and gifts")
            .DeprecationReason("Use XPurchase.IsSelectedForCheckout public property instead.");
#pragma warning restore VC0010
        Field(x => x.TaxCalculationEnabled).Description("Tax calculation enabled");
        Field(x => x.AnonymousUsersAllowed).Description("Allow anonymous users to visit the store ");
        Field(x => x.EmailVerificationEnabled).Description("Email address verification enabled");
        Field(x => x.EmailVerificationRequired).Description("Email address verification required");
        Field(x => x.SeoLinkType).Description("SEO links");
        Field(x => x.EnvironmentName).Description("Environment name");
        Field<PasswordOptionsType>("passwordRequirements").Description("Password requirements");
        Field<NonNullGraphType<ListGraphType<StringGraphType>>>("authenticationTypes");
        Field<NonNullGraphType<ListGraphType<NonNullGraphType<ModuleSettingsType>>>>("modules");
    }
}
