namespace NuVelocity.Rebound;

[PropertyRoot("CScriptPositionCalculatedPoint",
    typeof(ScriptPositionCalculatedPoint))]
public class ScriptPositionCalculatedPoint : ScriptPosition
{
    // FIXME: handle X and Y type and value
    [Property("Relative To Starting Point")]
    public bool RelativeToStartingPoint { get; set; }
}
