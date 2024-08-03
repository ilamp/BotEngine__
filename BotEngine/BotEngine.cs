using CommunityToolkit.Mvvm.ComponentModel;

namespace BotEngine;

public partial class BotEngine : ObservableObject, iBotEngine
{
    public void Connect(string deviceIp, int port)
    {
        DeviceIP = deviceIp;
        DevicePort = port;
        EnsureDeviceConnected();
    }
}
