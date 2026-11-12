using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.Xapi.Core.Binding;

public class GenericModelBinder<TResult> : IIndexModelBinder
{
    public BindingInfo BindingInfo { get; set; }

    public virtual object BindModel(SearchDocument searchDocument)
    {
        var result = AbstractTypeFactory<TResult>.TryCreateInstance();

        result.BindIndexedProperties(searchDocument);

        return result;
    }
}
