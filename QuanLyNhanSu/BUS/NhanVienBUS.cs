using System.Data;
using System.Text.RegularExpressions;
using QuanLyNhanSu.DAL;
using QuanLyNhanSu.DTO;

namespace QuanLyNhanSu.BUS
{
    public class NhanVienBUS
    {
        private readonly NhanVienDAL dal = new();
        private readonly PhongBanDAL pbDal = new();

        public DataTable LayPhongBan() => pbDal.LayTatCa();
        public DataTable TimKiem(string tuKhoa, int maPB) => dal.TimKiem(tuKhoa?.Trim() ?? "", maPB);
        public int DemNhanVien() => dal.DemNhanVien();

        public bool Them(NhanVienDTO nv) { KiemTra(nv); return dal.Them(nv); }
        public bool Sua(NhanVienDTO nv) { KiemTra(nv); return dal.Sua(nv); }
        public bool Xoa(int maNV) => dal.Xoa(maNV);

        public bool ChuyenPhongBan(int maNV, int maPBCu, int maPBMoi)
        {
            if (maNV <= 0) throw new ArgumentException("Chưa chọn nhân viên.");
            if (maPBMoi <= 0) throw new ArgumentException("Chưa chọn phòng ban mới.");
            if (maPBMoi == maPBCu) throw new ArgumentException("Nhân viên đang ở phòng ban này rồi.");
            return dal.ChuyenPhongBan(maNV, maPBMoi);
        }

        private static void KiemTra(NhanVienDTO nv)
        {
            if (string.IsNullOrWhiteSpace(nv.HoTen)) throw new ArgumentException("Họ tên không được trống.");
            if (!Regex.IsMatch(nv.CCCD ?? "", @"^\d{12}$")) throw new ArgumentException("CCCD phải gồm đúng 12 chữ số.");
            if (nv.MaPB <= 0) throw new ArgumentException("Chưa chọn phòng ban.");
            if (nv.LuongCoBan <= 0) throw new ArgumentException("Lương cơ bản phải lớn hơn 0.");
            if (nv.HeSoLuong <= 0) throw new ArgumentException("Hệ số lương phải lớn hơn 0.");
            if (nv.NgaySinh > DateTime.Today.AddYears(-18)) throw new ArgumentException("Nhân viên phải đủ 18 tuổi.");
        }
    }
}
