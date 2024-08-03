using OpenCvSharp;
using System.Drawing;
using Tesseract;

namespace BotEngine;

public partial class BotEngine
{
    public string Read(Rectangle? targetArea = null, OcrSettings settings = null)
    {
        EnsureDeviceConnected();

        var screenshot = CaptureScreenshot();

        return Read(screenshot, targetArea, settings);
    }

    public string Read(Bitmap screenshot, Rectangle? targetArea, OcrSettings settings = null)
    {
        using (var ocrEngine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
        {
            Mat img = OpenCvSharp.Extensions.BitmapConverter.ToMat(screenshot);

            if (targetArea.HasValue)
            {
                img = new Mat(img, new OpenCvSharp.Rect(targetArea.Value.X, targetArea.Value.Y, targetArea.Value.Width, targetArea.Value.Height));
            }

            if (settings.Ensure300Dpi)
            {
                int dpi = 300;
                float scaleFactor = dpi / screenshot.HorizontalResolution;
                if (scaleFactor > 1)
                {
                    Cv2.Resize(img, img, new OpenCvSharp.Size(img.Width * scaleFactor, img.Height * scaleFactor));
                }
            }

            if (settings.Resize)
            {
                Cv2.Resize(img, img, new OpenCvSharp.Size(img.Width * 2, img.Height * 2));
            }

            if (settings.Grayscale)
            {
                Cv2.CvtColor(img, img, ColorConversionCodes.BGR2GRAY);
            }

            if (settings.Threshold)
            {
                Cv2.Threshold(img, img, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
            }

            if (settings.AdaptiveThreshold)
            {
                Cv2.AdaptiveThreshold(img, img, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 11, 2);
            }

            if (settings.Denoise)
            {
                Cv2.FastNlMeansDenoising(img, img);
            }

            if (settings.EdgeDetection)
            {
                Cv2.Canny(img, img, 100, 200);
            }

            if (settings.Invert)
            {
                Cv2.BitwiseNot(img, img);
            }

            using (Pix pix = Pix.LoadFromMemory(img.ToBytes()))
            {
                var page = ocrEngine.Process(pix);
                return page.GetText();
            }
        }
    }
}
