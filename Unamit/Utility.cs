using Nancy;
using Nancy.ModelBinding;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Unamit
{
  public static class Utility
  {
    public static string GetId()
    {
      return Guid.NewGuid().ToString().Replace("-", "");
    }

    public static IDbConnection Connect()
    {
      var cs = ConfigurationManager.ConnectionStrings["Db"].ConnectionString;
      var conn = new SqlConnection(cs);

      conn.Open();
      return conn;
    }

    public static string Hash(this string s, int i = 777)
    {
      if (i <= 0) return s;

      var hash = new StringBuilder();
      foreach (var b in new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(s), 0, Encoding.UTF8.GetByteCount(s))) hash.Append(b.ToString("x2"));
      return hash.ToString().Hash(--i);
    }

    public static T TryBind<T>(this INancyModule t)
    {
      try
      {
        return t.Bind<T>();
      }
      catch
      {
        return default(T);
      }
    }
  }
}