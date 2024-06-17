using GraphQL;

namespace VirtoCommerce.Xapi.Core.Infrastructure
{
    public interface IExtendableQuery
    {
        public void Map(IResolveFieldContext context);
    }
}
