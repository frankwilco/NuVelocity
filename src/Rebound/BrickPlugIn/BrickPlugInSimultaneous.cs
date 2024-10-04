namespace NuVelocity.Rebound;

[PropertyRoot("CBrickPlugInSimultaneous", typeof(BrickPlugInSimultaneous))]
public class BrickPlugInSimultaneous : BrickPlugIn
{
    [Property("Simultaneous PlugIns")]
    [PropertyArray("Simultaneous PlugIn")]
    public List<BrickPlugIn> PlugIns { get; set; }
}
