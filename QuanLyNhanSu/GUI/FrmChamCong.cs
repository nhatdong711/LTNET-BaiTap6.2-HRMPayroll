using System.Data;
using QuanLyNhanSu.BUS;
using QuanLyNhanSu.DAL;

namespace QuanLyNhanSu.GUI
{
    /// <summary>
    /// Chấm công ngắt kết nối: nạp DataTable bằng SqlDataAdapter, nhập ngày công / giờ OT trực tiếp trên lưới,
    /// "Lưu chấm công" = SqlCommandBuilder + adapter.Update(); "Chốt lương tháng" = SqlTransaction.
    /// </summary>
    public class FrmChamCong : Form
    {
        private readonly ChamCongBUS ccBus = new();
        private readonly LuongBUS luongBus = new();
        private readonly DateTimePicker dtpThang;
        private readonly Label lblTrangThai, lblThayDoi, lblTong;
        private readonly DataGridView dgv;
        private readonly Button btnLuu, btnTinh, btnChot;
        private DataTable bang;
        private string thangNam;
        private bool daChot;

        public FrmChamCong()
        {
            UI.ChuanBiForm(this, "Chấm công & Chốt lương", 1000, 660);
            UI.TieuDe(this, "CHẤM CÔNG THÁNG && CHỐT LƯƠNG", 20, 12);

            UI.Nhan(this, "Tháng / Năm:", 20, 62);
            dtpThang = new DateTimePicker
            {
                Left = 125, Top = 62, Width = 120, Format = DateTimePickerFormat.Custom, CustomFormat = "MM/yyyy",
                ShowUpDown = true, Value = DateTime.Today
            };
            Controls.Add(dtpThang);
            UI.Nut(this, "Nạp dữ liệu", 260, 57, 130).Click += (s, e) => NapDuLieu();

            lblTrangThai = new Label { Left = 410, Top = 58, AutoSize = true, Font = new Font("Segoe UI", 11f, FontStyle.Bold) };
            Controls.Add(lblTrangThai);
            lblThayDoi = new Label { Left = 410, Top = 85, AutoSize = true, ForeColor = Color.FromArgb(192, 57, 43) };
            Controls.Add(lblThayDoi);

            var goiY = UI.Nhan(this, "Nhập trực tiếp \"Số ngày công\", \"Giờ OT\" trên lưới (ô vàng).   " +
                                     "Thực lĩnh = (Lương CB × Hệ số / 22) × Ngày công + Giờ OT × 50.000", 20, 112);
            goiY.ForeColor = Color.DimGray;
            goiY.Font = new Font("Segoe UI", 9f, FontStyle.Italic);

            dgv = UI.Luoi(this, 20, 140, 960, 400);
            dgv.ReadOnly = false;
            dgv.EditMode = DataGridViewEditMode.EditOnEnter;
            dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgv.DataError += (s, e) =>
            {
                MessageBox.Show("Giá trị không hợp lệ – vui lòng nhập số.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
            };
            dgv.CellValueChanged += (s, e) => CapNhatNhanThayDoi();

            lblTong = new Label { Left = 20, Top = 550, AutoSize = true, Font = UI.FontDam, ForeColor = UI.MauChinh };
            Controls.Add(lblTong);

            btnLuu = UI.Nut(this, "💾  Lưu chấm công", 20, 590, 220);
            btnLuu.Click += (s, e) => LuuChamCong();
            btnTinh = UI.Nut(this, "🧮  Tính lương (xem trước)", 260, 590, 260, Color.FromArgb(52, 101, 164));
            btnTinh.Click += (s, e) => TinhLuongXemTruoc();
            btnChot = UI.Nut(this, "🔒  CHỐT LƯƠNG THÁNG", 700, 590, 280, Color.FromArgb(230, 126, 34));
            btnChot.Click += (s, e) => ChotLuong();
            foreach (var b in new[] { btnLuu, btnTinh, btnChot }) b.Height = 44;

            Load += (s, e) => NapDuLieu();
            FormClosing += (s, e) =>
            {
                if (bang?.GetChanges() != null && !daChot &&
                    !UI.XacNhan("Có dữ liệu chấm công chưa lưu. Vẫn đóng cửa sổ?")) e.Cancel = true;
            };
        }

        private void NapDuLieu()
        {
            try
            {
                thangNam = ChamCongBUS.ThangNam(dtpThang.Value);
                bang = ccBus.NapChamCong(thangNam);
                daChot = ccBus.DaChot(thangNam);

                dgv.DataSource = bang.DefaultView;
                foreach (var c in new[] { "MaCong", "ThangNam", "DaChot" })
                    if (dgv.Columns.Contains(c)) dgv.Columns[c].Visible = false;
                dgv.Columns["MaNV"].DisplayIndex = 0;
                dgv.Columns["HoTen"].DisplayIndex = 1;
                dgv.Columns["TenPB"].DisplayIndex = 2;
                UI.DatTieuDeCot(dgv, ("MaNV", "Mã NV"), ("HoTen", "Họ tên"), ("TenPB", "Phòng ban"),
                    ("SoNgayCong", "Số ngày công"), ("SoGioOT", "Giờ OT"), ("ThucLinhDuKien", "Thực lĩnh dự kiến"));
                dgv.Columns["MaNV"].FillWeight = 45;

                foreach (DataGridViewColumn c in dgv.Columns)
                {
                    bool choSua = !daChot && (c.Name == "SoNgayCong" || c.Name == "SoGioOT");
                    c.ReadOnly = !choSua;
                    c.DefaultCellStyle.BackColor = choSua ? Color.FromArgb(255, 249, 196) : Color.Empty;
                    if (c.Name == "SoNgayCong" || c.Name == "SoGioOT")
                        c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                lblTrangThai.Text = daChot
                    ? $"🔒 Tháng {thangNam}: ĐÃ CHỐT LƯƠNG – dữ liệu chấm công bị khóa"
                    : $"✏️ Tháng {thangNam}: đang chấm công (chưa chốt)";
                lblTrangThai.ForeColor = daChot ? Color.FromArgb(192, 57, 43) : UI.MauChinh;
                btnLuu.Enabled = !daChot;
                lblTong.Text = "";
                if (daChot) TinhLuongXemTruoc();
                CapNhatNhanThayDoi();
            }
            catch (Exception ex) { UI.BaoLoi(ex); }
        }

        private void CapNhatNhanThayDoi()
        {
            if (bang == null) return;
            DataTable thayDoi = bang.GetChanges();
            int moi = thayDoi?.Select(null, null, DataViewRowState.Added).Length ?? 0;
            int sua = thayDoi?.Select(null, null, DataViewRowState.ModifiedCurrent).Length ?? 0;
            lblThayDoi.Text = thayDoi == null
                ? "Không có thay đổi chưa lưu."
                : $"Chưa lưu: {moi} dòng mới (Added), {sua} dòng sửa (Modified) – bấm \"Lưu chấm công\".";
        }

        private void LuuChamCong()
        {
            try
            {
                dgv.EndEdit();
                bang.DefaultView.RowStateFilter = DataViewRowState.CurrentRows;
                int n = ccBus.LuuChamCong(bang);
                UI.BaoThanhCong($"Đã đồng bộ {n} dòng chấm công tháng {thangNam} về SQL Server\n" +
                                "bằng SqlCommandBuilder + adapter.Update() (1 thao tác).");
                NapDuLieu();
            }
            catch (Exception ex) { UI.BaoLoi(ex); }
        }

        /// <summary>Tính thực lĩnh trên DataTable (chỉ hiển thị, không ghi CSDL).</summary>
        private void TinhLuongXemTruoc()
        {
            try
            {
                dgv.EndEdit();
                DataTable nv = new NhanVienBUS().TimKiem("", 0);
                decimal tong = 0;
                foreach (DataRow r in bang.Rows)
                {
                    DataRow n = nv.Select("MaNV = " + r["MaNV"]).FirstOrDefault();
                    if (n == null) continue;
                    decimal tl = LuongBUS.TinhThucLinh((decimal)n["LuongCoBan"], Convert.ToDouble(n["HeSoLuong"]),
                        Convert.ToDouble(r["SoNgayCong"]), Convert.ToDouble(r["SoGioOT"]));
                    bool giuNguyen = r.RowState == DataRowState.Unchanged;
                    r["ThucLinhDuKien"] = tl;
                    if (giuNguyen) r.AcceptChanges();   // cột hiển thị không làm dòng thành Modified
                    tong += tl;
                }
                lblTong.Text = $"Tổng quỹ lương tháng {thangNam}: {UI.Tien(tong)}  ({bang.Rows.Count} nhân viên)";
                CapNhatNhanThayDoi();
            }
            catch (Exception ex) { UI.BaoLoi(ex); }
        }

        private void ChotLuong()
        {
            try
            {
                dgv.EndEdit();
                if (bang.GetChanges() != null)
                {
                    if (!UI.XacNhan("Chấm công còn thay đổi chưa lưu. Lưu rồi chốt lương?")) return;
                    ccBus.LuuChamCong(bang);
                }
                if (!UI.XacNhan($"Chốt lương tháng {thangNam}?\nSau khi chốt, dữ liệu chấm công sẽ bị khóa.")) return;

                int n = luongBus.ChotLuong(thangNam);
                UI.BaoThanhCong($"Đã COMMIT: ghi bảng lương cho {n} nhân viên và khóa chấm công tháng {thangNam}.\n" +
                                $"Tổng quỹ lương: {UI.Tien(luongBus.TongQuyLuong(thangNam))}");
                NapDuLieu();
            }
            catch (GiaoDichException ex)
            {
                MessageBox.Show(ex.Message, "Transaction Rollback", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex) { UI.BaoLoi(ex); }
        }
    }
}
