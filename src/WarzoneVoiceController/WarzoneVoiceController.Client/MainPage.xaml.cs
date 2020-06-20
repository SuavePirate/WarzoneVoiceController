using Alexa.NET.Request.Type;
using Microsoft.AspNetCore.SignalR.Client;
using SerialArduino;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
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
        public static MainPage Current;
        public MainPage()
        {
            this.InitializeComponent(); 
            listOfDevices = new ObservableCollection<DeviceListEntry>();

            mapDeviceWatchersToDeviceSelector = new Dictionary<DeviceWatcher, String>();
            watchersStarted = false;
            watchersSuspended = false;

            isAllDevicesEnumerated = false;
            Current = this;
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

            connection.On<IntentRequest>("ReloadIntent", async (intentReqest) =>
            {
                await HitKey("reload");
                CommandLog.Text += "\nReloading";
            });

            connection.On<IntentRequest>("ArmorIntent", async (intentReqest) =>
            {
                await HitKey("armor");
                CommandLog.Text += "\nPutting on armor";
            });
            connection.On<IntentRequest>("SprintIntent", async (intentReqest) =>
            {
                await HitKey("sprint");
                CommandLog.Text += "\nSprinting";
            });
            connection.On<IntentRequest>("AttackIntent", async (intentReqest) =>
            {
                await HitKey("shoot");
                //LeftClick();
                CommandLog.Text += "\nAttack";
            });
            connection.On<IntentRequest>("AimIntent", async (intentReqest) =>
            {
                await HitKey("aim");
                //LeftClick();
                CommandLog.Text += "\nAiming";
            });
            connection.On<IntentRequest>("PingIntent", async (intentReqest) =>
            {
                await HitKey("ping");
                CommandLog.Text += "\nPinging";
            });
            connection.On<IntentRequest>("EnemyPingIntent", async (intentReqest) =>
            {
                await HitKey("ping");
                await HitKey("ping");
                CommandLog.Text += "\nPinging enemy";
            });
            connection.On<IntentRequest>("CutChuteIntent", async (intentReqest) =>
            {
                await HitKey("cut");
                CommandLog.Text += "\nCutting chute";
            });
            connection.On<IntentRequest>("JumpIntent", async (intentReqest) =>
            {
                await HitKey("jump");
                CommandLog.Text += "\nJumping";
            });
            connection.On<IntentRequest>("CrouchIntent", async (intentReqest) =>
            {
                await HitKey("crouch");
                CommandLog.Text += "\nCrouching";
            });
            connection.On<IntentRequest>("ProneIntent", async (intentReqest) =>
            {
                //HitKey(VirtualKey.LeftControl);
                await HitKey("prone");
                CommandLog.Text += "\nGoing prone";
            });
            connection.On<IntentRequest>("JumpIntent", async (intentReqest) =>
            {
                await HitKey("jump");
                CommandLog.Text += "\nJumping";
            });
            connection.On<IntentRequest>("GrenadeIntent", async (intentReqest) =>
            {
                await HitKey("grenade");
                CommandLog.Text += "\nGrenade out!";
            });
            connection.On<IntentRequest>("AlternateGrenadeIntent", async (intentReqest) =>
            {
                await HitKey("altgrenade");
                CommandLog.Text += "\nUsing alternate grenade";
            });
            connection.On<IntentRequest>("SwitchWeaponsIntent", async (intentReqest) =>
            {
                await HitKey("swap");
                CommandLog.Text += "\nSwapping";
            });
            connection.On<IntentRequest>("MapIntent", async (intentReqest) =>
            {
                await HitKey("map");
                CommandLog.Text += "\nToggling map";
            });
            connection.On<IntentRequest>("UseItemIntent", async (intentReqest) =>
            {
                await HitKey("item");
                CommandLog.Text += "\nUsing item";
            });
            connection.On<IntentRequest>("MoveForwardIntent", async (intentReqest) =>
            {
                var duration = intentReqest?.Intent?.Slots?.ContainsKey("Duration") == true ? intentReqest.Intent.Slots["Duration"].Value : null;

                await HitKey($"forwards:{duration}");
                CommandLog.Text += "\nMoving forward";
            });
            connection.On<IntentRequest>("MoveBackwardsIntent", async (intentReqest) =>
            {
                var duration = intentReqest?.Intent?.Slots?.ContainsKey("Duration") == true ? intentReqest.Intent.Slots["Duration"].Value : null;

                await HitKey($"backwards");
                CommandLog.Text += "\nMoving backwards";
            });
            connection.On<IntentRequest>("MoveLeftIntent", async (intentReqest) =>
            {
                var duration = intentReqest?.Intent?.Slots?.ContainsKey("Duration") == true ? intentReqest.Intent.Slots["Duration"].Value : null;

                await HitKey($"left");
                CommandLog.Text += "\nMoving left";
            });
            connection.On<IntentRequest>("MoveRightIntent", async (intentReqest) =>
            {
                var duration = intentReqest?.Intent?.Slots?.ContainsKey("Duration") == true ? intentReqest.Intent.Slots["Duration"].Value : null;

                await HitKey($"right");
                CommandLog.Text += "\nMoving right";
            });
            await connection.StartAsync();
        }

        DataWriter DataWriterObject = null;
        private async Task HitKey(string command)
        {

            if (EventHandlerForDevice.Current.IsDeviceConnected)
            {
                try
                {
                    DataWriterObject = new DataWriter(EventHandlerForDevice.Current.Device.OutputStream);
                    DataWriterObject.WriteString($"{command}\n");
                    await WriteAsync(CancellationToken.None);
                }
                catch (OperationCanceledException /*exception*/)
                {
                    Debugger.Break();
                }
                catch (Exception exception)
                {
                    Debugger.Break();
                    Debug.WriteLine(exception.Message.ToString());
                }
                finally
                {
                    DataWriterObject.DetachStream();
                    DataWriterObject = null;
                }
            }
            else
            {
                Debugger.Break();
            }

            // NOTE: this doesn't work with video games. Needs to be USB - so now we send serial commands
            //var inputInjector = InputInjector.TryCreate();
            //var keyInfo = new InjectedInputKeyboardInfo();
            //keyInfo.VirtualKey = (ushort)key;
            //inputInjector.InjectKeyboardInput(new[] { keyInfo });
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






        // this is all from the windows Arduino Serial example:.... not my fault

        object WriteCancelLock = new object();
        private async Task WriteAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> storeAsyncTask;

            // Don't start any IO if we canceled the task
            lock (WriteCancelLock)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Cancellation Token will be used so we can stop the task operation explicitly
                // The completion function should still be called so that we can properly handle a canceled task
                storeAsyncTask = DataWriterObject.StoreAsync().AsTask(cancellationToken);
            }

            UInt32 bytesWritten = await storeAsyncTask;
        }

        private const String ButtonNameDisconnectFromDevice = "Disconnect from device";
        private const String ButtonNameDisableReconnectToDevice = "Do not automatically reconnect to device that was just closed";

        private SuspendingEventHandler appSuspendEventHandler;
        private EventHandler<Object> appResumeEventHandler;

        private ObservableCollection<DeviceListEntry> listOfDevices;

        private Dictionary<DeviceWatcher, String> mapDeviceWatchersToDeviceSelector;
        private Boolean watchersSuspended;
        private Boolean watchersStarted;

        // Has all the devices enumerated by the device watcher?
        private Boolean isAllDevicesEnumerated;


        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// 
        /// Create the DeviceWatcher objects when the user navigates to this page so the UI list of devices is populated.
        /// </summary>
        /// <param name="eventArgs">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            // If we are connected to the device or planning to reconnect, we should disable the list of devices
            // to prevent the user from opening a device without explicitly closing or disabling the auto reconnect
            if (EventHandlerForDevice.Current.IsDeviceConnected
                || (EventHandlerForDevice.Current.IsEnabledAutoReconnect
                && EventHandlerForDevice.Current.DeviceInformation != null))
            {
                UpdateConnectDisconnectButtonsAndList(false);

                // These notifications will occur if we are waiting to reconnect to device when we start the page
                EventHandlerForDevice.Current.OnDeviceConnected = this.OnDeviceConnected;
                EventHandlerForDevice.Current.OnDeviceClose = this.OnDeviceClosing;
            }
            else
            {
                UpdateConnectDisconnectButtonsAndList(true);
            }

            // Begin watching out for events
            StartHandlingAppEvents();

            // Initialize the desired device watchers so that we can watch for when devices are connected/removed
            InitializeDeviceWatchers();
            StartDeviceWatchers();

            DeviceListSource.Source = listOfDevices;
        }

        /// <summary>
        /// Unregister from App events and DeviceWatcher events because this page will be unloaded.
        /// </summary>
        /// <param name="eventArgs"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs eventArgs)
        {
            StopDeviceWatchers();
            StopHandlingAppEvents();

            // We no longer care about the device being connected
            EventHandlerForDevice.Current.OnDeviceConnected = null;
            EventHandlerForDevice.Current.OnDeviceClose = null;
        }

        private async void ConnectToDevice_Click(Object sender, RoutedEventArgs eventArgs)
        {
            var selection = ConnectDevices.SelectedItems;
            DeviceListEntry entry = null;

            if (selection.Count > 0)
            {
                var obj = selection[0];
                entry = (DeviceListEntry)obj;

                if (entry != null)
                {
                    // Create an EventHandlerForDevice to watch for the device we are connecting to
                    EventHandlerForDevice.CreateNewEventHandlerForDevice();

                    // Get notified when the device was successfully connected to or about to be closed
                    EventHandlerForDevice.Current.OnDeviceConnected = this.OnDeviceConnected;
                    EventHandlerForDevice.Current.OnDeviceClose = this.OnDeviceClosing;

                    // It is important that the FromIdAsync call is made on the UI thread because the consent prompt, when present,
                    // can only be displayed on the UI thread. Since this method is invoked by the UI, we are already in the UI thread.
                    Boolean openSuccess = await EventHandlerForDevice.Current.OpenDeviceAsync(entry.DeviceInformation, entry.DeviceSelector);

                    // Disable connect button if we connected to the device
                    UpdateConnectDisconnectButtonsAndList(!openSuccess);
                }
            }
        }

        private void DisconnectFromDevice_Click(Object sender, RoutedEventArgs eventArgs)
        {
            var selection = ConnectDevices.SelectedItems;
            DeviceListEntry entry = null;

            // Prevent auto reconnect because we are voluntarily closing it
            // Re-enable the ConnectDevice list and ConnectToDevice button if the connected/opened device was removed.
            EventHandlerForDevice.Current.IsEnabledAutoReconnect = false;

            if (selection.Count > 0)
            {
                var obj = selection[0];
                entry = (DeviceListEntry)obj;

                if (entry != null)
                {
                    EventHandlerForDevice.Current.CloseDevice();
                }
            }

            UpdateConnectDisconnectButtonsAndList(true);
        }

        /// <summary>
        /// Initialize device watchers to look for the Serial Arduino device
        ///
        /// GetDeviceSelector return an AQS string that can be passed directly into DeviceWatcher.createWatcher() or  DeviceInformation.createFromIdAsync(). 
        ///
        /// In this sample, a DeviceWatcher will be used to watch for devices because we can detect surprise device removals.
        /// </summary>
        private void InitializeDeviceWatchers()
        {
            //string deviceSelector = Windows.Devices.SerialCommunication.SerialDevice.GetDeviceSelectorFromUsbVidPid(
            //                                                        ArduinoDevice.Vid, ArduinoDevice.Pid);
            var deviceSelector = SerialDevice.GetDeviceSelector("COM8");
            // Create a device watcher to look for instances of the Serial Device that match the device selector
            // used earlier.

            var deviceWatcher = DeviceInformation.CreateWatcher(deviceSelector);

            // Allow the EventHandlerForDevice to handle device watcher events that relates or effects our device (i.e. device removal, addition, app suspension/resume)
            AddDeviceWatcher(deviceWatcher, deviceSelector);
        }

        private void StartHandlingAppEvents()
        {
            appSuspendEventHandler = new SuspendingEventHandler(this.OnAppSuspension);
            appResumeEventHandler = new EventHandler<Object>(this.OnAppResume);

            // This event is raised when the app is exited and when the app is suspended
            App.Current.Suspending += appSuspendEventHandler;

            App.Current.Resuming += appResumeEventHandler;
        }

        private void StopHandlingAppEvents()
        {
            // This event is raised when the app is exited and when the app is suspended
            App.Current.Suspending -= appSuspendEventHandler;

            App.Current.Resuming -= appResumeEventHandler;
        }

        /// <summary>
        /// Registers for Added, Removed, and Enumerated events on the provided deviceWatcher before adding it to an internal list.
        /// </summary>
        /// <param name="deviceWatcher"></param>
        /// <param name="deviceSelector">The AQS used to create the device watcher</param>
        private void AddDeviceWatcher(DeviceWatcher deviceWatcher, String deviceSelector)
        {
            deviceWatcher.Added += new TypedEventHandler<DeviceWatcher, DeviceInformation>(this.OnDeviceAdded);
            deviceWatcher.Removed += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(this.OnDeviceRemoved);
            deviceWatcher.EnumerationCompleted += new TypedEventHandler<DeviceWatcher, Object>(this.OnDeviceEnumerationComplete);

            mapDeviceWatchersToDeviceSelector.Add(deviceWatcher, deviceSelector);
        }

        /// <summary>
        /// Starts all device watchers including ones that have been individually stopped.
        /// </summary>
        private void StartDeviceWatchers()
        {
            // Start all device watchers
            watchersStarted = true;
            isAllDevicesEnumerated = false;

            foreach (DeviceWatcher deviceWatcher in mapDeviceWatchersToDeviceSelector.Keys)
            {
                if ((deviceWatcher.Status != DeviceWatcherStatus.Started)
                    && (deviceWatcher.Status != DeviceWatcherStatus.EnumerationCompleted))
                {
                    deviceWatcher.Start();
                }
            }
        }

        /// <summary>
        /// Stops all device watchers.
        /// </summary>
        private void StopDeviceWatchers()
        {
            // Stop all device watchers
            foreach (DeviceWatcher deviceWatcher in mapDeviceWatchersToDeviceSelector.Keys)
            {
                if ((deviceWatcher.Status == DeviceWatcherStatus.Started)
                    || (deviceWatcher.Status == DeviceWatcherStatus.EnumerationCompleted))
                {
                    deviceWatcher.Stop();
                }
            }

            // Clear the list of devices so we don't have potentially disconnected devices around
            ClearDeviceEntries();

            watchersStarted = false;
        }

        /// <summary>
        /// Creates a DeviceListEntry for a device and adds it to the list of devices in the UI
        /// </summary>
        /// <param name="deviceInformation">DeviceInformation on the device to be added to the list</param>
        /// <param name="deviceSelector">The AQS used to find this device</param>
        private void AddDeviceToList(DeviceInformation deviceInformation, String deviceSelector)
        {
            // search the device list for a device with a matching interface ID
            var match = FindDevice(deviceInformation.Id);

            // Add the device if it's new
            if (match == null)
            {
                // Create a new element for this device interface, and queue up the query of its
                // device information
                match = new DeviceListEntry(deviceInformation, deviceSelector);

                // Add the new element to the end of the list of devices
                listOfDevices.Add(match);
            }
        }

        private void RemoveDeviceFromList(String deviceId)
        {
            // Removes the device entry from the interal list; therefore the UI
            var deviceEntry = FindDevice(deviceId);

            listOfDevices.Remove(deviceEntry);
        }

        private void ClearDeviceEntries()
        {
            listOfDevices.Clear();
        }

        /// <summary>
        /// Searches through the existing list of devices for the first DeviceListEntry that has
        /// the specified device Id.
        /// </summary>
        /// <param name="deviceId">Id of the device that is being searched for</param>
        /// <returns>DeviceListEntry that has the provided Id; else a nullptr</returns>
        private DeviceListEntry FindDevice(String deviceId)
        {
            if (deviceId != null)
            {
                foreach (DeviceListEntry entry in listOfDevices)
                {
                    if (entry.DeviceInformation.Id == deviceId)
                    {
                        return entry;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// We must stop the DeviceWatchers because device watchers will continue to raise events even if
        /// the app is in suspension, which is not desired (drains battery). We resume the device watcher once the app resumes again.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnAppSuspension(Object sender, SuspendingEventArgs args)
        {
            if (watchersStarted)
            {
                watchersSuspended = true;
                StopDeviceWatchers();
            }
            else
            {
                watchersSuspended = false;
            }
        }

        /// <summary>
        /// See OnAppSuspension for why we are starting the device watchers again
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnAppResume(Object sender, Object args)
        {
            if (watchersSuspended)
            {
                watchersSuspended = false;
                StartDeviceWatchers();
            }
        }

        /// <summary>
        /// We will remove the device from the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInformationUpdate"></param>
        private async void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate deviceInformationUpdate)
        {
            await Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                new DispatchedHandler(() =>
                {
                    RemoveDeviceFromList(deviceInformationUpdate.Id);
                }));
        }

        /// <summary>
        /// This function will add the device to the listOfDevices so that it shows up in the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInformation"></param>
        private async void OnDeviceAdded(DeviceWatcher sender, DeviceInformation deviceInformation)
        {
            await Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                new DispatchedHandler(() =>
                {
                   
                    AddDeviceToList(deviceInformation, mapDeviceWatchersToDeviceSelector[sender]);
                }));
        }

        /// <summary>
        /// Notify the UI whether or not we are connected to a device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void OnDeviceEnumerationComplete(DeviceWatcher sender, Object args)
        {
            await Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                new DispatchedHandler(() =>
                {
                    isAllDevicesEnumerated = true;

                    // If we finished enumerating devices and the device has not been connected yet, the OnDeviceConnected method
                    // is responsible for selecting the device in the device list (UI); otherwise, this method does that.
                    if (EventHandlerForDevice.Current.IsDeviceConnected)
                    {
                        SelectDeviceInList(EventHandlerForDevice.Current.DeviceInformation.Id);

                        ButtonDisconnectFromDevice.Content = ButtonNameDisconnectFromDevice;

                      
                        EventHandlerForDevice.Current.ConfigureCurrentlyConnectedDevice();
                    }
                    else if (EventHandlerForDevice.Current.IsEnabledAutoReconnect && EventHandlerForDevice.Current.DeviceInformation != null)
                    {
                        // We will be reconnecting to a device
                        ButtonDisconnectFromDevice.Content = ButtonNameDisableReconnectToDevice;

                    }
                }));
        }

        /// <summary>
        /// If all the devices have been enumerated, select the device in the list we connected to. Otherwise let the EnumerationComplete event
        /// from the device watcher handle the device selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInformation"></param>
        private void OnDeviceConnected(EventHandlerForDevice sender, DeviceInformation deviceInformation)
        {
            // Find and select our connected device
            if (isAllDevicesEnumerated)
            {
                SelectDeviceInList(EventHandlerForDevice.Current.DeviceInformation.Id);

                ButtonDisconnectFromDevice.Content = ButtonNameDisconnectFromDevice;
            }

        }
      
        /// <summary>
        /// The device was closed. If we will autoreconnect to the device, reflect that in the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInformation"></param>
        private async void OnDeviceClosing(EventHandlerForDevice sender, DeviceInformation deviceInformation)
        {
            await Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                new DispatchedHandler(() =>
                {
                    // We were connected to the device that was unplugged, so change the "Disconnect from device" button
                    // to "Do not reconnect to device"
                    if (ButtonDisconnectFromDevice.IsEnabled && EventHandlerForDevice.Current.IsEnabledAutoReconnect)
                    {
                        ButtonDisconnectFromDevice.Content = ButtonNameDisableReconnectToDevice;
                    }
                }));
        }

        /// <summary>
        /// Selects the item in the UI's listbox that corresponds to the provided device id. If there are no
        /// matches, we will deselect anything that is selected.
        /// </summary>
        /// <param name="deviceIdToSelect">The device id of the device to select on the list box</param>
        private void SelectDeviceInList(String deviceIdToSelect)
        {
            // Don't select anything by default.
            ConnectDevices.SelectedIndex = -1;

            for (int deviceListIndex = 0; deviceListIndex < listOfDevices.Count; deviceListIndex++)
            {
                if (listOfDevices[deviceListIndex].DeviceInformation.Id == deviceIdToSelect)
                {
                    ConnectDevices.SelectedIndex = deviceListIndex;

                    break;
                }
            }
        }

        /// <summary>
        /// When ButtonConnectToDevice is disabled, ConnectDevices list will also be disabled.
        /// </summary>
        /// <param name="enableConnectButton">The state of ButtonConnectToDevice</param>
        private void UpdateConnectDisconnectButtonsAndList(Boolean enableConnectButton)
        {
            ButtonConnectToDevice.IsEnabled = enableConnectButton;
            ButtonDisconnectFromDevice.IsEnabled = !ButtonConnectToDevice.IsEnabled;

            ConnectDevices.IsEnabled = ButtonConnectToDevice.IsEnabled;
        }
    }
}
