using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.Models;
using IdentityServer4;
using IdentityModel;
using IdentityServer4.Validation;
using IdentityServer4.Configuration;
using Microsoft.EntityFrameworkCore.Internal;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DataEntry.Dao;

namespace DataEntry
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public class TestValidator : IResourceOwnerPasswordValidator
        {
            public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
            {
                if (context.UserName == "test" && context.Password == "test")
                {
                    context.Result = new GrantValidationResult(
                        subject: "818727",
                        authenticationMethod: "custom");
                }
                else
                {
                    context.Result = new GrantValidationResult(
                        TokenRequestErrors.InvalidGrant,
                        "invalid custom credential");
                }

                return;
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            //services.AddMvcCore()
            //    .AddAuthorization()
            //    .AddJsonFormatters();

            services.AddDbContext<DataEntryDBContext>(options =>
                options.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=DataEntry;Trusted_Connection=True;"));
            //Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<DataEntryDBContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            //services.AddTransient<IEmailSender, EmailSender>();

            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(GetIdentityResources())
                .AddInMemoryApiResources(GetApis())
                .AddInMemoryClients(GetClients())
                .AddAspNetIdentity<ApplicationUser>()
                .AddResourceOwnerValidator<TestValidator>();

            //services.AddCors(options =>
            //{
            //    // this defines a CORS policy called "default"
            //    options.AddPolicy("default", policy =>
            //    {
            //        policy.WithOrigins("http://localhost:5003")
            //            .AllowAnyHeader()
            //            .AllowAnyMethod();
            //    });
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    ReactHotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseIdentityServer();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }

        protected IEnumerable<Client> GetClients()
        {
            var javascriptClient = new Client
            {
                ClientId = "js",
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },

                ClientName = "JavaScript Client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                AllowAccessTokensViaBrowser = true,

                RedirectUris = { "http://localhost:5003/callback.html" },
                PostLogoutRedirectUris = { "http://localhost:5003/index.html" },
                AllowedCorsOrigins = { "http://localhost:5003" },

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.Profile,
                    "api1"
                }
            };

            return new List<Client> { javascriptClient };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.Email(),
                new IdentityResources.Profile(),
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new[]
            {
                new ApiResource("api1", "Some API 1")
            };
        }
    }
}
