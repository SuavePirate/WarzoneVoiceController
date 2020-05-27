using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WarzoneVoiceController.Client
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        HubConnection connection;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            connection = new HubConnectionBuilder()
                .WithUrl("https://warzonevoicecontroller.azurewebsites.net/warzone")
                .Build();

            connection.Closed += (error) =>
            {
                ConnectButton.Content = "You were disconnected. Click to reconnect";
                return Task.CompletedTask;
            };
            connection.Reconnected += (message) =>
            {
                ConnectButton.Content = "You are connected";
                return Task.CompletedTask;
            };

            connection.On("ReloadIntent", () =>
            {
                HitKey(VirtualKey.R);
                CommandLog.Text += "\nReloading";
            });

            connection.On("ArmorIntent", () =>
            {
                HitKey(VirtualKey.Number4);
                CommandLog.Text += "\nPutting on armor";
            });
            connection.On("SprintIntent", () =>
            {
                HitKey(VirtualKey.LeftShift);
                CommandLog.Text += "\nSprinting";
            });
            connection.On("AttackIntent", () =>
            {
                ////HitKey(VirtualKey.R);
                LeftClick();
                CommandLog.Text += "\nAttack";
            });
            connection.On("PingIntent", () =>
            {
                HitKey(VirtualKey.LeftMenu);
                CommandLog.Text += "\nPinging";
            });
            connection.On("EnemyPingIntent", () =>
            {
                HitKey(VirtualKey.LeftMenu);
                HitKey(VirtualKey.LeftMenu);
                CommandLog.Text += "\nPinging enemy";
            });
            connection.On("CutChuteIntent", () =>
            {
                HitKey(VirtualKey.C);
                CommandLog.Text += "\nCutting chute";
            });
            connection.On("JumpIntent", () =>
            {
                HitKey(VirtualKey.Space);
                CommandLog.Text += "\nJumping";
            });
            connection.On("CrouchIntent", () =>
            {
                HitKey(VirtualKey.LeftControl);
                HitLowLevelKey();
                CommandLog.Text += "\nCrouching";
            });
            connection.On("ProneIntent", () =>
            {
                //HitKey(VirtualKey.LeftControl);
                CommandLog.Text += "\nGoing prone";
            });
            await connection.StartAsync();
        }

        private void HitKey(VirtualKey key)
        {
            var inputInjector = InputInjector.TryCreate();
            var keyInfo = new InjectedInputKeyboardInfo();
            keyInfo.VirtualKey = (ushort)key;
            inputInjector.InjectKeyboardInput(new[] { keyInfo });
        }
        // Import the user32.dll
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);


        public const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag
        public const int VK_RCONTROL = 0xA3; //Right Control key code


        private void HitLowLevelKey()
        {
            keybd_event(VK_RCONTROL, 0, KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event(VK_RCONTROL, 0, KEYEVENTF_KEYUP, 0);
        }

        private void LeftClick()
        {
            var inputInject = InputInjector.TryCreate();
            var mouseInfo = new InjectedInputMouseInfo();
            mouseInfo.MouseOptions = InjectedInputMouseOptions.LeftDown;
            inputInject.InjectMouseInput(new[] { mouseInfo });
        }
        private void RightClick()
        {
            var inputInject = InputInjector.TryCreate();
            var mouseInfo = new InjectedInputMouseInfo();
            mouseInfo.MouseOptions = InjectedInputMouseOptions.RightDown;
            inputInject.InjectMouseInput(new[] { mouseInfo });
        }
    }
}
