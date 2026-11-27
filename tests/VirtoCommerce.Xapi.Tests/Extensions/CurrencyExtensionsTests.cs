using FluentAssertions;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Currency;
using VirtoCommerce.Xapi.Core.Extensions;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Extensions;

public class CurrencyExtensionsTests
{
    // VCST-6824: the culture-specific copy must keep the rounding settings, otherwise a currency
    // without minor units (DecimalDigits = 0) is rounded and formatted with 2 digits in the cart
    [Theory]
    [InlineData(0, "Rounding001", "AwayFromZero")]
    [InlineData(2, "Rounding005Up", "ToZero")]
    [InlineData(3, "Rounding001", "ToEven")]
    public void GetCurrencyForLanguage_KeepsRoundingSettings(int decimalDigits, string roundingType, string midpointRounding)
    {
        // Arrange
        var currency = new Currency(new Language("en-US"), code: "TST", name: "Test", symbol: "T", exchangeRate: 1)
        {
            DecimalDigits = decimalDigits,
            RoundingType = roundingType,
            MidpointRounding = midpointRounding,
            CustomFormatting = "T #,##0.###",
            RoundingPolicy = new DefaultMoneyRoundingPolicy(),
        };

        // Act
        var result = new[] { currency }.GetCurrencyForLanguage("TST", "de-DE");

        // Assert
        result.DecimalDigits.Should().Be(decimalDigits);
        result.RoundingType.Should().Be(roundingType);
        result.MidpointRounding.Should().Be(midpointRounding);
        result.CustomFormatting.Should().Be(currency.CustomFormatting);
        result.RoundingPolicy.Should().BeSameAs(currency.RoundingPolicy);
        result.CultureName.Should().Be("de-DE");
    }

    [Fact]
    public void GetCurrencyForLanguage_ZeroDecimalDigits_MoneyIsRoundedToWholeUnits()
    {
        // Arrange
        var currency = new Currency(new Language("en-US"), code: "TST", name: "Test", symbol: "T", exchangeRate: 1)
        {
            DecimalDigits = 0,
            RoundingPolicy = new DefaultMoneyRoundingPolicy(),
        };

        // Act
        var result = new[] { currency }.GetCurrencyForLanguage("TST", "en-US");

        // Assert: 7.20 -> 7
        new Money(7.2m, result).Amount.Should().Be(7m);
    }
}
