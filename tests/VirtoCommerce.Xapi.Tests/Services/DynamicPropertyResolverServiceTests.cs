using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Xapi.Data.Services;
using VirtoCommerce.Xapi.Tests.Helpers.Stubs;
using Xunit;

namespace VirtoCommerce.Xapi.Tests.Services
{
    public class DynamicPropertyResolverServiceTests
    {
        [Theory]
        [MemberData(nameof(GetPropertyData))]
        public async Task LoadDynamicPropertyValuesTest(IHasDynamicProperties entity, List<DynamicPropertyObjectValue> expectedResults)
        {
            // Arrange
            var dynamicPropertySearchServiceMock = new Mock<IDynamicPropertySearchService>();
            dynamicPropertySearchServiceMock
                .Setup(x => x.SearchAsync(It.Is<DynamicPropertySearchCriteria>(y => y.ObjectType == entity.ObjectType), It.IsAny<bool>()))
                .Returns(Task.FromResult(new DynamicPropertySearchResult { Results = _properties }));

            // Act
            var target = new DynamicPropertyResolverService(dynamicPropertySearchServiceMock.Object);
            var result = await target.LoadDynamicPropertyValues(entity, "en-US");

            // Assert
            foreach (var expected in expectedResults)
            {
                result.Should().ContainSingle(x => x.PropertyName == expected.PropertyName);
            }
        }

        private static readonly DynamicProperty _property1 = new DynamicProperty
        {
            Id = "PropertyId_1",
            Name = "PropertyName_1",
        };

        private static readonly DynamicProperty _property2 = new DynamicProperty
        {
            Id = "PropertyId_2",
            Name = "PropertyName_2",
        };

        // properties meta data
        private static readonly List<DynamicProperty> _properties = new List<DynamicProperty> { _property1, _property2 };

        public static IEnumerable<object[]> GetPropertyData { get; } = new List<object[]>
        {
            // should return empty properties with metadata for an entity with no dynamic property values
            new object[]
            {
                // entity
                new StubDynamicPropertiesOwner(),
                // results
                new List<DynamicPropertyObjectValue>
                {
                    new DynamicPropertyObjectValue
                    {
                        PropertyId = _property1.Id,
                        PropertyName = _property1.Name
                    },
                    new DynamicPropertyObjectValue
                    {
                        PropertyId = _property2.Id,
                        PropertyName = _property2.Name
                    },
                },
            },

            // should compare properties by id
            new object[]
            {
                // entity
                new StubDynamicPropertiesOwner(new List<DynamicObjectProperty>
                {
                    new DynamicObjectProperty
                    {
                        Values = new List<DynamicPropertyObjectValue>
                        {
                            new DynamicPropertyObjectValue
                            {
                                PropertyId = _property1.Id
                            }
                        }
                    },
                    new DynamicObjectProperty
                    {
                        Values = new List<DynamicPropertyObjectValue>
                        {
                            new DynamicPropertyObjectValue
                            {
                                PropertyId = _property2.Id
                            }
                        }
                    },
                }),
                // results
                new List<DynamicPropertyObjectValue>
                {
                    new DynamicPropertyObjectValue
                    {
                        PropertyId = _property1.Id,
                        PropertyName = _property1.Name
                    },
                    new DynamicPropertyObjectValue
                    {
                        PropertyId = _property2.Id,
                        PropertyName = _property2.Name
                    },
                },
            },

            // should compare properties by name
            new object[]
            {
                // entity
                new StubDynamicPropertiesOwner(new List<DynamicObjectProperty>
                {
                    new DynamicObjectProperty
                    {
                        Name = _property1.Name,
                        Values = new List<DynamicPropertyObjectValue>
                        {
                            new DynamicPropertyObjectValue
                            {
                                PropertyId = Guid.NewGuid().ToString(),
                                PropertyName = _property1.Name
                            }
                        }
                    },
                    new DynamicObjectProperty
                    {
                        Name = _property2.Name,
                        Values = new List<DynamicPropertyObjectValue>
                        {
                            new DynamicPropertyObjectValue
                            {
                                PropertyId = Guid.NewGuid().ToString(),
                                PropertyName = _property2.Name
                            }
                        }
                    },
                }),
                // results
                new List<DynamicPropertyObjectValue>
                {
                    new DynamicPropertyObjectValue
                    {
                        PropertyId = _property1.Id,
                        PropertyName = _property1.Name
                    },
                    new DynamicPropertyObjectValue
                    {
                        PropertyId = _property2.Id,
                        PropertyName = _property2.Name
                    },
                },
            },
        };
    }
}
