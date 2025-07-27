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
    public class KhamSucKhoeKetLuanService : IBaseDetailService<KhamSucKhoeKetLuanModel>
    {
        private readonly string _collection = "kham_suc_khoe_ket_luan";
        private readonly IHttpClientService _httpClientService;

        private readonly string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name"
            + ",phan_loai_suc_khoe.id,phan_loai_suc_khoe.code"
            + ",luot_kham.id,luot_kham.ma_luot_kham"
            + ",file_url.id,file_url.filename_disk,file_url.filename_download";

        public KhamSucKhoeKetLuanService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KhamSucKhoeKetLuanModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic
                    ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeKetLuanModel>>>(url)
                    : await _httpClientService.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeKetLuanModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeKetLuanModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeKetLuanModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeKetLuanModel>.CreateErrorResponse<List<KhamSucKhoeKetLuanModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KhamSucKhoeKetLuanModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KhamSucKhoeKetLuanModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<KhamSucKhoeKetLuanModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<KhamSucKhoeKetLuanModel> { Data = response.Data?.Data }
                    : new RequestHttpResponse<KhamSucKhoeKetLuanModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeKetLuanModel>.CreateErrorResponse<KhamSucKhoeKetLuanModel>(ex);
            }
        }

        /// <summary>
        /// Maps a <see cref="KhamSucKhoeKetLuanModel"/> to a <see cref="KhamSucKhoeKetLuanCRUDModel"/>.
        /// </summary>
        /// <param name="model">The source model containing health examination conclusion data to be mapped. Cannot be null.</param>
        /// <returns>A <see cref="KhamSucKhoeKetLuanCRUDModel"/> populated with data from the specified <paramref name="model"/>.</returns>
        private static KhamSucKhoeKetLuanCRUDModel MapToCRUDModel(KhamSucKhoeKetLuanModel model)
        {
            return new()
            {
                active = model.active,
                code = model.code,
                description = model.description,
                name = model.name,
                sort = model.sort,
                status = model.status,
                luot_kham = model.luot_kham?.id,
                phan_loai_suc_khoe = model.phan_loai_suc_khoe?.id,
                bs_ket_luan = model.bs_ket_luan?.id,
                benh_tat_ket_luan = model.benh_tat_ket_luan,
                chu_ky = model.chu_ky,
                de_nghi = model.de_nghi,
                file_url = model.file_url?.id,
                ma_luot_kham = model.ma_luot_kham,
                ngay_ket_luan = model.ngay_ket_luan,
                nguoi_ket_luan = model.nguoi_ket_luan
            };
        }

        /// <summary>
        /// Asynchronously creates a list of health conclusion models by sending them to a specified API endpoint.
        /// </summary>
        /// <remarks>This method sends the provided list of models to a predefined API endpoint using an
        /// HTTP POST request. If the operation is unsuccessful, the response will contain error details.</remarks>
        /// <param name="model">The list of <see cref="KhamSucKhoeKetLuanModel"/> instances to be created. Cannot be null.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing the list of created <see cref="KhamSucKhoeKetLuanModel"/>
        /// instances if successful; otherwise, contains error information.</returns>
        public async Task<RequestHttpResponse<List<KhamSucKhoeKetLuanModel>>> CreateAsync(List<KhamSucKhoeKetLuanModel> model)
        {
            if (model == null)
            {
                return new RequestHttpResponse<List<KhamSucKhoeKetLuanModel>>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "Vui lòng nhập đầy đủ thông tin" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var createModel = model.Select(c => MapToCRUDModel(c)).ToList();
                var response = await _httpClientService.PostAPIAsync<RequestHttpResponse<List<KhamSucKhoeKetLuanModel>>>($"items/{_collection}?fields={Fields}", createModel);

                if (!response.IsSuccess)
                {
                    return new RequestHttpResponse<List<KhamSucKhoeKetLuanModel>> { Errors = response.Errors };
                }

                return response.Data ?? new RequestHttpResponse<List<KhamSucKhoeKetLuanModel>>();
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeKetLuanModel>.CreateErrorResponse<List<KhamSucKhoeKetLuanModel>>(ex);
            }
        }

        /// <summary>
        /// Asynchronously updates a list of health conclusion records.
        /// </summary>
        /// <remarks>This method sends a PATCH request to update the specified records. If the operation
        /// fails, an error response is returned.</remarks>
        /// <param name="model">A list of <see cref="KhamSucKhoeKetLuanModel"/> objects to be updated. Each object must have a valid
        /// non-zero ID.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a boolean indicating the success of the update operation.
        /// Returns <see langword="false"/> if the input list is null, empty, or contains any records with an ID of
        /// zero.</returns>
        public async Task<RequestHttpResponse<bool>> UpdateAsync(List<KhamSucKhoeKetLuanModel> model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<List<KhamSucKhoeKetLuanModel>>>($"items/{_collection}?fields={Fields}", updateModel);

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeKetLuanModel>.CreateErrorResponse<bool>(ex);
            }
        }

        /// <summary>
        /// Asynchronously deletes the specified health conclusion records.
        /// </summary>
        /// <remarks>This method sends a PATCH request to mark the specified records as deleted. Ensure
        /// that each record in the list has a valid ID before calling this method.</remarks>
        /// <param name="model">A list of <see cref="KhamSucKhoeKetLuanModel"/> objects representing the records to be deleted. Each object
        /// must have a valid non-zero ID.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a boolean value indicating whether the deletion was
        /// successful. Returns <see langword="false"/> if the input list is null, empty, or contains invalid IDs.</returns>
        public async Task<RequestHttpResponse<bool>> DeleteAsync(List<KhamSucKhoeKetLuanModel> model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<List<KhamSucKhoeKetLuanModel>>>($"items/{_collection}?fields={Fields}", model.Select(c => new { id = c.id, deleted = true }));

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeKetLuanModel>.CreateErrorResponse<bool>(ex);
            }
        }
    }
}