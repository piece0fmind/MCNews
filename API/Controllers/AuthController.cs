
using API.Features.Auth.Command;
using Application.Features.Auth.RefreshToken.Command;
using Application.Features.Auth.RevokeToken.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IMediator _mediator;
        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            try
            {
                var loginResponse = await _mediator.Send(command);
                return Ok(loginResponse);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
                throw;
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenCommand refreshTokenRequest)
        {
            var refreshTokenResponse = await _mediator.Send(refreshTokenRequest);
            return Ok(refreshTokenResponse);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> RevokeToken(RevokeTokenCommand revokeTokenRequest)
        {
            var revokeResponse = await _mediator.Send(revokeTokenRequest);
            return Ok(revokeResponse);
        }
    }
}
