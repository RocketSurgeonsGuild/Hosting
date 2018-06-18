using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Rocket.Surgery.AspNetCore.Hosting.Cli
{
    public class WebHostWrapper : IDisposable
    {
        private readonly IWebHost _host;
        private readonly CliServer _server;

        public WebHostWrapper(IWebHost host)
        {
            _server = host.Services.GetRequiredService<IServer>() as CliServer;
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public Uri BaseAddress => _server.BaseAddress;

        public IWebHost Host => _host;

        public HttpMessageHandler CreateHandler()
        {
            var pathBase = BaseAddress == null ? PathString.Empty : PathString.FromUriComponent(BaseAddress);
            return new ClientHandler(pathBase, _server._application);
        }

        public HttpClient CreateClient()
        {
            return new HttpClient(CreateHandler()) { BaseAddress = BaseAddress };
        }

        public WebSocketClient CreateWebSocketClient()
        {
            var pathBase = BaseAddress == null ? PathString.Empty : PathString.FromUriComponent(BaseAddress);
            return new WebSocketClient(pathBase, _server._application);
        }

        /// <summary>
        /// Begins constructing a request message for submission.
        /// </summary>
        /// <param name="path"></param>
        /// <returns><see cref="RequestBuilder"/> to use in constructing additional request details.</returns>
        public RequestBuilder CreateRequest(string path)
        {
            return new RequestBuilder(this, path);
        }

        public void Dispose()
        {
            _host.Dispose();
        }
    }
}
