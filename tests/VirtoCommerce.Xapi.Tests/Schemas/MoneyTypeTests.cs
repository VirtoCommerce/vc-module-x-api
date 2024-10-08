using System.Linq;
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
        public void MoneyType_Amount_ShouldResolve()
        {
            // Arrange
            var money = GetMoney();
            var resolveContext = new ResolveFieldContext()
            {
                Source = money
            };

            // Act
            var result = _moneyType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("Amount")).Resolver.Resolve(resolveContext);

            // Assert
            result.Should().BeOfType<decimal>();
            ((decimal)result).Should().Be(money.Amount);
        }

        [Fact]
        public void MoneyType_DecimalDigits_ShouldResolve()
        {
            // Arrange
            var money = GetMoney();
            var resolveContext = new ResolveFieldContext()
            {
                Source = money
            };

            // Act
            var result = _moneyType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("DecimalDigits")).Resolver.Resolve(resolveContext);

            // Assert
            result.Should().BeOfType<int>();
            ((int)result).Should().Be(money.DecimalDigits);
        }

        [Fact]
        public void MoneyType_FormattedAmount_ShouldResolve()
        {
            // Arrange
            var money = GetMoney();
            var resolveContext = new ResolveFieldContext()
            {
                Source = money
            };

            // Act
            var result = _moneyType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("FormattedAmount")).Resolver.Resolve(resolveContext);

            // Assert
            result.Should().BeOfType<string>();
            ((string)result).Should().Be(money.FormattedAmount);
        }

        [Fact]
        public void MoneyType_FormattedAmountWithoutCurrency_ShouldResolve()
        {
            // Arrange
            var money = GetMoney();
            var resolveContext = new ResolveFieldContext()
            {
                Source = money
            };

            // Act
            var result = _moneyType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("FormattedAmountWithoutCurrency")).Resolver.Resolve(resolveContext);

            // Assert
            result.Should().BeOfType<string>();
            ((string)result).Should().Be(money.FormattedAmountWithoutCurrency);
        }

        [Fact]
        public void MoneyType_FormattedAmountWithoutPoint_ShouldResolve()
        {
            // Arrange
            var money = GetMoney();
            var resolveContext = new ResolveFieldContext()
            {
                Source = money
            };

            // Act
            var result = _moneyType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("FormattedAmountWithoutPoint")).Resolver.Resolve(resolveContext);

            // Assert
            result.Should().BeOfType<string>();
            ((string)result).Should().Be(money.FormattedAmountWithoutPoint);
        }

        [Fact]
        public void MoneyType_FormattedAmountWithoutPointAndCurrency_ShouldResolve()
        {
            // Arrange
            var money = GetMoney();
            var resolveContext = new ResolveFieldContext()
            {
                Source = money
            };

            // Act
            var result = _moneyType.Fields.FirstOrDefault(x => x.Name.EqualsInvariant("FormattedAmountWithoutPointAndCurrency")).Resolver.Resolve(resolveContext);

            // Assert
            result.Should().BeOfType<string>();
            ((string)result).Should().Be(money.FormattedAmountWithoutPointAndCurrency);
        }
    }
}
