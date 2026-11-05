using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyNhanSu.DTO;

namespace QuanLyNhanSu.DAL
{
    public class LuongDAL
    {
        /// <summary>
        /// Chốt lương tháng trong MỘT SqlTransaction:
        ///  1. Kiểm tra tháng chưa chốt  2. INSERT BangLuong cho toàn bộ nhân viên
        ///  3. Khóa chấm công (DaChot = 1)  4. Commit. Lỗi ở bất kỳ bước nào → Rollback.
        /// </summary>
        public int ChotLuong(string thangNam, List<BangLuongDTO> dsLuong)
        {
            using var conn = DBConnection.GetConnection();
            conn.Open();
            SqlTransaction trans = conn.BeginTransaction();
            try
            {
                // 1. Tháng đã chốt thì không cho chốt lại
                using (var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM BangLuong WITH (UPDLOCK) WHERE ThangNam = @tn", conn, trans))
                {
                    cmd.Parameters.Add("@tn", SqlDbType.VarChar, 7).Value = thangNam;
                    if ((int)cmd.ExecuteScalar() > 0)
                    {
                        trans.Rollback();
                        throw new GiaoDichException(
                            $"Tháng {thangNam} đã được chốt lương trước đó.\n" +
                            "Giao dịch đã ROLLBACK – không ghi trùng bảng lương.");
                    }
                }

                // 2. Ghi bảng lương từng nhân viên
                foreach (var bl in dsLuong)
                {
                    using var cmd = new SqlCommand(
                        "INSERT INTO BangLuong (MaNV, ThangNam, ThucLinh) VALUES (@nv, @tn, @tl)", conn, trans);
                    cmd.Parameters.Add("@nv", SqlDbType.Int).Value = bl.MaNV;
                    cmd.Parameters.Add("@tn", SqlDbType.VarChar, 7).Value = thangNam;
                    var p = cmd.Parameters.Add("@tl", SqlDbType.Decimal);
                    p.Precision = 18; p.Scale = 2; p.Value = bl.ThucLinh;
                    cmd.ExecuteNonQuery();
                }

                // 3. Khóa dữ liệu chấm công của tháng
                using (var cmd = new SqlCommand(
                    "UPDATE BangChamCong SET DaChot = 1 WHERE ThangNam = @tn", conn, trans))
                {
                    cmd.Parameters.Add("@tn", SqlDbType.VarChar, 7).Value = thangNam;
                    cmd.ExecuteNonQuery();
                }

                // 4. Hoàn tất
                trans.Commit();
                return dsLuong.Count;
            }
            catch (GiaoDichException) { throw; }
            catch (Exception ex)
            {
                try { trans.Rollback(); } catch { }
                throw new GiaoDichException("Lỗi khi chốt lương, giao dịch đã ROLLBACK:\n" + ex.Message);
            }
        }

        public DataTable LayBangLuong(string thangNam)
        {
            using var conn = DBConnection.GetConnection();
            using var cmd = new SqlCommand(
                "SELECT bl.MaNV, nv.HoTen, pb.TenPB, nv.LuongCoBan, nv.HeSoLuong, cc.SoNgayCong, cc.SoGioOT, bl.ThucLinh " +
                "FROM BangLuong bl JOIN NhanVien nv ON bl.MaNV = nv.MaNV " +
                "LEFT JOIN PhongBan pb ON nv.MaPB = pb.MaPB " +
                "LEFT JOIN BangChamCong cc ON cc.MaNV = bl.MaNV AND cc.ThangNam = bl.ThangNam " +
                "WHERE bl.ThangNam = @tn ORDER BY bl.MaNV", conn);
            cmd.Parameters.Add("@tn", SqlDbType.VarChar, 7).Value = thangNam;
            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        /// <summary>Tổng quỹ lương tháng – ExecuteScalar.</summary>
        public decimal TongQuyLuong(string thangNam)
        {
            using var conn = DBConnection.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT ISNULL(SUM(ThucLinh), 0) FROM BangLuong WHERE ThangNam = @tn", conn);
            cmd.Parameters.Add("@tn", SqlDbType.VarChar, 7).Value = thangNam;
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        /// <summary>Thống kê lương theo phòng ban – SqlDataReader.</summary>
        public List<ThongKePhongBanDTO> ThongKeTheoPhongBan(string thangNam)
        {
            var ds = new List<ThongKePhongBanDTO>();
            using var conn = DBConnection.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT pb.TenPB, COUNT(bl.MaNV) AS SoNV, ISNULL(SUM(bl.ThucLinh), 0) AS TongLuong " +
                "FROM PhongBan pb LEFT JOIN NhanVien nv ON nv.MaPB = pb.MaPB " +
                "LEFT JOIN BangLuong bl ON bl.MaNV = nv.MaNV AND bl.ThangNam = @tn " +
                "GROUP BY pb.MaPB, pb.TenPB ORDER BY TongLuong DESC", conn);
            cmd.Parameters.Add("@tn", SqlDbType.VarChar, 7).Value = thangNam;
            using SqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
                ds.Add(new ThongKePhongBanDTO { TenPB = rd.GetString(0), SoNhanVien = rd.GetInt32(1), TongLuong = rd.GetDecimal(2) });
            return ds;
        }

        /// <summary>Các tháng đã chốt lương (mới nhất trước).</summary>
        public List<string> CacThangDaChot()
        {
            var ds = new List<string>();
            using var conn = DBConnection.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT DISTINCT ThangNam, RIGHT(ThangNam, 4) AS Nam, LEFT(ThangNam, 2) AS Thang FROM BangLuong ORDER BY Nam DESC, Thang DESC", conn);
            using var rd = cmd.ExecuteReader();
            while (rd.Read()) ds.Add(rd.GetString(0));
            return ds;
        }
    }

    /// <summary>Lỗi nghiệp vụ trong giao dịch (đã Rollback).</summary>
    public class GiaoDichException : Exception
    {
        public GiaoDichException(string msg) : base(msg) { }
    }
}
