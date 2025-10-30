using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Services;
using VirtoCommerce.Xapi.Core;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Models;
using VirtoCommerce.Xapi.Core.Queries;
using VirtoCommerce.Xapi.Core.Services;
using VirtoCommerce.Xapi.Core.Subscriptions;
using StoreSettingGeneral = VirtoCommerce.StoreModule.Core.ModuleConstants.Settings.General;
using StoreSettingSeo = VirtoCommerce.StoreModule.Core.ModuleConstants.Settings.SEO;

namespace VirtoCommerce.Xapi.Data.Queries;

public class GetStoreQueryHandler : IQueryHandler<GetStoreQuery, StoreResponse>
{
    private readonly IStoreService _storeService;
    private readonly IStoreSearchService _storeSearchService;
    private readonly IStoreCurrencyResolver _storeCurrencyResolver;
    private readonly ISettingsManager _settingsManager;
    private readonly IdentityOptions _identityOptions;
    private readonly GraphQLWebSocketOptions _webSocketOptions;
    private readonly StoresOptions _storeOptions;
    private readonly IStoreAuthenticationService _storeAuthenticationService;
    private readonly IStoreDomainResolverService _storeDomainResolverService;

    public GetStoreQueryHandler(
        IStoreService storeService,
        IStoreSearchService storeSearchService,
        IStoreCurrencyResolver storeCurrencyResolver,
        ISettingsManager settingsManager,
        IOptions<IdentityOptions> identityOptions,
        IOptions<GraphQLWebSocketOptions> webSocketOptions,
        IOptions<StoresOptions> storeOptions,
        IStoreAuthenticationService storeAuthenticationService,
        IStoreDomainResolverService storeDomainResolverService)
    {
        _storeService = storeService;
        _storeSearchService = storeSearchService;
        _storeCurrencyResolver = storeCurrencyResolver;
        _settingsManager = settingsManager;
        _storeAuthenticationService = storeAuthenticationService;
        _identityOptions = identityOptions.Value;
        _webSocketOptions = webSocketOptions.Value;
        _storeOptions = storeOptions.Value;
        _storeDomainResolverService = storeDomainResolverService;
    }

    public async Task<StoreResponse> Handle(GetStoreQuery request, CancellationToken cancellationToken)
    {
        var storeResolverRequest = CreateStoreResolveRequest(request);
        var store = await _storeDomainResolverService.GetStoreAsync(storeResolverRequest);

        if (store == null)
        {
            return null;
        }

        var availableLanguages = !store.Languages.IsNullOrEmpty() ? store.Languages.Select(x => new Language(x)).ToList() : [];

        var cultureName = GetCultureName(request.CultureName, store.DefaultLanguage, availableLanguages);

        var allCurrencies = await _storeCurrencyResolver.GetAllStoreCurrenciesAsync(store.Id, cultureName);
        var availableCurrencies = allCurrencies.Where(x => store.Currencies.Contains(x.Code)).ToList();
        var defaultCurrency = await _storeCurrencyResolver.GetStoreCurrencyAsync(store.DefaultCurrency, store.Id, cultureName);

        var defaultLanguage = store.DefaultLanguage != null ? new Language(store.DefaultLanguage) : Language.InvariantLanguage;

        var response = new StoreResponse
        {
            StoreId = store.Id,
            StoreName = store.Name,
            CatalogId = store.Catalog,
            StoreUrl = store.Url,
            DefaultCurrency = defaultCurrency,
            AvailableCurrencies = availableCurrencies,
            DefaultLanguage = defaultLanguage,
            AvailableLanguages = availableLanguages,
            GraphQLSettings = new GraphQLSettings
            {
                KeepAliveInterval = _webSocketOptions.KeepAliveInterval,
            }
        };

        if (!store.Settings.IsNullOrEmpty())
        {
            response.Settings = new StoreSettings
            {
#pragma warning disable VC0009
                IsSpa = store.Settings.GetValue<bool>(StoreSettingGeneral.IsSpa),
                QuotesEnabled = store.Settings.GetValue<bool>(new SettingDescriptor { Name = "Quotes.EnableQuotes" }),
                SubscriptionEnabled = store.Settings.GetValue<bool>(new SettingDescriptor { Name = "Subscription.EnableSubscriptions" }),
#pragma warning restore VC0009
#pragma warning disable VC0010
                CreateAnonymousOrderEnabled = store.Settings.GetValue<bool>(new SettingDescriptor { Name = "XOrder.CreateAnonymousOrderEnabled" }),
                DefaultSelectedForCheckout = store.Settings.GetValue<bool>(new SettingDescriptor { Name = "XPurchase.IsSelectedForCheckout" }),
#pragma warning restore VC0010
                TaxCalculationEnabled = store.Settings.GetValue<bool>(StoreSettingGeneral.TaxCalculationEnabled),
                AnonymousUsersAllowed = store.Settings.GetValue<bool>(StoreSettingGeneral.AllowAnonymousUsers),
                EmailVerificationEnabled = store.Settings.GetValue<bool>(StoreSettingGeneral.EmailVerificationEnabled),
                EmailVerificationRequired = store.Settings.GetValue<bool>(StoreSettingGeneral.EmailVerificationRequired),
                SeoLinkType = store.Settings.GetValue<string>(StoreSettingSeo.SeoLinksType),

                EnvironmentName = _settingsManager.GetValue<string>(ModuleConstants.Settings.General.EnvironmentName),
                PasswordRequirements = _identityOptions.Password,

                AuthenticationTypes = (await _storeAuthenticationService.GetStoreSchemesAsync(store.Id))
                    .Where(x => x.IsActive)
                    .Select(x => x.Name)
                    .ToList(),

                Modules = ToModulesSettings(store.Settings)
            };
        }

        return response;
    }

