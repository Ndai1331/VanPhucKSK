using CoreAdminWeb.Model.Contract;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Http;
using System.Net;
using CoreAdminWeb.Model.Configuration;
using Microsoft.Extensions.Options;

namespace CoreAdminWeb.Services.Contract
{
    /// <summary>
    /// Service for managing fertilizer production facilities
    /// </summary>
    public class ContractService : IBaseService<ContractModel>
    {
        private readonly string _collection = "contract";
        private const string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name"
            + ",cong_ty.id,cong_ty.name"
            + ",contract_type.id,contract_type.code"
            + ",file_hd.id,file_hd.filename_disk,file_hd.filename_download";
        private readonly IHttpClientService _httpClientService;
         private readonly IOptions<DrCoreApi> _appSettings;

        public ContractService(IHttpClientService httpClientService, IOptions<DrCoreApi> appSettings)
        {
            _httpClientService = httpClientService;
            _appSettings = appSettings;
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
        private static ContractCRUDModel MapToCRUDModel(ContractModel model)
        {
            return new()
            {
                code = model.code,
                name = model.name,
                description = model.description,
                sort = model.sort,
                status = model.status.ToString(),
                cong_ty = model.cong_ty?.id,
                contract_type = model.contract_type?.id,
                ngay_hop_dong = model.ngay_hop_dong,
                ngay_hieu_luc = model.ngay_hieu_luc,
                ngay_het_han = model.ngay_het_han,
                gia_tri_hop_dong = model.gia_tri_hop_dong,
                so_tien_tam_ung = model.so_tien_tam_ung,
                ti_le_chap_nhan_thanh_toan = model.ti_le_chap_nhan_thanh_toan,
                phe_duyet = model.phe_duyet,
                han_che = model.han_che,
                ten_khoa_phong = model.ten_khoa_phong,
                nguoi_theo_doi = model.nguoi_theo_doi,
                so_dien_thoai_lien_he = model.so_dien_thoai_lien_he,
                nguoi_gioi_thieu = model.nguoi_gioi_thieu,
                dien_thoai = model.dien_thoai,
                email = model.email,
                file_hd = model.file_hd?.id.ToString()
            };
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<ContractModel>>> GetAllAsync(string query)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<List<ContractModel>>>(url);

                if (response?.Data != null)
                {
                    if (response.Data.Data != null) {
                        foreach (var item in response.Data.Data)
                        {
                            if (item.file_hd != null)
                            {
                                item.file_hd.filename_disk = $"{_appSettings.Value.BaseUrl}assets/{item.file_hd.id}?download=";
                                item.file_hd.filename_download = $"{item.file_hd.filename_download}";
                            }
                        }
                    }
                    else
                    {
                        response.Data.Data = new List<ContractModel>();
                    }
                }

                return response.IsSuccess
                    ? new RequestHttpResponse<List<ContractModel>> { Data = response.Data?.Data }
                    : new RequestHttpResponse<List<ContractModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<List<ContractModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<ContractModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<ContractModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<ContractModel>>($"items/{_collection}/{id}?fields={Fields}");
                if (response?.Data != null)
                {
                    if (response.Data.Data != null && response.Data.Data.file_hd != null)
                    {
                        response.Data.Data.file_hd.filename_disk = $"{_appSettings.Value.BaseUrl}assets/{response.Data?.Data?.file_hd?.id}?download=";
                        response.Data!.Data.file_hd.filename_download = $"{response.Data?.Data?.file_hd?.filename_download}";
                    }
                }
                
                return response.IsSuccess
                    ? new RequestHttpResponse<ContractModel> { Data = response.Data?.Data }
                    : new RequestHttpResponse<ContractModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<ContractModel>(ex);
            }
        }

        /// <summary>
        /// Creates a new fertilizer production facility
        /// </summary>
        public async Task<RequestHttpResponse<ContractModel>> CreateAsync(ContractModel model)
        {
            if (model == null)
            {
                return new RequestHttpResponse<ContractModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "Vui lòng nhập đầy đủ thông tin" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var createModel = MapToCRUDModel(model);
                var response = await _httpClientService.PostAPIAsync<RequestHttpResponse<ContractModel>>($"items/{_collection}?fields={Fields}", createModel);

                if (!response.IsSuccess)
                {
                    return new RequestHttpResponse<ContractModel> { Errors = response.Errors };
                }

                return response.Data ?? new RequestHttpResponse<ContractModel>();
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<ContractModel>(ex);
            }
        }

        /// <summary>
        /// Updates an existing fertilizer production facility
        /// </summary>
        public async Task<RequestHttpResponse<bool>> UpdateAsync(ContractModel model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<ContractModel>>($"items/{_collection}/{model.id}?fields={Fields}", updateModel);

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
        public async Task<RequestHttpResponse<bool>> DeleteAsync(ContractModel model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<ContractModel>>($"items/{_collection}/{model.id}?fields={Fields}", new { deleted = true });

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