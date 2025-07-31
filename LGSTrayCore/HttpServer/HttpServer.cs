using EmbedIO;
using EmbedIO.Net;
using EmbedIO.WebApi;
using LGSTrayPrimitives;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace LGSTrayCore.HttpServer
{
    public class HttpServer : IHostedService, IDisposable
    {
        private readonly AppSettings _appSettings;
        private readonly HttpControllerFactory _httpControllerFactory;

        private CancellationTokenSource _serverCts = null!;
        private WebServer _server = null!;

        public HttpServer(IOptions<AppSettings> appSettings, HttpControllerFactory httpControllerFactory)
        {
            _appSettings = appSettings.Value;
            _httpControllerFactory = httpControllerFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _serverCts = new();
            _server = CreateServer(_appSettings, _httpControllerFactory);
            _server.RunAsync(_serverCts.Token);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _serverCts.Cancel();

            return Task.CompletedTask;
        }

        private static WebServer CreateServer(AppSettings appSettings, HttpControllerFactory httpControllerFactory)
        {
            EndPointManager.UseIpv6 = appSettings.HTTPServer.UseIpv6;

            var server = new WebServer(o => o.WithUrlPrefix(appSettings.HTTPServer.UrlPrefix))
                .WithWebApi("/", m => m.WithController(httpControllerFactory.CreateController));

            return server;
        }

        #region Dispose
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this._server?.Dispose();
                    this._serverCts?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
