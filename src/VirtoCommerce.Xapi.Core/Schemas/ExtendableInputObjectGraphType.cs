using System;
using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using VirtoCommerce.Xapi.Core.Helpers;

namespace VirtoCommerce.Xapi.Core.Schemas;

public class ExtendableInputObjectGraphType : ExtendableInputObjectGraphType<object>
{
}

public class ExtendableInputObjectGraphType<TSourceType> : InputObjectGraphType<TSourceType>
{
    private Func<IDictionary<string, object>, object> _parseDictionary;
    private bool _initialized;

    public ExtendableInputObjectGraphType()
    {
        if (typeof(TSourceType) == typeof(object))
        {
            // when typeof(TSourceType) != typeof(object)
            _parseDictionary = static x =>
            {
                return x;
            };
        }
    }

    public override FieldType AddField(FieldType fieldType)
    {
        fieldType.Type = GraphTypeExtensionHelper.GetActualComplexType(fieldType.Type);

        return base.AddField(fieldType);
    }


    public override void Initialize(ISchema schema)
    {
        if (_initialized)
        {
            throw new InvalidOperationException($"The graph type with name '{Name}' has already been initialized. Make sure that you do not use the same instance of a graph type in multiple schemas. It may be so if you registered this graph type as singleton; see https://graphql-dotnet.github.io/docs/getting-started/dependency-injection/ for more info.");
        }

        _initialized = true;

        // when typeof(TSourceType) != typeof(object)
        if (_parseDictionary != null)
        {
            return;
        }

        var actualType = GenericTypeHelper.GetActualType<TSourceType>();

        var conversion = ValueConverter.GetConversion(typeof(IDictionary<string, object>), actualType);
        if (conversion != null)
        {
            _parseDictionary = conversion;
        }
        else if (GlobalSwitches.DynamicallyCompileToObject)
        {
            _parseDictionary = data =>
            {
                _parseDictionary = ObjectExtensions.CompileToObject(actualType, this);
                var result = _parseDictionary(data);
                return result;
            };
        }
        else
        {
            _parseDictionary = data => data.ToObject(actualType, this);
        }
    }

    public override object ParseDictionary(IDictionary<string, object> value)
    {
        if (value == null)
        {
            return null;
        }

        object result = null;
        if (_parseDictionary != null)
        {
            result = _parseDictionary(value);
            return result;
        }

        var actualType = GenericTypeHelper.GetActualType<TSourceType>();
        result = value.ToObject(actualType, this);
        return result;
    }
}
