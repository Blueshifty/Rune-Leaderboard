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
    public abstract class RegisterService
    {
        public class RegisterRequest
        {
            public string Username { get; set; } = default!;
            public string Password { get; set; } = default!;
            public string DeviceId { get; set; } = default!;
        }

        public class RegisterResponse
        {
            public string Token { get; set; } = default!;
        }


        public class RegisterRequestHandler
        {
            private readonly ConfigurationOptions.PostgresSqlOptions _postgreSqlOptions;
            private readonly JwtTokenService _jwtTokenService;
            private readonly UserPasswordHashingService _userPasswordHashingService;

            public RegisterRequestHandler(
                IOptions<ConfigurationOptions.PostgresSqlOptions> postgreSqlOptions,
                JwtTokenService jwtTokenService,
                UserPasswordHashingService userPasswordHashingService)
            {
                _postgreSqlOptions = postgreSqlOptions.Value;
                _jwtTokenService = jwtTokenService;
                _userPasswordHashingService = userPasswordHashingService;
            }

            public async Task<DataResult<RegisterResponse>> HandleAsync(RegisterRequest request)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.DeviceId))
                        return DataResult<RegisterResponse>.InvalidRequest();

                    using var connection = new NpgsqlConnection(_postgreSqlOptions.ConnectionString);

                    var sql = @"SELECT * FROM ""Players"" WHERE ""Username"" = @Username";

                    var existingPlayer = await connection.QueryFirstOrDefaultAsync<Player>(sql, new { Username = request.Username });

                    if (existingPlayer != null)
                        return new DataResult<RegisterResponse>(message: "Username already exists", status: ResultStatus.RequestInvalid);

                    _userPasswordHashingService.CreatePasswordHash(request.Password, out var passwordHash, out var passwordSalt);

                    sql = @"INSERT INTO ""Players"" (""Username"", ""DeviceId"",  ""PasswordSalt"", ""PasswordHash"") VALUES(@Username, @DeviceId, @PasswordSalt, @PasswordHash)";

                    await connection.ExecuteAsync(sql, new { Username = request.Username, DeviceId = request.DeviceId, PasswordSalt = passwordSalt, PasswordHash = passwordHash });

                    var token = _jwtTokenService.CreateAccessToken(new Player
                    {
                        Username = request.Username,
                        DeviceId = request.DeviceId,
                    },
                    string.Empty); // Dont have time to implement refresh token logic

                    return new DataResult<RegisterResponse>(data: new RegisterResponse { Token = token.AccessToken });
                }
                catch (Exception ex)
                {
                    Log.Fatal("{message} {stackTrace}", ex.Message, ex.StackTrace);

                    return DataResult<RegisterResponse>.Error();
                }
            }
        }
    }
}
