using Nancy;

namespace Unamit.Modules
{
  public class Base : NancyModule
  {
    public Base()
    {
      Get["/"] = _ => View["Content/index.html"];
    }
  }
}