namespace VirtoCommerce.Xapi.Core.Models;

public class GraphQLComplexetyValidationOptions
{
    public bool Enable { get; set; }

    /// <summary>
    /// Ignore introspection fields during complexety validation
    /// </summary>
    public bool IgnoreIntrospection { get; set; }

    /// <summary>
    /// Default GraphQL.NET value is null
    /// /// </summary>
    public int? MaxDepth { get; set; }

    /// <summary>
    /// Default GraphQL.NET value is null
    /// </summary>
    public int? MaxComplexity { get; set; }

    /// <summary>
    /// Default GraphQL.NET value is 1
    /// </summary>
    public double? ScalarFieldImpact { get; set; }

    /// <summary>
    /// Default GraphQL.NET value is 1
    /// </summary>
    public double? ObjectFieldImpact { get; set; }

    /// <summary>
    /// Default GraphQL.NET value is 20
    /// </summary>
    public double? ListImpactMultiplied { get; set; }
}
