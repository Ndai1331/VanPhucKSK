
using CoreAdminWeb.Helpers;
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
                                                               [FromQuery] List<string>? luotKhams,
                                                               [FromQuery] int offset = 0,
                                                               [FromQuery] int limit = 10,
                                                               [FromQuery] string isSign = "false")
    {

        var response = new RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>>();
        try
        {
            if (!bool.TryParse(isSign, out var isSignBool))
            {
                isSignBool = false;
            }
            // Validate parameters
            var validLimit = limit <= 0 ? 10 : limit;
            var validOffset = offset < 0 ? 0 : offset;

            string where = string.Empty;
            if (!string.IsNullOrEmpty(luotKham))
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += " WHERE ";
                }
                else
                {
                    where += " AND ";
                }

                where += "ksk.luot_kham = @luotKham";
            }
            if (!string.IsNullOrEmpty(maLuotKham))
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += " WHERE ";
                }
                else
                {
                    where += " AND ";
                }

                where += "sksk.ma_luot_kham = @maLuotKham";
            }
            if (luotKhams != null && luotKhams.Any())
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += " WHERE ";
                }
                else
                {
                    where += " AND ";
                }

                where += $"ksk.luot_kham IN ({string.Join(",", luotKhams.Select(c => $"'{c}'"))})";
            }

            // Query đếm tổng số bản ghi
            var countSql = @"
            SELECT COUNT(*) as TotalCount
            FROM kham_suc_khoe_san_phu_khoa ksk
            LEFT JOIN SoKhamSucKhoe sksk ON sksk.id = ksk.luot_kham
            " + where;

            // Query lấy dữ liệu với phân trang
            var dataSql = @"
            SELECT 
                ksk.[id]
                ,ksk.[status]
                ,ksk.[sort]
                ,ksk.[user_created]
                ,ksk.[date_created]
                ,ksk.[user_updated]
                ,ksk.[date_updated]
                ,ksk.[ma_luot_kham]
                ,ksk.[tien_su_san_phu_khoa]
                ,ksk.[tuoi_bat_dau_kinh]
                ,ksk.[tinh_chat_kinh]
                ,ksk.[chu_ky_kinh]
                ,ksk.[luong_kinh]
                ,ksk.[dau_bung_kinh]
                ,ksk.[da_lap_gia_dinh]
                ,ksk.[para]
                ,ksk.[so_lan_mo_san_phu_khoa]
                ,ksk.[mo_san_phu_khoa_ghi_ro]
                ,ksk.[ap_dung_bptt]
                ,ksk.[bptt_ghi_ro]
                ,ksk.[ket_qua]
                ,ksk.[nguoi_ket_luan]
