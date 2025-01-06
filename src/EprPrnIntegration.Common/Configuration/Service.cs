﻿using System.Diagnostics.CodeAnalysis;

namespace EprPrnIntegration.Common.Configuration;

[ExcludeFromCodeCoverage]
public class Service
{
    public string? AccountBaseUrl { get; set; }
    public string? AccountEndPointName { get; set; }
    public string? BearerToken { get; set; }
    public string? HttpClientName { get; set; }
    public int? Retries { get; set; }
    public string? PrnBaseUrl { get; set; }
    public string? PrnEndPointName { get; set; }
}