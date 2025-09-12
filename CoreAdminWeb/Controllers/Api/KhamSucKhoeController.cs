
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
    public async Task<IActionResult> GetSanPhuKhoaByMaLuotKham([FromQuery] string? maLuotKham,
                                                    [FromQuery] int offset = 0,
                                                    [FromQuery] int limit = 10)
    {

        var response = new RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>>();
        try
        {
            // Validate parameters
            var validLimit = limit <= 0 ? 10 : limit;
            var validOffset = offset < 0 ? 0 : offset;

            var where = " WHERE ksk.luot_kham = @maLuotKham";

            // Query đếm tổng số bản ghi
            var countSql = @"
            SELECT COUNT(*) as TotalCount
            FROM kham_suc_khoe_san_phu_khoa ksk" + where;

            // Query lấy dữ liệu với phân trang
            var dataSql = @"
            SELECT 
                ksk.*,
                plsk.id as phan_loai_id,
                plsk.name as phan_loai_name,
                plsk.code as phan_loai_code
            FROM kham_suc_khoe_san_phu_khoa ksk
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
    public async Task<IActionResult> GetTheLucByMaLuotKham([FromQuery] string? maLuotKham,
                                                    [FromQuery] int offset = 0,
                                                    [FromQuery] int limit = 10)
    {

        var response = new RequestHttpResponse<List<KhamSucKhoeTheLucModel>>();
        try
        {
            // Validate parameters
            var validLimit = limit <= 0 ? 10 : limit;
            var validOffset = offset < 0 ? 0 : offset;

            var where = " WHERE ksk.luot_kham = @maLuotKham";

            // Query đếm tổng số bản ghi
            var countSql = @"
            SELECT COUNT(*) as TotalCount
            FROM kham_suc_khoe_the_luc ksk" + where;

            // Query lấy dữ liệu với phân trang
            var dataSql = @"
            SELECT 
                ksk.*,
                plsk.id as phan_loai_id,
                plsk.name as phan_loai_name,
                plsk.code as phan_loai_code
            FROM kham_suc_khoe_the_luc ksk
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
                            chieu_cao = Convert.ToDecimal(reader["chieu_cao"]?.ToString() ?? "0"),
                            can_nang = Convert.ToDecimal(reader["can_nang"]?.ToString() ?? "0"),
                            bmi = Convert.ToDecimal(reader["bmi"]?.ToString() ?? "0"),
                            nhip_tho = Convert.ToDecimal(reader["nhip_tho"]?.ToString() ?? "0"),
                            mach = reader["mach"] as int?,
                            huyet_ap = reader["huyet_ap"]?.ToString(),
                            abi = Convert.ToDecimal(reader["abi"]?.ToString() ?? "0"),
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
    public async Task<IActionResult> GetKetLuanByMaLuotKham([FromQuery] string? maLuotKham,
                                                    [FromQuery] int offset = 0,
                                                    [FromQuery] int limit = 10)
    {

        var response = new RequestHttpResponse<List<KhamSucKhoeKetLuanModel>>();
        try
        {
            // Validate parameters
            var validLimit = limit <= 0 ? 10 : limit;
            var validOffset = offset < 0 ? 0 : offset;

            var where = " WHERE ksk.luot_kham = @maLuotKham";

            // Query đếm tổng số bản ghi
            var countSql = @"
            SELECT COUNT(*) as TotalCount
            FROM kham_suc_khoe_ket_luan ksk" + where;

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
            LEFT JOIN phan_loai_suc_khoe plsk ON 
                (ksk.phan_loai IS NOT NULL AND plsk.id = ksk.phan_loai_suc_khoe)
                OR
                (ksk.phan_loai IS NULL AND plsk.code = ksk.ma_phan_loai_suc_khoe)
            LEFT JOIN custom_users bs_ket_luan ON bs_ket_luan.id = ksk.nguoi_ket_luan
            LEFT JOIN files file_url ON file_url.id = ksk.file_url
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
                            file_url = reader["file_url"] != DBNull.Value ? new FileModel
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
                            bs_ket_luan = reader["bs_ket_luan"] != DBNull.Value ? new UserModel
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