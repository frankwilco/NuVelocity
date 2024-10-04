namespace NuVelocity.Rebound;

[PropertyRoot("CScriptPositionNamedObject", typeof(ScriptPositionNamedObject))]
public class ScriptPositionNamedObject : ScriptPosition
{
    // TODO: perhaps this should use a custom type to represent
    // brick name.
    [Property("Position")]
    public string Position { get; set; }
}
