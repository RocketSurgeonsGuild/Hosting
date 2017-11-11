namespace Rocket.Surgery.Hosting
{
    public class HostingEnvironment : IHostingEnvironment
    {
        public HostingEnvironment(string environmentName, string applicationName, string webRootPath, string contentRootPath)
        {
            EnvironmentName = environmentName;
            ApplicationName = applicationName;
            WebRootPath = webRootPath;
            ContentRootPath = contentRootPath;
        }

        public string EnvironmentName { get;  }
        public string ApplicationName { get; }
        public string WebRootPath { get;  }
        public string ContentRootPath { get; }
    }
}
