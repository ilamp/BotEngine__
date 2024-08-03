using OpenCvSharp.Extensions;
using OpenCvSharp;
using System.Drawing;

namespace BotEngine;

public partial class BotEngine
{
    public Rectangle? Find(string templatePath, double minThreshold = 0.8)
    {
        EnsureDeviceConnected();
        return Find(CaptureScreenshot(), templatePath, minThreshold);
    }

    public Rectangle? Find(Bitmap screenshot, string templatePath, double minThreshold = 0.8)
    {
        using (Mat src = BitmapConverter.ToMat(screenshot))
        using (Mat template = new Mat(templatePath, ImreadModes.Color))
        using (Mat result = new Mat())
        {
            Cv2.MatchTemplate(src, template, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out OpenCvSharp.Point minLoc, out OpenCvSharp.Point maxLoc);

            if (maxVal >= minThreshold)
            {
                return new Rectangle(maxLoc.X, maxLoc.Y, template.Width, template.Height);
            }
            else
            {
                return null;
            }
        }
    }
}
