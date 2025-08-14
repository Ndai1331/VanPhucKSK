namespace CoreAdminWeb.Model.DanhSachDoan
{
    public class ImportDoanKhamModel
    {
        public string MaLuotKham { get; set; } = string.Empty;
        public string SoThuTu { get; set; } = string.Empty;
        public string MaBenhNhan { get; set; } = string.Empty;
        public string? TenBenhNhan { get; set; }
        public string? GioiTinh { get; set; }
        public string? NgaySinh { get; set; }
        public string? SoDienThoai { get; set; }
        public string? CCCD { get; set; }
        public string? NoiCap { get; set; }
        public string? MaXa { get; set; }
        public string? MaTinh { get; set; }
        public string? DiaChi { get; set; }
        public string? Email { get; set; }
    }
}
