using System;
using System.Net.Http;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchClientObject = TwitchLib.Client.TwitchClient;
namespace WarzoneVoiceController.TwitchClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Bot bot = new Bot();
            Console.ReadLine();
        }
    }

    class Bot
    {
        TwitchClientObject client;

        public Bot()
        {
            var credentials = new ConnectionCredentials(Keys.TwitchAccount, Keys.TwitchToken);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            var customClient = new WebSocketClient(clientOptions);
            client = new TwitchClientObject(customClient);
            client.Initialize(credentials, Keys.TwitchChannel);

            client.OnLog += Client_OnLog;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnWhisperReceived += Client_OnWhisperReceived;
            client.OnNewSubscriber += Client_OnNewSubscriber;
            client.OnConnected += Client_OnConnected;

            client.Connect();
        }

        private void Client_OnLog(object sender, OnLogArgs e)
        {
            Console.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
        }

        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            Console.WriteLine($"Connected to {e.AutoJoinChannel}");
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Console.WriteLine("Hey guys! I am a bot connected via TwitchLib!");
            client.SendMessage(e.Channel, "Hey guys! I am a bot connected via TwitchLib!");
        }

        private async void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            // Get the message. Validate it is a command with "!". Then send to API to then route
            using (var client = new HttpClient())
            {
                var command = "FallbackIntent";
                switch(e.ChatMessage.Message)
                {
                    case "!reload": command = "ReloadIntent";
                        break;
                    case "!map":
                        command = "MapIntent";
                        break;
                    case "!forward":
                        command = "MoveForwardIntent";
                        break;
                    case "!backwards":
                        command = "MoveBackwardsIntent";
                        break;
                    case "!left":
                        command = "MoveLeftIntent";
                        break;
                    case "!right":
                        command = "MoveRightIntent";
                        break;
                    case "!ping":
                        command = "PingIntent";
                        break;
                    case "!sprint":
                        command = "SprintIntent";
                        break;
                    case "!crouch":
                        command = "CrouchIntent";
                        break;
                    case "!shoot":
                        command = "AttackIntent";
                        break;

                }

                var response = await client.PostAsync($"https://warzonevoicecontroller.azurewebsites.net/api/command/{command}", null);
                Console.WriteLine(response);
            }

}

        private void Client_OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
        {
            if (e.WhisperMessage.Username == "my_friend")
                client.SendWhisper(e.WhisperMessage.Username, "Hey! Whispers are so cool!!");
        }

        private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime)
                client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points! So kind of you to use your Twitch Prime on this channel!");
            else
                client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points!");
        }
    }
}
