using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLyNhanSu.DAL
{
    /// <summary>
    /// Chấm công theo MÔ HÌNH NGẮT KẾT NỐI (Disconnected Model):
    /// SqlDataAdapter.Fill → DataSet/DataTable (sửa trên lưới, không giữ kết nối)
    /// → SqlCommandBuilder sinh INSERT/UPDATE → adapter.Update() đồng bộ về SQL Server 1 lần.
    /// </summary>
    public class ChamCongDAL
    {
        private readonly SqlConnection conn = DBConnection.GetConnection();
        private SqlDataAdapter adapter;
        private SqlCommandBuilder builder;
        public DataSet DataSet { get; } = new DataSet("HRM");

        /// <summary>Nạp chấm công của tháng; nhân viên chưa có dòng nào sẽ được thêm dòng mới (RowState = Added).</summary>
        public DataTable NapChamCong(string thangNam)
        {
            if (DataSet.Tables.Contains("BangChamCong")) DataSet.Tables.Remove("BangChamCong");

            adapter = new SqlDataAdapter(
                "SELECT MaCong, MaNV, ThangNam, SoNgayCong, SoGioOT, DaChot FROM BangChamCong WHERE ThangNam = @tn", conn);
            adapter.SelectCommand.Parameters.Add("@tn", SqlDbType.VarChar, 7).Value = thangNam;
            adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey; // lấy khóa chính, identity
            builder = new SqlCommandBuilder(adapter) { ConflictOption = ConflictOption.OverwriteChanges };

            adapter.Fill(DataSet, "BangChamCong");     // adapter tự mở rồi đóng kết nối
            DataTable dt = DataSet.Tables["BangChamCong"];

            // Khóa tạm âm cho dòng mới, SQL Server sẽ cấp MaCong thật khi Update
            DataColumn colMa = dt.Columns["MaCong"];
            colMa.AutoIncrement = true; colMa.AutoIncrementSeed = -1; colMa.AutoIncrementStep = -1;

            // Cột chỉ để hiển thị (không có trong bảng nên SqlCommandBuilder bỏ qua)
            dt.Columns.Add("HoTen", typeof(string));
            dt.Columns.Add("TenPB", typeof(string));
            dt.Columns.Add("ThucLinhDuKien", typeof(decimal));

            // Danh sách toàn bộ nhân viên
            var dsNV = new DataTable();
            using (var da = new SqlDataAdapter(
                "SELECT nv.MaNV, nv.HoTen, ISNULL(pb.TenPB, N'') AS TenPB FROM NhanVien nv " +
                "LEFT JOIN PhongBan pb ON nv.MaPB = pb.MaPB ORDER BY nv.MaNV", conn))
                da.Fill(dsNV);

            foreach (DataRow nv in dsNV.Rows)
            {
                int maNV = (int)nv["MaNV"];
                DataRow r = dt.Select("MaNV = " + maNV).FirstOrDefault();
                if (r == null)
                {
                    r = dt.NewRow();
                    r["MaNV"] = maNV;
                    r["ThangNam"] = thangNam;
                    r["SoNgayCong"] = 0d;
                    r["SoGioOT"] = 0d;
                    r["DaChot"] = false;
                    dt.Rows.Add(r);                     // RowState = Added
                }
                bool chuaDoi = r.RowState == DataRowState.Unchanged;
                r["HoTen"] = nv["HoTen"];
                r["TenPB"] = nv["TenPB"];
                if (chuaDoi) r.AcceptChanges();         // cột hiển thị không tính là thay đổi
            }
            dt.DefaultView.Sort = "MaNV ASC";
            return dt;
        }

        /// <summary>Đồng bộ toàn bộ thay đổi về SQL Server trong 1 lần gọi adapter.Update().</summary>
        public int LuuChamCong()
        {
            DataTable dt = DataSet.Tables["BangChamCong"];
            if (dt == null) return 0;
            return adapter.Update(dt);
        }

        public bool DaChot(string thangNam)
        {
            using var c = DBConnection.GetConnection();
            c.Open();
            using var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM BangChamCong WHERE ThangNam = @tn AND DaChot = 1", c);
            cmd.Parameters.Add("@tn", SqlDbType.VarChar, 7).Value = thangNam;
            return (int)cmd.ExecuteScalar() > 0;
        }

        /// <summary>Lấy dữ liệu chấm công + lương cơ bản để BUS tính thực lĩnh.</summary>
        public List<DTO.ChamCongDTO> LayDuLieuTinhLuong(string thangNam)
        {
            var ds = new List<DTO.ChamCongDTO>();
            using var c = DBConnection.GetConnection();
            c.Open();
            using var cmd = new SqlCommand(
                "SELECT cc.MaNV, cc.ThangNam, cc.SoNgayCong, cc.SoGioOT, nv.LuongCoBan, nv.HeSoLuong " +
                "FROM BangChamCong cc JOIN NhanVien nv ON cc.MaNV = nv.MaNV WHERE cc.ThangNam = @tn", c);
            cmd.Parameters.Add("@tn", SqlDbType.VarChar, 7).Value = thangNam;
            using SqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                ds.Add(new DTO.ChamCongDTO
                {
                    MaNV = rd.GetInt32(0),
                    ThangNam = rd.GetString(1),
                    SoNgayCong = rd.GetDouble(2),
                    SoGioOT = rd.GetDouble(3),
                    LuongCoBan = rd.GetDecimal(4),
                    HeSoLuong = rd.GetDouble(5)
                });
            }
            return ds;
        }
    }
}
