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
    public class KhamSucKhoeKetQuaCanLamSangService : IBaseDetailService<KhamSucKhoeKetQuaCanLamSangModel>
    {
        private readonly string _collection = "kham_suc_khoe_ket_qua_can_lam_sang";
        private readonly IHttpClientService _httpClientService;

        private readonly string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name"
            + ",ket_qua_cls.id,ket_qua_cls.ten_cls,ket_qua_cls.ket_qua_cls,ket_qua_cls.danh_gia_cls,ket_qua_cls.chi_so_cls"
            + ",luot_kham.id,luot_kham.ma_luot_kham";

        public KhamSucKhoeKetQuaCanLamSangService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KhamSucKhoeKetQuaCanLamSangModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic
                    ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeKetQuaCanLamSangModel>>>(url)
                    : await _httpClientService.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeKetQuaCanLamSangModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeKetQuaCanLamSangModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeKetQuaCanLamSangModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeKetQuaCanLamSangModel>.CreateErrorResponse<List<KhamSucKhoeKetQuaCanLamSangModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KhamSucKhoeKetQuaCanLamSangModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KhamSucKhoeKetQuaCanLamSangModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<KhamSucKhoeKetQuaCanLamSangModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<KhamSucKhoeKetQuaCanLamSangModel> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<KhamSucKhoeKetQuaCanLamSangModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeKetQuaCanLamSangModel>.CreateErrorResponse<KhamSucKhoeKetQuaCanLamSangModel>(ex);
            }
        }

        private static KhamSucKhoeKetQuaCanLamSangCRUDModel MapToCRUDModel(KhamSucKhoeKetQuaCanLamSangModel model)
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
                ket_qua = model.ket_qua,
                ket_qua_cls = model.ket_qua_cls?.id,
                type = model.type,
            };
        }

        /// <summary>
        /// Asynchronously creates a list of health check results by sending them to a specified API endpoint.
        /// </summary>
        /// <remarks>This method sends the provided list of health check results to a predefined API
        /// endpoint. If the API call is unsuccessful, the method returns a response with error details. In case of an
        /// exception during the operation, a generic error response is returned.</remarks>
        /// <param name="model">The list of <see cref="KhamSucKhoeKetQuaCanLamSangModel"/> objects to be created. Cannot be null.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a list of <see cref="KhamSucKhoeKetQuaCanLamSangModel"/>
        /// objects if the operation is successful. If the operation fails, the response contains error details.</returns>
        public async Task<RequestHttpResponse<List<KhamSucKhoeKetQuaCanLamSangModel>>> CreateAsync(List<KhamSucKhoeKetQuaCanLamSangModel> model)
        {
            if (model == null)
            {
                return new RequestHttpResponse<List<KhamSucKhoeKetQuaCanLamSangModel>>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "Vui lòng nhập đầy đủ thông tin" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var createModel = model.Select(c => MapToCRUDModel(c)).ToList();
                var response = await _httpClientService.PostAPIAsync<RequestHttpResponse<List<KhamSucKhoeKetQuaCanLamSangModel>>>($"items/{_collection}?fields={Fields}", createModel);

                if (!response.IsSuccess)
                {
                    return new RequestHttpResponse<List<KhamSucKhoeKetQuaCanLamSangModel>> { Errors = response.Errors };
                }

                return response.Data ?? new RequestHttpResponse<List<KhamSucKhoeKetQuaCanLamSangModel>>();
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeKetQuaCanLamSangModel>.CreateErrorResponse<List<KhamSucKhoeKetQuaCanLamSangModel>>(ex);
            }
        }

        /// <summary>
        /// Asynchronously updates a list of clinical result models.
        /// </summary>
        /// <remarks>This method sends a PATCH request to update the specified models. Ensure that each
        /// model in the list has a valid ID.</remarks>
        /// <param name="model">The list of <see cref="KhamSucKhoeKetQuaCanLamSangModel"/> instances to update. Each model must have a
        /// non-zero ID.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a boolean indicating the success of the update operation.
        /// Returns <see langword="false"/> if the input list is null, empty, or contains models with an ID of zero.</returns>
        public async Task<RequestHttpResponse<bool>> UpdateAsync(List<KhamSucKhoeKetQuaCanLamSangModel> model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<List<KhamSucKhoeKetQuaCanLamSangModel>>>($"items/{_collection}?fields={Fields}", updateModel);

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeKetQuaCanLamSangModel>.CreateErrorResponse<bool>(ex);
            }
        }

        /// <summary>
        /// Asynchronously deletes the specified health check results by marking them as deleted.
        /// </summary>
        /// <remarks>This method sends a PATCH request to mark the specified items as deleted. If the
        /// operation fails, an error response is returned.</remarks>
        /// <param name="model">A list of <see cref="KhamSucKhoeKetQuaCanLamSangModel"/> objects to be deleted. Each object must have a
        /// valid non-zero ID.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a boolean indicating the success of the operation. Returns
        /// <see langword="false"/> if the input list is null, empty, or contains invalid IDs.</returns>
        public async Task<RequestHttpResponse<bool>> DeleteAsync(List<KhamSucKhoeKetQuaCanLamSangModel> model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<List<KhamSucKhoeKetQuaCanLamSangModel>>>($"items/{_collection}?fields={Fields}", model.Select(c => new { id = c.id, deleted = true }));

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeKetQuaCanLamSangModel>.CreateErrorResponse<bool>(ex);
            }
        }
    }
}