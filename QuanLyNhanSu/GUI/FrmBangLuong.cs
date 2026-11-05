using QuanLyNhanSu.BUS;

namespace QuanLyNhanSu.GUI
{
    /// <summary>Báo cáo: bảng lương tháng, tổng quỹ lương (ExecuteScalar), thống kê theo phòng ban (SqlDataReader).</summary>
    public class FrmBangLuong : Form
    {
        private readonly LuongBUS bus = new();
        private readonly ComboBox cboThang;
        private readonly Label lblQuy, lblSoNV, lblTB;
        private readonly DataGridView dgvLuong, dgvPB;

        public FrmBangLuong()
        {
            UI.ChuanBiForm(this, "Bảng lương – Báo cáo thống kê", 1000, 660);
            UI.TieuDe(this, "BẢNG LƯƠNG && BÁO CÁO THỐNG KÊ", 20, 12);

            UI.Nhan(this, "Tháng đã chốt:", 20, 62);
            cboThang = UI.Chon(this, 140, 62, 120);
            cboThang.SelectedIndexChanged += (s, e) => TaiBaoCao();

            lblQuy = The("Tổng quỹ lương (ExecuteScalar)", 20, 300);
            lblSoNV = The("Số nhân viên nhận lương", 340, 300);
            lblTB = The("Thực lĩnh bình quân", 660, 320);

            UI.Nhan(this, "BẢNG LƯƠNG CHI TIẾT", 20, 200, true);
            dgvLuong = UI.Luoi(this, 20, 225, 960, 250);

            UI.Nhan(this, "THỐNG KÊ THEO PHÒNG BAN (SqlDataReader)", 20, 485, true);
            dgvPB = UI.Luoi(this, 20, 510, 960, 135);

            Load += (s, e) =>
            {
                try
                {
                    cboThang.DataSource = bus.CacThangDaChot();
                    if (cboThang.Items.Count == 0) MessageBox.Show("Chưa có tháng nào được chốt lương.");
                }
                catch (Exception ex) { UI.BaoLoi(ex); }
            };
        }

        private Label The(string tieuDe, int x, int w)
        {
            var p = new Panel { Left = x, Top = 100, Width = w, Height = 85, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            p.Controls.Add(new Label { Text = tieuDe, Left = 12, Top = 10, AutoSize = true, ForeColor = Color.Gray });
            var v = new Label { Left = 12, Top = 38, AutoSize = true, Font = new Font("Segoe UI", 16f, FontStyle.Bold), ForeColor = UI.MauChinh };
            p.Controls.Add(v);
            Controls.Add(p);
            return v;
        }

        private void TaiBaoCao()
        {
            if (cboThang.SelectedItem is not string thang) return;
            try
            {
                var dt = bus.LayBangLuong(thang);
                dgvLuong.DataSource = dt;
                UI.DatTieuDeCot(dgvLuong, ("MaNV", "Mã"), ("HoTen", "Họ tên"), ("TenPB", "Phòng ban"),
                    ("LuongCoBan", "Lương CB"), ("HeSoLuong", "Hệ số"), ("SoNgayCong", "Ngày công"),
                    ("SoGioOT", "Giờ OT"), ("ThucLinh", "Thực lĩnh"));
                dgvLuong.Columns["MaNV"].FillWeight = 40;
                dgvLuong.Columns["ThucLinh"].DefaultCellStyle.Font = UI.FontDam;

                decimal quy = bus.TongQuyLuong(thang);
                lblQuy.Text = UI.Tien(quy);
                lblSoNV.Text = dt.Rows.Count + " người";
                lblTB.Text = dt.Rows.Count == 0 ? "0 đ" : UI.Tien(Math.Round(quy / dt.Rows.Count, 0));

                dgvPB.DataSource = bus.ThongKeTheoPhongBan(thang);
                UI.DatTieuDeCot(dgvPB, ("TenPB", "Phòng ban"), ("SoNhanVien", "Số NV"),
                    ("TongLuong", "Tổng lương"), ("LuongTrungBinh", "Lương bình quân"));
            }
            catch (Exception ex) { UI.BaoLoi(ex); }
        }
    }
}
