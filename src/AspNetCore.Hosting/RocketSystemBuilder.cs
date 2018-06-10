using Microsoft.AspNetCore.Builder;

namespace Rocket.Surgery.AspNetCore.Hosting
{
    public class RocketSystemBuilder : RocketApplicationBuilder<IRocketApplicationBuilder>
    {
        public RocketSystemBuilder(IRocketApplicationBuilder parent, IApplicationBuilder fork)
            : this(parent, parent, fork)
        {
        }

        public RocketSystemBuilder(IRocketApplicationBuilder parent, IRocketApplicationBuilder root, IApplicationBuilder fork)
            : base(parent, new RocketApplicationBuilder(fork, parent.Configuration))
        {
            // TODO: Fork the container here as well
            Root = root;
        }

        public IRocketApplicationBuilder Root { get; }
    }
}