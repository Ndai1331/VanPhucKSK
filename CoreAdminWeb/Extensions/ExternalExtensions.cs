using CoreAdminWeb.Services.DocxToPdfConverter;
using CoreAdminWeb.Workers;

namespace CoreAdminWeb.Extensions
{
    public static class ExternalExtensions
    {
        public static IServiceCollection AddLibreOfficeUnoConversion(this IServiceCollection services)
        {
            services.AddHostedService<AutoRemoveAllExportedFileHostedService>();
            services.AddSingleton<IDocxToPdfConverter, LibreOfficeDocxToPdfConverter>();
            services.AddSingleton<DocxToPdfBatchService>();
            return services;
        }
    }
}
