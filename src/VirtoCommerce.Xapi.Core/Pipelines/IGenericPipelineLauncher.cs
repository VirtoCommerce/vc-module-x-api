using System.Threading.Tasks;

namespace VirtoCommerce.Xapi.Core.Pipelines
{
    public interface IGenericPipelineLauncher
    {
        Task Execute<TParameter>(TParameter parameter) where TParameter : class;
    }
}
