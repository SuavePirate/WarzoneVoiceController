using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using WarzoneVoiceController.Api.Configuration;
using WarzoneVoiceController.Api.Hubs;

namespace WarzoneVoiceController.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlexaController : ControllerBase
    {
        private readonly IHubContext<WarzoneHub> _warzoneHub;
        private readonly AlexaSkillSettings _settings;

        public AlexaController(IHubContext<WarzoneHub> warzoneHub, IOptions<AlexaSkillSettings> options)
        {
            _settings = options.Value;
            _warzoneHub = warzoneHub;
        }

        [HttpPost("HandleRequest")]
        public async Task<ActionResult> HandleRequest([FromBody]SkillRequest skillRequest)
        { 
            // validate the alexa request: https://developer.amazon.com/docs/custom-skills/host-a-custom-skill-as-a-web-service.html
            //var isValid = await ValidateRequest(Request, skillRequest);
            //if (!isValid)
            //    return BadRequest("Validation errors");


            // if it's Launch request, then say hello and tell the user the commands
            if (skillRequest?.GetRequestType() == typeof(LaunchRequest))
            {
                return Ok(ResponseBuilder.Ask("Welcome to the warzone controller, issue a command like 'jump' or 'cut chute'.", null));
            }
            // if it's an intent request, then choose what command based on the name of the intent
            else if (skillRequest?.GetRequestType() == typeof(IntentRequest))
            {
                var intentRequest = skillRequest.Request as IntentRequest;
                await _warzoneHub.Clients.All.SendAsync(intentRequest.Intent.Name, intentRequest);
                switch (intentRequest.Intent.Name)
                {
                    case "ReloadIntent": return Ok(ResponseBuilder.Tell("You are reloading"));
                    case "ArmorIntent": return Ok(ResponseBuilder.Tell("You are putting on armor"));
                    case "SprintIntent": return Ok(ResponseBuilder.Tell("You are sprinting"));
                    case "AttackIntent": return Ok(ResponseBuilder.Tell("You are attacking"));
                    case "ShieldIntent": return Ok(ResponseBuilder.Tell("You are using your shield"));
                    case "PingIntent": return Ok(ResponseBuilder.Tell("You are pinging"));
                    case "EnemyPingIntent": return Ok(ResponseBuilder.Tell("You are pinging bad guys"));
                    case "CutChuteIntent": return Ok(ResponseBuilder.Tell("You are cutting your chute"));
                    case "JumpIntent": return Ok(ResponseBuilder.Tell("You are jumping"));
                    case "CrouchIntent": return Ok(ResponseBuilder.Tell("You are crouching"));
                    case "ProneIntent": return Ok(ResponseBuilder.Tell("You are prone"));
                    case "KillstreakIntent": return Ok(ResponseBuilder.Tell("You are using a kill streak item."));
                    case "UseItemIntent": return Ok(ResponseBuilder.Tell("You are using your item"));
                    case "MapIntent": return Ok(ResponseBuilder.Tell("Map time!"));
                    case "SwitchWeaponsIntent": return Ok(ResponseBuilder.Tell("Switching weapons"));
                    case "GrenadeCommand": return Ok(ResponseBuilder.Tell("Grenade out!"));
                    case "AlternateGrenadeIntent": return Ok(ResponseBuilder.Tell("Using alternate"));
                    case "MoveForwardIntent":
                    case "MoveBackwardsIntent":
                    case "MoveLeftIntent":
                    case "MoveRightIntent":
                        return Ok(ResponseBuilder.Tell("Moving!"));


                    case "AMAZON.HelpIntent": return Ok(ResponseBuilder.Ask("Happy to help! You can issue commands such as 'jump', 'reload', 'put on shields' and more! Try saying one of those.", null));
                    case "AMAZON.StopIntent":
                    case "AMAZON.CancelIntent": return Ok(ResponseBuilder.Tell("Thanks for using the Warzone Controller. Come back later."));
                }
            }

            return Ok(ResponseBuilder.Ask("You said something I don't know what to do with. Try saying something like 'jump' or 'reload'.", null));
        }
        private async Task<bool> ValidateRequest(HttpRequest request, SkillRequest skillRequest)
        {
            try
            {

                if (!string.IsNullOrEmpty(_settings.SkillId) && _settings.SkillId != skillRequest.Session.Application.ApplicationId)
                    return false;


                // get the signature url
                request.Headers.TryGetValue("SignatureCertChainUrl", out var signatureChainUrl);
                if (string.IsNullOrWhiteSpace(signatureChainUrl))
                {
                    return false;
                }

                Uri certUrl;
                try
                {
                    certUrl = new Uri(signatureChainUrl);
                }
                catch
                {
                    return false;
                }

                // get the signature
                request.Headers.TryGetValue("Signature", out var signature);
                if (string.IsNullOrWhiteSpace(signature))
                {
                    return false;
                }

                // get the raw body
                var body = "";
                request.EnableBuffering();
                using (var stream = new StreamReader(request.Body))
                {
                    stream.BaseStream.Position = 0;
                    body = stream.ReadToEnd();
                    stream.BaseStream.Position = 0;
                }

                if (string.IsNullOrWhiteSpace(body))
                {
                    return false;
                }

                // validate
                bool isTimestampValid = RequestVerification.RequestTimestampWithinTolerance(skillRequest);
                bool valid = await RequestVerification.Verify(signature, certUrl, body);

                if (!valid || !isTimestampValid) { return false; } else { return true; }

            }
            catch
            {
                return false;
            }
        }
    }
}