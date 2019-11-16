using MediatR;

namespace Demo.Engine.Core.Requests.Keyboard
{
    /// <summary>
    /// Gets current snapshot of the keyboard state
    /// </summary>
    public class KeyboardStateRequest : IRequest<KeyboardStateResponse>
    {
    }
}