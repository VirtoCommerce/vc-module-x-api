using System.Collections.Generic;
using GraphQL.Types;

namespace VirtoCommerce.Xapi.Core.Infrastructure;

public interface IHasArguments
{
    IEnumerable<QueryArgument> GetArguments();
}
