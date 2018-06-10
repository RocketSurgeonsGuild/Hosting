using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Builders;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.CommandLine;

namespace Rocket.Surgery.Hosting
{
    public interface IRocketSurgeryHostBuilder<T> : IBuilder
    {
        /// <summary>
        /// Add a delegate to the scanner, that runs before scanning.
        /// </summary>
        /// <param name="delegate">The delegate</param>
        T PrependDelegate(Delegate @delegate);

        /// <summary>
        /// Adds a convention to the scanner, that runs before scanning.
        /// </summary>
        /// <param name="convention">The convention</param>
        T PrependConvention(IConvention convention);

        /// <summary>
        /// Add a delegate to the scanner, that runs after scanning.
        /// </summary>
        /// <param name="delegate">The delegate</param>
        T AppendDelegate(Delegate @delegate);

        /// <summary>
        /// Adds a convention to the scanner, that runs after scanning.
        /// </summary>
        /// <param name="convention">The convention</param>
        T AppendConvention(IConvention convention);

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
