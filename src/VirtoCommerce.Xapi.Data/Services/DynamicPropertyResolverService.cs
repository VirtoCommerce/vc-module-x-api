using System;
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
            ArgumentNullException.ThrowIfNull(entity);

            var entityDynamicProperties = await GetDynamicPropertyMetaData(entity);

            var result = entity.DynamicProperties?
                .SelectMany(x => x.Values.Select(v =>
                {
                    var clone = (DynamicPropertyObjectValue)v.Clone();
                    clone.PropertyName = GetLocalizedPropertyName(entityDynamicProperties, x.Name, cultureName);
                    return clone;
                }))
                ?? [];

            if (!cultureName.IsNullOrEmpty())
            {
                result = result.Where(x => x.Locale.IsNullOrEmpty() || x.Locale.EqualsInvariant(cultureName));
            }

            // find and add all the properties without values
            var emptyValues = ResolvePropertiesWithoutValues(entityDynamicProperties, entity, cultureName);

            return result.Union(emptyValues);
        }

        private static IEnumerable<DynamicPropertyObjectValue> ResolvePropertiesWithoutValues(IList<DynamicProperty> entityDynamicProperties, IHasDynamicProperties entity, string cultureName)
        {
            var entryDynamicProperties = entity.DynamicProperties ?? Enumerable.Empty<DynamicObjectProperty>();
            var existingDynamicProperties = entityDynamicProperties
                .Where(p => entryDynamicProperties.Any(x => x.Id == p.Id || x.Name.EqualsInvariant(p.Name)));
            var propertiesWithoutValue = entityDynamicProperties.Except(existingDynamicProperties);
            var emptyValues = propertiesWithoutValue.Select(CreateDynamicPropertyObjectValue(entity, cultureName));
            return emptyValues;
        }

        private async Task<IList<DynamicProperty>> GetDynamicPropertyMetaData(IHasDynamicProperties entity)
        {
            var criteria = AbstractTypeFactory<DynamicPropertySearchCriteria>.TryCreateInstance();
            criteria.ObjectType = entity.ObjectType;
            var searchResult = await _dynamicPropertySearchService.SearchAllNoCloneAsync(criteria);
            return searchResult;
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

        private static string GetLocalizedPropertyName(IList<DynamicProperty> entityDynamicProperties, string dynamicPropertyName, string cultureName)
        {
            if (string.IsNullOrEmpty(cultureName))
            {
                return dynamicPropertyName;
            }

            var dynamicProperty = entityDynamicProperties.FirstOrDefault(x => x.Name.EqualsInvariant(dynamicPropertyName));
            if (dynamicProperty == null)
            {
                return dynamicPropertyName;
            }

            return GetLocalizedPropertyName(dynamicProperty, cultureName);
        }

        private static string GetLocalizedPropertyName(DynamicProperty dynamicProperty, string cultureName)
        {
            ArgumentNullException.ThrowIfNull(dynamicProperty);

            if (string.IsNullOrEmpty(cultureName) || dynamicProperty.DisplayNames.IsNullOrEmpty())
            {
                return dynamicProperty.Name;
            }

            var localizedName = dynamicProperty.DisplayNames
                .FirstOrDefault(n => string.Equals(n.Locale, cultureName, StringComparison.InvariantCulture))
                ?.Name;

            return string.IsNullOrEmpty(localizedName) ? dynamicProperty.Name : localizedName;
        }
    }
}
