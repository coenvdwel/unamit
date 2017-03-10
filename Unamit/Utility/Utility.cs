using Nancy;
using Nancy.ModelBinding;
using System;

namespace Unamit.Utility
{
  public static class Utility
  {
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
  }
}