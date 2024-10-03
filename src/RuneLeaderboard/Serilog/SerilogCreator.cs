using Serilog;

namespace Api.Serilog;

public static class SerilogLogCreator
{
    public static void CreateLogger(IConfiguration configuration)
    {
        if (configuration["App:Environment"] == "DEV")
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();
        }
        else
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo.File("/home/rune-leaderboard-logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
    }
}

