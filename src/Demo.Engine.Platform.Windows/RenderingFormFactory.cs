using Demo.Engine.Core.Models.Options;
using Demo.Engine.Core.Platform;
using Demo.Engine.Windows.Platform.Netstandard.Win32;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Demo.Engine.Platform.Windows
{
    public class RenderingFormFactory : IRenderingFormFactory
    {
        private readonly ILogger<RenderingForm> _renderingFormLogger;
        private readonly IMediator _mediator;

        public RenderingFormFactory(
            IOptions<FormSettings> formSettings,
            ILogger<RenderingForm> renderingFormLogger,
            IMediator mediator)
            : base(formSettings)
        {
            _renderingFormLogger = renderingFormLogger;
            _mediator = mediator;
        }

        public override IRenderingForm Create() =>
            new RenderingForm(
                _renderingFormLogger,
                _formSettings,
                _mediator);
    }
}