﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Lucca.Logs.Tests
{
    public class ConfigTest
    {

        [Fact(Skip = "Later")]
        public void NoConfigShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                ServiceProvider provider = TestHelper.Register<DummyLogPlayer>(loggingBuilder =>
                {
                    loggingBuilder.AddLuccaLogs(o =>
                    {
                    }, null);
                });
                provider.GetRequiredService<DummyLogPlayer>();
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
                    loggingBuilder.AddLuccaLogs(config.GetSection("LuccaLoggerOptions"));
                });
                provider.GetRequiredService<DummyLogPlayer>();
            });
        }


        [Fact]
        public void HardcodedConfigShouldNotThrowIfOk()
        {
            ServiceProvider provider = TestHelper.Register<DummyLogPlayer>(loggingBuilder =>
            {
                loggingBuilder.AddLuccaLogs(conf =>
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
                loggingBuilder.AddLuccaLogs(config.GetSection("LuccaLogs"));
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
                loggingBuilder.AddLuccaLogs(config.GetSection("LuccaLogs"));
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
