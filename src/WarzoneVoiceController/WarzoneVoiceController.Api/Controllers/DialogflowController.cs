using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Google.Cloud.Dialogflow.V2.Intent.Types;
using static Google.Cloud.Dialogflow.V2.Intent.Types.Message.Types;

namespace WarzoneVoiceController.Api.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class DialogflowController : ControllerBase
    {
        // A Protobuf JSON parser configured to ignore unknown fields. This makes
        // the action robust against new fields being introduced by Dialogflow.
        private static readonly JsonParser jsonParser =
            new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));


        [HttpPost("HandleRequest")]
        public async Task<ActionResult> HandleRequest([FromBody]dynamic request)
        {  // Read the request JSON asynchronously, as the Google.Protobuf library
           // doesn't (yet) support asynchronous parsing.
           //string requestJson;
           //using (TextReader reader = new StreamReader(Request.Body))
           //{
           //    requestJson = await reader.ReadToEndAsync();
           //}

            //// Parse the body of the request using the Protobuf JSON parser,
            //// *not* Json.NET.
            //var request = jsonParser.Parse<WebhookRequest>(requestJson);

            // Note: you should authenticate the request here.

            var message = new Message
            {
                Text = new Text
                {
                    Text_ = {"Hello world"}
                }
            };

          
            var payload = new MapField<string, Value>();

            var responseText = "Hello word";
            var textToSpeech = new Value
            {
                StringValue = responseText
            };

            var simpleResponse = new Struct
            {
                
            };
            simpleResponse.Fields.Add("textToSpeech", textToSpeech);


            var googlePayload = new Google.Protobuf.WellKnownTypes.Value
            {
                StructValue = new Google.Protobuf.WellKnownTypes.Struct
                {
                }
            };
            payload.Add("google", googlePayload);

            

            // Populate the response
            var response = new WebhookResponse()
            {
                FulfillmentMessages = {message},
                Payload = new Google.Protobuf.WellKnownTypes.Struct
                {
                    Fields = { payload }
                }
            };

            // Ask Protobuf to format the JSON to return.
            // Again, we don't want to use Json.NET - it doesn't know how to handle Struct
            // values etc.
            string responseJson = response.ToString();
            return Content(responseJson, "application/json");
        }
    }
}
