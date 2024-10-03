using Api.Business.Results;
using Api.Business.Services.Auth;
using Api.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class AuthController : BaseController
    {
        private readonly RegisterService.RegisterRequestHandler _registerRequestHandler;
        private readonly LoginService.LoginRequestHandler _loginRequestHandler;

        public AuthController(
            RegisterService.RegisterRequestHandler registerRequestHandler,
            LoginService.LoginRequestHandler loginRequestHandler
            )
        {
            _registerRequestHandler = registerRequestHandler;
            _loginRequestHandler = loginRequestHandler;
        }

        [HttpPost]
        public async Task<ActionResult<DataResult<RegisterService.RegisterResponse>>> Register(RegisterService.RegisterRequest request)
            => await _registerRequestHandler.HandleAsync(request);

        [HttpPost]
        public async Task<ActionResult<DataResult<LoginService.LoginResponse>>> Login(LoginService.LoginRequest request)
            => await _loginRequestHandler.HandleAsync(request);
    }
}
