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
    public class SoKhamSucKhoeService : IBaseDetailService<SoKhamSucKhoeModel>
    {
        private readonly string _collection = "SoKhamSucKhoe";
        private readonly IHttpClientService _httpClientService;

        private readonly string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name"
            + ",benh_nhan.id,benh_nhan.first_name,benh_nhan.last_name,benh_nhan.so_dien_thoai"
            + ",benh_nhan.ngay_sinh,benh_nhan.gioi_tinh,benh_nhan.so_dinh_danh,benh_nhan.dia_chi"
            + ",benh_nhan.ma_benh_nhan"
            + ",MaDotKham.ma_hop_dong_ksk.code";

        public SoKhamSucKhoeService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<SoKhamSucKhoeModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic
                    ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<SoKhamSucKhoeModel>>>(url)
                    : await _httpClientService.GetAPIAsync<RequestHttpResponse<List<SoKhamSucKhoeModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<SoKhamSucKhoeModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<SoKhamSucKhoeModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<SoKhamSucKhoeModel>.CreateErrorResponse<List<SoKhamSucKhoeModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<SoKhamSucKhoeModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<SoKhamSucKhoeModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<SoKhamSucKhoeModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<SoKhamSucKhoeModel> { Data = response.Data?.Data }
                    : new RequestHttpResponse<SoKhamSucKhoeModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<SoKhamSucKhoeModel>.CreateErrorResponse<SoKhamSucKhoeModel>(ex);
            }
        }

        /// <summary>
        /// Maps a <see cref="SoKhamSucKhoeModel"/> to a <see cref="SoKhamSucKhoeCRUDModel"/>.
        /// </summary>
        /// <param name="model">The <see cref="SoKhamSucKhoeModel"/> instance to map from. Cannot be null.</param>
        /// <returns>A new <see cref="SoKhamSucKhoeCRUDModel"/> instance populated with data from the specified <paramref
        /// name="model"/>.</returns>
        private static SoKhamSucKhoeCRUDModel MapToCRUDModel(SoKhamSucKhoeModel model)
        {
            return new()
            {
                active = model.active,
                code = model.code,
                name = model.name,
                description = model.description,
                status = model.status.ToString(),
                ngay_kham = model.ngay_kham,
                ngay_lap_so = model.ngay_lap_so,
                ma_benh_nhan = model.ma_benh_nhan,
                ma_luot_kham = model.ma_luot_kham,
                nguoi_lap = model.nguoi_lap,
                chu_ky_nls = model.chu_ky_nls,
                xac_nhan_nld = model.xac_nhan_nld,
                send_mail = model.send_mail,
                ma_cong_ty = model.ma_cong_ty,
                GhiChu = model.GhiChu,
                ChuanDoan = model.ChuanDoan,
                Type = model.Type,
                deleted = model.deleted,
                benh_nhan = model.benh_nhan?.id,
                MaDotKham = model.MaDotKham?.id
            };
        }

        /// <summary>
        /// Asynchronously creates a list of health check records by sending them to a specified API endpoint.
        /// </summary>
        /// <remarks>This method sends a POST request to the API to create the specified health check
        /// records. If the operation fails, the response will contain error details.</remarks>
        /// <param name="model">The list of <see cref="SoKhamSucKhoeModel"/> instances to be created. Cannot be null.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing the list of created <see cref="SoKhamSucKhoeModel"/>
        /// instances if successful; otherwise, contains error information.</returns>
        public async Task<RequestHttpResponse<List<SoKhamSucKhoeModel>>> CreateAsync(List<SoKhamSucKhoeModel> model)
        {
            if (model == null)
            {
                return new RequestHttpResponse<List<SoKhamSucKhoeModel>>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "Vui lòng nhập đầy đủ thông tin" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var createModel = model.Select(c => MapToCRUDModel(c)).ToList();
                var response = await _httpClientService.PostAPIAsync<RequestHttpResponse<List<SoKhamSucKhoeModel>>>($"items/{_collection}?fields={Fields}", createModel);

                if (!response.IsSuccess)
                {
                    return new RequestHttpResponse<List<SoKhamSucKhoeModel>> { Errors = response.Errors };
                }

                return response.Data ?? new RequestHttpResponse<List<SoKhamSucKhoeModel>>();
            }
            catch (Exception ex)
            {
                return IBaseGetService<SoKhamSucKhoeModel>.CreateErrorResponse<List<SoKhamSucKhoeModel>>(ex);
            }
        }

        /// <summary>
        /// Asynchronously updates a list of health check records.
        /// </summary>
        /// <remarks>This method sends a PATCH request to update the specified health check records. If
        /// the operation fails, an error response is returned.</remarks>
        /// <param name="model">A list of <see cref="SoKhamSucKhoeModel"/> objects to be updated. Each object must have a non-zero ID.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a boolean indicating the success of the update operation.
        /// Returns <see langword="false"/> if the input list is null, empty, or contains any records with an ID of
        /// zero.</returns>
        public async Task<RequestHttpResponse<bool>> UpdateAsync(List<SoKhamSucKhoeModel> model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<List<SoKhamSucKhoeModel>>>($"items/{_collection}?fields={Fields}", updateModel);

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return IBaseGetService<SoKhamSucKhoeModel>.CreateErrorResponse<bool>(ex);
            }
        }

        /// <summary>
        /// Asynchronously deletes the specified health check records.
        /// </summary>
        /// <remarks>This method sends a PATCH request to mark the specified records as deleted. Ensure
        /// that the <paramref name="model"/> parameter is not null and contains valid records.</remarks>
        /// <param name="model">A list of <see cref="SoKhamSucKhoeModel"/> objects representing the records to be deleted. Each object must
        /// have a valid non-zero ID.</param>
        /// <returns>A <see cref="RequestHttpResponse{Boolean}"/> indicating the success or failure of the delete operation.
        /// Returns <see langword="false"/> if the input list is null, empty, or contains records with an ID of zero.</returns>
        public async Task<RequestHttpResponse<bool>> DeleteAsync(List<SoKhamSucKhoeModel> model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<List<SoKhamSucKhoeModel>>>($"items/{_collection}?fields={Fields}", model.Select(c => new { id = c.id, deleted = true }));

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return IBaseGetService<SoKhamSucKhoeModel>.CreateErrorResponse<bool>(ex);
            }
        }
    }
}