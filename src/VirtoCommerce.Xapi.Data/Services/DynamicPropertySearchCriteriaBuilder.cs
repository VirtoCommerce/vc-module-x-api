using System;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using VirtoCommerce.Xapi.Core.Index;

namespace VirtoCommerce.Xapi.Data.Services
{
    public class DynamicPropertySearchCriteriaBuilder
    {
        private readonly ISearchPhraseParser _phraseParser;
        private readonly DynamicPropertySearchCriteria _searchCriteria;

        public DynamicPropertySearchCriteriaBuilder(ISearchPhraseParser phraseParser) : this()
        {
            _phraseParser = phraseParser;
        }

        public DynamicPropertySearchCriteriaBuilder()
        {
            _searchCriteria = AbstractTypeFactory<DynamicPropertySearchCriteria>.TryCreateInstance();
        }

        public virtual DynamicPropertySearchCriteria Build()
        {
            return _searchCriteria.Clone() as DynamicPropertySearchCriteria;
        }

        public DynamicPropertySearchCriteriaBuilder ParseFilters(string filterPhrase)
        {
            if (filterPhrase == null)
            {
                return this;
            }
            if (_phraseParser == null)
            {
                throw new OperationCanceledException("phrase parser must be set");
            }

            var parseResult = _phraseParser.Parse(filterPhrase);

            foreach (var term in parseResult.Filters.OfType<TermFilter>())
            {
                term.MapTo(_searchCriteria);
            }

            return this;
        }

        public DynamicPropertySearchCriteriaBuilder WithLanguage(string language)
        {
            _searchCriteria.LanguageCode = language ?? _searchCriteria.LanguageCode;
            return this;
        }

        public DynamicPropertySearchCriteriaBuilder WithPaging(int skip, int take)
        {
            _searchCriteria.Skip = skip;
            _searchCriteria.Take = take;
            return this;
        }

        public DynamicPropertySearchCriteriaBuilder WithSorting(string sort)
        {
            _searchCriteria.Sort = sort ?? _searchCriteria.Sort;
            return this;
        }

        public DynamicPropertySearchCriteriaBuilder WithObjectType(string objectType)
        {
            _searchCriteria.ObjectType = objectType ?? _searchCriteria.ObjectType;
            return this;
        }
    }
}
