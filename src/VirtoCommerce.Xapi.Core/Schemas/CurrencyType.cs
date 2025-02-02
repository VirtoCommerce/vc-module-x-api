using VirtoCommerce.CoreModule.Core.Currency;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    public class CurrencyType : ExtendableGraphType<Currency>
    {
        public CurrencyType()
        {
            Field(x => x.Code, nullable: false).Description("Currency code may be used ISO 4217");
            Field(x => x.Symbol, nullable: false).Description("Symbol");
            Field(x => x.ExchangeRate, nullable: false).Description("Exchange rate");
            Field(x => x.CustomFormatting, nullable: true).Description("Currency custom formatting");
            Field(x => x.EnglishName, nullable: false).Description("Currency English name");
            Field(x => x.CultureName, nullable: false).Description("Currency English name");
        }
    }
}
