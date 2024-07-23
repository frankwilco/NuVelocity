namespace NuVelocity;

[PropertyRoot("CSequenceOfCoordinates", "Sequence of Coordinates")]
public class SequenceOfCoordinates
{
    public List<Coordinates> Coordinates { get; set; }

    [Property("Coordinates")]
    protected Coordinates[] CoordinatesArray
    {
        get { return Coordinates.ToArray(); }
        set { Coordinates = new(value); }
    }
}
