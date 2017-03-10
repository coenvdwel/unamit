using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Unamit.Utility
{
  public static class Db
  {
    public static IDbConnection Connect()
    {
      var cs = ConfigurationManager.ConnectionStrings["Db"].ConnectionString;
      var conn = new SqlConnection(cs);

      conn.Open();
      return conn;
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

    public static T TryScalar<T>(this IDbConnection db, string sql, object param = null)
    {
      try
      {
        return db.ExecuteScalar<T>(sql, param);
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

    public static bool TryScalar<T>(this IDbConnection db, string sql, object param, T ok)
    {
      try
      {
        return db.ExecuteScalar<T>(sql, param).Equals(ok);
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
  }
}