using System;
using System.Collections.Generic;
using System.Text;
using Durwella.UrlShortening.Web.ServiceInterface;
using Durwella.UrlShortening.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Durwella.UrlShortening.Web
{
    public class Startup: IProtectedPathList
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddScoped((_) => Configuration);
            services.AddScoped<IConfigSettings, ConfigSettings>();
            services.AddScoped<IAliasRepository>((_) => 
                new AzureTableAliasRepository(Configuration["ConnectionStrings:AzureStorage"])
                {
                    LockAge = TimeSpan.FromMinutes(Configuration.GetValue("LockUrlMinutes", 12*60))
                });
            services.AddScoped<IProtectedPathList>((_) => this);
            services.AddScoped<IHashScheme, Sha1Base64Scheme>();
            services.AddScoped<IUrlUnwrapper, WebClientUrlUnwrapper>();
            services.AddScoped<IUrlShorteningService, UrlShorteningService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserRepository>((_) =>
                new BasicAdminCredentialUserRepository(Configuration["AdminPassword"]));

            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["Auth:Secret"]));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                // iss
                ValidateIssuer = true,
                ValidIssuer = Configuration["Auth:Issuer"],

                // aud
                ValidateAudience = true,
                ValidAudience = Configuration["Auth:Audience"],

                // validate expiry
                ValidateLifetime = true,

                ClockSkew = TimeSpan.FromMinutes(1)
            };

            services
                .AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = tokenValidationParameters;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc();
        }

        public IList<string> ProtectedPaths => new List<string>()
        {
            "admin",
            "auth/validate",
            "auth/cred"
        };
    }
}
