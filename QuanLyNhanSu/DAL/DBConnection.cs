using System.Configuration;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace QuanLyNhanSu.DAL
{
    /// <summary>Quản lý chuỗi kết nối và khởi tạo CSDL HRMDB.</summary>
    public static class DBConnection
    {
        public static string ConnStr =>
            ConfigurationManager.ConnectionStrings["HRMDB"]?.ConnectionString
            ?? @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=HRMDB;Integrated Security=True;TrustServerCertificate=True";

        public static SqlConnection GetConnection() => new SqlConnection(ConnStr);

        /// <summary>Nếu CSDL chưa tồn tại thì chạy Database/ScriptDB.sql (tách theo GO).</summary>
        public static void KhoiTaoCSDL()
        {
            var builder = new SqlConnectionStringBuilder(ConnStr);
            string dbName = builder.InitialCatalog;
            builder.InitialCatalog = "master";

            using var conn = new SqlConnection(builder.ConnectionString);
            conn.Open();
            using (var check = new SqlCommand("SELECT DB_ID(@db)", conn))
            {
                check.Parameters.AddWithValue("@db", dbName);
                if (check.ExecuteScalar() != DBNull.Value) return; // đã có CSDL
            }

            string path = Path.Combine(AppContext.BaseDirectory, "ScriptDB.sql");
            string script = File.ReadAllText(path);
            foreach (string batch in Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(batch)) continue;
                using var cmd = new SqlCommand(batch, conn);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
