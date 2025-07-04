using CoreAdminWeb.Model;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Http;
using System.Net;

namespace CoreAdminWeb.Services.DanhMucDungChung
{
    /// <summary>
    /// Service for managing XaPhuong
    /// </summary>
    public class XaPhuongService : IBaseService<XaPhuongModel>
    {
        private readonly string _collection = "xa_phuong";
        private readonly IHttpClientService _httpClientService;
        private const string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name,tinh.id,tinh.name";

        public XaPhuongService(IHttpClientService httpClientService)
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
        private static XaPhuongCRUDModel MapToCRUDModel(XaPhuongModel model)
        {
            return new()
            {
                code = model.code,
                name = model.name,
                description = model.description,
                sort = model.sort,
                tinh = model.tinh?.id ?? 0
            };
        }

        /// <summary>
        /// Gets all XaPhuong
        /// </summary>
        public async Task<RequestHttpResponse<List<XaPhuongModel>>> GetAllAsync(string query)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<List<XaPhuongModel>>>(url);
                
                return response.IsSuccess 
                    ? new RequestHttpResponse<List<XaPhuongModel>> { Data = response.Data.Data }
                    : new RequestHttpResponse<List<XaPhuongModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<List<XaPhuongModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a XaPhuong by ID
        /// </summary>
        public async Task<RequestHttpResponse<XaPhuongModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<XaPhuongModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<XaPhuongModel>>($"items/{_collection}/{id}?fields={Fields}");
                
                return response.IsSuccess
                    ? new RequestHttpResponse<XaPhuongModel> { Data = response.Data.Data }
                    : new RequestHttpResponse<XaPhuongModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<XaPhuongModel>(ex);
            }
        }

        /// <summary>
        /// Creates a new XaPhuong
        /// </summary>
        public async Task<RequestHttpResponse<XaPhuongModel>> CreateAsync(XaPhuongModel model)
        {
            if (model == null)
            {
                return new RequestHttpResponse<XaPhuongModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "Vui lòng nhập đầy đủ thông tin" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var createModel = MapToCRUDModel(model);
                var response = await _httpClientService.PostAPIAsync<RequestHttpResponse<XaPhuongCRUDModel>>($"items/{_collection}", createModel);

                if (!response.IsSuccess)
                {
                    return new RequestHttpResponse<XaPhuongModel> { Errors = response.Errors };
                }

                return new RequestHttpResponse<XaPhuongModel>
                {
                    Data = new()
                    {
                        code = response.Data.Data.code,
                        name = response.Data.Data.name
                    }
                };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<XaPhuongModel>(ex);
            }
        }

        /// <summary>
        /// Updates an existing XaPhuong
        /// </summary>
        public async Task<RequestHttpResponse<bool>> UpdateAsync(XaPhuongModel model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<XaPhuongCRUDModel>>($"items/{_collection}/{model.id}", updateModel);

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
        /// Deletes a XaPhuong
        /// </summary>
        public async Task<RequestHttpResponse<bool>> DeleteAsync(XaPhuongModel model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<XaPhuongCRUDModel>>($"items/{_collection}/{model.id}", new { deleted = true });

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