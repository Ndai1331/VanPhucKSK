using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model.Dashboard.General;
using CoreAdminWeb.Model.Reports;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Net;

namespace CoreAdminWeb.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController(ApplicationDbContext context) : ControllerBase
    {
        [HttpGet("contract-unit-prices")]
        public async Task<IActionResult> GetTrackContractUnitPricesData([FromQuery] int contract, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int offset = 0, [FromQuery] int limit = 10)
        {
            var response = new RequestHttpResponse<List<ReportBaoCaoTheoDoiDonGiaTheoHopDongModel>>();
            try
            {
                var validLimit = limit <= 0 ? 10 : limit;
                var validOffset = offset < 0 ? 0 : offset;

                string whereClause = @" WHERE (dmtt.deleted IS NULL OR dmtt.deleted = 0)
                            AND (ct.deleted IS NULL OR ct.deleted = 0)
                            AND ct.id = @Contract";

                if (fromDate.HasValue)
                {
                    whereClause += $" AND ct.ngay_hop_dong >= '{fromDate:yyyy-MM-dd}'";
                }

                if (toDate.HasValue)
                {
                    whereClause += $" AND ct.ngay_hop_dong <= '{toDate:yyyy-MM-dd}'";
                }

                var queries = new DashboardQuery[]
                {
                    new DashboardQuery
                    {
                        Sql = @"
                            SELECT
	                            dm.id [Id],
	                            dm.code [MaDinhMuc],
	                            dm.[name] [TenDinhMuc],
	                            CAST(ISNULL(dm.DonGia, 0) AS decimal) [DonGiaDM],
	                            ISNULL(SUM(dmtt.so_luong), 0) [SoLuong],
	                            CAST(ISNULL(dmtt.don_gia_tt, 0) AS decimal) [DonGiaTT],
	                            CAST(ISNULL(dmtt.thanh_tien_tt, 0) AS decimal) [ThanhTienTT]
                            FROM kham_suc_khoe_dinh_muc_thuc_te dmtt
                            INNER JOIN [contract] ct ON ct.id = dmtt.[contract]
                            INNER JOIN contract_type ctt ON ctt.Id = ct.contract_type
                            INNER JOIN danh_muc_dinh_muc dm ON dm.id = dmtt.ma_dinh_muc"
                            + whereClause
                            + " GROUP BY dm.id,dm.code,dm.[name],dm.DonGia,dmtt.don_gia_tt,dmtt.thanh_tien_tt"
                            + " ORDER BY dm.[name], dmtt.don_gia_tt OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY",
                        Action = (object? obj) =>
                        {
                            if (obj is DbDataReader reader)
                            {
                                response.Data = new List<ReportBaoCaoTheoDoiDonGiaTheoHopDongModel>();
                                while (reader.Read())
                                {
                                    response.Data.Add(new ReportBaoCaoTheoDoiDonGiaTheoHopDongModel()
                                    {
                                        Id = reader["Id"] as int? ?? 0,
                                        MaDinhMuc= reader["MaDinhMuc"] as string,
                                        TenDinhMuc= reader["TenDinhMuc"] as string,
                                        DonGiaDM= reader["DonGiaDM"] as decimal? ?? 0,
                                        SoLuong = reader["SoLuong"] as int? ?? 0,
                                        DonGiaTT= reader["DonGiaTT"] as decimal? ?? 0,
                                        ThanhTienTT= reader["ThanhTienTT"] as decimal? ?? 0,
                                    });
                                }
                            }
                        }
                    },
                    new DashboardQuery
                    {
                        Sql = @"
                            SELECT
	                            COUNT(1)
                            FROM kham_suc_khoe_dinh_muc_thuc_te dmtt
                            INNER JOIN [contract] ct ON ct.id = dmtt.[contract]
                            INNER JOIN contract_type ctt ON ctt.Id = ct.contract_type
                            INNER JOIN danh_muc_dinh_muc dm ON dm.id = dmtt.ma_dinh_muc"
                            + whereClause
                            + " GROUP BY dm.id,dm.code,dm.[name],dm.DonGia,dmtt.don_gia_tt,dmtt.thanh_tien_tt",
                        Action = (object? obj) =>
                        {
                            response.Meta = new Meta(){
                                total_count = Convert.ToInt32(obj ?? 0),
                                limit = validLimit,
                                offset = validOffset
                            };
                        },
                        IsCountQuery = true
                    }
                };

                await context.Database.OpenConnectionAsync();
                foreach (var q in queries)
                {
                    if (q.Action == null)
                    {
                        continue;
                    }

                    using var cmd = context.Database.GetDbConnection().CreateCommand();
                    cmd.CommandText = q.Sql;
                    cmd.Parameters.Add(new SqlParameter("@Contract", contract));
                    cmd.Parameters.Add(new SqlParameter("@offset", validOffset));
                    cmd.Parameters.Add(new SqlParameter("@limit", validLimit));

                    if (q.IsCountQuery)
                    {
                        var reader = await cmd.ExecuteScalarAsync();
                        q.Action(reader);
                    }
                    else
                    {
                        using var reader = await cmd.ExecuteReaderAsync();

                        q.Action(reader);
                    }
                }

                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Errors.Add(BaseServiceHelper.CreateErrorResponse(ex));
                response.StatusCode = HttpStatusCode.InternalServerError;
                return BadRequest(response);
            }
            finally
            {
                if (context.Database.GetDbConnection().State == ConnectionState.Open)
                {
                    await context.Database.CloseConnectionAsync();
                }
            }
        }
    }
}