" + (isSignBool ? ",bs_ket_luan.chu_ky_bac_si as chu_ky" : "") + @"
                ,ksk.[deleted]
                ,ksk.[luot_kham]
                ,ksk.[phan_loai]
                ,ksk.[ma_phan_loai]
                ,ksk.[ma_nguoi_ket_luan],
                plsk.id as phan_loai_id,
                plsk.name as phan_loai_name,
                plsk.code as phan_loai_code
            FROM kham_suc_khoe_san_phu_khoa ksk
            LEFT JOIN SoKhamSucKhoe sksk ON sksk.id = ksk.luot_kham
            LEFT JOIN phan_loai_suc_khoe plsk ON 
                (ksk.phan_loai IS NOT NULL AND plsk.id = ksk.phan_loai)
                OR
                (ksk.phan_loai IS NULL AND plsk.code = ksk.ma_phan_loai)
            LEFT JOIN custom_users bs_ket_luan ON bs_ket_luan.ma_tai_khoan = ksk.ma_nguoi_ket_luan
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
                            id = DataSetHelper.ReadInt(reader, "id", 0),
                            ma_luot_kham = DataSetHelper.ReadString(reader, "ma_luot_kham"),
                            luot_kham = new SoKhamSucKhoeModel()
                            {
                                id = DataSetHelper.ReadInt(reader, "luot_kham", 0),
                                ma_luot_kham = DataSetHelper.ReadString(reader, "ma_luot_kham")
                            },
                            tien_su_san_phu_khoa = DataSetHelper.ReadString(reader, "tien_su_san_phu_khoa"),
                            tuoi_bat_dau_kinh = DataSetHelper.ReadInt(reader, "tuoi_bat_dau_kinh"),
                            tinh_chat_kinh = DataSetHelper.ReadString(reader, "tinh_chat_kinh"),
                            chu_ky_kinh = DataSetHelper.ReadString(reader, "chu_ky_kinh"),
                            luong_kinh = DataSetHelper.ReadString(reader, "luong_kinh"),
                            dau_bung_kinh = DataSetHelper.ReadBool(reader, "dau_bung_kinh"),
                            da_lap_gia_dinh = DataSetHelper.ReadBool(reader, "da_lap_gia_dinh"),
                            para = DataSetHelper.ReadString(reader, "para"),
                            so_lan_mo_san_phu_khoa = DataSetHelper.ReadInt(reader, "so_lan_mo_san_phu_khoa"),
                            mo_san_phu_khoa_ghi_ro = DataSetHelper.ReadString(reader, "mo_san_phu_khoa_ghi_ro"),
                            ap_dung_bptt = DataSetHelper.ReadBool(reader, "ap_dung_bptt"),
                            bptt_ghi_ro = DataSetHelper.ReadString(reader, "bptt_ghi_ro"),
                            ket_qua = DataSetHelper.ReadString(reader, "ket_qua"),
                            phan_loai = reader["phan_loai_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = DataSetHelper.ReadInt(reader, "phan_loai_id", 0),
                                name = DataSetHelper.ReadString(reader, "phan_loai_name"),
                                code = DataSetHelper.ReadString(reader, "phan_loai_code")
                            } : null,
                            nguoi_ket_luan = DataSetHelper.ReadString(reader, "nguoi_ket_luan"),
                            chu_ky = DataSetHelper.ReadString(reader, "chu_ky")
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
    public async Task<IActionResult> GetTheLucByMaLuotKham([FromQuery] string? luotKham,
                                                           [FromQuery] string? maLuotKham,
                                                           [FromQuery] List<string>? luotKhams,
                                                           [FromQuery] int offset = 0,
                                                           [FromQuery] int limit = 10)
    {

        var response = new RequestHttpResponse<List<KhamSucKhoeTheLucModel>>();
        try
        {
            // Validate parameters
            var validLimit = limit <= 0 ? 10 : limit;
            var validOffset = offset < 0 ? 0 : offset;

            string where = string.Empty;
            if (!string.IsNullOrEmpty(luotKham))
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += " WHERE ";
                }
                else
                {
                    where += " AND ";
                }

                where += "ksk.luot_kham = @luotKham";
            }
            if (!string.IsNullOrEmpty(maLuotKham))
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += " WHERE ";
                }
                else
                {
                    where += " AND ";
                }

                where += "sksk.ma_luot_kham = @maLuotKham";
            }
            if (luotKhams != null && luotKhams.Any())
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += " WHERE ";
                }
                else
                {
                    where += " AND ";
                }

                where += $"ksk.luot_kham IN ({string.Join(",", luotKhams.Select(c => $"'{c}'"))})";
            }

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
                            id = DataSetHelper.ReadInt(reader, "id", 0),
                            ma_luot_kham = DataSetHelper.ReadString(reader, "ma_luot_kham"),
                            luot_kham = new SoKhamSucKhoeModel()
                            {
                                id = DataSetHelper.ReadInt(reader, "luot_kham", 0),
                                ma_luot_kham = DataSetHelper.ReadString(reader, "ma_luot_kham")
                            },
                            chieu_cao = reader["chieu_cao"] == DBNull.Value ? null : Convert.ToDecimal(reader["chieu_cao"]),
                            can_nang = reader["can_nang"] == DBNull.Value ? null : Convert.ToDecimal(reader["can_nang"]),
                            bmi = reader["bmi"] == DBNull.Value ? null : Convert.ToDecimal(reader["bmi"]),
                            nhip_tho = reader["nhip_tho"] == DBNull.Value ? null : Convert.ToDecimal(reader["nhip_tho"]),
                            abi = reader["abi"] == DBNull.Value ? null : Convert.ToDecimal(reader["abi"]),
                            mach = DataSetHelper.ReadInt(reader, "mach"),
                            huyet_ap = DataSetHelper.ReadString(reader, "huyet_ap"),
                            phan_loai = reader["phan_loai_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = DataSetHelper.ReadInt(reader, "phan_loai_id", 0),
                                name = DataSetHelper.ReadString(reader, "phan_loai_name"),
                                code = DataSetHelper.ReadString(reader, "phan_loai_code")
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
    public async Task<IActionResult> GetKetLuanByMaLuotKham([FromQuery] string? luotKham,
                                                            [FromQuery] string? maLuotKham,
                                                            [FromQuery] List<string>? luotKhams,
                                                            [FromQuery] int offset = 0,
                                                            [FromQuery] int limit = 10,
                                                            [FromQuery] string isSign = "false")
    {

        var response = new RequestHttpResponse<List<KhamSucKhoeKetLuanModel>>();
        try
        {
            if (!bool.TryParse(isSign, out var isSignBool))
            {
                isSignBool = false;
            }

            // Validate parameters
            var validLimit = limit <= 0 ? 10 : limit;
            var validOffset = offset < 0 ? 0 : offset;

            string where = string.Empty;
            if (!string.IsNullOrEmpty(luotKham))
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += " WHERE ";
                }
                else
                {
                    where += " AND ";
                }

                where += "ksk.luot_kham = @luotKham";
            }
            if (!string.IsNullOrEmpty(maLuotKham))
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += " WHERE ";
                }
                else
                {
                    where += " AND ";
                }

                where += "sksk.ma_luot_kham = @maLuotKham";
            }
            if (luotKhams != null && luotKhams.Any())
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += " WHERE ";
                }
                else
                {
                    where += " AND ";
                }

                where += $"ksk.luot_kham IN ({string.Join(",", luotKhams.Select(c => $"'{c}'"))})";
            }

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
                bs_ket_luan.chuc_danh as bs_ket_luan_chuc_danh,
