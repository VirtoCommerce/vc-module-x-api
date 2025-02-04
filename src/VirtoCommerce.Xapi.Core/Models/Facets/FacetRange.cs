namespace VirtoCommerce.Xapi.Core.Models.Facets
{
    public sealed class FacetRange
    {
        public decimal? From { get; set; }
        public bool IncludeFrom { get; set; }
        public string FromStr { get; set; }

        public decimal? To { get; set; }
        public bool IncludeTo { get; set; }
        public string ToStr { get; set; }

        public long Count { get; set; }
        public long Total { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }

        public bool IsSelected { get; set; }
        public string Label { get; set; }
    }
}
