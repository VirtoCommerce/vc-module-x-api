using System;
using VirtoCommerce.CoreModule.Core.Seo;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    [Obsolete("Class is deprecated, please use SEO module instead.", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
    public class SeoInfoType : ExtendableGraphType<SeoInfo>
    {
        public SeoInfoType()
        {
            Name = "SeoInfo";

            Field(x => x.Id, nullable: false);
            Field(x => x.Name, nullable: true);
            Field(x => x.SemanticUrl, nullable: false);
            Field(x => x.PageTitle, nullable: true);
            Field(x => x.MetaDescription, nullable: true);
            Field(x => x.ImageAltDescription, nullable: true);
            Field(x => x.MetaKeywords, nullable: true);
            Field(x => x.StoreId, nullable: true);
            Field(x => x.ObjectId, nullable: false);
            Field(x => x.ObjectType, nullable: false);
            Field(x => x.IsActive, nullable: false);
            Field(x => x.LanguageCode, nullable: true);
        }
    }
}
