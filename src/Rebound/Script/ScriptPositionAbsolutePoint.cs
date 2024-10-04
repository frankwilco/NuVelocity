namespace NuVelocity.Rebound;

[PropertyRoot("CScriptPositionAbsolutePoint",
    typeof(ScriptPositionAbsolutePoint))]
public class ScriptPositionAbsolutePoint : ScriptPosition
{
    [Property("Position")]
    public FloatCoordinates Position { get; set; }
}
