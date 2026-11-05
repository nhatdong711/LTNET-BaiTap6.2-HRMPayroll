using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyNhanSu.DTO;

namespace QuanLyNhanSu.DAL
{
    public class NhanVienDAL
    {
      
        public DataTable TimKiem(string tuKhoa, int maPB)
        {
            using var conn = DBConnection.GetConnection();
            using var cmd = new SqlCommand(
                "SELECT nv.MaNV, nv.HoTen, nv.NgaySinh, nv.CCCD, nv.MaPB, pb.TenPB, nv.LuongCoBan, nv.HeSoLuong " +
                "FROM NhanVien nv LEFT JOIN PhongBan pb ON nv.MaPB = pb.MaPB " +
                "WHERE (nv.HoTen LIKE @tk OR nv.CCCD LIKE @tk) AND (@pb = 0 OR nv.MaPB = @pb) " +
                "ORDER BY nv.MaNV", conn);
            cmd.Parameters.Add("@tk", SqlDbType.NVarChar, 110).Value = "%" + tuKhoa + "%";
            cmd.Parameters.Add("@pb", SqlDbType.Int).Value = maPB;
            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public int DemNhanVien()
        {
            using var conn = DBConnection.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM NhanVien", conn);
            return (int)cmd.ExecuteScalar();
        }

        public bool Them(NhanVienDTO nv)
        {
            using var conn = DBConnection.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "INSERT INTO NhanVien (HoTen, NgaySinh, CCCD, MaPB, LuongCoBan, HeSoLuong) " +
                "VALUES (@ten, @ns, @cccd, @pb, @luong, @heso)", conn);
            ThemThamSo(cmd, nv);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Sua(NhanVienDTO nv)
        {
            using var conn = DBConnection.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "UPDATE NhanVien SET HoTen = @ten, NgaySinh = @ns, CCCD = @cccd, MaPB = @pb, " +
                "LuongCoBan = @luong, HeSoLuong = @heso WHERE MaNV = @ma", conn);
            ThemThamSo(cmd, nv);
            cmd.Parameters.Add("@ma", SqlDbType.Int).Value = nv.MaNV;
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool ChuyenPhongBan(int maNV, int maPBMoi)
        {
            using var conn = DBConnection.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("UPDATE NhanVien SET MaPB = @pb WHERE MaNV = @ma", conn);
            cmd.Parameters.Add("@pb", SqlDbType.Int).Value = maPBMoi;
            cmd.Parameters.Add("@ma", SqlDbType.Int).Value = maNV;
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Xoa(int maNV)
        {
            using var conn = DBConnection.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("DELETE FROM NhanVien WHERE MaNV = @ma", conn);
            cmd.Parameters.Add("@ma", SqlDbType.Int).Value = maNV;
            return cmd.ExecuteNonQuery() > 0;
        }

        private static void ThemThamSo(SqlCommand cmd, NhanVienDTO nv)
        {
            cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = nv.HoTen;
            cmd.Parameters.Add("@ns", SqlDbType.DateTime).Value = nv.NgaySinh.Date;
            cmd.Parameters.Add("@cccd", SqlDbType.VarChar, 20).Value = nv.CCCD;
            cmd.Parameters.Add("@pb", SqlDbType.Int).Value = nv.MaPB;
            var p = cmd.Parameters.Add("@luong", SqlDbType.Decimal);
            p.Precision = 18; p.Scale = 2; p.Value = nv.LuongCoBan;
            cmd.Parameters.Add("@heso", SqlDbType.Float).Value = nv.HeSoLuong;
        }
    }
}
