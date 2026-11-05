using Microsoft.Data.SqlClient;

namespace QuanLyNhanSu.GUI
{
    /// <summary>Hàm tiện ích tạo control và hiển thị lỗi thống nhất cho các Form.</summary>
    internal static class UI
    {
        public static readonly Font FontChu = new("Segoe UI", 10f);
        public static readonly Font FontDam = new("Segoe UI", 10f, FontStyle.Bold);
        public static readonly Color MauChinh = Color.FromArgb(0, 120, 103);
        public static readonly Color MauNen = Color.FromArgb(245, 247, 250);

        public static void ChuanBiForm(Form f, string tieuDe, int w, int h)
        {
            f.Text = tieuDe;
            f.ClientSize = new Size(w, h);
            f.Font = FontChu;
            f.BackColor = MauNen;
            f.StartPosition = FormStartPosition.CenterScreen;
            f.FormBorderStyle = FormBorderStyle.FixedSingle;
            f.MaximizeBox = false;
            // Tự co giãn theo DPI màn hình (125%, 150%...) vì control được đặt theo pixel ở 96 DPI
            f.AutoScaleMode = AutoScaleMode.None;
            f.Load += (s, e) =>
            {
                float k = f.DeviceDpi / 96f;
                if (Math.Abs(k - 1f) > 0.01f)
                {
                    f.ClientSize = new Size((int)(f.ClientSize.Width * k), (int)(f.ClientSize.Height * k));
                    PhongTo(f, k);
                }
                Rectangle man = Screen.FromControl(f).WorkingArea;
                f.Location = new Point(man.Left + Math.Max(0, (man.Width - f.Width) / 2),
                                       man.Top + Math.Max(0, (man.Height - f.Height) / 2));
            };
        }

        /// <summary>Nhân vị trí/kích thước mọi control con theo hệ số DPI.</summary>
        private static void PhongTo(Control cha, float k)
        {
            foreach (Control c in cha.Controls)
            {
                c.SetBounds((int)(c.Left * k), (int)(c.Top * k), (int)(c.Width * k), (int)(c.Height * k));
                PhongTo(c, k);
            }
        }

        public static Label TieuDe(Control cha, string text, int x, int y)
        {
            var l = new Label
            {
                Text = text, Left = x, Top = y, AutoSize = true,
                Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = MauChinh
            };
            cha.Controls.Add(l);
            return l;
        }

        public static Label Nhan(Control cha, string text, int x, int y, bool dam = false)
        {
            var l = new Label { Text = text, Left = x, Top = y + 3, AutoSize = true, Font = dam ? FontDam : FontChu };
            cha.Controls.Add(l);
            return l;
        }

        public static TextBox O(Control cha, int x, int y, int w = 220)
        {
            var t = new TextBox { Left = x, Top = y, Width = w };
            cha.Controls.Add(t);
            return t;
        }

        public static NumericUpDown So(Control cha, int x, int y, int w, decimal max, int thapPhan = 0)
        {
            var n = new NumericUpDown
            {
                Left = x, Top = y, Width = w, Maximum = max, DecimalPlaces = thapPhan,
                ThousandsSeparator = true, TextAlign = HorizontalAlignment.Right
            };
            cha.Controls.Add(n);
            return n;
        }

        public static ComboBox Chon(Control cha, int x, int y, int w = 220)
        {
            var c = new ComboBox { Left = x, Top = y, Width = w, DropDownStyle = ComboBoxStyle.DropDownList };
            cha.Controls.Add(c);
            return c;
        }

        public static Button Nut(Control cha, string text, int x, int y, int w = 100, Color? mau = null)
        {
            var b = new Button
            {
                Text = text, Left = x, Top = y, Width = w, Height = 34,
                FlatStyle = FlatStyle.Flat, BackColor = mau ?? MauChinh, ForeColor = Color.White,
                Font = FontDam, Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            cha.Controls.Add(b);
            return b;
        }

        public static DataGridView Luoi(Control cha, int x, int y, int w, int h)
        {
            var g = new DataGridView
            {
                Left = x, Top = y, Width = w, Height = h,
                ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White, RowHeadersVisible = false,
                EnableHeadersVisualStyles = false
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = MauChinh;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = FontDam;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(235, 245, 243);
            cha.Controls.Add(g);
            return g;
        }

        /// <summary>Đổi tiêu đề cột và định dạng số tiền.</summary>
        public static void DatTieuDeCot(DataGridView g, params (string cot, string tieuDe)[] ds)
        {
            foreach (var (cot, tieuDe) in ds)
            {
                if (!g.Columns.Contains(cot)) continue;
                g.Columns[cot].HeaderText = tieuDe;
            }
            foreach (DataGridViewColumn c in g.Columns)
            {
                if (c.ValueType == typeof(decimal))
                {
                    c.DefaultCellStyle.Format = "N0";
                    c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                if (c.ValueType == typeof(DateTime)) c.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            }
        }

        public static string Tien(decimal so) => so.ToString("N0") + " đ";

        public static void BaoThanhCong(string msg) =>
            MessageBox.Show(msg, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public static bool XacNhan(string msg) =>
            MessageBox.Show(msg, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        /// <summary>Hiển thị lỗi thân thiện: lỗi nghiệp vụ (BUS) và lỗi ràng buộc SQL.</summary>
        public static void BaoLoi(Exception ex)
        {
            string msg = ex.Message;
            if (ex is SqlException sql)
            {
                msg = sql.Number switch
                {
                    2627 or 2601 => "Dữ liệu bị trùng (CCCD đã tồn tại hoặc tháng này đã có bảng lương).",
                    547 => "Không thể thực hiện vì vi phạm ràng buộc dữ liệu (đang được dùng ở bảng khác hoặc giá trị không hợp lệ).",
                    _ => "Lỗi SQL Server: " + sql.Message
                };
            }
            var icon = ex is ArgumentException ? MessageBoxIcon.Warning : MessageBoxIcon.Error;
            MessageBox.Show(msg, "Lỗi", MessageBoxButtons.OK, icon);
        }
    }
}
