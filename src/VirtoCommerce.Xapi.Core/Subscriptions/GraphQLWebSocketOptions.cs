using System;

namespace VirtoCommerce.Xapi.Core.Subscriptions
{
    public class GraphQLWebSocketOptions
    {
        public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(45);
    }
}
