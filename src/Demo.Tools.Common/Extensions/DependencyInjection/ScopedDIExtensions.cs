using Microsoft.Extensions.DependencyInjection;

namespace Demo.Tools.Common.Extensions.DependencyInjection
{
    public static class ScopedDIExtensions
    {
        /// <summary>
        /// Adds a single scoped instance of <typeparamref name="TImplementation"/> that can be retrived by either <typeparamref name="TService1"/> or <typeparamref name="TService2"/>
        /// </summary>
        /// <typeparam name="TService1"></typeparam>
        /// <typeparam name="TService2"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddScoped<TService1, TService2, TImplementation>(
            this IServiceCollection services)
            where TService1 : class
            where TService2 : class
            where TImplementation : class, TService1, TService2 => services.AddScoped<TImplementation>()
                .AddScopedInternalFactory<TService1, TImplementation>()
                .AddScopedInternalFactory<TService2, TImplementation>();

        private static IServiceCollection AddScopedInternalFactory<TService, TImplementation>(
            this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService => services.AddScoped<TService, TImplementation>(x => x.GetRequiredService<TImplementation>());
    }
}