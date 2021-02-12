using System;
using System.Security.Claims;
using System.Threading.Tasks;
using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Spawn.Demo.Logging;
using Spawn.Demo.Store;

namespace Spawn.Demo.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var formatProvider = new TemplateTextFormatter();

            ILogger logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(formatProvider)
                .CreateLogger();
            Log.Logger = logger;

            services
                .AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
                .AddBasic(options =>
                {
                    options.Realm = "spawn-demo";
                    options.AllowInsecureProtocol = true;
                    options.Events = new BasicAuthenticationEvents
                    {
                        OnValidateCredentials = context =>
                        {
                            var claims = new[]
                            {
                                new Claim(ClaimTypes.NameIdentifier, context.Username, ClaimValueTypes.String),
                                new Claim(ClaimTypes.Name, context.Username, ClaimValueTypes.String)
                            };
                            
                            context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                            context.Success();
                            
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();

            services
                .AddMvc(config =>
                {
                    config.Filters.Add(new AuthorizeFilter());
                });

            var todoConnString = BuildTodoConnectionString();
            var accountConnString = BuildAccountConnectionString();

            services
                .AddSingleton(Log.Logger)
                .AddScoped(_ => new TodoConnectionService(todoConnString, bool.Parse(Configuration["DatabasePooling"])))
                .AddScoped(_ => new AccountConnectionService(accountConnString, bool.Parse(Configuration["DatabasePooling"])))
                .AddScoped<IAccountStore, AccountStore>()
                .AddScoped<IOrganizationStore, OrganizationStore>()
                .AddScoped<IProjectStore, ProjectStore>()
                .AddScoped<ITodoStore, TodoStore>()
                .AddCors(options =>
                {
                    options.AddPolicy("AllowLocalSpawnDemoClient",
                        builder => builder
                            .SetIsOriginAllowed(_ => true)
                            .AllowCredentials()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                    );
                });
        }

        private string BuildTodoConnectionString()
        {
            var connectionString = Configuration["TodoDatabaseConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString),"Todo database connection string was null");
            }
            return connectionString;
        }

        private string BuildAccountConnectionString()
        {
            var connectionString = Configuration["AccountDatabaseConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString),"Account database connection string was null");
            }
            return connectionString;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors("AllowLocalSpawnDemoClient");

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseRouting();
            app.UseEndpoints(routeBuilder => routeBuilder.MapControllers());
        }
    }
}