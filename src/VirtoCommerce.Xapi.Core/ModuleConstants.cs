using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.Xapi.Core
{
    public static class ModuleConstants
    {
        public static string GraphQlPath => "/graphql";

        public static class ConfigKeys
        {
            public const string GraphQl = "VirtoCommerce:GraphQL";
            public const string GraphQlPlayground = "VirtoCommerce:GraphQLPlayground";
            public const string GraphQlWebSocket = "VirtoCommerce:GraphQLWebSocket";
            public const string Stores = "VirtoCommerce:Stores";
        }

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

                public static SettingDescriptor ReturnModuleVersion { get; } = new()
                {
                    Name = "XAPI.Security.ReturnModuleVersion",
                    ValueType = SettingValueType.Boolean,
                    GroupName = "Platform|Security",
                    DefaultValue = true,
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
                    AllowedValues = ["start", "end"],
                    IsPublic = true
                };

                public static SettingDescriptor PageTitleDivider { get; } = new SettingDescriptor
                {
                    Name = "Frontend.PageTitleDivider",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Virto Commerce Frontend",
                    DefaultValue = string.Empty,
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
                    DefaultValue = string.Empty,
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

                public static SettingDescriptor ContinueShoppingLink { get; } = new SettingDescriptor
                {
                    Name = "Frontend.ContinueShoppingLink",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Virto Commerce Frontend",
                    DefaultValue = string.Empty,
                    IsPublic = true
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        yield return EnvironmentName;
                        yield return ReturnModuleVersion;
                        yield return PageTitleWithStoreName;
                        yield return PageTitleStoreNameAlign;
                        yield return PageTitleDivider;
                        yield return SupportPhoneNumber;
                        yield return CatalogMenuLinkListName;
                        yield return CatalogEmptyCategoriesEnabled;
                        yield return ContinueShoppingLink;
                    }
                }
            }

            public static IEnumerable<SettingDescriptor> StoreLevelSettings
            {
                get
                {
                    yield return General.PageTitleWithStoreName;
                    yield return General.PageTitleStoreNameAlign;
                    yield return General.PageTitleDivider;
                    yield return General.SupportPhoneNumber;
                    yield return General.CatalogMenuLinkListName;
                    yield return General.CatalogEmptyCategoriesEnabled;
                    yield return General.ContinueShoppingLink;
                }
            }
        }
    }
}
