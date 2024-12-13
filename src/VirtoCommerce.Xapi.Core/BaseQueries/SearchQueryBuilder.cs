using GraphQL.Types;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Helpers;
using VirtoCommerce.Xapi.Core.Infrastructure;
using static VirtoCommerce.Xapi.Core.ModuleConstants;

namespace VirtoCommerce.Xapi.Core.BaseQueries;

public abstract class SearchQueryBuilder<TQuery, TResult, TItem, TItemGraphType>
    : QueryBuilder<TQuery, TResult, TItemGraphType>
    where TQuery : IQuery<TResult>, IExtendableQuery, IHasArguments, ISearchQuery
    where TResult : GenericSearchResult<TItem>
    where TItemGraphType : IGraphType
{
    protected virtual int DefaultPageSize => Connections.DefaultPageSize;

    protected SearchQueryBuilder(IMediator mediator, IAuthorizationService authorizationService)
        : base(mediator, authorizationService)
    {
    }

    protected override FieldType GetFieldType()
    {
        var builder = GraphTypeExtensionHelper.CreateConnection<TItemGraphType, object>(Name)
            .PageSize(DefaultPageSize);

        ConfigureArguments(builder.FieldType);

        builder.ResolveAsync(async context =>
        {
            var (query, response) = await Resolve(context);
            return new PagedConnection<TItem>(response.Results, query.Skip, query.Take, response.TotalCount);
        });

        return builder.FieldType;
    }
}
