using Microsoft.Extensions.Logging;

namespace TaskUtils.Models;

public static class StaticLogger
{
    internal static ILogger logger = StaticLogger.GetLogger();

    public static ILogger GetLogger()
    {
        return LoggerFactory.Create((Action<ILoggingBuilder>) (builder => builder.AddConsole())).CreateLogger(string.Empty);
    }
}