namespace NuVelocity.Rebound;

[PropertyRoot("CBrickBase", typeof(BrickBase))]
public class BrickBase
{
    public bool IsInfinity { get; set; }

    // Debug/Brick Position
    [Property("Brick Position")]
    public FloatCoordinates Position { get; set; }

    // Debug/Style Sheet
    [Property("Style Sheet")]
    public string StyleSheet { get; set; }

    [Property("Name")]
    public string Name { get; set; }

    [Property("PlugIns")]
    public BrickPlugInSimultaneous PlugIns { get; set; }

    [Property("Should Be Destroyed If Hit By Player Ship")]
    public bool DestroyedIfHitByPlayerShip { get; set; }

    // Infinity Properties
    [Property("Scale X")]
    public float ScaleX { get; set; }

    [Property("Scale Y")]
    public float ScaleY { get; set; }

    // TODO: unit - Degrees
    [Property("Rotate")]
    public float Rotate { get; set; }

    [Property("Tint")]
    public RgbColor Tint { get; set; }

    #region Serializer methods

    public bool ShouldSerializeScaleX()
    {
        return IsInfinity;
    }

    public bool ShouldSerializeScaleY()
    {
        return IsInfinity;
    }

    public bool ShouldSerializeRotate()
    {
        return IsInfinity;
    }

    public bool ShouldSerializeTint()
    {
        return IsInfinity;
    }

    #endregion
}
