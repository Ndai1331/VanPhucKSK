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
    public class KhamSucKhoeTienSuService : IBaseDetailService<KhamSucKhoeTienSuModel>
    {
        private readonly string _collection = "kham_suc_khoe_tien_su";
        private readonly IHttpClientService _httpClientService;

        private readonly string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name"
            + ",luot_kham.id,luot_kham.ma_luot_kham";

        public KhamSucKhoeTienSuService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KhamSucKhoeTienSuModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic
                    ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeTienSuModel>>>(url)
                    : await _httpClientService.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeTienSuModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeTienSuModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeTienSuModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeTienSuModel>.CreateErrorResponse<List<KhamSucKhoeTienSuModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KhamSucKhoeTienSuModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KhamSucKhoeTienSuModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<KhamSucKhoeTienSuModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<KhamSucKhoeTienSuModel> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<KhamSucKhoeTienSuModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeTienSuModel>.CreateErrorResponse<KhamSucKhoeTienSuModel>(ex);
            }
        }

        /// <summary>
        /// Maps a <see cref="KhamSucKhoeTienSuModel"/> to a <see cref="KhamSucKhoeTienSuCRUDModel"/>.
        /// </summary>
        /// <remarks>This method performs a direct mapping of properties from <see
        /// cref="KhamSucKhoeTienSuModel"/> to <see cref="KhamSucKhoeTienSuCRUDModel"/>. Ensure that the <paramref
        /// name="model"/> is not null before calling this method to avoid a <see
        /// cref="NullReferenceException"/>.</remarks>
        /// <param name="model">The source model containing health history data to be mapped.</param>
        /// <returns>A <see cref="KhamSucKhoeTienSuCRUDModel"/> populated with data from the specified <paramref name="model"/>.</returns>
        private static KhamSucKhoeTienSuCRUDModel MapToCRUDModel(KhamSucKhoeTienSuModel model)
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
                benh_nghe_nghiep = model.benh_nghe_nghiep,
                ma_luot_kham = model.ma_luot_kham,
                nam_phat_hien = model.nam_phat_hien,
                nam_phat_hien_benh_nghe_nghiep = model.nam_phat_hien_benh_nghe_nghiep,
                ten_benh = model.ten_benh,
                tien_su_gia_dinh = model.tien_su_gia_dinh
            };
        }

        /// <summary>
        /// Asynchronously creates a list of health history records by sending them to a specified API endpoint.
        /// </summary>
        /// <remarks>This method sends the provided health history records to a remote API for creation.
        /// If the operation is unsuccessful, the response will contain error details.</remarks>
        /// <param name="model">The list of <see cref="KhamSucKhoeTienSuModel"/> instances to be created. Cannot be null.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing the list of created <see cref="KhamSucKhoeTienSuModel"/>
        /// instances if successful; otherwise, contains error information.</returns>
        public async Task<RequestHttpResponse<List<KhamSucKhoeTienSuModel>>> CreateAsync(List<KhamSucKhoeTienSuModel> model)
        {
            if (model == null)
            {
                return new RequestHttpResponse<List<KhamSucKhoeTienSuModel>>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "Vui lòng nhập đầy đủ thông tin" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var createModel = model.Select(c => MapToCRUDModel(c)).ToList();
                var response = await _httpClientService.PostAPIAsync<RequestHttpResponse<List<KhamSucKhoeTienSuModel>>>($"items/{_collection}?fields={Fields}", createModel);

                if (!response.IsSuccess)
                {
                    return new RequestHttpResponse<List<KhamSucKhoeTienSuModel>> { Errors = response.Errors };
                }

                return response.Data ?? new RequestHttpResponse<List<KhamSucKhoeTienSuModel>>();
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeTienSuModel>.CreateErrorResponse<List<KhamSucKhoeTienSuModel>>(ex);
            }
        }

        /// <summary>
        /// Asynchronously updates a list of health history records.
        /// </summary>
        /// <remarks>This method sends a PATCH request to update the specified records. Ensure that each
        /// record in the list has a valid ID.</remarks>
        /// <param name="model">A list of <see cref="KhamSucKhoeTienSuModel"/> objects to be updated. Each object must have a non-zero ID.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a boolean indicating the success of the update operation.
        /// Returns <see langword="false"/> if the input list is null, empty, or contains any records with an ID of
        /// zero.</returns>
        public async Task<RequestHttpResponse<bool>> UpdateAsync(List<KhamSucKhoeTienSuModel> model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<List<KhamSucKhoeTienSuModel>>>($"items/{_collection}?fields={Fields}", updateModel);

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeTienSuModel>.CreateErrorResponse<bool>(ex);
            }
        }

        /// <summary>
        /// Asynchronously deletes the specified health history records.
        /// </summary>
        /// <remarks>The method sends a PATCH request to mark the specified records as deleted. Ensure
        /// that the list contains valid records with non-zero IDs.</remarks>
        /// <param name="model">A list of <see cref="KhamSucKhoeTienSuModel"/> objects representing the records to be deleted. Each object
        /// must have a valid non-zero ID.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a boolean value indicating whether the deletion was
        /// successful. Returns <see langword="false"/> if the input list is null, empty, or contains invalid IDs.</returns>
        public async Task<RequestHttpResponse<bool>> DeleteAsync(List<KhamSucKhoeTienSuModel> model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<List<KhamSucKhoeTienSuModel>>>($"items/{_collection}?fields={Fields}", model.Select(c => new { id = c.id, deleted = true }));

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeTienSuModel>.CreateErrorResponse<bool>(ex);
            }
        }
    }
}