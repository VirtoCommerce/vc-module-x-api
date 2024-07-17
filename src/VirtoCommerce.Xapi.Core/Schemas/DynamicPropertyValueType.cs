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

            Field<StringGraphType>("name",
                "Property name",
                resolve: context => context.Source.PropertyName);
            Field<NonNullGraphType<StringGraphType>>(nameof(DynamicPropertyObjectValue.ValueType),
                "Value type",
                resolve: context => context.Source.ValueType.ToString());
            Field<NonNullGraphType<DynamicPropertyValueTypeEnum>>("dynamicPropertyValueType",
                "Value type",
                resolve: context => context.Source.ValueType);
            Field<DynamicPropertyValueGraphType>(nameof(DynamicPropertyObjectValue.Value),
                "Property value",
                resolve: context => context.Source.Value);

            FieldAsync<DictionaryItemType>("dictionaryItem", "Associated dictionary item", resolve: async context =>
            {
                var id = context.Source.ValueId;
                if (id.IsNullOrEmpty())
                {
                    return null;
                }

                var items = await _dynamicPropertyDictionaryItemsService.GetDynamicPropertyDictionaryItemsAsync(new[] { id });

                return items.FirstOrDefault();
            });

            FieldAsync<DynamicPropertyType>("dynamicProperty", "Associated dynamic property", resolve: async context =>
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
