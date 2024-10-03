namespace Api.Constants;

public static class AuthConstants
{
    // Dont have time to implement refresh token logic.
    public const int JwtTokenValidUntilMinutes = 10000;
    public const int RefreshTokenValidUntilDays = 10000;

    public static class JwtClaimNames
    {
        public const string PlayerId = "PlayerId";
        public const string DeviceId = "DeviceId";
    }
}
