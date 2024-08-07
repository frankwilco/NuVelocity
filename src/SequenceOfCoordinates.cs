namespace NuVelocity;

[PropertyRoot("CSequenceOfCoordinates", typeof(SequenceOfCoordinates))]
public class SequenceOfCoordinates
{
    [Property("Coordinates")]
    [PropertyArray("Coordinate")]
    public List<Coordinates> Coordinates { get; set; }

    public SequenceOfCoordinates()
    {
        Coordinates = new();
    }
}
