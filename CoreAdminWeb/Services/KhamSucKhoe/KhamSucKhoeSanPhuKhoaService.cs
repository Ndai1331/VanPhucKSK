using CoreAdminWeb.Http;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Http;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.Json;
namespace CoreAdminWeb.Services.KhamSucKhoe
{
    /// <summary>
    /// Service for managing fertilizer production facilities
    /// </summary>
    public class KhamSucKhoeSanPhuKhoaService : IBaseDetailService<KhamSucKhoeSanPhuKhoaModel>
    {
        private readonly string _collection = "kham_suc_khoe_san_phu_khoa";
        private readonly IHttpClientService _httpClientService;

        private readonly string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name"
            + ",phan_loai.id,phan_loai.name"
            + ",luot_kham.id,luot_kham.ma_luot_kham";

        public KhamSucKhoeSanPhuKhoaService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic
                    ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>>>(url)
                    : await _httpClientService.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeSanPhuKhoaModel>.CreateErrorResponse<List<KhamSucKhoeSanPhuKhoaModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KhamSucKhoeSanPhuKhoaModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KhamSucKhoeSanPhuKhoaModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<KhamSucKhoeSanPhuKhoaModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<KhamSucKhoeSanPhuKhoaModel> { Data = response.Data?.Data }
                    : new RequestHttpResponse<KhamSucKhoeSanPhuKhoaModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeSanPhuKhoaModel>.CreateErrorResponse<KhamSucKhoeSanPhuKhoaModel>(ex);
            }
        }

        /// <summary>
        /// Maps a <see cref="KhamSucKhoeSanPhuKhoaModel"/> to a <see cref="KhamSucKhoeSanPhuKhoaCRUDModel"/>.
        /// </summary>
        /// <remarks>This method performs a direct mapping of properties from the <paramref name="model"/>
        /// to a new instance of <see cref="KhamSucKhoeSanPhuKhoaCRUDModel"/>. Nullable properties in the source model
        /// are handled by mapping their identifiers if available.</remarks>
        /// <param name="model">The source model containing health examination data to be mapped.</param>
        /// <returns>A <see cref="KhamSucKhoeSanPhuKhoaCRUDModel"/> populated with data from the specified <paramref
        /// name="model"/>.</returns>
        private static KhamSucKhoeSanPhuKhoaCRUDModel MapToCRUDModel(KhamSucKhoeSanPhuKhoaModel model)
        {
            return new()
            {
                active = model.active,
                code = model.code,
                name = model.name,
                description = model.description,
                status = model.status.ToString(),
                sort = model.sort,
                luot_kham = model.luot_kham?.id,
                phan_loai = model.phan_loai?.id,
                ket_qua = model.ket_qua,
                luong_kinh = model.luong_kinh,
                dau_bung_kinh = model.dau_bung_kinh,
                ma_luot_kham = model.ma_luot_kham,
                ap_dung_bptt = model.ap_dung_bptt,
                bptt_ghi_ro = model.bptt_ghi_ro,
                chu_ky = model.chu_ky,
                chu_ky_kinh = model.chu_ky_kinh,
                da_lap_gia_dinh = model.da_lap_gia_dinh,
                mo_san_phu_khoa_ghi_ro = model.mo_san_phu_khoa_ghi_ro,
                nguoi_ket_luan = model.nguoi_ket_luan,
                para = model.para,
                so_lan_mo_san_phu_khoa = model.so_lan_mo_san_phu_khoa,
                tien_su_san_phu_khoa = model.tien_su_san_phu_khoa,
                tinh_chat_kinh = model.tinh_chat_kinh,
                tuoi_bat_dau_kinh = model.tuoi_bat_dau_kinh
            };
        }

        /// <summary>
        /// Asynchronously creates a list of KhamSucKhoeSanPhuKhoaModel items by sending them to a specified API
        /// endpoint.
        /// </summary>
        /// <remarks>This method sends a POST request to the API to create the specified items. If the
        /// request is unsuccessful, the response will include error information.</remarks>
        /// <param name="model">The list of <see cref="KhamSucKhoeSanPhuKhoaModel"/> items to be created. Cannot be null.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a list of <see cref="KhamSucKhoeSanPhuKhoaModel"/> items
        /// if the operation is successful. If the operation fails, the response contains error details.</returns>
        public async Task<RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>>> CreateAsync(List<KhamSucKhoeSanPhuKhoaModel> model)
        {
            if (model == null)
            {
                return new RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "Vui lòng nhập đầy đủ thông tin" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var createModel = model.Select(c => MapToCRUDModel(c)).ToList();
                var response = await _httpClientService.PostAPIAsync<RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>>>($"items/{_collection}?fields={Fields}", createModel);

                if (!response.IsSuccess)
                {
                    return new RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>> { Errors = response.Errors };
                }

                return response.Data ?? new RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>>();
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeSanPhuKhoaModel>.CreateErrorResponse<List<KhamSucKhoeSanPhuKhoaModel>>(ex);
            }
        }

        /// <summary>
        /// Asynchronously updates a list of health records in the system.
        /// </summary>
        /// <remarks>This method sends a PATCH request to update the specified health records. If the
        /// operation is successful, the response will indicate success; otherwise, it will contain error
        /// details.</remarks>
        /// <param name="model">A list of <see cref="KhamSucKhoeSanPhuKhoaModel"/> objects representing the health records to update. Each
        /// record must have a valid non-zero identifier.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a boolean indicating the success of the update operation.
        /// Returns <see langword="false"/> with an error message if the input list is null, empty, or contains records
        /// with an invalid identifier.</returns>
        public async Task<RequestHttpResponse<bool>> UpdateAsync(List<KhamSucKhoeSanPhuKhoaModel> model)
        {
            if (model == null || model.Any(c => c.id == 0) || !model.Any())
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
                var updateModel = model.Select(c =>
                {
                    string jsonStr = JsonSerializer.Serialize(MapToCRUDModel(c));
                    JObject jObject = JObject.Parse(jsonStr);
                    dynamic dynamicObject = jObject;
                    dynamicObject.id = c.id;

                    return dynamicObject;
                }).ToList();
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>>>($"items/{_collection}?fields={Fields}", updateModel);

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeSanPhuKhoaModel>.CreateErrorResponse<bool>(ex);
            }
        }

        /// <summary>
        /// Asynchronously deletes the specified health records by marking them as deleted.
        /// </summary>
        /// <param name="model">A list of <see cref="KhamSucKhoeSanPhuKhoaModel"/> objects representing the records to be deleted. Each
        /// record must have a valid non-zero ID.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a boolean value indicating whether the operation was
        /// successful. Returns <see langword="false"/> with an error message if the input list is null, empty, or
        /// contains records with an ID of zero.</returns>
        public async Task<RequestHttpResponse<bool>> DeleteAsync(List<KhamSucKhoeSanPhuKhoaModel> model)
        {
            if (model == null || model.Any(c => c.id == 0) || !model.Any())
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>>>($"items/{_collection}?fields={Fields}", model.Select(c => new { id = c.id, deleted = true }));

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeSanPhuKhoaModel>.CreateErrorResponse<bool>(ex);
            }
        }
    }
}