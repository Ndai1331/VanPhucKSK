
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
/// HoaDonDienTu API Controller
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class HoaDonDienTuController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HoaDonDienTuController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("electronic-invoice")]
    public async Task<IActionResult> GetElectronicInvoice([FromQuery]string maBenhNhan, [FromQuery]int offset = 0, [FromQuery]int limit = 10)
    {

        var response = new RequestHttpResponse<List<HoaDonDienTuModel>>();
        try
        {
            // Validate parameters
            var validLimit = limit <= 0 || limit > 100 ? 10 : limit;
            var validOffset = offset < 0 ? 0 : offset;

            // Query đếm tổng số bản ghi
            var countSql = @"
                SELECT COUNT(DISTINCT sksk.id) as TotalCount
                FROM SoKhamSucKhoe sksk 
                Left join hoa_don_dien_tu hd on hd.ma_luot_kham  = sksk.ma_luot_kham
                where sksk.ma_benh_nhan  =@maBenhNhan
                and hd.id is not null";

            // Query lấy dữ liệu với phân trang
            var dataSql = @"
                select hd.ma_luot_kham, hd.so_hoa_don, hd.ngay_hoa_don, hd.ma_hoa_don, hd.ma_giao_dich, hd.ma_so_thue,
                hd.url_tra_cuu
                from SoKhamSucKhoe sksk  
                Left join hoa_don_dien_tu hd on hd.ma_luot_kham  = sksk.ma_luot_kham
                where sksk.ma_benh_nhan  =@maBenhNhan
                and hd.id is not null
                ORDER BY hd.ngay_hoa_don DESC
                OFFSET @offset ROWS 
                FETCH NEXT @limit ROWS ONLY";

            var results = new List<HoaDonDienTuModel>();
            int totalCount = 0;

            await _context.Database.OpenConnectionAsync();

            // Lấy tổng số bản ghi
            using (var countCommand = _context.Database.GetDbConnection().CreateCommand())
            {
                countCommand.CommandText = countSql;
                countCommand.Parameters.Add(new SqlParameter("@maBenhNhan", maBenhNhan));
                
                var countResult = await countCommand.ExecuteScalarAsync();
                totalCount = Convert.ToInt32(countResult ?? 0);
            }

            // Lấy dữ liệu với phân trang
            using (var dataCommand = _context.Database.GetDbConnection().CreateCommand())
            {
                dataCommand.CommandText = dataSql;
                dataCommand.Parameters.Add(new SqlParameter("@maBenhNhan", maBenhNhan));
                dataCommand.Parameters.Add(new SqlParameter("@offset", validOffset));
                dataCommand.Parameters.Add(new SqlParameter("@limit", validLimit));
                
                using (var reader = await dataCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var item = new HoaDonDienTuModel
                        {
                            ma_luot_kham = reader["ma_luot_kham"]?.ToString(),
                            so_hoa_don = reader["so_hoa_don"]?.ToString(),
                            ngay_hoa_don = reader["ngay_hoa_don"] as DateTime?,
                            ma_hoa_don = reader["ma_hoa_don"]?.ToString(),
                            ma_giao_dich = reader["ma_giao_dich"]?.ToString(),
                            ma_so_thue = reader["ma_so_thue"]?.ToString(),
                            url_tra_cuu = reader["url_tra_cuu"]?.ToString(),
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