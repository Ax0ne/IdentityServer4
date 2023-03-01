// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace IdentityServer4.Configuration;

/// <summary>
/// Cookie configuration for the user session plumbing
/// </summary>
public class PostConfigureApplicationCookieTicketStore : IPostConfigureOptions<CookieAuthenticationOptions>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _scheme;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <param name="identityServerOptions"></param>
    /// <param name="options"></param>
    public PostConfigureApplicationCookieTicketStore(IHttpContextAccessor httpContextAccessor, IdentityServerOptions identityServerOptions, IOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions> options)
    {
        _httpContextAccessor = httpContextAccessor;

        _scheme = identityServerOptions.Authentication.CookieAuthenticationScheme ??
             options.Value.DefaultAuthenticateScheme ??
             options.Value.DefaultScheme;
    }

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <param name="scheme"></param>
    public PostConfigureApplicationCookieTicketStore(IHttpContextAccessor httpContextAccessor, string scheme)
    {
        _httpContextAccessor = httpContextAccessor;
        _scheme = scheme;
    }

    /// <inheritdoc />
    public void PostConfigure(string name, CookieAuthenticationOptions options)
    {
        if (name == _scheme)
        {
            LicenseValidator.ValidateServerSideSessions();

            var sessionStore = _httpContextAccessor.HttpContext!.RequestServices.GetService<IServerSideSessionStore>();
            if (sessionStore is InMemoryServerSideSessionStore)
            {
                var logger = _httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("IdentityServer4.Startup");
                logger.LogInformation("You are using the in-memory version of the user session store. This will store user authentication sessions server side, but in memory only. If you are using this feature in production, you want to switch to a different store implementation.");
            }

            options.SessionStore = new TicketStoreShim(_httpContextAccessor);
        }
    }
}

/// <summary>
/// this shim class is needed since ITicketStore is not configured in DI, rather it's a property 
/// of the cookie options and coordinated with PostConfigureApplicationCookie. #lame
/// https://github.com/aspnet/AspNetCore/issues/6946 
/// </summary>
internal class TicketStoreShim : ITicketStore
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    public TicketStoreShim(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// The inner
    /// </summary>
    private IServerSideTicketStore Inner => _httpContextAccessor.HttpContext!.RequestServices.GetRequiredService<IServerSideTicketStore>();

    /// <inheritdoc />
    public Task RemoveAsync(string key)
    {
        return Inner.RemoveAsync(key);
    }

    /// <inheritdoc />
    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        return Inner.RenewAsync(key, ticket);
    }

    /// <inheritdoc />
    public Task<AuthenticationTicket> RetrieveAsync(string key)
    {
        return Inner.RetrieveAsync(key);
    }

    /// <inheritdoc />
    public Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        return Inner.StoreAsync(ticket);
    }
}
