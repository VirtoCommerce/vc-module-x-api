using GraphQL.Types;

namespace VirtoCommerce.Xapi.Core.Infrastructure
{
    public interface ISchemaBuilder
    {
        void Build(ISchema schema);
    }
}
