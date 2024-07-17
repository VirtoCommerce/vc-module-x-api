using System.Linq;
using GraphQL.Types;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Xapi.Core.Extensions;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    public class DictionaryItemType : ExtendableGraphType<DynamicPropertyDictionaryItem>
    {
        public DictionaryItemType()
        {
            Field(x => x.Id, nullable: false).Description("Id");
            Field(x => x.Name, nullable: false).Description("Name");
            Field<StringGraphType>("label",
                "Localized dictionary item value",
                resolve: context =>
            {
                var culture = context.GetValue<string>("cultureName");
                return context.Source.DisplayNames.FirstOrDefault(x => culture.IsNullOrEmpty() || x.Locale.EqualsInvariant(culture))?.Name;
            });
        }
    }
}
