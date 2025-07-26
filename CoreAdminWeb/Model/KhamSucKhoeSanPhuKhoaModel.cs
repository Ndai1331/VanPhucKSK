using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class KhamSucKhoeSanPhuKhoaModel : BaseModel<int>
    {
        public string? ma_luot_kham { get; set; }
        public string? tien_su_san_phu_khoa { get; set; }
        public int? tuoi_bat_dau_kinh { get; set; }
        public string? tinh_chat_kinh { get; set; }
        public string? chu_ky_kinh { get; set; }
        public string? luong_kinh { get; set; }
        public bool? dau_bung_kinh { get; set; } = false;
        public bool? da_lap_gia_dinh { get; set; } = false;
        public string? para { get; set; }
        public int? so_lan_mo_san_phu_khoa { get; set; }
        public string? mo_san_phu_khoa_ghi_ro { get; set; }
        public bool? ap_dung_bptt { get; set; } = false;
        public string? bptt_ghi_ro { get; set; }
        public string? ket_qua { get; set; }
        public string? phan_loai { get; set; }
        public string? nguoi_ket_luan { get; set; }
        public string? chu_ky { get; set; }

        public SoKhamSucKhoeModel? luot_kham { get; set; }
    }
}