using System;
using System.IO;
using System.Net;

namespace Unamit.Utility
{
  public static class Client
  {
    public static string Get(string uri)
    {
      try
      {
        var req = (HttpWebRequest)WebRequest.Create(uri);
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