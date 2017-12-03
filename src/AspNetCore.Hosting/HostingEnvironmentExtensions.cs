using System;
using Microsoft.AspNetCore.Hosting.Internal;
using Rocket.Surgery.AspNetCore.Hosting;
using IRsgHostingEnvironment = Rocket.Surgery.Hosting.IHostingEnvironment;
using RsgHostingEnvironment = Rocket.Surgery.Hosting.HostingEnvironment;
using RsgEnvironmentName = Rocket.Surgery.AspNetCore.Hosting.EnvironmentName;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Hosting
{
    public static class HostingEnvironmentExtensions
    {
        public static IRsgHostingEnvironment ToRocketSurgeryHostingEnvironment(this IHostingEnvironment environment)
        {
            return new RsgHostingEnvironment(
                environment.EnvironmentName,
                environment.ApplicationName,
                environment.WebRootPath,
                environment.ContentRootPath);
        }

        /// <summary>
        /// Checks if the current hosting environment name is <see cref="RsgEnvironmentName.Development"/>.
        /// </summary>
        /// <param name="hostingEnvironment">An instance of <see cref="IHostingEnvironment"/>.</param>
        /// <returns>True if the environment name is <see cref="RsgEnvironmentName.Development"/>, otherwise false.</returns>
        public static bool IsDevelopment(this IRsgHostingEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }

            return hostingEnvironment.IsEnvironment(RsgEnvironmentName.Development);
        }

        /// <summary>
        /// Checks if the current hosting environment name is <see cref="RsgEnvironmentName.Test"/>.
        /// </summary>
        /// <param name="hostingEnvironment">An instance of <see cref="IHostingEnvironment"/>.</param>
        /// <returns>True if the environment name is <see cref="RsgEnvironmentName.Test"/>, otherwise false.</returns>
        public static bool IsTest(this IRsgHostingEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }

            return hostingEnvironment.IsEnvironment(RsgEnvironmentName.Test);
        }

        /// <summary>
        /// Checks if the current hosting environment name is <see cref="RsgEnvironmentName.Staging"/>.
        /// </summary>
        /// <param name="hostingEnvironment">An instance of <see cref="IHostingEnvironment"/>.</param>
        /// <returns>True if the environment name is <see cref="RsgEnvironmentName.Staging"/>, otherwise false.</returns>
        public static bool IsStaging(this IRsgHostingEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }

            return hostingEnvironment.IsEnvironment(RsgEnvironmentName.Staging);
        }

        /// <summary>
        /// Checks if the current hosting environment name is <see cref="RsgEnvironmentName.Production"/>.
        /// </summary>
        /// <param name="hostingEnvironment">An instance of <see cref="IHostingEnvironment"/>.</param>
        /// <returns>True if the environment name is <see cref="RsgEnvironmentName.Production"/>, otherwise false.</returns>
        public static bool IsProduction(this IRsgHostingEnvironment hostingEnvironment)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }

            return hostingEnvironment.IsEnvironment(RsgEnvironmentName.Production);
        }

        /// <summary>
        /// Compares the current hosting environment name against the specified value.
        /// </summary>
        /// <param name="hostingEnvironment">An instance of <see cref="IHostingEnvironment"/>.</param>
        /// <param name="environmentName">Environment name to validate against.</param>
        /// <returns>True if the specified name is the same as the current environment, otherwise false.</returns>
        public static bool IsEnvironment(
            this IRsgHostingEnvironment hostingEnvironment,
            string environmentName)
        {
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }

            return string.Equals(
                hostingEnvironment.EnvironmentName,
                environmentName,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
