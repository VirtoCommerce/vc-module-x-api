using GraphQL.Types;

namespace VirtoCommerce.Xapi.Core.Infrastructure
{
    public interface IGraphTypeHook
    {
        string TypeName { get; set; }

        void BeforeTypeInitialized(IGraphType graphType);
    }
}
