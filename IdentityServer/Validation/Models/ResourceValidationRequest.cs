// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System;
using System.Collections.Generic;

namespace IdentityServer4.Validation;

/// <summary>
/// Models the request to validate scopes and resource indicators for a client.
/// </summary>
public class ResourceValidationRequest
{
    /// <summary>
    /// The client.
    /// </summary>
    public Client Client { get; set; }

    /// <summary>
    /// The requested scope values.
    /// </summary>
    public IEnumerable<string> Scopes { get; set; }

    /// <summary>
    /// The requested resource indicators.
    /// </summary>
    public IEnumerable<string> ResourceIndicators { get; set; }

    /// <summary>
    /// Flag that indicates that validation should allow requested scopes to match non-isolated resources.
    /// If set to false, then only the scopes that match the exact resource indicators requested will be allowed.
    /// </summary>
    [Obsolete("IncludeNonIsolatedApiResources is no longer used and will be removed in a future version.")]
    public bool IncludeNonIsolatedApiResources { get; set; }
}