using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Builders;
using GraphQL.Types;
using MediatR;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Queries;
using static VirtoCommerce.Xapi.Core.ModuleConstants;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    public class DynamicPropertyType : ExtendableGraphType<DynamicProperty>
    {
        public DynamicPropertyType(IMediator mediator)
        {
            Field(x => x.Id, nullable: false).Description("Id");
            Field<NonNullGraphType<StringGraphType>>("Name").Resolve(context => context.Source.Name);
            Field(x => x.ObjectType, nullable: false).Description("Object type");
            Field<StringGraphType>("label")
                .Description("Localized property name")
                .Resolve(context =>
                {
                    var culture = context.GetValue<string>("cultureName");
                    return context.Source.DisplayNames.FirstOrDefault(x => culture.IsNullOrEmpty() || x.Locale.EqualsInvariant(culture))?.Name;
                });
            Field(x => x.DisplayOrder, nullable: true).Description("The order for the dynamic property to display");
            Field<NonNullGraphType<StringGraphType>>(nameof(DynamicProperty.ValueType))
                .Description("Value type")
                .DeprecationReason("Use dynamicPropertyValueType instead")
                .Resolve(context => context.Source.ValueType.ToString());
            Field<NonNullGraphType<DynamicPropertyValueTypeEnum>>("dynamicPropertyValueType")
                .Description("Value type")
                .Resolve(context => context.Source.ValueType);

            Field<NonNullGraphType<BooleanGraphType>>("isArray").Resolve(context => context.Source.IsArray).Description("Is dynamic property value an array");
            Field<NonNullGraphType<BooleanGraphType>>("isDictionary").Resolve(context => context.Source.IsDictionary).Description("Is dynamic property value a dictionary");
            Field<NonNullGraphType<BooleanGraphType>>("isMultilingual").Resolve(context => context.Source.IsMultilingual).Description("Is dynamic property value multilingual");
            Field<NonNullGraphType<BooleanGraphType>>("isRequired").Resolve(context => context.Source.IsRequired).Description("Is dynamic property value required");

            Connection<DictionaryItemType>(name: "dictionaryItems")
              .Argument<StringGraphType>("filter", "")
              .Argument<StringGraphType>("cultureName", "")
              .Argument<StringGraphType>("sort", "")
              .PageSize(Connections.DefaultPageSize)
              .ResolveAsync(async context =>
              {
                  return await ResolveConnectionAsync(mediator, context);
              });
        }

        private static async Task<object> ResolveConnectionAsync(IMediator mediator, IResolveConnectionContext<DynamicProperty> context)
        {
            _ = int.TryParse(context.After, out var skip);

            var query = context.GetDynamicPropertiesQuery<SearchDynamicPropertyDictionaryItemQuery>();
            query.PropertyId = context.Source.Id;
            query.Skip = skip;
            query.Take = context.First ?? context.PageSize ?? Connections.DefaultPageSize;
            query.Sort = context.GetArgument<string>("sort");
            query.Filter = context.GetArgument<string>("filter");

            context.CopyArgumentsToUserContext();

            var response = await mediator.Send(query);

            return new PagedConnection<DynamicPropertyDictionaryItem>(response.Results, query.Skip, query.Take, response.TotalCount);
        }
    }
}
