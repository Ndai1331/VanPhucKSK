using CoreAdminWeb.Model;
using CoreAdminWeb.Providers;
using CoreAdminWeb.Services;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.DanhMucDungChung;
using CoreAdminWeb.Services.Files;
using CoreAdminWeb.Services.FTP;
using CoreAdminWeb.Services.Http;
using CoreAdminWeb.Services.KhamSucKhoe;
using CoreAdminWeb.Services.Menus;
using CoreAdminWeb.Services.PDFService;
using CoreAdminWeb.Services.Posts;
using CoreAdminWeb.Services.Settings;
using CoreAdminWeb.Services.Users;


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
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IBaseService<LoaiDinhMucModel>, LoaiDinhMucService>();
            services.AddScoped<IBaseService<DinhMucModel>, DinhMucService>();
            services.AddScoped<IBaseService<ContractTypeModel>, ContractTypeService>();
            services.AddScoped<IBaseService<CongTyModel>, CongTyService>();
            services.AddScoped<IBaseService<PhanLoaiSucKhoeModel>, PhanLoaiSucKhoeService>();
            services.AddScoped<IBaseService<KhamSucKhoeCongTyModel>, KhamSucKhoeCongTyService>();
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

        }
    }
}