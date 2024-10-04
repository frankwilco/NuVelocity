namespace NuVelocity.Rebound.Infinity;

[PropertyRoot("CStillFrameSelector", typeof(FrameFrameSelector))]
public class StillFrameSelector : FrameSelector
{
    // reference to sequence file name.
    [Property("Image")]
    public string Image { get; set; }

    [Property("Frame")]
    public int Frame { get; set; }
}
