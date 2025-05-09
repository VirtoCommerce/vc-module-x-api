using VirtoCommerce.CoreModule.Core.Seo;

namespace VirtoCommerce.Xapi.Core.Models;
public class PipelineSeoInfoRequest
{
    public SeoSearchCriteria SeoSearchCriteria { get; set; }
    public SeoInfo SeoInfo { get; set; }
}
