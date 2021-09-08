using IPInfo;
using IPInfo.Batch;
using IPInfo.IPStack;
using IPInfo.Persistent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace IPInfo.Web
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
            #region IP lookup Configuration

            // ipstack http provider + db + cache
            var ipConfig = Configuration.GetSection("IPStack").Get<IPStackConfiguration>();
            var dbConfig = Configuration.GetSection("Persistent").Get<PersistentConfiguration>();
            var cacheConfig = Configuration.GetSection("Caching").Get<CachingConfiguration>();

            services.AddSingleton(ipConfig);
            services.AddSingleton<ObjectCache>(MemoryCache.Default);

            services.AddDbContext<IPDetailsDataContext>((options) =>
            {
                options.UseSqlServer(dbConfig.ConnectionString);
            });

            services.AddTransient<IPStackInfoProvider>();

            services.AddTransient<IIPInfoProvider>(services => 
            { 
                var ipStack = services.GetRequiredService<IPStackInfoProvider>();
                var dataCtx = services.GetRequiredService<IPDetailsDataContext>();
                var cache = services.GetRequiredService<ObjectCache>();

                var dbProvider = new PersistentInfoProvider(dbConfig, ipStack, dataCtx);
                var cachingProvider = new CachingInfoProvider(cacheConfig, dbProvider, cache);

                return cachingProvider;
            });

            // background hosted service for batch update requests  
            var batchConfig = Configuration.GetSection("BatchUpdate").Get<BatchUpdateConfiguration>();

            services.AddSingleton(batchConfig);
            services.AddSingleton<IBatchJobRepository, InMemoryBatchJobRepository>();

            services.AddTransient<IIPInfoBatchUpdateProvider, BatchUpdateProvider>();

            services.AddHostedService<BatchUpdateService>();

            #endregion

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "IPInfo", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Novibet v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
