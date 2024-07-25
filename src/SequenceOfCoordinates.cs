namespace NuVelocity;

[PropertyRoot("CSequenceOfCoordinates", "Sequence of Coordinates")]
public class SequenceOfCoordinates
{
    [PropertyArray("Coordinates", "Coordinate")]
    public List<Coordinates> Coordinates { get; set; }

    public SequenceOfCoordinates()
    {
        Coordinates = new();
    }
}
