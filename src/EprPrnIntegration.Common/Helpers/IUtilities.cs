﻿using EprPrnIntegration.Common.Models;
using EprPrnIntegration.Common.Models.Queues;

namespace EprPrnIntegration.Common.Helpers;

public interface IUtilities
{
    Task<DeltaSyncExecution> GetDeltaSyncExecution(NpwdDeltaSyncType syncType);
    Task SetDeltaSyncExecution(DeltaSyncExecution syncExecution, DateTime latestRun);
    void AddCustomEvent(string eventName, IDictionary<string, string> eventData);
    Task<Stream> CreateErrorEventsCsvStreamAsync(List<ErrorEvent> errorEvents);
}