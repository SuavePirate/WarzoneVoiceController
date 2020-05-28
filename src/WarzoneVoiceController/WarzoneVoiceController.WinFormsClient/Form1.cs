using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace WarzoneVoiceController.WinFormsClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        protected async override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            
            var connection = new HubConnectionBuilder()
               .WithUrl("https://warzonevoicecontroller.azurewebsites.net/warzone")
               .Build();

            connection.Closed += (error) =>
            {
                return Task.CompletedTask;
            };
            connection.Reconnected += (message) =>
            {
                return Task.CompletedTask;
            };

            connection.On("ReloadIntent", () =>
            {
                Console.WriteLine("Reloading");
            });

            connection.On("ArmorIntent", () =>
            {
                Console.WriteLine("Putting on armor"); 
                var sim = new InputSimulator();

                // Press 0 key
                sim.Keyboard.KeyPress(VirtualKeyCode.VK_4);
            });
            connection.On("SprintIntent", () =>
            {
                Console.WriteLine("Sprinting");
            });
            connection.On("AttackIntent", () =>
            {
                Console.WriteLine("Attack");
            });
            connection.On("PingIntent", () =>
            {
                Console.WriteLine("Pinging");
            });
            connection.On("EnemyPingIntent", () =>
            {
                Console.WriteLine("Pinging enemy");
            });
            connection.On("CutChuteIntent", () =>
            {
                Console.WriteLine("Cutting chute");
            });
            connection.On("JumpIntent", () =>
            {
                Console.WriteLine("Jumping");
            });
            connection.On("CrouchIntent", () =>
            {
                HitLowLevelKey();
                Console.WriteLine("Crouching");
            });
            connection.On("ProneIntent", () =>
            {
                //HitKey(VirtualKey.LeftControl);
                Console.WriteLine("Going prone");
            });
            await connection.StartAsync();
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

    }
}
