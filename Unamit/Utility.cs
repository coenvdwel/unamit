using System;

namespace Unamit
{
  public static class Utility
  {
    public static string GetId()
    {
      return Guid.NewGuid().ToString().Replace("-", "");
    }
  }
}