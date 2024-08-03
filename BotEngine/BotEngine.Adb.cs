using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.Models;
using AdvancedSharpAdbClient.Receivers;
using BotEngine.Exceptions;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace BotEngine;

#nullable enable

public partial class BotEngine : ObservableObject
{
    [ObservableProperty]
    private AdbServer server = new AdbServer();

    [ObservableProperty]
    private IAdbClient serverClient = new AdbClient();

    [ObservableProperty]
    private DeviceData? device;

    [ObservableProperty]
    private DeviceClient? client;

    [ObservableProperty]
    private string? deviceIP;

    [ObservableProperty]
    private int? devicePort;

    private string DeviceID => DeviceIP + ":" + DevicePort;

    [ObservableProperty]
    public Size? screenSize;

    public void EnsureAdbServerStarted()
    {
        if (AdbServer.Instance.GetStatus().IsRunning)
        {
            var result = Server.StartServer(@"adb.exe", false);
            if (result != StartServerResult.Started)
            {
                throw new FailedToStartAdbServerException();
            }
        }
    }

    [MemberNotNull(nameof(Client), nameof(Device))]
    public void EnsureDeviceConnected()
    {
        EnsureAdbServerStarted();

        var deviceId = DeviceIP + ":" + DevicePort;

        if (Client == null)
        {
            try
            {
                ServerClient.Connect(deviceId);
                var deviceData = ServerClient.GetDevices().FirstOrDefault();
                if (!deviceData.IsEmpty)
                {
                    Device = deviceData;
                }
                Device = deviceData;
            }
            catch (Exception)
            {
                throw new DeviceNotConnectedException();
            }
        }

        if (Device == null || Client == null)
        {
            throw new DeviceNotConnectedException();
        }
    }

    private string ExecuteAdbCommandWithResult(string command)
    {
        EnsureDeviceConnected();
        var receiver = new ConsoleOutputReceiver();
        ServerClient.ExecuteRemoteCommand(command, Device.Value, receiver);
        return receiver.ToString();
    }

    [MemberNotNull(nameof(ScreenSize))]
    public void UpdateScreenSize()
    {
        string output = ExecuteAdbCommandWithResult($"-s {DeviceID} shell wm size");
        Match match = Regex.Match(output, @"Physical size: (\d+)x(\d+)");
        if (match.Success)
        {
            int width = int.Parse(match.Groups[1].Value);
            int height = int.Parse(match.Groups[2].Value);
            ScreenSize = new Size(width, height);
        }
        else
        {
            throw new Exception("Unable to retrieve screen size.");
        }
    }

    public Size GetScreenSize()
    {
        UpdateScreenSize();
        return ScreenSize.Value;
    }

    public Bitmap CaptureScreenshot()
    {
        EnsureDeviceConnected();

        var screenshot = ServerClient.GetFrameBuffer(Device.Value).ToImage();
        if (screenshot == null)
        {
            throw new Exception("Failed to take screenshot.");
        }

        return screenshot;
    }

    public void Scroll(int startX, int startY, int endX, int endY, int delay)
    {
        EnsureDeviceConnected();
        Client.Swipe(startX, startY, endX, endY, delay);
    }

    public void Tap(int x, int y)
    {
        EnsureDeviceConnected();
        Client.Click(x, y);
    }

    public void EnterText(string text)
    {
        EnsureDeviceConnected();
        Client.SendText(text);
    }

    public void PressKey(string keyCode)
    {
        EnsureDeviceConnected();
        Client.SendKeyEvent(keyCode);
    }

    public void ZoomIn()
    {
        EnsureDeviceConnected();
        var screen = GetScreenSize();
        int centerX = screen.Width / 2;
        int centerY = screen.Height / 2;
        int distance = screen.Width / 4;

        int startX1 = centerX - distance;
        int startY1 = centerY;
        int startX2 = centerX + distance;
        int startY2 = centerY;
        int endX1 = centerX;
        int endY1 = centerY;
        int endX2 = centerX;
        int endY2 = centerY;

        ExecuteAdbCommandWithResult($"input touchscreen swipe {startX1} {startY1} {endX1} {endY1}");
        ExecuteAdbCommandWithResult($"input touchscreen swipe {startX2} {startY2} {endX2} {endY2}");
    }

    public void ZoomOut()
    {
        EnsureDeviceConnected();
        var screen = GetScreenSize();
        int centerX = screen.Width / 2;
        int centerY = screen.Height / 2;
        int distance = screen.Width / 4;

        int startX1 = centerX;
        int startY1 = centerY;
        int startX2 = centerX;
        int startY2 = centerY;
        int endX1 = centerX - distance;
        int endY1 = centerY;
        int endX2 = centerX + distance;
        int endY2 = centerY;

        ExecuteAdbCommandWithResult($"input touchscreen swipe {startX1} {startY1} {endX1} {endY1}");
        ExecuteAdbCommandWithResult($"input touchscreen swipe {startX2} {startY2} {endX2} {endY2}");
    }
}