" + (isSignBool ? "bs_ket_luan.chu_ky_bac_si as bs_ket_luan_chu_ky_bac_si, " : "") + @"
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
                            id = DataSetHelper.ReadInt(reader, "id", 0),
                            ma_luot_kham = DataSetHelper.ReadString(reader, "ma_luot_kham"),
                            luot_kham = new SoKhamSucKhoeModel()
                            {
                                id = DataSetHelper.ReadInt(reader, "luot_kham", 0),
                                ma_luot_kham = DataSetHelper.ReadString(reader, "ma_luot_kham")
                            },
                            benh_tat_ket_luan = DataSetHelper.ReadString(reader, "benh_tat_ket_luan"),
                            nguoi_ket_luan = DataSetHelper.ReadString(reader, "nguoi_ket_luan"),
                            ngay_ket_luan = reader["ngay_ket_luan"] as DateTime?,
                            chu_ky = DataSetHelper.ReadString(reader, "chu_ky"),
                            de_nghi = DataSetHelper.ReadString(reader, "de_nghi"),
                            file_url = reader["file_url_id"] != DBNull.Value ? new FileModel
                            {
                                id = reader["file_url_id"] as Guid? ?? Guid.Empty,
                                filename_disk = DataSetHelper.ReadString(reader, "file_url_filename_disk"),
                                filename_download = DataSetHelper.ReadString(reader, "file_url_filename_download")
                            } : null,
                            phan_loai_suc_khoe = reader["phan_loai_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = DataSetHelper.ReadInt(reader, "phan_loai_id", 0),
                                name = DataSetHelper.ReadString(reader, "phan_loai_name"),
                                code = DataSetHelper.ReadString(reader, "phan_loai_code")
                            } : null,
                            bs_ket_luan = reader["bs_ket_luan_id"] != DBNull.Value ? new UserModel
                            {
                                id = reader["bs_ket_luan_id"] as Guid? ?? Guid.Empty,
                                ma_tai_khoan = DataSetHelper.ReadString(reader, "bs_ket_luan_ma_tai_khoan"),
                                first_name = DataSetHelper.ReadString(reader, "bs_ket_luan_first_name"),
                                last_name = DataSetHelper.ReadString(reader, "bs_ket_luan_last_name"),
                                chu_ky_bac_si = DataSetHelper.ReadString(reader, "bs_ket_luan_chu_ky_bac_si"),
                                chuc_danh = DataSetHelper.ReadString(reader, "bs_ket_luan_chuc_danh")
                            } : null,
                            isAbnormal = DataSetHelper.ReadBool(reader, "isAbnormal", false)
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
                                                               [FromQuery] List<string>? luotKhams,
                                                               [FromQuery] int offset = 0,
                                                               [FromQuery] int limit = 10,
                                                               [FromQuery] string isSign = "false")
    {
        var response = new RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>>();
        try
        {
            // Validate parameters
            var validLimit = limit <= 0 ? 10 : limit;
            var validOffset = offset < 0 ? 0 : offset;

            string where = string.Empty;
            if (!string.IsNullOrEmpty(luotKham))
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += " WHERE ";
                }
                else
                {
                    where += " AND ";
                }

                where += "ksk.luot_kham = @luotKham";
            }
            if (!string.IsNullOrEmpty(maLuotKham))
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += " WHERE ";
                }
                else
                {
                    where += " AND ";
                }

                where += "sksk.ma_luot_kham = @maLuotKham";
            }
            if (luotKhams != null && luotKhams.Any())
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += " WHERE ";
                }
                else
                {
                    where += " AND ";
                }

                where += $"ksk.luot_kham IN ({string.Join(",", luotKhams.Select(c => $"'{c}'"))})";
            }

            if (!bool.TryParse(isSign, out var isSignBool))
            {
                isSignBool = false;
            }
            // Query đếm tổng số bản ghi
            var countSql = @"
            SELECT COUNT(*) as TotalCount
            FROM kham_suc_khoe_kham_chuyen_khoa ksk
            LEFT JOIN SoKhamSucKhoe sksk ON sksk.id = ksk.luot_kham
            " + where;

            var selectedField = string.Empty;
            var joinClause = string.Empty;
            if (isSignBool)
            {
                selectedField = @"
    bs_tuan_hoan_user.chu_ky_bac_si   AS chu_ky_tuan_hoan
    , bs_ho_hap_user.chu_ky_bac_si      AS chu_ky_ho_hap
    , bs_tieu_hoa_user.chu_ky_bac_si    AS chu_ky_tieu_hoa
    , bs_than_tiet_nieu_user.chu_ky_bac_si AS chu_ky_than_tiet_nieu
    , bs_noi_tiet_user.chu_ky_bac_si    AS chu_ky_noi_tiet
    , bs_co_xuong_khop_user.chu_ky_bac_si AS chu_ky_co_xuong_khop
    , bs_than_kinh_user.chu_ky_bac_si   AS chu_ky_than_kinh
    , bs_tam_than_user.chu_ky_bac_si    AS chu_ky_tam_than
    , bs_ngoai_khoa_user.chu_ky_bac_si  AS chu_ky_ngoai_khoa
    , bs_mat_user.chu_ky_bac_si         AS chu_ky_mat
    , bs_tmh_user.chu_ky_bac_si         AS chu_ky_tmh
    , bs_rhm_user.chu_ky_bac_si         AS chu_ky_rhm
    , bs_ket_luan_user.chu_ky_bac_si    AS chu_ky_ket_luan,
";
                joinClause = @"
OUTER APPLY (SELECT TOP 1 cu.chu_ky_bac_si FROM custom_users cu WHERE cu.ma_tai_khoan = ksk.ma_bs_tuan_hoan) bs_tuan_hoan_user
OUTER APPLY (SELECT TOP 1 cu.chu_ky_bac_si FROM custom_users cu WHERE cu.ma_tai_khoan = ksk.ma_bs_ho_hap) bs_ho_hap_user
OUTER APPLY (SELECT TOP 1 cu.chu_ky_bac_si FROM custom_users cu WHERE cu.ma_tai_khoan = ksk.ma_bs_tieu_hoa) bs_tieu_hoa_user
OUTER APPLY (SELECT TOP 1 cu.chu_ky_bac_si FROM custom_users cu WHERE cu.ma_tai_khoan = ksk.ma_bs_than_tiet_nieu) bs_than_tiet_nieu_user
OUTER APPLY (SELECT TOP 1 cu.chu_ky_bac_si FROM custom_users cu WHERE cu.ma_tai_khoan = ksk.ma_bs_noi_tiet) bs_noi_tiet_user
OUTER APPLY (SELECT TOP 1 cu.chu_ky_bac_si FROM custom_users cu WHERE cu.ma_tai_khoan = ksk.ma_bs_co_xuong_khop) bs_co_xuong_khop_user
OUTER APPLY (SELECT TOP 1 cu.chu_ky_bac_si FROM custom_users cu WHERE cu.ma_tai_khoan = ksk.ma_bs_than_kinh) bs_than_kinh_user
OUTER APPLY (SELECT TOP 1 cu.chu_ky_bac_si FROM custom_users cu WHERE cu.ma_tai_khoan = ksk.ma_bs_tam_than) bs_tam_than_user
OUTER APPLY (SELECT TOP 1 cu.chu_ky_bac_si FROM custom_users cu WHERE cu.ma_tai_khoan = ksk.ma_bs_ngoai_khoa) bs_ngoai_khoa_user
OUTER APPLY (SELECT TOP 1 cu.chu_ky_bac_si FROM custom_users cu WHERE cu.ma_tai_khoan = ksk.ma_bs_mat) bs_mat_user
OUTER APPLY (SELECT TOP 1 cu.chu_ky_bac_si FROM custom_users cu WHERE cu.ma_tai_khoan = ksk.ma_bs_tmh) bs_tmh_user
OUTER APPLY (SELECT TOP 1 cu.chu_ky_bac_si FROM custom_users cu WHERE cu.ma_tai_khoan = ksk.ma_bs_rhm) bs_rhm_user
OUTER APPLY (SELECT TOP 1 cu.chu_ky_bac_si FROM custom_users cu WHERE cu.ma_tai_khoan = ksk.ma_bs_ket_luan) bs_ket_luan_user
";
            }
            selectedField += @"
