using System;
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
        public DynamicPropertyValueType(IDynamicPropertyDictionaryItemsService dynamicPropertyDictionaryItemsService)
        {
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

            Field<DictionaryItemType>("dictionaryItem")
                .Description("Associated dictionary item")
                .ResolveAsync(async context =>
                {
                    var id = context.Source.ValueId;

                    return string.IsNullOrEmpty(id)
                        ? null
                        : (await dynamicPropertyDictionaryItemsService.GetDynamicPropertyDictionaryItemsAsync([id])).FirstOrDefault();
                });

            Field<DynamicPropertyType>("dynamicProperty")
                .Description("Associated dynamic property")
                .ResolveAsync(async context =>
            {
                var id = context.Source.PropertyId;
                if (string.IsNullOrEmpty(id))
                {
                    return null;
                }

                var query = context.GetDynamicPropertiesQuery<GetDynamicPropertyQuery>();
                query.IdOrName = id;

                var response = await context.GetMediator().Send(query);

                return response.DynamicProperty;
            });
        }

        [Obsolete("Use the constructor without IMediator. The mediator is resolved from context.RequestServices per request.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        public DynamicPropertyValueType(IMediator mediator, IDynamicPropertyDictionaryItemsService dynamicPropertyDictionaryItemsService)
            : this(dynamicPropertyDictionaryItemsService)
        {
        }
    }
}
