using System;
using System.Reflection;
using Rocket.Surgery.Builders;
using Rocket.Surgery.Conventions;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public interface IRocketSurgeryHostBuilder<T> : IConventionHostBuilder
    {
        /// <summary>
        /// Exclude a convention by type.
        /// </summary>
        /// <param name="type">The type</param>
        T ExceptConvention(Type type);

        /// <summary>
        /// Exclude all conventions from an assembly.
        /// </summary>
        /// <param name="assembly">The assembly</param>
        T ExceptConvention(Assembly assembly);
    }
}
