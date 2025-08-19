namespace CoreAdminWeb.Model.Reports
{
    public class ReportBaoCaoTheoDoiDonGiaTheoHopDongModel
    {
        public int Id { get; set; }
        public string? MaDinhMuc { get; set; }
        public string? TenDinhMuc { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGiaDM { get; set; }
        public decimal DonGiaTT { get; set; }
        public decimal ThanhTienTT { get; set; }
        public decimal ThanhTienDM
        {
            get
            {
                return SoLuong * DonGiaDM;
            }
        }
        public decimal ChenhLech
        {
            get
            {
                return ThanhTienTT - ThanhTienDM;
            }
        }
        public float TyLe
        {
            get
            {
                if (ThanhTienDM == 0)
                {
                    return 0;
                }

                return (float)(ChenhLech / ThanhTienDM) * 100;
            }
        }
    }
}
