using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Server.lib
{
    public static class Provider
    {
        private static readonly ServiceCollection services;
        public static readonly IServiceProvider serviceProvider;

        static Provider()
        {
            services = new ServiceCollection();
            AddService(services);
            AddLogger(services);
            services.AddLogging((builder) => builder.SetMinimumLevel(LogLevel.Trace));
            serviceProvider = services.BuildServiceProvider();
            ConfigLog(serviceProvider);
        }
        private static void AddService(ServiceCollection services)
        {
            services.AddSingleton<Service.ServerService>();
            // services.AddSingleton<NodeService>();
        }
        private static void AddLogger(ServiceCollection services)
        {
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        }

        public static void ConfigLog(IServiceProvider serviceProvider, string logPath = "nlog.config")
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            // var cwd = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            //configure NLog
            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
            // NLog.LogManager.LoadConfiguration(Path.Join(cwd,logPath));
            NLog.LogManager.LoadConfiguration(logPath);
        }
    }
}