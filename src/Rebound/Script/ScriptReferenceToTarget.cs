namespace NuVelocity.Rebound;

[PropertyRoot("CScriptReferenceToTarget", typeof(ScriptReferenceToTarget))]
public class ScriptReferenceToTarget
{
    [Property("Name")]
    public string Name { get; set; }
}
