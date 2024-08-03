namespace BotEngine;

public class OcrSettings
{
    public bool Grayscale { get; set; } = false;
    public bool Threshold { get; set; } = false;
    public bool Denoise { get; set; } = false;
    public bool Resize { get; set; } = false;
    public bool AdaptiveThreshold { get; set; } = false;
    public bool EdgeDetection { get; set; } = false;
    public bool Invert { get; set; } = false;
    public bool Ensure300Dpi { get; set; } = false;
}