ksk.[id]
  , ksk.[status]
  , ksk.[sort]
  , ksk.[user_created]
  , ksk.[date_created]
  , ksk.[user_updated]
  , ksk.[date_updated]
  , ksk.[ma_luot_kham]

  , ksk.[kq_nk_tuan_hoan]
  , ksk.[kq_nk_ho_hap]
  , ksk.[kq_nk_tieu_hoa]
  , ksk.[bs_tuan_hoan]
  , ksk.[bs_ho_hap]
  , ksk.[bs_tieu_hoa]
  , ksk.[kq_nk_than_tiet_nieu]
  , ksk.[bs_than_tiet_nieu]
  , ksk.[kq_nk_noi_tiet]
  , ksk.[bs_noi_tiet]
  , ksk.[kq_nk_co_xuong_khop]
  , ksk.[bs_co_xuong_khop]
  , ksk.[kq_nk_than_kinh]
  , ksk.[bs_than_kinh]
  , ksk.[kq_nk_tam_than]
  , ksk.[bs_tam_than]
  , ksk.[kq_ngoai_khoa]
  , ksk.[kq_da_lieu]
  , ksk.[bs_ngoai_khoa]
  , ksk.[thi_luc_khong_kinh_phai]
  , ksk.[thi_luc_khong_kinh_trai]
  , ksk.[thi_luc_co_kinh_phai]
  , ksk.[thi_luc_co_kinh_trai]
  , ksk.[benh_mat]
  , ksk.[bs_mat]
  , ksk.[tmh_nt_trai]
  , ksk.[tmh_ntham_trai]
  , ksk.[tmh_ntham_phai]
  , ksk.[tmh_nt_phai]
  , ksk.[benh_tai_mui_hong]
  , ksk.[bs_tmh]
  , ksk.[kq_rhm_ham_tren]
  , ksk.[kq_rhm_ham_duoi]
  , ksk.[benh_rhm]
  , ksk.[bs_rhm]
  , ksk.[deleted]
  , ksk.[bs_ket_luan]
  , ksk.[luot_kham]

  , pl_nk_tuan_hoan_res.id   AS plsk_nk_tuan_hoan_id
  , pl_nk_tuan_hoan_res.[name] AS plsk_nk_tuan_hoan_name
  , pl_nk_tuan_hoan_res.[code] AS plsk_nk_tuan_hoan_code

  , pl_nk_ho_hap_res.id      AS plsk_nk_ho_hap_id
  , pl_nk_ho_hap_res.[name]  AS plsk_nk_ho_hap_name
  , pl_nk_ho_hap_res.[code]  AS plsk_nk_ho_hap_code

  , pl_nk_tieu_hoa_res.id    AS plsk_nk_tieu_hoa_id
  , pl_nk_tieu_hoa_res.[name] AS plsk_nk_tieu_hoa_name
  , pl_nk_tieu_hoa_res.[code] AS plsk_nk_tieu_hoa_code

  , pl_nk_than_tiet_nieu_res.id    AS plsk_nk_than_tiet_nieu_id
  , pl_nk_than_tiet_nieu_res.[name] AS plsk_nk_than_tiet_nieu_name
  , pl_nk_than_tiet_nieu_res.[code] AS plsk_nk_than_tiet_nieu_code

  , pl_nk_noi_tiet_res.id     AS plsk_nk_noi_tiet_id
  , pl_nk_noi_tiet_res.[name] AS plsk_nk_noi_tiet_name
  , pl_nk_noi_tiet_res.[code] AS plsk_nk_noi_tiet_code

  , pl_nk_co_xuong_khop_res.id    AS plsk_nk_co_xuong_khop_id
  , pl_nk_co_xuong_khop_res.[name] AS plsk_nk_co_xuong_khop_name
  , pl_nk_co_xuong_khop_res.[code] AS plsk_nk_co_xuong_khop_code

  , pl_nk_than_kinh_res.id    AS plsk_nk_than_kinh_id
  , pl_nk_than_kinh_res.[name] AS plsk_nk_than_kinh_name
  , pl_nk_than_kinh_res.[code] AS plsk_nk_than_kinh_code

  , pl_nk_tam_than_res.id     AS plsk_nk_tam_than_id
  , pl_nk_tam_than_res.[name] AS plsk_nk_tam_than_name
  , pl_nk_tam_than_res.[code] AS plsk_nk_tam_than_code

  , pl_ngoai_khoa_res.id      AS plsk_ngoai_khoa_id
  , pl_ngoai_khoa_res.[name]  AS plsk_ngoai_khoa_name
  , pl_ngoai_khoa_res.[code]  AS plsk_ngoai_khoa_code

  , pl_da_lieu_res.id         AS plsk_da_lieu_id
  , pl_da_lieu_res.[name]     AS plsk_da_lieu_name
  , pl_da_lieu_res.[code]     AS plsk_da_lieu_code

  , pl_mat_res.id             AS plsk_mat_id
  , pl_mat_res.[name]         AS plsk_mat_name
  , pl_mat_res.[code]         AS plsk_mat_code

  , pl_tmh_res.id             AS plsk_tmh_id
  , pl_tmh_res.[name]         AS plsk_tmh_name
  , pl_tmh_res.[code]         AS plsk_tmh_code

  , pl_rhm_res.id             AS plsk_rhm_id
  , pl_rhm_res.[name]         AS plsk_rhm_name
  , pl_rhm_res.[code]         AS plsk_rhm_code
