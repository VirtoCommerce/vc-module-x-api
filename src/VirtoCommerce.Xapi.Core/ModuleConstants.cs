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

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        yield return EnvironmentName;
                        yield return CreateAnonymousOrder;
                        yield return IsSelectedForCheckout;
                    }
                }
            }

            public static IEnumerable<SettingDescriptor> StoreLevelSettings
            {
                get
                {
                    yield return General.CreateAnonymousOrder;
                    yield return General.IsSelectedForCheckout;
                }
            }
        }
    }
}
