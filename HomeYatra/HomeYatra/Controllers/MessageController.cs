using HomeYatra.Interfaces;
using HomeYatra.Models;
using HomeYatra.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeYatra.Controllers
{

    [AllowAnonymous]
    //[EnableCors]
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageProvider _smsProvider;
        public MessageController(IMessageProvider smsProvider)
        {
            _smsProvider = smsProvider;
        }

        [HttpPost("Send")]
        public async Task<ActionResult<MessageResponse>> SendMessage([FromBody] MessageRequest msgRequest)
        {
            var response = await _smsProvider.SendMessageAsync(msgRequest);
            if (response.StatusCode == 200)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
    }
}
