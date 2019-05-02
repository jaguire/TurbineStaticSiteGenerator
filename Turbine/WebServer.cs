using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using Nano.Web.Core;
using Nano.Web.Core.Host.HttpListener;
using OutputColorizer;

namespace Turbine
{
    public interface IWebServer
    {
        void Start();
    }

    public class WebServer : IWebServer
    {
        private readonly string baseUrl;
        private readonly NanoConfiguration nano;

        public WebServer(IAppSettings appSettings)
        {
            var port = GetUnusedPort();
            baseUrl = $"http://localhost:{port}";
            nano = new NanoConfiguration
            {
                ApplicationName = "Turbine",
                EnableVerboseErrors = appSettings.Verbose,
            };

            // logging
            if (appSettings.Verbose)
                nano.GlobalEventHandler.PostInvokeHandlers.Add(context =>
                {
                    var address = context.Request.Url.ToString().Replace(baseUrl, "/").Replace("//", "/");
                    var statusName = Enum.GetName(typeof(Constants.HttpStatusCode), context.Response.HttpStatusCode);
                    Colorizer.WriteLine($"WebServer: [DarkYellow!{address} => {context.Response.HttpStatusCode} {statusName}]");
                });
            nano.GlobalEventHandler.UnhandledExceptionHandlers.Add((exception, context) =>
            {
                var address = context.Request.Url.ToString().Replace(baseUrl, "/").Replace("//", "/");
                Colorizer.WriteLine($"WebServer: [DarkRed!{address} => Exception: {exception.Message}]");
            });

            // pulse
            var startTime = DateTime.Now;
            if (appSettings.Verbose)
                nano.AddBackgroundTask("Uptime", (int)TimeSpan.FromMinutes(1).TotalMilliseconds, () =>
                {
                    var uptime = DateTime.Now - startTime;
                    Colorizer.WriteLine($"WebServer: [DarkYellow!Uptime {uptime}]");
                    return uptime;
                });

            // hosting
            nano.AddDirectory("/", appSettings.Output, returnHttp404WhenFileWasNotFound: true);
            nano.DisableCorrelationId();
            nano.EnableCors();
        }

        public void Start()
        {
            using (HttpListenerNanoServer.Start(nano, baseUrl))
            {
                Colorizer.WriteLine($"[DarkYellow!Web Server Listening on {baseUrl}]");
                Process.Start(baseUrl);
                Startup.Exit.WaitOne();
            }
        }

        private static int GetUnusedPort()
        {
            var activePorts = IPGlobalProperties.GetIPGlobalProperties()
                                                .GetActiveTcpConnections()
                                                .Select(x => x.LocalEndPoint.Port)
                                                .ToList();
            const int min = 49152;
            const int max = 65535;
            var random = new Random();
            var port = random.Next(min, max);
            while (activePorts.Contains(port))
                port = random.Next(min, max);
            return port;
        }
    }
}
