using System;
using System.Threading.Tasks;
using GraphQL.Builders;
using GraphQL.Resolvers;
using GraphQL.Types;
using MediatR;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Helpers;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Queries;
using VirtoCommerce.Xapi.Core.Schemas;
using static VirtoCommerce.Xapi.Core.ModuleConstants;

namespace VirtoCommerce.Xapi.Data.Schemas
{
    public class DynamicPropertySchema : ISchemaBuilder
    {
        public DynamicPropertySchema()
        {
        }

        [Obsolete("Use the constructor without IMediator. The mediator is resolved from context.RequestServices per request.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        public DynamicPropertySchema(IMediator mediator)
            : this()
        {
        }

        public void Build(ISchema schema)
        {
            var dynamicPropertyField = new FieldType
            {
                Name = "dynamicProperty",
                Arguments = new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "idOrName", Description = "Id or name of the dynamic property" },
                    new QueryArgument<StringGraphType> { Name = "cultureName", Description = "Culture name (\"en-US\")" },
                    new QueryArgument<StringGraphType> { Name = "objectType", Description = "Object type of the dynamic property" }
                ),
                Type = GraphTypeExtensionHelper.GetActualType<DynamicPropertyType>(),
                Resolver = new FuncFieldResolver<object>(async context =>
                {
                    context.CopyArgumentsToUserContext();

                    var query = context.GetDynamicPropertiesQuery<GetDynamicPropertyQuery>();
                    query.IdOrName = context.GetArgument<string>("idOrName");
                    query.ObjectType = context.GetArgument<string>("objectType");

                    var response = await context.GetMediator().Send(query);

                    return response.DynamicProperty;
                })
            };
            schema.Query.AddField(dynamicPropertyField);

            var dynamicPropertiesConnectionBuilder = GraphTypeExtensionHelper.CreateConnection<DynamicPropertyType, object>("dynamicProperties")
                .Argument<StringGraphType>("cultureName", "The culture name (\"en-US\")")
                .Argument<StringGraphType>("filter", "This parameter applies a filter to the query results")
                .Argument<StringGraphType>("sort", "The sort expression")
                .Argument<StringGraphType>("objectType", "Object type of the dynamic property")
                .PageSize(Connections.DefaultPageSize);

            dynamicPropertiesConnectionBuilder.ResolveAsync(async context => await ResolveDynamicPropertiesConnectionAsync(context));

            schema.Query.AddField(dynamicPropertiesConnectionBuilder.FieldType);
        }

        private static async Task<object> ResolveDynamicPropertiesConnectionAsync(IResolveConnectionContext<object> context)
        {
            int.TryParse(context.After, out var skip);

            var query = context.GetDynamicPropertiesQuery<SearchDynamicPropertiesQuery>();
            query.Skip = skip;
            query.Take = context.First ?? context.PageSize ?? Connections.DefaultPageSize;
            query.Sort = context.GetArgument<string>("sort");
            query.Filter = context.GetArgument<string>("filter");
            query.ObjectType = context.GetArgument<string>("objectType");

            context.CopyArgumentsToUserContext();

            var response = await context.GetMediator().Send(query);

            return new PagedConnection<DynamicProperty>(response.Results, query.Skip, query.Take, response.TotalCount);
        }
    }
}
