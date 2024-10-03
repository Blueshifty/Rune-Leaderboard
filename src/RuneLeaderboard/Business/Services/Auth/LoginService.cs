using Api.Business.Results;
using Api.Business.Utilities.Security.Auth.Jwt.Model;
using Api.Business.Utilities.Security.Encryption.UserPassword;
using Api.Data.Postgres.Models;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using Serilog;

namespace Api.Business.Services.Auth
{
    public abstract class LoginService
    {
        public class LoginRequest
        {
            public string Username { get; set; } = default!;
            public string Password { get; set; } = default!;
        }

        public class LoginResponse
        {
            public string Token { get; set; } = default!;
        }

        public class LoginRequestHandler
        {
            private readonly ConfigurationOptions.PostgresSqlOptions _postgreSqlOptions;
            private readonly UserPasswordHashingService _userPasswordHashingService;
            private readonly JwtTokenService _jwtTokenService;

            public LoginRequestHandler(
                IOptions<ConfigurationOptions.PostgresSqlOptions> postgreSqlOptions,
                UserPasswordHashingService userPasswordHashingService,
                JwtTokenService jwtTokenService)
            {
                _postgreSqlOptions = postgreSqlOptions.Value;
                _userPasswordHashingService = userPasswordHashingService;
                _jwtTokenService = jwtTokenService;
            }

            public async Task<DataResult<LoginResponse>> HandleAsync(LoginRequest request)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                        return DataResult<LoginResponse>.InvalidRequest();

                    using var connection = new NpgsqlConnection(_postgreSqlOptions.ConnectionString);

                    var sql = @"SELECT * FROM ""Players"" WHERE ""Username"" = @Username";

                    var player = await connection.QueryFirstOrDefaultAsync<Player>(sql, new { Username = request.Username });

                    if (player == null || !_userPasswordHashingService.VerifyPasswordHash(request.Password, player.PasswordHash, player.PasswordSalt))
                    {
                        return new DataResult<LoginResponse>(message: "Username or password is wrong", status: ResultStatus.RequestInvalid);
                    }

                    var jwtToken = _jwtTokenService.CreateAccessToken(player, string.Empty); // Dont have time to implement refresh token logic

                    return new DataResult<LoginResponse>(data: new LoginResponse { Token = jwtToken.AccessToken });
                }
                catch (Exception ex)
                {
                    Log.Error("{message} {stackTrace}", ex.Message, ex.StackTrace);

                    return DataResult<LoginResponse>.Error();
                }
            }
        }
    }
}
