using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Execution;
using GraphQL.Types;
using GraphQLParser;
using GraphQLParser.AST;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Xapi.Core.Extensions
{
    public static class AstFieldExtensions
    {
        public static IEnumerable<string> GetAllNodesPaths(this IEnumerable<(GraphQLField Field, FieldType FieldType)> fields, IResolveFieldContext context)
        {
            return fields.SelectMany(x => x.Field.GetAllTreeNodesPaths(context)).Distinct();
        }

        private static IEnumerable<string> GetAllTreeNodesPaths(this ASTNode node, IResolveFieldContext context, string path = null)
        {
            if (node is GraphQLField field)
            {
                path = path != null ? string.Join(".", path, field.Name.ToString()) : field.Name.ToString();
            }

            // combine fragment nodes and other children nodes
            var combinedNodes = GetCombinedChildrenNodes(node, context);
            if (combinedNodes.Any())
            {
                var childrenPaths = combinedNodes.Where(n => context != null && ShouldIncludeNode(n, context))
                    .SelectMany(n => n.GetAllTreeNodesPaths(context, path));
                foreach (var childPath in childrenPaths.DefaultIfEmpty(path))
                {
                    yield return childPath;
                }
            }
            else
            {
                yield return path;
            }
        }

        private static List<ASTNode> GetCombinedChildrenNodes(ASTNode node, IResolveFieldContext context)
        {
            var combinedNodes = new List<ASTNode>();

            if (node is not IHasSelectionSetNode selectionNode ||
                selectionNode.SelectionSet == null ||
                selectionNode.SelectionSet.Selections.IsNullOrEmpty())
            {
                return combinedNodes;
            }

            var fragmentDefenitions = context?.Document?.Definitions?.OfType<GraphQLFragmentDefinition>().ToList();
            if (fragmentDefenitions.IsNullOrEmpty())
            {
                return selectionNode.SelectionSet.Selections ?? combinedNodes;
            }

            foreach (var child in selectionNode.SelectionSet.Selections)
            {
                if (child is GraphQLFragmentSpread fragment)
                {
                    var fragmentDefenition = fragmentDefenitions.FirstOrDefault(x => x.FragmentName.Name == fragment.FragmentName.Name);
                    if (fragmentDefenition?.SelectionSet != null && fragmentDefenition.SelectionSet.Selections != null)
                    {
                        combinedNodes.AddRange(fragmentDefenition.SelectionSet.Selections);
                    }
                }
                else
                {
                    combinedNodes.Add(child);
                }
            }

            return combinedNodes;
        }

        private static bool ShouldIncludeNode(ASTNode node, IResolveFieldContext context)
        {
            var directives = node is IHasDirectivesNode haveDirectives ? haveDirectives.Directives : null;

            if (directives != null)
            {
                var directive = directives.Find(context.Schema.Directives.Skip.Name);
                if (directive != null)
                {
                    var arg = context.Schema.Directives.Skip.Arguments!.Find("if")!;
                    var value = ExecutionHelper.CoerceValue(arg.ResolvedType!, directive.Arguments?.ValueFor(arg.Name), context.Variables, arg.DefaultValue).Value;
                    if (value is true)
                    {
                        return false;
                    }
                }

                directive = directives.Find(context.Schema.Directives.Include.Name);
                if (directive != null)
                {
                    var arg = context.Schema.Directives.Include.Arguments!.Find("if")!;
                    var value = ExecutionHelper.CoerceValue(arg.ResolvedType!, directive.Arguments?.ValueFor(arg.Name), context.Variables, arg.DefaultValue).Value;

                    return value is true;
                }
            }

            return true;
        }
    }
}