";
            joinClause += @"
OUTER APPLY (SELECT TOP 1 pls.* FROM phan_loai_suc_khoe pls
             WHERE (ksk.pl_nk_tuan_hoan IS NOT NULL AND pls.id = ksk.pl_nk_tuan_hoan)
                OR (ksk.pl_nk_tuan_hoan IS NULL AND pls.code = ksk.ma_pl_nk_tuan_hoan)) pl_nk_tuan_hoan_res
OUTER APPLY (SELECT TOP 1 pls.* FROM phan_loai_suc_khoe pls
             WHERE (ksk.pl_nk_ho_hap IS NOT NULL AND pls.id = ksk.pl_nk_ho_hap)
                OR (ksk.pl_nk_ho_hap IS NULL AND pls.code = ksk.ma_pl_nk_ho_hap)) pl_nk_ho_hap_res
OUTER APPLY (SELECT TOP 1 pls.* FROM phan_loai_suc_khoe pls
             WHERE (ksk.pl_nk_tieu_hoa IS NOT NULL AND pls.id = ksk.pl_nk_tieu_hoa)
                OR (ksk.pl_nk_tieu_hoa IS NULL AND pls.code = ksk.ma_pl_nk_tieu_hoa)) pl_nk_tieu_hoa_res
OUTER APPLY (SELECT TOP 1 pls.* FROM phan_loai_suc_khoe pls
             WHERE (ksk.pl_nk_than_tiet_nieu IS NOT NULL AND pls.id = ksk.pl_nk_than_tiet_nieu)
                OR (ksk.pl_nk_than_tiet_nieu IS NULL AND pls.code = ksk.ma_pl_nk_than_tiet_nieu)) pl_nk_than_tiet_nieu_res
