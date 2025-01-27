using GraphQL.Types;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.Xapi.Core.Extensions;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    public class DiscountType : ExtendableGraphType<Discount>
    {
        public DiscountType()
        {
            Field(x => x.Coupon, nullable: true).Description("Coupon");
            Field(x => x.Description, nullable: true).Description("Value of discount description");
            Field(x => x.PromotionId, nullable: true).Description("Value of promotion id");
            Field<NonNullGraphType<DecimalGraphType>>("amount")
                .Description("Discount amount")
                .Resolve(context => context.Source.DiscountAmount);
            Field<NonNullGraphType<MoneyType>>("moneyAmount")
                .Description("Discount amount in the specified currency")
                .Resolve(context => context.Source.DiscountAmount.ToMoney(context.GetCurrencyByCode(context.Source.Currency)));
            Field<NonNullGraphType<DecimalGraphType>>("amountWithTax")
                .Description("Discount amount with tax")
                .Resolve(context => context.Source.DiscountAmountWithTax);
            Field<NonNullGraphType<MoneyType>>("moneyAmountWithTax")
                .Description("Discount amount with tax in the specified currency")
                .Resolve(context => context.Source.DiscountAmountWithTax.ToMoney(context.GetCurrencyByCode(context.Source.Currency)));
        }
    }
}
