using System.Data;
using QuanLyNhanSu.BUS;
using QuanLyNhanSu.DTO;

namespace QuanLyNhanSu.GUI
{
    /// <summary>Quản lý hồ sơ nhân sự: thêm / sửa / xóa / chuyển phòng ban / tìm kiếm.</summary>
    public class FrmNhanVien : Form
    {
        private readonly NhanVienBUS bus = new();
        private readonly TextBox txtMa, txtTen, txtCCCD, txtTim;
        private readonly DateTimePicker dtpNgaySinh;
        private readonly ComboBox cboPB, cboChuyen, cboLocPB;
        private readonly NumericUpDown numLuong, numHeSo;
        private readonly DataGridView dgv;
        private int maPBHienTai;

        public FrmNhanVien()
        {
            UI.ChuanBiForm(this, "Hồ sơ nhân sự", 1000, 650);
            UI.TieuDe(this, "QUẢN LÝ HỒ SƠ NHÂN SỰ", 20, 12);

            UI.Nhan(this, "Mã NV:", 20, 60);       txtMa = UI.O(this, 130, 60, 80); txtMa.ReadOnly = true;
            UI.Nhan(this, "Họ tên:", 20, 97);      txtTen = UI.O(this, 130, 97, 250);
            UI.Nhan(this, "Ngày sinh:", 20, 134);
            dtpNgaySinh = new DateTimePicker { Left = 130, Top = 134, Width = 150, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Value = new DateTime(2000, 1, 1) };
            Controls.Add(dtpNgaySinh);

            UI.Nhan(this, "CCCD:", 410, 60);        txtCCCD = UI.O(this, 530, 60, 180);
            UI.Nhan(this, "Phòng ban:", 410, 97);  cboPB = UI.Chon(this, 530, 97, 180);
            UI.Nhan(this, "Lương cơ bản:", 410, 134); numLuong = UI.So(this, 530, 134, 180, 1_000_000_000);
            numLuong.Increment = 500000;
            UI.Nhan(this, "Hệ số lương:", 740, 60); numHeSo = UI.So(this, 850, 60, 120, 20, 2);
            numHeSo.Increment = 0.01m; numHeSo.Value = 1;

            // Chuyển phòng ban
            var grp = new GroupBox { Text = "Chuyển phòng ban", Left = 740, Top = 92, Width = 240, Height = 80, Font = UI.FontDam };
            Controls.Add(grp);
            cboChuyen = UI.Chon(grp, 10, 32, 120); cboChuyen.Font = UI.FontChu;
            UI.Nut(grp, "Chuyển", 140, 28, 90, Color.FromArgb(142, 68, 173)).Click += (s, e) => ChuyenPB();

            int y = 185;
            UI.Nut(this, "Thêm", 20, y).Click += (s, e) => ThucHien(() => bus.Them(DocForm()), "Đã thêm nhân viên.");
            UI.Nut(this, "Sửa", 130, y, 100, Color.FromArgb(52, 101, 164)).Click += (s, e) =>
            {
                if (txtMa.Text == "") { MessageBox.Show("Chọn nhân viên trên lưới trước."); return; }
                ThucHien(() => bus.Sua(DocForm()), "Đã cập nhật hồ sơ.");
            };
            UI.Nut(this, "Xóa", 240, y, 100, Color.FromArgb(192, 57, 43)).Click += (s, e) =>
            {
                if (txtMa.Text == "") { MessageBox.Show("Chọn nhân viên trên lưới trước."); return; }
                if (UI.XacNhan($"Xóa nhân viên {txtTen.Text}?"))
                    ThucHien(() => bus.Xoa(int.Parse(txtMa.Text)), "Đã xóa nhân viên.");
            };
            UI.Nut(this, "Làm mới", 350, y, 100, Color.Gray).Click += (s, e) => { XoaForm(); TaiDuLieu(); };

            // Tìm kiếm: tên hoặc CCCD + lọc phòng ban
            UI.Nhan(this, "Tìm tên / CCCD:", 480, y + 5);
            txtTim = UI.O(this, 600, y + 4, 150);
            txtTim.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { TaiDuLieu(); e.SuppressKeyPress = true; } };
            cboLocPB = UI.Chon(this, 760, y + 4, 130);
            cboLocPB.SelectedIndexChanged += (s, e) => TaiDuLieu();
            UI.Nut(this, "Tìm", 900, y, 80).Click += (s, e) => TaiDuLieu();

            dgv = UI.Luoi(this, 20, 235, 960, 395);
            dgv.CellClick += (s, e) => ChonDong(e.RowIndex);

