
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

            var where = " WHERE (hd.deleted = 0) AND (ct.deleted = 0) AND ct.id IS NOT NULL AND (sksk.deleted = 0 OR sksk.deleted IS NULL)";
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
                WITH kq_src AS (
                    SELECT
                        luot_kham_code = NULLIF(LTRIM(RTRIM(CAST(kq.ma_luot_kham AS NVARCHAR(200)))), N''),
                        grp_class = CASE
                            WHEN kq.ma_cls = 'XN210' THEN 'A'
                            WHEN kq.ma_cls <> 'XN210' AND kq.ten_loai_cls = N'Huyết học' THEN 'B'
                            WHEN kq.ma_cls <> 'XN210' AND kq.ten_loai_cls = N'CDHA'       THEN 'C'
                            WHEN kq.ma_cls <> 'XN210' AND kq.ten_loai_cls IS NOT NULL
                                 AND kq.ten_loai_cls <> N'CDHA' AND kq.ten_loai_cls <> N'Huyết học' THEN 'D'
                            ELSE NULL
                        END,
                        grp_show = CASE
                            WHEN kq.ma_cls = 'XN210' THEN N'XN Nước tiểu'
                            WHEN kq.ten_loai_cls IN (N'Huyết học', N'CDHA') THEN kq.ten_loai_cls
                            ELSE N'XN Khác'
                        END,
                        ten_cls   = kq.ten_cls,
                        expr_item = CASE 
                            WHEN kq.ma_cls = 'XN210' THEN kq.chi_so + N': ' + kq.ket_qua_chi_so
                            ELSE kq.ket_qua_chi_so
                        END
                    FROM dbo.ket_qua_can_lam_sang_chi_tiet kq
                ),
                cls_items AS (
                    SELECT
                        s.luot_kham_code, s.grp_class, s.grp_show, s.ten_cls,
                        cls_items = CAST(STRING_AGG(CAST(s.expr_item AS NVARCHAR(MAX)), NCHAR(13) + NCHAR(10) + N'       ') AS NVARCHAR(MAX))
                    FROM kq_src s
                    WHERE s.luot_kham_code IS NOT NULL
                      AND s.expr_item IS NOT NULL AND LTRIM(RTRIM(s.expr_item)) <> N''
                    GROUP BY s.luot_kham_code, s.grp_class, s.grp_show, s.ten_cls
                ),
                cls_block AS (
                    SELECT
                        i.luot_kham_code, i.grp_class, i.grp_show, i.ten_cls,
                        cls_block = CAST(N'   ' + i.ten_cls + NCHAR(13) + NCHAR(10) + N'       ' + i.cls_items AS NVARCHAR(MAX))
                    FROM cls_items i
                ),
                grp_agg AS (
                    SELECT
                        b.luot_kham_code, b.grp_class, b.grp_show,
                        grp_block = CAST(STRING_AGG(CAST(b.cls_block AS NVARCHAR(MAX)), NCHAR(13) + NCHAR(10)) AS NVARCHAR(MAX))
                    FROM cls_block b
                    GROUP BY b.luot_kham_code, b.grp_class, b.grp_show
                ),
                final_agg AS (
                    SELECT
                        g.luot_kham_code,
                        ket_qua_cls = CAST(
                            STRING_AGG(
                                CAST(g.grp_show + NCHAR(13) + NCHAR(10) + g.grp_block AS NVARCHAR(MAX)),
                                NCHAR(13) + NCHAR(10)
                            )
                            WITHIN GROUP (ORDER BY 
                                CASE g.grp_class WHEN 'A' THEN 1 WHEN 'B' THEN 2 WHEN 'C' THEN 3 WHEN 'D' THEN 4 ELSE 5 END,
                                g.grp_show
                            ) AS NVARCHAR(MAX)
                        )
                    FROM grp_agg g
                    GROUP BY g.luot_kham_code
                )
                SELECT
                    sksk.id, sksk.sort, sksk.ma_luot_kham, ct.code,
                    u.id AS user_id, u.last_name, u.first_name, u.ngay_sinh, u.gioi_tinh,
                    ts.ten_benh, ts.tien_su_gia_dinh,
                    tl.chieu_cao, tl.can_nang, tl.bmi, tl.mach, tl.huyet_ap,
                    ck.kq_nk_tuan_hoan, ck.kq_nk_ho_hap, ck.kq_nk_tieu_hoa, ck.kq_nk_than_tiet_nieu, ck.kq_nk_noi_tiet,
                    ck.kq_nk_co_xuong_khop, ck.kq_nk_than_kinh, ck.kq_nk_tam_than, ck.kq_ngoai_khoa,
                    spk.ket_qua, ck.benh_mat, ck.benh_tai_mui_hong, ck.benh_rhm, ck.kq_da_lieu,
                    kl.benh_tat_ket_luan, kl.de_nghi,
                    plsk.name AS phan_loai_suc_khoe,
                    fa.ket_qua_cls AS can_lam_sang_results
                FROM SoKhamSucKhoe sksk 
                LEFT JOIN kham_suc_khoe_cong_ty ct    ON ct.id = sksk.MaDotKham
                LEFT JOIN contract hd                  ON hd.id = ct.ma_hop_dong_ksk
                LEFT JOIN custom_users u               ON u.id = sksk.benh_nhan 
                LEFT JOIN kham_suc_khoe_tien_su ts     ON ts.luot_kham = sksk.id
                LEFT JOIN kham_suc_khoe_the_luc tl     ON tl.luot_kham = sksk.id
                LEFT JOIN kham_suc_khoe_kham_chuyen_khoa ck ON ck.luot_kham = sksk.id
                LEFT JOIN kham_suc_khoe_san_phu_khoa spk    ON spk.luot_kham = sksk.id
                LEFT JOIN kham_suc_khoe_ket_luan kl         ON kl.luot_kham = sksk.id
                LEFT JOIN phan_loai_suc_khoe plsk
                    ON (kl.phan_loai_suc_khoe IS NOT NULL AND TRY_CONVERT(INT, kl.phan_loai_suc_khoe) = plsk.id)
                    OR (kl.phan_loai_suc_khoe IS NULL AND kl.ma_phan_loai_suc_khoe = plsk.code)
                LEFT JOIN final_agg fa
                    ON fa.luot_kham_code = NULLIF(LTRIM(RTRIM(CAST(sksk.ma_luot_kham AS NVARCHAR(200)))), N'')
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
                            phan_loai_suc_khoe = reader["phan_loai_suc_khoe"]?.ToString(),
                            can_lam_sang_results = reader["can_lam_sang_results"]?.ToString()
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