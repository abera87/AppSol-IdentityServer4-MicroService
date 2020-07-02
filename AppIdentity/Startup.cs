// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using AppIdentity.Data.Identity;
using Microsoft.AspNetCore.Identity;
using IdentityServer4.EntityFramework.DbContexts;
using System.Linq;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Services;

namespace AppIdentity
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var dbConnectrionString = Configuration.GetConnectionString("AppIdentityAuthServerDB");
            var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<ApplicationDBContext>(builder =>
                                builder.UseSqlServer(dbConnectrionString,
                                sqlOptions => sqlOptions.MigrationsAssembly(migrationAssembly)));

            services.AddIdentity<IdentityUser, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDBContext>();



            // uncomment, if you want to add an MVC-based UI
            services.AddControllersWithViews();

            var builder = services.AddIdentityServer(options =>
            {
                // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                options.EmitStaticAudienceClaim = true;
            })
            /*
            this time replacing the calls to AddInMemoryClients, AddInMemoryIdentityResources, AddInMemoryApiScopes, and AddInMemoryApiResources
            */
                .AddOperationalStore(options =>
                    options.ConfigureDbContext = dbBuilder =>
                    dbBuilder.UseSqlServer(
                        dbConnectrionString,
                        sqlOptions => sqlOptions.MigrationsAssembly(migrationAssembly)))
                .AddConfigurationStore(options =>
                    options.ConfigureDbContext = dbBuilder =>
                    dbBuilder.UseSqlServer(
                        dbConnectrionString,
                        sqlOptions => sqlOptions.MigrationsAssembly(migrationAssembly)))
                .AddAspNetIdentity<IdentityUser>();


            // // in memory configuration
            // builder.AddInMemoryIdentityResources(Config.IdentityResources)
            //         .AddInMemoryApiResources(Config.ApiResources)
            //         .AddInMemoryApiScopes(Config.ApiScopes)
            //         .AddInMemoryClients(Config.Clients)
            //         .AddTestUsers(Config.GetTestUsers);


            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

            //services.AddTransient<IProfileService, IdentityClaimsProfileService>();
            
            // this is required to allow account/register api call from angular
            services.AddCors();

        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // feed seed data from Config class
            InitializeDbTestData(app);

            app.UseRouting();

            app.UseIdentityServer();
            // uncomment if you want to add MVC
            app.UseStaticFiles();

            // this is required to allow account/register api call from angular else client origin added from config file client section
            app.UseCors(policy =>
                    {
                        policy.WithOrigins("http://localhost:4200");
                        policy.AllowAnyHeader();
                        policy.AllowAnyMethod();
                        policy.AllowCredentials();
                    });

            // uncomment, if you want to add MVC
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }



        private static void InitializeDbTestData(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();
                serviceScope.ServiceProvider.GetRequiredService<ApplicationDBContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

                if (!context.Clients.Any())
                {
                    foreach (var client in Config.Clients)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.IdentityResources)
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiScopes.Any())
                {
                    foreach (var scope in Config.ApiScopes)
                    {
                        context.ApiScopes.Add(scope.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.ApiResources)
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }


                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                if (!userManager.Users.Any())
                {
                    foreach (var testUser in Config.GetTestUsers)
                    {
                        var identityUser = new IdentityUser(testUser.Username)
                        {
                            Id = testUser.SubjectId
                        };

                        userManager.CreateAsync(identityUser, "Password123!").Wait();
                        userManager.AddClaimsAsync(identityUser, testUser.Claims.ToList()).Wait();
                    }
                }

            }
        }
    }
}
