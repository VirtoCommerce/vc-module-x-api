using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.Xapi.Core
{
    public static class ModuleConstants
    {
        public static class Connections
        {
            public const int DefaultPageSize = 20;
        }

        public static class Parameters
        {
            public const string StoreId = "storeId";
        }

        public static class AnonymousUser
        {
            public static string UserName => "Anonymous";
        }

        public static class Settings
        {
            public static class General
            {
                public static SettingDescriptor EnvironmentName { get; } = new()
                {
                    Name = "VirtoCommerce.Platform.EnvironmentName",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Platform|General",
                    DefaultValue = string.Empty,
                };

                public static SettingDescriptor CreateAnonymousOrder { get; } = new SettingDescriptor
                {
                    Name = "XOrder.CreateAnonymousOrderEnabled",
                    ValueType = SettingValueType.Boolean,
                    GroupName = "Orders|General",
                    DefaultValue = true
                };

                public static SettingDescriptor IsSelectedForCheckout { get; } = new SettingDescriptor
                {
                    Name = "XPurchase.IsSelectedForCheckout",
                    ValueType = SettingValueType.Boolean,
                    GroupName = "Cart|General",
                    DefaultValue = true
                };

                public static SettingDescriptor PageTitleWithStoreName { get; } = new SettingDescriptor
                {
                    Name = "Frontend.PageTitleWithStoreName",
                    ValueType = SettingValueType.Boolean,
                    GroupName = "Virto Commerce Frontend",
                    DefaultValue = true,
                    IsPublic = true
                };

                public static SettingDescriptor PageTitleStoreNameAlign { get; } = new SettingDescriptor
                {
                    Name = "Frontend.PageTitleStoreNameAlign",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Virto Commerce Frontend",
                    DefaultValue = "start",
                    IsPublic = true
                };

                public static SettingDescriptor PageTitleDivider { get; } = new SettingDescriptor
                {
                    Name = "Frontend.PageTitleDivider",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Virto Commerce Frontend",
                    DefaultValue = " · ",
                    IsPublic = true
                };

                public static SettingDescriptor SupportPhoneNumber { get; } = new SettingDescriptor
                {
                    Name = "Frontend.SupportPhoneNumber",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Virto Commerce Frontend",
                    DefaultValue = string.Empty,
                    IsPublic = true
                };

                public static SettingDescriptor CatalogMenuLinkListName { get; } = new SettingDescriptor
                {
                    Name = "Frontend.CatalogMenuLinkListName",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Virto Commerce Frontend",
                    DefaultValue = "catalog-menu",
                    IsPublic = true
                };

                public static SettingDescriptor CatalogEmptyCategoriesEnabled { get; } = new SettingDescriptor
                {
                    Name = "Frontend.CatalogEmptyCategoriesEnabled",
                    ValueType = SettingValueType.Boolean,
                    GroupName = "Virto Commerce Frontend",
                    DefaultValue = false,
                    IsPublic = true
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        yield return EnvironmentName;
                        yield return CreateAnonymousOrder;
                        yield return IsSelectedForCheckout;
                        yield return PageTitleWithStoreName;
                        yield return PageTitleStoreNameAlign;
                        yield return PageTitleDivider;
                        yield return SupportPhoneNumber;
                        yield return CatalogMenuLinkListName;
                        yield return CatalogEmptyCategoriesEnabled;
                    }
                }
            }

            public static IEnumerable<SettingDescriptor> StoreLevelSettings
            {
                get
                {
                    yield return General.CreateAnonymousOrder;
                    yield return General.IsSelectedForCheckout;
                    yield return General.PageTitleWithStoreName;
                    yield return General.PageTitleStoreNameAlign;
                    yield return General.PageTitleDivider;
                    yield return General.SupportPhoneNumber;
                    yield return General.CatalogMenuLinkListName;
                    yield return General.CatalogEmptyCategoriesEnabled;

                }
            }
        }
    }
}
