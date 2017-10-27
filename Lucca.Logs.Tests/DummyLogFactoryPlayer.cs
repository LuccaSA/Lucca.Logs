﻿using Microsoft.Extensions.Logging;

namespace Lucca.Logs.Tests
{
    public class DummyLogFactoryPlayer : ILogPlayer<DummyLogFactoryPlayer>
    {
        public ILogger<DummyLogFactoryPlayer> Logger { get; }

        public DummyLogFactoryPlayer(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger<DummyLogFactoryPlayer>();
        }
    }
}