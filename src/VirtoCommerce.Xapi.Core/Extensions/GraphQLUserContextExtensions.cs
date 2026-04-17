using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Xapi.Core.Extensions;

public static class GraphQLUserContextExtensions
{
    public const string OperatorUserNameKey = "OperatorUserName";

    public static bool TrySetOperatorUserName(this GraphQLUserContext userContext, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return userContext.TryAdd(OperatorUserNameKey, value);
    }

    public static bool TryGetOperatorUserName(this GraphQLUserContext userContext, out string value)
    {
        if (!userContext.TryGetValue(OperatorUserNameKey, out var objectValue))
        {
            value = null;
            return false;
        }

        value = objectValue as string;

        return !string.IsNullOrEmpty(value);
    }
}
