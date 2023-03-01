// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4.Validation;

/// <summary>
/// Context for backchannel authentication request id validation.
/// </summary>
public class BackchannelAuthenticationRequestIdValidationContext
{
    /// <summary>
    /// Gets or sets the authentication request id.
    /// </summary>
    /// <value>
    /// The device code.
    /// </value>
    public string AuthenticationRequestId { get; set; }

    /// <summary>
    /// Gets or sets the request.
    /// </summary>
    /// <value>
    /// The request.
    /// </value>
    public ValidatedTokenRequest Request { get; set; }

    /// <summary>
    /// Gets or sets the result.
    /// </summary>
    /// <value>
    /// The result.
    /// </value>
    public TokenRequestValidationResult Result { get; set; }
}