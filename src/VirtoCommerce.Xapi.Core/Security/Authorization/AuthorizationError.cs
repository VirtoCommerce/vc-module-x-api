using System;
using System.Collections;
using GraphQL;
using VirtoCommerce.Xapi.Core.Helpers;

namespace VirtoCommerce.Xapi.Core.Security.Authorization
{
    /// <summary>
    /// Represents an authorization error
    /// </summary>
    public class AuthorizationError : ExecutionError
    {
        public AuthorizationError(string message) : base(message) => Code = Constants.UnauthorizedCode;

        public AuthorizationError(string message, IDictionary data) : base(message, data) => Code = Constants.UnauthorizedCode;

        public AuthorizationError(string message, Exception exception) : base(message, exception) => Code = Constants.UnauthorizedCode;

        public AuthorizationError(string message, string code) : base(message) => Code = code;

        /// <summary>
        /// Creates "Anonymous access denied" error
        /// </summary>
        public static AuthorizationError AnonymousAccessDenied()
        {
            return new AuthorizationError("Anonymous access denied or access token has expired or is invalid.", Constants.UnauthorizedCode);
        }

        /// <summary>
        /// Creates "Password expired" error
        /// </summary>
        public static AuthorizationError PasswordExpired()
        {
            return new AuthorizationError("This user has their password expired. Please change the password using 'changePassword' command.", Constants.PasswordExpiredCode);
        }

        /// <summary>
        /// Creates "User locked" error
        /// </summary>
        public static AuthorizationError UserLocked()
        {
            return new AuthorizationError("This user is locked.", Constants.UserLockedCode);
        }

        /// <summary>
        /// Creates generic "Access denied" error
        /// </summary>
        public static AuthorizationError Forbidden()
        {
            return new AuthorizationError("Access denied.", Constants.ForbiddenCode);
        }

        /// <summary>
        /// Creates an error with Forbidden code and given message
        /// </summary>
        public static AuthorizationError Forbidden(string message)
        {
            return new AuthorizationError(message, Constants.ForbiddenCode);
        }

        /// <summary>
        /// Creates "Permission required" error
        /// </summary>
        public static AuthorizationError PermissionRequired(string permission)
        {
            return new AuthorizationError($"User doesn't have the required permission '{permission}'.", Constants.ForbiddenCode);
        }
    }
}
