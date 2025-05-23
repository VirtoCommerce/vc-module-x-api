using FluentAssertions;
using GraphQL.Types;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Helpers;
using Xunit;

namespace VirtoCommerce.Xapi.Tests
{
    public class GraphTypeExtenstionHelperTests
    {
        [Fact]
        public void GetActualType_HasOverriddenType_OverriddenTypeReturned()
        {
            // Arrange
            AbstractTypeFactory<IGraphType>.OverrideType<FooType, FooTypeExtended>();

            // Act
            var targetType = GraphTypeExtensionHelper.GetActualType<FooType>();

            // Assert
            targetType.Name.Should().Be(nameof(FooTypeExtended));
        }

        [Fact]
        public void GetComplexType_HasOverriddenType_OverriddenTypeReturned()
        {
            // Arrange
            AbstractTypeFactory<IGraphType>.OverrideType<FooType, FooTypeExtended>();

            // Act
            var targetType = GraphTypeExtensionHelper.GetActualComplexType<FooType>();

            // Assert
            targetType.Name.Should().Be(nameof(FooTypeExtended));
        }

        [Fact]
        public void GetComplexTypeTwoLevels_HasOverriddenType_OverriddenTypeReturned()
        {
            // Arrange
            AbstractTypeFactory<IGraphType>.OverrideType<FooType, FooTypeExtended>();

            // Act
            var targetType = GraphTypeExtensionHelper.GetActualComplexType<FooComplex<FooType>>();

            // Assert
            typeof(FooComplex<FooType>).GenericTypeArguments.Should().OnlyContain(x => x.Name.EqualsIgnoreCase(nameof(FooType)));
            targetType.GenericTypeArguments.Should().OnlyContain(x => x.Name.EqualsIgnoreCase(nameof(FooTypeExtended)));
        }

        [Fact]
        public void GetComplexTypeThreeLevels_HasOverriddenType_OverriddenTypeReturned()
        {
            // Arrange
            AbstractTypeFactory<IGraphType>.OverrideType<FooType, FooTypeExtended>();

            // Act
            var targetType = GraphTypeExtensionHelper.GetActualComplexType<FooComplex2<FooComplex<FooType>>>();

            // Assert
            typeof(FooComplex2<FooComplex<FooType>>).GenericTypeArguments[0].GenericTypeArguments.Should().OnlyContain(x => x.Name.EqualsIgnoreCase(nameof(FooType)));
            targetType.GenericTypeArguments[0].GenericTypeArguments.Should().OnlyContain(x => x.Name.EqualsIgnoreCase(nameof(FooTypeExtended)));
        }

        public class FooType : GraphType
        {
        }

        public class FooTypeExtended : FooType
        {
        }

        public class FooComplex : GraphType
        {
        }

        public class FooComplex<T> : FooComplex where T : GraphType
        {
        }

        public class FooComplex2 : GraphType
        {
        }

        public class FooComplex2<T> : FooComplex where T : GraphType
        {
        }
    }
}
