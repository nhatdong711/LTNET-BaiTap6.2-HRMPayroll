using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLyNhanSu.DAL
{
    public class PhongBanDAL
    {
        public DataTable LayTatCa()
        {
            using var conn = DBConnection.GetConnection();
            using var da = new SqlDataAdapter("SELECT MaPB, TenPB, Mota FROM PhongBan ORDER BY MaPB", conn);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }
}
