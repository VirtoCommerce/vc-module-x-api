using System;

namespace VirtoCommerce.Xapi.Core.Models;

[AttributeUsage(AttributeTargets.Class)]
public class OptionalGraphQlTypesContainerAttribute : Attribute
{
    public string DependencyName { get; }

    public OptionalGraphQlTypesContainerAttribute(string dependencyName)
    {
        ArgumentNullException.ThrowIfNull(dependencyName);

        DependencyName = dependencyName;
    }
}
