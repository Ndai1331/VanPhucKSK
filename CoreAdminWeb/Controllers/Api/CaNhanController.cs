
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
/// CaNhanController API Controller
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CaNhanController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CaNhanController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("medical-data")]
    public async Task<IActionResult> GetMedicalData([FromQuery] string? benhNhan, [FromQuery] string? congTy, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int offset = 0, [FromQuery] int limit = 10)
    {

        var response = new RequestHttpResponse<List<HoSoKhamSucKhoeTT32Model>>();
        try
        {
            // Validate parameters
            var validLimit = limit <= 0 ? 10 : limit;
            var validOffset = offset < 0 ? 0 : offset;
            var predicates = new List<string>();
            if (!string.IsNullOrEmpty(benhNhan))
            {
                predicates.Add("sksk.benh_nhan = @benhNhan");
            }
            if (!string.IsNullOrEmpty(congTy))
            {
                predicates.Add("hd.cong_ty = @congTy");
            }
            if (fromDate.HasValue)
            {
                predicates.Add("sksk.ngay_kham >= @fromDate");
            }
            if (toDate.HasValue)
            {
                predicates.Add("sksk.ngay_kham < DATEADD(DAY, 1, @toDate)");
            }
            var where = predicates.Count == 0 ? string.Empty : " WHERE " + string.Join(" AND ", predicates);

            // Query đếm tổng số bản ghi
            var countSql = @"
                SELECT COUNT(DISTINCT sksk.id) as TotalCount
                FROM SoKhamSucKhoe sksk 
                Left join kham_suc_khoe_cong_ty ct on ct.id = sksk.MaDotKham
                Left join contract hd on hd.id = ct.ma_hop_dong_ksk" + where;

            // Query lấy dữ liệu với phân trang
            var dataSql = @"
                select sksk.id, sksk.ma_luot_kham, sksk.ngay_kham, u.ma_tai_khoan as ma_benh_nhan, u.last_name, u.first_name,
                kl.benh_tat_ket_luan, kl.de_nghi, kl.tu_van, plsk.name as phan_loai_suc_khoe,
                c.name as cong_ty
                from SoKhamSucKhoe sksk 
                Left join kham_suc_khoe_cong_ty ct on ct.id = sksk.MaDotKham
                Left join contract hd on hd.id = ct.ma_hop_dong_ksk
                Left join CongTy c on c.id = hd.cong_ty
                Left join custom_users u on u.id = sksk.benh_nhan 
                Left join kham_suc_khoe_ket_luan kl on kl.ma_luot_kham = sksk.ma_luot_kham
                Left join phan_loai_suc_khoe plsk on kl.phan_loai_suc_khoe = plsk.id
                " + where + @"
                GROUP BY sksk.id, sksk.ma_luot_kham, sksk.ngay_kham, u.ma_tai_khoan, u.last_name, u.first_name,
                kl.benh_tat_ket_luan, kl.de_nghi, kl.tu_van, plsk.name, c.name
                ORDER BY sksk.id
                OFFSET @offset ROWS 
                FETCH NEXT @limit ROWS ONLY";

            var results = new List<HoSoKhamSucKhoeTT32Model>();
            int totalCount = 0;

            await _context.Database.OpenConnectionAsync(HttpContext.RequestAborted);
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = countSql + ";\n" + dataSql;
            if (!string.IsNullOrEmpty(benhNhan)) command.Parameters.Add(new SqlParameter("@benhNhan", benhNhan));
            if (!string.IsNullOrEmpty(congTy)) command.Parameters.Add(new SqlParameter("@congTy", congTy));
            if (fromDate.HasValue) command.Parameters.Add(new SqlParameter("@fromDate", fromDate.Value));
            if (toDate.HasValue) command.Parameters.Add(new SqlParameter("@toDate", toDate.Value));
            command.Parameters.Add(new SqlParameter("@offset", validOffset));
            command.Parameters.Add(new SqlParameter("@limit", validLimit));

            // Count and page data are returned as two result sets in one round trip.
            using var reader = await command.ExecuteReaderAsync(HttpContext.RequestAborted);
            if (await reader.ReadAsync(HttpContext.RequestAborted))
            {
                totalCount = Convert.ToInt32(reader["TotalCount"]);
            }

            if (await reader.NextResultAsync(HttpContext.RequestAborted))
            {
                while (await reader.ReadAsync(HttpContext.RequestAborted))
                {
                    results.Add(new HoSoKhamSucKhoeTT32Model
                    {
                        id = reader["id"] as int? ?? 0,
                        ma_luot_kham = reader["ma_luot_kham"]?.ToString(),
                        ngay_kham = reader["ngay_kham"] as DateTime?,
                        ma_benh_nhan = reader["ma_benh_nhan"]?.ToString(),
                        last_name = reader["last_name"]?.ToString(),
                        first_name = reader["first_name"]?.ToString(),
                        benh_tat_ket_luan = reader["benh_tat_ket_luan"]?.ToString(),
                        de_nghi = reader["de_nghi"]?.ToString(),
                        phan_loai_suc_khoe = reader["phan_loai_suc_khoe"]?.ToString(),
                        cong_ty = reader["cong_ty"]?.ToString(),
                        tu_van = reader["tu_van"]?.ToString(),
                    });
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
            return StatusCode(StatusCodes.Status500InternalServerError, response);
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
