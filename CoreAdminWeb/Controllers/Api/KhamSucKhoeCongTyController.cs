
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.Base;
using CoreAdminWeb.Model.KhamSucKhoes;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model.User;
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
public class KhamSucKhoeCongTyController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public KhamSucKhoeCongTyController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("get-list")]
    public async Task<IActionResult> GetData([FromQuery] int? id,
                                             [FromQuery] string? searchText,
                                             [FromQuery] string? status,
                                             [FromQuery] int offset = 0,
                                             [FromQuery] int limit = 10)
    {

        var response = new RequestHttpResponse<List<KhamSucKhoeCongTyModel>>();
        try
        {
            // Validate parameters
            var validLimit = limit <= 0 ? 10 : limit;
            var validOffset = offset < 0 ? 0 : offset;

            var where = @" WHERE (k.deleted IS NULL OR k.deleted = 0)
AND (c.deleted IS NULL OR c.deleted = 0)
AND (ct.deleted IS NULL OR ct.deleted = 0)";

            if (id.HasValue)
            {
                where += " AND ksk.id = @id";
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                where += @" AND (
c.code = @searchText
OR c.name like N'%' + @searchText + '%'
OR ct.code = @searchText
OR ct.name like N'%' + @searchText + '%'
OR k.code = @searchText)";
            }

            if (!string.IsNullOrEmpty(status))
            {
                where += @" AND (k.status = @status)";
            }

            // Query đếm tổng số bản ghi
            var countSql = @" COUNT(*) as TotalCount ";

            // Query lấy dữ liệu với phân trang
            var fieldSql = @" k.[id]
    ,k.[status]
    ,k.[sort]
    ,k.[user_created]
    ,k.[date_created]
    ,k.[user_updated]
    ,k.[date_updated]
    ,k.[so_luong_du_kien]
    ,k.[so_luong_thuc_te]
    ,k.[ngay_du_kien_kham]
    ,k.[ngay_ket_thuc]
    ,k.[description]
    ,k.[Ksk_status]
    ,k.[ma_hop_dong_ksk]
    ,k.[bs_tuan_hoan]
    ,k.[bs_san_phu_khoa]
    ,k.[bs_ket_luan]
    ,k.[bs_rang_ham_mat]
    ,k.[bs_tai_mui_hong]
    ,k.[bs_mat]
    ,k.[bs_ho_hap]
    ,k.[bs_tieu_hoa]
    ,k.[bs_noi_tiet]
    ,k.[bs_than_tiet_nieu]
    ,k.[bs_co_xuong_khop]
    ,k.[bs_than_kinh]
    ,k.[bs_tam_than]
    ,k.[bs_ngoai_khoa]
    ,k.[code]
    ,k.[deleted]
    ,k.[nguoi_lap_so]
    ,k.[ma_don_vi]
    ,k.[ma_bs_tuan_hoan]
    ,k.[ma_bs_san_phu_khoa]
    ,k.[ma_bs_ket_luan]
    ,k.[ma_bs_rang_ham_mat]
    ,k.[ma_bs_tai_mui_hong]
    ,k.[ma_bs_mat]
    ,k.[ma_bs_ho_hap]
    ,k.[ma_bs_tieu_hoa]
    ,k.[ma_bs_noi_tiet]
    ,k.[ma_bs_than_tiet_nieu]
    ,k.[ma_bs_co_xuong_khop]
    ,k.[ma_bs_than_kinh]
    ,k.[ma_bs_tam_than]
    ,k.[ma_bs_ngoai_khoa],

    uc.first_name AS user_created_first_name,
    uc.last_name  AS user_created_last_name,
    uu.first_name AS user_updated_first_name,
    uu.last_name  AS user_updated_last_name,

    c.id   AS ma_hop_dong_ksk_id,
    c.code AS ma_hop_dong_ksk_code,
    c.name AS ma_hop_dong_ksk_name,
    ct.id  AS ma_hop_dong_ksk_cong_ty_id,
    ct.code AS ma_hop_dong_ksk_cong_ty_code,
    ct.name AS ma_hop_dong_ksk_cong_ty_name,

    nls.id           AS nguoi_lap_so_id,
    nls.ma_tai_khoan AS nguoi_lap_so_ma_tai_khoan,
    nls.first_name   AS nguoi_lap_so_first_name,
    nls.last_name    AS nguoi_lap_so_last_name,

    COALESCE(bstt_code.id,          bstt_guid.id)          AS bs_tuan_hoan_id,
    COALESCE(bstt_code.ma_tai_khoan,  bstt_guid.ma_tai_khoan)  AS bs_tuan_hoan_ma_tai_khoan,
    COALESCE(bstt_code.first_name,  bstt_guid.first_name)  AS bs_tuan_hoan_first_name,
    COALESCE(bstt_code.last_name,   bstt_guid.last_name)   AS bs_tuan_hoan_last_name,

    COALESCE(bshh_code.id,          bshh_guid.id)          AS bs_ho_hap_id,
    COALESCE(bshh_code.ma_tai_khoan,bshh_guid.ma_tai_khoan)AS bs_ho_hap_ma_tai_khoan,
    COALESCE(bshh_code.first_name,  bshh_guid.first_name)  AS bs_ho_hap_first_name,
    COALESCE(bshh_code.last_name,   bshh_guid.last_name)   AS bs_ho_hap_last_name,

    COALESCE(bsth_code.id,          bsth_guid.id)          AS bs_tieu_hoa_id,
    COALESCE(bsth_code.ma_tai_khoan,bsth_guid.ma_tai_khoan)AS bs_tieu_hoa_ma_tai_khoan,
    COALESCE(bsth_code.first_name,  bsth_guid.first_name)  AS bs_tieu_hoa_first_name,
    COALESCE(bsth_code.last_name,   bsth_guid.last_name)   AS bs_tieu_hoa_last_name,

    COALESCE(bsttn_code.id,          bsttn_guid.id)          AS bs_than_tiet_nieu_id,
    COALESCE(bsttn_code.ma_tai_khoan,bsttn_guid.ma_tai_khoan)AS bs_than_tiet_nieu_ma_tai_khoan,
    COALESCE(bsttn_code.first_name,  bsttn_guid.first_name)  AS bs_than_tiet_nieu_first_name,
    COALESCE(bsttn_code.last_name,   bsttn_guid.last_name)   AS bs_than_tiet_nieu_last_name,

    COALESCE(bsnt_code.id,          bsnt_guid.id)          AS bs_noi_tiet_id,
    COALESCE(bsnt_code.ma_tai_khoan,bsnt_guid.ma_tai_khoan)AS bs_noi_tiet_ma_tai_khoan,
    COALESCE(bsnt_code.first_name,  bsnt_guid.first_name)  AS bs_noi_tiet_first_name,
    COALESCE(bsnt_code.last_name,   bsnt_guid.last_name)   AS bs_noi_tiet_last_name,

    COALESCE(bscxk_code.id,          bscxk_guid.id)          AS bs_co_xuong_khop_id,
    COALESCE(bscxk_code.ma_tai_khoan,bscxk_guid.ma_tai_khoan)AS bs_co_xuong_khop_ma_tai_khoan,
    COALESCE(bscxk_code.first_name,  bscxk_guid.first_name)  AS bs_co_xuong_khop_first_name,
    COALESCE(bscxk_code.last_name,   bscxk_guid.last_name)   AS bs_co_xuong_khop_last_name,

    COALESCE(bstk_code.id,          bstk_guid.id)          AS bs_than_kinh_id,
    COALESCE(bstk_code.ma_tai_khoan,bstk_guid.ma_tai_khoan)AS bs_than_kinh_ma_tai_khoan,
    COALESCE(bstk_code.first_name,  bstk_guid.first_name)  AS bs_than_kinh_first_name,
    COALESCE(bstk_code.last_name,   bstk_guid.last_name)   AS bs_than_kinh_last_name,

    COALESCE(bstt2_code.id,          bstt2_guid.id)          AS bs_tam_than_id,
    COALESCE(bstt2_code.ma_tai_khoan,bstt2_guid.ma_tai_khoan)AS bs_tam_than_ma_tai_khoan,
    COALESCE(bstt2_code.first_name,  bstt2_guid.first_name)  AS bs_tam_than_first_name,
    COALESCE(bstt2_code.last_name,   bstt2_guid.last_name)   AS bs_tam_than_last_name,

    COALESCE(bsnk_code.id,          bsnk_guid.id)          AS bs_ngoai_khoa_id,
    COALESCE(bsnk_code.ma_tai_khoan,bsnk_guid.ma_tai_khoan)AS bs_ngoai_khoa_ma_tai_khoan,
    COALESCE(bsnk_code.first_name,  bsnk_guid.first_name)  AS bs_ngoai_khoa_first_name,
    COALESCE(bsnk_code.last_name,   bsnk_guid.last_name)   AS bs_ngoai_khoa_last_name,

    COALESCE(bsm_code.id,          bsm_guid.id)          AS bs_mat_id,
    COALESCE(bsm_code.ma_tai_khoan,bsm_guid.ma_tai_khoan)AS bs_mat_ma_tai_khoan,
    COALESCE(bsm_code.first_name,  bsm_guid.first_name)  AS bs_mat_first_name,
    COALESCE(bsm_code.last_name,   bsm_guid.last_name)   AS bs_mat_last_name,

    COALESCE(bstmh_code.id,          bstmh_guid.id)          AS bs_tai_mui_hong_id,
    COALESCE(bstmh_code.ma_tai_khoan,bstmh_guid.ma_tai_khoan)AS bs_tai_mui_hong_ma_tai_khoan,
    COALESCE(bstmh_code.first_name,  bstmh_guid.first_name)  AS bs_tai_mui_hong_first_name,
    COALESCE(bstmh_code.last_name,   bstmh_guid.last_name)   AS bs_tai_mui_hong_last_name,

    COALESCE(bsrhm_code.id,          bsrhm_guid.id)          AS bs_rang_ham_mat_id,
    COALESCE(bsrhm_code.ma_tai_khoan,bsrhm_guid.ma_tai_khoan)AS bs_rang_ham_mat_ma_tai_khoan,
    COALESCE(bsrhm_code.first_name,  bsrhm_guid.first_name)  AS bs_rang_ham_mat_first_name,
    COALESCE(bsrhm_code.last_name,   bsrhm_guid.last_name)   AS bs_rang_ham_mat_last_name,

    COALESCE(bsspk_code.id,          bsspk_guid.id)          AS bs_san_phu_khoa_id,
    COALESCE(bsspk_code.ma_tai_khoan,bsspk_guid.ma_tai_khoan)AS bs_san_phu_khoa_ma_tai_khoan,
    COALESCE(bsspk_code.first_name,  bsspk_guid.first_name)  AS bs_san_phu_khoa_first_name,
    COALESCE(bsspk_code.last_name,   bsspk_guid.last_name)   AS bs_san_phu_khoa_last_name,

    COALESCE(bskl_code.id,          bskl_guid.id)          AS bs_ket_luan_id,
    COALESCE(bskl_code.ma_tai_khoan,bskl_guid.ma_tai_khoan)AS bs_ket_luan_ma_tai_khoan,
    COALESCE(bskl_code.first_name,  bskl_guid.first_name)  AS bs_ket_luan_first_name,
    COALESCE(bskl_code.last_name,   bskl_guid.last_name)   AS bs_ket_luan_last_name ";
            var cteSQL = @";WITH U AS (
    SELECT id, ma_tai_khoan, first_name, last_name, chu_ky_bac_si
    FROM dbo.custom_users
)
SELECT";
            var joinSQL = @"FROM dbo.kham_suc_khoe_cong_ty k
