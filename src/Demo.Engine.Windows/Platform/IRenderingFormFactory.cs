using Demo.Engine.Windows.Models.Options;
using Demo.Engine.Windows.Platform.Netstandard;
using Microsoft.Extensions.Options;

namespace Demo.Engine.Windows.Platform
{
    public abstract class IRenderingFormFactory
    {
        protected readonly FormSettings _formSettings;

        protected IRenderingFormFactory(IOptions<FormSettings> formSettings)
        {
            _formSettings = formSettings.Value;
        }

        public abstract IRenderingForm Create();
    }
}