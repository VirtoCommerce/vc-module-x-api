using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Xapi.Core.BaseQueries;

public abstract class Query<TResult> : IQuery<TResult>, IExtendableQuery, IHasArguments
{
    public abstract IEnumerable<QueryArgument> GetArguments();

    public abstract void Map(IResolveFieldContext context);

    protected QueryArgument Argument<T>(string name, string description = null)
        where T : IGraphType
    {
        return new QueryArgument<T>
        {
            Name = name,
            Description = description,
        };
    }
}
