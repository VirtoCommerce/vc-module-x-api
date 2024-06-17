using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Queries.BaseQueries;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.Xapi.Core.Queries;

public abstract class LocalizedSettingQueryHandler<TQuery> : IQueryHandler<TQuery, LocalizedSettingResponse>
    where TQuery : LocalizedSettingQuery
{
    private readonly ILocalizableSettingService _localizableSettingService;

    protected LocalizedSettingQueryHandler(ILocalizableSettingService localizableSettingService)
    {
        _localizableSettingService = localizableSettingService;
    }

    protected abstract SettingDescriptor Setting { get; }

    public async Task<LocalizedSettingResponse> Handle(TQuery request, CancellationToken cancellationToken)
    {
        var result = AbstractTypeFactory<LocalizedSettingResponse>.TryCreateInstance();
        result.Items = await _localizableSettingService.GetValuesAsync(Setting.Name, request.CultureName);

        return result;
    }
}
