using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog;
using NLog.Web;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace OnlyOfficeDocumentClientNetCore
{
    public class Startup
    {

        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            Common.Tools.VirtualPath = Common.Appsettings.app(new string[] { "OnlyOffice", "filePath" });
            Common.Tools.Secret = Common.Appsettings.app(new string[] { "OnlyOffice", "secret" });
            long fileSizeMax = 0;
            long.TryParse( Common.Appsettings.app(new string[] { "OnlyOffice", "FileSizeMax" }),out fileSizeMax);

            Common.Tools.FileSizeMax = fileSizeMax;
            services.Configure<KestrelServerOptions>(x => x.AllowSynchronousIO = true)
                       .Configure<IISServerOptions>(x => x.AllowSynchronousIO = true);

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseStaticFiles();
            app.UseAuthorization();
            loggerFactory.AddNLog();//使用NLog作为日志记录工具  

            NLog.Web.NLogBuilder.ConfigureNLog("nlog.config");
            app.UseStatusCodePages();//把错误码返回前台，比如是404

            app.UseRouting();
            app.UseCors(MyAllowSpecificOrigins);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireCors(MyAllowSpecificOrigins);
                endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
