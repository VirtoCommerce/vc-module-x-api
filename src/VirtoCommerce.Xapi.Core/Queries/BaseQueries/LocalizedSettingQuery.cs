using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Queries.BaseQueries;

public abstract class LocalizedSettingQuery : Query<LocalizedSettingResponse>
{
    public string CultureName { get; set; }

    public override IEnumerable<QueryArgument> GetArguments()
    {
        yield return Argument<StringGraphType>(nameof(CultureName));
    }

    public override void Map(IResolveFieldContext context)
    {
        CultureName = context.GetArgument<string>(nameof(CultureName));
    }
}
