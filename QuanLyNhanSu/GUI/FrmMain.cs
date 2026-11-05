using QuanLyNhanSu.BUS;

namespace QuanLyNhanSu.GUI
{
    /// <summary>Màn hình chính / tổng quan hệ thống nhân sự.</summary>
    public class FrmMain : Form
    {
        private readonly Label lblSoNV, lblSoPB, lblThang, lblQuy;

        public FrmMain()
        {
            UI.ChuanBiForm(this, "HRM & Payroll – Hệ thống quản lý nhân sự, tính lương", 760, 470);

            var banner = new Panel { Left = 0, Top = 0, Width = 760, Height = 80, BackColor = UI.MauChinh };
            Controls.Add(banner);
            banner.Controls.Add(new Label
            {
                Text = "👔  HRM && PAYROLL SYSTEM", AutoSize = true, Left = 24, Top = 14,
                Font = new Font("Segoe UI", 17f, FontStyle.Bold), ForeColor = Color.White
            });
            banner.Controls.Add(new Label
            {
                Text = "3-Tier (DTO – DAL – BUS – GUI) · Disconnected Model (DataSet / SqlDataAdapter) · SqlTransaction",
                AutoSize = true, Left = 28, Top = 50, ForeColor = Color.FromArgb(220, 235, 255)
            });

            lblSoNV = The("Tổng nhân viên", 24);
            lblSoPB = The("Số phòng ban", 206);
            lblThang = The("Tháng chốt gần nhất", 388);
            lblQuy = The("Quỹ lương tháng đó", 570);

            int y = 230;
            NutChucNang("🧑‍💼  Hồ sơ nhân sự", 24, y, () => new FrmNhanVien(), 220);
            NutChucNang("🗓️  Chấm công && Chốt lương", 270, y, () => new FrmChamCong(), 466, Color.FromArgb(230, 126, 34));
            NutChucNang("📊  Bảng lương – Báo cáo thống kê", 24, y + 80, () => new FrmBangLuong(), 712, Color.FromArgb(39, 174, 96));

            Controls.Add(new Label
            {
                Text = "Sinh viên: Trần Nhật Đông – MSV 24810320177 – Lớp D19QTANM1",
                AutoSize = true, Left = 24, Top = 430, ForeColor = Color.Gray
            });
            Load += (s, e) => CapNhatTongQuan();
        }

        private Label The(string tieuDe, int x)
        {
            var p = new Panel { Left = x, Top = 100, Width = 166, Height = 100, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            p.Controls.Add(new Label { Text = tieuDe, Left = 10, Top = 12, AutoSize = true, ForeColor = Color.Gray });
            var v = new Label { Left = 10, Top = 45, AutoSize = true, Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = UI.MauChinh };
            p.Controls.Add(v);
            Controls.Add(p);
            return v;
        }

        private void NutChucNang(string text, int x, int y, Func<Form> taoForm, int w, Color? mau = null)
        {
            var b = UI.Nut(this, text, x, y, w, mau);
            b.Height = 60;
            b.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            b.Click += (s, e) =>
            {
                using var f = taoForm();
                f.ShowDialog(this);
                CapNhatTongQuan();
            };
        }

        private void CapNhatTongQuan()
        {
            try
            {
                var nvBus = new NhanVienBUS();
                var lBus = new LuongBUS();
                lblSoNV.Text = nvBus.DemNhanVien().ToString();
                lblSoPB.Text = nvBus.LayPhongBan().Rows.Count.ToString();
                string thang = lBus.CacThangDaChot().FirstOrDefault();
                lblThang.Text = thang ?? "Chưa có";
                lblQuy.Text = thang == null ? "0 đ" : UI.Tien(lBus.TongQuyLuong(thang));
            }
            catch (Exception ex) { UI.BaoLoi(ex); }
        }
    }
}
