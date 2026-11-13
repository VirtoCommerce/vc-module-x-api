using System;
using FluentAssertions;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;
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

    public class StringFieldsModel
    {
        [BindIndexField(FieldName = FieldA)]
        public string A { get; set; }

        [BindIndexField(FieldName = FieldB)]
        public string B { get; set; }

        public string NotBound { get; set; }
    }

    public class BindingBaseModel
    {
        [BindIndexField(FieldName = FieldA)]
        public string A { get; set; }
    }

    public class BindingDerivedModel : BindingBaseModel
    {
        [BindIndexField(FieldName = FieldB)]
        public string B { get; set; }
    }

    /// <summary>
    /// A binder reads its field name off its own instance, so two properties sharing a binder type must
    /// not share the instance: resolving one would retarget the other's field name, and a concurrent
    /// bind would then read a property from the wrong index field. Single-threaded: it asserts that the
    /// two properties get distinct binder instances, each with its own field name. The eight-thread
    /// reproduction of the original defect was run once by hand and is deliberately not in the suite.
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

    [Fact]
    public void BindIndexedProperties_TwoPropertiesSharingABinderType_BindsEachFromItsOwnField()
    {
        var first = new StringFieldsModel();
        var second = new StringFieldsModel();

        first.BindIndexedProperties(new SearchDocument { [FieldA] = "a", [FieldB] = "b" });
        second.BindIndexedProperties(new SearchDocument { [FieldA] = "b", [FieldB] = "a" });

        first.A.Should().Be("a");
        first.B.Should().Be("b");
        second.A.Should().Be("b");
        second.B.Should().Be("a");
    }

    [Fact]
    public void BindIndexedProperties_PropertyWithoutAttribute_IsLeftUntouched()
    {
        var model = new StringFieldsModel { NotBound = "keep" };

        model.BindIndexedProperties(new SearchDocument { [FieldA] = "a", [FieldB] = "b" });

        model.NotBound.Should().Be("keep");
    }

    /// <summary>
    /// Binders are cached per runtime type, not per the type the caller declared: a property an
    /// <c>AbstractTypeFactory</c> override adds must be bound too, and bound from its own field.
    /// </summary>
    [Fact]
    public void GenericModelBinder_TypeOverriddenInAbstractTypeFactory_BindsPropertiesTheOverrideAdds()
    {
        AbstractTypeFactory<BindingBaseModel>.OverrideType<BindingBaseModel, BindingDerivedModel>();

        var result = new GenericModelBinder<BindingBaseModel>().BindModel(new SearchDocument { [FieldA] = "a", [FieldB] = "b" });

        var derived = result.Should().BeOfType<BindingDerivedModel>().Subject;
        derived.A.Should().Be("a");
        derived.B.Should().Be("b");
    }

    [Fact]
    public void BindIndexedProperties_NullTarget_Throws()
    {
        object target = null;

        var act = () => target.BindIndexedProperties(new SearchDocument());

        act.Should().Throw<ArgumentNullException>();
    }
}
