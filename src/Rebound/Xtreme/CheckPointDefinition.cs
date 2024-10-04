namespace NuVelocity.Rebound.Xtreme;

[PropertyRoot("CCheckPointDefinition", typeof(CheckPointDefinition))]
public class CheckPointDefinition
{
    [Property("Round Number")]
    public int RoundNumber { get; set; }

    // filename reference to sequence.
    [Property("Image")]
    public string Image { get; set; }
}
