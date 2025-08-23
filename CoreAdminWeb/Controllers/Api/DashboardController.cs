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
                            JOIN [contract] ct ON ct.id = ksk.ma_hop_dong_ksk
                            WHERE CAST(ngay_du_kien_kham AS DATE) BETWEEN @FromDate AND @ToDate
                            AND ct.cong_ty = @MaDonVi
                            GROUP BY ksk.id",
                        Action = async (object? obj) =>
                        {
                            if (obj is DbDataReader reader)
                            {
                                response.Data.CompanyExamination = new CompanyExaminationModel();
                                while (await reader.ReadAsync())
                                {
                                    response.Data.CompanyExamination.ToTalExaminations = reader["SoDotKham"] as int? ?? 0;
                                    response.Data.CompanyExamination.TotalExaminationRecords = reader["SoLuotKham"] as int? ?? 0;
                                    response.Data.CompanyExamination.TotalCost = reader["ChiPhi"] as decimal? ?? 0;
                                    response.Data.CompanyExamination.AbnormalCases = reader["CaBatThuong"] as int? ?? 0;
                                }
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
		                            MAX(tmpKsk.id)
	                            FROM kham_suc_khoe_cong_ty tmpKsk
                                JOIN [contract] ct ON ct.id = tmpKsk.ma_hop_dong_ksk
	                            WHERE (tmpKsk.deleted IS NULL OR tmpKsk.deleted = 0)
	                            AND CAST(tmpKsk.ngay_du_kien_kham AS DATE) BETWEEN @FromDate AND @ToDate
                                AND ct.cong_ty = @MaDonVi
                            )",
                        Action = async (object? obj) =>
                        {
                            if (obj is DbDataReader reader)
                            {
                                response.Data.CompanyHealthExamination = new CompanyHealthExaminationModel();
                                while (await reader.ReadAsync())
                                {
                                    response.Data.CompanyHealthExamination.LastDate = reader.GetDateTime(reader.GetOrdinal("LastDate"));
                                    response.Data.CompanyHealthExamination.TotalExaminationRecords = reader["SoLuotKham"] as int?;
                                    response.Data.CompanyHealthExamination.AbnormalCases = reader["CaBatThuong"] as int?;
                                }
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
                            JOIN [contract] ct ON ct.id = kskct.ma_hop_dong_ksk
                            WHERE (kskkt.deleted IS NULL OR kskkt.deleted = 0)
                            AND CAST(kskct.ngay_du_kien_kham AS DATE) BETWEEN @FromDate AND @ToDate
                            AND ct.cong_ty = @MaDonVi
                            GROUP BY plsk.[name], kskct.ngay_du_kien_kham",
                        Action = async (object? obj) =>
                        {
                            if (obj is DbDataReader reader)
                            {
                                response.Data.HealthClassifications = [];
                                while (await reader.ReadAsync())
                                {
                                    response.Data.HealthClassifications.Add(new HealthClassificationModel
                                    {
                                        Name = reader["Name"] as string ?? string.Empty,
                                        Count = reader["Count"] as int? ?? 0,
                                        Date = reader.GetDateTime(reader.GetOrdinal("Date"))
                                    });
                                }
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
                            JOIN [contract] ct ON ct.id = kskct.ma_hop_dong_ksk
                            WHERE (kskkt.deleted IS NULL OR kskkt.deleted = 0)
                            AND kskct.ngay_du_kien_kham BETWEEN @FromDate AND @ToDate
                            AND ct.cong_ty = @MaDonVi
                            GROUP BY kskkt.benh_tat_ket_luan
                            ORDER BY COUNT(1) DESC",
                        Action = async (object? obj) =>
                        {
                            if (obj is DbDataReader reader)
                            {
                                response.Data.CommonDiseases = [];
                                while (await reader.ReadAsync())
                                {
                                    response.Data.CommonDiseases.Add(new CommonDiseaseModel
                                    {
                                        Name = reader["BenhTat"] as string ?? string.Empty,
                                        Count = reader["Count"] as int? ?? 0
                                    });
                                }
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
            var response = new RequestHttpResponse<CompanySummaryReportDashboardModel>();
            try
            {
                response.Data = new CompanySummaryReportDashboardModel();

                var queries = new DashboardQuery[]
                {
                    new DashboardQuery
                    {
                        Sql = @"
                            WITH kskct_f AS (
                                SELECT
                                    kskct.id           AS kskct_id,
                                    kskct.[status]     AS ksk_status,
                                    ct.id              AS contract_id,
                                    ct.[status]        AS ct_status
                                FROM kham_suc_khoe_cong_ty kskct
                                JOIN [contract] ct
                                        ON ct.id = kskct.ma_hop_dong_ksk
                                WHERE (kskct.deleted IS NULL OR kskct.deleted = 0)
                                    AND (ct.deleted   IS NULL OR ct.deleted   = 0)
                                    AND CAST(kskct.ngay_du_kien_kham AS DATE) BETWEEN @FromDate AND @ToDate
                                    AND (kskct.id = @DoanKhamId OR @DoanKhamId IS NULL)
                            ),
                            patient AS (
                                SELECT
                                    MaDotKham,
                                    SUM(CASE WHEN [status] = 'published'  THEN 1 ELSE 0 END) AS published_cnt,
                                    SUM(CASE WHEN [status] <> 'published' THEN 1 ELSE 0 END) AS unpublished_cnt
                                FROM SoKhamSucKhoe
                                GROUP BY MaDotKham
                            ),
                            totals AS (
                                SELECT
                                    COUNT(*) AS [Count],
                                    SUM(CASE WHEN f.ksk_status = 'locked' OR f.ct_status = 'locked' THEN 1 ELSE 0 END) AS [DoneCount],
                                    SUM(CASE WHEN f.ksk_status <> 'locked' AND f.ct_status <> 'locked' THEN 1 ELSE 0 END) AS [ProcessingCount],
                                    COALESCE(SUM(p.published_cnt),   0) AS [PatientDoneCount],
                                    COALESCE(SUM(p.unpublished_cnt), 0) AS [PatientProcessingCount]
                                FROM kskct_f f
                                LEFT JOIN patient p ON p.MaDotKham = f.kskct_id
                            ),
                            contracts AS (
                                SELECT DISTINCT contract_id FROM kskct_f
                            ),
                            cost_per_contract AS (
                                SELECT
                                    d.[contract] AS contract_id,
                                    SUM(CAST(d.thanh_tien_dm    AS decimal(18,2))) AS ChiPhiDuKien,
                                    SUM(CAST(d.chi_phi_thuc_te  AS decimal(18,2))) AS ChiPhiThucTe
                                FROM kham_suc_khoe_dinh_muc_thuc_te d
                                JOIN contracts c ON c.contract_id = d.[contract]
                                GROUP BY d.[contract]
                            ),
                            agg_costs AS (
                                SELECT
                                    SUM(ChiPhiDuKien) AS ChiPhiDuKien,
                                    SUM(ChiPhiThucTe) AS ChiPhiThucTe
                                FROM cost_per_contract
                            )
                            SELECT
                                t.[Count],
                                t.[DoneCount],
                                t.[ProcessingCount],
                                t.[PatientDoneCount],
                                t.[PatientProcessingCount],
                                CAST(COALESCE(ac.ChiPhiDuKien, 0.0) AS decimal(18,2)) AS [ChiPhiDuKien],
                                CAST(COALESCE(ac.ChiPhiThucTe, 0.0) AS decimal(18,2)) AS [ChiPhiThucTe]
                            FROM totals t
                            CROSS JOIN agg_costs ac
                        ",
                        Action = async (object? obj) =>
                        {
                            if (obj is DbDataReader reader)
                            {
                                response.Data.Summary = new CompanySummaryReportDashboardSummaryModel();
                                while (await reader.ReadAsync())
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
                        }
                    },
                    new DashboardQuery
                    {
                        Sql = @"
                            WITH base AS (
                                SELECT kskct.id
                                        , kskct.[status] AS ksk_status
                                        , ct.[status]    AS ct_status
                                FROM kham_suc_khoe_cong_ty kskct
                                JOIN [contract] ct
                                    ON ct.id = kskct.ma_hop_dong_ksk
                                WHERE (kskct.deleted IS NULL OR kskct.deleted = 0)
                                    AND (ct.deleted   IS NULL OR ct.deleted   = 0)
                                    AND kskct.ngay_du_kien_kham >= DATEADD(DAY, 1, @ToDate)
                                    AND (kskct.id = @DoanKhamId OR @DoanKhamId IS NULL)
                            ),
                            totals AS (
                                SELECT
                                    SUM(CASE WHEN ksk_status <> 'locked' AND ct_status <> 'locked' THEN 1 ELSE 0 END) AS [Count]
                                FROM base
                            ),
                            patients AS (
                                SELECT COUNT_BIG(*) AS [PatientCount]
                                FROM SoKhamSucKhoe s
                                JOIN base b ON s.MaDotKham = b.id
                            )
                            SELECT
                                COALESCE(t.[Count], 0)           AS [Count],
                                COALESCE(p.[PatientCount], 0)    AS [PatientCount]
                            FROM totals t
                            CROSS JOIN patients p
                        ",
                        Action = async (object? obj) =>
                        {
                            if (obj is DbDataReader reader)
                            {
                                response.Data.Feature = new CompanySummaryReportDashboardSummaryFeatureModel();
                                while (await reader.ReadAsync())
                                {
                                    response.Data.Feature.Count = reader["Count"] as int? ?? 0;
                                    response.Data.Feature.PatientCount = reader["PatientCount"] as int? ?? 0;
                                }
                            }
                        }
                    },
                    new DashboardQuery
                    {
                        Sql = @"
                            SELECT
	                            ct.code [MaHopDong],
	                            dmdm.[name] [DinhMuc],
	                            CAST(COALESCE(ct.[gia_tri_hop_dong], 0.0) AS decimal) [GiaTriHopDong],
	                            CAST(COALESCE(SUM(kskdm.chi_phi_thuc_te), 0.0) AS decimal) [ChiPhiThucTe],
	                            CAST(COALESCE(SUM(kskdm.thanh_tien_dm), 0.0) AS decimal) [ChiPhiDuKien]
                            FROM kham_suc_khoe_dinh_muc_thuc_te kskdm
                            INNER JOIN [contract] ct ON ct.id = kskdm.[contract]
                            INNER JOIN danh_muc_dinh_muc dmdm ON dmdm.id = kskdm.MaDinhMuc
                            WHERE (kskdm.deleted IS NULL OR kskdm.deleted = 0) AND (ct.deleted IS NULL OR ct.deleted = 0)
                            AND CAST(ct.ngay_hieu_luc AS DATE) BETWEEN @FromDate AND @ToDate
                            AND EXISTS(SELECT Id FROM kham_suc_khoe_cong_ty kskct WHERE kskct.id = @DoanKhamId OR @DoanKhamId IS NULL)
                            GROUP BY ct.code, dmdm.[name], ct.[gia_tri_hop_dong]
                        ",
                        Action = async (object? obj) =>
                        {
                            if (obj is DbDataReader reader)
                            {
                                response.Data.Revenues = [];
                                while (await reader.ReadAsync())
                                {
                                    response.Data.Revenues.Add(new CompanySummaryReportDashboardRevenueModel
                                    {
                                        MaHopDong = reader["MaHopDong"] as string ?? "",
                                        DinhMuc = reader["DinhMuc"] as string ?? "",
                                        GiaTriHopDong = reader["GiaTriHopDong"] as decimal? ?? 0,
                                        ChiPhiThucTe = reader["ChiPhiThucTe"] as decimal? ?? 0,
                                        ChiPhiDuKien = reader["ChiPhiDuKien"] as decimal? ?? 0
                                    });
                                }
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
                            AND (kskct.id = @DoanKhamId OR @DoanKhamId IS NULL)
                            GROUP BY comp.[name], sksk.ngay_kham
                        ",
                        Action = async (object? obj) =>
                        {
                            if (obj is DbDataReader reader)
                            {
                                response.Data.NoteSummaries = [];
                                while (await reader.ReadAsync())
                                {
                                    response.Data.NoteSummaries.Add(new CompanySummaryReportDashboardNoteSummaryModel
                                    {
                                        MaDonVi = reader["MaDonVi"] as string ?? string.Empty,
                                        NgayKham = reader["NgayKham"] as DateTime?,
                                        Count = reader["Count"] as int? ?? 0
                                    });
                                }
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
                    cmd.Parameters.Add(new SqlParameter("@DoanKhamId", companyHelthCheckId ?? (object)DBNull.Value));

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
