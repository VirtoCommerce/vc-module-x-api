using System.Collections.Generic;

namespace VirtoCommerce.Xapi.Core.Models;

public class GraphQLOptions
{
    public IList<string> ForbiddenAuthenticationTypes { get; set; } = [];
}
