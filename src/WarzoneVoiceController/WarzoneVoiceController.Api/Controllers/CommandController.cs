using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarzoneVoiceController.Api.Hubs;

namespace WarzoneVoiceController.Api.Controllers
{
    [Route("api/[controller]")]
    public class CommandController : ControllerBase
    {
        private readonly IHubContext<WarzoneHub> _warzoneHub;

        public CommandController(IHubContext<WarzoneHub> warzoneHub)
        {
            _warzoneHub = warzoneHub;
        }

        [HttpPost("{commandName}")]
        public async Task<string> SendCommand(string commandName)
        {
            await _warzoneHub.Clients.All.SendAsync(commandName, null);
            return "Success";
        }
    }
}
