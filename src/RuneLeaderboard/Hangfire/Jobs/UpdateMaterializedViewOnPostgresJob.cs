using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using Serilog;

namespace Api.Hangfire.Jobs;

public class UpdateMaterializedViewOnPostgresJob
{

    private readonly ConfigurationOptions.PostgresSqlOptions _postgreSqlOptions;

    public UpdateMaterializedViewOnPostgresJob(IOptions<ConfigurationOptions.PostgresSqlOptions> postgreSqlOptions)
    {
        _postgreSqlOptions = postgreSqlOptions.Value;
    }

    public async Task RunAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(_postgreSqlOptions.ConnectionString);

            await connection.OpenAsync();

            var sql = @"REFRESH MATERIALIZED VIEW CONCURRENTLY ""LeaderboardRankingsMat""";

            await connection.ExecuteAsync(sql);
        }
        catch (Exception ex)
        {
            Log.Fatal("{ message} {stackTrace}", ex.Message, ex.StackTrace);
        }
    }
}
