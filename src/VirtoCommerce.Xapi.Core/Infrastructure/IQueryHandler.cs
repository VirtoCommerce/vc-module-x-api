using MediatR;

namespace VirtoCommerce.Xapi.Core.Infrastructure
{
    public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult> where TQuery : IQuery<TResult>
    {
    }
}
