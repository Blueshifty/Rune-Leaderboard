namespace Api;

public class ConfigurationOptions
{
    public class JwtOptions
    {
        public const string Jwt = "Jwt";
        public string SecurityKey { get; set; } = default!;
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
    }

    public class PostgresSqlOptions
    {
        public const string PostgreSql = "Postgresql";
        public string ConnectionString { get; set; } = default!;
    }

    public class EncryptionOptions
    {
        public const string Encryption = "Encryption";
        public string AesKey { get; set; } = default!;
    }

    public class AppOptions
    {
        public const string App = "App";
        public string Name { get; set; } = default!;
        public string Environment { get; set; } = default!;
    }

    public class RedisOptions
    {
        public const string Redis = "Redis";
        public string Endpoint { get; set; } = default!;
    }
}
