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
using IdentityServer4.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using IdentityServer4.Extensions;

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

        public class TestProfileService : IProfileService
        {
            private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;
            private readonly UserManager<ApplicationUser> _userManager;

            public TestProfileService(IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, UserManager<ApplicationUser> userManager)
            {
                _claimsFactory = claimsFactory;
                _userManager = userManager;
            }

            public async Task GetProfileDataAsync(ProfileDataRequestContext context)
            {
                var sub = context.Subject.GetSubjectId();
                var user = await _userManager.FindByIdAsync(sub);
                if (user == null)
                {
                    throw new ArgumentException("");
                }

                var principal = await _claimsFactory.CreateAsync(user);
                var claims = principal.Claims.ToList();

                //Add more claims like this
                //claims.Add(new System.Security.Claims.Claim("MyProfileID", user.Id));

                context.IssuedClaims = claims;
            }

            public async Task IsActiveAsync(IsActiveContext context)
            {
                var sub = context.Subject.GetSubjectId();
                var user = await _userManager.FindByIdAsync(sub);
                context.IsActive = user != null;
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
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<DataEntryDBContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            //services.AddTransient<IEmailSender, EmailSender>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Audience = "http://localhost:5003/resources";
                options.Authority = "http://localhost:5003/";
                options.RequireHttpsMetadata = false;
                //options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                //{
                //    NameClaimType = JwtClaimTypes.Name,
                //    RoleClaimType = JwtClaimTypes.Role
                //};
            });

            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(GetIdentityResources())
                .AddInMemoryApiResources(GetApis())
                .AddInMemoryClients(GetClients())
                .AddAspNetIdentity<ApplicationUser>()
                .AddProfileService<TestProfileService>();
            //.AddResourceOwnerValidator<TestValidator>();

            

            //services.AddAuthentication(options =>
            //{
            //    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultAuthenticateScheme = OpenIdConnectDefaults.
            //}).AddJwtBearer(options =>
            //{
            //    options.Authority = "http://localhost:5003/";
            //    options.Audience = "resource-server";
            //    options.RequireHttpsMetadata = false;
            //});

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
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            app.UseIdentityServer()
                .UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<DataEntryDBContext>();
                var sampleData = new SampleData();
                await sampleData.InitializeAsync(context);
            }
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
                    IdentityServerConstants.StandardScopes.OpenId
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
                new IdentityResources.OpenId(),
                new IdentityResource
                {
                    Name = JwtClaimTypes.Role,
                    UserClaims = new List<string> { JwtClaimTypes.Role }
                }
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new[]
            {
                new ApiResource
                {
                    Name = "openid",
                    Description = "Some API 1",
                    UserClaims =
                    {
                        JwtClaimTypes.Role
                    }
                }
            };
        }
    }
}