    private static string GetCultureName(string cultureName, string defaultCultureName, List<Language> availableLanguages)
    {
        if (cultureName.IsNullOrEmpty())
        {
            cultureName = defaultCultureName;
        }
        else if (cultureName.Length == 2)
        {
            cultureName = availableLanguages.FirstOrDefault(x => cultureName == x.TwoLetterLanguageName)?.CultureName;
            cultureName ??= defaultCultureName;
        }

        return cultureName;
    }

    protected virtual StoreDomainRequest CreateStoreResolveRequest(GetStoreQuery request)
    {
        var storeResolverRequest = AbstractTypeFactory<StoreDomainRequest>.TryCreateInstance();
        storeResolverRequest.StoreId = request.StoreId;
        storeResolverRequest.Domain = request.Domain;
        return storeResolverRequest;
    }

    [Obsolete("Not being called anymore. Use IStoreDomainResolverService.ResolveStoreByDomain(string domain) method.", DiagnosticId = "VC0010", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
    protected virtual async Task<Store> ResolveStoreByDomain(string domain)
    {
        if (_storeOptions.Domains.TryGetValue(domain, out var storeId))
        {
            return await _storeService.GetByIdAsync(storeId, clone: false);
        }

        var criteria = AbstractTypeFactory<StoreSearchCriteria>.TryCreateInstance();
        criteria.Domain = domain;
        criteria.Take = 1;

        var result = await _storeSearchService.SearchAsync(criteria, clone: false);
        return result.Results.FirstOrDefault();
    }

    protected virtual ModuleSettings[] ToModulesSettings(ICollection<ObjectSettingEntry> settings)
    {
        var result = new List<ModuleSettings>();

        foreach (var settingByModule in settings.Where(s => s.IsPublic).GroupBy(s => s.ModuleId))
        {
            var moduleSettings = new ModuleSettings
            {
                ModuleId = settingByModule.Key,
                Settings = settingByModule.Select(s => new ModuleSetting
                {
                    Name = s.Name,
                    Value = ToSettingValue(s)
                }).ToArray(),
            };

            if (moduleSettings.Settings.Length > 0)
            {
                result.Add(moduleSettings);
            }
        }

        return [.. result];
    }

    protected virtual object ToSettingValue(ObjectSettingEntry s)
    {
        if (s.IsDictionary)
        {
            return JsonConvert.SerializeObject(s.AllowedValues ?? []);
        }
        else
        {
            var result = s.Value ?? s.DefaultValue;

            if (result == null)
            {
                switch (s.ValueType)
                {
                    case SettingValueType.Boolean:
                        result = false;
                        break;
                }
            }
            return result;
        }
    }
}
