using CoreAdminWeb.Model;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Http;
using System.Net;

namespace CoreAdminWeb.Services
{
    /// <summary>
    /// Service for managing fertilizer production facilities
    /// </summary>
    public class KhamSucKhoeCongTyService : IBaseService<KhamSucKhoeCongTyModel>
    {
        private readonly string _collection = "kham_suc_khoe_cong_ty";
        private const string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name";
        private readonly IHttpClientService _httpClientService;

        public KhamSucKhoeCongTyService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        /// <summary>
        /// Creates a response with error handling
        /// </summary>
        private static RequestHttpResponse<T> CreateErrorResponse<T>(Exception ex)
        {
            return new RequestHttpResponse<T>
            {
                Errors = new List<ErrorResponse> { new() { Message = ex.Message } },
                StatusCode = HttpStatusCode.InternalServerError
            };
        }

        /// <summary>
        /// Maps a model to CRUD model
        /// </summary>
        private static KhamSucKhoeCongTyCRUDModel MapToCRUDModel(KhamSucKhoeCongTyModel model)
        {
            return new()
            {
                code = model.code,
                name = model.name,
                description = model.description,
                sort = model.sort,
                active = model.active,
                status = model.status,
                ma_don_vi = model.ma_don_vi,
                ma_hop_dong_ksk = model.ma_hop_dong_ksk,
                so_luong_du_kien = model.so_luong_du_kien,
                so_luong_thuc_te = model.so_luong_thuc_te,
                ngay_du_kien_kham = model.ngay_du_kien_kham,
                ngay_ket_thuc = model.ngay_ket_thuc,
                bs_tuan_hoan = model.bs_tuan_hoan,
                chu_ky_tuan_hoan = model.chu_ky_tuan_hoan,
                bs_ho_hap = model.bs_ho_hap,
                chu_ky_ho_hap = model.chu_ky_ho_hap,
                bs_tieu_hoa = model.bs_tieu_hoa,
                chu_ky_tieu_hoa = model.chu_ky_tieu_hoa,
                bs_than_tiet_nieu = model.bs_than_tiet_nieu,
                chu_ky_than_tiet_nieu = model.chu_ky_than_tiet_nieu,
                bs_noi_tiet = model.bs_noi_tiet,
                chu_ky_noi_tiet = model.chu_ky_noi_tiet,
                bs_co_xuong_khop = model.bs_co_xuong_khop,
                chu_ky_co_xuong_khop = model.chu_ky_co_xuong_khop,
                bs_than_kinh = model.bs_than_kinh,
                chu_ky_than_kinh = model.chu_ky_than_kinh,
                bs_tam_than = model.bs_tam_than,
                chu_ky_tam_than = model.chu_ky_tam_than,
                bs_ngoai_khoa = model.bs_ngoai_khoa,
                chu_ky_ngoai_khoa = model.chu_ky_ngoai_khoa,
                bs_mat = model.bs_mat,
                chu_ky_mat = model.chu_ky_mat,
                bs_tai_mui_hong = model.bs_tai_mui_hong,
                chu_ky_tai_mui_hong = model.chu_ky_tai_mui_hong,
                bs_rang_ham_mat = model.bs_rang_ham_mat,
                chu_ky_rang_ham_mat = model.chu_ky_rang_ham_mat,
                bs_san_phu_khoa = model.bs_san_phu_khoa,
                chu_ky_san_phu_khoa = model.chu_ky_san_phu_khoa,
                bs_ket_luan = model.bs_ket_luan,
                chu_ky_ket_luan = model.chu_ky_ket_luan,
                Ksk_status = model.Ksk_status
            };
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KhamSucKhoeCongTyModel>>> GetAllAsync(string query)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeCongTyModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeCongTyModel>> { Data = response.Data?.Data }
                    : new RequestHttpResponse<List<KhamSucKhoeCongTyModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<List<KhamSucKhoeCongTyModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KhamSucKhoeCongTyModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KhamSucKhoeCongTyModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<KhamSucKhoeCongTyModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<KhamSucKhoeCongTyModel> { Data = response.Data?.Data }
                    : new RequestHttpResponse<KhamSucKhoeCongTyModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<KhamSucKhoeCongTyModel>(ex);
            }
        }

        /// <summary>
        /// Creates a new fertilizer production facility
        /// </summary>
        public async Task<RequestHttpResponse<KhamSucKhoeCongTyModel>> CreateAsync(KhamSucKhoeCongTyModel model)
        {
            if (model == null)
            {
                return new RequestHttpResponse<KhamSucKhoeCongTyModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "Vui lòng nhập đầy đủ thông tin" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var createModel = MapToCRUDModel(model);
                var response = await _httpClientService.PostAPIAsync<RequestHttpResponse<KhamSucKhoeCongTyCRUDModel>>($"items/{_collection}?fields={Fields}", createModel);

                if (!response.IsSuccess)
                {
                    return new RequestHttpResponse<KhamSucKhoeCongTyModel> { Errors = response.Errors };
                }

                return new RequestHttpResponse<KhamSucKhoeCongTyModel>
                {
                    Data = new()
                    {
                        code = response.Data?.Data?.code,
                        name = response.Data?.Data?.name
                    }
                };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<KhamSucKhoeCongTyModel>(ex);
            }
        }

        /// <summary>
        /// Updates an existing fertilizer production facility
        /// </summary>
        public async Task<RequestHttpResponse<bool>> UpdateAsync(KhamSucKhoeCongTyModel model)
        {
            if (model == null || model.id == 0)
            {
                return new RequestHttpResponse<bool>
                {
                    Data = false,
                    Errors = new List<ErrorResponse> { new() { Message = "Vui lòng chọn bản ghi để cập nhật" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var updateModel = MapToCRUDModel(model);
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<KhamSucKhoeCongTyCRUDModel>>($"items/{_collection}/{model.id}?fields={Fields}", updateModel);

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<bool>(ex);
            }
        }

        /// <summary>
        /// Deletes a fertilizer production facility
        /// </summary>
        public async Task<RequestHttpResponse<bool>> DeleteAsync(KhamSucKhoeCongTyModel model)
        {
            if (model == null || model.id == 0)
            {
                return new RequestHttpResponse<bool>
                {
                    Data = false,
                    Errors = new List<ErrorResponse> { new() { Message = "Vui lòng chọn bản ghi để xoá" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<KhamSucKhoeCongTyModel>>($"items/{_collection}/{model.id}?fields={Fields}", new { deleted = true });

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<bool>(ex);
            }
        }
    }
}