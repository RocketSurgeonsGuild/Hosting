// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Context = Microsoft.AspNetCore.Hosting.Internal.HostingApplication.Context;

namespace Rocket.Surgery.AspNetCore.Hosting.Cli
{
    public class CliServer : IServer
    {
        private const string ServerName = nameof(CliServer);
        private bool _disposed = false;
        internal IHttpApplication<Context> _application;

        public CliServer()
            : this(new FeatureCollection())
        {
        }

        public CliServer(IFeatureCollection featureCollection)
        {
            Features = featureCollection ?? throw new ArgumentNullException(nameof(featureCollection));
        }

        public IFeatureCollection Features { get; }

        public Uri BaseAddress { get; set; } = new Uri("http://localhost/");

        /// <summary>
        /// Creates, configures, sends, and returns a <see cref="HttpContext"/>. This completes as soon as the response is started.
        /// </summary>
        /// <returns></returns>
        public async Task<HttpContext> SendAsync(Action<HttpContext> configureContext, CancellationToken cancellationToken = default)
        {
            if (configureContext == null)
            {
                throw new ArgumentNullException(nameof(configureContext));
            }

            var builder = new HttpContextBuilder(_application);
            builder.Configure(context =>
            {
                var request = context.Request;
                request.Scheme = BaseAddress.Scheme;
                request.Host = HostString.FromUriComponent(BaseAddress);
                if (BaseAddress.IsDefaultPort)
                {
                    request.Host = new HostString(request.Host.Host);
                }
                var pathBase = PathString.FromUriComponent(BaseAddress);
                if (pathBase.HasValue && pathBase.Value.EndsWith("/"))
                {
                    pathBase = new PathString(pathBase.Value.Substring(0, pathBase.Value.Length - 1));
                }
                request.PathBase = pathBase;
            });
            builder.Configure(configureContext);
            return await builder.SendAsync(cancellationToken).ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        Task IServer.StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            _application = new ApplicationWrapper<Context>((IHttpApplication<Context>)application, () =>
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
            });

            return Task.CompletedTask;
        }

        Task IServer.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private class ApplicationWrapper<TContext> : IHttpApplication<TContext>
        {
            private readonly IHttpApplication<TContext> _application;
            private readonly Action _preProcessRequestAsync;

            public ApplicationWrapper(IHttpApplication<TContext> application, Action preProcessRequestAsync)
            {
                _application = application;
                _preProcessRequestAsync = preProcessRequestAsync;
            }

            public TContext CreateContext(IFeatureCollection contextFeatures)
            {
                return _application.CreateContext(contextFeatures);
            }

            public void DisposeContext(TContext context, Exception exception)
            {
                _application.DisposeContext(context, exception);
            }

            public Task ProcessRequestAsync(TContext context)
            {
                _preProcessRequestAsync();
                return _application.ProcessRequestAsync(context);
            }
        }
    }
}
