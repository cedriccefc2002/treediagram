using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Server.lib.Service
{
    public static class Provider
    {
        private static readonly ServiceCollection services;

        static Provider()
        {
            services = new ServiceCollection();
            services.AddSingleton<ServerService>();
            services.AddSingleton<NodeService>();
        }
        public static IServiceProvider Init(string logPath = "nlog.config")
        {
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging((builder) => builder.SetMinimumLevel(LogLevel.Trace));

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            // var cwd = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            //configure NLog
            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
            // NLog.LogManager.LoadConfiguration(Path.Join(cwd,logPath));
            NLog.LogManager.LoadConfiguration(logPath);
            return serviceProvider;
        }
    }

}