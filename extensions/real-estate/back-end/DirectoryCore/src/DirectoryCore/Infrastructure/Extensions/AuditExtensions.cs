using System.Linq;
using Microsoft.Extensions.Logging;

namespace DirectoryCore.Infrastructure.Extensions
{
    public static class AuditExtensions
    {
        /// <summary>
        /// Log an Audit event
        /// </summary>
        /// <param name="logger">The logger through which the audit message will be recorded.</param>
        /// <param name="userId">The user id</param>
        /// <param name="messageTemplate">A log message describing the operation, in message template format.</param>
        /// <param name="args">Arguments to the log message. These will be stored and captured only when the
        /// operation completes, so do not pass arguments that are mutated during the operation.</param>
        public static void Audit(
            this ILogger logger,
            string userId,
            string messageTemplate,
            params object[] args
        )
        {
            using (var logScope = logger.BeginScope("AuditExtensions"))
            {
                messageTemplate = $"{messageTemplate} by {{userId}}";
                args = args.Append(userId).ToArray();
                logger.Log(LogLevel.Information, null, messageTemplate, args);
            }
        }
    }
}
