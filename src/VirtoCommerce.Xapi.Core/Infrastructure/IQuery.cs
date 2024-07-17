using MediatR;

namespace VirtoCommerce.Xapi.Core.Infrastructure
{
    public interface IQuery<out TResult> : IRequest<TResult>
    {
    }
}
