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
    public class KhamSucKhoeCanLamSanService : IBaseDetailService<KhamSucKhoeCanLamSangModel>
    {
        private readonly string _collection = "kham_suc_khoe_can_lam_sang";
        private readonly IHttpClientService _httpClientService;

        private readonly string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name"
            + ",luot_kham.id,luot_kham.ma_luot_kham"
            + ",file_cls.id,file_cls.filename_disk,file_cls.filename_download";

        public KhamSucKhoeCanLamSanService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KhamSucKhoeCanLamSangModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic
                    ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeCanLamSangModel>>>(url)
                    : await _httpClientService.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeCanLamSangModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeCanLamSangModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeCanLamSangModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeCanLamSangModel>.CreateErrorResponse<List<KhamSucKhoeCanLamSangModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KhamSucKhoeCanLamSangModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KhamSucKhoeCanLamSangModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<KhamSucKhoeCanLamSangModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<KhamSucKhoeCanLamSangModel> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<KhamSucKhoeCanLamSangModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeCanLamSangModel>.CreateErrorResponse<KhamSucKhoeCanLamSangModel>(ex);
            }
        }

        /// <summary>
        /// Maps a <see cref="KhamSucKhoeCanLamSangModel"/> to a <see cref="KhamSucKhoeCanLamSangCRUDModel"/>.
        /// </summary>
        /// <param name="model">The source model containing the data to be mapped. Cannot be null.</param>
        /// <returns>A new instance of <see cref="KhamSucKhoeCanLamSangCRUDModel"/> populated with data from the specified
        /// <paramref name="model"/>.</returns>
        private static KhamSucKhoeCanLamSangCRUDModel MapToCRUDModel(KhamSucKhoeCanLamSangModel model)
        {
            return new()
            {
                active = model.active,
                code = model.code,
                description = model.description,
                name = model.name,
                sort = model.sort,
                ma_cls = model.ma_cls,
                ten_cls = model.ten_cls,
                bs_cls = model.bs_cls,
                chi_so_cls = model.chi_so_cls,
                chu_ky_cls = model.chu_ky_cls,
                file_cls = model.file_cls?.id,
                ket_qua_cls = model.ket_qua_cls,
                luot_kham = model.luot_kham?.id,
                ma_luot_kham = model.ma_luot_kham,
                status = model.status.ToString(),
                url_cls = model.url_cls,
                danh_gia_cls = model.danh_gia_cls,
                SendMail = model.SendMail,
                SendNotification = model.SendNotification,
                SendZaloOa = model.SendZaloOa
            };
        }

        /// <summary>
        /// Asynchronously creates a list of health check laboratory models by sending them to a specified API endpoint.
        /// </summary>
        /// <remarks>This method sends a POST request to the API to create the specified models. If the
        /// operation fails, the response will contain error details.</remarks>
        /// <param name="model">The list of <see cref="KhamSucKhoeCanLamSangModel"/> instances to be created. Cannot be null.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing the list of created <see
        /// cref="KhamSucKhoeCanLamSangModel"/> instances if successful; otherwise, contains error information.</returns>
        public async Task<RequestHttpResponse<List<KhamSucKhoeCanLamSangModel>>> CreateAsync(List<KhamSucKhoeCanLamSangModel> model)
        {
            if (model == null)
            {
                return new RequestHttpResponse<List<KhamSucKhoeCanLamSangModel>>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "Vui lòng nhập đầy đủ thông tin" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var createModel = model.Select(c => MapToCRUDModel(c)).ToList();
                var response = await _httpClientService.PostAPIAsync<RequestHttpResponse<List<KhamSucKhoeCanLamSangModel>>>($"items/{_collection}?fields={Fields}", createModel);

                if (!response.IsSuccess)
                {
                    return new RequestHttpResponse<List<KhamSucKhoeCanLamSangModel>> { Errors = response.Errors };
                }

                return response.Data ?? new RequestHttpResponse<List<KhamSucKhoeCanLamSangModel>>();
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeCanLamSangModel>.CreateErrorResponse<List<KhamSucKhoeCanLamSangModel>>(ex);
            }
        }

        /// <summary>
        /// Asynchronously updates a list of health check models in the database.
        /// </summary>
        /// <remarks>This method sends a PATCH request to update the specified models. Ensure that each
        /// model in the list has a valid ID.</remarks>
        /// <param name="model">A list of <see cref="KhamSucKhoeCanLamSangModel"/> objects to be updated. Each model must have a non-zero ID.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a boolean indicating the success of the update operation.
        /// Returns <see langword="false"/> if the input list is null, empty, or contains models with an ID of zero.</returns>
        public async Task<RequestHttpResponse<bool>> UpdateAsync(List<KhamSucKhoeCanLamSangModel> model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<List<KhamSucKhoeCanLamSangModel>>>($"items/{_collection}?fields={Fields}", updateModel);

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeCanLamSangModel>.CreateErrorResponse<bool>(ex);
            }
        }

        /// <summary>
        /// Asynchronously deletes the specified health check records.
        /// </summary>
        /// <remarks>This method sends a PATCH request to mark the specified records as deleted. Ensure
        /// that the <paramref name="model"/> parameter is not null and contains valid records.</remarks>
        /// <param name="model">A list of <see cref="KhamSucKhoeCanLamSangModel"/> objects representing the records to be deleted. Each
        /// object must have a valid non-zero ID.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a boolean indicating the success of the operation. Returns
        /// <see langword="false"/> if the input list is null, empty, or contains records with an ID of zero.</returns>
        public async Task<RequestHttpResponse<bool>> DeleteAsync(List<KhamSucKhoeCanLamSangModel> model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<List<KhamSucKhoeCanLamSangModel>>>($"items/{_collection}?fields={Fields}", model.Select(c => new { id = c.id, deleted = true }));

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeCanLamSangModel>.CreateErrorResponse<bool>(ex);
            }
        }
    }
}