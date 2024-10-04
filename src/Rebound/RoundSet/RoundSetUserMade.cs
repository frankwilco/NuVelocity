namespace NuVelocity.Rebound;

[PropertyRoot("CRoundSetUserMade", typeof(RoundSetUserMade), false)]
public class RoundSetUserMade : RoundSet
{
    [Property("Comment",
        isDynamic: true)]
    public string? Comment { get; set; }

    [Property("Round List")]
    [PropertyArray("Round")]
    public List<BrickLayout> RoundList { get; set; }

    public RoundSetUserMade()
    {
        RoundList = new();
    }
}
