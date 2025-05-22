using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Schemas;

namespace VirtoCommerce.Xapi.Data.Schemas;

public class SlugInfoResponseType : ExtendableGraphType<SlugInfoResponse>
{
    public SlugInfoResponseType()
    {
        Field<SeoInfoType>("entityInfo").Description("SEO info").Resolve(context => context.Source.EntityInfo);
    }
}
