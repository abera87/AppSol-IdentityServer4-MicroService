// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;

namespace AppIdentity
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Email(),
                new IdentityResources.Profile(),
                new IdentityResource{
                    Name = "Role",
                    UserClaims = new List<string>{"role"}
                }
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("CatalogService.read","Read Catalog service"),
                new ApiScope("CatalogService.write","write Catalog Service"),
                new ApiScope("OrderService.read","Read Order service"),
                new ApiScope("OrderService.write","write Order Service")
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            { 
                //client for Client Credentials
                new Client{
                    ClientId = "postman",
                    ClientName = "Postman client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets={
                        new Secret("ScopeSecret".Sha256())
                    },
                    AllowedScopes ={"CatalogService.read","OrderService.read"},
                    AccessTokenLifetime = (30*30) //3600
                },
                // new client for Implicit folow
                new Client{
                    ClientId = "AngularSPA",
                    ClientName="Angular SPA Client",
                    AllowedGrantTypes = GrantTypes.Implicit,

                    RedirectUris = {"http://localhost:4200/signin-oidc"},
                    PostLogoutRedirectUris = {"http://localhost:4200/signout-callback-oidc"},

                    AllowedScopes = new List<string>{"openid", "profile", "email",  "CatalogService.read"},
                    AllowAccessTokensViaBrowser = true,
                    AllowedCorsOrigins ={"http://localhost:4200"},
                    AccessTokenLifetime = 3600
                }
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new ApiResource[]
            {
                new ApiResource{
                    Name ="CatalogService",
                    DisplayName="Catalog Service",
                    Description="Catalog related all api expose here",
                    Scopes=new List<string>{"CatalogService.read","CatalogService.write"},
                    ApiSecrets=new List<Secret>{new Secret("ScopeSecret".Sha256())},
                    UserClaims=new List<string>{"role"}
                },
                new ApiResource{
                    Name ="OrderService",
                    DisplayName="Order Service",
                    Description="Order related all api expose here",
                    Scopes=new List<string>{"OrderService.read","OrderService.write"},
                    ApiSecrets=new List<Secret>{new Secret("ScopeSecret".Sha256())},
                    UserClaims=new List<string>{"role"}
                }
            };

        public static List<IdentityServer4.Test.TestUser> GetTestUsers =>
            new List<IdentityServer4.Test.TestUser>{
                new IdentityServer4.Test.TestUser{
                    SubjectId="User1",
                    Username="user1",
                    Password="1234",
                    Claims = new List<System.Security.Claims.Claim>{
                        new System.Security.Claims.Claim(IdentityModel.JwtClaimTypes.Email, "scott@scottbrady91.com"),
                        new System.Security.Claims.Claim(IdentityModel.JwtClaimTypes.Role, "admin")
                    }
                }
            };
    }
}