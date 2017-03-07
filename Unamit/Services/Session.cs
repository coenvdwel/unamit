using Nancy;
using System.Collections.Generic;
using System.Linq;

namespace Unamit.Services
{
  public class Session : NancyModule
  {
    public static List<Models.Session> Sessions = new List<Models.Session>();

    public Session() : base("/login")
    {
      //Post["/"] = _ => Sessions.AddNew();
      //Post["/{sessionId}/{componentType}"] = args =>
      //{
      //  var s = Sessions.First(x => x.Id == args.SessionId);
      //  return "";
      //};

      //.Select(type => Activator.CreateInstance(type) as IComponent)
    }
  }
}