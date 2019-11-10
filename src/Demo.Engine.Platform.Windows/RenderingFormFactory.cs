using Demo.Engine.Core.Models.Options;
using Demo.Engine.Core.Platform;
using Demo.Engine.Windows.Platform.Netstandard.Win32;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Demo.Engine.Platform.Windows
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