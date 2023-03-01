// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer4.Hosting.DynamicProviders;

/// <summary>
/// Caching decorator for IIdentityProviderStore
/// </summary>
/// <typeparam name="T"></typeparam>
public class CachingIdentityProviderStore<T> : IIdentityProviderStore
    where T : IIdentityProviderStore
{
    private readonly IIdentityProviderStore _inner;
    private readonly ICache<IdentityProvider> _cache;
    private readonly ICache<IEnumerable<IdentityProviderName>> _allCache;
    private readonly IdentityServerOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="inner"></param>
    /// <param name="cache"></param>
    /// <param name="allCache"></param>
    /// <param name="options"></param>
    /// <param name="httpContextAccessor"></param>
    public CachingIdentityProviderStore(T inner, 
        ICache<IdentityProvider> cache,
        ICache<IEnumerable<IdentityProviderName>> allCache,
        IdentityServerOptions options,
        IHttpContextAccessor httpContextAccessor)
    {
        _inner = inner;
        _cache = cache;
        _allCache = allCache;
        _options = options;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<IdentityProviderName>> GetAllSchemeNamesAsync()
    {
                
        var result = await _allCache.GetOrAddAsync("__all__", 
            _options.Caching.IdentityProviderCacheDuration, 
            async () => await _inner.GetAllSchemeNamesAsync());
        return result;
    }

    /// <inheritdoc/>
    public async Task<IdentityProvider> GetBySchemeAsync(string scheme)
    {
                
        var result = await _cache.GetOrAddAsync(scheme,
            _options.Caching.IdentityProviderCacheDuration,
            async () =>
            {
                var item = await _inner.GetBySchemeAsync(scheme);
                RemoveCacheEntry(item);
                return item;
            });
        return result;
    }

    // when items are re-added, we remove the corresponding options from the 
    // options monitor since those instances are cached my the authentication handler plumbing
    // this keeps theirs in sync with ours when we re-load from the DB
    void RemoveCacheEntry(IdentityProvider idp)
    {
        if (idp != null)
        {
            var provider = _options.DynamicProviders.FindProviderType(idp.Type);
            if (provider != null)
            {
                var optionsMonitorType = typeof(IOptionsMonitorCache<>).MakeGenericType(provider.OptionsType);
                // need to resolve the provide type dynamically, thus the need for the http context accessor
                var optionsCache = _httpContextAccessor.HttpContext.RequestServices.GetService(optionsMonitorType);
                if (optionsCache != null)
                {
                    var mi = optionsMonitorType.GetMethod("TryRemove");
                    if (mi != null)
                    {
                        mi.Invoke(optionsCache, new[] { idp.Scheme });
                    }
                }
            }
        }
    }
}