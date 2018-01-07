using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DataEntry.Dao;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DataEntry
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        //public class TestProfileService : IProfileService
        //{
        //    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;
        //    private readonly UserManager<ApplicationUser> _userManager;

        //    public TestProfileService(IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, UserManager<ApplicationUser> userManager)
        //    {
        //        _claimsFactory = claimsFactory;
        //        _userManager = userManager;
        //    }

        //    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        //    {
        //        var sub = context.Subject.GetSubjectId();
        //        var user = await _userManager.FindByIdAsync(sub);
        //        if (user == null)
        //        {
        //            throw new ArgumentException("");
        //        }

        //        var principal = await _claimsFactory.CreateAsync(user);
        //        var claims = principal.Claims.ToList();

        //        context.IssuedClaims = claims;
        //    }

        //    public async Task IsActiveAsync(IsActiveContext context)
        //    {
        //        var sub = context.Subject.GetSubjectId();
        //        var user = await _userManager.FindByIdAsync(sub);
        //        context.IsActive = user != null;
        //    }
        //}

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
            });

            services.AddDbContext<DataEntryDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<DataEntryDBContext>()
                .AddDefaultTokenProviders();

            // The key length needs to be of sufficient length, or otherwise an error will occur.
            var tokenSecretKey = Encoding.UTF8.GetBytes("superlongsecrettokenkeyhere");

            var tokenValidationParameters = new TokenValidationParameters
            {
                // Token signature will be verified using a private key.
                ValidateIssuerSigningKey = true,
                RequireSignedTokens = true,
                IssuerSigningKey = new SymmetricSecurityKey(tokenSecretKey),

                // Token will only be valid if contains "accelist.com" for "iss" claim.
                ValidateIssuer = true,
                ValidIssuer = "dataentry.com",

                // Token will only be valid if contains "accelist.com" for "aud" claim.
                ValidateAudience = true,
                ValidAudience = "dataentry.com",

                // Token will only be valid if not expired yet, with 5 minutes clock skew.
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = new TimeSpan(0, 5, 0),

                ValidateActor = false,
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;
                options.RequireHttpsMetadata = false;
            });

            // Add application services.
            //services.AddTransient<IEmailSender, EmailSender>();
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

            app.UseAuthentication();

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
                context.Database.Migrate();
                var sampleData = new SampleData();
                await sampleData.InitializeAsync(context);
            }
        }

        //protected IEnumerable<Client> GetClients()
        //{
        //    var javascriptClient = new Client
        //    {
        //        ClientId = "js",
        //        ClientSecrets =
        //        {
        //            new Secret("secret".Sha256())
        //        },

        //        ClientName = "JavaScript Client",
        //        AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
        //        AllowAccessTokensViaBrowser = true,

        //        RedirectUris = { "http://localhost/callback.html" },
        //        PostLogoutRedirectUris = { "http://localhost/index.html" },
        //        AllowedCorsOrigins = { "http://localhost" },

        //        AllowedScopes =
        //        {
        //            IdentityServerConstants.StandardScopes.Profile,
        //            IdentityServerConstants.StandardScopes.OpenId
        //        }
        //    };

        //    return new List<Client> { javascriptClient };
        //}

        //public static IEnumerable<IdentityResource> GetIdentityResources()
        //{
        //    return new List<IdentityResource>
        //    {
        //        new IdentityResources.Email(),
        //        new IdentityResources.Profile(),
        //        new IdentityResources.OpenId(),
        //        new IdentityResource
        //        {
        //            Name = JwtClaimTypes.Role,
        //            UserClaims = new List<string> { JwtClaimTypes.Role }
        //        }
        //    };
        //}

        //public static IEnumerable<ApiResource> GetApis()
        //{
        //    return new[]
        //    {
        //        new ApiResource
        //        {
        //            Name = "openid",
        //            Description = "Some API 1",
        //            UserClaims =
        //            {
        //                JwtClaimTypes.Role
        //            }
        //        }
        //    };
        //}
    }
}
