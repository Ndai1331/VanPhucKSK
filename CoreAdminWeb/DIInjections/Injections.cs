using CoreAdminWeb.Services;
using CoreAdminWeb.Services.Files;
using CoreAdminWeb.Services.Settings;
using CoreAdminWeb.Services.Users;
using CoreAdminWeb.Providers;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Model;

namespace CoreAdminWeb.DIInjections
{
    public static class DIInjections
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, ApiAuthenticationStateProvider>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IBaseGetService<SoKhamSucKhoeModel>, SoKhamSucKhoeService>();
            services.AddScoped<IBaseGetService<KhamSucKhoeCanLamSanModel>, KhamSucKhoeCanLamSanService>();
            services.AddScoped<IBaseGetService<KhamSucKhoeChuyenKhoaModel>, KhamSucKhoeChuyenKhoaService>();
            services.AddScoped<IBaseGetService<KhamSucKhoeKetLuanModel>, KhamSucKhoeKetLuanService>();
            services.AddScoped<IBaseGetService<KhamSucKhoeNgheNghiepModel>, KhamSucKhoeNgheNghiepService>();
            services.AddScoped<IBaseGetService<KhamSucKhoeSanPhuKhoaModel>, KhamSucKhoeSanPhuKhoaService>();
            services.AddScoped<IBaseGetService<KhamSucKhoeTheLucModel>, KhamSucKhoeTheLucService>();
            services.AddScoped<IBaseGetService<KhamSucKhoeTienSuModel>, KhamSucKhoeTienSuService>();
            services.AddScoped<AlertService>();
        }
    }
}