using NuVelocity.Rebound.Infinity;

namespace NuVelocity.Rebound;

[PropertyRoot("CBrickLayout", typeof(BrickLayout))]
public class BrickLayout
{
    private int _thumbnailQuality;
    private bool _fastForwardOnEmptyScreen;

    [Property("Display Name")]
    public string DisplayName { get; set; }

    [Property("Author")]
    public string Author { get; set; }

    [Property("Note 1")]
    public string Note1 { get; set; }

    [Property("Note 2")]
    public string Note2 { get; set; }

    [Property("Note 3")]
    public string Note3 { get; set; }

    [Property("Note 4")]
    public string Note4 { get; set; }

    [Property("Note 5")]
    public string Note5 { get; set; }

    [Property("Source")]
    public string Source { get; set; }

    [Property("Background Type")]
    public string BackgroundType { get; set; }

    [Property("Last Brick ID Assigned")]
    public int LastBrickIDAssigned { get; set; }

    // reference to power-up file name
    [Property("Automatic Power Ups")]
    [PropertyArray("Automatic")]
    public List<string> AutomaticPowerUps { get; set; }

    [Property("Disallow Automatic Power-Ups")]
    public bool DisallowAutomaticPowerUps { get; set; }

    [Property("Disallow All Level Breakers")]
    public bool DisallowAllLevelBreakers { get; set; }

    // reference to power-up file name
    [Property("Disallowed Power-Ups")]
    [PropertyArray("Disallowed")]
    public List<string> DisallowedPowerUps { get; set; }

    [Property("Additional Chance Of Getting Conditional Extra Life")]
    public int ConditionalExtraLifeChance { get; set; }

    [Property("Bricks")]
    [PropertyArray("Brick")]
    public List<BrickBase> Bricks { get; set; }

    [Property("Capture Center")]
    public Coordinates CaptureCenter { get; set; }

    [Property("Capture Width")]
    public int CaptureWidth { get; set; }

    [Property("Show Capture Rect")]
    public bool ShowCaptureRectangle { get; set; }

    [Property("Automatic Thumbnail")]
    public bool AutomaticThumbnail { get; set; }

    [Property("Thumbnail Quality")]
    public int ThumbnailQuality
    {
        get => _thumbnailQuality;
        set
        {
            if (value >= 0 && value <= 100)
            {
                _thumbnailQuality = value;
            }
        }
    }

    [Property("CRCOfLayoutAtTimeOfLastCatupre")]
    public int LayoutCrcHash { get; set; }

    [PropertyArray("Compressed Thumbnail")]
    public byte[] Thumbnail { get; set; }

    // Infinity Additions

    [Property("Blank Background Color")]
    public RgbColor BlankBackgroundColor { get; set; }

    [Property("Canvas Color")]
    public RgbColor CanvasColor { get; set; }

    [Property("Allow Poisonous Spaceship")]
    public bool AllowPoisonousSpaceship { get; set; }

    [Property("Allow Bombing Spaceship")]
    public bool AllowBombingSpaceship { get; set; }

    [Property("Skip Brick Layer")]
    public bool SkipBrickLayer { get; set; }

    // Supersedes the Thumbnail Quality property.
    [Property("Compression Quality")]
    public int CompressionQuality
    {
        get => ThumbnailQuality;
        set => ThumbnailQuality = value;
    }

    [Property("Sharp Thumbnail")]
    public bool SharpThumbnail { get; set; }

    [Property("Has Been Completed With CRC")]
    public int CompletedWithCrc { get; set; }

    [Property("Got 5 Rings With CRC")]
    public int Got5RingsWithCrc { get; set; }

    [Property("CRC When Saved")]
    public int WhenSavedCrc { get; set; }

    [Property("Name Of Editor Used")]
    public string EditorName { get; set; }

    [Property("Version Of Editor Used")]
    public int EditorVersion { get; set; }

    [Property("Was Current Version Tested")]
    public bool WasCurrentVersionTested { get; set; }

    [Property("Best Number Of Rings")]
    public int BestNumberOfRings { get; set; }

    [Property("DecorationSet")]
    public DecorationSet DecorationSet { get; set; }

    [Property("ProjectedToCanvasDecorationSet")]
    public DecorationSet ProjectedToCanvasDecorationSet { get; set; }

    // TODO: unit - Frames (60th of a second)
    [Property("Number Of Frames To Update Before Brick Layer")]
    public int FrameUpdateCountBeforeBrickLayer { get; set; }

    // TODO: editor should display as: Music To Play Type
    // perhaps an editor override name can be added to the attr.
    [Property("Music To Play")]
    public MusicPicker MusicToPlay { get; set; }

    [Property("FF When Nothing On Screem")]
    internal bool FFWhenNothingOnScreem
    {
        get => _fastForwardOnEmptyScreen;
        set => _fastForwardOnEmptyScreen = value;
    }

    public bool FastForwardWhenNothingOnScreen
    {
        get => _fastForwardOnEmptyScreen;
        set => _fastForwardOnEmptyScreen = value;
    }

    // TODO: unit - seconds
    [Property("Seconds Of Nothing Before FF")]
    public float SecondsOfNothingBeforeFastForward { get; set; }

    // TODO: unit - X normal speed
    [Property("FastForward Speed")]
    public int FastForwardSpeed { get; set; }

    [Property("Has Been Edited In Ricochet Infinity")]
    public bool EditorWasRicochetInfinity { get; set; }

    [Property("Play Log Stats")]
    public PlayLogStats PlayLogStats { get; set; }

    [Property("Brick Layer Effects")]
    [PropertyArray("Brick Layer Effect")]
    public List<BrickLayout> BrickLayerEffects { get; set; }

    public BrickLayout()
    {
        DisplayName = string.Empty;
        Author = string.Empty;
        Note1 = string.Empty;
        Note2 = string.Empty;
        Note3 = string.Empty;
        Note4 = string.Empty;
        Note5 = string.Empty;
        Source = string.Empty;
        BackgroundType = string.Empty;
        AutomaticPowerUps = new();
        DisallowedPowerUps = new();
        Bricks = new();
        CaptureCenter = new();
        Thumbnail = Array.Empty<byte>();
    }
}
