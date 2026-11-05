namespace QuanLyNhanSu.DTO
{
    /// <summary>Dữ liệu 1 dòng chấm công kèm thông tin lương để tính thực lĩnh.</summary>
    public class ChamCongDTO
    {
        public int MaNV { get; set; }
        public string ThangNam { get; set; }
        public double SoNgayCong { get; set; }
        public double SoGioOT { get; set; }
        public decimal LuongCoBan { get; set; }
        public double HeSoLuong { get; set; }
    }
}
