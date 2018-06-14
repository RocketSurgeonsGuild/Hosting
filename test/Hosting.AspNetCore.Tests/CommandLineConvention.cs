using Rocket.Surgery.Extensions.CommandLine;

namespace Rocket.Surgery.Hosting.AspNetCore.Tests
{
    class CommandLineConvention : ICommandLineConvention
    {
        class Command
        {
            public int OnExecute()
            {
                return 1001;
            }
        }

        public void Register(ICommandLineConventionContext context)
        {
            context.AddCommand<Command>("dosomething");
        }
    }
}