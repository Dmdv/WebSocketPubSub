using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PubSubSignalR.Hubs;
using PubSubSignalR.Options;
using PubSubSignalR.Services;

namespace PubSubSignalR
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<RedisSettingsOptions>(configuration.GetSection(nameof(RedisSettingsOptions)));
            services.AddGrpc();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders |= ForwardedHeaders.XForwardedFor;
                options.ForwardedHeaders |= ForwardedHeaders.XForwardedProto;
            });

            var redisConnectionString = configuration.GetSection("RedisSettingsOptions:ConnectionString").Value;

            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.KeepAliveInterval = TimeSpan.FromSeconds(5);
            }).AddStackExchangeRedis(redisConnectionString, options => {
                options.Configuration.ChannelPrefix = "signalr";
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseHttpsRedirection();
            }
            else
            {
                // Production env is running with a k8s loadbalancer using an HTTP Scheme
                // We need UseForwardedHeaders to forward the request in HTTPS
                // NOTE: UseHttpsRedirection and UseHsts doesn't work in the k8s environment
                app.UseForwardedHeaders();
            }

            app.UseRouting();
            app.UseCors(builder => builder.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<NotificationGrpcService>();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("gRPC requires HTTP/2");
                    endpoints.MapHub<PubSubHub>($"/{nameof(PubSubHub)}");
                });
            });
        }
    }
}
