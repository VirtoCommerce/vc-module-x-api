using System;
using FluentAssertions;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Tests.Helpers;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Models
{
    public class ProductPriceTests : MoqHelper
    {
        private const decimal _listPrice = 100m;

        public static TheoryData<decimal, decimal> DiscountAmounts => new()
        {
            { 10m, 0.1m },
            { 12.5m, 0.125m },
            { 7.5m, 0.075m },
            { 33.33m, 0.3333m },
            { 0.5m, 0.005m },
        };

        [Theory]
        [MemberData(nameof(DiscountAmounts))]
        public void ProductPrice_DiscountPercent_ShouldBeRelativeToListPrice(decimal discountAmount, decimal expectedDiscountPercent)
        {
            // Arrange
            var productPrice = GetProductPrice(_listPrice, discountAmount);

            // Act
            var result = productPrice.DiscountPercent;

            // Assert
            result.Should().Be(expectedDiscountPercent);
        }

        [Fact]
        public void ProductPrice_DiscountPercent_ShouldBeZeroWhenListPriceZero()
        {
            // Arrange
            var productPrice = GetProductPrice(0, 10m);

            // Act
            var result = productPrice.DiscountPercent;

            // Assert
            result.Should().Be(0m);
        }

        private ProductPrice GetProductPrice(decimal listPrice, decimal discountAmount, Action<Currency> configureCurrency = null)
        {
            var currency = GetCurrency();
            configureCurrency?.Invoke(currency);

            return new ProductPrice(currency)
            {
                ListPrice = new Money(listPrice, currency),
                DiscountAmount = new Money(discountAmount, currency),
            };
        }
    }
}
