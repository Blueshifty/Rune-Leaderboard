using Api.Business.Results;
using Api.Business.Services.Leaderboard;
using Api.Business.Utilities.Security.Auth.Jwt;
using Api.Controllers.Base;
using Api.Data.Postgres.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class LeaderboardController : BaseController
    {
        private readonly GetRankByPlayerId.GetRankByPlayerIdRequestHandler _getRankByPlayerIdRequestHandler;
        private readonly GetRankByRangeService.GetRankByRangeRequestHandler _getRankByRangeRequestHandler;
        private readonly MatchResultService.MatchResultRequestHandler _matchResultRequestHandler;
        private readonly DummyLeaderboardService.DummyLeaderboardServiceRequestHandler _dummyLeaderboardServiceRequestHandler;

        private readonly ClaimService _claimService;

        public LeaderboardController(GetRankByPlayerId.GetRankByPlayerIdRequestHandler getRankByPlayerIdRequestHandler,
            GetRankByRangeService.GetRankByRangeRequestHandler getRankByRangeRequestHandler,
            MatchResultService.MatchResultRequestHandler matchResultRequestHandler,
            DummyLeaderboardService.DummyLeaderboardServiceRequestHandler dummyLeaderboardServiceRequestHandler,
            ClaimService claimService)
        {
            _getRankByPlayerIdRequestHandler = getRankByPlayerIdRequestHandler;
            _getRankByRangeRequestHandler = getRankByRangeRequestHandler;
            _matchResultRequestHandler = matchResultRequestHandler;
            _dummyLeaderboardServiceRequestHandler = dummyLeaderboardServiceRequestHandler;

            _claimService = claimService;
        }

        [HttpPost]
        public async Task<ActionResult<DataResult<LeaderboardRanking>>> GetMyRanking()
        {
            var playerId = _claimService.GetPlayerId();

            if (playerId == null)
            {
                return new ObjectResult(new { message = "Not authorized." })
                {
                    StatusCode = 401
                };
            }

            return await _getRankByPlayerIdRequestHandler.HandleAsync(playerId.Value);
        }


        [HttpPost]
        public async Task<ActionResult<DataResult<List<LeaderboardRanking>>>> GetRankByRange(GetRankByRangeService.GetRankByRangeRequest request)
            => await _getRankByRangeRequestHandler.HandleAsync(request);

        [HttpPost]
        public async Task<ActionResult<DataResult<MatchResultService.MatchResultResponse>>> MatchResult(MatchResultService.MatchResultRequest request)
        {
            var playerId = _claimService.GetPlayerId();

            if (playerId == null)
            {
                return new ObjectResult(new { message = "Not authorized." })
                {
                    StatusCode = 401
                };
            }

            return await _matchResultRequestHandler.HandleAsync(request, playerId.Value);
        }

        [HttpPost]
        public async Task<ActionResult<Result>> CreateDummyData(DummyLeaderboardService.DummyLeaderboardServiceRequest request)
            => await _dummyLeaderboardServiceRequestHandler.HandleAsync(request);
    }
}
