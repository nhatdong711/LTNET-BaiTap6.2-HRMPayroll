namespace QuanLyNhanSu.DTO
{
    public class ThongKePhongBanDTO
    {
        public string TenPB { get; set; }
        public int SoNhanVien { get; set; }
        public decimal TongLuong { get; set; }
        public decimal LuongTrungBinh => SoNhanVien == 0 ? 0 : Math.Round(TongLuong / SoNhanVien, 0);
    }
}
