using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.Xapi.Core.Binding
{
    public class DefaultPropertyIndexBinder : IIndexModelBinder
    {
        public BindingInfo BindingInfo { get; set; }

        public object BindModel(SearchDocument searchDocument)
        {
            var fieldName = BindingInfo?.FieldName;
            if (!string.IsNullOrEmpty(fieldName) && searchDocument.TryGetValue(fieldName, out var value))
            {
                return value;
            }
            return null;
        }
    }
}
