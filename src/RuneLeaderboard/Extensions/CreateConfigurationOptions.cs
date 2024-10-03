namespace Api.Extensions;

public static class CreateConfigurationOptions
{
    public static void CreateOptions(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<ConfigurationOptions.JwtOptions>(configuration.GetSection(ConfigurationOptions.JwtOptions.Jwt));
        serviceCollection.Configure<ConfigurationOptions.PostgresSqlOptions>(configuration.GetSection(ConfigurationOptions.PostgresSqlOptions.PostgreSql));
        serviceCollection.Configure<ConfigurationOptions.EncryptionOptions>(configuration.GetSection(ConfigurationOptions.EncryptionOptions.Encryption));
        serviceCollection.Configure<ConfigurationOptions.RedisOptions>(configuration.GetSection(ConfigurationOptions.RedisOptions.Redis));
    }
}

