using System.Linq;
using GraphQL;
using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.Xapi.Core.Schemas;

public class LocalizedSettingResponseType : ExtendableGraphType<LocalizedSettingResponse>
{
    public LocalizedSettingResponseType()
    {
        Field<ListGraphType<KeyValueType>>(nameof(LocalizedSettingResponse.Items).ToCamelCase(), resolve: context =>
            context.Source.Items?.Select(x => new KeyValue { Key = x.Key, Value = x.Value }));
    }
}
