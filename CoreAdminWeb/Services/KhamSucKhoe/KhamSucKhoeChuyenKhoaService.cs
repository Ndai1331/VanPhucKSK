using CoreAdminWeb.Http;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Http;
using System.Net;
namespace CoreAdminWeb.Services.KhamSucKhoe
{
    /// <summary>
    /// Service for managing fertilizer production facilities
    /// </summary>
    public class KhamSucKhoeChuyenKhoaService : IBaseGetService<KhamSucKhoeChuyenKhoaModel>
    {
        private readonly string _collection = "kham_suc_khoe_kham_chuyen_khoa";
        private readonly IHttpClientService _httpClientService;

        private readonly string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name"
            + ",pl_nk_tuan_hoan.id,pl_nk_tuan_hoan.code,pl_nk_tuan_hoan.name"
            + ",pl_nk_ho_hap.id,pl_nk_ho_hap.code,pl_nk_ho_hap.name"
            + ",pl_nk_tieu_hoa.id,pl_nk_tieu_hoa.code,pl_nk_tieu_hoa.name"
            + ",pl_nk_than_tiet_nieu.id,pl_nk_than_tiet_nieu.code,pl_nk_than_tiet_nieu.name"
            + ",pl_nk_noi_tiet.id,pl_nk_noi_tiet.code,pl_nk_noi_tiet.name"
            + ",pl_nk_co_xuong_khop.id,pl_nk_co_xuong_khop.code,pl_nk_co_xuong_khop.name"
            + ",pl_nk_than_kinh.id,pl_nk_than_kinh.code,pl_nk_than_kinh.name"
            + ",pl_nk_tam_than.id,pl_nk_tam_than.code,pl_nk_tam_than.name"
            + ",pl_ngoai_khoa.id,pl_ngoai_khoa.code,pl_ngoai_khoa.name"
            + ",pl_da_lieu.id,pl_da_lieu.code,pl_da_lieu.name"
            + ",pl_mat.id,pl_mat.code,pl_mat.name"
            + ",pl_tmh.id,pl_tmh.code,pl_tmh.name"
            + ",pl_rhm.id,pl_rhm.code,pl_rhm.name"
            + ",luot_kham.id,luot_kham.ma_luot_kham";

        public KhamSucKhoeChuyenKhoaService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic
                    ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>>>(url)
                    : await _httpClientService.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeChuyenKhoaModel>.CreateErrorResponse<List<KhamSucKhoeChuyenKhoaModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KhamSucKhoeChuyenKhoaModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KhamSucKhoeChuyenKhoaModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<KhamSucKhoeChuyenKhoaModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<KhamSucKhoeChuyenKhoaModel> { Data = response.Data?.Data }
                    : new RequestHttpResponse<KhamSucKhoeChuyenKhoaModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeChuyenKhoaModel>.CreateErrorResponse<KhamSucKhoeChuyenKhoaModel>(ex);
            }
        }
    }
}