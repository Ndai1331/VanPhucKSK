
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CoreAdminWeb.Models;
using CoreAdminWeb.Model;
using System.Net;
using System.Data;
using CoreAdminWeb.Model.RequestHttps;

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
    public async Task<IActionResult> GetMedicalData([FromQuery]string? benhNhan, [FromQuery]string? congTy, [FromQuery]DateTime? fromDate, [FromQuery]DateTime? toDate, [FromQuery]int offset = 0, [FromQuery]int limit = 10)
    {

        var response = new RequestHttpResponse<List<HoSoKhamSucKhoeTT32Model>>();
        try
        {
            // Validate parameters
            var validLimit = limit <= 0 || limit > 100 ? 10 : limit;
            var validOffset = offset < 0 ? 0 : offset;
            var where = "";
            if (!string.IsNullOrEmpty(benhNhan))
            {
                where += " WHERE sksk.benh_nhan = '" + benhNhan + "'";
            }
            if (!string.IsNullOrEmpty(congTy))
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += " WHERE ";
                }
                else
                {
                    where += " AND ";
                }
                where += "hd.cong_ty = " + congTy;
            }
            if (fromDate.HasValue)
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += " WHERE ";
                }
                else
                {
                    where += " AND ";
                }
                where += "sksk.ngay_kham >= '" + fromDate.Value.ToString("yyyy-MM-dd") + "'";
            }
            if (toDate.HasValue)
            {
                if (string.IsNullOrEmpty(where))
                {
                    where += " WHERE ";
                }
                else
                {
                    where += " AND ";
                }
                where += "sksk.ngay_kham <= '" + toDate.Value.ToString("yyyy-MM-dd") + "'";
            }

            // Query đếm tổng số bản ghi
            var countSql = @"
                SELECT COUNT(DISTINCT sksk.id) as TotalCount
                FROM SoKhamSucKhoe sksk 
                Left join kham_suc_khoe_cong_ty ct on ct.id = sksk.MaDotKham
                Left join contract hd on hd.id = ct.ma_hop_dong_ksk" + where;

            // Query lấy dữ liệu với phân trang
            var dataSql = @"
                select sksk.id, sksk.ma_luot_kham, sksk.ngay_kham, u.ma_benh_nhan, u.last_name, u.first_name,
                kl.benh_tat_ket_luan, kl.de_nghi, plsk.name as phan_loai_suc_khoe,
                (select top 1 name from CongTy where id = hd.cong_ty) as cong_ty
                from SoKhamSucKhoe sksk 
                Left join kham_suc_khoe_cong_ty ct on ct.id = sksk.MaDotKham
                Left join contract hd on hd.id = ct.ma_hop_dong_ksk
                Left join custom_users u on u.id = sksk.benh_nhan 
                Left join kham_suc_khoe_ket_luan kl on kl.ma_luot_kham = sksk.ma_luot_kham
                Left join phan_loai_suc_khoe plsk on kl.phan_loai_suc_khoe = plsk.id
                " + where + @"
                GROUP BY sksk.id, sksk.ma_luot_kham, sksk.ngay_kham, u.ma_benh_nhan, u.last_name, u.first_name,
                kl.benh_tat_ket_luan, kl.de_nghi, plsk.name, hd.cong_ty
                ORDER BY sksk.id
                OFFSET @offset ROWS 
                FETCH NEXT @limit ROWS ONLY";

            var results = new List<HoSoKhamSucKhoeTT32Model>();
            int totalCount = 0;

            await _context.Database.OpenConnectionAsync();

            // Lấy tổng số bản ghi
            using (var countCommand = _context.Database.GetDbConnection().CreateCommand())
            {
                countCommand.CommandText = countSql;
                if (!string.IsNullOrEmpty(benhNhan))
                {
                    countCommand.Parameters.Add(new SqlParameter("@benhNhan", benhNhan));
                }
                if (!string.IsNullOrEmpty(congTy))
                {
                    countCommand.Parameters.Add(new SqlParameter("@congTy", congTy));
                }
                
                var countResult = await countCommand.ExecuteScalarAsync();
                totalCount = Convert.ToInt32(countResult ?? 0);
            }

            // Lấy dữ liệu với phân trang
            using (var dataCommand = _context.Database.GetDbConnection().CreateCommand())
            {
                dataCommand.CommandText = dataSql;
                if (!string.IsNullOrEmpty(benhNhan))
                {
                    dataCommand.Parameters.Add(new SqlParameter("@benhNhan", benhNhan));
                }
                if (!string.IsNullOrEmpty(congTy))
                {
                    dataCommand.Parameters.Add(new SqlParameter("@congTy", congTy));
                }
                dataCommand.Parameters.Add(new SqlParameter("@offset", validOffset));
                dataCommand.Parameters.Add(new SqlParameter("@limit", validLimit));
                
                using (var reader = await dataCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var item = new HoSoKhamSucKhoeTT32Model
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