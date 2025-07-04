using CoreAdminWeb.Model;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.RequestHttp;
using CoreAdminWeb.Services.BaseServices;
using System.Net;

namespace CoreAdminWeb.Services
{
    /// <summary>
    /// Service for managing fertilizer production facilities
    /// </summary>
    public class LoaiDinhMucService : IBaseService<LoaiDinhMucModel>
    {
        private readonly string _collection = "loai_dinh_muc";
        private const string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name";

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
        private static LoaiDinhMucCRUDModel MapToCRUDModel(LoaiDinhMucModel model)
        {
            return new()
            {
                code = model.code,
                name = model.name,
                description = model.description,
                sort = model.sort,
            };
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<LoaiDinhMucModel>>> GetAllAsync(string query)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = await RequestClient.GetAPIAsync<RequestHttpResponse<List<LoaiDinhMucModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<LoaiDinhMucModel>> { Data = response.Data.Data }
                    : new RequestHttpResponse<List<LoaiDinhMucModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<List<LoaiDinhMucModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<LoaiDinhMucModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<LoaiDinhMucModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await RequestClient.GetAPIAsync<RequestHttpResponse<LoaiDinhMucModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<LoaiDinhMucModel> { Data = response.Data.Data }
                    : new RequestHttpResponse<LoaiDinhMucModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<LoaiDinhMucModel>(ex);
            }
        }

        /// <summary>
        /// Creates a new fertilizer production facility
        /// </summary>
        public async Task<RequestHttpResponse<LoaiDinhMucModel>> CreateAsync(LoaiDinhMucModel model)
        {
            if (model == null)
            {
                return new RequestHttpResponse<LoaiDinhMucModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "Vui lòng nhập đầy đủ thông tin" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var createModel = MapToCRUDModel(model);
                var response = await RequestClient.PostAPIAsync<RequestHttpResponse<LoaiDinhMucCRUDModel>>($"items/{_collection}", createModel);

                if (!response.IsSuccess)
                {
                    return new RequestHttpResponse<LoaiDinhMucModel> { Errors = response.Errors };
                }

                return new RequestHttpResponse<LoaiDinhMucModel>
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
                return CreateErrorResponse<LoaiDinhMucModel>(ex);
            }
        }

        /// <summary>
        /// Updates an existing fertilizer production facility
        /// </summary>
        public async Task<RequestHttpResponse<bool>> UpdateAsync(LoaiDinhMucModel model)
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
                var response = await RequestClient.PatchAPIAsync<RequestHttpResponse<LoaiDinhMucCRUDModel>>($"items/{_collection}/{model.id}", updateModel);

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
        public async Task<RequestHttpResponse<bool>> DeleteAsync(LoaiDinhMucModel model)
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
                var response = await RequestClient.PatchAPIAsync<RequestHttpResponse<LoaiDinhMucCRUDModel>>($"items/{_collection}/{model.id}", new { deleted = true });

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