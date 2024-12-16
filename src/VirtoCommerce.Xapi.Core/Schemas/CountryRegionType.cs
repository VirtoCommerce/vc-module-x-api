using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    public class CountryRegionType : ExtendableGraphType<CountryRegion>
    {
        public CountryRegionType()
        {
            Field(x => x.Id, nullable: false).Description("Code of country region. For example 'AL'.");
            Field(x => x.Name, nullable: false).Description("Name of country region. For example 'Alabama'.");
        }
    }
}
