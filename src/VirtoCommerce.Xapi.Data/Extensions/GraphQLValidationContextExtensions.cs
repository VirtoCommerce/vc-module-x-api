using System.Linq;
using GraphQL.Validation;
using GraphQLParser.AST;

namespace VirtoCommerce.Xapi.Data.Extensions;

public static class GraphQLValidationContextExtensions
{
    public static bool IsIntrospectionRequest(this ValidationContext validationContext)
    {
        return validationContext.Document.Definitions.OfType<GraphQLOperationDefinition>().All(
            op => op.Operation == OperationType.Query && op.SelectionSet.Selections.All(
                node => node is GraphQLField field && (field.Name.Value == "__schema" || field.Name.Value == "__type")));
    }
}
