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
                                (
                                    SELECT
                                        COUNT(1)
                                    FROM kham_suc_khoe_ket_luan kskkl
                                    INNER JOIN SoKhamSucKhoe sksk ON sksk.id = kskkl.luot_kham
                                    WHERE kskkl.isAbnormal = 1 and sksk.MaDotKham = ksk.id
                                ) CaBatThuong
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
	                            (
                                    SELECT
                                        COUNT(1)
                                    FROM kham_suc_khoe_ket_luan kskkl
                                    INNER JOIN SoKhamSucKhoe sksk ON sksk.id = kskkl.luot_kham
                                    WHERE kskkl.isAbnormal = 1 and sksk.MaDotKham = ksk.id
                                ) CaBatThuong
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
                            GROUP BY kskkt.benh_tat_ket_luan
                            ORDER BY COUNT(1) DESC",
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
        [HttpGet("company-summary-report")]
        public async Task<IActionResult> GetCompanySummaryReportData([FromQuery] int? companyHelthCheckId, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var response = new RequestHttpResponse<CompanyReportDashboardModel>();
            try
            {
                response.Data = new CompanyReportDashboardModel();

                var queries = new DashboardQuery[]
                {
                    new DashboardQuery
                    {
                        Sql = @"
                            SELECT
	                            COUNT(1) [Count],
	                            COUNT(CASE WHEN kskct.[status] = 'locked' OR ct.[status] = 'locked' THEN 1 END) [DoneCount],
	                            COUNT(CASE WHEN kskct.[status] <> 'locked' AND ct.[status] <> 'locked' THEN 1 END) [ProcessingCount],
	                            COALESCE((SELECT COUNT(1) FROM SoKhamSucKhoe sksk WHERE sksk.MaDotKham = kskct.id AND sksk.[status] = 'published'),0) [PatientDoneCount],
	                            COALESCE((SELECT COUNT(1) FROM SoKhamSucKhoe sksk WHERE sksk.MaDotKham = kskct.id AND sksk.[status] <> 'published'),0) [PatientProcessingCount],
	                            COALESCE((SELECT SUM(thanh_tien_dm) FROM kham_suc_khoe_dinh_muc_thuc_te kskdm WHERE kskdm.[contract] = ct.id),0) [ChiPhiDuKien],
	                            COALESCE((SELECT SUM(chi_phi_thuc_te) FROM kham_suc_khoe_dinh_muc_thuc_te kskdm WHERE kskdm.[contract] = ct.id),0) [ChiPhiThucTe]
                            FROM kham_suc_khoe_cong_ty kskct
                            INNER JOIN [contract] ct ON ct.id = kskct.ma_hop_dong_ksk
                            WHERE (kskct.deleted IS NULL OR kskct.deleted = 0) AND (ct.deleted IS NULL OR ct.deleted = 0)
                            AND CAST(kskct.ngay_du_kien_kham AS DATE) BETWEEN @FromDate AND @ToDate
                            AND kskct.id = @DoanKhamId
                            GROUP BY kskct.id, ct.id
                        ",
                        Action = (DbDataReader reader) =>
                        {
                            response.Data.Summary = new CompanyReportDashboardSummaryModel();
                            while (reader.Read())
                            {
                                response.Data.Summary.Count = reader["Count"] as int? ?? 0;
                                response.Data.Summary.DoneCount = reader["DoneCount"] as int? ?? 0;
                                response.Data.Summary.ProcessingCount = reader["ProcessingCount"] as int? ?? 0;
                                response.Data.Summary.PatientDoneCount = reader["PatientDoneCount"] as int? ?? 0;
                                response.Data.Summary.PatientProcessingCount = reader["PatientProcessingCount"] as int? ?? 0;
                                response.Data.Summary.ChiPhiDuKien = reader["ChiPhiDuKien"] as decimal? ?? 0;
                                response.Data.Summary.ChiPhiThucTe = reader["ChiPhiThucTe"] as decimal? ?? 0;
                            }
                        }
                    },
                    new DashboardQuery
                    {
                        Sql = @"
                            SELECT
	                            COUNT(CASE WHEN kskct.[status] <> 'locked' AND ct.[status] <> 'locked' THEN 1 END) [Count],
	                            COALESCE((SELECT COUNT(1) FROM SoKhamSucKhoe sksk WHERE sksk.MaDotKham = kskct.id),0) [PatientCount]
                            FROM kham_suc_khoe_cong_ty kskct
                            INNER JOIN [contract] ct ON ct.id = kskct.ma_hop_dong_ksk
                            WHERE (kskct.deleted IS NULL OR kskct.deleted = 0) AND (ct.deleted IS NULL OR ct.deleted = 0)
                            AND CAST(kskct.ngay_du_kien_kham AS DATE) > @ToDate
                            GROUP BY kskct.id, ct.id
                        ",
                        Action = (DbDataReader reader) =>
                        {
                            response.Data.Feature = new CompanyReportDashboardSummaryFeatureModel();
                            while (reader.Read())
                            {
                                response.Data.Feature.Count = reader["Count"] as int? ?? 0;
                                response.Data.Feature.PatientCount = reader["PatientCount"] as int? ?? 0;
                            }
                        }
                    },
                    new DashboardQuery
                    {
                        Sql = @"
                            SELECT
	                            ct.code [MaHopDong],
	                            dmdm.[name] [DinhMuc],
	                            COALESCE(SUM(kskdm.chi_phi_thuc_te), 0) [ChiPhiThucTe],
	                            COALESCE(SUM(kskdm.thanh_tien_dm), 0) [ChiPhiDuKien]
                            FROM kham_suc_khoe_dinh_muc_thuc_te kskdm
                            INNER JOIN [contract] ct ON ct.id = kskdm.[contract]
                            INNER JOIN danh_muc_dinh_muc dmdm ON dmdm.id = kskdm.MaDinhMuc
                            WHERE (kskdm.deleted IS NULL OR kskdm.deleted = 0) AND (ct.deleted IS NULL OR ct.deleted = 0)
                            AND CAST(ct.ngay_hieu_luc AS DATE) BETWEEN @FromDate AND @ToDate
                            GROUP BY ct.code, dmdm.[name]
                        ",
                        Action = (DbDataReader reader) =>
                        {
                            response.Data.Revenues = [];
                            while (reader.Read())
                            {
                                response.Data.Revenues.Add(new CompanyReportDashboardRevenueModel
                                {
                                    MaHopDong = reader["MaHopDong"] as string ?? string.Empty,
                                    DinhMuc = reader["DinhMuc"] as string ?? string.Empty,
                                    ChiPhiThucTe = reader["ChiPhiThucTe"] as decimal? ?? 0,
                                    ChiPhiDuKien = reader["ChiPhiDuKien"] as decimal? ?? 0
                                });
                            }
                        }
                    },
                    new DashboardQuery
                    {
                        Sql = @"
                            SELECT
	                            comp.[name] [MaDonVi],
	                            sksk.ngay_kham [NgayKham],
	                            COUNT(1) [Count]
                            FROM SoKhamSucKhoe sksk
                            INNER JOIN kham_suc_khoe_cong_ty kskct ON kskct.id = sksk.MaDotKham
                            INNER JOIN [contract] ct ON ct.id = kskct.ma_hop_dong_ksk
                            INNER JOIN CongTy comp ON comp.id = ct.cong_ty
                            WHERE (sksk.deleted IS NULL OR sksk.deleted = 0) AND (kskct.deleted IS NULL OR kskct.deleted = 0)
                            AND CAST(sksk.ngay_kham AS DATE) BETWEEN @FromDate AND @ToDate
                            AND kskct.id = @DoanKhamId
                            GROUP BY comp.[name], sksk.ngay_kham
                        ",
                        Action = (DbDataReader reader) =>
                        {
                            response.Data.NoteSummaries = [];
                            while (reader.Read())
                            {
                                response.Data.NoteSummaries.Add(new CompanyReportDashboardNoteSummaryModel
                                {
                                    MaDonVi = reader["MaDonVi"] as string ?? string.Empty,
                                    NgayKham = reader["NgayKham"] as DateTime?,
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
                    cmd.Parameters.Add(new SqlParameter("@DoanKhamId", companyHelthCheckId));
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
