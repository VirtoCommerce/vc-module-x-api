using MediatR;

namespace VirtoCommerce.Xapi.Core.Infrastructure
{
    public interface ICommand<out TResult> : IRequest<TResult>
    {
    }
}
