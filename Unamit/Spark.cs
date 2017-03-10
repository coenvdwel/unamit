using FluentScheduler;
using Nancy.Hosting.Self;
using System;
using System.Configuration;
using Topshelf;
using Unamit.Services;
using Unamit.Utility;

namespace Unamit
{
  public class Spark : NancyHost
  {
    public static Uri Uri = new Uri($"http://localhost:{ConfigurationManager.AppSettings["port"]}");

    public Spark() : base(Uri, new Bootstrapper(), new HostConfiguration { UrlReservations = new UrlReservations { CreateAutomatically = true } })
    {
    }

    public new void Start()
    {
      JobManager.Initialize(new Babybytes());
      JobManager.Initialize(new Groups());

      base.Start();
    }

    public new void Stop()
    {
      JobManager.Stop();

      base.Stop();
    }

    public static void Main()
    {
      HostFactory.Run(x =>
      {
        x.Service<Spark>(y =>
        {
          y.ConstructUsing(_ => new Spark());
          y.WhenStarted(z => z.Start());
          y.WhenStopped(z => z.Stop());
        });

        x.RunAsLocalSystem();
        x.SetServiceName("Unamit");
        x.StartAutomatically();
      });
    }
  }
}