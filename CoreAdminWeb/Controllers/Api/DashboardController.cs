using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model.Dashboard.General;
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
    /// <summary>
    /// Provides endpoints for accessing dashboard-related data.
    /// </summary>
    /// <remarks>This controller handles requests related to medical data and provides aggregated information
    /// based on the specified date range and company.</remarks>
    /// <param name="context"></param>
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController(ApplicationDbContext context) : ControllerBase
    {
        [HttpGet("company-medical-data")]
        public async Task<IActionResult> GetCompanyMedicalData([FromQuery] string company, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var response = new RequestHttpResponse<GeneralDashboardModel>();
            try
            {
                response.Data = new GeneralDashboardModel();

                var queries = new DashboardQuery[]
                {
                    new DashboardQuery
                    {
                        Sql = @"
                            SELECT
                                COUNT(1) SoDotKham,
                                (SELECT COUNT(1) FROM SoKhamSucKhoe sksk WHERE sksk.MaDotKham = ksk.id) SoLuotKham,
                                (SELECT SUM(kskdm.thanh_tien_tt) FROM kham_suc_khoe_dinh_muc_thuc_te kskdm WHERE kskdm.MaDotKham = ksk.id) ChiPhi,
                                0 CaBatThuong
                            FROM kham_suc_khoe_cong_ty ksk
                            WHERE CAST(ngay_du_kien_kham AS DATE) BETWEEN @FromDate AND @ToDate
                            AND ma_don_vi = @MaDonVi
                            GROUP BY ksk.id",
                        Action = (DbDataReader reader) =>
                        {
                            response.Data.CompanyExamination = new CompanyExaminationModel();
                            while (reader.Read())
                            {
                                response.Data.CompanyExamination.ToTalExaminations = reader["SoDotKham"] as int? ?? 0;
                                response.Data.CompanyExamination.TotalExaminationRecords = reader["SoLuotKham"] as int? ?? 0;
                                response.Data.CompanyExamination.TotalCost = reader["ChiPhi"] as decimal? ?? 0;
                                response.Data.CompanyExamination.AbnormalCases = reader["CaBatThuong"] as int? ?? 0;
                            }
                        }
                    },
                    new DashboardQuery
                    {
                        Sql = @"
                            SELECT TOP 1
	                            ksk.ngay_du_kien_kham [LastDate],
	                            (SELECT COUNT(1) FROM SoKhamSucKhoe sksk WHERE sksk.MaDotKham = ksk.id) SoLuotKham,
	                            0 CaBatThuong
                            FROM kham_suc_khoe_cong_ty ksk
                            WHERE ksk.id = (
	                            SELECT
		                            MAX(id)
	                            FROM kham_suc_khoe_cong_ty tmpKsk
	                            WHERE (tmpKsk.deleted IS NULL OR tmpKsk.deleted = 0)
	                            AND CAST(tmpKsk.ngay_du_kien_kham AS DATE) BETWEEN @FromDate AND @ToDate
                                AND tmpKsk.ma_don_vi = @MaDonVi
                            )",
                        Action = (DbDataReader reader) =>
                        {
                            response.Data.CompanyHealthExamination = new CompanyHealthExaminationModel();
                            while (reader.Read())
                            {
                                response.Data.CompanyHealthExamination.LastDate = reader.GetDateTime(reader.GetOrdinal("LastDate"));
                                response.Data.CompanyHealthExamination.TotalExaminationRecords = reader["SoLuotKham"] as int?;
                                response.Data.CompanyHealthExamination.AbnormalCases = reader["CaBatThuong"] as int?;
                            }
                        }
                    },
                    new DashboardQuery
                    {
                        Sql = @"
                            SELECT
                                plsk.[name] [Name],
                                kskct.ngay_du_kien_kham [Date],
                                COUNT(1) [Count]
                            FROM kham_suc_khoe_ket_luan kskkt
                            JOIN SoKhamSucKhoe sksk ON sksk.id = kskkt.luot_kham
                            JOIN kham_suc_khoe_cong_ty kskct ON kskct.id = sksk.MaDotKham
                            JOIN phan_loai_suc_khoe plsk ON plsk.id = kskkt.phan_loai_suc_khoe
                            WHERE (kskkt.deleted IS NULL OR kskkt.deleted = 0)
                            AND CAST(kskct.ngay_du_kien_kham AS DATE) BETWEEN @FromDate AND @ToDate
                            AND kskct.ma_don_vi = @MaDonVi
                            GROUP BY plsk.[name], kskct.ngay_du_kien_kham",
                        Action = (DbDataReader reader) =>
                        {
                            response.Data.HealthClassifications = [];
                            while (reader.Read())
                            {
                                response.Data.HealthClassifications.Add(new HealthClassificationModel
                                {
                                    Name = reader["Name"] as string ?? string.Empty,
                                    Count = reader["Count"] as int? ?? 0,
                                    Date = reader.GetDateTime(reader.GetOrdinal("Date"))
                                });
                            }
                        }
                    },
                    new DashboardQuery
                    {
                        Sql = @"
                            SELECT TOP 10
                                kskkt.benh_tat_ket_luan [BenhTat],
                                COUNT(1) [Count]
                            FROM kham_suc_khoe_ket_luan kskkt
                            JOIN SoKhamSucKhoe sksk ON sksk.id = kskkt.luot_kham
                            JOIN kham_suc_khoe_cong_ty kskct ON kskct.id = sksk.MaDotKham
                            WHERE (kskkt.deleted IS NULL OR kskkt.deleted = 0)
                            AND kskct.ngay_du_kien_kham BETWEEN @FromDate AND @ToDate
                            AND kskct.ma_don_vi = @MaDonVi
                            GROUP BY kskkt.benh_tat_ket_luan",
                        Action = (DbDataReader reader) =>
                        {
                            response.Data.CommonDiseases = [];
                            while (reader.Read())
                            {
                                response.Data.CommonDiseases.Add(new CommonDiseaseModel
                                {
                                    Name = reader["BenhTat"] as string ?? string.Empty,
                                    Count = reader["Count"] as int? ?? 0
                                });
                            }
                        }
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
                    cmd.Parameters.Add(new SqlParameter("@FromDate", fromDate));
                    cmd.Parameters.Add(new SqlParameter("@ToDate", toDate));
                    cmd.Parameters.Add(new SqlParameter("@MaDonVi", company));
                    using var reader = await cmd.ExecuteReaderAsync();

                    q.Action(reader);
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
