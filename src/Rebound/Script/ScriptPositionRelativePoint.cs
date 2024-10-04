namespace NuVelocity.Rebound;

[PropertyRoot("CScriptPositionPointRelativeToBrickStartingPosition",
    typeof(ScriptPositionRelativePoint))]
public class ScriptPositionRelativePoint : ScriptPosition
{
    [Property("Position")]
    public FloatCoordinates Position { get; set; }
}
