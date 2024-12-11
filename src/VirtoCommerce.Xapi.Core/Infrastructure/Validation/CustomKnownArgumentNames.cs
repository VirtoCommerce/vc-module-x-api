using System;
using System.Threading.Tasks;
using GraphQL.Types;
using GraphQL.Validation;
using GraphQL.Validation.Errors;
using GraphQLParser.AST;

namespace VirtoCommerce.Xapi.Core.Infrastructure.Validation
{
    public class CustomKnownArgumentNames : IValidationRule
    {
        private readonly INodeVisitor _nodeVisitor;

        public CustomKnownArgumentNames()
        {
            _nodeVisitor = new MatchingNodeVisitor<GraphQLArgument>(Validate);
        }

        public virtual ValueTask<INodeVisitor> ValidateAsync(ValidationContext context) => new(_nodeVisitor);

        protected virtual void Validate(GraphQLArgument node, ValidationContext context)
        {
            var argument = context.TypeInfo.GetAncestor(2);
            switch (argument)
            {
                case GraphQLField:
                    {
                        var field = context.TypeInfo.GetFieldDef();
                        if (field != null)
                        {
                            var fieldArgument = field.Arguments?.Find(node.Name);
                            if (fieldArgument == null)
                            {
                                var fieldClone = new FieldType { Name = field.Name }; // clone without arguments
                                var parentType = context.TypeInfo.GetParentType() ?? throw new InvalidOperationException("Parent type must not be null.");
                                context.ReportError(new KnownArgumentNamesError(context, node, fieldClone, parentType));
                            }
                        }

                        break;
                    }

                case GraphQLDirective:
                    {
                        var directive = context.TypeInfo.GetDirective();
                        if (directive != null)
                        {
                            var directiveArgument = directive.Arguments?.Find(node.Name);
                            if (directiveArgument == null)
                            {
                                var directiveClone = new Directive(directive.Name, directive.Locations); // clone without arguments
                                context.ReportError(new KnownArgumentNamesError(context, node, directiveClone));
                            }
                        }

                        break;
                    }
            }
        }
    }
}
