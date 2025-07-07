using CoreAdminWeb.Model;
using CoreAdminWeb.Providers;
using CoreAdminWeb.Services;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.DanhMucDungChung;
using CoreAdminWeb.Services.Files;
using CoreAdminWeb.Services.Http;
using CoreAdminWeb.Services.KhamSucKhoe;
using CoreAdminWeb.Services.PDFService;
using CoreAdminWeb.Services.Posts;
using CoreAdminWeb.Services.Settings;
using CoreAdminWeb.Services.Users;
using CoreAdminWeb.Services.FTP;


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
            services.AddScoped<IBaseGetService<KetQuaCanLamSangModel>, KetQuaCanLamSangService>();
            services.AddScoped<IBaseGetService<MedicalAgencyModel>, MedicalAgencyService>();
            services.AddScoped<IBaseGetService<PostModel>, PostService>();
            services.AddScoped<AlertService>();

            // PDF Service Configuration
            services.AddScoped<IPdfService, PdfService>();

            // FTP Service Configuration
            services.AddScoped<IFtpService>(provider =>
            {
                var configuration = provider.GetService<IConfiguration>();
                var ftpConfig = configuration!.GetSection("FTPConfig").Get<CoreAdminWeb.Model.Configuration.FTPConfig>();
                return new FtpService(ftpConfig!);
            });

            // HTTP Client Service Configuration - replaces static RequestClient
            services.AddScoped<IHttpClientService, HttpClientService>();

            // DanhMucDungChung Services
            services.AddScoped<CongTyService>();
            services.AddScoped<TinhService>();
            services.AddScoped<XaPhuongService>();
            services.AddScoped<FolderService>();

            // KhamSucKhoe Services
            services.AddScoped<KhamSucKhoeCanLamSanService>();
            services.AddScoped<KhamSucKhoeChuyenKhoaService>();
            services.AddScoped<KhamSucKhoeKetLuanService>();
            services.AddScoped<KhamSucKhoeNgheNghiepService>();
            services.AddScoped<KhamSucKhoeTheLucService>();
            services.AddScoped<KhamSucKhoeTienSuService>();
            services.AddScoped<KhamSucKhoeSanPhuKhoaService>();
            services.AddScoped<SoKhamSucKhoeService>();

            // Post Services
            services.AddScoped<PostService>();


        }
    }
}