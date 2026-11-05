using QuanLyNhanSu.DAL;
using QuanLyNhanSu.GUI;

namespace QuanLyNhanSu
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            try
            {
                // Tự tạo CSDL HRMDB + dữ liệu mẫu nếu chưa có
                DBConnection.KhoiTaoCSDL();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không kết nối được SQL Server:\n" + ex.Message +
                    "\n\nKiểm tra chuỗi kết nối trong App.config.", "Lỗi CSDL",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Application.Run(new FrmMain());
        }
    }
}
