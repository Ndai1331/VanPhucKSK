using CoreAdminWeb.Http;
using CoreAdminWeb.Model.KhamSucKhoes;
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
    public class KhamSucKhoeChuyenKhoaService : IBaseDetailService<KhamSucKhoeChuyenKhoaModel>
    {
        private readonly string _collection = "kham_suc_khoe_kham_chuyen_khoa";
        private readonly IHttpClientService _httpClientService;

        private readonly string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name"
            + ",pl_nk_tuan_hoan.id,pl_nk_tuan_hoan.code,pl_nk_tuan_hoan.name"
            + ",pl_nk_ho_hap.id,pl_nk_ho_hap.code,pl_nk_ho_hap.name"
            + ",pl_nk_tieu_hoa.id,pl_nk_tieu_hoa.code,pl_nk_tieu_hoa.name"
            + ",pl_nk_than_tiet_nieu.id,pl_nk_than_tiet_nieu.code,pl_nk_than_tiet_nieu.name"
            + ",pl_nk_noi_tiet.id,pl_nk_noi_tiet.code,pl_nk_noi_tiet.name"
            + ",pl_nk_co_xuong_khop.id,pl_nk_co_xuong_khop.code,pl_nk_co_xuong_khop.name"
            + ",pl_nk_than_kinh.id,pl_nk_than_kinh.code,pl_nk_than_kinh.name"
            + ",pl_nk_tam_than.id,pl_nk_tam_than.code,pl_nk_tam_than.name"
            + ",pl_ngoai_khoa.id,pl_ngoai_khoa.code,pl_ngoai_khoa.name"
            + ",pl_da_lieu.id,pl_da_lieu.code,pl_da_lieu.name"
            + ",pl_mat.id,pl_mat.code,pl_mat.name"
            + ",pl_tmh.id,pl_tmh.code,pl_tmh.name"
            + ",pl_rhm.id,pl_rhm.code,pl_rhm.name"
            + ",luot_kham.id,luot_kham.ma_luot_kham";



        public KhamSucKhoeChuyenKhoaService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic
                    ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>>>(url)
                    : await _httpClientService.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeChuyenKhoaModel>.CreateErrorResponse<List<KhamSucKhoeChuyenKhoaModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KhamSucKhoeChuyenKhoaModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KhamSucKhoeChuyenKhoaModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<KhamSucKhoeChuyenKhoaModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<KhamSucKhoeChuyenKhoaModel> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<KhamSucKhoeChuyenKhoaModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeChuyenKhoaModel>.CreateErrorResponse<KhamSucKhoeChuyenKhoaModel>(ex);
            }
        }

        /// <summary>
        /// Maps a <see cref="KhamSucKhoeChuyenKhoaModel"/> to a <see cref="KhamSucKhoeChuyenKhoaCRUDModel"/>.
        /// </summary>
        /// <remarks>This method performs a direct mapping of properties from the <paramref name="model"/>
        /// to a new instance of <see cref="KhamSucKhoeChuyenKhoaCRUDModel"/>. Nullable properties in the source model
        /// are handled appropriately, with null checks applied where necessary.</remarks>
        /// <param name="model">The source model containing health examination data to be mapped.</param>
        /// <returns>A <see cref="KhamSucKhoeChuyenKhoaCRUDModel"/> populated with data from the specified <paramref
        /// name="model"/>.</returns>
        private static KhamSucKhoeChuyenKhoaCRUDModel MapToCRUDModel(KhamSucKhoeChuyenKhoaModel model)
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
                benh_mat = model.benh_mat,
                thi_luc_co_kinh_phai = model.thi_luc_co_kinh_phai,
                thi_luc_co_kinh_trai = model.thi_luc_co_kinh_trai,
                thi_luc_khong_kinh_phai = model.thi_luc_khong_kinh_phai,
                thi_luc_khong_kinh_trai = model.thi_luc_khong_kinh_trai,
                kq_da_lieu = model.kq_da_lieu,
                pl_da_lieu = model.pl_da_lieu?.id,
                kq_ngoai_khoa = model.kq_ngoai_khoa,
                pl_ngoai_khoa = model.pl_ngoai_khoa?.id,
                kq_nk_tam_than = model.kq_nk_tam_than,
                pl_nk_tam_than = model.pl_nk_tam_than?.id,
                chu_ky_tam_than = model.chu_ky_tam_than,
                bs_tam_than = model.bs_tam_than,
                kq_nk_than_kinh = model.kq_nk_than_kinh,
                pl_nk_than_kinh = model.pl_nk_than_kinh?.id,
                chu_ky_than_kinh = model.chu_ky_than_kinh,
                bs_than_kinh = model.bs_than_kinh,
                kq_nk_co_xuong_khop = model.kq_nk_co_xuong_khop,
                pl_nk_co_xuong_khop = model.pl_nk_co_xuong_khop?.id,
                chu_ky_co_xuong_khop = model.chu_ky_co_xuong_khop,
                bs_co_xuong_khop = model.bs_co_xuong_khop,
                kq_nk_noi_tiet = model.kq_nk_noi_tiet,
                pl_nk_noi_tiet = model.pl_nk_noi_tiet?.id,
                chu_ky_noi_tiet = model.chu_ky_noi_tiet,
                bs_noi_tiet = model.bs_noi_tiet,
                kq_nk_than_tiet_nieu = model.kq_nk_than_tiet_nieu,
                pl_nk_than_tiet_nieu = model.pl_nk_than_tiet_nieu?.id,
                chu_ky_than_tiet_nieu = model.chu_ky_than_tiet_nieu,
                bs_than_tiet_nieu = model.bs_than_tiet_nieu,
                kq_nk_tieu_hoa = model.kq_nk_tieu_hoa,
                pl_nk_tieu_hoa = model.pl_nk_tieu_hoa?.id,
                chu_ky_tieu_hoa = model.chu_ky_tieu_hoa,
                bs_tieu_hoa = model.bs_tieu_hoa,
                kq_nk_ho_hap = model.kq_nk_ho_hap,
                pl_nk_ho_hap = model.pl_nk_ho_hap?.id,
                chu_ky_ho_hap = model.chu_ky_ho_hap,
                bs_ho_hap = model.bs_ho_hap,
                kq_nk_tuan_hoan = model.kq_nk_tuan_hoan,
                pl_nk_tuan_hoan = model.pl_nk_tuan_hoan?.id,
                chu_ky_tuan_hoan = model.chu_ky_tuan_hoan,
                bs_tuan_hoan = model.bs_tuan_hoan,
                pl_mat = model.pl_mat?.id,
                chu_ky_mat = model.chu_ky_mat,
                bs_mat = model.bs_mat,
                kq_rhm_ham_duoi = model.kq_rhm_ham_duoi,
                kq_rhm_ham_tren = model.kq_rhm_ham_tren,
                pl_rhm = model.pl_rhm?.id,
                chu_ky_rhm = model.chu_ky_rhm,
                bs_rhm = model.bs_rhm,
                chu_ky_ket_luan = model.chu_ky_ket_luan,
                bs_ket_luan = model.bs_ket_luan,
                benh_rhm = model.benh_rhm,
                benh_tai_mui_hong = model.benh_tai_mui_hong,
                tmh_nt_trai = model.tmh_nt_trai,
                tmh_ntham_trai = model.tmh_ntham_trai,
                tmh_nt_phai = model.tmh_nt_phai,
                tmh_ntham_phai = model.tmh_ntham_phai,
                pl_tmh = model.pl_tmh?.id,
                bs_ngoai_khoa = model.bs_ngoai_khoa,
                bs_tmh = model.bs_tmh,
                ma_luot_kham = model.ma_luot_kham,
                chu_ky_ngoai_khoa = model.chu_ky_ngoai_khoa,
                chu_ky_tmh = model.chu_ky_tmh
            };
        }

        /// <summary>
        /// Asynchronously creates a list of specialized health check models by sending them to a specified API
        /// endpoint.
        /// </summary>
        /// <remarks>This method sends the provided models to a predefined API endpoint using an HTTP POST
        /// request. If the API call is unsuccessful, the method returns the errors encountered during the
        /// request.</remarks>
        /// <param name="model">The list of <see cref="KhamSucKhoeChuyenKhoaModel"/> instances to be created. Cannot be null.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a list of <see cref="KhamSucKhoeChuyenKhoaModel"/> if the
        /// operation is successful. If the operation fails, the response contains error details.</returns>
        public async Task<RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>>> CreateAsync(List<KhamSucKhoeChuyenKhoaModel> model)
        {
            if (model == null)
            {
                return new RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "Vui lòng nhập đầy đủ thông tin" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var createModel = model.Select(c => MapToCRUDModel(c)).ToList();
                var response = await _httpClientService.PostAPIAsync<RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>>>($"items/{_collection}?fields={Fields}", createModel);

                if (!response.IsSuccess)
                {
                    return new RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>> { Errors = response.Errors };
                }

                return response.Data ?? new RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>>();
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeChuyenKhoaModel>.CreateErrorResponse<List<KhamSucKhoeChuyenKhoaModel>>(ex);
            }
        }

        /// <summary>
        /// Asynchronously updates a list of health check models in the specified collection.
        /// </summary>
        /// <remarks>This method sends a PATCH request to update the specified models. Ensure that each
        /// model in the list has a valid ID.</remarks>
        /// <param name="model">A list of <see cref="KhamSucKhoeChuyenKhoaModel"/> objects to be updated. Each model must have a non-zero
        /// ID.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a boolean indicating the success of the update operation.
        /// Returns <see langword="false"/> if the input list is null, empty, or contains models with an ID of zero.</returns>
        public async Task<RequestHttpResponse<bool>> UpdateAsync(List<KhamSucKhoeChuyenKhoaModel> model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>>>($"items/{_collection}?fields={Fields}", updateModel);

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeChuyenKhoaModel>.CreateErrorResponse<bool>(ex);
            }
        }

        /// <summary>
        /// Asynchronously deletes the specified health check records.
        /// </summary>
        /// <remarks>This method sends a PATCH request to mark the specified records as deleted. Ensure
        /// that each record in the list has a valid ID.</remarks>
        /// <param name="model">A list of <see cref="KhamSucKhoeChuyenKhoaModel"/> objects representing the records to be deleted. Each
        /// object must have a non-zero ID.</param>
        /// <returns>A <see cref="RequestHttpResponse{T}"/> containing a boolean indicating the success of the operation. Returns
        /// <see langword="false"/> if the input list is null, empty, or contains records with an ID of zero.</returns>
        public async Task<RequestHttpResponse<bool>> DeleteAsync(List<KhamSucKhoeChuyenKhoaModel> model)
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
                var response = await _httpClientService.PatchAPIAsync<RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>>>($"items/{_collection}?fields={Fields}", model.Select(c => new { id = c.id, deleted = true }));

                return new RequestHttpResponse<bool>
                {
                    Data = response.IsSuccess,
                    Errors = response.Errors
                };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeChuyenKhoaModel>.CreateErrorResponse<bool>(ex);
            }
        }
    }
}