OUTER APPLY (SELECT TOP 1 pls.* FROM phan_loai_suc_khoe pls
             WHERE (ksk.pl_nk_noi_tiet IS NOT NULL AND pls.id = ksk.pl_nk_noi_tiet)
                OR (ksk.pl_nk_noi_tiet IS NULL AND pls.code = ksk.ma_pl_nk_noi_tiet)) pl_nk_noi_tiet_res
OUTER APPLY (SELECT TOP 1 pls.* FROM phan_loai_suc_khoe pls
             WHERE (ksk.pl_nk_co_xuong_khop IS NOT NULL AND pls.id = ksk.pl_nk_co_xuong_khop)
                OR (ksk.pl_nk_co_xuong_khop IS NULL AND pls.code = ksk.ma_pl_nk_co_xuong_khop)) pl_nk_co_xuong_khop_res
OUTER APPLY (SELECT TOP 1 pls.* FROM phan_loai_suc_khoe pls
             WHERE (ksk.pl_nk_than_kinh IS NOT NULL AND pls.id = ksk.pl_nk_than_kinh)
                OR (ksk.pl_nk_than_kinh IS NULL AND pls.code = ksk.ma_pl_nk_than_kinh)) pl_nk_than_kinh_res
OUTER APPLY (SELECT TOP 1 pls.* FROM phan_loai_suc_khoe pls
             WHERE (ksk.pl_nk_tam_than IS NOT NULL AND pls.id = ksk.pl_nk_tam_than)
                OR (ksk.pl_nk_tam_than IS NULL AND pls.code = ksk.ma_pl_nk_tam_than)) pl_nk_tam_than_res
OUTER APPLY (SELECT TOP 1 pls.* FROM phan_loai_suc_khoe pls
             WHERE (ksk.pl_ngoai_khoa IS NOT NULL AND pls.id = ksk.pl_ngoai_khoa)
                OR (ksk.pl_ngoai_khoa IS NULL AND pls.code = ksk.ma_pl_ngoai_khoa)) pl_ngoai_khoa_res
OUTER APPLY (SELECT TOP 1 pls.* FROM phan_loai_suc_khoe pls
             WHERE (ksk.pl_da_lieu IS NOT NULL AND pls.id = ksk.pl_da_lieu)
                OR (ksk.pl_da_lieu IS NULL AND pls.code = ksk.ma_pl_da_lieu)) pl_da_lieu_res
OUTER APPLY (SELECT TOP 1 pls.* FROM phan_loai_suc_khoe pls
             WHERE (ksk.pl_mat IS NOT NULL AND pls.id = ksk.pl_mat)
                OR (ksk.pl_mat IS NULL AND pls.code = ksk.ma_pl_mat)) pl_mat_res
OUTER APPLY (SELECT TOP 1 pls.* FROM phan_loai_suc_khoe pls
             WHERE (ksk.pl_tmh IS NOT NULL AND pls.id = ksk.pl_tmh)
                OR (ksk.pl_tmh IS NULL AND pls.code = ksk.ma_pl_tmh)) pl_tmh_res
OUTER APPLY (SELECT TOP 1 pls.* FROM phan_loai_suc_khoe pls
             WHERE (ksk.pl_rhm IS NOT NULL AND pls.id = ksk.pl_rhm)
                OR (ksk.pl_rhm IS NULL AND pls.code = ksk.ma_pl_rhm)) pl_rhm_res
";

            // Query lấy dữ liệu với phân trang
            var dataSql = "SELECT " + selectedField + @" 
 FROM kham_suc_khoe_kham_chuyen_khoa AS ksk
