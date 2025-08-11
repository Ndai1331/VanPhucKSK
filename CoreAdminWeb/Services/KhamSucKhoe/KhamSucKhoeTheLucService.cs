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
    public class KhamSucKhoeTheLucService : IBaseDetailService<KhamSucKhoeTheLucModel>
    {
        private readonly string _collection = "kham_suc_khoe_the_luc";
        private readonly IHttpClientService _httpClientService;

        private readonly string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name"
            + ",phan_loai.id,phan_loai.name"
            + ",luot_kham.id,luot_kham.ma_luot_kham";

        public KhamSucKhoeTheLucService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KhamSucKhoeTheLucModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic
                    ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeTheLucModel>>>(url)
                    : await _httpClientService.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeTheLucModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeTheLucModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeTheLucModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeTheLucModel>.CreateErrorResponse<List<KhamSucKhoeTheLucModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KhamSucKhoeTheLucModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KhamSucKhoeTheLucModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<KhamSucKhoeTheLucModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<KhamSucKhoeTheLucModel> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<KhamSucKhoeTheLucModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeTheLucModel>.CreateErrorResponse<KhamSucKhoeTheLucModel>(ex);
            }
        }

        /// <summary>
        /// Maps a <see cref="KhamSucKhoeTheLucModel"/> to a <see cref="KhamSucKhoeTheLucCRUDModel"/>.
        /// </summary>
        /// <remarks>This method performs a direct mapping of properties from the source model to the CRUD
        /// model, including optional properties such as <c>luot_kham</c> and <c>phan_loai</c> which are mapped from
        /// their respective IDs if available.</remarks>
        /// <param name="model">The source model containing health and physical examination data to be mapped.</param>
        /// <returns>A <see cref="KhamSucKhoeTheLucCRUDModel"/> populated with data from the specified <paramref name="model"/>.</returns>
        private static KhamSucKhoeTheLucCRUDModel MapToCRUDModel(KhamSucKhoeTheLucModel model)
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
                bmi = model.bmi,
                chieu_cao = model.chieu_cao,
                can_nang = model.can_nang,
                huyet_ap = model.huyet_ap,
                nhip_tho = model.nhip_tho,
                mach = model.mach,
                ma_luot_kham = model.ma_luot_kham,
                phan_loai = model.phan_loai?.id
            };
        }

        /// <summary>
        /// Asynchronously creates a list of health and fitness records by sending them to a specified API endpoint.
        /// </summary>
        /// <remarks>This method sends the provided list of models to a predefined API endpoint using an
        /// HTTP POST request. If the request is unsuccessful, the response will include error information.</remarks>
        /// <param name="model">The list of <see cref="KhamSucKhoeTheLucModel"/> instances to be created. Cannot be null.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a list of <see cref="KhamSucKhoeTheLucModel"/> if the
        /// operation is successful. If the operation fails, the response contains error details.</returns>
        public async Task<RequestHttpResponse<List<KhamSucKhoeTheLucModel>>> CreateAsync(List<KhamSucKhoeTheLucModel> model)
        {
            if (model == null)
            {
                return new RequestHttpResponse<List<KhamSucKhoeTheLucModel>>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "Vui lòng nhập đầy đủ thông tin" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var createModel = model.Select(c => MapToCRUDModel(c)).ToList();
                var response = await _httpClientService.PostAPIAsync<RequestHttpResponse<List<KhamSucKhoeTheLucModel>>>($"items/{_collection}?fields={Fields}", createModel);

                if (!response.IsSuccess)
                {
                    return new RequestHttpResponse<List<KhamSucKhoeTheLucModel>> { Errors = response.Errors };
                }

                return response.Data ?? new RequestHttpResponse<List<KhamSucKhoeTheLucModel>>();
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeTheLucModel>.CreateErrorResponse<List<KhamSucKhoeTheLucModel>>(ex);
            }
        }

        /// <summary>
        /// Asynchronously updates a list of health and fitness records.
        /// </summary>
        /// <remarks>This method sends a PATCH request to update the specified records. If the operation
        /// fails, the response will include error details.</remarks>
        /// <param name="model">A list of <see cref="KhamSucKhoeTheLucModel"/> objects to be updated. Each object must have a non-zero ID.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a boolean indicating the success of the update operation.
        /// Returns <see langword="false"/> if the input list is null, empty, or contains any records with an ID of
        /// zero.</returns>
        public async Task<RequestHttpResponse<bool>> UpdateAsync(List<KhamSucKhoeTheLucModel> model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<List<KhamSucKhoeTheLucModel>>>($"items/{_collection}?fields={Fields}", updateModel);

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeTheLucModel>.CreateErrorResponse<bool>(ex);
            }
        }

        /// <summary>
        /// Asynchronously deletes the specified health and fitness records.
        /// </summary>
        /// <remarks>This method sends a PATCH request to mark the specified records as deleted. Ensure
        /// that the list contains valid records with non-zero IDs.</remarks>
        /// <param name="model">A list of <see cref="KhamSucKhoeTheLucModel"/> objects representing the records to be deleted. Each object
        /// must have a valid non-zero ID.</param>
        /// <returns>A <see cref="RequestHttpResponse{Boolean}"/> indicating the success or failure of the delete operation.
        /// Returns <see langword="false"/> if the input list is null, empty, or contains records with an ID of zero.</returns>
        public async Task<RequestHttpResponse<bool>> DeleteAsync(List<KhamSucKhoeTheLucModel> model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<List<KhamSucKhoeTheLucModel>>>($"items/{_collection}?fields={Fields}", model.Select(c => new { id = c.id, deleted = true }));

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeTheLucModel>.CreateErrorResponse<bool>(ex);
            }
        }
    }
}