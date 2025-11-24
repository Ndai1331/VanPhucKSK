using CoreAdminWeb.Services.DocxToPdfConverter;
using CoreAdminWeb.Workers;

namespace CoreAdminWeb.DIInjections
{
    public static class ExternalInjections
    {
        public static IServiceCollection AddExternalConfiguration(this IServiceCollection services)
        {
            services.AddSingleton<IDocxToPdfConverter, LibreOfficeDocxToPdfConverter>();
            services.AddSingleton<DocxToPdfBatchService>();

            services.AddHostedService<AutoRemoveAllExportedFileHostedService>();
            services.AddHostedService<LibreOfficeUnoListenerHostedService>();
            return services;
        }
    }
}
