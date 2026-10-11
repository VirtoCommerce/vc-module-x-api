using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Xapi.Core.Models;

namespace VirtoCommerce.Xapi.Core.Services;

/// <summary>
/// Facade for the handful of conversions used directly by the x-api module itself
/// (as opposed to conversions owned by a specific X-module, e.g. <see cref="IFacetMapper"/>).
/// </summary>
public interface IXapiMapper
{
    /// <returns>Null if <paramref name="source"/> is null.</returns>
    ExpVendor ToExpVendor(Member source);
}
