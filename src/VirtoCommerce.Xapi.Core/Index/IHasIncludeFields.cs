using System.Collections.Generic;

namespace VirtoCommerce.Xapi.Core.Index
{
    public interface IHasIncludeFields
    {
        IList<string> IncludeFields { get; set; }
    }
}
