namespace NuVelocity.Rebound.Infinity;

[PropertyRoot("CPlayLogStats", typeof(PlayLogStats))]
public class PlayLogStats
{
    [Property("LivesLost")]
    public int LivesLost { get; set; }

    [Property("RecallUpdates")]
    public int RecallUpdates { get; set; }
}
