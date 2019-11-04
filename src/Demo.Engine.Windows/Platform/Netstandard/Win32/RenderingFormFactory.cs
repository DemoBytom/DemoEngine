using Demo.Engine.Windows.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Demo.Engine.Windows.Platform.Netstandard.Win32
{
    public class RenderingFormFactory : IRenderingFormFactory
    {
        private readonly ILogger<RenderingForm> _renderingFormLogger;

        public RenderingFormFactory(
            IOptions<FormSettings> formSettings,
            ILogger<RenderingForm> renderingFormLogger)
            : base(formSettings)
        {
            _renderingFormLogger = renderingFormLogger;
        }

        public override IRenderingForm Create() =>
            new RenderingForm(
                _renderingFormLogger,
                _formSettings);
    }
}