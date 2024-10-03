using Api.Hangfire.Jobs;
using Hangfire;

namespace Api.Hangfire;

public class ScheduleBackgroundJobs
{
    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<UpdateMaterializedViewOnPostgresJob>("Update Materialized View", j => j.RunAsync(), "*/5 * * * *");
        RecurringJob.AddOrUpdate<UpdateTopRanksOnRedisJob>("Update Top Ranks On Redis", j => j.RunAsync(), "*/5 * * * *");
    }
}
