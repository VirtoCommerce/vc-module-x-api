using GraphQL.Types;

namespace VirtoCommerce.Xapi.Core.Infrastructure
{
    public interface ISchemaFactory
    {
        ISchema GetSchema();
    }
}
