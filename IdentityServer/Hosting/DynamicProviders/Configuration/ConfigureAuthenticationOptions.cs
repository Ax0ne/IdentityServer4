// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IdentityServer4.Hosting.DynamicProviders;

/// <summary>
/// Helper class for configuring authentication options from a dynamic identity provider
/// </summary>
/// <typeparam name="TAuthenticationOptions"></typeparam>
/// <typeparam name="TIdentityProvider"></typeparam>
public abstract class ConfigureAuthenticationOptions<TAuthenticationOptions, TIdentityProvider> : IConfigureNamedOptions<TAuthenticationOptions>
    where TAuthenticationOptions : AuthenticationSchemeOptions
    where TIdentityProvider : IdentityProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    public ConfigureAuthenticationOptions(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public void Configure(string name, TAuthenticationOptions options)
    {
        // we have to resolve these here due to DI lifetime issues
        var providerOptions = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<DynamicProviderOptions>();
        var cache = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<DynamicAuthenticationSchemeCache>();
            
        var idp = cache.GetIdentityProvider<TIdentityProvider>(name);
        if (idp != null)
        {
            var pathPrefix = providerOptions.PathPrefix + "/" + idp.Scheme;
            var ctx = new ConfigureAuthenticationContext<TAuthenticationOptions, TIdentityProvider>
            {
                IdentityProvider = idp,
                AuthenticationOptions = options,
                DynamicProviderOptions = providerOptions,
                PathPrefix = pathPrefix
            };
            Configure(ctx);
        }
    }

    /// <summary>
    /// Allows for configuring the handler options from the identity provider configuration.
    /// </summary>
    /// <param name="context"></param>
    protected abstract void Configure(ConfigureAuthenticationContext<TAuthenticationOptions, TIdentityProvider> context);

    /// <inheritdoc/>
    public void Configure(TAuthenticationOptions options)
    {
    }
}