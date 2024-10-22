using System;

namespace VirtoCommerce.Xapi.Core.Infrastructure;

[AttributeUsage(AttributeTargets.Class)]
public class SubSchemaNameAttribute : Attribute
{
    public string Name { get; set; }

    public SubSchemaNameAttribute(string name)
    {
        Name = name;
    }
}
