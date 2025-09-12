
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.KhamSucKhoes;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net;

namespace CoreAdminWeb.Controllers.Api;

/// <summary>
/// DanhSachDoan API Controller
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class KhamSucKhoeController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public KhamSucKhoeController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("get-data-san-phu-khoa-by-ma-luot-kham")]
    public async Task<IActionResult> GetSanPhuKhoaByMaLuotKham([FromQuery] string? luotKham,
                                                    [FromQuery] string? maLuotKham,
                                                    [FromQuery] int offset = 0,
                                                    [FromQuery] int limit = 10)
    {

        var response = new RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>>();
        try
        {
            // Validate parameters
            var validLimit = limit <= 0 ? 10 : limit;
            var validOffset = offset < 0 ? 0 : offset;
        
            var where = luotKham != null ? " WHERE ksk.luot_kham = @luotKham" : " WHERE sksk.ma_luot_kham = @maLuotKham";

            // Query đếm tổng số bản ghi
            var countSql = @"
            SELECT COUNT(*) as TotalCount
            FROM kham_suc_khoe_san_phu_khoa ksk
            LEFT JOIN SoKhamSucKhoe sksk ON sksk.id = ksk.luot_kham
            " + where;

            // Query lấy dữ liệu với phân trang
            var dataSql = @"
            SELECT 
                ksk.*,
                plsk.id as phan_loai_id,
                plsk.name as phan_loai_name,
                plsk.code as phan_loai_code
            FROM kham_suc_khoe_san_phu_khoa ksk
            LEFT JOIN SoKhamSucKhoe sksk ON sksk.id = ksk.luot_kham
            LEFT JOIN phan_loai_suc_khoe plsk ON 
                (ksk.phan_loai IS NOT NULL AND plsk.id = ksk.phan_loai)
                OR
                (ksk.phan_loai IS NULL AND plsk.code = ksk.ma_phan_loai)
            " + where + @"
            ORDER BY ksk.id
            OFFSET @offset ROWS 
            FETCH NEXT @limit ROWS ONLY";

            var results = new List<KhamSucKhoeSanPhuKhoaModel>();
            int totalCount = 0;

            await _context.Database.OpenConnectionAsync();

            // Lấy tổng số bản ghi
            using (var countCommand = _context.Database.GetDbConnection().CreateCommand())
            {
                countCommand.CommandText = countSql;
                if (!string.IsNullOrEmpty(luotKham))
                {
                    countCommand.Parameters.Add(new SqlParameter("@luotKham", luotKham));
                }
                if (!string.IsNullOrEmpty(maLuotKham))
                {
                    countCommand.Parameters.Add(new SqlParameter("@maLuotKham", maLuotKham));
                }
                var countResult = await countCommand.ExecuteScalarAsync();
                totalCount = Convert.ToInt32(countResult ?? 0);
            }

            // Lấy dữ liệu với phân trang
            using (var dataCommand = _context.Database.GetDbConnection().CreateCommand())
            {
                dataCommand.CommandText = dataSql;
                if (!string.IsNullOrEmpty(luotKham))
                {
                    dataCommand.Parameters.Add(new SqlParameter("@luotKham", luotKham));
                }
                if (!string.IsNullOrEmpty(maLuotKham))
                {
                    dataCommand.Parameters.Add(new SqlParameter("@maLuotKham", maLuotKham));
                }
                dataCommand.Parameters.Add(new SqlParameter("@offset", validOffset));
                dataCommand.Parameters.Add(new SqlParameter("@limit", validLimit));

                using (var reader = await dataCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var item = new KhamSucKhoeSanPhuKhoaModel
                        {
                            id = reader["id"] as int? ?? 0,
                            ma_luot_kham = reader["ma_luot_kham"]?.ToString(),
                            luot_kham = new SoKhamSucKhoeModel()
                            {
                                id = reader["luot_kham"] as int? ?? 0,
                                ma_luot_kham = reader["ma_luot_kham"]?.ToString()
                            },
                            tien_su_san_phu_khoa = reader["tien_su_san_phu_khoa"]?.ToString(),
                            tuoi_bat_dau_kinh = reader["tuoi_bat_dau_kinh"] as int?,
                            tinh_chat_kinh = reader["tinh_chat_kinh"]?.ToString(),
                            chu_ky_kinh = reader["chu_ky_kinh"]?.ToString(),
                            luong_kinh = reader["luong_kinh"]?.ToString(),
                            dau_bung_kinh = reader["dau_bung_kinh"] as bool?,
                            da_lap_gia_dinh = reader["da_lap_gia_dinh"] as bool?,
                            para = reader["para"]?.ToString(),
                            so_lan_mo_san_phu_khoa = reader["so_lan_mo_san_phu_khoa"] as int?,
                            mo_san_phu_khoa_ghi_ro = reader["mo_san_phu_khoa_ghi_ro"]?.ToString(),
                            ap_dung_bptt = reader["ap_dung_bptt"] as bool?,
                            bptt_ghi_ro = reader["bptt_ghi_ro"]?.ToString(),
                            ket_qua = reader["ket_qua"]?.ToString(),
                            phan_loai = reader["phan_loai_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = reader["phan_loai_id"] as int? ?? 0,
                                name = reader["phan_loai_name"]?.ToString(),
                                code = reader["phan_loai_code"]?.ToString()
                            } : null,
                            nguoi_ket_luan = reader["nguoi_ket_luan"]?.ToString(),
                            chu_ky = reader["chu_ky"]?.ToString()
                        };

                        results.Add(item);
                    }
                }
            }

            response.Data = results;
            response.Meta = CreateMetaDataWithPagination(totalCount, results.Count, validOffset, validLimit);
            response.StatusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.Errors.Add(CreateErrorResponse(ex));
            response.StatusCode = HttpStatusCode.InternalServerError;
            return BadRequest(response);
        }
        finally
        {
            if (_context.Database.GetDbConnection().State == ConnectionState.Open)
            {
                await _context.Database.CloseConnectionAsync();
            }
        }
    }

    [HttpGet("get-data-the-luc-by-ma-luot-kham")]
    public async Task<IActionResult> GetTheLucByMaLuotKham(
            [FromQuery] string? luotKham,
            [FromQuery] string? maLuotKham,
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 10)
    {

        var response = new RequestHttpResponse<List<KhamSucKhoeTheLucModel>>();
        try
        {
            // Validate parameters
            var validLimit = limit <= 0 ? 10 : limit;
            var validOffset = offset < 0 ? 0 : offset;

            var where = luotKham != null ? " WHERE ksk.luot_kham = @luotKham" : " WHERE sksk.ma_luot_kham = @maLuotKham";

            // Query đếm tổng số bản ghi
            var countSql = @"
            SELECT COUNT(*) as TotalCount
            FROM kham_suc_khoe_the_luc ksk
            LEFT JOIN SoKhamSucKhoe sksk ON sksk.id = ksk.luot_kham
            " + where;

            // Query lấy dữ liệu với phân trang
            var dataSql = @"
            SELECT 
                ksk.*,
                plsk.id as phan_loai_id,
                plsk.name as phan_loai_name,
                plsk.code as phan_loai_code
            FROM kham_suc_khoe_the_luc ksk
            LEFT JOIN SoKhamSucKhoe sksk ON sksk.id = ksk.luot_kham            
            LEFT JOIN phan_loai_suc_khoe plsk ON 
                (ksk.phan_loai IS NOT NULL AND plsk.id = ksk.phan_loai)
                OR
                (ksk.phan_loai IS NULL AND plsk.code = ksk.ma_phan_loai)
            " + where + @"
            ORDER BY ksk.id
            OFFSET @offset ROWS 
            FETCH NEXT @limit ROWS ONLY";

            var results = new List<KhamSucKhoeTheLucModel>();
            int totalCount = 0;

            await _context.Database.OpenConnectionAsync();

            // Lấy tổng số bản ghi
            using (var countCommand = _context.Database.GetDbConnection().CreateCommand())
            {
                countCommand.CommandText = countSql;
                if (!string.IsNullOrEmpty(luotKham))
                {
                    countCommand.Parameters.Add(new SqlParameter("@luotKham", luotKham));
                }
                if (!string.IsNullOrEmpty(maLuotKham))
                {
                    countCommand.Parameters.Add(new SqlParameter("@maLuotKham", maLuotKham));
                }
                var countResult = await countCommand.ExecuteScalarAsync();
                totalCount = Convert.ToInt32(countResult ?? 0);
            }

            // Lấy dữ liệu với phân trang
            using (var dataCommand = _context.Database.GetDbConnection().CreateCommand())
            {
                dataCommand.CommandText = dataSql;
                if (!string.IsNullOrEmpty(luotKham))
                {
                    dataCommand.Parameters.Add(new SqlParameter("@luotKham", luotKham));
                }
                if (!string.IsNullOrEmpty(maLuotKham))
                {
                    dataCommand.Parameters.Add(new SqlParameter("@maLuotKham", maLuotKham));
                }
                dataCommand.Parameters.Add(new SqlParameter("@offset", validOffset));
                dataCommand.Parameters.Add(new SqlParameter("@limit", validLimit));

                using (var reader = await dataCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var item = new KhamSucKhoeTheLucModel
                        {
                            id = reader["id"] as int? ?? 0,
                            ma_luot_kham = reader["ma_luot_kham"]?.ToString(),
                            luot_kham = new SoKhamSucKhoeModel()
                            {
                                id = reader["luot_kham"] as int? ?? 0,
                                ma_luot_kham = reader["ma_luot_kham"]?.ToString()
                            },
                            chieu_cao = reader["chieu_cao"] == DBNull.Value ? null : Convert.ToDecimal(reader["chieu_cao"]),
                            can_nang  = reader["can_nang"]  == DBNull.Value ? null : Convert.ToDecimal(reader["can_nang"]),
                            bmi       = reader["bmi"]       == DBNull.Value ? null : Convert.ToDecimal(reader["bmi"]),
                            nhip_tho  = reader["nhip_tho"]  == DBNull.Value ? null : Convert.ToDecimal(reader["nhip_tho"]),
                            abi       = reader["abi"]       == DBNull.Value ? null : Convert.ToDecimal(reader["abi"]),
                            mach = reader["mach"] as int?,
                            huyet_ap = reader["huyet_ap"]?.ToString(),
                            phan_loai = reader["phan_loai_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = reader["phan_loai_id"] as int? ?? 0,
                                name = reader["phan_loai_name"]?.ToString(),
                                code = reader["phan_loai_code"]?.ToString()
                            } : null
                        };

                        results.Add(item);
                    }
                }
            }

            response.Data = results;
            response.Meta = CreateMetaDataWithPagination(totalCount, results.Count, validOffset, validLimit);
            response.StatusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.Errors.Add(CreateErrorResponse(ex));
            response.StatusCode = HttpStatusCode.InternalServerError;
            return BadRequest(response);
        }
        finally
        {
            if (_context.Database.GetDbConnection().State == ConnectionState.Open)
            {
                await _context.Database.CloseConnectionAsync();
            }
        }
    }

    [HttpGet("get-data-ket-luan-by-ma-luot-kham")]
    public async Task<IActionResult> GetKetLuanByMaLuotKham(
            [FromQuery] string? luotKham,
            [FromQuery] string? maLuotKham,
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 10)
    {

        var response = new RequestHttpResponse<List<KhamSucKhoeKetLuanModel>>();
        try
        {
            // Validate parameters
            var validLimit = limit <= 0 ? 10 : limit;
            var validOffset = offset < 0 ? 0 : offset;

            var where = luotKham != null ? " WHERE ksk.luot_kham = @luotKham" : " WHERE sksk.ma_luot_kham = @maLuotKham";

            // Query đếm tổng số bản ghi
            var countSql = @"
            SELECT COUNT(*) as TotalCount
            FROM kham_suc_khoe_ket_luan ksk
            LEFT JOIN SoKhamSucKhoe sksk ON sksk.id = ksk.luot_kham
            " + where;

            // Query lấy dữ liệu với phân trang
            var dataSql = @"
            SELECT 
                ksk.*,
                plsk.id as phan_loai_id,
                plsk.name as phan_loai_name,
                plsk.code as phan_loai_code,
                bs_ket_luan.id as bs_ket_luan_id,
                bs_ket_luan.ma_tai_khoan as bs_ket_luan_ma_tai_khoan,
                bs_ket_luan.first_name as bs_ket_luan_first_name,
                bs_ket_luan.last_name as bs_ket_luan_last_name,
                file_url.id as file_url_id,
                file_url.filename_disk as file_url_filename_disk,
                file_url.filename_download as file_url_filename_download
            FROM kham_suc_khoe_ket_luan ksk
            LEFT JOIN SoKhamSucKhoe sksk ON sksk.id = ksk.luot_kham            
            LEFT JOIN phan_loai_suc_khoe plsk ON 
                (ksk.phan_loai_suc_khoe IS NOT NULL AND plsk.id = ksk.phan_loai_suc_khoe)
                OR
                (ksk.phan_loai_suc_khoe IS NULL AND plsk.code = ksk.ma_phan_loai_suc_khoe)
            LEFT JOIN custom_users bs_ket_luan ON bs_ket_luan.id = ksk.bs_ket_luan
            LEFT JOIN custom_files file_url ON file_url.id = ksk.file_url
            " + where + @"
            ORDER BY ksk.id
            OFFSET @offset ROWS 
            FETCH NEXT @limit ROWS ONLY";

            var results = new List<KhamSucKhoeKetLuanModel>();
            int totalCount = 0;

            await _context.Database.OpenConnectionAsync();

            // Lấy tổng số bản ghi
            using (var countCommand = _context.Database.GetDbConnection().CreateCommand())
            {
                countCommand.CommandText = countSql;
                if (!string.IsNullOrEmpty(luotKham))
                {
                    countCommand.Parameters.Add(new SqlParameter("@luotKham", luotKham));
                }
                if (!string.IsNullOrEmpty(maLuotKham))
                {
                    countCommand.Parameters.Add(new SqlParameter("@maLuotKham", maLuotKham));
                }
                var countResult = await countCommand.ExecuteScalarAsync();
                totalCount = Convert.ToInt32(countResult ?? 0);
            }

            // Lấy dữ liệu với phân trang
            using (var dataCommand = _context.Database.GetDbConnection().CreateCommand())
            {
                dataCommand.CommandText = dataSql;
                if (!string.IsNullOrEmpty(luotKham))
                {
                    dataCommand.Parameters.Add(new SqlParameter("@luotKham", luotKham));
                }
                if (!string.IsNullOrEmpty(maLuotKham))
                {
                    dataCommand.Parameters.Add(new SqlParameter("@maLuotKham", maLuotKham));
                }
                dataCommand.Parameters.Add(new SqlParameter("@offset", validOffset));
                dataCommand.Parameters.Add(new SqlParameter("@limit", validLimit));

                using (var reader = await dataCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var item = new KhamSucKhoeKetLuanModel
                        {
                            id = reader["id"] as int? ?? 0,
                            ma_luot_kham = reader["ma_luot_kham"]?.ToString(),
                            luot_kham = new SoKhamSucKhoeModel()
                            {
                                id = reader["luot_kham"] as int? ?? 0,
                                ma_luot_kham = reader["ma_luot_kham"]?.ToString()
                            },
                            benh_tat_ket_luan = reader["benh_tat_ket_luan"]?.ToString(),
                            nguoi_ket_luan = reader["nguoi_ket_luan"]?.ToString(),
                            ngay_ket_luan = reader["ngay_ket_luan"] as DateTime?,
                            chu_ky = reader["chu_ky"]?.ToString(),
                            de_nghi = reader["de_nghi"]?.ToString(),
                            file_url = reader["file_url_id"] != DBNull.Value ? new FileModel
                            {
                                id = reader["file_url_id"] as Guid? ?? Guid.Empty,
                                filename_disk = reader["file_url_filename_disk"]?.ToString(),
                                filename_download = reader["file_url_filename_download"]?.ToString()
                            } : null,
                            phan_loai_suc_khoe = reader["phan_loai_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = reader["phan_loai_id"] as int? ?? 0,
                                name = reader["phan_loai_name"]?.ToString(),
                                code = reader["phan_loai_code"]?.ToString()
                            } : null,
                            bs_ket_luan = reader["bs_ket_luan_id"] != DBNull.Value ? new UserModel
                            {
                                id = reader["bs_ket_luan_id"] as Guid? ?? Guid.Empty,
                                ma_tai_khoan = reader["bs_ket_luan_ma_tai_khoan"]?.ToString() ?? string.Empty,
                                first_name = reader["bs_ket_luan_first_name"]?.ToString() ?? string.Empty,
                                last_name = reader["bs_ket_luan_last_name"]?.ToString() ?? string.Empty
                            } : null,
                            isAbnormal = reader["isAbnormal"] as bool? ?? false
                        };

                        results.Add(item);
                    }
                }
            }

            response.Data = results;
            response.Meta = CreateMetaDataWithPagination(totalCount, results.Count, validOffset, validLimit);
            response.StatusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.Errors.Add(CreateErrorResponse(ex));
            response.StatusCode = HttpStatusCode.InternalServerError;
            return BadRequest(response);
        }
        finally
        {
            if (_context.Database.GetDbConnection().State == ConnectionState.Open)
            {
                await _context.Database.CloseConnectionAsync();
            }
        }
    }

    [HttpGet("get-data-chuyen-khoa-by-ma-luot-kham")]
    public async Task<IActionResult> GetChuyenKhoaByMaLuotKham([FromQuery] string? luotKham,
                                                    [FromQuery] string? maLuotKham,
                                                    [FromQuery] int offset = 0,
                                                    [FromQuery] int limit = 10)
    {

        var response = new RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>>();
        try
        {
            // Validate parameters
            var validLimit = limit <= 0 ? 10 : limit;
            var validOffset = offset < 0 ? 0 : offset;

            var where = luotKham != null ? " WHERE ksk.luot_kham = @luotKham" : " WHERE sksk.ma_luot_kham = @maLuotKham";

            // Query đếm tổng số bản ghi
            var countSql = @"
            SELECT COUNT(*) as TotalCount
            FROM kham_suc_khoe_kham_chuyen_khoa ksk
            LEFT JOIN SoKhamSucKhoe sksk ON sksk.id = ksk.luot_kham
            " + where;

            // Query lấy dữ liệu với phân trang
            var dataSql = @"
            SELECT 
                ksk.*,
                plsk_nk_tuan_hoan.id as plsk_nk_tuan_hoan_id,
                plsk_nk_tuan_hoan.name as plsk_nk_tuan_hoan_name,
                plsk_nk_tuan_hoan.code as plsk_nk_tuan_hoan_code,
                plsk_nk_ho_hap.id as plsk_nk_ho_hap_id,
                plsk_nk_ho_hap.name as plsk_nk_ho_hap_name,
                plsk_nk_ho_hap.code as plsk_nk_ho_hap_code,
                plsk_nk_tieu_hoa.id as plsk_nk_tieu_hoa_id,
                plsk_nk_tieu_hoa.name as plsk_nk_tieu_hoa_name,
                plsk_nk_tieu_hoa.code as plsk_nk_tieu_hoa_code,
                plsk_nk_than_tiet_nieu.id as plsk_nk_than_tiet_nieu_id,
                plsk_nk_than_tiet_nieu.name as plsk_nk_than_tiet_nieu_name,
                plsk_nk_than_tiet_nieu.code as plsk_nk_than_tiet_nieu_code,
                plsk_nk_noi_tiet.id as plsk_nk_noi_tiet_id,
                plsk_nk_noi_tiet.name as plsk_nk_noi_tiet_name,
                plsk_nk_noi_tiet.code as plsk_nk_noi_tiet_code,
                plsk_nk_co_xuong_khop.id as plsk_nk_co_xuong_khop_id,
                plsk_nk_co_xuong_khop.name as plsk_nk_co_xuong_khop_name,
                plsk_nk_co_xuong_khop.code as plsk_nk_co_xuong_khop_code,
                plsk_nk_than_kinh.id as plsk_nk_than_kinh_id,
                plsk_nk_than_kinh.name as plsk_nk_than_kinh_name,
                plsk_nk_than_kinh.code as plsk_nk_than_kinh_code,
                plsk_nk_tam_than.id as plsk_nk_tam_than_id,
                plsk_nk_tam_than.name as plsk_nk_tam_than_name,
                plsk_nk_tam_than.code as plsk_nk_tam_than_code,
                plsk_ngoai_khoa.id as plsk_ngoai_khoa_id,
                plsk_ngoai_khoa.name as plsk_ngoai_khoa_name,
                plsk_ngoai_khoa.code as plsk_ngoai_khoa_code,
                plsk_da_lieu.id as plsk_da_lieu_id,
                plsk_da_lieu.name as plsk_da_lieu_name,
                plsk_da_lieu.code as plsk_da_lieu_code,
                plsk_mat.id as plsk_mat_id,
                plsk_mat.name as plsk_mat_name,
                plsk_mat.code as plsk_mat_code,
                plsk_tmh.id as plsk_tmh_id,
                plsk_tmh.name as plsk_tmh_name,
                plsk_tmh.code as plsk_tmh_code,
                plsk_rhm.id as plsk_rhm_id,
                plsk_rhm.name as plsk_rhm_name,
                plsk_rhm.code as plsk_rhm_code
            FROM kham_suc_khoe_kham_chuyen_khoa ksk
            LEFT JOIN SoKhamSucKhoe sksk ON sksk.id = ksk.luot_kham            
            LEFT JOIN phan_loai_suc_khoe plsk_nk_tuan_hoan ON 
                (ksk.pl_nk_tuan_hoan IS NOT NULL AND plsk_nk_tuan_hoan.id = ksk.pl_nk_tuan_hoan)
                OR
                (ksk.pl_nk_tuan_hoan IS NULL AND plsk_nk_tuan_hoan.code = ksk.ma_pl_nk_tuan_hoan)
            LEFT JOIN phan_loai_suc_khoe plsk_nk_ho_hap ON 
                (ksk.pl_nk_ho_hap IS NOT NULL AND plsk_nk_ho_hap.id = ksk.pl_nk_ho_hap)
                OR
                (ksk.pl_nk_ho_hap IS NULL AND plsk_nk_ho_hap.code = ksk.ma_pl_nk_ho_hap)
            LEFT JOIN phan_loai_suc_khoe plsk_nk_tieu_hoa ON 
                (ksk.pl_nk_tieu_hoa IS NOT NULL AND plsk_nk_tieu_hoa.id = ksk.pl_nk_tieu_hoa)
                OR
                (ksk.pl_nk_tieu_hoa IS NULL AND plsk_nk_tieu_hoa.code = ksk.ma_pl_nk_tieu_hoa)
            LEFT JOIN phan_loai_suc_khoe plsk_nk_than_tiet_nieu ON 
                (ksk.pl_nk_than_tiet_nieu IS NOT NULL AND plsk_nk_than_tiet_nieu.id = ksk.pl_nk_than_tiet_nieu)
                OR
                (ksk.pl_nk_than_tiet_nieu IS NULL AND plsk_nk_than_tiet_nieu.code = ksk.ma_pl_nk_than_tiet_nieu)
            LEFT JOIN phan_loai_suc_khoe plsk_nk_noi_tiet ON 
                (ksk.pl_nk_noi_tiet IS NOT NULL AND plsk_nk_noi_tiet.id = ksk.pl_nk_noi_tiet)
                OR
                (ksk.pl_nk_noi_tiet IS NULL AND plsk_nk_noi_tiet.code = ksk.ma_pl_nk_noi_tiet)
            LEFT JOIN phan_loai_suc_khoe plsk_nk_co_xuong_khop ON 
                (ksk.pl_nk_co_xuong_khop IS NOT NULL AND plsk_nk_co_xuong_khop.id = ksk.pl_nk_co_xuong_khop)
                OR
                (ksk.pl_nk_co_xuong_khop IS NULL AND plsk_nk_co_xuong_khop.code = ksk.ma_pl_nk_co_xuong_khop)
            LEFT JOIN phan_loai_suc_khoe plsk_nk_than_kinh ON 
                (ksk.pl_nk_than_kinh IS NOT NULL AND plsk_nk_than_kinh.id = ksk.pl_nk_than_kinh)
                OR
                (ksk.pl_nk_than_kinh IS NULL AND plsk_nk_than_kinh.code = ksk.ma_pl_nk_than_kinh)
            LEFT JOIN phan_loai_suc_khoe plsk_nk_tam_than ON 
                (ksk.pl_nk_tam_than IS NOT NULL AND plsk_nk_tam_than.id = ksk.pl_nk_tam_than)
                OR
                (ksk.pl_nk_tam_than IS NULL AND plsk_nk_tam_than.code = ksk.ma_pl_nk_tam_than)
            LEFT JOIN phan_loai_suc_khoe plsk_ngoai_khoa ON 
                (ksk.pl_ngoai_khoa IS NOT NULL AND plsk_ngoai_khoa.id = ksk.pl_ngoai_khoa)
                OR
                (ksk.pl_ngoai_khoa IS NULL AND plsk_ngoai_khoa.code = ksk.ma_pl_ngoai_khoa)
            LEFT JOIN phan_loai_suc_khoe plsk_da_lieu ON 
                (ksk.pl_da_lieu IS NOT NULL AND plsk_da_lieu.id = ksk.pl_da_lieu)
                OR
                (ksk.pl_da_lieu IS NULL AND plsk_da_lieu.code = ksk.ma_pl_da_lieu)
            LEFT JOIN phan_loai_suc_khoe plsk_mat ON 
                (ksk.pl_mat IS NOT NULL AND plsk_mat.id = ksk.pl_mat)
                OR
                (ksk.pl_mat IS NULL AND plsk_mat.code = ksk.ma_pl_mat)
            LEFT JOIN phan_loai_suc_khoe plsk_tmh ON 
                (ksk.pl_tmh IS NOT NULL AND plsk_tmh.id = ksk.pl_tmh)
                OR
                (ksk.pl_tmh IS NULL AND plsk_tmh.code = ksk.ma_pl_tmh)
            LEFT JOIN phan_loai_suc_khoe plsk_rhm ON 
                (ksk.pl_rhm IS NOT NULL AND plsk_rhm.id = ksk.pl_rhm)
                OR
                (ksk.pl_rhm IS NULL AND plsk_rhm.code = ksk.ma_pl_rhm)
            " + where + @"
            ORDER BY ksk.id
            OFFSET @offset ROWS 
            FETCH NEXT @limit ROWS ONLY";

            var results = new List<KhamSucKhoeChuyenKhoaModel>();
            int totalCount = 0;

            await _context.Database.OpenConnectionAsync();

            // Lấy tổng số bản ghi
            using (var countCommand = _context.Database.GetDbConnection().CreateCommand())
            {
                countCommand.CommandText = countSql;
                if (!string.IsNullOrEmpty(luotKham))
                {
                    countCommand.Parameters.Add(new SqlParameter("@luotKham", luotKham));
                }
                if (!string.IsNullOrEmpty(maLuotKham))
                {
                    countCommand.Parameters.Add(new SqlParameter("@maLuotKham", maLuotKham));
                }
                var countResult = await countCommand.ExecuteScalarAsync();
                totalCount = Convert.ToInt32(countResult ?? 0);
            }

            // Lấy dữ liệu với phân trang
            using (var dataCommand = _context.Database.GetDbConnection().CreateCommand())
            {
                dataCommand.CommandText = dataSql;
                if (!string.IsNullOrEmpty(luotKham))
                {
                    dataCommand.Parameters.Add(new SqlParameter("@luotKham", luotKham));
                }
                if (!string.IsNullOrEmpty(maLuotKham))
                {
                    dataCommand.Parameters.Add(new SqlParameter("@maLuotKham", maLuotKham));
                }
                dataCommand.Parameters.Add(new SqlParameter("@offset", validOffset));
                dataCommand.Parameters.Add(new SqlParameter("@limit", validLimit));

                using (var reader = await dataCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var item = new KhamSucKhoeChuyenKhoaModel
                        {
                            id = reader["id"] as int? ?? 0,
                            ma_luot_kham = reader["ma_luot_kham"]?.ToString(),
                            luot_kham = new SoKhamSucKhoeModel()
                            {
                                id = reader["luot_kham"] as int? ?? 0,
                                ma_luot_kham = reader["ma_luot_kham"]?.ToString()
                            },

                            pl_nk_tuan_hoan = reader["plsk_nk_tuan_hoan_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = reader["plsk_nk_tuan_hoan_id"] as int? ?? 0,
                                name = reader["plsk_nk_tuan_hoan_name"]?.ToString(),
                                code = reader["plsk_nk_tuan_hoan_code"]?.ToString()
                            } : null,
                            kq_nk_tuan_hoan = reader["kq_nk_tuan_hoan"]?.ToString(),
                            chu_ky_tuan_hoan = reader["chu_ky_tuan_hoan"]?.ToString(),
                            bs_tuan_hoan = reader["bs_tuan_hoan"]?.ToString(),

                            pl_nk_ho_hap = reader["plsk_nk_ho_hap_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = reader["plsk_nk_ho_hap_id"] as int? ?? 0,
                                name = reader["plsk_nk_ho_hap_name"]?.ToString(),
                                code = reader["plsk_nk_ho_hap_code"]?.ToString()
                            } : null,
                            kq_nk_ho_hap = reader["kq_nk_ho_hap"]?.ToString(),
                            chu_ky_ho_hap = reader["chu_ky_ho_hap"]?.ToString(),
                            bs_ho_hap = reader["bs_ho_hap"]?.ToString(),

                            pl_nk_tieu_hoa = reader["plsk_nk_tieu_hoa_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = reader["plsk_nk_tieu_hoa_id"] as int? ?? 0,
                                name = reader["plsk_nk_tieu_hoa_name"]?.ToString(),
                                code = reader["plsk_nk_tieu_hoa_code"]?.ToString()
                            } : null,
                            kq_nk_tieu_hoa = reader["kq_nk_tieu_hoa"]?.ToString(),
                            chu_ky_tieu_hoa = reader["chu_ky_tieu_hoa"]?.ToString(),
                            bs_tieu_hoa = reader["bs_tieu_hoa"]?.ToString(),

                            pl_nk_than_tiet_nieu = reader["plsk_nk_than_tiet_nieu_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = reader["plsk_nk_than_tiet_nieu_id"] as int? ?? 0,
                                name = reader["plsk_nk_than_tiet_nieu_name"]?.ToString(),
                                code = reader["plsk_nk_than_tiet_nieu_code"]?.ToString()
                            } : null,
                            kq_nk_than_tiet_nieu = reader["kq_nk_than_tiet_nieu"]?.ToString(),
                            chu_ky_than_tiet_nieu = reader["chu_ky_than_tiet_nieu"]?.ToString(),
                            bs_than_tiet_nieu = reader["bs_than_tiet_nieu"]?.ToString(),

                            pl_nk_noi_tiet = reader["plsk_nk_noi_tiet_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = reader["plsk_nk_noi_tiet_id"] as int? ?? 0,
                                name = reader["plsk_nk_noi_tiet_name"]?.ToString(),
                                code = reader["plsk_nk_noi_tiet_code"]?.ToString()
                            } : null,
                            kq_nk_noi_tiet = reader["kq_nk_noi_tiet"]?.ToString(),
                            chu_ky_noi_tiet = reader["chu_ky_noi_tiet"]?.ToString(),
                            bs_noi_tiet = reader["bs_noi_tiet"]?.ToString(),

                            pl_nk_co_xuong_khop = reader["plsk_nk_co_xuong_khop_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = reader["plsk_nk_co_xuong_khop_id"] as int? ?? 0,
                                name = reader["plsk_nk_co_xuong_khop_name"]?.ToString(),
                                code = reader["plsk_nk_co_xuong_khop_code"]?.ToString()
                            } : null,
                            kq_nk_co_xuong_khop = reader["kq_nk_co_xuong_khop"]?.ToString(),
                            chu_ky_co_xuong_khop = reader["chu_ky_co_xuong_khop"]?.ToString(),
                            bs_co_xuong_khop = reader["bs_co_xuong_khop"]?.ToString(),

                            pl_nk_than_kinh = reader["plsk_nk_than_kinh_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = reader["plsk_nk_than_kinh_id"] as int? ?? 0,
                                name = reader["plsk_nk_than_kinh_name"]?.ToString(),
                                code = reader["plsk_nk_than_kinh_code"]?.ToString()
                            } : null,
                            kq_nk_than_kinh = reader["kq_nk_than_kinh"]?.ToString(),
                            chu_ky_than_kinh = reader["chu_ky_than_kinh"]?.ToString(),
                            bs_than_kinh = reader["bs_than_kinh"]?.ToString(),

                            pl_nk_tam_than = reader["plsk_nk_tam_than_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = reader["plsk_nk_tam_than_id"] as int? ?? 0,
                                name = reader["plsk_nk_tam_than_name"]?.ToString(),
                                code = reader["plsk_nk_tam_than_code"]?.ToString()
                            } : null,
                            kq_nk_tam_than = reader["kq_nk_tam_than"]?.ToString(),
                            chu_ky_tam_than = reader["chu_ky_tam_than"]?.ToString(),
                            bs_tam_than = reader["bs_tam_than"]?.ToString(),

                            pl_ngoai_khoa = reader["plsk_ngoai_khoa_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = reader["plsk_ngoai_khoa_id"] as int? ?? 0,
                                name = reader["plsk_ngoai_khoa_name"]?.ToString(),
                                code = reader["plsk_ngoai_khoa_code"]?.ToString()
                            } : null,
                            kq_ngoai_khoa = reader["kq_ngoai_khoa"]?.ToString(),
                            chu_ky_ngoai_khoa = reader["chu_ky_ngoai_khoa"]?.ToString(),
                            bs_ngoai_khoa = reader["bs_ngoai_khoa"]?.ToString(),

                            pl_da_lieu = reader["plsk_da_lieu_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = reader["plsk_da_lieu_id"] as int? ?? 0,
                                name = reader["plsk_da_lieu_name"]?.ToString(),
                                code = reader["plsk_da_lieu_code"]?.ToString()
                            } : null,
                            kq_da_lieu = reader["kq_da_lieu"]?.ToString(),

                            pl_mat = reader["plsk_mat_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = reader["plsk_mat_id"] as int? ?? 0,
                                name = reader["plsk_mat_name"]?.ToString(),
                                code = reader["plsk_mat_code"]?.ToString()
                            } : null,
                            benh_mat = reader["benh_mat"]?.ToString(),
                            chu_ky_mat = reader["chu_ky_mat"]?.ToString(),
                            bs_mat = reader["bs_mat"]?.ToString(),
                            thi_luc_khong_kinh_phai = reader["thi_luc_khong_kinh_phai"]?.ToString(),
                            thi_luc_khong_kinh_trai = reader["thi_luc_khong_kinh_trai"]?.ToString(),
                            thi_luc_co_kinh_phai = reader["thi_luc_co_kinh_phai"]?.ToString(),
                            thi_luc_co_kinh_trai = reader["thi_luc_co_kinh_trai"]?.ToString(),

                            pl_tmh = reader["plsk_tmh_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = reader["plsk_tmh_id"] as int? ?? 0,
                                name = reader["plsk_tmh_name"]?.ToString(),
                                code = reader["plsk_tmh_code"]?.ToString()
                            } : null,
                            benh_tai_mui_hong = reader["benh_tai_mui_hong"]?.ToString(),
                            chu_ky_tmh = reader["chu_ky_tmh"]?.ToString(),
                            bs_tmh = reader["bs_tmh"]?.ToString(),
                            tmh_nt_trai = reader["tmh_nt_trai"]?.ToString(),
                            tmh_ntham_trai = reader["tmh_ntham_trai"]?.ToString(),
                            tmh_nt_phai = reader["tmh_nt_phai"]?.ToString(),
                            tmh_ntham_phai = reader["tmh_ntham_phai"]?.ToString(),

                            pl_rhm = reader["plsk_rhm_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = reader["plsk_rhm_id"] as int? ?? 0,
                                name = reader["plsk_rhm_name"]?.ToString(),
                                code = reader["plsk_rhm_code"]?.ToString()
                            } : null,
                            benh_rhm = reader["benh_rhm"]?.ToString(),
                            chu_ky_rhm = reader["chu_ky_rhm"]?.ToString(),
                            bs_rhm = reader["bs_rhm"]?.ToString(),
                            kq_rhm_ham_tren = reader["kq_rhm_ham_tren"]?.ToString(),
                            kq_rhm_ham_duoi = reader["kq_rhm_ham_duoi"]?.ToString(),
                            
                            bs_ket_luan = reader["bs_ket_luan"]?.ToString(),
                            chu_ky_ket_luan = reader["chu_ky_ket_luan"]?.ToString()
                        };

                        results.Add(item);
                    }
                }
            }

            response.Data = results;
            response.Meta = CreateMetaDataWithPagination(totalCount, results.Count, validOffset, validLimit);
            response.StatusCode = HttpStatusCode.OK;
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.Errors.Add(CreateErrorResponse(ex));
            response.StatusCode = HttpStatusCode.InternalServerError;
            return BadRequest(response);
        }
        finally
        {
            if (_context.Database.GetDbConnection().State == ConnectionState.Open)
            {
                await _context.Database.CloseConnectionAsync();
            }
        }
    }

    private Meta CreateMetaDataWithPagination(int totalCount, int filterCount, int offset, int limit)
    {
        return new Meta
        {
            total_count = totalCount,
            filter_count = filterCount,
            offset = offset,
            limit = limit,
            page_count = (int)Math.Ceiling((double)totalCount / limit),
        };
    }
    private ErrorResponse CreateErrorResponse(Exception ex)
    {
        return new ErrorResponse
        {
            Message = "Internal server error",
            Code = "INTERNAL_ERROR",
            Reason = ex.Message,
            Extensions = new ExtensionsResponse
            {
                code = "INTERNAL_ERROR",
                reason = ex.Message
            }
        };
    }

}