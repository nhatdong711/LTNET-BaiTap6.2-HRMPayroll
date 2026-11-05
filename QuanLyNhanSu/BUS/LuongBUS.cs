using System.Data;
using QuanLyNhanSu.DAL;
using QuanLyNhanSu.DTO;

namespace QuanLyNhanSu.BUS
{
    public class LuongBUS
    {
        public const decimal DonGiaOT = 50000m;
        public const int NgayCongChuan = 22;

        private readonly LuongDAL dal = new();
        private readonly ChamCongDAL ccDal = new();

        /// <summary>ThucLinh = (LuongCoBan * HeSoLuong / 22) * SoNgayCong + SoGioOT * 50.000</summary>
        public static decimal TinhThucLinh(decimal luongCoBan, double heSo, double soNgayCong, double soGioOT)
        {
            decimal tl = luongCoBan * (decimal)heSo / NgayCongChuan * (decimal)soNgayCong
                       + (decimal)soGioOT * DonGiaOT;
            return Math.Round(tl, 0);
        }

        /// <summary>Tính lương từ dữ liệu chấm công ĐÃ LƯU rồi chốt trong 1 transaction.</summary>
        public int ChotLuong(string thangNam)
        {
            List<ChamCongDTO> ds = ccDal.LayDuLieuTinhLuong(thangNam);
            if (ds.Count == 0)
                throw new ArgumentException($"Tháng {thangNam} chưa có dữ liệu chấm công. Hãy nhập và bấm \"Lưu chấm công\" trước.");

            var dsLuong = ds.Select(c => new BangLuongDTO
            {
                MaNV = c.MaNV,
                ThangNam = thangNam,
                ThucLinh = TinhThucLinh(c.LuongCoBan, c.HeSoLuong, c.SoNgayCong, c.SoGioOT)
            }).ToList();
            return dal.ChotLuong(thangNam, dsLuong);
        }

        public DataTable LayBangLuong(string thangNam) => dal.LayBangLuong(thangNam);
        public decimal TongQuyLuong(string thangNam) => dal.TongQuyLuong(thangNam);
        public List<ThongKePhongBanDTO> ThongKeTheoPhongBan(string thangNam) => dal.ThongKeTheoPhongBan(thangNam);
        public List<string> CacThangDaChot() => dal.CacThangDaChot();
    }
}
