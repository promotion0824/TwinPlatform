using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using Willow.Common;

namespace Willow.Logging
{
    public static class LoggerExtensions
    {
        public static void LogError(this ILogger logger, string message, Exception ex, object properties)
        {
            InternalLog(logger, LogLevel.Error, message, ex, properties);
        }

        public static void LogWarning(this ILogger logger, string message, Exception ex, object properties)
        {
            InternalLog(logger, LogLevel.Warning, message, ex, properties);
        }
        
        public static void LogCritical(this ILogger logger, string message, Exception ex, object properties)
        {
            InternalLog(logger, LogLevel.Critical, message, ex, properties);
        }

        public static void LogInformation(this ILogger logger, string message, object properties)
        {
            InternalLog(logger, LogLevel.Information, message, null, properties);
        }

        #region Private

        private static void InternalLog(ILogger log, LogLevel level, string message, Exception ex, object properties)
        {
            var props = properties?.ToDictionary() ?? new Dictionary<string, object>();
                
            props["Log Level"] = level.ToString();

            // Add exception data to log's custom properties
            AddData(ex, props);

            log.Log<IDictionary<string, object>>(level, 0, props, ex, (state, exF)=>
            {
                if(String.IsNullOrWhiteSpace(message) && exF != null)
                    return exF.Message;

                return message;
            });
        } 

        private static void AddData(Exception ex, IDictionary<string, object> props)
        {
            if(ex != null)
            { 
                foreach(var key in ex.Data.Keys)
                    props[key.ToString()] = ex.Data[key];

                if(ex.InnerException != null)
                {
                    if(ex.InnerException is AggregateException aggrEx)
                    {
                        foreach(var exInner in aggrEx.InnerExceptions)
                            AddData(exInner, props); 
                    }
                    else
                       AddData(ex.InnerException, props); 
                }
            }
        } 

        #endregion
    }
}
