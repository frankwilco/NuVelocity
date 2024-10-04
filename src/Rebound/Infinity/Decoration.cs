namespace NuVelocity.Rebound.Infinity;

[PropertyRoot("CDecoration", typeof(Decoration))]
public abstract class Decoration
{
    [Property("Position")]
    public Coordinates Position { get; set; }

    [Property("Blur Radius")]
    public float BlurRadius { get; set; }
}
