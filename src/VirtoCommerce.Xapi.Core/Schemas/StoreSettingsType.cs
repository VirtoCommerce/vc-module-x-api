using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Schemas;

public class StoreSettingsType : ExtendableGraphType<StoreSettings>
{
    public StoreSettingsType()
    {
        Field(x => x.TaxCalculationEnabled).Description("Tax calculation enabled");
        Field(x => x.AnonymousUsersAllowed).Description("Allow anonymous users to visit the store ");
        Field(x => x.EmailVerificationEnabled).Description("Email address verification enabled");
        Field(x => x.EmailVerificationRequired).Description("Email address verification required");
        Field(x => x.CreateAnonymousOrderEnabled).Description("Allow anonymous users to create orders (XAPI)");
        Field(x => x.SeoLinkType).Description("SEO links");
        Field(x => x.DefaultSelectedForCheckout).Description("Default \"Selected for checkout\" state for new line items and gifts");
        Field(x => x.EnvironmentName).Description("Environment name");
        Field<PasswordOptionsType>("passwordRequirements").Description("Password requirements");
        Field<NonNullGraphType<ListGraphType<StringGraphType>>>("authenticationTypes");
        Field<NonNullGraphType<ListGraphType<NonNullGraphType<ModuleSettingsType>>>>("modules");
    }
}
