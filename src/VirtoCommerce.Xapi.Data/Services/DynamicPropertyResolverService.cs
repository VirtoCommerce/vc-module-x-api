using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Xapi.Core.Services;

namespace VirtoCommerce.Xapi.Data.Services
{
    public class DynamicPropertyResolverService : IDynamicPropertyResolverService
    {
        private readonly IDynamicPropertySearchService _dynamicPropertySearchService;

        public DynamicPropertyResolverService(IDynamicPropertySearchService dynamicPropertySearchService)
        {
            _dynamicPropertySearchService = dynamicPropertySearchService;
        }

        /// <summary>
        /// Load the dynamic property values for an entity. Include empty meta-data for missing values.
        /// </summary>
        /// <returns>Loaded Dynamic Property Values for specified entity</returns>
        public async Task<IEnumerable<DynamicPropertyObjectValue>> LoadDynamicPropertyValues(IHasDynamicProperties entity, string cultureName)
        {
            var result = entity.DynamicProperties?
                .SelectMany(x => x.Values.Select(v =>
                {
                    var clone = (DynamicPropertyObjectValue)v.Clone();
                    clone.PropertyName = GetLocalizedPropertyName(x, cultureName);
                    return clone;
                }))
                ?? [];

            if (!cultureName.IsNullOrEmpty())
            {
                result = result.Where(x => x.Locale.IsNullOrEmpty() || x.Locale.EqualsInvariant(cultureName));
            }

            // find and add all the properties without values
            var emptyValues = await ResolvePropertiesWithoutValues(entity, cultureName);

            return result.Union(emptyValues);
        }

        private async Task<IEnumerable<DynamicPropertyObjectValue>> ResolvePropertiesWithoutValues(IHasDynamicProperties entity, string cultureName)
        {
            var criteria = AbstractTypeFactory<DynamicPropertySearchCriteria>.TryCreateInstance();
            criteria.ObjectType = entity.ObjectType;
            var searchResult = await _dynamicPropertySearchService.SearchAllNoCloneAsync(criteria);

            var entryDynamicProperties = entity.DynamicProperties ?? Enumerable.Empty<DynamicObjectProperty>();
            var existingDynamicProperties = searchResult
                .Where(p => entryDynamicProperties.Any(x => x.Id == p.Id || x.Name.EqualsInvariant(p.Name)));
            var propertiesWithoutValue = searchResult.Except(existingDynamicProperties);
            var emptyValues = propertiesWithoutValue.Select(CreateDynamicPropertyObjectValue(entity, cultureName));
            return emptyValues;
        }

        private static System.Func<DynamicProperty, DynamicPropertyObjectValue> CreateDynamicPropertyObjectValue(IHasDynamicProperties entity, string cultureName) =>
            x => new DynamicPropertyObjectValue
            {
                ObjectId = entity.Id,
                ObjectType = entity.ObjectType,
                PropertyId = x.Id,
                PropertyName = GetLocalizedPropertyName(x, cultureName),
                ValueType = x.ValueType
            };

        private static string GetLocalizedPropertyName(DynamicProperty dynamicProperty, string cultureName)
        {
            if (string.IsNullOrEmpty(cultureName) || dynamicProperty.DisplayNames.IsNullOrEmpty())
            {
                return dynamicProperty.Name;
            }

            var localizedName = dynamicProperty.DisplayNames
                .FirstOrDefault(n => string.Equals(n.Locale, cultureName, System.StringComparison.InvariantCulture))
                ?.Name;

            return string.IsNullOrEmpty(localizedName) ? dynamicProperty.Name : localizedName;
        }
    }
}
