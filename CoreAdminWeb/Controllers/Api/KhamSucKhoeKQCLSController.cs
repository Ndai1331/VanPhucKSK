using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model.KhamSucKhoes;
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
public class KhamSucKhoeKQCLSController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public KhamSucKhoeKQCLSController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("get-ket-qua")]
    public async Task<IActionResult> GetData([FromQuery] string? maLuotKham, [FromQuery] List<string>? maLuotKhams)
    {

        var response = new RequestHttpResponse<List<KetQuaCLSChiTietModel>>();
        try
        {
            // Validate parameters

            string seedValue = string.Empty;
            if (!string.IsNullOrEmpty(maLuotKham))
            {
                seedValue += $"('{maLuotKham}')";
            }

            if (maLuotKhams != null && maLuotKhams.Any())
            {
                seedValue += string.Join(",", maLuotKhams.Select(c => $"('{c}')"));
            }

            if (string.IsNullOrEmpty(seedValue))
            {
                seedValue = "(NULL)";
            }

            var dataSQL = @";WITH
seed_raw AS (
    SELECT v.ma_luot_kham
    FROM (VALUES
        " + seedValue + @"
    ) AS v(ma_luot_kham)
),
lk_seed AS(
    SELECT DISTINCT LTRIM(RTRIM(ma_luot_kham)) AS ma_luot_kham
    FROM seed_raw
    WHERE ma_luot_kham IS NOT NULL
),
lk_base AS(
    SELECT DISTINCT LTRIM(RTRIM(kq.ma_luot_kham)) AS ma_luot_kham
    FROM dbo.ket_qua_can_lam_sang_chi_tiet kq
),
lk_pick AS(
    SELECT s.ma_luot_kham FROM lk_seed s
    UNION
    SELECT b.ma_luot_kham FROM lk_base b
    WHERE NOT EXISTS(SELECT 1 FROM lk_seed)
),
lk AS(
    SELECT
        m.ma_benh_nhan,
        p.ma_luot_kham
    FROM lk_pick p
    LEFT JOIN (
        SELECT DISTINCT
            LTRIM(RTRIM(ma_luot_kham)) AS ma_luot_kham,
            LTRIM(RTRIM(ma_benh_nhan)) AS ma_benh_nhan
        FROM dbo.ket_qua_can_lam_sang_chi_tiet
    ) m ON m.ma_luot_kham = p.ma_luot_kham
),
src AS(
  SELECT
      kq.ma_benh_nhan,
      kq.ma_luot_kham,
      grp_class = CASE
        WHEN kq.ma_cls = 'XN210' THEN 'A'
        WHEN kq.ma_cls<> 'XN210' AND kq.ten_loai_cls = N'Huyết học' THEN 'B'
        WHEN kq.ma_cls<> 'XN210' AND kq.ten_loai_cls = N'CDHA'       THEN 'C'
        WHEN kq.ma_cls<> 'XN210' AND kq.ten_loai_cls IS NOT NULL
             AND kq.ten_loai_cls<> N'CDHA' AND kq.ten_loai_cls<> N'Huyết học' THEN 'D'
        ELSE NULL
      END,
      grp_show = CASE
        WHEN kq.ma_cls = 'XN210' THEN N'XN Nước tiểu'
        WHEN kq.ten_loai_cls IN(N'Huyết học', N'CDHA') THEN kq.ten_loai_cls
        ELSE N'XN Khác'
      END,
      expr_item = CASE WHEN kq.ma_cls = 'XN210'
                       THEN kq.chi_so + N': ' + kq.ket_qua_chi_so
                       ELSE kq.ket_qua_chi_so END,
      expr_bt = CAST(kq.bat_thuong AS nvarchar(max)),
      file_nm = CAST(kq.ten_file   AS nvarchar(max)),
      kq.ten_cls
  FROM dbo.ket_qua_can_lam_sang_chi_tiet kq
  INNER JOIN lk ON lk.ma_luot_kham = LTRIM(RTRIM(kq.ma_luot_kham))
),
per_cls AS(
  SELECT
      s.ma_benh_nhan, s.ma_luot_kham, s.grp_show, s.grp_class, s.ten_cls,
      block_text = s.ten_cls + N':' + CHAR(10) +
                   STRING_AGG(CAST(s.expr_item AS nvarchar(max)), CHAR(10))
  FROM src s
  GROUP BY s.ma_benh_nhan, s.ma_luot_kham, s.grp_show, s.grp_class, s.ten_cls
),
grouped AS(
  SELECT
      pc.ma_benh_nhan, pc.ma_luot_kham, pc.grp_show AS ten_loai_cls,
      STRING_AGG(pc.block_text, CHAR(10)+CHAR(10))
        WITHIN GROUP(ORDER BY pc.ten_cls) AS ket_qua_cls
  FROM per_cls pc
  GROUP BY pc.ma_benh_nhan, pc.ma_luot_kham, pc.grp_show
),
agg_other AS(
  SELECT
      s.ma_benh_nhan, s.ma_luot_kham, s.grp_show,
      COALESCE(
        STRING_AGG(CASE WHEN s.grp_class = 'A' THEN s.expr_bt END, N'; '),
        STRING_AGG(CASE WHEN s.grp_class<> 'A' THEN s.expr_bt END, N'| ')
      ) AS bat_thuong,
      COALESCE(
        MAX(CASE WHEN s.grp_class = 'A' THEN s.file_nm END),
        STRING_AGG(CASE WHEN s.grp_class<> 'A' THEN s.file_nm END, N'| ')
      ) AS ten_file
  FROM src s
  GROUP BY s.ma_benh_nhan, s.ma_luot_kham, s.grp_show
),
grp_dim AS(
  SELECT lk.ma_benh_nhan, lk.ma_luot_kham, 1 AS ord, N'CDHA'           AS ten_loai_cls FROM lk
  UNION ALL SELECT lk.ma_benh_nhan, lk.ma_luot_kham, 2, N'Huyết học'                  FROM lk
  UNION ALL SELECT lk.ma_benh_nhan, lk.ma_luot_kham, 3, N'XN Nước tiểu'               FROM lk
  UNION ALL SELECT lk.ma_benh_nhan, lk.ma_luot_kham, 4, N'XN Khác'                     FROM lk
)
SELECT
    gd.ma_benh_nhan,
    gd.ma_luot_kham,
    gd.ten_loai_cls,
    COALESCE(g.ket_qua_cls, N'') AS ket_qua_cls,
    COALESCE(a.bat_thuong, N'') AS bat_thuong,
    COALESCE(a.ten_file, N'') AS ten_file
FROM grp_dim gd
LEFT JOIN grouped  g
  ON g.ma_benh_nhan = gd.ma_benh_nhan
 AND g.ma_luot_kham = gd.ma_luot_kham
 AND g.ten_loai_cls = gd.ten_loai_cls
LEFT JOIN agg_other a
  ON a.ma_benh_nhan = gd.ma_benh_nhan
 AND a.ma_luot_kham = gd.ma_luot_kham
 AND a.grp_show = gd.ten_loai_cls
ORDER BY gd.ma_benh_nhan, gd.ma_luot_kham, gd.ord;
            ";

            var results = new List<KetQuaCLSChiTietModel>();

            await _context.Database.OpenConnectionAsync();

            // Lấy dữ liệu với phân trang
            using (var dataCommand = _context.Database.GetDbConnection().CreateCommand())
            {
                dataCommand.CommandText = dataSQL;

                using (var reader = await dataCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var item = new KetQuaCLSChiTietModel
                        {
                            bat_thuong = DataSetHelper.ReadString(reader, "bat_thuong"),
                            ket_qua_cls = DataSetHelper.ReadString(reader, "ket_qua_cls"),
                            ma_benh_nhan = DataSetHelper.ReadString(reader, "ma_benh_nhan"),
                            ma_luot_kham = DataSetHelper.ReadString(reader, "ma_luot_kham"),
                            ten_file = DataSetHelper.ReadString(reader, "ten_file"),
                            ten_loai_cls = DataSetHelper.ReadString(reader, "ten_loai_cls")
                        };

                        results.Add(item);
                    }
                }
            }

            response.Data = results;
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

    [HttpGet("get-ket-qua-chi-tiet")]
    public async Task<IActionResult> GetDetailData([FromQuery] string? maLuotKham, [FromQuery] string? maCls, [FromQuery] int? offset, [FromQuery] int? limit)
    {

        var response = new RequestHttpResponse<List<KetQuaCLSChiTietModel>>();
        try
        {
            // Validate parameters

            string whereClause = string.Empty;
            string pagingClause = string.Empty;
            if (!string.IsNullOrEmpty(maLuotKham))
            {
                whereClause = " AND kq.ma_luot_kham = @maLuotKham";
            }
            if (!string.IsNullOrEmpty(maCls))
            {
                whereClause = " AND kq.ma_cls = @maCls";
            }

            if (limit.HasValue && limit > 0)
            {
                if (!offset.HasValue || offset < 0)
                {
                    offset = 0;
                }

                pagingClause = " OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY ";
            }

            var dataSQL = @";WITH src AS (
  SELECT
      kq.ma_benh_nhan,
      kq.ma_luot_kham,
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
      expr_item = CASE 
        WHEN kq.ma_cls = 'XN210' 
             THEN kq.chi_so + N': ' + kq.ket_qua_chi_so
        ELSE kq.ket_qua_chi_so
      END,
      expr_bt  = CAST(kq.bat_thuong AS nvarchar(max)),
      file_nm  = CAST(kq.ten_file   AS nvarchar(max)),
      kq.ma_cls,
      kq.ten_cls,
      sk.ngay_kham
  FROM dbo.ket_qua_can_lam_sang_chi_tiet AS kq
  LEFT JOIN SoKhamSucKhoe sk ON sk.ma_luot_kham = kq.ma_luot_kham
  WHERE (
        kq.ma_cls = 'XN210'
     OR (kq.ma_cls <> 'XN210' AND (
            kq.ten_loai_cls = N'Huyết học'
         OR kq.ten_loai_cls = N'CDHA'
         OR (kq.ten_loai_cls IS NOT NULL AND kq.ten_loai_cls <> N'CDHA' AND kq.ten_loai_cls <> N'Huyết học')
     )))" + whereClause + @"
),
grouped AS (
  SELECT
    s.ma_benh_nhan,
    s.ma_luot_kham,
    s.ma_cls,
    s.ten_cls,
    s.grp_show AS ten_loai_cls,
    (
        CASE WHEN MIN(CASE WHEN s.grp_class = 'A' THEN 1 ELSE 0 END) = 1
             THEN s.grp_show ELSE s.ten_cls END
    ) + N':' + CHAR(13) + CHAR(10)
      + STRING_AGG(CAST(s.expr_item AS nvarchar(max)), CHAR(13) + CHAR(10)) AS ket_qua_cls,
    COALESCE(
      STRING_AGG(CASE WHEN s.grp_class = 'A' THEN s.expr_bt END, N'; '),
      STRING_AGG(CASE WHEN s.grp_class <> 'A' THEN s.expr_bt END, N'| ')
    ) AS bat_thuong,
    COALESCE(
      MAX(CASE WHEN s.grp_class = 'A' THEN s.file_nm END),
      STRING_AGG(CASE WHEN s.grp_class <> 'A' THEN s.file_nm END, N'| ')
    ) AS ten_file,
    MIN(s.ngay_kham) AS ngay_kham,
    order_grp = MIN(CASE s.grp_class WHEN 'A' THEN 1 WHEN 'B' THEN 2 WHEN 'C' THEN 3 ELSE 4 END)
  FROM src AS s
  GROUP BY
      s.ma_benh_nhan,
      s.ma_luot_kham,
      s.grp_show,
      s.ma_cls,
      s.ten_cls
),
grp_dim AS (
  SELECT 1 AS ord, N'CDHA'           AS ten_loai_cls UNION ALL
  SELECT 2       , N'Huyết học'                      UNION ALL
  SELECT 3       , N'XN Nước tiểu'                   UNION ALL
  SELECT 4       , N'XN Khác'
),
pairs AS (
  SELECT DISTINCT s.ma_benh_nhan, s.ma_luot_kham
  FROM src s
  UNION ALL
  SELECT NULL AS ma_benh_nhan, NULL AS ma_luot_kham
  WHERE NOT EXISTS (SELECT 1 FROM src)
),
base4 AS (
  SELECT
    p.ma_benh_nhan,
    p.ma_luot_kham,
    gd.ten_loai_cls,
    gd.ord AS order_grp
  FROM pairs p
  CROSS JOIN grp_dim gd
)
SELECT
    COALESCE(g.ma_benh_nhan, b.ma_benh_nhan) AS ma_benh_nhan,
    b.ma_luot_kham,
    b.ten_loai_cls,
    g.ket_qua_cls,
    g.bat_thuong,
    g.ten_file,
    g.ngay_kham,
    g.ma_cls,
    g.ten_cls,
    b.order_grp
FROM base4 b
LEFT JOIN grouped g
  ON g.ma_luot_kham = b.ma_luot_kham
 AND g.ten_loai_cls = b.ten_loai_cls
ORDER BY
    b.ma_luot_kham,
    b.order_grp,
    b.ten_loai_cls,
    g.ma_cls,
    g.ten_cls" + pagingClause;

            var results = new List<KetQuaCLSChiTietModel>();

            await _context.Database.OpenConnectionAsync();

            using (var dataCommand = _context.Database.GetDbConnection().CreateCommand())
            {
                dataCommand.CommandText = dataSQL;
                if (!string.IsNullOrEmpty(maLuotKham))
                {
                    dataCommand.Parameters.Add(new SqlParameter("@maLuotKham", maLuotKham));
                }

                if (!string.IsNullOrEmpty(maCls))
                {
                    dataCommand.Parameters.Add(new SqlParameter("@maCls", maCls));
                }

                if (limit.HasValue && limit > 0)
                {
                    dataCommand.Parameters.Add(new SqlParameter("@offset", offset));
                    dataCommand.Parameters.Add(new SqlParameter("@limit", limit));
                }

                using (var reader = await dataCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var item = new KetQuaCLSChiTietModel
                        {
                            bat_thuong = DataSetHelper.ReadString(reader, "bat_thuong"),
                            ket_qua_cls = DataSetHelper.ReadString(reader, "ket_qua_cls"),
                            ma_benh_nhan = DataSetHelper.ReadString(reader, "ma_benh_nhan"),
                            ma_luot_kham = DataSetHelper.ReadString(reader, "ma_luot_kham"),
                            ten_file = DataSetHelper.ReadString(reader, "ten_file"),
                            ten_loai_cls = DataSetHelper.ReadString(reader, "ten_loai_cls"),
                            ma_cls = DataSetHelper.ReadString(reader, "ma_cls"),
                            ten_cls = DataSetHelper.ReadString(reader, "ten_cls"),
                            ngay_kham = DataSetHelper.ReadDateTime(reader, "ngay_kham")
                        };

                        results.Add(item);
                    }
                }
            }

            response.Data = results;
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