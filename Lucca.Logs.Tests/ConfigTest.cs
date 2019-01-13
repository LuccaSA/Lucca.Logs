using System;
using System.IO;
using Lucca.Logs.AspnetCore;
using Lucca.Logs.Shared;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lucca.Logs.Tests
{
    public class ConfigTest
    {
        [Fact]
        public void NoConfigShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                TestHelper.Register<DummyLogPlayer>(loggingBuilder =>
                {
                    loggingBuilder.AddLuccaLogs(new HostingEnvironment(), (IConfigurationSection)null);
                });
            });
        }

        [Fact]
        public void NoHostingEnvShouldThrowException()
        {
            IConfigurationRoot config = LoadConfig("missing.json");
            Assert.Throws<ArgumentNullException>(() =>
            {
                TestHelper.Register<DummyLogPlayer>(loggingBuilder =>
                {
                    loggingBuilder.AddLuccaLogs(null, config.GetSection("LuccaLoggerOptions"));
                });
            });
        }

        [Fact]
        public void NoConfigFoundShouldThrowException()
        {
            IConfigurationRoot config = LoadConfig("missing.json");
            Assert.Throws<LogConfigurationException>(() =>
            {
                ServiceProvider provider = TestHelper.Register<DummyLogPlayer>(loggingBuilder =>
                {
                    loggingBuilder.AddLuccaLogs(new HostingEnvironment(), config.GetSection("LuccaLoggerOptions"));
                });
                provider.GetRequiredService<DummyLogPlayer>();
            });
        }


        [Fact]
        public void HardcodedConfigShouldNotThrowIfOk()
        {
            ServiceProvider provider = TestHelper.Register<DummyLogPlayer>(loggingBuilder =>
            {
                loggingBuilder.AddLuccaLogs(new HostingEnvironment(), conf =>
                 {

                 });
            });
            provider.GetRequiredService<DummyLogPlayer>();
        }

        [Fact]
        public void LoadConfigFromJsonMinimal()
        {
            IConfigurationRoot config = LoadConfig("minimal.json");

            ServiceProvider provider = TestHelper.Register<InjectOption>(loggingBuilder =>
            {
                loggingBuilder.AddLuccaLogs(new HostingEnvironment(), config.GetSection("LuccaLogs"));
            });
            var injected = provider.GetRequiredService<InjectOption>();

            Assert.Equal("test", injected.Options.ConnectionString);
        }

        [Fact]
        public void LoadConfigFromJsonStandard()
        {
            IConfigurationRoot config = LoadConfig("standard.json");

            ServiceProvider provider = TestHelper.Register<InjectOption>(loggingBuilder =>
            {
                loggingBuilder.AddLuccaLogs(new HostingEnvironment(), config.GetSection("LuccaLogs"));
            });
            var injected = provider.GetRequiredService<InjectOption>();

            Assert.Equal("myConnectionString", injected.Options.ConnectionString);
        }

        [Fact]
        public void LoadConfig_InnerNlog()
        {
            // TODO
        }

        [Fact]
        public void LoadConfig_InnerExceptional()
        {
            // TODO
        }

        private static IConfigurationRoot LoadConfig(string configFile) => new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(@"Configs\" + configFile, false, true)
            .Build();
    }
}
