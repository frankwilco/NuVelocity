namespace NuVelocity.Rebound;

[PropertyRoot("CBrickPlugInSequence", typeof(BrickPlugInSequence))]
public class BrickPlugInSequence : BrickPlugIn
{
    [Property("Steps")]
    [PropertyArray("Step")]
    public List<BrickPlugIn> PlugIns { get; set; }

    [Property("Current Step Index")]
    public int CurrentStepIndex { get; set; }

    [Property("When Done")]
    public SequenceWhenDone WhenDone { get; set; }

    #region Serializer methods

    private static bool ShouldSerializeCurrentStepIndex()
    {
        return false;
    }

    #endregion
}
