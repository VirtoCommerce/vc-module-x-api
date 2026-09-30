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
            var field = new StorePluginType().Fields.FirstOrDefault(x => x.Name.EqualsIgnoreCase("contributions"));

            field.Should().NotBeNull();
            // Before schema init, an unwrapped CLR reference means a nullable String.
            field.Type.Should().Be<GraphQLClrOutputTypeReference<string>>();
        }
    }
}
