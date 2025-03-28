using System.Threading.Tasks;
using GraphQL.Introspection;
using GraphQL.Types;
using Microsoft.Extensions.Options;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Infrastructure
{
    public sealed class CustomSchemaFilter : ISchemaFilter
    {
        private readonly Task<bool> _isAllowed;

        public CustomSchemaFilter(IOptions<GraphQLPlaygroundOptions> playgroundOptions)
        {
            _isAllowed = Task.FromResult(playgroundOptions.Value.Enable);
        }

        public Task<bool> AllowType(IGraphType type) => _isAllowed;

        public Task<bool> AllowDirective(Directive directive) => _isAllowed;

        public Task<bool> AllowArgument(IFieldType field, QueryArgument argument) => _isAllowed;

        public Task<bool> AllowEnumValue(EnumerationGraphType parent, EnumValueDefinition enumValue) => _isAllowed;

        public Task<bool> AllowField(IGraphType parent, IFieldType field) => _isAllowed;
    }
}
