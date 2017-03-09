using FluentScheduler;
using Nancy;
using Nancy.Hosting.Self;
using System;
using System.Configuration;
using Topshelf;
using Unamit.Services;

namespace Unamit
{
  public class Spark : NancyHost
  {
    public Spark()
      : base(new Uri($"http://localhost:{ConfigurationManager.AppSettings["port"]}"), new DefaultNancyBootstrapper(), new HostConfiguration { UrlReservations = new UrlReservations { CreateAutomatically = true } })
    {
      JobManager.Initialize(new Babybytes());
      JobManager.Initialize(new Groups());
    }

    public static void Main()
    {
      HostFactory.Run(x =>
      {
        x.Service<Spark>(y =>
        {
          y.ConstructUsing(_ => new Spark());
          y.WhenStarted(z => z.Start());
          y.WhenStopped(z =>
          {
            z.Stop();
            JobManager.Stop();
          });
        });

        x.RunAsLocalSystem();
        x.SetServiceName("Unamit");
        x.StartAutomatically();
      });
    }
  }
}