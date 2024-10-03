using Api.Business.Utilities.Security.Auth.Jwt.Model;
using Api.Business.Utilities.Security.Encryption.UserPassword;

namespace Api.Extensions;

public static class AddSingletonServices
{
    public static void AddMySingleton(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<JwtTokenService>();
        serviceCollection.AddSingleton<UserPasswordHashingService>();
    }
}
