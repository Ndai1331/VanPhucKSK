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
    public class KhamSucKhoeNgheNghiepService : IBaseDetailService<KhamSucKhoeNgheNghiepModel>
    {
        private readonly string _collection = "kham_suc_khoe_nghe_nghiep";
        private readonly IHttpClientService _httpClientService;

        private readonly string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name"
            + ",luot_kham.id,luot_kham.ma_luot_kham";

        public KhamSucKhoeNgheNghiepService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KhamSucKhoeNgheNghiepModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic
                    ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeNgheNghiepModel>>>(url)
                    : await _httpClientService.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeNgheNghiepModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeNgheNghiepModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeNgheNghiepModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeNgheNghiepModel>.CreateErrorResponse<List<KhamSucKhoeNgheNghiepModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KhamSucKhoeNgheNghiepModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KhamSucKhoeNgheNghiepModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<KhamSucKhoeNgheNghiepModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<KhamSucKhoeNgheNghiepModel> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<KhamSucKhoeNgheNghiepModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeNgheNghiepModel>.CreateErrorResponse<KhamSucKhoeNgheNghiepModel>(ex);
            }
        }

        /// <summary>
        /// Maps a <see cref="KhamSucKhoeNgheNghiepModel"/> to a <see cref="KhamSucKhoeNgheNghiepCRUDModel"/>.
        /// </summary>
        /// <remarks>This method creates a new instance of <see cref="KhamSucKhoeNgheNghiepCRUDModel"/>
        /// and copies relevant properties from the provided <paramref name="model"/>. The <c>luot_kham</c> property is
        /// set to the <c>id</c> of the <c>luot_kham</c> object in the source model, if it is not null.</remarks>
        /// <param name="model">The source model containing health examination data to be mapped.</param>
        /// <returns>A <see cref="KhamSucKhoeNgheNghiepCRUDModel"/> populated with data from the specified <paramref
        /// name="model"/>.</returns>
        private static KhamSucKhoeNgheNghiepCRUDModel MapToCRUDModel(KhamSucKhoeNgheNghiepModel model)
        {
            return new()
            {
                active = model.active,
                code = model.code,
                description = model.description,
                name = model.name,
                sort = model.sort,
                status = model.status.ToString(),
                luot_kham = model.luot_kham?.id,
                den_ngay = model.den_ngay,
                ma_luot_kham = model.ma_luot_kham,
                ngay_vao_lam = model.ngay_vao_lam,
                nghe_nghiep = model.nghe_nghiep,
                noi_cong_tac = model.noi_cong_tac
            };
        }

        /// <summary>
        /// Asynchronously creates a list of health check models by sending them to a specified API endpoint.
        /// </summary>
        /// <remarks>If the <paramref name="model"/> is null, the method returns a response with a <see
        /// cref="HttpStatusCode.BadRequest"/> status and an error message. In case of an exception during the
        /// operation, an error response is generated.</remarks>
        /// <param name="model">The list of <see cref="KhamSucKhoeNgheNghiepModel"/> instances to be created. Cannot be null.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing the list of created <see
        /// cref="KhamSucKhoeNgheNghiepModel"/> instances if successful; otherwise, contains error information.</returns>
        public async Task<RequestHttpResponse<List<KhamSucKhoeNgheNghiepModel>>> CreateAsync(List<KhamSucKhoeNgheNghiepModel> model)
        {
            if (model == null)
            {
                return new RequestHttpResponse<List<KhamSucKhoeNgheNghiepModel>>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "Vui lòng nhập đầy đủ thông tin" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var createModel = model.Select(c => MapToCRUDModel(c)).ToList();
                var response = await _httpClientService.PostAPIAsync<RequestHttpResponse<List<KhamSucKhoeNgheNghiepModel>>>($"items/{_collection}?fields={Fields}", createModel);

                if (!response.IsSuccess)
                {
                    return new RequestHttpResponse<List<KhamSucKhoeNgheNghiepModel>> { Errors = response.Errors };
                }

                return response.Data ?? new RequestHttpResponse<List<KhamSucKhoeNgheNghiepModel>>();
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeNgheNghiepModel>.CreateErrorResponse<List<KhamSucKhoeNgheNghiepModel>>(ex);
            }
        }

        /// <summary>
        /// Asynchronously updates a list of health and occupational safety records.
        /// </summary>
        /// <remarks>This method sends a PATCH request to update the specified records. Ensure that each
        /// record in the list has a valid ID before calling this method.</remarks>
        /// <param name="model">The list of <see cref="KhamSucKhoeNgheNghiepModel"/> objects to update. Each object must have a valid
        /// non-zero ID.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a boolean indicating the success of the update operation.
        /// Returns <see langword="false"/> if the input list is null, empty, or contains any records with an ID of
        /// zero.</returns>
        public async Task<RequestHttpResponse<bool>> UpdateAsync(List<KhamSucKhoeNgheNghiepModel> model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<List<KhamSucKhoeNgheNghiepModel>>>($"items/{_collection}?fields={Fields}", updateModel);

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeNgheNghiepModel>.CreateErrorResponse<bool>(ex);
            }
        }

        /// <summary>
        /// Asynchronously deletes the specified health check records.
        /// </summary>
        /// <param name="model">A list of <see cref="KhamSucKhoeNgheNghiepModel"/> objects representing the records to be deleted. Each
        /// object must have a valid non-zero ID.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a boolean indicating the success of the operation. Returns
        /// <see langword="false"/> with an error message if the input list is null, empty, or contains invalid IDs.</returns>
        public async Task<RequestHttpResponse<bool>> DeleteAsync(List<KhamSucKhoeNgheNghiepModel> model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<List<KhamSucKhoeNgheNghiepModel>>>($"items/{_collection}?fields={Fields}", model.Select(c => new { id = c.id, deleted = true }));

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeNgheNghiepModel>.CreateErrorResponse<bool>(ex);
            }
        }
    }
}