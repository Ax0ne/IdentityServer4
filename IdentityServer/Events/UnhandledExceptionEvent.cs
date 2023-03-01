// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;

namespace IdentityServer4.Events;

/// <summary>
/// Event for unhandled exceptions
/// </summary>
/// <seealso cref="Event" />
public class UnhandledExceptionEvent : Event
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnhandledExceptionEvent"/> class.
    /// </summary>
    /// <param name="ex">The ex.</param>
    public UnhandledExceptionEvent(Exception ex)
        : base(EventCategories.Error,
            "Unhandled Exception",
            EventTypes.Error,
            EventIds.UnhandledException,
            ex.Message)
    {
        Exception = ex;
        Details = ex.ToString();
    }

    /// <summary>
    /// Gets or sets the details.
    /// </summary>
    /// <value>
    /// The details.
    /// </value>
    public string Details { get; set; }

    /// <summary>
    /// Gets or sets the exception.
    /// </summary>
    /// <value>
    /// The exception.
    /// </value>
    public Exception Exception { get; set; }
}