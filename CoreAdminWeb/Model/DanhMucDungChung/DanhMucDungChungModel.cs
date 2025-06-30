using CoreAdminWeb.Enums;
using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class QLCLHinhThucXuPhatModel : BaseModel<int>
    {
        public string? mo_ta{set;get;}
    }
    public class QLCLHinhThucXuPhatCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.active.ToString();
        public string? mo_ta{set;get;}

    }
    public class QLCLHanhViViPhamModel : BaseModel<int>
    {
        public string? muc_phat{set;get;}
    }
    public class QLCLHanhViViPhamCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.active.ToString();
        public string? muc_phat{set;get;}

    }
    public class QLCLLoaiHinhCoSoModel : BaseModel<int>
    {
    }
    public class QLCLLoaiHinhCoSoCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.active.ToString();
    }

    public class QLCLNguyenLieuCheBienModel : BaseModel<int>
    {
    }
    public class QLCLNguyenLieuCheBienCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.active.ToString();
    }


    public class QLCLPhamViHoatDongModel : BaseModel<int>
    {
        public TinhModel? province { get; set; }
        public XaPhuongModel? ward { get; set; }
        public string? khu_vuc_hoat_dong { get; set; }
        public string? thong_tin_vung_nguyen_lieu { get; set; }
        public string? doi_tac_tieu_thu { get; set; }
        public bool? pham_vi_noi_dia { get; set; }
        public bool? pham_vi_xuat_khau { get; set; }
    }
    public class QLCLPhamViHoatDongCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.active.ToString();
        public int? province { get; set; }
        public int? ward { get; set; }
        public string? khu_vuc_hoat_dong { get; set; }
        public string? thong_tin_vung_nguyen_lieu { get; set; }
        public string? doi_tac_tieu_thu { get; set; }
        public bool? pham_vi_noi_dia { get; set; }
        public bool? pham_vi_xuat_khau { get; set; }
    }
    

    public class CountryModel : BaseModel<int>
    {
    }
    public class CountryCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.active.ToString();
    }
    public class QLCLLoaiHinhKinhDoanhModel : BaseModel<int>
    {
    }
    public class QLCLLoaiHinhKinhDoanhCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.active.ToString();
    }


    public class DonViTinhModel : BaseModel<int>
    {
    }
    public class DonViTinhCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.active.ToString();

    }

    public class TinhModel : BaseModel<int>
    {
    }
    public class TinhCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.active.ToString();

    }
    public class XaPhuongModel : BaseModel<int>
    {
        public int? ProvinceId { get; set; }
    }
    public class XaPhuongCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.active.ToString();
    }


    public class QLCLLoaiSanPhamModel : BaseModel<int>
    {
        public string? english_name { set; get; }
    }
    public class QLCLLoaiSanPhamCRUDModel : BaseDetailModel
    {
        public string? english_name { set; get; }
        public new string status { set; get; } = Status.active.ToString();
    }

    public class QLCLSanPhamSanXuatModel : BaseModel<int>
    {
        public QLCLLoaiSanPhamModel? loai_sp { set; get; }
        public string? english_name { set; get; }
        public string? tieu_chuan_chat_luong { set; get; }
        public string? tieu_chuan_kiem_dich { set; get; }
    }
    public class QLCLSanPhamSanXuatCRUDModel : BaseDetailModel
    {
        public int? loai_sp { set; get; }
        public string? english_name { set; get; }
        public string? tieu_chuan_chat_luong { set; get; }
        public string? tieu_chuan_kiem_dich { set; get; }
        public new string status { set; get; } = Status.active.ToString();
    }


    public class QLCLLoaiVatTuModel : BaseModel<int>
    {
    }
    public class QLCLLoaiVatTuCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.active.ToString();
    }

    public class QLCLVatTuNongNghiepModel : BaseModel<int>
    {
        public QLCLLoaiVatTuModel? loai_vat_tu { set; get; }
        public DonViTinhModel? don_vi_tinh { set; get; }
        public string? nha_san_xuat { set; get; }
        public string? tieu_chuan_chat_luong { set; get; }
    }
    public class QLCLVatTuNongNghiepCRUDModel : BaseDetailModel
    {
        public int? loai_vat_tu { set; get; }
        public int? don_vi_tinh { set; get; }
        public string? nha_san_xuat { set; get; }
        public string? tieu_chuan_chat_luong { set; get; }
        public new string status { set; get; } = Status.active.ToString();
    }




}