using System;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using GraphQL.Types.Relay;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    public class ExtendableConnectionType<TNodeType> : ConnectionType<TNodeType, EdgeType<TNodeType>>
        where TNodeType : IGraphType
    {
        public new FieldType Field<TGraphType>(
        string name,
        string description = null,
            QueryArguments arguments = null,
            Func<IResolveFieldContext<object>, object> resolve = null,
            string deprecationReason = null)
            where TGraphType : IGraphType
        {
            return AddField(new FieldType
            {
                Name = name,
                Description = description,
                DeprecationReason = deprecationReason,
                Type = typeof(TGraphType),
                Arguments = arguments,
                Resolver = resolve != null
                    ? new FuncFieldResolver<object, object>(resolve)
                    : null
            });
        }
    }
}
