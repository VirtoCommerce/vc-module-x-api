//using System;
//using VirtoCommerce.Xapi.Core.Models;

//namespace VirtoCommerce.Xapi.Core.Schemas
//{
//    [Obsolete("Class is deprecated, please use SEO module instead.", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
//    public class SlugInfoResponseType : ExtendableGraphType<SlugInfoResponse>
//    {
//        public SlugInfoResponseType()
//        {
//            Field<SeoInfoType>("entityInfo").Description("SEO info").Resolve(context => context.Source.EntityInfo);
//        }
//    }
//}
