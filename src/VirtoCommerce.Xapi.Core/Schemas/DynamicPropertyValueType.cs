using System.Linq;
using GraphQL.Types;
using MediatR;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Queries;
using VirtoCommerce.Xapi.Core.Schemas.ScalarTypes;

namespace VirtoCommerce.Xapi.Core.Schemas
{
    public class DynamicPropertyValueType : ExtendableGraphType<DynamicPropertyObjectValue>
    {
        private readonly IDynamicPropertyDictionaryItemsService _dynamicPropertyDictionaryItemsService;

        public DynamicPropertyValueType(IMediator mediator, IDynamicPropertyDictionaryItemsService dynamicPropertyDictionaryItemsService)
        {
            _dynamicPropertyDictionaryItemsService = dynamicPropertyDictionaryItemsService;

            Field<StringGraphType>("name")
                .Description("Property name")
                .Resolve(context => context.Source.PropertyName);
            Field<NonNullGraphType<StringGraphType>>(nameof(DynamicPropertyObjectValue.ValueType))
                .Description("Value type")
                .Resolve(context => context.Source.ValueType.ToString());
            Field<NonNullGraphType<DynamicPropertyValueTypeEnum>>("dynamicPropertyValueType")
                .Description("Value type")
                .Resolve(context => context.Source.ValueType);
            Field<DynamicPropertyValueGraphType>(nameof(DynamicPropertyObjectValue.Value))
                .Description("Property value")
                .Resolve(context => context.Source.Value);

            Field<DictionaryItemType>("dictionaryItem").Description("Associated dictionary item").ResolveAsync(async context =>
            {
                var id = context.Source.ValueId;
                if (id.IsNullOrEmpty())
                {
                    return null;
                }

                var items = await _dynamicPropertyDictionaryItemsService.GetDynamicPropertyDictionaryItemsAsync([id]);

                return items.FirstOrDefault();
            });

            Field<DynamicPropertyType>("dynamicProperty").Description("Associated dynamic property").ResolveAsync(async context =>
            {
                var id = context.Source.PropertyId;
                if (id.IsNullOrEmpty())
                {
                    return null;
                }

                var query = context.GetDynamicPropertiesQuery<GetDynamicPropertyQuery>();
                query.IdOrName = id;

                var response = await mediator.Send(query);

                return response.DynamicProperty;
            });
        }
    }
}
