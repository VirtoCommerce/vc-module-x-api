using System;
using System.Collections.Generic;
using FluentAssertions;
using GraphQL.Introspection;
using GraphQL.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Queries;
using VirtoCommerce.Xapi.Core.Schemas;
using VirtoCommerce.Xapi.Data.Queries;
using VirtoCommerce.Xapi.Data.Schemas;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Infrastructure
{
    public class ScopedSchemaFactoryTests
    {
        // The marker types come from the Data and Core assemblies, the test builders are declared in the test assembly,
        // so a test builder is always "outside" the scoped assembly and reaches it only through its inheritance chain.

        // Subclass of a concrete builder declared in the Data assembly, like a client override registered via OverrideSchemaBuilder.
        private sealed class ClientCoreSchema : CoreSchema
        {
        }

        // Derives only from abstract builder bases declared in the Core assembly.
        private sealed class TestQueryBuilder : QueryBuilder<GetStoreQuery, StoreResponse, StoreResponseType>
        {
            protected override string Name => "test";

            public TestQueryBuilder(IAuthorizationService authorizationService)
                : base(authorizationService)
            {
            }
        }

        private sealed class TestSchemaBuilder : ISchemaBuilder
        {
            public void Build(ISchema schema)
            {
            }
        }

        // Exposes the scope filter on demand and keeps every builder out of the schema build,
        // so the result does not depend on when the base class builds its schema.
        private sealed class ProbeScopedSchemaFactory<TMarker> : ScopedSchemaFactory<TMarker>
        {
            public ProbeScopedSchemaFactory(IEnumerable<ISchemaBuilder> schemaBuilders)
                : base(schemaBuilders, new ServiceCollection().BuildServiceProvider(), Mock.Of<ISchemaFilter>())
            {
            }

            public List<ISchemaBuilder> ApplyScope() => base.GetSchemaBuilders();

            protected override List<ISchemaBuilder> GetSchemaBuilders()
            {
                return [];
            }
        }

        private static List<ISchemaBuilder> GetScopedBuilders<TMarker>(params ISchemaBuilder[] builders)
        {
            return new ProbeScopedSchemaFactory<TMarker>(builders).ApplyScope();
        }

        [Fact]
        public void GetSchemaBuilders_SubclassOutsideMarkerAssemblyOfConcreteBuilderInMarkerAssembly_KeepsBuilder()
        {
            var builder = new ClientCoreSchema();

            var result = GetScopedBuilders<CoreSchema>(builder);

            result.Should().ContainSingle().Which.Should().BeSameAs(builder);
        }

        [Fact]
        public void GetSchemaBuilders_ConcreteBuilderInMarkerAssembly_KeepsBuilder()
        {
            var builder = new CoreSchema();

            var result = GetScopedBuilders<CoreSchema>(builder);

            result.Should().ContainSingle().Which.Should().BeSameAs(builder);
        }

        [Fact]
        public void GetSchemaBuilders_BuilderOutsideMarkerAssemblyDerivingOnlyFromAbstractBaseInMarkerAssembly_DropsBuilder()
        {
            var builder = new TestQueryBuilder(Mock.Of<IAuthorizationService>());

            var result = GetScopedBuilders<SchemaFactory>(builder);

            result.Should().BeEmpty();
        }

        [Fact]
        public void GetSchemaBuilders_BuilderWithoutMarkerAssemblyAncestor_DropsBuilder()
        {
            var result = GetScopedBuilders<CoreSchema>(new TestSchemaBuilder(), new TestQueryBuilder(Mock.Of<IAuthorizationService>()));

            result.Should().BeEmpty();
        }

        [Fact]
        public void GetSchemaBuilders_MixedBuilders_KeepsOnlyMarkerAssemblyDescendants()
        {
            var subclass = new ClientCoreSchema();
            var concrete = new GetStoreQueryBuilder(Mock.Of<IAuthorizationService>());

            var result = GetScopedBuilders<CoreSchema>(new TestSchemaBuilder(), subclass, concrete);

            result.Should().Equal(subclass, concrete);
        }
    }
}
