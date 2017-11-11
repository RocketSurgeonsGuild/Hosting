using System;
using System.Collections.Generic;
using System.Text;
using FakeItEasy;
using FluentAssertions;
using Xunit;

namespace Rocket.Surgery.Extensions.Hosting.Tests
{
    public class SimpleTest
    {
        [Fact]
        public void ConvertsFromAspNetCoreHosting_To_RocketSurgeryHosting()
        {
            var fake = A.Fake<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();
            A.CallTo(() => fake.EnvironmentName).Returns(nameof(IHostingEnvironment.EnvironmentName));
            A.CallTo(() => fake.ApplicationName).Returns(nameof(IHostingEnvironment.ApplicationName));
            A.CallTo(() => fake.ContentRootPath).Returns(nameof(IHostingEnvironment.ContentRootPath));
            A.CallTo(() => fake.WebRootPath).Returns(nameof(IHostingEnvironment.WebRootPath));

            var environment = fake.ToRocketSurgeryHostingEnvironment();
            environment.EnvironmentName.Should().Be(nameof(IHostingEnvironment.EnvironmentName));
            environment.ApplicationName.Should().Be(nameof(IHostingEnvironment.ApplicationName));
            environment.ContentRootPath.Should().Be(nameof(IHostingEnvironment.ContentRootPath));
            environment.WebRootPath.Should().Be(nameof(IHostingEnvironment.WebRootPath));
        }
    }
}
