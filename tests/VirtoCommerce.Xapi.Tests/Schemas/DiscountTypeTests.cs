using System.Linq;
using System.Threading.Tasks;
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
        public async Task DiscountType_Coupon_ShouldResolve()
        {
            // Arrange
            var discount = GetDiscount();
            var resolveContext = new ResolveFieldContext
            {
                Source = discount
            };

            // Act
            var result = await _discountType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("Coupon")).Resolver.ResolveAsync(resolveContext);

            // Assert
            result.Should().BeOfType<string>();
            ((string)result).Should().Be(discount.Coupon);
        }

        [Fact]
        public async Task DiscountType_Description_ShouldResolve()
        {
            // Arrange
            var discount = GetDiscount();
            var resolveContext = new ResolveFieldContext
            {
                Source = discount
            };

            // Act
            var result = await _discountType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("Description")).Resolver.ResolveAsync(resolveContext);

            // Assert
            result.Should().BeOfType<string>();
            ((string)result).Should().Be(discount.Description);
        }

        [Fact]
        public async Task DiscountType_PromotionId_ShouldResolve()
        {
            // Arrange
            var discount = GetDiscount();
            var resolveContext = new ResolveFieldContext
            {
                Source = discount
            };

            // Act
            var result = await _discountType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("PromotionId")).Resolver.ResolveAsync(resolveContext);

            // Assert
            result.Should().BeOfType<string>();
            ((string)result).Should().Be(discount.PromotionId);
        }

        [Fact]
        public async Task DiscountType_Amount_ShouldResolve()
        {
            // Arrange
            var discount = GetDiscount();
            var resolveContext = new ResolveFieldContext
            {
                Source = discount
            };

            // Act
            var result = await _discountType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("Amount")).Resolver.ResolveAsync(resolveContext);

            // Assert
            result.Should().BeOfType<decimal>();
            ((decimal)result).Should().Be(discount.DiscountAmount);
        }

        [Fact]
        public async Task DiscountType_AmountWithTax_ShouldResolve()
        {
            // Arrange
            var discount = GetDiscount();
            var resolveContext = new ResolveFieldContext
            {
                Source = discount
            };

            // Act
            var result = await _discountType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("AmountWithTax")).Resolver.ResolveAsync(resolveContext);

            // Assert
            result.Should().BeOfType<decimal>();
            ((decimal)result).Should().Be(discount.DiscountAmountWithTax);
        }
    }
}
