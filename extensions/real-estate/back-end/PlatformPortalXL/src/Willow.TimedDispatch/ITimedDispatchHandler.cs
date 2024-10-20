using System.Threading.Tasks;

namespace Willow.TimedDispatch
{
    public interface ITimedDispatchHandler
    {
        Task OnTimedDispatch(object state);
    }
}
