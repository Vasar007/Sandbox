using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sandbox.TelegramBot.Services;
using Telegram.Bot.Types;

namespace Sandbox.TelegramBot.Controllers;

//[Route("api/[controller]")]
//[ApiController]
public sealed class WebhookController : ControllerBase
{
    public WebhookController()
    {
    }

    [HttpPost]
    public async Task<IActionResult> Post(
        [FromServices] HandleUpdateService handleUpdateService,
        [FromBody] Update update)
    {
        await handleUpdateService.EchoAsync(update);
        return Ok();
    }
}
