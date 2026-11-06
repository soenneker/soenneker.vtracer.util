using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Vtracer.Util.Abstract;
using Soenneker.Utils.Directory.Registrars;
using Soenneker.Utils.File.Registrars;
using Soenneker.Utils.Process.Registrars;
using Soenneker.Utils.Paths.Resources.Registrars;

namespace Soenneker.Vtracer.Util.Registrars;

public static class VtracerUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IVtracerUtil"/> as a singleton service.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddVtracerUtilAsSingleton(this IServiceCollection services)
    {
        services.AddDirectoryUtilAsSingleton()
                .AddFileUtilAsSingleton()
                .AddProcessUtilAsSingleton()
                .AddResourcesPathUtilAsSingleton()
                .TryAddSingleton<IVtracerUtil, VtracerUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IVtracerUtil"/> as a scoped service.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddVtracerUtilAsScoped(this IServiceCollection services)
    {
        services.AddDirectoryUtilAsScoped()
                .AddFileUtilAsScoped()
                .AddProcessUtilAsScoped()
                .AddResourcesPathUtilAsScoped()
                .TryAddScoped<IVtracerUtil, VtracerUtil>();

        return services;
    }
}

