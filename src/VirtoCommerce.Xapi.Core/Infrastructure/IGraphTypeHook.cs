using System.Collections.Generic;
using GraphQL.Types;

namespace VirtoCommerce.Xapi.Core.Infrastructure
{
    public interface IGraphTypeHook
    {
        public string TypeName { get; set; }

        public IList<FieldType> BeforeTypeInitialized(IGraphType graphType);
    }
}
