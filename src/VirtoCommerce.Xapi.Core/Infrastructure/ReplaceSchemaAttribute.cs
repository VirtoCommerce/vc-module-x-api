using System;

namespace VirtoCommerce.Xapi.Core.Infrastructure;

[AttributeUsage(AttributeTargets.Class)]
public class ReplaceSchemaAttribute(Type schemaToReplace) : Attribute
{
    public Type SchemaToReplace => schemaToReplace;
}
