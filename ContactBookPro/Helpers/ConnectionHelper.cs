using Npgsql;
using System.Reflection;

namespace ContactBookPro.Helpers
{
    public static class ConnectionHelper
    {
        public static string GetConnectionString(IConfiguration configuration)
        {
            // if we are running locally this connection string will have a value
            var connectionString = configuration.GetSection("pgSettings")["pgConnection"];
            // if we are running on Heroku, this connection string will have a value
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            // ternary opertaor to return a value depending on we run it locally or on Heroku
            return string.IsNullOrEmpty(databaseUrl) ? connectionString : BuildConnectionString(databaseUrl);
        }

        // DATASE_URL is not a valid connection string for our purposes. It needs be connverted to a proper connection string
        private static string BuildConnectionString(string databaseUrl)
        {
            // built-in method to convert url to a uri
            var databaseUri = new Uri(databaseUrl);
            //get user info out of uri and split them into an array
            var userInfo = databaseUri.UserInfo.Split(':');
            // pull the information out of the database, uri amd userInfo
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Require,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }
    }
}
