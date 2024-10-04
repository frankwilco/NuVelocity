namespace NuVelocity.Rebound.Xtreme;

[PropertyRoot("CRoundSet", typeof(RoundSet), false)]
public class RoundSet
{
    [Property("Round List")]
    [PropertyArray("Round")]
    public List<string> RoundList { get; set; }

    [PropertyArray("Checkpoints")]
    public List<CheckPointDefinition> Checkpoints { get; set; }

    public RoundSet()
    {
        RoundList = new();
        Checkpoints = new();
    }
}
