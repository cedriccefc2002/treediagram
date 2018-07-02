using System;
using System.IO;
using System.Reflection;
using AutoMapper;
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
            AddIceBridge(ref services);
            AddgRPCBridge(ref services);
            AddRepository(ref services);
            AddService(ref services);
            AddLogger(ref services);
            services.AddLogging((builder) => builder.SetMinimumLevel(LogLevel.Trace));
            serviceProvider = services.BuildServiceProvider();
            ConfigLog(ref serviceProvider);
            AutoMapperInitialize();
        }
        private static void AutoMapperInitialize()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<lib.Service.AutoMapper.NodeProfile>();
                x.AddProfile<lib.Service.AutoMapper.NodeRelationshipProfile>();
            });
        }
        private static void AddRepository(ref ServiceCollection services)
        {
            services.AddSingleton<Repository.IRepository, Repository.Neo4jRepository>();
        }
        private static void AddService(ref ServiceCollection services)
        {
            services.AddSingleton<Service.ServerService>();
            services.AddSingleton<Service.EventService>();
        }
        private static void AddIceBridge(ref ServiceCollection services)
        {
            services.AddSingleton<IceBridge.Server>();
        }
        private static void AddgRPCBridge(ref ServiceCollection services)
        {
            services.AddSingleton<gRPCBridge.Server>();
        }
        private static void AddLogger(ref ServiceCollection services)
        {
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        }
        private static void ConfigLog(ref IServiceProvider serviceProvider, string logPath = "nlog.config")
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