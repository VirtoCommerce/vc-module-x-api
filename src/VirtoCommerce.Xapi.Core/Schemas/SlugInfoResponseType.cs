using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    public class SlugInfoResponseType : ExtendableGraphType<SlugInfoResponse>
    {
        public SlugInfoResponseType()
        {
            Field<SeoInfoType>("entityInfo").Description("SEO info").Resolve(context => context.Source.EntityInfo);
        }
    }
}
