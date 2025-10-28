using System;

namespace VirtoCommerce.Xapi.Core.Models;

[AttributeUsage(AttributeTargets.Class)]
public class OptionalGraphQlTypesContainerAttribute : Attribute
{
    public string DependencyName { get; set; }

    public OptionalGraphQlTypesContainerAttribute()
    {
    }
}
