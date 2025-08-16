using CoreAdminWeb.Model;
using CoreAdminWeb.Model.Contract;
using CoreAdminWeb.Model.Dashboard.General;
using CoreAdminWeb.Model.KhamSucKhoes;
using CoreAdminWeb.Providers;
using CoreAdminWeb.Services;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.CaNhanSoKhamSucKhoe;
using CoreAdminWeb.Services.Contract;
using CoreAdminWeb.Services.DanhMucDungChung;
using CoreAdminWeb.Services.DanhSachDoanSoKhamSucKhoe;
using CoreAdminWeb.Services.Dashboard;
using CoreAdminWeb.Services.Files;
using CoreAdminWeb.Services.FTP;
using CoreAdminWeb.Services.Http;
using CoreAdminWeb.Services.ICaNhanSoKhamSucKhoeService;
using CoreAdminWeb.Services.IDanhSachDoanSoKhamSucKhoeService;
using CoreAdminWeb.Services.IDashboardService;
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
            services.AddScoped<IBaseDetailService<SoKhamSucKhoeModel>, SoKhamSucKhoeService>();
            services.AddScoped<IBaseDetailService<KhamSucKhoeCanLamSangModel>, KhamSucKhoeCanLamSanService>();
            services.AddScoped<IBaseDetailService<KhamSucKhoeChuyenKhoaModel>, KhamSucKhoeChuyenKhoaService>();
            services.AddScoped<IBaseDetailService<KhamSucKhoeKetLuanModel>, KhamSucKhoeKetLuanService>();
            services.AddScoped<IBaseDetailService<KhamSucKhoeNgheNghiepModel>, KhamSucKhoeNgheNghiepService>();
            services.AddScoped<IBaseDetailService<KhamSucKhoeSanPhuKhoaModel>, KhamSucKhoeSanPhuKhoaService>();
            services.AddScoped<IBaseDetailService<KhamSucKhoeTheLucModel>, KhamSucKhoeTheLucService>();
            services.AddScoped<IBaseDetailService<KhamSucKhoeTienSuModel>, KhamSucKhoeTienSuService>();
            services.AddScoped<IBaseDetailService<KhamSucKhoeKetQuaCanLamSangModel>, KhamSucKhoeKetQuaCanLamSangService>();
            services.AddScoped<IBaseGetService<MedicalAgencyModel>, MedicalAgencyService>();
            services.AddScoped<IBaseGetService<PostModel>, PostService>();
            services.AddScoped<IBaseGetService<HoaDonDienTuModel>, ElectronicInvoiceService>();
            services.AddScoped<IBaseGetService<KetQuaCanLamSangModel>, KetQuaCanLamSangService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IBaseService<LoaiDinhMucModel>, LoaiDinhMucService>();
            services.AddScoped<IBaseService<DinhMucModel>, DinhMucService>();
            services.AddScoped<IBaseService<ContractTypeModel>, ContractTypeService>();
            services.AddScoped<IBaseService<CongTyModel>, CongTyService>();
            services.AddScoped<IBaseService<PhanLoaiSucKhoeModel>, PhanLoaiSucKhoeService>();
            services.AddScoped<IBaseService<KhamSucKhoeCongTyModel>, KhamSucKhoeCongTyService>();
            services.AddScoped<IBaseService<ContractModel>, ContractService>();
            services.AddScoped<IBaseService<XaPhuongModel>, XaPhuongService>();
            services.AddScoped<IBaseService<TinhModel>, TinhService>();
            services.AddScoped<IContractDinhMucService, ContractDinhMucService>();
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
            services.AddScoped<IDanhSachDoanSoKhamSucKhoeService<DanhSachDoanSoKhamSucKhoeModel>, DanhSachDoanSoKhamSucKhoeService>();
            services.AddScoped<ICaNhanSoKhamSucKhoeService<HoSoKhamSucKhoeTT32Model>, CaNhanSoKhamSucKhoeService>();
            services.AddScoped<IDashboardService<GeneralDashboardModel>, DashboardService>();
            services.AddScoped<IDashboardService<CompanySummaryReportDashboardModel>, CompanyReportDashboardService>();
            services.AddScoped(typeof(IExportExcelService<>), typeof(ExportExcelService<>));
        }
    }
}

