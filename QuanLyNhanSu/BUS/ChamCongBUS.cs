using System.Data;
using QuanLyNhanSu.DAL;

namespace QuanLyNhanSu.BUS
{
    public class ChamCongBUS
    {
        private readonly ChamCongDAL dal = new();

        public static string ThangNam(DateTime d) => d.ToString("MM/yyyy");

        public DataTable NapChamCong(string thangNam) => dal.NapChamCong(thangNam);
        public bool DaChot(string thangNam) => dal.DaChot(thangNam);

        /// <summary>Kiểm tra dữ liệu trên lưới rồi đồng bộ bằng adapter.Update().</summary>
        public int LuuChamCong(DataTable dt)
        {
            foreach (DataRow r in dt.Rows)
            {
                if (r.RowState == DataRowState.Deleted) continue;
                double ngay = Convert.ToDouble(r["SoNgayCong"]);
                double ot = Convert.ToDouble(r["SoGioOT"]);
                if (ngay < 0 || ngay > 31)
                    throw new ArgumentException($"Nhân viên {r["HoTen"]}: số ngày công phải từ 0 đến 31.");
                if (ot < 0 || ot > 200)
                    throw new ArgumentException($"Nhân viên {r["HoTen"]}: số giờ OT không hợp lệ (0 – 200).");
            }
            return dal.LuuChamCong();
        }
    }
}
