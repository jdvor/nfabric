namespace NFabric.Core.Tests
{
    using NFabric.Core.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using Xunit;

    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void NamedServiceSingletons()
        {
            var services = new ServiceCollection();
            services.AddSingletons<IMyService>(
                ("A", typeof(ServiceA)),
                ("B", typeof(ServiceB)));

            var provider = services.BuildServiceProvider();
            var a1 = provider.GetRequiredService<Func<string, IMyService>>()("A");
            var b1 = provider.GetRequiredService<Func<string, IMyService>>()("B");
            var a2 = provider.GetRequiredService<Func<string, IMyService>>()("A");
            var b2 = provider.GetRequiredService<Func<string, IMyService>>()("B");

            Assert.True(a1 is ServiceA);
            Assert.True(object.ReferenceEquals(a1, a2));
            Assert.True(b1 is ServiceB);
            Assert.True(object.ReferenceEquals(b1, b2));
        }

        [Fact]
        public void NamedServiceTransients()
        {
            var services = new ServiceCollection();
            services.AddTransients<IMyService>(
                ("A", typeof(ServiceA)),
                ("B", typeof(ServiceB)));

            var provider = services.BuildServiceProvider();
            var a1 = provider.GetRequiredService<Func<string, IMyService>>()("A");
            var b1 = provider.GetRequiredService<Func<string, IMyService>>()("B");
            var a2 = provider.GetRequiredService<Func<string, IMyService>>()("A");
            var b2 = provider.GetRequiredService<Func<string, IMyService>>()("B");

            Assert.True(a1 is ServiceA);
            Assert.True(!object.ReferenceEquals(a1, a2));
            Assert.True(b1 is ServiceB);
            Assert.True(!object.ReferenceEquals(b1, b2));
        }
    }

    public interface IMyService
    {
    }

    public class ServiceA : IMyService
    {
    }

    public class ServiceB : IMyService
    {
    }
}
