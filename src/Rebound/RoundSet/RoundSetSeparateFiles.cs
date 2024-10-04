namespace NuVelocity.Rebound;

[PropertyRoot("CRoundSetSeparateFiles", typeof(RoundSetSeparateFiles), false)]
public class RoundSetSeparateFiles : RoundSet
{
    [Property("Round List")]
    [PropertyArray("Round")]
    public List<string> RoundList { get; set; }

    public RoundSetSeparateFiles()
    {
        RoundList = new();
    }
}
