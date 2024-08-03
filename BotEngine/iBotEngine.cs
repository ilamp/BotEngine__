namespace BotEngine;

using System.Drawing;

public interface iBotEngine
{
    void UpdateScreenSize();
    Size GetScreenSize();
    Bitmap CaptureScreenshot();
    void Tap(int x, int y);
    void Scroll(int startX, int startY, int endX, int endY, int speed);
    void EnterText(string text);
    void PressKey(string keyCode);
    void ZoomIn();
    void ZoomOut();
    string Read(Bitmap screenshot, Rectangle? targetArea = null, OcrSettings settings = null);
    Rectangle? Find(Bitmap screenshot, string templatePath, double minThreshold = 0.8);
}