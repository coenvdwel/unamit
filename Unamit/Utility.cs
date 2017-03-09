using Dapper;
using Nancy;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;

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

      return (user = Modules.Session.Sessions.Get(sid) as string) != null;
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
      catch (Exception ex)
      {
#if DEBUG
        throw ex;
#else
        return default(T);
#endif
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
#if DEBUG
        throw ex;
#else
        return false;
#endif
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
#if DEBUG
        throw ex;
#else
        return new T[0];
#endif
      }
    }

    public static string Get(string uri)
    {
      try
      {
        var req = (HttpWebRequest)WebRequest.Create(uri);
        req.UserAgent = "Unamit";

        using (var res = (HttpWebResponse)req.GetResponse())
        {
          using (var s = res.GetResponseStream())
          {
            if (s == null) return null;
            using (var r = new StreamReader(s))
            {
              return r.ReadToEnd();
            }
          }
        }
      }
      catch (Exception ex)
      {
#if DEBUG
        throw ex;
#else
        return null;
#endif
      }
    }
  }
}