LEFT JOIN U uc  ON uc.id = k.user_created
LEFT JOIN U uu  ON uu.id = k.user_updated
LEFT JOIN U nls ON nls.id = k.nguoi_lap_so
LEFT JOIN dbo.contract c ON c.id = k.ma_hop_dong_ksk
LEFT JOIN dbo.CongTy  ct ON ct.id = c.cong_ty
LEFT JOIN U bstt_code ON bstt_code.ma_tai_khoan = k.ma_bs_tuan_hoan
LEFT JOIN U bstt_guid ON k.ma_bs_tuan_hoan IS NULL AND bstt_guid.id = k.bs_tuan_hoan
LEFT JOIN U bshh_code ON bshh_code.ma_tai_khoan = k.ma_bs_ho_hap
LEFT JOIN U bshh_guid ON k.ma_bs_ho_hap IS NULL AND bshh_guid.id = k.bs_ho_hap
LEFT JOIN U bsth_code ON bsth_code.ma_tai_khoan = k.ma_bs_tieu_hoa
LEFT JOIN U bsth_guid ON k.ma_bs_tieu_hoa IS NULL AND bsth_guid.id = k.bs_tieu_hoa
LEFT JOIN U bsttn_code ON bsttn_code.ma_tai_khoan = k.ma_bs_than_tiet_nieu
LEFT JOIN U bsttn_guid ON k.ma_bs_than_tiet_nieu IS NULL AND bsttn_guid.id = k.bs_than_tiet_nieu
LEFT JOIN U bsnt_code ON bsnt_code.ma_tai_khoan = k.ma_bs_noi_tiet
LEFT JOIN U bsnt_guid ON k.ma_bs_noi_tiet IS NULL AND bsnt_guid.id = k.bs_noi_tiet
LEFT JOIN U bscxk_code ON bscxk_code.ma_tai_khoan = k.ma_bs_co_xuong_khop
LEFT JOIN U bscxk_guid ON k.ma_bs_co_xuong_khop IS NULL AND bscxk_guid.id = k.bs_co_xuong_khop
LEFT JOIN U bstk_code ON bstk_code.ma_tai_khoan = k.ma_bs_than_kinh
LEFT JOIN U bstk_guid ON k.ma_bs_than_kinh IS NULL AND bstk_guid.id = k.bs_than_kinh
LEFT JOIN U bstt2_code ON bstt2_code.ma_tai_khoan = k.ma_bs_tam_than
LEFT JOIN U bstt2_guid ON k.ma_bs_tam_than IS NULL AND bstt2_guid.id = k.bs_tam_than
LEFT JOIN U bsnk_code ON bsnk_code.ma_tai_khoan = k.ma_bs_ngoai_khoa
LEFT JOIN U bsnk_guid ON k.ma_bs_ngoai_khoa IS NULL AND bsnk_guid.id = k.bs_ngoai_khoa
LEFT JOIN U bsm_code ON bsm_code.ma_tai_khoan = k.ma_bs_mat
LEFT JOIN U bsm_guid ON k.ma_bs_mat IS NULL AND bsm_guid.id = k.bs_mat
LEFT JOIN U bstmh_code ON bstmh_code.ma_tai_khoan = k.ma_bs_tai_mui_hong
LEFT JOIN U bstmh_guid ON k.ma_bs_tai_mui_hong IS NULL AND bstmh_guid.id = k.bs_tai_mui_hong
LEFT JOIN U bsrhm_code ON bsrhm_code.ma_tai_khoan = k.ma_bs_rang_ham_mat
LEFT JOIN U bsrhm_guid ON k.ma_bs_rang_ham_mat IS NULL AND bsrhm_guid.id = k.bs_rang_ham_mat
LEFT JOIN U bsspk_code ON bsspk_code.ma_tai_khoan = k.ma_bs_san_phu_khoa
LEFT JOIN U bsspk_guid ON k.ma_bs_san_phu_khoa IS NULL AND bsspk_guid.id = k.bs_san_phu_khoa
LEFT JOIN U bskl_code ON bskl_code.ma_tai_khoan = k.ma_bs_ket_luan
LEFT JOIN U bskl_guid ON k.ma_bs_ket_luan IS NULL AND bskl_guid.id = k.bs_ket_luan";
            var pagingSQL = @"
            ORDER BY k.id
            OFFSET @offset ROWS 
            FETCH NEXT @limit ROWS ONLY";
            var results = new List<KhamSucKhoeCongTyModel>();
            int totalCount = 0;

            await _context.Database.OpenConnectionAsync();

            // Lấy tổng số bản ghi
            using (var countCommand = _context.Database.GetDbConnection().CreateCommand())
            {
                countCommand.CommandText = cteSQL + countSql + joinSQL + where;
                if (id.HasValue && id > 0)
                {
                    countCommand.Parameters.Add(new SqlParameter("@id", id));
                }
                if (!string.IsNullOrEmpty(searchText))
                {
                    countCommand.Parameters.Add(new SqlParameter("@searchText", searchText));
                }
                if (!string.IsNullOrEmpty(status))
                {
                    countCommand.Parameters.Add(new SqlParameter("@status", status));
                }
                var countResult = await countCommand.ExecuteScalarAsync();
                totalCount = Convert.ToInt32(countResult ?? 0);
            }

            // Lấy dữ liệu với phân trang
            using (var dataCommand = _context.Database.GetDbConnection().CreateCommand())
            {
                dataCommand.CommandText = cteSQL + fieldSql + joinSQL + where + pagingSQL;
                if (id.HasValue && id > 0)
                {
                    dataCommand.Parameters.Add(new SqlParameter("@id", id));
                }
                if (!string.IsNullOrEmpty(searchText))
                {
                    dataCommand.Parameters.Add(new SqlParameter("@searchText", searchText));
                }
                if (!string.IsNullOrEmpty(status))
                {
                    dataCommand.Parameters.Add(new SqlParameter("@status", status));
                }
                dataCommand.Parameters.Add(new SqlParameter("@offset", validOffset));
                dataCommand.Parameters.Add(new SqlParameter("@limit", validLimit));

                using (var reader = await dataCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var item = new KhamSucKhoeCongTyModel
                        {
                            id = reader["id"] as int? ?? 0,
                            bs_co_xuong_khop = MapUserOrNull(reader, "bs_co_xuong_khop"),
                            bs_ho_hap = MapUserOrNull(reader, "bs_ho_hap"),
                            bs_ket_luan = MapUserOrNull(reader, "bs_ket_luan"),
                            bs_mat = MapUserOrNull(reader, "bs_mat"),
                            bs_ngoai_khoa = MapUserOrNull(reader, "bs_ngoai_khoa"),
                            bs_rang_ham_mat = MapUserOrNull(reader, "bs_rang_ham_mat"),
                            bs_san_phu_khoa = MapUserOrNull(reader, "bs_san_phu_khoa"),
                            bs_tam_than = MapUserOrNull(reader, "bs_tam_than"),
                            bs_than_kinh = MapUserOrNull(reader, "bs_than_kinh"),
                            bs_than_tiet_nieu = MapUserOrNull(reader, "bs_than_tiet_nieu"),
                            bs_tieu_hoa = MapUserOrNull(reader, "bs_tieu_hoa"),
                            bs_tuan_hoan = MapUserOrNull(reader, "bs_tuan_hoan"),
                            code = reader["code"]?.ToString(),
                            deleted = reader["deleted"] as bool? ?? false,
                            description = reader["description"]?.ToString(),
                            Ksk_status = reader["Ksk_status"]?.ToString(),
                            ma_hop_dong_ksk = new Model.Contract.ContractModel()
                            {
                                id = reader["ma_hop_dong_ksk_id"] as int? ?? 0,
                                code = reader["ma_hop_dong_ksk_code"]?.ToString(),
                                name = reader["ma_hop_dong_ksk_name"]?.ToString(),
                                cong_ty = new CongTyModel()
                                {
                                    id = reader["ma_hop_dong_ksk_cong_ty_id"] as int? ?? 0,
                                    code = reader["ma_hop_dong_ksk_cong_ty_code"]?.ToString(),
                                    name = reader["ma_hop_dong_ksk_cong_ty_name"]?.ToString()
                                }
                            },
                            ma_don_vi = reader["ma_don_vi"]?.ToString(),
                            ngay_du_kien_kham = reader["ngay_du_kien_kham"] as DateTime?,
                            ngay_ket_thuc = reader["ngay_ket_thuc"] as DateTime?,
                            nguoi_lap_so = MapUserOrNull(reader, "nguoi_lap_so"),
                            so_luong_du_kien = reader["so_luong_du_kien"] as int?,
                            so_luong_thuc_te = reader["so_luong_thuc_te"] as int?,
                            user_created = new UserModel
                            {
                                id = reader["user_created"] as Guid? ?? Guid.Empty,
                                first_name = reader["user_created_first_name"]?.ToString() ?? string.Empty,
                                last_name = reader["user_created_last_name"]?.ToString() ?? string.Empty
                            },
                            date_created = reader["date_created"] as DateTime? ?? DateTime.Now,
                            user_updated = new UserModel
                            {
                                id = reader["user_updated"] as Guid? ?? Guid.Empty,
                                first_name = reader["user_updated_first_name"]?.ToString() ?? string.Empty,
                                last_name = reader["user_updated_last_name"]?.ToString() ?? string.Empty
                            },
                            date_updated = reader["date_updated"] as DateTime?,
                            status = ReadStatus(reader["status"]),
                            bs_noi_tiet = MapUserOrNull(reader, "bs_noi_tiet"),
                            bs_tai_mui_hong = MapUserOrNull(reader, "bs_tai_mui_hong"),
                            ma_bs_co_xuong_khop = reader["ma_bs_co_xuong_khop"]?.ToString(),
                            ma_bs_ho_hap = reader["ma_bs_ho_hap"]?.ToString(),
                            ma_bs_ket_luan = reader["ma_bs_ket_luan"]?.ToString(),
                            ma_bs_mat = reader["ma_bs_mat"]?.ToString(),
                            ma_bs_ngoai_khoa = reader["ma_bs_ngoai_khoa"]?.ToString(),
                            ma_bs_rang_ham_mat = reader["ma_bs_rang_ham_mat"]?.ToString(),
                            ma_bs_san_phu_khoa = reader["ma_bs_san_phu_khoa"]?.ToString(),
                            ma_bs_tam_than = reader["ma_bs_tam_than"]?.ToString(),
                            ma_bs_than_kinh = reader["ma_bs_than_kinh"]?.ToString(),
                            ma_bs_than_tiet_nieu = reader["ma_bs_than_tiet_nieu"]?.ToString(),
                            ma_bs_tieu_hoa = reader["ma_bs_tieu_hoa"]?.ToString(),
                            ma_bs_tuan_hoan = reader["ma_bs_tuan_hoan"]?.ToString(),
                            ma_bs_noi_tiet = reader["ma_bs_noi_tiet"]?.ToString(),
                            ma_bs_tai_mui_hong = reader["ma_bs_tai_mui_hong"]?.ToString(),
                            sort = reader["sort"] as int? ?? 0
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

    static Status ReadStatus(object val)
    {
        if (val == null || val == DBNull.Value)
        {
            return Status.published;
        }

        if (val is int i)
        {
            return (Status)i;
        }

        var s = val.ToString()?.Trim();

        if (int.TryParse(s, out var n))
        {
            return (Status)n;
        }

        if (Enum.TryParse<Status>(s, true, out var byName))
        {
            return byName;
        }

        // map tay nếu tên enum khác string trong DB
        switch (s?.ToLowerInvariant())
        {
            case "published": return Status.published;
            case "draft": return Status.draft;
            case "removed": return Status.removed;
            default: return Status.published;
        }
    }

    static bool TryGetOrdinal(IDataRecord r, string name, out int ordinal)
    {
        try { ordinal = r.GetOrdinal(name); return true; }
        catch (IndexOutOfRangeException) { ordinal = -1; return false; }
    }

    static string ReadString(IDataRecord r, string name)
    {
        return TryGetOrdinal(r, name, out var i) && i >= 0 && !r.IsDBNull(i) ? r.GetString(i) : string.Empty;
    }

    static Guid? ReadGuidNullable(IDataRecord r, string name)
    {
        if (!TryGetOrdinal(r, name, out var i) || i < 0 || r.IsDBNull(i))
        {
            return null;
        }

        var v = r.GetValue(i);
        if (v is Guid g)
        {
            return g;
        }

        if (v is string s && Guid.TryParse(s, out var g2))
        {
            return g2;
        }

        return null;
    }

    static UserModel? MapUserOrNull(IDataRecord r, string prefix)
    {
        var id = ReadGuidNullable(r, $"{prefix}_id");
        if (id == null || id == Guid.Empty)
        {
            return null;
        }

        return new UserModel
        {
            id = id.Value,
            ma_tai_khoan = ReadString(r, $"{prefix}_ma_tai_khoan"),   // nếu cột này không có trong SELECT thì ReadString trả ""
            first_name = ReadString(r, $"{prefix}_first_name"),
            last_name = ReadString(r, $"{prefix}_last_name")
        };
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