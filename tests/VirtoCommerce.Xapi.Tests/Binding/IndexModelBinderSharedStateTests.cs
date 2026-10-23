using System.Linq;
using FluentAssertions;
using VirtoCommerce.Xapi.Core.Binding;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Binding;

public class IndexModelBinderSharedStateTests
{
    private const string FieldA = "field_a";
    private const string FieldB = "field_b";

    public class TwoFieldsModel
    {
        [BindIndexField(FieldName = FieldA, BinderType = typeof(DefaultPropertyIndexBinder))]
        public object A { get; set; }

        [BindIndexField(FieldName = FieldB, BinderType = typeof(DefaultPropertyIndexBinder))]
        public object B { get; set; }

        public object NotBound { get; set; }
    }

    /// <summary>
    /// A binder reads its field name off its own instance, so two properties sharing a binder type must
    /// not share the instance: resolving one would retarget the other's field name, and a concurrent
    /// bind would then read a property from the wrong index field. Caching per binder type did exactly
    /// that — measured as one property receiving another's value under eight threads.
    /// </summary>
    [Fact]
    public void GetIndexModelBinder_TwoPropertiesSharingABinderType_KeepsTheirFieldNamesIndependent()
    {
        var propertyA = typeof(TwoFieldsModel).GetProperty(nameof(TwoFieldsModel.A));
        var propertyB = typeof(TwoFieldsModel).GetProperty(nameof(TwoFieldsModel.B));

        var binderA = propertyA.GetIndexModelBinder();
        var binderB = propertyB.GetIndexModelBinder();

        binderB.Should().NotBeSameAs(binderA);
        binderA.BindingInfo.FieldName.Should().Be(FieldA);
        binderB.BindingInfo.FieldName.Should().Be(FieldB);
    }

    /// <summary>
    /// The caller owns what it gets back. Handing out a shared instance would put a public setter on
    /// process-wide state — a smaller copy of the defect this class exists to pin down.
    /// </summary>
    [Fact]
    public void GetIndexModelBinder_SameProperty_ReturnsAnInstanceTheCallerOwns()
    {
        var property = typeof(TwoFieldsModel).GetProperty(nameof(TwoFieldsModel.A));

        var first = property.GetIndexModelBinder();
        var second = property.GetIndexModelBinder();

        second.Should().NotBeSameAs(first);

        first.BindingInfo.FieldName = "rewritten";

        property.GetIndexModelBinder().BindingInfo.FieldName.Should().Be(FieldA);
    }

    /// <summary>
    /// The type-level cache is the whole optimization: applying the "an instance the caller owns" rule
    /// above to this path too would silently restore per-document binder construction, with every test
    /// still green.
    /// </summary>
    [Fact]
    public void GetBoundProperties_CalledTwice_ReusesTheSameBinders()
    {
        var first = typeof(TwoFieldsModel).GetBoundProperties();
        var second = typeof(TwoFieldsModel).GetBoundProperties();

        second.Select(x => x.Binder).Should().Equal(first.Select(x => x.Binder));
    }

    /// <summary>
    /// Callers bind every returned pair unguarded, so a property without a binder must never appear:
    /// its null binder would throw on the first document of every request.
    /// </summary>
    [Fact]
    public void GetBoundProperties_ExcludesPropertiesWithoutABinder()
    {
        var boundProperties = typeof(TwoFieldsModel).GetBoundProperties();

        boundProperties.Select(x => x.Property.Name).Should().BeEquivalentTo([nameof(TwoFieldsModel.A), nameof(TwoFieldsModel.B)]);
        boundProperties.Should().OnlyContain(x => x.Binder != null);
    }

    /// <summary>
    /// The pair carries the binder the property resolves to, so the caller never looks it up a second time.
    /// </summary>
    [Fact]
    public void GetBoundProperties_PairsEachPropertyWithItsOwnBinder()
    {
        var boundProperties = typeof(TwoFieldsModel).GetBoundProperties();

        foreach (var boundProperty in boundProperties)
        {
            boundProperty.Binder.BindingInfo.FieldName.Should().Be(boundProperty.Property.Name == nameof(TwoFieldsModel.A) ? FieldA : FieldB);
        }
    }

    [Fact]
    public void GetBoundProperties_TypeWithNoAttributedProperty_ReturnsEmpty()
    {
        typeof(BindingInfo).GetBoundProperties().Should().BeEmpty();
    }
}
