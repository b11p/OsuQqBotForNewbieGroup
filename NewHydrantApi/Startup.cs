using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace NewHydrantApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = Configuration.GetConnectionString("NewbieDatabase_Postgres");
            var dataSourceBuilder = NewbieContext.GetDataSourceBuilder(connectionString);
            var dataSource = dataSourceBuilder.Build();
            services.AddDbContext<NewbieContext>(
                options => options.UseNpgsql(dataSource,
                    options => options.EnableRetryOnFailure()),
                ServiceLifetime.Transient);
            services.AddControllers();
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "消防栓 API", Version = "v1" });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // proxy
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.AllowedHosts.Add("api.bleatingsheep.org");
                options.AllowedHosts.Add("api2.bleatingsheep.org");
                options.AllowedHosts.Add("xn--6orp08a.bleatingsheep.org"); // 接口.bleatingsheep.org
                options.AllowedHosts.Add("bleatingsheep.org");

                // get global IPv6 prefixes. (default /64)
                var knownPrefixes = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetUnicastAddresses()
                    .Select(i => i.Address)
                    .Where(a => a?.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
                        && !IPAddress.IsLoopback(a)
                        && !a.IsIPv6Teredo
                        && ((a.GetAddressBytes()[0] ^ 0b00100000) & 0b11100000) == 0) // is global unicast address.
                    .Select(localIPv6 =>
                    {
                        // get /64 prefixes
                        byte[] ipBytes = new byte[16];
                        Array.Copy(localIPv6.GetAddressBytes(), ipBytes, 8);
                        return new IPAddress(ipBytes);
                    }).Distinct();

                // add prefixes to known networks.
                foreach (IPAddress ip in knownPrefixes)
                {
                    options.KnownNetworks.Add(new IPNetwork(ip, 64));
                }

                options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("172.16.0.0"), 12)); // dockerr internal
                options.ForwardLimit = 1;
                options.ForwardedHeaders = ForwardedHeaders.All;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // proxy
            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hydrant API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
