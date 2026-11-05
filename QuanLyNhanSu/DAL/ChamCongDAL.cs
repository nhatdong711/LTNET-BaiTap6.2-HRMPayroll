using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLyNhanSu.DAL
{
    
    public class ChamCongDAL
    {
        private readonly SqlConnection conn = DBConnection.GetConnection();
        private SqlDataAdapter adapter;
        private SqlCommandBuilder builder;
        public DataSet DataSet { get; } = new DataSet("HRM");

        
        public DataTable NapChamCong(string thangNam)
        {
            if (DataSet.Tables.Contains("BangChamCong")) DataSet.Tables.Remove("BangChamCong");

            adapter = new SqlDataAdapter(
                "SELECT MaCong, MaNV, ThangNam, SoNgayCong, SoGioOT, DaChot FROM BangChamCong WHERE ThangNam = @tn", conn);
            adapter.SelectCommand.Parameters.Add("@tn", SqlDbType.VarChar, 7).Value = thangNam;
            adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey; 
            builder = new SqlCommandBuilder(adapter) { ConflictOption = ConflictOption.OverwriteChanges };

            adapter.Fill(DataSet, "BangChamCong");    
            DataTable dt = DataSet.Tables["BangChamCong"];

            DataColumn colMa = dt.Columns["MaCong"];
            colMa.AutoIncrement = true; colMa.AutoIncrementSeed = -1; colMa.AutoIncrementStep = -1;

            dt.Columns.Add("HoTen", typeof(string));
            dt.Columns.Add("TenPB", typeof(string));
            dt.Columns.Add("ThucLinhDuKien", typeof(decimal));

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
                    dt.Rows.Add(r);                     
                }
                bool chuaDoi = r.RowState == DataRowState.Unchanged;
                r["HoTen"] = nv["HoTen"];
                r["TenPB"] = nv["TenPB"];
                if (chuaDoi) r.AcceptChanges();       
            }
            dt.DefaultView.Sort = "MaNV ASC";
            return dt;
        }

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
