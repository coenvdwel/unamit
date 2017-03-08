using Dapper;
using Nancy;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Unamit
{
  public static class Utility
  {
    public static string GetId()
    {
      return Guid.NewGuid().ToString().Replace("-", "");
    }

    public static bool LoggedIn(INancyModule m)
    {
      string user;
      return LoggedIn(m, out user);
    }

    public static bool LoggedIn(INancyModule m, out string user)
    {
      var sid = (string)m.Request.Query["sid"];
      if (string.IsNullOrEmpty(sid))
      {
        user = null;
        return false;
      }

      return (user = Services.Session.Sessions.Get(sid) as string) != null;
    }

    public static IDbConnection Connect()
    {
      var cs = ConfigurationManager.ConnectionStrings["Db"].ConnectionString;
      var conn = new SqlConnection(cs);

      conn.Open();
      return conn;
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

    public static bool TryExecute(this IDbConnection db, string sql, object param = null, int ok = 1)
    {
      try
      {
        return db.Execute(sql, param) == ok;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public static IEnumerable<T> TryQuery<T>(this IDbConnection db, string sql, object param = null)
    {
      try
      {
        return db.Query<T>(sql, param);
      }
      catch (Exception ex)
      {
        return new T[0];
      }
    }
  }
}