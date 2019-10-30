using Demo.Engine.Windows.Models.Options;
using Microsoft.Extensions.Options;

namespace Demo.Engine.Windows.Platform.Netstandard.Win32
{
    public class RenderingFormFactory : IRenderingFormFactory
    {
        public RenderingFormFactory(IOptions<FormSettings> formSettings)
            : base(formSettings)
        {
        }

        public override IRenderingForm Create() =>
            new RenderingForm(_formSettings);
    }
}