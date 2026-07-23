using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using GraphQL.Types;
using MediatR;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Data.Extensions;
using Xunit;

namespace VirtoCommerce.Xapi.Tests
{
    public class GraphQLSingletonMediatorInjectionTests
    {
        [Fact]
        public void GraphQLSingletonTypes_ShouldNotCtorInjectMediator()
        {
            var assemblies = new[]
            {
                typeof(ISchemaBuilder).Assembly, // VirtoCommerce.Xapi.Core
                typeof(ServiceCollectionExtensions).Assembly, // VirtoCommerce.Xapi.Data
            };

            var offendingConstructors =
                from assembly in assemblies
                from type in assembly.GetTypes()
                where !type.IsAbstract && (typeof(IGraphType).IsAssignableFrom(type) || typeof(ISchemaBuilder).IsAssignableFrom(type))
                from ctor in type.GetConstructors()
                where ctor.GetCustomAttribute<ObsoleteAttribute>() is null
                where ctor.GetParameters().Any(parameter => parameter.ParameterType == typeof(IMediator))
                select $"{type.FullName}({string.Join(", ", ctor.GetParameters().Select(parameter => parameter.ParameterType.Name))})";

            offendingConstructors.Should().BeEmpty(
                "GraphQL types and schema builders are singletons (built once with the schema); a non-obsolete " +
                "constructor-injected IMediator would be captured against the root service provider and break (or silently " +
                "un-scope) any Scoped dependency of the handlers it dispatches to - resolve it per request instead via " +
                "IResolveFieldContext.GetMediator().");
        }
    }
}
