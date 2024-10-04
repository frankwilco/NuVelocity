namespace NuVelocity.Rebound.Infinity;

[PropertyRoot("CFrameFrameSelector", typeof(FrameFrameSelector))]
public class FrameFrameSelector : FrameSelector
{
    // reference to frame file name.
    [Property("Frame")]
    public string Frame { get; set; }
}