            Load += (s, e) => { TaiPhongBan(); TaiDuLieu(); };
        }

        private void TaiPhongBan()
        {
            DataTable pb = bus.LayPhongBan();
            cboPB.DataSource = pb.Copy(); cboPB.DisplayMember = "TenPB"; cboPB.ValueMember = "MaPB";
            cboChuyen.DataSource = pb.Copy(); cboChuyen.DisplayMember = "TenPB"; cboChuyen.ValueMember = "MaPB";

            DataTable loc = pb.Copy();
            DataRow tatCa = loc.NewRow(); tatCa["MaPB"] = 0; tatCa["TenPB"] = "— Tất cả phòng ban —";
            loc.Rows.InsertAt(tatCa, 0);
            cboLocPB.DataSource = loc; cboLocPB.DisplayMember = "TenPB"; cboLocPB.ValueMember = "MaPB";
        }

        private void TaiDuLieu()
        {
            if (cboLocPB == null || dgv == null) return;
            try
            {
                int pb = cboLocPB.SelectedValue is int v ? v : 0;
                dgv.DataSource = bus.TimKiem(txtTim.Text, pb);
                UI.DatTieuDeCot(dgv, ("MaNV", "Mã"), ("HoTen", "Họ tên"), ("NgaySinh", "Ngày sinh"), ("CCCD", "CCCD"),
                    ("TenPB", "Phòng ban"), ("LuongCoBan", "Lương cơ bản"), ("HeSoLuong", "Hệ số"));
                if (dgv.Columns.Contains("MaPB")) dgv.Columns["MaPB"].Visible = false;
                if (dgv.Columns.Contains("NgaySinh")) dgv.Columns["NgaySinh"].DefaultCellStyle.Format = "dd/MM/yyyy";
                if (dgv.Columns.Contains("MaNV")) dgv.Columns["MaNV"].FillWeight = 40;
                if (dgv.Columns.Contains("HeSoLuong")) dgv.Columns["HeSoLuong"].FillWeight = 50;
            }
            catch (Exception ex) { UI.BaoLoi(ex); }
        }

        private void ChonDong(int i)
        {
            if (i < 0) return;
            var r = dgv.Rows[i];
            txtMa.Text = r.Cells["MaNV"].Value.ToString();
            txtTen.Text = r.Cells["HoTen"].Value.ToString();
            if (r.Cells["NgaySinh"].Value is DateTime ns) dtpNgaySinh.Value = ns;
            txtCCCD.Text = r.Cells["CCCD"].Value?.ToString();
            maPBHienTai = r.Cells["MaPB"].Value is int pb ? pb : 0;
            if (maPBHienTai > 0) cboPB.SelectedValue = maPBHienTai;
            numLuong.Value = Convert.ToDecimal(r.Cells["LuongCoBan"].Value);
            numHeSo.Value = Convert.ToDecimal(r.Cells["HeSoLuong"].Value);
        }

        private NhanVienDTO DocForm() => new()
        {
            MaNV = int.TryParse(txtMa.Text, out int ma) ? ma : 0,
            HoTen = txtTen.Text.Trim(),
            NgaySinh = dtpNgaySinh.Value.Date,
            CCCD = txtCCCD.Text.Trim(),
            MaPB = cboPB.SelectedValue is int pb ? pb : 0,
            LuongCoBan = numLuong.Value,
            HeSoLuong = (double)numHeSo.Value
        };

        private void ChuyenPB()
        {
            try
            {
                int maNV = int.TryParse(txtMa.Text, out int m) ? m : 0;
                int moi = cboChuyen.SelectedValue is int v ? v : 0;
                if (bus.ChuyenPhongBan(maNV, maPBHienTai, moi))
                {
                    UI.BaoThanhCong($"Đã chuyển {txtTen.Text} sang {cboChuyen.Text}.");
                    XoaForm(); TaiDuLieu();
                }
            }
            catch (Exception ex) { UI.BaoLoi(ex); }
        }

        private void XoaForm()
        {
            txtMa.Clear(); txtTen.Clear(); txtCCCD.Clear();
            numLuong.Value = 0; numHeSo.Value = 1; maPBHienTai = 0;
            dtpNgaySinh.Value = new DateTime(2000, 1, 1);
        }

        private void ThucHien(Func<bool> hanhDong, string thongBao)
        {
            try
            {
                if (hanhDong()) { UI.BaoThanhCong(thongBao); XoaForm(); TaiDuLieu(); }
            }
            catch (Exception ex) { UI.BaoLoi(ex); }
        }
    }
}
