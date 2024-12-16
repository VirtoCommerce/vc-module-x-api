using System.Threading.Tasks;
using GraphQL;
using GraphQL.Validation;
using GraphQL.Validation.Errors;
using GraphQLParser.AST;

namespace VirtoCommerce.Xapi.Core.Infrastructure.Validation
{
    public class CustomFieldsOnCorrectType : ValidationRuleBase
    {
        private readonly INodeVisitor _nodeVisitor;

        public CustomFieldsOnCorrectType()
        {
            _nodeVisitor = new MatchingNodeVisitor<GraphQLField>(Validate);
        }

        public override ValueTask<INodeVisitor> GetPreNodeVisitorAsync(ValidationContext context) => new(_nodeVisitor);

        protected virtual void Validate(GraphQLField node, ValidationContext context)
        {
            var type = context.TypeInfo.GetParentType()?.GetNamedType();

            if (type != null)
            {
                var field = context.TypeInfo.GetFieldDef();
                if (field == null)
                {
                    context.ReportError(new FieldsOnCorrectTypeError(context, node, type, suggestedTypeNames: null, suggestedFieldNames: null));
                }
            }
        }
    }
}
