using Demo.Engine.Core.Models.Options;
using Microsoft.Extensions.Options;

namespace Demo.Engine.Core.Platform
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