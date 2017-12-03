using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    class ImplictHostingEnvironment : Rocket.Surgery.Hosting.IHostingEnvironment, IHostingEnvironment
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public ImplictHostingEnvironment(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public string EnvironmentName
        {
            get => _hostingEnvironment.EnvironmentName;
            set => _hostingEnvironment.EnvironmentName = value;
        }

        public string ApplicationName
        {
            get => _hostingEnvironment.ApplicationName;
            set => _hostingEnvironment.ApplicationName = value;
        }

        public string WebRootPath
        {
            get => _hostingEnvironment.WebRootPath;
            set => _hostingEnvironment.WebRootPath = value;
        }

        public IFileProvider WebRootFileProvider
        {
            get => _hostingEnvironment.WebRootFileProvider;
            set => _hostingEnvironment.WebRootFileProvider = value;
        }

        public string ContentRootPath
        {
            get => _hostingEnvironment.ContentRootPath;
            set => _hostingEnvironment.ContentRootPath = value;
        }

        public IFileProvider ContentRootFileProvider
        {
            get => _hostingEnvironment.ContentRootFileProvider;
            set => _hostingEnvironment.ContentRootFileProvider = value;
        }
    }
}
