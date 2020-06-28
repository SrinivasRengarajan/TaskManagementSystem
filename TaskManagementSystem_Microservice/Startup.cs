using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementSystem_Microservice.DBContexts;
using TaskManagementSystem_Microservice.Repository;

namespace TaskManagementSystem_Microservice
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. This method is used to add services to the container.
        /// For Dependency Injection Design Pattern (Resolving Dependencies at the rutime)
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            //Registering the DB Context and configuring it to connect to MS Sql Server 2019 with the ConnectionString provided
            //Thereby resolving the DBContext dependency at run time.
            services.AddDbContext<TaskContext>(o => o.UseSqlServer(Configuration.GetConnectionString("TaskDB")));

            //Registering the Repository Service and its implementation, thereby resolving the repository's dependency at runtime
            //Transient Lifetime is used which gives a new instance of the service each time it is requested.
            //This would work best for lightweight services.
            services.AddTransient<ITaskRepository, TaskRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //For showing error messages with status codes redirecting to error page
            app.UseStatusCodePagesWithReExecute("/error/{0}");

            //Handles the exceptions by catching them, redirecting to error page
            app.UseExceptionHandler("/error/500");

            app.UseHttpsRedirection();

            //For making routing decisions with the help of HTTP Endpoint and HTTP Context
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                //Adding endpoints for API Controller actions
                endpoints.MapControllers();
            });
        }
    }
}
