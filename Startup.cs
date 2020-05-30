using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Storage;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using SmithSmsStatusFetcher.JobHelpers;
using SmithSmsStatusFetcher.Jobs;
using SmithSmsStatusFetcher.Models;
using SmithSmsStatusFetcher.Services;
using SmithSmsStatusFetcher.Settings;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SmithSmsStatusFetcher
{
    public class Startup
    {

        internal IServiceCollection ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {

            services.Configure<TwilioSecrets>(options =>
            {
                options = LoadTwilioSecrets(context.Configuration, options);
            });
            services.Configure<DbSettings>(options =>
            {
                options = LoadDbSettings(context.Configuration, options);
            });
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<DbSettings>>().Value);
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TwilioSecrets>>().Value);

            services.AddTransient<TwilioProcessingService>();

            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

            services.AddSingleton<QuartzJobRunner>();
            services.AddHostedService<QuartzHostedService>();

            services.AddScoped<MessageStatusProcessorJob>();


            var sp = services.BuildServiceProvider();
            DbSettings dbSettings = sp.GetService<DbSettings>();


            services.AddDbContext<SmithDbContext>(options => options
                            .UseMySql(dbSettings.ConnectionString,
                                mysqlOptions =>
                                    mysqlOptions.ServerVersion(new ServerVersion(new Version(10, 4, 6), ServerType.MariaDb))));

            services.AddSingleton(new JobSchedule(
                jobType: typeof(MessageStatusProcessorJob),
                                                  cronExpression: "0/3 * * * * ?") // every 3 seconds  
                                                                                   //cronExpression: "0 5 0 ? * * *")  // every minute
                );

            return services;
        }

        internal TwilioSecrets LoadTwilioSecrets(IConfiguration config, TwilioSecrets secrets)
        {

            secrets.AuthToken = GetSetting<string>(config, "TwilioSecrets:AuthToken");
            secrets.AccountSid = GetSetting<string>(config, "TwilioSecrets:AccountSid");
            return secrets;
        }
        internal DbSettings LoadDbSettings(IConfiguration config, DbSettings settings)
        {

            settings.Username = GetSetting<string>(config, "Database:Username");
            settings.Password = GetSetting<string>(config, "Database:Password");
            settings.Server = GetSetting<string>(config, "Database:Server");
            settings.Database = GetSetting<string>(config, "Database:Database");

            return settings;
        }



        public T GetSetting<T>(IConfiguration config, string fullKey)
        {
            var setting = config.GetSection(fullKey).Value;
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)(converter.ConvertFromInvariantString(setting));
        }
        internal async Task Run(IHostBuilder builder)
        {
            await builder.RunConsoleAsync();
        }


    }
}
