using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Unamit
{
  public static class Database
  {
    public static IDbConnection Connect()
    {
      var cs = ConfigurationManager.ConnectionStrings["Db"].ConnectionString;
      var conn = new SqlConnection(cs);

      conn.Open();
      return conn;
    }
  }
}