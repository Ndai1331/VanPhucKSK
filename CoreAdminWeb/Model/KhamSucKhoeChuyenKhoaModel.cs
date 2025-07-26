using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class KhamSucKhoeChuyenKhoaModel : BaseModel<int>
    {
        public string? ma_luot_kham { get; set; }
        public string? kq_nk_tuan_hoan { get; set; }
        public PhanLoaiSucKhoeModel? pl_nk_tuan_hoan { get; set; }
        public string? chu_ky_tuan_hoan { get; set; }
        public string? bs_tuan_hoan { get; set; }

        public string? kq_nk_ho_hap { get; set; }
        public PhanLoaiSucKhoeModel? pl_nk_ho_hap { get; set; }
        public string? chu_ky_ho_hap { get; set; }
        public string? bs_ho_hap { get; set; }

        public string? kq_nk_tieu_hoa { get; set; }
        public PhanLoaiSucKhoeModel? pl_nk_tieu_hoa { get; set; }
        public string? chu_ky_tieu_hoa { get; set; }
        public string? bs_tieu_hoa { get; set; }

        public string? kq_nk_than_tiet_nieu { get; set; }
        public PhanLoaiSucKhoeModel? pl_nk_than_tiet_nieu { get; set; }
        public string? chu_ky_than_tiet_nieu { get; set; }
        public string? bs_than_tiet_nieu { get; set; }

        public string? kq_nk_noi_tiet { get; set; }
        public PhanLoaiSucKhoeModel? pl_nk_noi_tiet { get; set; }
        public string? chu_ky_noi_tiet { get; set; }
        public string? bs_noi_tiet { get; set; }

        public string? kq_nk_co_xuong_khop { get; set; }
        public PhanLoaiSucKhoeModel? pl_nk_co_xuong_khop { get; set; }
        public string? chu_ky_co_xuong_khop { get; set; }
        public string? bs_co_xuong_khop { get; set; }

        public string? kq_nk_than_kinh { get; set; }
        public PhanLoaiSucKhoeModel? pl_nk_than_kinh { get; set; }
        public string? chu_ky_than_kinh { get; set; }
        public string? bs_than_kinh { get; set; }

        public string? kq_nk_tam_than { get; set; }
        public PhanLoaiSucKhoeModel? pl_nk_tam_than { get; set; }
        public string? chu_ky_tam_than { get; set; }
        public string? bs_tam_than { get; set; }

        public string? kq_ngoai_khoa { get; set; }
        public PhanLoaiSucKhoeModel? pl_ngoai_khoa { get; set; }
        public string? kq_da_lieu { get; set; }
        public PhanLoaiSucKhoeModel? pl_da_lieu { get; set; }
        public string? chu_ky_ngoai_khoa { get; set; }
        public string? bs_ngoai_khoa { get; set; }

        public string? thi_luc_khong_kinh_phai { get; set; }
        public string? thi_luc_khong_kinh_trai { get; set; }
        public string? thi_luc_co_kinh_phai { get; set; }
        public string? thi_luc_co_kinh_trai { get; set; }

        public string? benh_mat { get; set; }
        public PhanLoaiSucKhoeModel? pl_mat { get; set; }
        public string? bs_mat { get; set; }
        public string? chu_ky_mat { get; set; }

        public string? tmh_nt_trai { get; set; }
        public string? tmh_ntham_trai { get; set; }
        public string? tmh_nt_phai { get; set; }
        public string? tmh_ntham_phai { get; set; }

        public string? benh_tai_mui_hong { get; set; }
        public PhanLoaiSucKhoeModel? pl_tmh { get; set; }
        public string? bs_tmh { get; set; }
        public string? chu_ky_tmh { get; set; }

        public string? kq_rhm_ham_tren { get; set; }
        public string? kq_rhm_ham_duoi { get; set; }
        public string? benh_rhm { get; set; }
        public PhanLoaiSucKhoeModel? pl_rhm { get; set; }
        public string? bs_rhm { get; set; }
        public string? chu_ky_rhm { get; set; }
        public string? bs_ket_luan { get; set; }
        public string? chu_ky_ket_luan { get; set; }
        public new bool deleted { get; set; } = false;

        public SoKhamSucKhoeModel? luot_kham { get; set; }
    }

}