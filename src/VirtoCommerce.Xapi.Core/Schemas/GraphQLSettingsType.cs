using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    public class GraphQLSettingsType : ObjectGraphType<GraphQLSettings>
    {
        public GraphQLSettingsType()
        {
            Field(x => x.KeepAliveInterval, nullable: false).Description("Keep-alive message interval for GraphQL subscription");
        }
    }
}
