namespace Unamit.Models
{
  public class Session
  {
    public string Id;

    public Session()
    {
      Id = Utility.GetId();
    }
  }
}