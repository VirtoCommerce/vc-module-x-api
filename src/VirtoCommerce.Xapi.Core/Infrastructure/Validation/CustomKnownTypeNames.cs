using System.Threading.Tasks;
using GraphQL.Validation;
using GraphQL.Validation.Errors;
using GraphQLParser.AST;

namespace VirtoCommerce.Xapi.Core.Infrastructure.Validation
{
    public class CustomKnownTypeNames : IValidationRule
    {
        private readonly INodeVisitor _nodeVisitor;

        public CustomKnownTypeNames()
        {
            _nodeVisitor = new MatchingNodeVisitor<GraphQLNamedType>(Validate);
        }

        public virtual ValueTask<INodeVisitor> ValidateAsync(ValidationContext context) => new(_nodeVisitor);

        protected virtual void Validate(GraphQLNamedType node, ValidationContext context)
        {
            var type = context.Schema.AllTypes[node.Name];
            if (type == null)
            {
                context.ReportError(new KnownTypeNamesError(context, node, suggestedTypes: null));
            }
        }
    }
}
