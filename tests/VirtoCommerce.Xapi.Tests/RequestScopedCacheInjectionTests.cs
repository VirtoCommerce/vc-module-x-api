using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Data.Extensions;
using Xunit;

namespace VirtoCommerce.Xapi.Tests
{
    public class RequestScopedCacheInjectionTests
    {
        [Fact]
        public void XapiTypes_ShouldNotCtorInjectRequestScopedCache()
        {
            var assemblies = new[]
            {
                typeof(ISchemaBuilder).Assembly, // VirtoCommerce.Xapi.Core
                typeof(ServiceCollectionExtensions).Assembly, // VirtoCommerce.Xapi.Data
            };

            var offendingConstructors =
                from assembly in assemblies
                from type in assembly.GetTypes()
                where !type.IsAbstract
                from ctor in type.GetConstructors()
                where ctor.GetCustomAttribute<ObsoleteAttribute>() is null
                where ctor.GetParameters().Any(parameter => parameter.ParameterType == typeof(IRequestScopedCache))
                select $"{type.FullName}({string.Join(", ", ctor.GetParameters().Select(parameter => parameter.ParameterType.Name))})";

            offendingConstructors.Should().BeEmpty(
                "IRequestScopedCache is registered Scoped while its consumers here are not: schema builders are " +
                "singletons and several of them constructor-inject IUserManagerCore, so a constructor-injected " +
                "cache resolves once from the root provider and then outlives every request - silently, wherever " +
                "ValidateScopes is off, leaving a locked or deleted account with a cached 'allowed' verdict until " +
                "restart. Depend on IRequestScopedCacheAccessor, which re-reads the ambient request's cache.");
        }
    }
}
