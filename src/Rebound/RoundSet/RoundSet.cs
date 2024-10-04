namespace NuVelocity.Rebound;

[PropertyRoot("CRoundSet", typeof(RoundSet), false)]
public class RoundSet
{
    public bool IsInfinity { get; set; }

    [Property("Display Name")]
    public string DisplayName { get; set; }

    [Property("Author")]
    public string Author { get; set; }

    // Debug/Sort Rank
    [Property("Sort Rank")]
    public int SortRank { get; set; }

    // Debug/Award Trophy
    [Property("Award Trophy")]
    public bool AwardTrophy { get; set; }

    [Property("Round To Get Image From")]
    public int RoundToGetImageFrom { get; set; }

    // Infinity properties

    [Property("Description")]
    public string Description { get; set; }

    [Property("Brick Layer Effects")]
    [PropertyArray("Brick Layer Effect")]
    public List<BrickLayout> BrickLayerEffects { get; set; }

    #region Serializer methods

    private bool ShouldSerializeSortRank()
    {
        return !IsInfinity;
    }

    private bool ShouldSerializeAwardTrophy()
    {
        return !IsInfinity;
    }

    private bool ShouldSerializeDescription()
    {
        return IsInfinity;
    }

    private bool ShouldSerializeBrickLayerEffects()
    {
        return IsInfinity;
    }

    #endregion
}
