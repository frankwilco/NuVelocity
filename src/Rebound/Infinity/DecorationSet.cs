namespace NuVelocity.Rebound.Infinity;

[PropertyRoot("CDecorationSet", typeof(DecorationSet))]
public class DecorationSet
{
    [Property("Decorations")]
    [PropertyArray("Decoration")]
    List<Decoration> Decorations { get; set; }

    [Property("Size")]
    Coordinates Size { get; set; }
}
