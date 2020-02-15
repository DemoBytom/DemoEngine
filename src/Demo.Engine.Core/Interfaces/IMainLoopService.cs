using System.Threading.Tasks;
using Demo.Engine.Core.Services;

namespace Demo.Engine.Core.Interfaces
{
    public interface IMainLoopService
    {
        Task RunAsync(
            MainLoopService.UpdateCallback updateCallback,
            MainLoopService.RenderCallback renderCallback);

        void Stop();

        bool IsRunning { get; }
    }
}