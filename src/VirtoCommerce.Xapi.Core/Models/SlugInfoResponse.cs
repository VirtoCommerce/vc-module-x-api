using System;
using VirtoCommerce.CoreModule.Core.Seo;

namespace VirtoCommerce.Xapi.Core.Models
{
    [Obsolete("Class is deprecated, please use SEO module instead.", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
    public class SlugInfoResponse
    {
        public SeoInfo EntityInfo { get; set; }
        public string RedirectUrl { get; set; }
    }
}
