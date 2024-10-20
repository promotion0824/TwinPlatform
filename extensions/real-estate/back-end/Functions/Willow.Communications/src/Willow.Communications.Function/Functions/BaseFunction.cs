using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Willow.Common;
using Willow.Communications.Function.Exceptions;

namespace Willow.Communications.Function.Functions;
public class BaseFunction
{
    protected static Task Log(object? message, string functionName, FunctionContext executionContext, string type)
    {
        var log = GetLogger(functionName, executionContext);

        return Invoke(message, functionName, executionContext, type, log, () =>
        {
            return Task.CompletedTask;
        });
    }

    protected static Task Invoke<TINPUT>(string input, string functionName, FunctionContext executionContext, Func<TINPUT?, ILogger?, Task> doWork) where TINPUT : class
    {
        var log = GetLogger(functionName, executionContext);

        if (!TryDeserialize(input, log, out TINPUT? message))
        {
            return Task.CompletedTask;
        }

        return Invoke(message, functionName, executionContext, typeof(TINPUT).Name, log, () =>
        {
            return doWork(message, log);
        });
    }

    protected static async Task<TOUTPUT?> Invoke<TINPUT, TOUTPUT>(string input, string functionName, FunctionContext executionContext, Func<TINPUT?, ILogger?, Task<TOUTPUT?>> doWork) where TOUTPUT : class where TINPUT : class
    {
        var log = GetLogger(functionName, executionContext);

        if (!TryDeserialize(input, log, out TINPUT? message))
        {
            return null;
        }

        TOUTPUT? result = null;

        await Invoke(message, functionName, executionContext, typeof(TINPUT).Name, log, async () =>
        {
            result = await doWork(message, log);
        });

        return result;
    }


    #region Private

    private static async Task Invoke(object? message, string functionName, FunctionContext executionContext, string type, ILogger? log, Func<Task> doWork)
    {
        var logProperties = message?.ToDictionary();

        logProperties?.Remove("Data");
        logProperties?.Add("FunctionName", functionName);

        try
        {
            logProperties?.Add("RetryCount", executionContext?.RetryContext?.RetryCount);
            logProperties?.Add("MaxRetryCount", executionContext?.RetryContext?.MaxRetryCount);
        }
        catch
        {
            // Possible internal error
        }

        using var y = log?.BeginScope(logProperties);

        log?.LogInformation($"Start {functionName}.Run", logProperties);

        try
        {
            await doWork();
        }
        catch (Exception ex)
        {
            log?.LogError($"Error in processing {type} message", ex, logProperties);
            throw;
        }

        log?.LogInformation($"End {functionName}.Run", logProperties);
    }
 
    private static ILogger? GetLogger(string functionName, FunctionContext executionContext)
    {
        ILogger? log = null;

        try
        {
            log = executionContext.GetLogger(functionName);
        }
        catch
        {
            // No logger. Happens in unit tests.
        }

        return log;
    }

    private static bool TryDeserialize<TINPUT>(string input, ILogger? log, out TINPUT? result) where TINPUT : class
    {
        try
        {
            result = JsonConvert.DeserializeObject<TINPUT>(input);
            return Validate(result, log);
        }
        catch (Exception ex)
        {
            log?.LogError(ex, "Unable to deserialize {inputName}", typeof(TINPUT).Name);

            throw;
        }
    }

    private static bool Validate<TINPUT>(TINPUT? result, ILogger? log)
    {
        var errors = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(result!, new ValidationContext(result!, null, null), errors, true);

        if (!isValid)
        {
            log?.LogError("Validation failed for {inputName}", typeof(TINPUT).Name);

            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    log?.LogError(error.ErrorMessage);
                }
            }

            throw new InvalidMessageException(errors, $"{typeof(TINPUT).Name}");
        }
        return isValid;
    }

    #endregion
}
