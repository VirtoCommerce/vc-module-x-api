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
    public class MoneyTypeTests : MoqHelper
    {
        private readonly MoneyType _moneyType = new();

        [Fact]
        public void MoneyType_ShouldHaveProperFieldAmount()
        {
            // Assert
            _moneyType.Fields.Should().HaveCount(7);
        }

        [Fact]
        public async Task MoneyType_Amount_ShouldResolve()
        {
            // Arrange
            var money = GetMoney();
            var resolveContext = new ResolveFieldContext()
            {
                Source = money
            };

            // Act
            var result = await _moneyType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("Amount")).Resolver.ResolveAsync(resolveContext);

            // Assert
            result.Should().BeOfType<decimal>();
            ((decimal)result).Should().Be(money.Amount);
        }

        [Fact]
        public async Task MoneyType_DecimalDigits_ShouldResolve()
        {
            // Arrange
            var money = GetMoney();
            var resolveContext = new ResolveFieldContext()
            {
                Source = money
            };

            // Act
            var result = await _moneyType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("DecimalDigits")).Resolver.ResolveAsync(resolveContext);

            // Assert
            result.Should().BeOfType<int>();
            ((int)result).Should().Be(money.DecimalDigits);
        }

        [Fact]
        public async Task MoneyType_FormattedAmount_ShouldResolve()
        {
            // Arrange
            var money = GetMoney();
            var resolveContext = new ResolveFieldContext()
            {
                Source = money
            };

            // Act
            var result = await _moneyType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("FormattedAmount")).Resolver.ResolveAsync(resolveContext);

            // Assert
            result.Should().BeOfType<string>();
            ((string)result).Should().Be(money.FormattedAmount);
        }

        [Fact]
        public async Task MoneyType_FormattedAmountWithoutCurrency_ShouldResolve()
        {
            // Arrange
            var money = GetMoney();
            var resolveContext = new ResolveFieldContext()
            {
                Source = money
            };

            // Act
            var result = await _moneyType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("FormattedAmountWithoutCurrency")).Resolver.ResolveAsync(resolveContext);

            // Assert
            result.Should().BeOfType<string>();
            ((string)result).Should().Be(money.FormattedAmountWithoutCurrency);
        }

        [Fact]
        public async Task MoneyType_FormattedAmountWithoutPoint_ShouldResolve()
        {
            // Arrange
            var money = GetMoney();
            var resolveContext = new ResolveFieldContext()
            {
                Source = money
            };

            // Act
            var result = await _moneyType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("FormattedAmountWithoutPoint")).Resolver.ResolveAsync(resolveContext);

            // Assert
            result.Should().BeOfType<string>();
            ((string)result).Should().Be(money.FormattedAmountWithoutPoint);
        }

        [Fact]
        public async Task MoneyType_FormattedAmountWithoutPointAndCurrency_ShouldResolve()
        {
            // Arrange
            var money = GetMoney();
            var resolveContext = new ResolveFieldContext()
            {
                Source = money
            };

            // Act
            var result = await _moneyType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("FormattedAmountWithoutPointAndCurrency")).Resolver.ResolveAsync(resolveContext);

            // Assert
            result.Should().BeOfType<string>();
            ((string)result).Should().Be(money.FormattedAmountWithoutPointAndCurrency);
        }
    }
}