LEFT JOIN SoKhamSucKhoe AS sksk ON sksk.id = ksk.luot_kham
            " + joinClause + where + @"
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
                            id = DataSetHelper.ReadInt(reader, "id", 0),
                            ma_luot_kham = DataSetHelper.ReadString(reader, "ma_luot_kham"),
                            luot_kham = new SoKhamSucKhoeModel()
                            {
                                id = DataSetHelper.ReadInt(reader, "luot_kham", 0),
                                ma_luot_kham = DataSetHelper.ReadString(reader, "ma_luot_kham")
                            },

                            pl_nk_tuan_hoan = reader["plsk_nk_tuan_hoan_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = DataSetHelper.ReadInt(reader, "plsk_nk_tuan_hoan_id", 0),
                                name = DataSetHelper.ReadString(reader, "plsk_nk_tuan_hoan_name"),
                                code = DataSetHelper.ReadString(reader, "plsk_nk_tuan_hoan_code")
                            } : null,
                            kq_nk_tuan_hoan = DataSetHelper.ReadString(reader, "kq_nk_tuan_hoan"),
                            chu_ky_tuan_hoan = DataSetHelper.ReadString(reader, "chu_ky_tuan_hoan"),
                            bs_tuan_hoan = DataSetHelper.ReadString(reader, "bs_tuan_hoan"),

                            pl_nk_ho_hap = reader["plsk_nk_ho_hap_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = DataSetHelper.ReadInt(reader, "plsk_nk_ho_hap_id", 0),
                                name = DataSetHelper.ReadString(reader, "plsk_nk_ho_hap_name"),
                                code = DataSetHelper.ReadString(reader, "plsk_nk_ho_hap_code")
                            } : null,
                            kq_nk_ho_hap = DataSetHelper.ReadString(reader, "kq_nk_ho_hap"),
                            chu_ky_ho_hap = DataSetHelper.ReadString(reader, "chu_ky_ho_hap"),
                            bs_ho_hap = DataSetHelper.ReadString(reader, "bs_ho_hap"),

                            pl_nk_tieu_hoa = reader["plsk_nk_tieu_hoa_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = DataSetHelper.ReadInt(reader, "plsk_nk_tieu_hoa_id", 0),
                                name = DataSetHelper.ReadString(reader, "plsk_nk_tieu_hoa_name"),
                                code = DataSetHelper.ReadString(reader, "plsk_nk_tieu_hoa_code")
                            } : null,
                            kq_nk_tieu_hoa = DataSetHelper.ReadString(reader, "kq_nk_tieu_hoa"),
                            chu_ky_tieu_hoa = DataSetHelper.ReadString(reader, "chu_ky_tieu_hoa"),
                            bs_tieu_hoa = DataSetHelper.ReadString(reader, "bs_tieu_hoa"),

                            pl_nk_than_tiet_nieu = reader["plsk_nk_than_tiet_nieu_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = DataSetHelper.ReadInt(reader, "plsk_nk_than_tiet_nieu_id", 0),
                                name = DataSetHelper.ReadString(reader, "plsk_nk_than_tiet_nieu_name"),
                                code = DataSetHelper.ReadString(reader, "plsk_nk_than_tiet_nieu_code")
                            } : null,
                            kq_nk_than_tiet_nieu = DataSetHelper.ReadString(reader, "kq_nk_than_tiet_nieu"),
                            chu_ky_than_tiet_nieu = DataSetHelper.ReadString(reader, "chu_ky_than_tiet_nieu"),
                            bs_than_tiet_nieu = DataSetHelper.ReadString(reader, "bs_than_tiet_nieu"),

                            pl_nk_noi_tiet = reader["plsk_nk_noi_tiet_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = DataSetHelper.ReadInt(reader, "plsk_nk_noi_tiet_id", 0),
                                name = DataSetHelper.ReadString(reader, "plsk_nk_noi_tiet_name"),
                                code = DataSetHelper.ReadString(reader, "plsk_nk_noi_tiet_code")
                            } : null,
                            kq_nk_noi_tiet = DataSetHelper.ReadString(reader, "kq_nk_noi_tiet"),
                            chu_ky_noi_tiet = DataSetHelper.ReadString(reader, "chu_ky_noi_tiet"),
                            bs_noi_tiet = DataSetHelper.ReadString(reader, "bs_noi_tiet"),

                            pl_nk_co_xuong_khop = reader["plsk_nk_co_xuong_khop_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = DataSetHelper.ReadInt(reader, "plsk_nk_co_xuong_khop_id", 0),
                                name = DataSetHelper.ReadString(reader, "plsk_nk_co_xuong_khop_name"),
                                code = DataSetHelper.ReadString(reader, "plsk_nk_co_xuong_khop_code")
                            } : null,
                            kq_nk_co_xuong_khop = DataSetHelper.ReadString(reader, "kq_nk_co_xuong_khop"),
                            chu_ky_co_xuong_khop = DataSetHelper.ReadString(reader, "chu_ky_co_xuong_khop"),
                            bs_co_xuong_khop = DataSetHelper.ReadString(reader, "bs_co_xuong_khop"),

                            pl_nk_than_kinh = reader["plsk_nk_than_kinh_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = DataSetHelper.ReadInt(reader, "plsk_nk_than_kinh_id", 0),
                                name = DataSetHelper.ReadString(reader, "plsk_nk_than_kinh_name"),
                                code = DataSetHelper.ReadString(reader, "plsk_nk_than_kinh_code")
                            } : null,
                            kq_nk_than_kinh = DataSetHelper.ReadString(reader, "kq_nk_than_kinh"),
                            chu_ky_than_kinh = DataSetHelper.ReadString(reader, "chu_ky_than_kinh"),
                            bs_than_kinh = DataSetHelper.ReadString(reader, "bs_than_kinh"),

                            pl_nk_tam_than = reader["plsk_nk_tam_than_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = DataSetHelper.ReadInt(reader, "plsk_nk_tam_than_id", 0),
                                name = DataSetHelper.ReadString(reader, "plsk_nk_tam_than_name"),
                                code = DataSetHelper.ReadString(reader, "plsk_nk_tam_than_code")
                            } : null,
                            kq_nk_tam_than = DataSetHelper.ReadString(reader, "kq_nk_tam_than"),
                            chu_ky_tam_than = DataSetHelper.ReadString(reader, "chu_ky_tam_than"),
                            bs_tam_than = DataSetHelper.ReadString(reader, "bs_tam_than"),

                            pl_ngoai_khoa = reader["plsk_ngoai_khoa_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = DataSetHelper.ReadInt(reader, "plsk_ngoai_khoa_id", 0),
                                name = DataSetHelper.ReadString(reader, "plsk_ngoai_khoa_name"),
                                code = DataSetHelper.ReadString(reader, "plsk_ngoai_khoa_code")
                            } : null,
                            kq_ngoai_khoa = DataSetHelper.ReadString(reader, "kq_ngoai_khoa"),
                            chu_ky_ngoai_khoa = DataSetHelper.ReadString(reader, "chu_ky_ngoai_khoa"),
                            bs_ngoai_khoa = DataSetHelper.ReadString(reader, "bs_ngoai_khoa"),

                            pl_da_lieu = reader["plsk_da_lieu_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = DataSetHelper.ReadInt(reader, "plsk_da_lieu_id", 0),
                                name = DataSetHelper.ReadString(reader, "plsk_da_lieu_name"),
                                code = DataSetHelper.ReadString(reader, "plsk_da_lieu_code")
                            } : null,
                            kq_da_lieu = DataSetHelper.ReadString(reader, "kq_da_lieu"),

                            pl_mat = reader["plsk_mat_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = DataSetHelper.ReadInt(reader, "plsk_mat_id", 0),
                                name = DataSetHelper.ReadString(reader, "plsk_mat_name"),
                                code = DataSetHelper.ReadString(reader, "plsk_mat_code")
                            } : null,
                            benh_mat = DataSetHelper.ReadString(reader, "benh_mat"),
                            chu_ky_mat = DataSetHelper.ReadString(reader, "chu_ky_mat"),
                            bs_mat = DataSetHelper.ReadString(reader, "bs_mat"),
                            thi_luc_khong_kinh_phai = DataSetHelper.ReadString(reader, "thi_luc_khong_kinh_phai"),
                            thi_luc_khong_kinh_trai = DataSetHelper.ReadString(reader, "thi_luc_khong_kinh_trai"),
                            thi_luc_co_kinh_phai = DataSetHelper.ReadString(reader, "thi_luc_co_kinh_phai"),
                            thi_luc_co_kinh_trai = DataSetHelper.ReadString(reader, "thi_luc_co_kinh_trai"),

                            pl_tmh = reader["plsk_tmh_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = DataSetHelper.ReadInt(reader, "plsk_tmh_id", 0),
                                name = DataSetHelper.ReadString(reader, "plsk_tmh_name"),
                                code = DataSetHelper.ReadString(reader, "plsk_tmh_code")
                            } : null,
                            benh_tai_mui_hong = DataSetHelper.ReadString(reader, "benh_tai_mui_hong"),
                            chu_ky_tmh = DataSetHelper.ReadString(reader, "chu_ky_tmh"),
                            bs_tmh = DataSetHelper.ReadString(reader, "bs_tmh"),
                            tmh_nt_trai = DataSetHelper.ReadString(reader, "tmh_nt_trai"),
                            tmh_ntham_trai = DataSetHelper.ReadString(reader, "tmh_ntham_trai"),
                            tmh_nt_phai = DataSetHelper.ReadString(reader, "tmh_nt_phai"),
                            tmh_ntham_phai = DataSetHelper.ReadString(reader, "tmh_ntham_phai"),

                            pl_rhm = reader["plsk_rhm_id"] != DBNull.Value ? new PhanLoaiSucKhoeModel
                            {
                                id = DataSetHelper.ReadInt(reader, "plsk_rhm_id", 0),
                                name = DataSetHelper.ReadString(reader, "plsk_rhm_name"),
                                code = DataSetHelper.ReadString(reader, "plsk_rhm_code")
                            } : null,
                            benh_rhm = DataSetHelper.ReadString(reader, "benh_rhm"),
                            chu_ky_rhm = DataSetHelper.ReadString(reader, "chu_ky_rhm"),
                            bs_rhm = DataSetHelper.ReadString(reader, "bs_rhm"),
                            kq_rhm_ham_tren = DataSetHelper.ReadString(reader, "kq_rhm_ham_tren"),
                            kq_rhm_ham_duoi = DataSetHelper.ReadString(reader, "kq_rhm_ham_duoi"),

                            bs_ket_luan = DataSetHelper.ReadString(reader, "bs_ket_luan"),
                            chu_ky_ket_luan = DataSetHelper.ReadString(reader, "chu_ky_ket_luan")
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