namespace NuVelocity;

[PropertyRoot("CSequenceOfCoordinates", typeof(SequenceOfCoordinates))]
public class SequenceOfCoordinates
{
    [PropertyArray("Coordinates", "Coordinate")]
    public List<Coordinates> Coordinates { get; set; }

    public SequenceOfCoordinates()
    {
        Coordinates = new();
    }
}
