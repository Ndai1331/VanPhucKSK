
using CoreAdminWeb.Enums;
using CoreAdminWeb.Extensions;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.RequestHttps;
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
public class DanhSachDoanController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DanhSachDoanController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("medical-data")]
    public async Task<IActionResult> GetMedicalData([FromQuery] string? maDotKham,
                                                    [FromQuery] string? congTy,
                                                    [FromQuery] DateTime? fromDate,
                                                    [FromQuery] DateTime? toDate,
                                                    [FromQuery] string? maDieuTri,
                                                    [FromQuery] int? fromNumber,
                                                    [FromQuery] int? toNumber,
                                                    [FromQuery] int offset = 0,
                                                    [FromQuery] int limit = 10)
    {

        var response = new RequestHttpResponse<List<MedicalExaminationDto>>();
        try
        {
            // Validate parameters
            var validLimit = limit <= 0 ? 10 : limit;
            var validOffset = offset < 0 ? 0 : offset;

            var where = " WHERE (ct.deleted = 0 OR ct.deleted IS NULL) AND ct.id IS NOT NULL AND (sksk.deleted = 0 OR sksk.deleted IS NULL)";
            if (!string.IsNullOrEmpty(maDotKham))
            {
                where += " AND sksk.MaDotKham = " + maDotKham;
            }
            if (!string.IsNullOrEmpty(congTy))
            {
                where += " AND hd.cong_ty = " + congTy;
            }
            if (fromDate.HasValue)
            {
                where += " AND sksk.ngay_kham >= '" + fromDate.Value.ToString("yyyy-MM-dd") + "'";
            }
            if (toDate.HasValue)
            {
                where += " AND sksk.ngay_kham <= '" + toDate.Value.ToString("yyyy-MM-dd") + "'";
            }
            if (!string.IsNullOrEmpty(maDieuTri))
            {
                where += " AND sksk.ma_luot_kham = '" + maDieuTri + "'";
            }
            if (fromNumber.HasValue)
            {
                where += " AND sksk.sort >= " + fromNumber;
            }
            if (toNumber.HasValue)
            {
                where += " AND sksk.sort <= " + toNumber;
            }

            // Query đếm tổng số bản ghi
            var countSql = @"
                SELECT COUNT(DISTINCT sksk.id) as TotalCount
                FROM SoKhamSucKhoe sksk 
                Left join kham_suc_khoe_cong_ty ct on ct.id = sksk.MaDotKham
                Left join contract hd on hd.id = ct.ma_hop_dong_ksk" + where;

            // Query lấy dữ liệu với phân trang
            var dataSql = @"
                select sksk.id, sksk.sort, sksk.ma_luot_kham, ct.code, u.id as user_id, u.last_name, u.first_name, u.ngay_sinh, u.gioi_tinh,  
                ts.ten_benh, ts.tien_su_gia_dinh,
                tl.chieu_cao, tl.can_nang, tl.bmi, tl.mach, tl.huyet_ap,
                ck.kq_nk_tuan_hoan, ck.kq_nk_ho_hap, ck.kq_nk_tieu_hoa, ck.kq_nk_than_tiet_nieu, ck.kq_nk_noi_tiet,
                ck.kq_nk_co_xuong_khop, ck.kq_nk_than_kinh, ck.kq_nk_tam_than, ck.kq_ngoai_khoa,
                spk.ket_qua, ck.benh_mat, ck.benh_tai_mui_hong, ck.benh_rhm, ck.kq_da_lieu,
                kl.benh_tat_ket_luan, kl.de_nghi,  kl.tu_van, CONVERT(VARCHAR(10), kl.ngay_ket_luan, 103) ngay_ket_luan, plsk.name as phan_loai_suc_khoe,
                -- Gộp tất cả kết quả cận lâm sàng thành một cột, phân cách bằng dấu |
                (SELECT STRING_AGG(
                    CASE 
                        WHEN (kqcls.ten_can_lam_san IS NOT NULL AND kqcls.ket_luan_can_lam_sang IS NOT NULL) OR cls.ket_qua IS NOT NULL
                        THEN CONCAT(
                            ISNULL(
                                kqcls.ten_can_lam_san,
                                CASE WHEN cls.[type] = 'CDHATDCN' THEN N'CDHA - TDCN'
                                WHEN cls.[type] = 'XNCongThucMau' THEN N'XN Công thức máu'
                                WHEN cls.[type] = 'XNNuocTieu' THEN N'XN nước tiểu'
                                ELSE N'XN khác'
                                END
                            ),
                            ': ',
                            ISNULL(kqcls.ket_luan_can_lam_sang, cls.ket_qua)) 
                        ELSE NULL
                    END,
                    ' | ' )
                    FROM kham_suc_khoe_ket_qua_can_lam_sang cls
                    LEFT JOIN ket_qua_can_lam_san kqcls ON kqcls.id = cls.kq_cls
                    WHERE cls.luot_kham = sksk.id
                ) AS can_lam_sang_results,
                hd.status trang_thai_hop_dong
                from SoKhamSucKhoe sksk 
                Left join kham_suc_khoe_cong_ty ct on ct.id = sksk.MaDotKham
                Left join contract hd on hd.id = ct.ma_hop_dong_ksk
                Left join custom_users u on u.id = sksk.benh_nhan 
                Left join kham_suc_khoe_tien_su ts on ts.luot_kham = sksk.id
                Left join kham_suc_khoe_the_luc tl on tl.luot_kham = sksk.id
                Left join kham_suc_khoe_kham_chuyen_khoa ck on ck.luot_kham = sksk.id
                Left join kham_suc_khoe_san_phu_khoa spk on spk.luot_kham = sksk.id
                Left join kham_suc_khoe_ket_luan kl on kl.luot_kham = sksk.id
                Left join phan_loai_suc_khoe plsk on kl.phan_loai_suc_khoe = plsk.id
                " + where + @"
                ORDER BY ct.id, sksk.sort
                OFFSET @offset ROWS 
                FETCH NEXT @limit ROWS ONLY";

            var results = new List<MedicalExaminationDto>();
            int totalCount = 0;

            await _context.Database.OpenConnectionAsync();

            // Lấy tổng số bản ghi
            using (var countCommand = _context.Database.GetDbConnection().CreateCommand())
            {
                countCommand.CommandText = countSql;
                if (!string.IsNullOrEmpty(maDotKham))
                {
                    countCommand.Parameters.Add(new SqlParameter("@maDotKham", maDotKham));
                }
                if (!string.IsNullOrEmpty(congTy))
                {
                    countCommand.Parameters.Add(new SqlParameter("@congTy", congTy));
                }
                if (fromDate.HasValue)
                {
                    countCommand.Parameters.Add(new SqlParameter("@fromDate", fromDate));
                }
                if (toDate.HasValue)
                {
                    countCommand.Parameters.Add(new SqlParameter("@toDate", toDate));
                }

                var countResult = await countCommand.ExecuteScalarAsync();
                totalCount = Convert.ToInt32(countResult ?? 0);
            }

            // Lấy dữ liệu với phân trang
            using (var dataCommand = _context.Database.GetDbConnection().CreateCommand())
            {
                dataCommand.CommandText = dataSql;
                if (!string.IsNullOrEmpty(maDotKham))
                {
                    dataCommand.Parameters.Add(new SqlParameter("@maDotKham", maDotKham));
                }
                if (!string.IsNullOrEmpty(congTy))
                {
                    dataCommand.Parameters.Add(new SqlParameter("@congTy", congTy));
                }
                if (fromDate.HasValue)
                {
                    dataCommand.Parameters.Add(new SqlParameter("@fromDate", fromDate));
                }
                if (toDate.HasValue)
                {
                    dataCommand.Parameters.Add(new SqlParameter("@toDate", toDate));
                }
                dataCommand.Parameters.Add(new SqlParameter("@offset", validOffset));
                dataCommand.Parameters.Add(new SqlParameter("@limit", validLimit));

                using (var reader = await dataCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var item = new MedicalExaminationDto
                        {
                            id = reader["id"] as int?,
                            sort = reader["sort"] as int?,
                            ma_luot_kham = reader["ma_luot_kham"]?.ToString(),
                            code = reader["code"]?.ToString(),
                            user_id = reader["user_id"]?.ToString(),
                            last_name = reader["last_name"]?.ToString(),
                            first_name = reader["first_name"]?.ToString(),
                            ngay_sinh = reader["ngay_sinh"] as DateTime?,
                            gioi_tinh = reader["gioi_tinh"]?.ToString(),

                            ten_benh = reader["ten_benh"]?.ToString(),
                            tien_su_gia_dinh = reader["tien_su_gia_dinh"]?.ToString(),
                            chieu_cao = reader["chieu_cao"]?.ToString(),
                            can_nang = reader["can_nang"]?.ToString(),
                            bmi = reader["bmi"]?.ToString(),
                            mach = reader["mach"] as int?,
                            huyet_ap = reader["huyet_ap"]?.ToString(),
                            kq_nk_tuan_hoan = reader["kq_nk_tuan_hoan"]?.ToString(),
                            kq_nk_ho_hap = reader["kq_nk_ho_hap"]?.ToString(),
                            kq_nk_tieu_hoa = reader["kq_nk_tieu_hoa"]?.ToString(),
                            kq_nk_than_tiet_nieu = reader["kq_nk_than_tiet_nieu"]?.ToString(),
                            kq_nk_noi_tiet = reader["kq_nk_noi_tiet"]?.ToString(),
                            kq_nk_co_xuong_khop = reader["kq_nk_co_xuong_khop"]?.ToString(),
                            kq_nk_than_kinh = reader["kq_nk_than_kinh"]?.ToString(),
                            kq_nk_tam_than = reader["kq_nk_tam_than"]?.ToString(),
                            kq_ngoai_khoa = reader["kq_ngoai_khoa"]?.ToString(),

                            ket_qua_san_phu_khoa = reader["ket_qua"]?.ToString(),

                            benh_mat = reader["benh_mat"]?.ToString(),
                            benh_tai_mui_hong = reader["benh_tai_mui_hong"]?.ToString(),
                            benh_rhm = reader["benh_rhm"]?.ToString(),
                            kq_da_lieu = reader["kq_da_lieu"]?.ToString(),

                            benh_tat_ket_luan = reader["benh_tat_ket_luan"]?.ToString(),
                            de_nghi = reader["de_nghi"]?.ToString(),
                            tu_van = reader["tu_van"]?.ToString(),
                            phan_loai_suc_khoe = reader["phan_loai_suc_khoe"]?.ToString(),
                            can_lam_sang_results = reader["can_lam_sang_results"]?.ToString(),
                            ngay_ket_luan = reader["ngay_ket_luan"]?.ToString(),
                            trang_thai_hop_dong = (reader["trang_thai_hop_dong"] != DBNull.Value
                                ? reader["trang_thai_hop_dong"].ToString()?.ToEnum<TrangThaiHopDong>()
                                : null)
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