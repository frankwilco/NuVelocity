namespace NuVelocity.Rebound;

[PropertyRoot("CBrickPlugInWaitForBrickToBeDestroyed",
    typeof(BrickPlugInWaitForBrickToBeDestroyed))]
public class BrickPlugInWaitForBrickToBeDestroyed : BrickPlugIn
{
    [Property("Trigger Brick")]
    public ScriptReferenceToTarget TriggerBrick { get; set; }
}
