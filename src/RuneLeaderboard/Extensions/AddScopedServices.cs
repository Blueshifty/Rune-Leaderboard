using Api.Business.Services.Auth;
using Api.Business.Services.Leaderboard;
using Api.Business.Utilities.Security.Auth.Jwt;
using Api.Data.Redis;

namespace Api.Extensions;
public static class AddScopedServices
{
    public static void AddMyScoped(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<LoginService.LoginRequestHandler>();
        serviceCollection.AddScoped<RegisterService.RegisterRequestHandler>();
        serviceCollection.AddScoped<GetRankByPlayerId.GetRankByPlayerIdRequestHandler>();
        serviceCollection.AddScoped<GetRankByRangeService.GetRankByRangeRequestHandler>();
        serviceCollection.AddScoped<MatchResultService.MatchResultRequestHandler>();
        serviceCollection.AddScoped<MatchResultEncryptedService.MatchResultRequestHandler>();
        serviceCollection.AddScoped<DummyLeaderboardService.DummyLeaderboardServiceRequestHandler>();


        serviceCollection.AddScoped<RedisLeaderboardService>();
        serviceCollection.AddScoped<ClaimService>();
    }
}
