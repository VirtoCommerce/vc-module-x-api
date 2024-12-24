using GraphQL;
using GraphQL.Authorization;
using GraphQL.DI;
using VirtoCommerce.Xapi.Data.Security.Authorization;

namespace VirtoCommerce.Xapi.Data.Extensions
{
    public static class AuthorizationGraphQLBuilderExtensions
    {
        public static IGraphQLBuilder AddPermissionAuthorization(this IGraphQLBuilder builder)
        {
            builder.Services.TryRegister<IAuthorizationEvaluator, PermissionAuthorizationEvaluator>(GraphQL.DI.ServiceLifetime.Singleton);
            builder.AddValidationRule<AuthorizationValidationRule>(true);
            return builder;
        }
    }
}
