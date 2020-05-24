using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WarzoneVoiceController.Api.Hubs;

namespace WarzoneVoiceController.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlexaController : ControllerBase
    {
        private readonly IHubContext<WarzoneHub> _warzoneHub;

        public AlexaController(IHubContext<WarzoneHub> warzoneHub)
        {
            _warzoneHub = warzoneHub;
        }

        [HttpPost("HandleRequest")]
        public SkillResponse HandleRequest([FromBody]SkillRequest skillRequest)
        {
            // if it's Launch request, then say hello and tell the user the commands
            if(skillRequest?.GetRequestType() == typeof(LaunchRequest))
            {
                return ResponseBuilder.Ask("Welcome to the warzone controller, issue a command like 'jump' or 'cut chute'.", null);
            }
            // if it's an intent request, then choose what command based on the name of the intent
            else if (skillRequest?.GetRequestType() == typeof(IntentRequest))
            {
                var intentRequest = skillRequest.Request as IntentRequest;
                _warzoneHub.Clients.All.SendAsync(intentRequest.Intent.Name);
                switch (intentRequest.Intent.Name)
                {
                    case "UseItemIntent": return ResponseBuilder.Tell("You are using an item");
                    case "ArmorIntent": return ResponseBuilder.Tell("You are puting on armor");
                    case "SprintIntent": return ResponseBuilder.Tell("You are sprinting");
                    case "AttackIntent": return ResponseBuilder.Tell("You are attacking");
                    case "ShieldIntent": return ResponseBuilder.Tell("You are using your shield");
                    case "PingIntent": return ResponseBuilder.Tell("You are pinging");
                    case "EnemyPingIntent": return ResponseBuilder.Tell("You are pinging bad guys");
                    case "CutChuteIntent": return ResponseBuilder.Tell("You are cutting your chute");
                    case "JumpIntent": return ResponseBuilder.Tell("You are jumping");
                    case "ParryIntent": return ResponseBuilder.Tell("You are parrying");
                    case "ProneIntent": return ResponseBuilder.Tell("You are prone");
                }
            }

            return ResponseBuilder.Tell("You said something I dont' know what to do with. Bye.");
        }
    }
}