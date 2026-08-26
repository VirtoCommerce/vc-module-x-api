using System;
using System.Threading.Tasks;
using GraphQL;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.Extensions;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Security.Authorization;
using VirtoCommerce.Xapi.Core.Services;

namespace VirtoCommerce.Xapi.Data.Services
{
    public class UserManagerCore : IUserManagerCore
    {
        private const string CheckUserStateCacheKeyPrefix = "UserManagerCore.CheckUserState";

        private readonly Func<UserManager<ApplicationUser>> _userManagerFactory;
        private readonly IRequestScopedCacheAccessor _requestScopedCacheAccessor;

        // Takes the accessor rather than IRequestScopedCache itself: the schema builders and schemas that
        // consume IUserManagerCore are registered as singletons, so a constructor-injected scoped cache would be
        // captured against the root scope and shared by every request.
        public UserManagerCore(Func<UserManager<ApplicationUser>> userManagerFactory, IRequestScopedCacheAccessor requestScopedCacheAccessor)
        {
            _userManagerFactory = userManagerFactory;
            _requestScopedCacheAccessor = requestScopedCacheAccessor;
        }

        [Obsolete("Use the constructor with IRequestScopedCacheAccessor. Without it the user state is re-checked for every GraphQL root field.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        public UserManagerCore(Func<UserManager<ApplicationUser>> userManagerFactory)
            : this(userManagerFactory, requestScopedCacheAccessor: null)
        {
        }

        public virtual async Task<bool> IsLockedOutAsync(ApplicationUser user)
        {
            using var userManager = _userManagerFactory();

            var result = await userManager.IsLockedOutAsync(user);

            return result;
        }

        [Obsolete("Use CheckCurrentUserState()", DiagnosticId = "VC0009", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        public Task CheckUserState(string userId, bool allowAnonymous)
        {
            return CheckUserState(userId, allowAnonymous, isExternalSignIn: false, isImpersonated: false);
        }

        public Task CheckCurrentUserState(IResolveFieldContext context, bool allowAnonymous)
        {
            var principal = context.GetCurrentPrincipal();
            var userId = principal.GetCurrentUserId();
            var isExternalSignIn = principal.IsExternalSignIn();
            var isImpersonated = principal.IsImpersonated();

            return CheckUserState(userId, allowAnonymous, isExternalSignIn, isImpersonated);
        }

        /// <summary>
        /// Throws when the account behind the current token is unusable: missing, password expired, or locked out.
        /// </summary>
        /// <remarks>
        /// An override must decide from these parameters alone. The caller memoizes the outcome under a key built
        /// from them, so anything else it reads yields false cache hits - a wrong authorization answer - unless
        /// that input is added to the key as well.
        /// <br/><br/>
        /// A refusal is signalled by raising an <see cref="ExecutionError"/>, and that instance is never the one
        /// re-raised: every caller, the first included, gets a new base <see cref="AuthorizationError"/> - or a
        /// base <see cref="ExecutionError"/> for any other type - carrying Message and Code only.
        /// </remarks>
        protected virtual async Task ValidateUserStateAsync(string userId, bool allowAnonymous, bool isExternalSignIn, bool isImpersonated)
        {
            var userManager = _userManagerFactory();
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                if (allowAnonymous)
                {
                    return;
                }

                throw AuthorizationError.AnonymousAccessDenied();
            }

            if (user.PasswordExpired && !isExternalSignIn && !isImpersonated)
            {
                throw AuthorizationError.PasswordExpired();
            }

            if (await userManager.IsLockedOutAsync(user))
            {
                throw AuthorizationError.UserLocked();
            }
        }

        // Callers run this once per GraphQL root field, so a document selecting several fields repeats the same
        // check. Each run resolves the platform's UserManager factory, which opens a DI scope and builds a
        // UserManager over its own SecurityDbContext. Memoizing per request collapses it to one, and a refusal
        // stays cached so every field of the document is refused rather than only the first.
        protected virtual async Task CheckUserState(string userId, bool allowAnonymous, bool isExternalSignIn, bool isImpersonated)
        {
            // Null outside a request - a background job or startup - where there is nothing to bound the cache
            // to and the check simply runs uncached.
            var cache = _requestScopedCacheAccessor?.Cache;

            if (cache == null)
            {
                await ValidateUserStateAsync(userId, allowAnonymous, isExternalSignIn, isImpersonated);

                return;
            }

            // The key covers every argument; the memo also freezes the state the check reads, so a mid-request
            // SecurityCacheRegion.ExpireUser is observed on the next request rather than the next root field.
            var key = string.Join('|', CheckUserStateCacheKeyPrefix, userId, allowAnonymous, isExternalSignIn, isImpersonated);

            // What is cached is the refusal, not the exception raised for it: GraphQL.NET stamps Path and
            // Locations onto the ExecutionError it catches, so one instance shared by concurrent sibling fields
            // would report a single field's path for all of them. Each caller throws its own error instead.
            // The catch spans ExecutionError, not just AuthorizationError, because an override may raise any of
            // them and every one is used as-is. Anything else faults the cached task - GraphQL.NET wraps a
            // non-ExecutionError per field, so it aliases nothing, and the fault is the documented no-retry.
            var refusal = await cache.GetOrAddAsync<ExecutionError>(key, async () =>
            {
                try
                {
                    await ValidateUserStateAsync(userId, allowAnonymous, isExternalSignIn, isImpersonated);

                    return null;
                }
                catch (ExecutionError error)
                {
                    return error;
                }
            });

            if (refusal != null)
            {
                // A subclass carrying more than message and code is rebuilt as a plain ExecutionError - for every
                // caller alike, the first included, so no field reports a richer error than its siblings.
                throw refusal is AuthorizationError
                    ? new AuthorizationError(refusal.Message, refusal.Code)
                    : new ExecutionError(refusal.Message) { Code = refusal.Code };
            }
        }
    }
}
