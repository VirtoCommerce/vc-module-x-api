using System.Linq;
using FluentAssertions;
using GraphQL.Types;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Schemas;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Schemas
{
    public class StorePluginTypeTests
    {
        [Fact]
        public void Contributions_ShouldBeANullableString()
        {
            // The platform serves the object opaquely; the SPA parses it, so the schema does not
            // model its shape and a plugin without contributions stays valid.
            var field = new StorePluginType().Fields.FirstOrDefault(x => x.Name.EqualsIgnoreCase("contributions"));

            field.Should().NotBeNull();
            // Before schema initialization a CLR-inferred field carries a type reference; not wrapped
            // in NonNullGraphType, it becomes a nullable String.
            field.Type.Should().Be<GraphQLClrOutputTypeReference<string>>();
        }
    }
}
