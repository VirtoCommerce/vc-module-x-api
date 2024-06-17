using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.Xapi.Core.Binding
{
    public interface IIndexModelBinder
    {
        BindingInfo BindingInfo { get; set; }

        object BindModel(SearchDocument searchDocument);
    }
}
