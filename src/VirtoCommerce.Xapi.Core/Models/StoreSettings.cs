using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace VirtoCommerce.Xapi.Core.Models
{
    public class StoreSettings
    {
        [Obsolete("Use Quotes.EnableQuotes public property instead", DiagnosticId = "VC0009", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        public bool QuotesEnabled { get; set; }

        [Obsolete("Use Subscription.EnableSubscriptions public property instead", DiagnosticId = "VC0009", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        public bool SubscriptionEnabled { get; set; }

        public bool TaxCalculationEnabled { get; set; }

        public bool AnonymousUsersAllowed { get; set; }

        [Obsolete("Client application should use own business logic for SPA detection", DiagnosticId = "VC0009", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        public bool IsSpa { get; set; }

        public bool EmailVerificationEnabled { get; set; }

        public bool EmailVerificationRequired { get; set; }

        public bool CreateAnonymousOrderEnabled { get; set; }

        public string SeoLinkType { get; set; }

        public bool DefaultSelectedForCheckout { get; set; }

        public string EnvironmentName { get; set; }

        public PasswordOptions PasswordRequirements { get; set; }

        public IList<string> AuthenticationTypes { get; set; }

        public ModuleSettings[] Modules { get; set; }
    }
}
