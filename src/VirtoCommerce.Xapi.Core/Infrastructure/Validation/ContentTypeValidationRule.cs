using System.Threading.Tasks;
using GraphQL.Validation;
using Microsoft.AspNetCore.Http;

namespace VirtoCommerce.Xapi.Core.Infrastructure.Validation
{
    public class ContentTypeValidationRule : ValidationRuleBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContentTypeValidationRule(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override ValueTask<INodeVisitor> GetPreNodeVisitorAsync(ValidationContext context) => ValidateAsync(context);

        public virtual ValueTask<INodeVisitor> ValidateAsync(ValidationContext context)
        {
            var contentType = HttpContext?.Request?.ContentType;

            if (contentType == "application/json" ||
                contentType == "application/graphql" ||
                (contentType == null && IsWebSocketRequest))
            {
                return default;
            }

            context.ReportError(new ValidationError(string.Empty, string.Empty, "Non-supported media type."));
            return default;
        }

        private HttpContext HttpContext => _httpContextAccessor?.HttpContext;

        private bool IsWebSocketRequest => HttpContext?.WebSockets?.IsWebSocketRequest == true;
    }
}
