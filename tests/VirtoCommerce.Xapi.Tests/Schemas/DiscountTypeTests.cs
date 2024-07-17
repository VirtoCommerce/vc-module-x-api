using System.Linq;
using FluentAssertions;
using GraphQL;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Xapi.Core.Schemas;
using VirtoCommerce.Xapi.Tests.Helpers;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Schemas
{
    public class DiscountTypeTests : MoqHelper
    {
        private readonly DiscountType _discountType = new();

        [Fact]
        public void DiscountType_Coupon_ShouldResolve()
        {
            // Arrange
            var discount = GetDiscount();
            var resolveContext = new ResolveFieldContext
            {
                Source = discount
            };

            // Act
            var result = _discountType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("Coupon")).Resolver.Resolve(resolveContext);

            // Assert
            result.Should().BeOfType<string>();
            ((string)result).Should().Be(discount.Coupon);
        }

        [Fact]
        public void DiscountType_Description_ShouldResolve()
        {
            // Arrange
            var discount = GetDiscount();
            var resolveContext = new ResolveFieldContext
            {
                Source = discount
            };

            // Act
            var result = _discountType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("Description")).Resolver.Resolve(resolveContext);

            // Assert
            result.Should().BeOfType<string>();
            ((string)result).Should().Be(discount.Description);
        }

        [Fact]
        public void DiscountType_PromotionId_ShouldResolve()
        {
            // Arrange
            var discount = GetDiscount();
            var resolveContext = new ResolveFieldContext
            {
                Source = discount
            };

            // Act
            var result = _discountType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("PromotionId")).Resolver.Resolve(resolveContext);

            // Assert
            result.Should().BeOfType<string>();
            ((string)result).Should().Be(discount.PromotionId);
        }

        [Fact]
        public void DiscountType_Amount_ShouldResolve()
        {
            // Arrange
            var discount = GetDiscount();
            var resolveContext = new ResolveFieldContext
            {
                Source = discount
            };

            // Act
            var result = _discountType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("Amount")).Resolver.Resolve(resolveContext);

            // Assert
            result.Should().BeOfType<decimal>();
            ((decimal)result).Should().Be(discount.DiscountAmount);
        }

        [Fact]
        public void DiscountType_AmountWithTax_ShouldResolve()
        {
            // Arrange
            var discount = GetDiscount();
            var resolveContext = new ResolveFieldContext
            {
                Source = discount
            };

            // Act
            var result = _discountType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("AmountWithTax")).Resolver.Resolve(resolveContext);

            // Assert
            result.Should().BeOfType<decimal>();
            ((decimal)result).Should().Be(discount.DiscountAmountWithTax);
        }
    }
}
