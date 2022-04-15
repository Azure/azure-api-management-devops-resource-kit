// --------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  Licensed under the MIT License.
// --------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common
{
    /// <summary>
    /// Helper class for logging any actions. 
    /// You can setup logger passing Microsoft.Extensions.Logging.ILogger that works the best for you.
    /// </summary>
    public static class Logger
    {
        static ILogger InternalLogger;

        /// <summary>
        /// Specify which logger to use for internal apimtemplate library messaging
        /// </summary>
        /// <param name="logger">end-user logger</param>
        public static void SetupLogger(ILogger logger)
        {
            InternalLogger = logger;
        }

        public static void LogInformation(string message, params object[] args)
        {
            InternalLogger?.LogInformation(message, args);
        }

        public static void LogWarning(Exception exception, string message, params object[] args)
        {
            InternalLogger?.LogWarning(exception, message, args);
        }

        public static void LogWarning(string message, params object[] args)
        {
            InternalLogger?.LogWarning(message, args);
        }

        public static void LogError(string message, params object[] args)
        {
            InternalLogger?.LogError(message, args);
        }

        public static void LogError(Exception exception, string message, params object[] args)
        {
            InternalLogger?.LogError(exception, message, args);
        }
    }
}