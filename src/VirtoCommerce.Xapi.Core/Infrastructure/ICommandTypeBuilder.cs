using System;
using Microsoft.Extensions.DependencyInjection;

namespace VirtoCommerce.Xapi.Core.Infrastructure
{
    public interface ICommandTypeBuilder
    {
        IServiceCollection Services { get; }

        Type CommandType { get; }
    }
}
