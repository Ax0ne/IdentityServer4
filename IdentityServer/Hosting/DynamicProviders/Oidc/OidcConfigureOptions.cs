// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;

namespace IdentityServer4.Hosting.DynamicProviders;

class OidcConfigureOptions : ConfigureAuthenticationOptions<OpenIdConnectOptions, OidcProvider>
{
    public OidcConfigureOptions(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
    }

    protected override void Configure(ConfigureAuthenticationContext<OpenIdConnectOptions, OidcProvider> context)
    {
        context.AuthenticationOptions.SignInScheme = context.DynamicProviderOptions.SignInScheme;
        context.AuthenticationOptions.SignOutScheme = context.DynamicProviderOptions.SignOutScheme;

        context.AuthenticationOptions.Authority = context.IdentityProvider.Authority;
        context.AuthenticationOptions.RequireHttpsMetadata = context.IdentityProvider.Authority.StartsWith("https");
            
        context.AuthenticationOptions.ClientId = context.IdentityProvider.ClientId;
        context.AuthenticationOptions.ClientSecret = context.IdentityProvider.ClientSecret;

        context.AuthenticationOptions.ResponseType = context.IdentityProvider.ResponseType;
        context.AuthenticationOptions.ResponseMode =
            context.IdentityProvider.ResponseType.Contains("id_token") ? "form_post" : "query";
        context.AuthenticationOptions.UsePkce = context.IdentityProvider.UsePkce;

        context.AuthenticationOptions.Scope.Clear();
        foreach (var scope in context.IdentityProvider.Scopes)
        {
            context.AuthenticationOptions.Scope.Add(scope);
        }

        context.AuthenticationOptions.SaveTokens = true;
        context.AuthenticationOptions.GetClaimsFromUserInfoEndpoint = context.IdentityProvider.GetClaimsFromUserInfoEndpoint;
        context.AuthenticationOptions.DisableTelemetry = true;
#if NET5_0_OR_GREATER
        context.AuthenticationOptions.MapInboundClaims = false;
#else
            context.AuthenticationOptions.SecurityTokenValidator = new JwtSecurityTokenHandler 
            {
                MapInboundClaims = false
            };
#endif
        context.AuthenticationOptions.TokenValidationParameters.NameClaimType = JwtClaimTypes.Name;
        context.AuthenticationOptions.TokenValidationParameters.RoleClaimType = JwtClaimTypes.Role;
            
        context.AuthenticationOptions.CallbackPath = context.PathPrefix + "/signin";
        context.AuthenticationOptions.SignedOutCallbackPath = context.PathPrefix + "/signout-callback";
        context.AuthenticationOptions.RemoteSignOutPath = context.PathPrefix + "/signout";
    }
}